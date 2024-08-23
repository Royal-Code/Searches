namespace RoyalCode.Searches.Core;

/// <summary>
/// Component that applies the filtering conditions to the query.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public interface IFilterSpecifier<TQuery, in TFilter>
{
    /// <summary>
    /// Specify a query, apply the filter conditions to the query.
    /// </summary>
    /// <param name="query">The query object.</param>
    /// <param name="filter">The filter object.</param>
    /// <returns>A new (or the same) query object.</returns>
    TQuery Specify(TQuery query, TFilter filter);
}