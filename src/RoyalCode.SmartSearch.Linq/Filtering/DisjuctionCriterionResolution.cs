using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering;

internal class DisjuctionCriterionResolution : ICriterionResolution
{
    private readonly Type modelType;
    private readonly IReadOnlyList<JunctionProperty> group;

    public DisjuctionCriterionResolution(Type modelType, IReadOnlyList<JunctionProperty> group)
    {
        this.modelType = modelType;
        this.group = group;
    }

    public Expression CreateExpression(ParameterExpression queryParam, ParameterExpression filterParam)
    {
        var ctxType = typeof(DisjunctionContext<>).MakeGenericType(modelType);
        var ctxVar = Expression.Variable(ctxType, "ctx");
        var ctxAssign = Expression.Assign(ctxVar, Expression.New(ctxType));

        var body = new List<Expression> { ctxAssign };

        foreach (var junction in group)
        {
            var filterAccess = junction.FilterProperty.GetMemberAccess(filterParam);
            Expression appendCall;

            var appendMethod = ctxType.GetMethod("Append")!.MakeGenericMethod(
                Nullable.GetUnderlyingType(junction.FilterProperty.PropertyType) ?? junction.FilterProperty.PropertyType);

            // value expression: use Value for nullable
            Expression valueExpr = filterAccess;
            if (Nullable.GetUnderlyingType(filterAccess.Type) != null)
            {
                valueExpr = Expression.MakeMemberAccess(filterAccess, filterAccess.Type.GetProperty("Value")!);
            }

            appendCall = Expression.Call(ctxVar, appendMethod, Expression.Constant(junction), valueExpr);

            // build condition if IgnoreIfIsEmpty
            if (junction.Criterion.IgnoreIfIsEmpty)
            {
                var ifExpr = ExpressionGenerator.GetIfIsEmptyConstraintExpression(filterAccess, appendCall);
                body.Add(ifExpr);
            }
            else
            {
                body.Add(appendCall);
            }
        }

        // if (ctx.Any()) query = ctx.Apply(query);
        var anyCall = Expression.Call(ctxVar, ctxType.GetMethod("Any")!);
        var applyCall = Expression.Assign(queryParam, Expression.Call(ctxVar, ctxType.GetMethod("Apply")!, queryParam));
        body.Add(Expression.IfThen(anyCall, applyCall));

        return Expression.Block([ctxVar], body);
    }

    public bool IsLacking([NotNullWhen(true)] out Lack? lack)
    {
        lack = null;
        return lack is not null;
    }
}
