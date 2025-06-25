namespace RoyalCode.SmartSearch.Linq.Filtering;

/// <summary>
/// Component that applies the filtering conditions to the query,
/// where the query object is a <see cref="IQueryable{T}"/>.
/// </summary>
/// <typeparam name="TModel">The model of the <see cref="IQueryable{T}"/>.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public interface ISpecifier<TModel, in TFilter>
    where TModel : class
    where TFilter : class
{
    /// <summary>
    /// Applies the specified filter to the given query, returning a filtered result set.
    /// </summary>
    /// <param name="query">The initial query to which the filter will be applied. Cannot be null.</param>
    /// <param name="filter">The filter criteria used to refine the query results. Cannot be null.</param>
    /// <returns>An <see cref="IQueryable{TModel}"/> containing the filtered results based on the provided criteria.</returns>
    IQueryable<TModel> Specify(IQueryable<TModel> query, TFilter filter);
}