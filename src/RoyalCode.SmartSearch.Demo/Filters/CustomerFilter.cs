using RoyalCode.SmartSearch;

namespace RoyalCode.SmartSearch.Demo.Filters;

/// <summary>
/// Flat filter for customers. Every property binds cleanly from the query string, so it works both with
/// the AspNetCore helpers (<c>MapSearch</c>/<c>MapList</c>/<c>MapSelectFirst</c>) and with manual <c>ICriteria</c>.
/// </summary>
public sealed class CustomerFilter
{
    /// <summary>Case-insensitive "contains" over <c>Customer.Name</c>. Query: <c>?name=maria</c>.</summary>
    [Criterion(CriterionOperator.Contains, Case = CriterionCase.Insensitive)]
    public string? Name { get; set; }

    /// <summary>
    /// OR inferred from the property name: the token "Or" splits into a disjunction over
    /// <c>Name</c> and <c>Email</c>. Query: <c>?nameOrEmail=mario</c>.
    /// </summary>
    public string? NameOrEmail { get; set; }

    /// <summary>
    /// Filters the owned <c>Address</c> by a nested target path. Query: <c>?state=NY</c>.
    /// (An alternative, structured way to filter the address is shown with <see cref="AddressFilter"/>.)
    /// </summary>
    [Criterion("MainAddress.State")]
    public string? State { get; set; }
}
