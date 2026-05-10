namespace Shared.Events ;

public class OrderCreatedEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}