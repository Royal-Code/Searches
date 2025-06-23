namespace RoyalCode.SmartSearch.Filtering;

/// <summary>
/// Information about a filter to be applied to a query.
/// </summary>
public interface IFilter
{
    /// <summary>
    /// Applies the filter to the query by passing the filter to the handler.
    /// </summary>
    /// <param name="specifier">A specifier handler for applying filters to queries.</param>
    void ApplyFilter(ISpecifier specifier);
}