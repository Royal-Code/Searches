namespace RoyalCode.SmartSearch.Core;

/// <summary>
/// Information about a filter to be applied to a query.
/// </summary>
/// <param name="Filter">The filter instance.</param>
/// <typeparam name="TFilter">The filter type.</typeparam>
public class SearchFilter<TFilter>(TFilter Filter) : ISearchFilter
    where TFilter : class
{
    /// <inheritdoc />
    public void ApplyFilter(ISpecifierHandler handler)
    {
        handler.Handle(Filter);
    }
}
