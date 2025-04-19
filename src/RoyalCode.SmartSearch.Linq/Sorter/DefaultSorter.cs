// Ignore Spelling: sortings

using System.ComponentModel;
using RoyalCode.SmartSearch.Abstractions;

namespace RoyalCode.SmartSearch.Linq.Sorter;

/// <summary>
/// <para>
///     Default implementation of <see cref="ISorter{TModel}"/> using additional abstract componentes:
/// </para>
/// <list type="bullet">
///     <item>
///         <term><see cref="IOrderByProvider"/>: </term>
///         <description>Used to find <see cref="IOrderByHandler{TModel}"/>.</description>
///     </item>
///     <item>
///         <term><see cref="IOrderByHandler{TModel}"/>: </term>
///         <description>Used to apply the "Order By" through the <see cref="IOrderByBuilder{TModel}"/>.</description>
///     </item>
///     <item>
///         <term><see cref="IOrderByBuilder{TModel}"/>: </term>
///         <description>Receives the "Order By" expression and applies it to the query.</description>
///     </item>
/// </list>
/// </summary>
/// <typeparam name="TModel">The query source model type.</typeparam>
public class DefaultSorter<TModel> : ISorter<TModel>
    where TModel : class
{
    private readonly IOrderByProvider provider;

    /// <summary>
    /// Creates a new sorter with the <see cref="IOrderByProvider"/> to get the handlers.
    /// </summary>
    /// <param name="provider">The order by handlers provider.</param>
    public DefaultSorter(IOrderByProvider provider)
    {
        this.provider = provider;
    }

    /// <inheritdoc />
    public IQueryable<TModel> OrderBy(IQueryable<TModel> query, IEnumerable<ISorting> sortings)
    {
        var builder = new OrderByBuilder<TModel>(query);

        foreach (var sorting in sortings)
        {
            var handler = provider.GetHandler<TModel>(sorting.OrderBy);
            if (handler is null)
                continue;

            builder.CurrentDirection = sorting.Direction;
            handler.Handle(builder);
        }

        return builder.OrderedQueryable;
    }

    /// <inheritdoc />
    public IQueryable<TModel> DefaultOrderBy(IQueryable<TModel> query)
    {
        var handler = provider.GetDefaultHandler<TModel>();
        var builder = new OrderByBuilder<TModel>(query)
        {
            CurrentDirection = ListSortDirection.Ascending
        };
        handler.Handle(builder);
        return builder.OrderedQueryable;
    }
}