using RoyalCode.Extensions.PropertySelection;
using RoyalCode.SmartSearch.Core.Extensions;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Selector.Converters;

internal class NullableSelectorPropertyConverter : ISelectorPropertyConverter, ISelectorPropertyResolver
{
    public bool CanConvert(PropertyMatch selection, ISelectResolver resolver, out ISelectorPropertyConverter? converter)
    {
        // check if the target (entity) property is nullable and the origin (DTO) property is not nullable,
        // and the target property type is the same of the origin property type.
        var canResolve = selection.TargetSelection!.PropertyType.IsNullableType()
            && selection.OriginProperty.PropertyType.IsNotNullableType()
            && selection.TargetSelection.PropertyType.GetNullableUnderlyingType() == selection.OriginProperty.PropertyType;

        converter = canResolve ? this : null;
        return canResolve;
    }

    public Expression GetExpression(PropertyMatch selection, Expression parameter)
    {
        // the target property selection is a nullable property.
        // so, we need to generate a expression like:
        // e.Id.HasValue ? e.Id.Value : default(int)
        // where:
        // e is the parameter
        // Id is the Selection property,
        // int is the origin property type.

        var hasValueExpression = selection.TargetSelection!.SelectChild(nameof(Nullable<int>.HasValue))!.GetAccessExpression(parameter);
        var valueExpression = selection.TargetSelection!.SelectChild(nameof(Nullable<int>.Value))!.GetAccessExpression(parameter);
        var defaultExpression = Expression.Default(selection.OriginProperty.PropertyType);
        var ifExpression = Expression.Condition(hasValueExpression, valueExpression, defaultExpression);

        return ifExpression;
    }
}
