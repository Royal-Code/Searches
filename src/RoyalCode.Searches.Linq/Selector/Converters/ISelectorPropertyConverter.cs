using RoyalCode.Extensions.PropertySelection;
using System.Linq.Expressions;

namespace RoyalCode.Searches.Linq.Selector.Converters;

/// <summary>
/// <para>
///     Interface for a converter of <see cref="PropertyMatch"/> to <see cref="Expression"/>.
/// </para>
/// <para>
///     The expression is used to generate the selector of the <see cref="IQueryable{T}"/>.
/// </para>
/// <para>
///     For each property of the DTO, the <see cref="ISelectorExpressionGenerator"/> will try to find a converter
///     that can convert the <see cref="PropertyMatch"/> target selection to an <see cref="Expression"/>
///     to set the value of the DTO property.
/// </para>
/// </summary>
public interface ISelectorPropertyConverter
{
    /// <summary>
    /// Get the <see cref="Expression"/> for the <paramref name="selection"/>.
    /// This expression must read the value of the <paramref name="selection"/> target selection
    /// to set the value of the DTO property.
    /// </summary>
    /// <param name="selection">The selection to be converted.</param>
    /// <param name="parameter">The target parameter of the expression.</param>
    /// <returns>
    ///     An <see cref="Expression"/> that reads the value of the <paramref name="selection"/> target selection
    ///     to set the value of the DTO property.
    /// </returns>
    Expression GetExpression(PropertyMatch selection, Expression parameter);
}
