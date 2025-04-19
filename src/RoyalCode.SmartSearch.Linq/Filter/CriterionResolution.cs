using RoyalCode.Extensions.PropertySelection;
using RoyalCode.SmartSearch.Abstractions;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.SmartSearch.Linq.Filter;

internal sealed class CriterionResolution
{
    private Delegate? predicateFactory;

    public CriterionResolution(PropertyInfo property, CriterionAttribute? criterionAttribute)
    {
        FilterPropertyInfo = property;
        Criterion = criterionAttribute ?? new CriterionAttribute();
    }

    public PropertyInfo FilterPropertyInfo { get; set; }

    public CriterionAttribute Criterion { get; set; }

    public PropertySelection? TargetSelection { get; set; }

    public bool IsPending => TargetSelection is null && predicateFactory is null;

    public void AddPredicateFactory(Delegate predicateFactory)
    {
        this.predicateFactory = predicateFactory;
    }

    public Delegate? TryGetPredicateFactory<TModel>(Type filterPropertyType)
    {
        if (predicateFactory is null)
            return null;

        // if the filter property is nullable, get the underlying type
        filterPropertyType = Nullable.GetUnderlyingType(filterPropertyType) ?? filterPropertyType;

        // check if the predicate factory is compatible with the specified types (Func<TProperty, Expression<Func<TFilter, bool>>>)
        var predicateFactoryType = predicateFactory.GetType();
        var expectedType = typeof(Func<,>).MakeGenericType(filterPropertyType, typeof(Expression<Func<TModel, bool>>));
        if (expectedType.IsAssignableFrom(predicateFactoryType))
            return predicateFactory;

        throw new InvalidOperationException(string.Format(
            "The predicate factory is not compatible with the specified types, model {0}, filter property {1}.",
            typeof(TModel),
            filterPropertyType));
    }
}