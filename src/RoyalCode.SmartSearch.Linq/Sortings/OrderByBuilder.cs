using System.ComponentModel;
using System.Linq.Expressions;

#pragma warning disable S3358 // ternary operator

namespace RoyalCode.SmartSearch.Linq.Sortings;

/// <summary>
/// Builder for "Order By" clauses in a LINQ query.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public ref struct OrderByBuilder<TModel>
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
    /// </summary>
    internal ListSortDirection CurrentDirection { get; set; }

    /// <summary>
    /// Returns the ordered query.
    /// </summary>
    internal readonly IQueryable<TModel> OrderedQueryable => ordered ?? query;

    /// <inheritdoc />
    public OrderByBuilder<TModel> Add<TKey>(Expression<Func<TModel, TKey>> keySelector)
    {
        ordered = CurrentDirection == ListSortDirection.Ascending
            ? ordered is null
                ? query.OrderBy(keySelector)
                : ordered.ThenBy(keySelector)
            : ordered is null
                ? query.OrderByDescending(keySelector)
                : ordered.ThenByDescending(keySelector);

        return this;
    }
}