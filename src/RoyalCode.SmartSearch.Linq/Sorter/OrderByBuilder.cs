using System.ComponentModel;
using System.Linq.Expressions;
using RoyalCode.SmartSearch.Abstractions;

namespace RoyalCode.SmartSearch.Linq.Sorter;

/// <summary>
/// Internal implementation of <see cref="IOrderByBuilder{TModel}"/>.
/// Used by the <see cref="DefaultSorter{TModel}"/>.
/// </summary>
/// <typeparam name="TModel"></typeparam>
internal class OrderByBuilder<TModel> : IOrderByBuilder<TModel>
{
    private readonly IQueryable<TModel> query;
    private IOrderedQueryable<TModel>? ordered;

    /// <summary>
    /// Creates a new builder.
    /// </summary>
    /// <param name="query">To original query to be ordered.</param>
    public OrderByBuilder(IQueryable<TModel> query)
    {
        this.query = query;
    }

    /// <summary>
    /// <para>
    ///     The <see cref="ListSortDirection"/> of the current <see cref="ISorting"/>.
    /// </para>
    /// <para>
    ///     Used internally by <see cref="DefaultSorter{TModel}"/>.
    /// </para>
    /// </summary>
    internal ListSortDirection CurrentDirection { get; set; }

    /// <summary>
    /// Returns the ordered query.
    /// </summary>
    internal IQueryable<TModel> OrderedQueryable => ordered ?? query;

    /// <inheritdoc />
    public void Add<TKey>(Expression<Func<TModel, TKey>> keySelector)
    {
        ordered = CurrentDirection == ListSortDirection.Ascending
            ? ordered is null
                ? query.OrderBy(keySelector)
                : ordered.ThenBy(keySelector)
            : ordered is null
                ? query.OrderByDescending(keySelector)
                : ordered.ThenByDescending(keySelector);
    }
}