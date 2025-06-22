using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.UseCases
{
   
    public class GetOrderByExternalIdUseCase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderCache _orderCache;

        public GetOrderByExternalIdUseCase(IOrderRepository orderRepository, IOrderCache orderCache)
        {
            _orderRepository = orderRepository;
            _orderCache = orderCache;
        }

        public async Task<Order?> ExecuteAsync(string externalOrderId)
        {
            var cached = await _orderCache.GetOrderByExternalIdAsync(externalOrderId);
            if (cached != null)
                return cached;

            var order = await _orderRepository.GetByExternalOrderIdAsync(externalOrderId);
            if (order != null)
                await _orderCache.SetOrderByExternalIdAsync(externalOrderId, order, TimeSpan.FromMinutes(10));

            return order;
        }
    }
}
