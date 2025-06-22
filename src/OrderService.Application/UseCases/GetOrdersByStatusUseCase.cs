using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.UseCases
{
    public class GetOrdersByStatusUseCase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderCache _orderCache;

        public GetOrdersByStatusUseCase(
            IOrderRepository orderRepository,
            IOrderCache orderCache)
        {
            _orderRepository = orderRepository;
            _orderCache = orderCache;
        }

        public async Task<IEnumerable<Order>> ExecuteAsync(OrderStatus status)
        {
            
            var cached = await _orderCache.GetOrdersByStatusAsync(status);
            if (cached != null)
                return cached;

            
            var orders = await _orderRepository.GetOrdersByStatusAsync(status);

            
            await _orderCache.SetOrdersByStatusAsync(status, orders, TimeSpan.FromMinutes(5));

            return orders;
        }
    }
}
