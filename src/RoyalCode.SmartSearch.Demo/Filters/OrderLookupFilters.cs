using RoyalCode.SmartSearch;

namespace RoyalCode.SmartSearch.Demo.Filters;

/// <summary>Exact-match lookup by order id (used with <c>FirstOrDefault</c> + hints).</summary>
public sealed class OrderByIdFilter
{
    [Criterion(CriterionOperator.Equal)]
    public int? Id { get; set; }
}

/// <summary>Exact-match lookup by order number (used with <c>Single</c>).</summary>
public sealed class OrderByNumberFilter
{
    [Criterion(CriterionOperator.Equal)]
    public string? Number { get; set; }
}
