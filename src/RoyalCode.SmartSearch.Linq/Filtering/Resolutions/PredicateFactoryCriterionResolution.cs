using RoyalCode.Extensions.PropertySelection;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering.Resolutions;

internal class PredicateFactoryCriterionResolution : AbstractCriterionResolution
{
    private readonly Delegate predicateFactory;

    public PredicateFactoryCriterionResolution(
        PropertySelection property,
        CriterionAttribute criterionAttribute,
        FilterTarget filterTarget,
        Delegate predicateFactory)
        : base(property, criterionAttribute, filterTarget)
    {
        this.predicateFactory = predicateFactory;

        if (!CheckPredicateFactoryType(filterTarget, property.PropertyType))
        {
            Lack = new Lack
            {
                Description = $"The predicate factory for filter property '{FilterPropertySelection.PropertyName}' is not compatible with the specified types, model '{filterTarget.TargetType.FullName}', filter property '{property.PropertyType.FullName}'."
            };
        }
    }

    protected override Expression CreatePredicateExpression(ParameterExpression filterParam)
    {
        // create a expression to call the predicate factory for create the predicate expression.
        var predicateFactoryCall = Expression.Call(
            Expression.Constant(predicateFactory.Target),
            predicateFactory.Method,
            FilterPropertySelection.GetMemberAccess(filterParam));

        return predicateFactoryCall;
    }

    private bool CheckPredicateFactoryType(FilterTarget filterTarget, Type filterPropertyType)
    {
        // if the filter property is nullable, get the underlying type
        filterPropertyType = Nullable.GetUnderlyingType(filterPropertyType) ?? filterPropertyType;

        // check if the predicate factory is compatible with the specified types (Func<TProperty, Expression<Func<TFilter, bool>>>)
        var predicateFactoryType = predicateFactory.GetType();
        var funcType = typeof(Func<,>).MakeGenericType(filterTarget.ModelType, typeof(bool));
        var expressionFuncType = typeof(Expression<>).MakeGenericType(funcType);
        var expectedType = typeof(Func<,>).MakeGenericType(filterPropertyType, expressionFuncType);
        if (expectedType.IsAssignableFrom(predicateFactoryType))
            return true;

        return false;
    }
}
