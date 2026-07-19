namespace RoyalCode.SmartSearch.Demo.Domain;

/// <summary>
/// A single line of an <see cref="Order"/>.
/// </summary>
public sealed class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
