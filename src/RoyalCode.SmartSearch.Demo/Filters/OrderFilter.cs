using RoyalCode.SmartSearch;
using RoyalCode.SmartSearch.Demo.Domain;

namespace RoyalCode.SmartSearch.Demo.Filters;

/// <summary>
/// Kitchen-sink filter for orders: the canonical, heavily commented copy-paste reference. Every property is a
/// scalar so it binds cleanly from the query string and can be reused by both the manual and the mapped endpoints.
/// </summary>
public sealed class OrderFilter
{
    /// <summary>"Contains" over <c>Order.Number</c>. Query: <c>?number=1001</c>.</summary>
    [Criterion(CriterionOperator.Contains)]
    public string? Number { get; set; }

    /// <summary>
    /// Filters across the <c>Customer</c> navigation using a nested target path. Query: <c>?customerName=maria</c>.
    /// </summary>
    [Criterion("Customer.Name", CriterionOperator.Contains, Case = CriterionCase.Insensitive)]
    public string? CustomerName { get; set; }

    /// <summary>Equality over <c>Status</c>. Query: <c>?status=Paid</c>.</summary>
    public OrderStatus? Status { get; set; }

    /// <summary>Negation: excludes orders whose <c>Status</c> equals the value. Query: <c>?notStatus=Cancelled</c>.</summary>
    [Criterion("Status", Negation = true)]
    public OrderStatus? NotStatus { get; set; }

    /// <summary>Date range (lower bound) over <c>CreatedAt</c>. Query: <c>?createdAtFrom=2026-02-01</c>.</summary>
    [Criterion("CreatedAt", CriterionOperator.GreaterThanOrEqual)]
    public DateTime? CreatedAtFrom { get; set; }

    /// <summary>Date range (upper bound) over <c>CreatedAt</c>. Query: <c>?createdAtTo=2026-05-31</c>.</summary>
    [Criterion("CreatedAt", CriterionOperator.LessThanOrEqual)]
    public DateTime? CreatedAtTo { get; set; }

    /// <summary>
    /// <para>
    ///     Escape hatch for the automatic OR-from-name behavior. This property name contains the token "Or",
    ///     which would otherwise be split into a disjunction over members "Number" and "Code". We intend a single
    ///     criterion over <c>Order.Number</c>, so we set <see cref="CriterionAttribute.DisableOrFromName"/> and the
    ///     explicit target path. Query: <c>?numberOrCode=1001</c>. See the README for the full explanation.
    /// </para>
    /// </summary>
    [Criterion("Number", DisableOrFromName = true)]
    public string? NumberOrCode { get; set; }
}
