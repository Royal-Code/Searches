using RoyalCode.Extensions.PropertySelection;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering.Resolutions;

internal class DefaultOperatorCriterionResolution : AbstractCriterionResolution
{
    private PropertySelection? targetSelection;

    public DefaultOperatorCriterionResolution(PropertySelection property, CriterionAttribute criterionAttribute, FilterTarget filterTarget) 
        : base(property, criterionAttribute, filterTarget)
    {
        var targetProperty = Criterion.TargetPropertyPath ?? FilterPropertySelection.PropertyName;
        targetSelection = filterTarget.TrySelectProperty(targetProperty);

        if (targetSelection is null)
        {
            Lack = new Lack
            {
                Description = $"The target property path '{targetProperty}' could not be resolved in type '{filterTarget.TargetType.FullName}'."
            };
        }
        else if (!(targetSelection.PropertyType.IsAssignableFrom(FilterPropertySelection.PropertyType)
                || FilterPropertySelection.PropertyType.CheckTypes(targetSelection.PropertyType)))
        {
            Lack = new Lack
            {
                Description = $"The target property '{targetSelection}' for filter property '{FilterPropertySelection}' has incompatible type '{targetSelection.PropertyType.FullName}' (filter property type: '{FilterPropertySelection.PropertyType.FullName}')."
            };
        }
    }

    protected override Expression CreatePredicateExpression(ParameterExpression filterParam)
    {
        // the predicate function parameter, the entity/model of the query.
        var targetParam = Expression.Parameter(FilterTarget.ModelType, "e");

        var operatorExpression = ExpressionGenerator.CreateOperatorExpression(
            ExpressionGenerator.DiscoveryCriterionOperator(Criterion, FilterPropertySelection.Info),
            Criterion.Negation,
            FilterPropertySelection.GetMemberAccess(filterParam),
            targetSelection!.GetMemberAccess(targetParam));

        // generate the type of the predicate.
        var predicateType = typeof(Func<,>).MakeGenericType(
            FilterTarget.ModelType,
            typeof(bool));

        // create the lambda expression for the queryable
        var lambda = Expression.Lambda(predicateType, operatorExpression, targetParam);

        return lambda;
    }
}