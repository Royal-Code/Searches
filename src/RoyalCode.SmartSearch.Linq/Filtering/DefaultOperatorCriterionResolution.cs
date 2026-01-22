using RoyalCode.Extensions.PropertySelection;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.SmartSearch.Linq.Filtering;

internal class DefaultOperatorCriterionResolution : AbstractCriterionResolution
{
    private PropertySelection? targetSelection;

    public DefaultOperatorCriterionResolution(PropertyInfo property, CriterionAttribute criterionAttribute, Type modelType) 
        : base(property, criterionAttribute, modelType)
    {
        if (Criterion.TargetPropertyPath is not null)
        {
            targetSelection = modelType.TrySelectProperty(Criterion.TargetPropertyPath!);
            if (targetSelection is null)
            {
                Pending = new Lack
                {
                    Description = $"The target property path '{Criterion.TargetPropertyPath}' for filter property '{FilterPropertyInfo.Name}' could not be resolved in type '{modelType.FullName}'."
                };
            }
        }
        else
        {
            var propertySelection = modelType.TrySelectProperty(FilterPropertyInfo.Name);
            if (propertySelection is null)
            {
                Pending = new Lack
                {
                    Description = $"The target property path '{FilterPropertyInfo.Name}' for filter property '{FilterPropertyInfo.Name}' could not be resolved in type '{modelType.FullName}'."
                };
            }
            else if (propertySelection.PropertyType.IsAssignableFrom(FilterPropertyInfo.PropertyType)
                || FilterPropertyInfo.PropertyType.CheckTypes(propertySelection.PropertyType))
            {
                targetSelection = propertySelection;
            }
            else
            {
                Pending = new Lack
                {
                    Description = $"The target property '{propertySelection}' for filter property '{FilterPropertyInfo.Name}' has incompatible type '{propertySelection.PropertyType.FullName}' (filter property type: '{FilterPropertyInfo.PropertyType.FullName}')."
                };
            }
        }
    }

    protected override Expression CreatePredicateExpression(ParameterExpression filterParam)
    {
        // the predicate function parameter, the entity/model of the query.
        var targetParam = Expression.Parameter(ModelType, "e");

        var operatorExpression = ExpressionGenerator.CreateOperatorExpression(
            ExpressionGenerator.DiscoveryCriterionOperator(Criterion, FilterPropertyInfo),
            Criterion.Negation,
            FilterPropertyInfo.GetMemberAccess(filterParam),
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