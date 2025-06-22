using OrderService.Application.Interfaces;
using OrderService.Application.UseCases;
using OrderService.Infrastructure.Cache;
using OrderService.Infrastructure.Database;
using OrderService.Infrastructure.Repositories;
using OrderService.Worker;
using OrderService.Worker.Consumers;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

// Conexão com SQL Server
builder.Services.AddSingleton<SqlConnectionFactory>();


builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

builder.Services.AddScoped<IOrderCache, RedisCacheService>();


// Repositório e casos de uso
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<CreateOrderUseCase>();



// Worker que consome da fila
builder.Services.AddHostedService<OrderQueueConsumer>();

var host = builder.Build();
host.Run();
