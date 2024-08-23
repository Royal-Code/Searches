namespace RoyalCode.Searches.Core;

/// <summary>
/// Information about a filter to be applied to a query.
/// </summary>
/// <param name="ModelType">The query model type.</param>
/// <param name="Filter">The filter instance.</param>
/// <typeparam name="TFilter">The filter type.</typeparam>
public record SearchFilter<TFilter>(Type ModelType, TFilter Filter) : SearchFilter(ModelType)
    where TFilter : class
{
    /// <inheritdoc />
    public override void ApplyFilter(ISpecifierHandler handler)
    {
        handler.Handle(Filter);
    }
}

/// <summary>
/// Information about a filter to be applied to a query.
/// </summary>
/// <param name="ModelType">The query model type.</param>
public abstract record SearchFilter(Type ModelType)
{
    /// <summary>
    /// Applies the filter to the query by passing the filter to the handler.
    /// </summary>
    /// <param name="handler">A handler for applying filters to queries.</param>
    public abstract void ApplyFilter(ISpecifierHandler handler);
}