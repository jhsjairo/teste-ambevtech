using System.Text.Json;
using StackExchange.Redis;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Order = OrderService.Domain.Entities.Order;

namespace OrderService.Infrastructure.Cache
{
    public class RedisCacheService : IOrderCache
    {
        private readonly IDatabase _db;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<IEnumerable<Order>?> GetOrdersByStatusAsync(OrderStatus status)
        {
            string key = $"orders:status:{status}";
            var value = await _db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<IEnumerable<Order>>(value!);
        }

        public async Task SetOrdersByStatusAsync(OrderStatus status, IEnumerable<Order> orders, TimeSpan expiration)
        {
            string key = $"orders:status:{status}";
            var json = JsonSerializer.Serialize(orders);
            await _db.StringSetAsync(key, json, expiration);
        }

        public async Task<Order?> GetOrderByExternalIdAsync(string externalOrderId)
        {
            string key = $"orders:external:{externalOrderId}";
            var value = await _db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<Order>(value!);
        }

        public async Task SetOrderByExternalIdAsync(string externalOrderId, Order order, TimeSpan expiration)
        {
            string key = $"orders:external:{externalOrderId}";
            var json = JsonSerializer.Serialize(order);
            await _db.StringSetAsync(key, json, expiration);
        }
    }

}
