using RoyalCode.Searches.Abstractions;
using RoyalCode.Searches.Linq.Sorter;

namespace RoyalCode.Searches.Linq;

/// <summary>
/// <para>
///     Interface of the component to apply the sort operation to a query.
/// </para>
/// <para>
///     It has a default implementation, <see cref="DefaultSorter{TModel}"/>,
///     which makes use of other abstract components to automate sorting.
/// </para>
/// </summary>
/// <typeparam name="TModel">The query model type.</typeparam>
public interface ISorter<TModel>
    where TModel : class
{
    /// <summary>
    /// <para>
    ///     Applies sorting to the <paramref name="query"/>
    ///     according to the definitions of <paramref name="sortings"/>.
    /// </para>
    /// </summary>
    /// <param name="query">The query to sort.</param>
    /// <param name="sortings">The soring definitions.</param>
    /// <returns>A ordered query.</returns>
    IQueryable<TModel> OrderBy(IQueryable<TModel> query, IEnumerable<ISorting> sortings);

    /// <summary>
    /// <para>
    ///     Applies a default sort to the query so that it can be possible to execute the paged query.
    /// </para>
    /// <para>
    ///     Normally this default sorting is done on top of the Id.
    /// </para>
    /// </summary>
    /// <param name="query">The query to sort.</param>
    /// <returns>A ordered query.</returns>
    IQueryable<TModel> DefaultOrderBy(IQueryable<TModel> query);
}