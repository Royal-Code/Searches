using RoyalCode.SmartSearch.Demo.Domain;

namespace RoyalCode.SmartSearch.Demo.Dtos;

/// <summary>
/// Summary projection of an order. <c>CustomerName</c> comes from the customer navigation and <c>Total</c> is
/// computed from the items, so it is projected by a registered selector (see the search configuration).
/// </summary>
public sealed class OrderSummaryDto
{
    public int Id { get; set; }
    public string Number { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public string CustomerName { get; set; } = null!;
    public decimal Total { get; set; }
}
