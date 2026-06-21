using RoyalCode.Extensions.PropertySelection;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering.Resolutions;

internal class FilterExpressionGeneratorResolution : ICriterionResolution
{
    private readonly PropertySelection filterProperty;
    private readonly PropertySelection? targetProperty;
    private readonly Func<ExpressionGeneratorContext, Expression> expressionCreateFunction;
    private readonly Lack? lack;

    public FilterExpressionGeneratorResolution(
        PropertySelection filterProperty, 
        CriterionAttribute criterion, 
        FilterTarget filterTarget, 
        Type? genericAttrType)
    {
        if (genericAttrType is null)
        {
            lack = new Lack
            {
                Description = $"The filter expression generator attribute is missing the generic type argument for filter property '{filterProperty}'."
            };
            return;
        }

        this.filterProperty = filterProperty;

        var targetPropertyName = criterion.TargetPropertyPath ?? filterProperty.PropertyName;
        targetProperty = filterTarget.TrySelectProperty(targetPropertyName);
        if (targetProperty is null)
        {
            lack = new Lack
            {
                Description = $"The target property path '{targetProperty}' could not be resolved in type '{filterTarget.TargetType.FullName}'."
            };
            return;
        }

        var methodInfo = genericAttrType.GetMethod("GenerateExpression", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (methodInfo is null)
        {
            lack = new Lack
            {
                Description = $"The static method 'GenerateExpression' was not found in type '{genericAttrType.FullName}'."
            };
            return;
        }

        expressionCreateFunction = (Func<ExpressionGeneratorContext, Expression>)Delegate.CreateDelegate(
            typeof(Func<ExpressionGeneratorContext, Expression>),
            methodInfo);
    }

    public Expression CreateExpression(ParameterExpression queryParam, ParameterExpression filterParam)
    {
        // the predicate function parameter, the entity/model of the query.
        var targetParam = Expression.Parameter(targetProperty!.RootDeclaringType, "e");
        var targetMember = targetProperty.GetMemberAccess(targetParam);
        var filterMember = filterProperty.GetMemberAccess(filterParam);

        var context = new ExpressionGeneratorContext()
        {
            Query = queryParam,
            Filter = filterParam,
            Model = targetParam,
            ModelMember = targetMember,
            FilterMember = filterMember,
        };

        return expressionCreateFunction(context);
    }

    public bool IsLacking([NotNullWhen(true)] out Lack? lack)
    {
        lack = this.lack;
        return lack is not null;
    }
}

