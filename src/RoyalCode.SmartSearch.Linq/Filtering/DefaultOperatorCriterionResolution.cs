using RoyalCode.Extensions.PropertySelection;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering;

internal class DefaultOperatorCriterionResolution : AbstractCriterionResolution
{
    private PropertySelection? targetSelection;

    public DefaultOperatorCriterionResolution(PropertySelection property, CriterionAttribute criterionAttribute, Type modelType) 
        : base(property, criterionAttribute, modelType)
    {
        if (Criterion.TargetPropertyPath is not null)
        {
            targetSelection = modelType.TrySelectProperty(Criterion.TargetPropertyPath!);
            if (targetSelection is null)
            {
                Lack = new Lack
                {
                    Description = $"The target property path '{Criterion.TargetPropertyPath}' for filter property '{FilterPropertySelection.PropertyName}' could not be resolved in type '{modelType.FullName}'."
                };
            }
        }
        else
        {
            var propertySelection = modelType.TrySelectProperty(FilterPropertySelection.PropertyName);
            if (propertySelection is null)
            {
                Lack = new Lack
                {
                    Description = $"The target property path '{FilterPropertySelection.PropertyName}' for filter property '{FilterPropertySelection.PropertyName}' could not be resolved in type '{modelType.FullName}'."
                };
            }
            else if (propertySelection.PropertyType.IsAssignableFrom(FilterPropertySelection.PropertyType)
                || FilterPropertySelection.PropertyType.CheckTypes(propertySelection.PropertyType))
            {
                targetSelection = propertySelection;
            }
            else
            {
                Lack = new Lack
                {
                    Description = $"The target property '{propertySelection}' for filter property '{FilterPropertySelection.PropertyName}' has incompatible type '{propertySelection.PropertyType.FullName}' (filter property type: '{FilterPropertySelection.PropertyType.FullName}')."
                };
            }
        }
    }

    protected override Expression CreatePredicateExpression(ParameterExpression filterParam)
    {
        // the predicate function parameter, the entity/model of the query.
        var targetParam = Expression.Parameter(ModelType, "e");

        var operatorExpression = ExpressionGenerator.CreateOperatorExpression(
            ExpressionGenerator.DiscoveryCriterionOperator(Criterion, FilterPropertySelection.Info),
            Criterion.Negation,
            FilterPropertySelection.GetMemberAccess(filterParam),
            targetSelection!.GetMemberAccess(targetParam));

        // generate the type of the predicate.
        var predicateType = typeof(Func<,>).MakeGenericType(
            ModelType,
            typeof(bool));

        // create the lambda expression for the queryable
        var lambda = Expression.Lambda(predicateType, operatorExpression, targetParam);

        return lambda;
    }
}