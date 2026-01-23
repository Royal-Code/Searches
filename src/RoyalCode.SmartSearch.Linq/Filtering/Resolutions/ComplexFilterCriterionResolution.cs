using RoyalCode.Extensions.PropertySelection;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering.Resolutions;

internal class ComplexFilterCriterionResolution : ICriterionResolution
{
    private readonly PropertySelection filterProperty;
    private readonly Type? filterPropertyUnderlyingType;
    private readonly CriterionAttribute criterion;
    private readonly FilterTarget filterTarget;
    private readonly IReadOnlyList<ICriterionResolution> internalResolutions;

    private Lack? lack;

    public ComplexFilterCriterionResolution(
        PropertySelection filterProperty,
        CriterionAttribute criterion,
        FilterTarget filterTarget)
    {
        this.filterProperty = filterProperty;
        this.criterion = criterion;
        this.filterTarget = filterTarget;

        filterPropertyUnderlyingType = Nullable.GetUnderlyingType(filterProperty.PropertyType);
        internalResolutions = CreateInternalCriterionResolution();
    }

    private Type FilterPropertyType => filterPropertyUnderlyingType ?? filterProperty.PropertyType;

    public Expression CreateExpression(ParameterExpression queryParam, ParameterExpression filterParam)
    {
        List<Expression> expressions = [];

        ParameterExpression propertyFilterParam = Expression.Parameter(FilterPropertyType, "propertyFilter");

        // assign the property filter variable
        var assignPropertyFilter = filterPropertyUnderlyingType is not null
            ? Expression.Assign(propertyFilterParam, filterProperty.SelectChild("Value")!.GetMemberAccess(filterParam))
            : Expression.Assign(propertyFilterParam, filterProperty.GetMemberAccess(filterParam));

        expressions.Add(assignPropertyFilter);

        foreach (var resolution in internalResolutions)
        {
            var expr = resolution.CreateExpression(queryParam, filterParam);
            if (expr is not null)
            {
                expressions.Add(expr);
            }
        }

        // if the filterProperty.PropertyType is not a struct, then check for null before applying the internal expressions.
        if (!filterProperty.PropertyType.IsValueType || filterPropertyUnderlyingType is not null)
        {
            Expression notNullCheck = filterPropertyUnderlyingType is not null
                ? filterProperty.SelectChild("HasValue")!.GetMemberAccess(filterParam)
                : Expression.NotEqual(
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
        var targetPropertySelection = filterTarget.TrySelectProperty(criterion.TargetPropertyPath ?? filterProperty.PropertyName);

        // if not found, create a lack and return
        if (targetPropertySelection is null)
        {
            lack = new Lack
            {
                Description = $"The target property '{criterion.TargetPropertyPath ?? filterProperty.ToString()}' was not found on model type '{filterTarget.TargetType.FullName}'."
            };
            return [];
        }

        // now have 2 ways:
        // 1. the filter property and the target property are of the same type
        //    -> then create resolution for each property of the property type
        // 2. the filter property and the target property are of different types
        //    -> then check for ComplextFilter<T> attribute on the target property type
        //       and check for matching the types.
        //    -> then create a resolution for the filter property type and the defined target sub-property.

        if (FilterPropertyType == targetPropertySelection.PropertyType)
        {
            // create a new FilterTarget using the targetPropertySelection
            var newFilterTarget = new FilterTarget(filterTarget.ModelType, targetPropertySelection.PropertyType, targetPropertySelection);

            // check the filterProperty to be used as previous filter property
            var previousFilterProperty = filterPropertyUnderlyingType is not null
                ? filterProperty.SelectChild("Value")!
                : filterProperty;

            // return resolutions for the filter property and the new filter target
            return CriterionResolutions.CreateResolutions(previousFilterProperty, newFilterTarget);
        }
        else
        {
            // different types, check for ComplexFilter<T> attribute on target property type
            if (!targetPropertySelection.PropertyType.HasAttribute(typeof(ComplexFilterAttribute<>), out var attr))
            {
                lack = new Lack
                {
                    Description = $"The target property '{targetPropertySelection.PropertyName}' on model type '{filterTarget.TargetType.FullName}' does not have a ComplexFilter<> attribute required for criterion '{criterion.GetType().Name}'."
                };
                return [];
            }

            // todo
        }

        return resolutions;
    }
}
