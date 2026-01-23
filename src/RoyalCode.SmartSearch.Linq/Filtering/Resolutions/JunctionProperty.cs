using RoyalCode.Extensions.PropertySelection;
using System.Diagnostics.CodeAnalysis;

namespace RoyalCode.SmartSearch.Linq.Filtering.Resolutions;

internal class JunctionProperty
{
    private readonly Lack? lack;

    public JunctionProperty(PropertySelection property, CriterionAttribute criterion, Type modelType)
    {
        FilterProperty = property;
        Criterion = criterion;

        var propertySelection = GetPropertySelection(modelType);
        if (propertySelection is null)
        {
            lack = new Lack
            {
                Description = $"The target property '{Criterion.TargetPropertyPath ?? FilterProperty.PropertyName}' for filter property '{FilterProperty.PropertyName}' was not found in model type '{modelType.FullName}'."
            };
        }
        else
        {
            ModelPropertySelection = propertySelection;
            Operator = ExpressionGenerator.DiscoveryCriterionOperator(criterion, property.Info);
        }
    }

    public PropertySelection FilterProperty { get; }

    public CriterionAttribute Criterion { get; }

    public PropertySelection? ModelPropertySelection { get; }

    public CriterionOperator Operator { get; }

    [MemberNotNullWhen(false, nameof(ModelPropertySelection))]
    public bool IsLacking([NotNullWhen(true)] out Lack? lack)
    {
        lack = this.lack;
        return lack is not null;
    }

    private PropertySelection? GetPropertySelection(Type modelType)
    {
        return Criterion.TargetPropertyPath is not null
            ? modelType.TrySelectProperty(Criterion.TargetPropertyPath)
            : modelType.TrySelectProperty(FilterProperty.PropertyName);
    }
}