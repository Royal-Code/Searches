using RoyalCode.Extensions.PropertySelection;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering.Resolutions;

internal abstract class AbstractCriterionResolution : ICriterionResolution
{
    protected AbstractCriterionResolution(PropertySelection propertySelection, CriterionAttribute criterionAttribute, Type modelType)
    {
        Criterion = criterionAttribute;
        ModelType = modelType;
        FilterPropertySelection = propertySelection;
    }

    public PropertySelection FilterPropertySelection {  get; set; }

    public CriterionAttribute Criterion { get; set; }

    public Type ModelType { get; set; }

    protected Lack? Lack { get; set; }

    public virtual Expression CreateExpression(ParameterExpression queryParam, ParameterExpression filterParam)
    {
        if (IsLacking(out var lack))
            throw lack.ToException();

        // the predicate expression to pass to the queryable (to where method).
        Expression predicateExpression = CreatePredicateExpression(filterParam);

        // create the method call to apply the filter in the query.
        var methodCall = ExpressionGenerator.CreateWhereCall(
            ModelType,
            queryParam,
            predicateExpression);

        // assign the query with the value of the call.
        Expression assign = Expression.Assign(queryParam, methodCall);

        // create an expression to check if the filter property is empty
        if (Criterion.IgnoreIfIsEmpty)
            assign = ExpressionGenerator.GetIfIsEmptyConstraintExpression(
                FilterPropertySelection.GetAccessExpression(filterParam),
                assign);

        return assign;
    }

    public bool IsLacking([NotNullWhen(true)] out Lack? lack)
    {
        lack = Lack;
        return lack is not null;
    }

    protected abstract Expression CreatePredicateExpression(ParameterExpression filterParam);
}
