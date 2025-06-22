using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string ExternalOrderId { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();
    public decimal Total => Items.Sum(i => i.Total);
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core ou Dapper exige construtor sem parâmetros
    protected Order() { }

    public Order(string externalOrderId)
    {
        Id = Guid.NewGuid();
        ExternalOrderId = externalOrderId ?? throw new ArgumentNullException(nameof(externalOrderId));
        Status = OrderStatus.Processed;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(string productName, int quantity, decimal unitPrice)
    {
        Items.Add(new OrderItem(Id, productName, quantity, unitPrice));
    }
}
