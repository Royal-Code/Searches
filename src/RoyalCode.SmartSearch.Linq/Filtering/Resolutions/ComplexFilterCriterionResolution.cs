using RoyalCode.Extensions.PropertySelection;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering.Resolutions;

internal class ComplexFilterCriterionResolution : ICriterionResolution
{
    private readonly PropertySelection filterProperty;
    private readonly CriterionAttribute criterion;
    private readonly Type modelType;
    private readonly IReadOnlyList<ICriterionResolution> internalResolutions;

    private Lack? lack;

    public ComplexFilterCriterionResolution(
        PropertySelection filterProperty,
        CriterionAttribute criterion,
        Type modelType)
    {
        this.filterProperty = filterProperty;
        this.criterion = criterion;
        this.modelType = modelType;

        internalResolutions = CreateInternalCriterionResolution();
    }

    public Expression CreateExpression(ParameterExpression queryParam, ParameterExpression filterParam)
    {
        List<Expression> expressions = [];

        ParameterExpression propertyFilterParam = Expression.Parameter(filterProperty.PropertyType, "propertyFilter");
        var assignPropertyFilter = Expression.Assign(propertyFilterParam, filterProperty.GetMemberAccess(filterParam));
        expressions.Add(assignPropertyFilter);

        foreach (var resolution in internalResolutions)
        {
            var expr = resolution.CreateExpression(queryParam, propertyFilterParam);
            if (expr is not null)
            {
                expressions.Add(expr);
            }
        }

        // if the filterProperty.PropertyType is not a struct, then check for null before applying the internal expressions.
        if (!filterProperty.PropertyType.IsValueType || Nullable.GetUnderlyingType(filterProperty.PropertyType) is not null)
        {
            var notNullCheck = Expression.NotEqual(
                filterProperty.GetMemberAccess(filterParam),
                Expression.Constant(null, filterProperty.PropertyType));
            
            var block = Expression.Block([propertyFilterParam], expressions);
            var ifNotNullExpr = Expression.IfThen(notNullCheck, block);

            return ifNotNullExpr;
        }

        return Expression.Block([propertyFilterParam], expressions);
    }

    public bool IsLacking([NotNullWhen(true)] out Lack? lack)
    {
        lack = this.lack;
        return lack is not null;
    }

    private IReadOnlyList<ICriterionResolution> CreateInternalCriterionResolution()
    {
        List<ICriterionResolution> resolutions = [];

        // lockup for the target property selection
        var targetPropertySelection = modelType.TrySelectProperty(criterion.TargetPropertyPath ?? filterProperty.PropertyName);

        // if not found, create a lack and return
        if (targetPropertySelection is null)
        {
            lack = new Lack
            {
                Description = $"The target property '{criterion.TargetPropertyPath ?? filterProperty.PropertyName}' specified in criterion '{criterion.GetType().Name}' was not found on model type '{modelType.FullName}'."
            };
            return resolutions;
        }

        // now have 2 ways:
        // 1. the filter property and the target property are of the same type
        //    -> then create resolution for each property of the property type
        // 2. the filter property and the target property are of different types
        //    -> then check for ComplextFilter<T> attribute on the target property type
        //       and check for matching the types.
        //    -> then create a resolution for the filter property type and the defined target sub-property.

        if (filterProperty.PropertyType == targetPropertySelection.PropertyType)
        {
            // same type, create resolutions for each property of the type
            // todo
        }
        else
        {
            // different types, check for ComplexFilter<T> attribute on target property type
            if (!targetPropertySelection.PropertyType.HasAttribute(typeof(ComplexFilterAttribute<>), out var attr))
            {
                lack = new Lack
                {
                    Description = $"The target property '{targetPropertySelection.PropertyName}' on model type '{modelType.FullName}' does not have a ComplexFilter<> attribute required for criterion '{criterion.GetType().Name}'."
                };
                return resolutions;
            }

            
        }

        return resolutions;
    }
}
