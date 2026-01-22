using RoyalCode.Extensions.PropertySelection;
using System.Reflection;

namespace RoyalCode.SmartSearch.Linq.Filtering;

internal class JunctionProperty
{
    public JunctionProperty(PropertyInfo property, CriterionAttribute criterion, Type modelType)
    {
        FilterProperty = property;
        Criterion = criterion;

        ModelPropertySelection = GetPropertySelection(modelType);
        Operator = ExpressionGenerator.DiscoveryCriterionOperator(criterion, property);
    }

    public PropertyInfo FilterProperty { get;   }

    public CriterionAttribute Criterion { get; }

    public PropertySelection ModelPropertySelection { get; }

    public CriterionOperator Operator { get; }

    private PropertySelection GetPropertySelection(Type modelType)
    {
        return Criterion.TargetPropertyPath is not null
            ? modelType.SelectProperty(Criterion.TargetPropertyPath)
            : modelType.SelectProperty(FilterProperty.Name);
    }
}