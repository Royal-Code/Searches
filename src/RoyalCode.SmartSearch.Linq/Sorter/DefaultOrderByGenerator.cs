using RoyalCode.Extensions.PropertySelection;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Sorter;

/// <summary>
/// Default implementation of <see cref="IOrderByGenerator"/>, using <see cref="PropertySelection"/> for
/// lookup of properties.
/// </summary>
public class DefaultOrderByGenerator : IOrderByGenerator
{
    /// <inheritdoc/>
    public Expression? Generate<TModel>(string orderBy) where TModel : class
    {
        var selection = typeof(TModel).TrySelectProperty(orderBy);
        if (selection is null)
            return null;

        var parameter = Expression.Parameter(typeof(TModel), "x");
        var property = selection.GetAccessExpression(parameter);
        var delegateType = typeof(Func<,>).MakeGenericType(typeof(TModel), property.Type);
        var lambda = Expression.Lambda(delegateType, property, parameter);
        return lambda;
    }
}
