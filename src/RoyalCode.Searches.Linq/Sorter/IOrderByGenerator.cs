using System.Linq.Expressions;

namespace RoyalCode.Searches.Linq.Sorter;

/// <summary>
/// <para>
///     A generator to create a <see cref="Expression"/> to order by a property.
/// </para>
/// </summary>
public interface IOrderByGenerator
{
    /// <summary>
    /// <para>
    ///     Generate a <see cref="Expression"/> to order by a property.
    /// </para>
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="orderBy">The name of the property to order by.</param>
    /// <returns>The order by expression or null if the property is not found.</returns>
    Expression? Generate<TModel>(string orderBy)
        where TModel : class;
}
