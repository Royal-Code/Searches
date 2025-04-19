using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Sorter;

/// <summary>
/// <para>
///     Component for applying the sort expressions to the query.
/// </para>
/// <para>
///     This component is called by the <see cref="IOrderByHandler{TModel}"/>, which is liable for the expression.
/// </para>
/// </summary>
/// <typeparam name="TModel">The query source model type.</typeparam>
public interface IOrderByBuilder<TModel>
{
    /// <summary>
    /// Applies the sort expression to the query.
    /// </summary>
    /// <param name="keySelector">The sort expression.</param>
    /// <typeparam name="TKey">Selected property type.</typeparam>
    void Add<TKey>(Expression<Func<TModel, TKey>> keySelector);
}