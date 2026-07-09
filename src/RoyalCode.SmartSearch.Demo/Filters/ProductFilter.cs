using RoyalCode.SmartSearch;

namespace RoyalCode.SmartSearch.Demo.Filters;

/// <summary>
/// Filter for products, demonstrating equality, numeric range, an anchored <c>Like</c> and a <c>[Disjunction]</c>.
/// </summary>
public sealed class ProductFilter
{
    /// <summary>Equality over <c>Active</c>. Query: <c>?active=true</c>.</summary>
    public bool? Active { get; set; }

    /// <summary>Numeric range (lower bound) over <c>Price</c>. Query: <c>?priceMin=50</c>.</summary>
    [Criterion("Price", CriterionOperator.GreaterThanOrEqual)]
    public decimal? PriceMin { get; set; }

    /// <summary>Numeric range (upper bound) over <c>Price</c>. Query: <c>?priceMax=200</c>.</summary>
    [Criterion("Price", CriterionOperator.LessThanOrEqual)]
    public decimal? PriceMax { get; set; }

    /// <summary>
    /// Anchored <c>Like</c>: the value is used as the pattern as-is (no <c>%value%</c> wrapping),
    /// so <c>ABC%</c> matches SKUs that start with "ABC". Query: <c>?sku=ABC%25</c>.
    /// </summary>
    [Criterion(CriterionOperator.Like, Wrap = LikeWrap.None)]
    public string? Sku { get; set; }

    /// <summary>
    /// Part of the "text" disjunction: matches when <c>Name</c> contains the term OR <c>Sku</c> contains the
    /// (possibly different) <see cref="TextInSku"/> term. Query: <c>?textInName=mo&amp;textInSku=xyz</c>.
    /// </summary>
    [Disjunction("text"), Criterion("Name", CriterionOperator.Contains, Case = CriterionCase.Insensitive)]
    public string? TextInName { get; set; }

    /// <summary>Part of the "text" disjunction (see <see cref="TextInName"/>).</summary>
    [Disjunction("text"), Criterion("Sku", CriterionOperator.Contains, Case = CriterionCase.Insensitive)]
    public string? TextInSku { get; set; }
}
