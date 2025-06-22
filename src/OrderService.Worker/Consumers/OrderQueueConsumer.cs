using Azure.Messaging.ServiceBus;
using OrderService.Application.DTOs;
using OrderService.Application.UseCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Worker.Consumers
{

    public class OrderQueueConsumer : BackgroundService
    {

        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;
        private readonly CreateOrderUseCase _createOrderUseCase;
        private readonly ILogger<OrderQueueConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        public OrderQueueConsumer(
     IConfiguration configuration,
     IServiceScopeFactory scopeFactory,
     ILogger<OrderQueueConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var connectionString = configuration.GetConnectionString("ServiceBus");
            var queueName = configuration.GetValue<string>("ServiceBusQueue");

            _client = new ServiceBusClient(connectionString);
            _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _processor.ProcessMessageAsync += ProcessMessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            await _processor.StartProcessingAsync(stoppingToken);
        }

        private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var useCase = scope.ServiceProvider.GetRequiredService<CreateOrderUseCase>();

                var jsonBody = args.Message.Body.ToString();
                var orderDto = JsonSerializer.Deserialize<CreateOrderDto>(jsonBody);

                if (orderDto != null)
                {
                    await useCase.ExecuteAsync(orderDto);
                    _logger.LogInformation($"Pedido processado: {orderDto.ExternalOrderId}");
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem da fila");
            }
        }
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Erro no processamento da fila");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
