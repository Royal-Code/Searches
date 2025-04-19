namespace RoyalCode.SmartSearch.Core;

/// <summary>
/// Information about a filter to be applied to a query.
/// </summary>
public interface ISearchFilter
{
    /// <summary>
    /// Applies the filter to the query by passing the filter to the handler.
    /// </summary>
    /// <param name="handler">A handler for applying filters to queries.</param>
    public abstract void ApplyFilter(ISpecifierHandler handler);
}