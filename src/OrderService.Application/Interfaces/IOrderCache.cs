using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces
{
    public interface IOrderCache
    {
        Task<IEnumerable<Order>?> GetOrdersByStatusAsync(OrderStatus status);
        Task SetOrdersByStatusAsync(OrderStatus status, IEnumerable<Order> orders, TimeSpan expiration);

        Task<Order?> GetOrderByExternalIdAsync(string externalOrderId);
        Task SetOrderByExternalIdAsync(string externalOrderId, Order order, TimeSpan expiration);
    }
}
