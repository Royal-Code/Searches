namespace RoyalCode.SmartSearch.Demo.Domain;

/// <summary>
/// A sales order placed by a <see cref="Customer"/>.
/// </summary>
public sealed class Order
{
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public OrderStatus Status { get; set; }

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public List<OrderItem> Items { get; set; } = [];
}
