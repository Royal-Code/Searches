namespace RoyalCode.SmartSearch.Filtering;

/// <summary>
/// Information about a filter to be applied to a query.
/// </summary>
/// <param name="Filter">The filter instance.</param>
/// <typeparam name="TFilter">The filter type.</typeparam>
public sealed class EntityFilter<TFilter>(TFilter Filter) : IFilter
    where TFilter : class
{
    /// <inheritdoc />
    public void ApplyFilter(ISpecifier specifier)
    {
        specifier.Specify(Filter);
    }
}
