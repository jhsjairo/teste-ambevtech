using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.UseCases
{


    public class CreateOrderUseCase
    {
        private readonly IOrderRepository _orderRepository;

        public CreateOrderUseCase(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task ExecuteAsync(CreateOrderDto dto)
        {
            if (await _orderRepository.ExistsAsync(dto.ExternalOrderId))
            {
                // Ignorar duplicado ou lançar exceção, conforme política
                return;
            }

            var order = new Order(dto.ExternalOrderId);

            foreach (var item in dto.Items)
            {
                order.AddItem(item.ProductName, item.Quantity, item.UnitPrice);
            }

            await _orderRepository.SaveAsync(order);
        }
    }
}
