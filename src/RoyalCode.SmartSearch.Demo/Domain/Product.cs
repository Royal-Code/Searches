namespace RoyalCode.SmartSearch.Demo.Domain;

/// <summary>
/// A product that can be sold in an order.
/// </summary>
public sealed class Product
{
    public int Id { get; set; }

    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public bool Active { get; set; }
}
