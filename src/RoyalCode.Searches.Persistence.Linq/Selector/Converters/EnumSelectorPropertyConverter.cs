using RoyalCode.Extensions.PropertySelection;
using System.Linq.Expressions;

namespace RoyalCode.Searches.Persistence.Linq.Selector.Converters;

internal class EnumSelectorPropertyConverter : ISelectorPropertyConverter, ISelectorPropertyResolver
{
    // check if the target (entity) property is an enum and the origin (DTO) property is a enum.
    // and check if the value type of the enums are the same.
    // and check if they have the same number of values.
    public bool CanConvert(PropertyMatch selection, ISelectResolver resolver, out ISelectorPropertyConverter? converter)
    {
        // check if the target (entity) property is an enum and the origin (DTO) property is a enum.
        var isEnums = selection.TargetSelection!.PropertyType.IsEnum
            && selection.OriginProperty.PropertyType.IsEnum;

        // check if the value type of the enums are the same.
        var isSameValueType = isEnums && selection.TargetSelection.PropertyType.GetEnumUnderlyingType()
            == selection.OriginProperty.PropertyType.GetEnumUnderlyingType();

        // check if they have the same number of values.
        var isSameValuesCount = isSameValueType && selection.TargetSelection.PropertyType.GetEnumValues().Length
            == selection.OriginProperty.PropertyType.GetEnumValues().Length;

        converter = isSameValuesCount ? this : null;
        return isSameValuesCount;
    }

    public Expression GetExpression(PropertyMatch selection, Expression parameter)
    {
        // the target property selection is a enum property.
        // so, we need to generate a expression like:
        // (EnumType)e.Id
        // where:
        // e is the parameter
        // Id is the Selection property,
        // EnumType is the DTO property type.
        var selectionExpression = selection.TargetSelection!.GetAccessExpression(parameter);
        var convertExpression = Expression.Convert(selectionExpression, selection.OriginProperty.PropertyType);
        return convertExpression;
    }
}
