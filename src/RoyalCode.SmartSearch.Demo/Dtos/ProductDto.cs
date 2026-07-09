namespace RoyalCode.SmartSearch.Demo.Dtos;

/// <summary>
/// Projection of a product. Every member name matches <c>Product</c>, so it is projected by convention
/// (no registered selector) via <c>Select&lt;ProductDto&gt;()</c>.
/// </summary>
public sealed class ProductDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public bool Active { get; set; }
}
