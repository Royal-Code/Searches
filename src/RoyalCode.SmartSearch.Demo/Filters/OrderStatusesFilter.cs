using RoyalCode.SmartSearch;
using RoyalCode.SmartSearch.Demo.Domain;

namespace RoyalCode.SmartSearch.Demo.Filters;

/// <summary>
/// Demonstrates the <see cref="CriterionOperator.In"/> operator. The filter property must be declared exactly as
/// <see cref="IEnumerable{T}"/> (not <c>List&lt;T&gt;</c> nor an array), which is why the manual endpoint accepts an
/// array query parameter and assigns it to this property before filtering.
/// </summary>
public sealed class OrderStatusesFilter
{
    /// <summary><c>Order.Status IN (...)</c>. Query (via the manual endpoint): <c>?statuses=Paid&amp;statuses=Shipped</c>.</summary>
    [Criterion("Status")]
    public IEnumerable<OrderStatus>? Statuses { get; set; }
}
