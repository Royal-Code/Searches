namespace RoyalCode.SmartSearch.Linq.Sortings;

/// <summary>
/// <para>
///     A component capable of creating a query sort expression.
/// </para>
/// <para>
///     These expressions will be used in
///     the <c>OrderBy</c> or <c>OrderByDescending</c> methods of the <see cref="IQueryable{T}"/>
///     and <c>ThenBy</c> or <c>ThenByDescending</c> methods of the <see cref="IOrderedQueryable{T}"/>.
/// </para>
/// <para>
///     These components are managed and maintained by the <see cref="OrderByHandlersMap"/>.
/// </para>
/// </summary>
/// <typeparam name="TModel">The query source model type.</typeparam>
public interface IOrderByHandler<TModel>
    where TModel : class
{
    /// <summary>
    /// Use the <paramref name="builder"/> to add a sort expression to the query.
    /// </summary>
    /// <param name="builder">Used to apply the expressions to the query.</param>
    OrderByBuilder<TModel> Handle(OrderByBuilder<TModel> builder);
}