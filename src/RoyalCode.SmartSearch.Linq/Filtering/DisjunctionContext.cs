using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering;

internal sealed class DisjunctionContext<TModel>
    where TModel : class
{
    private readonly List<Expression> predicates = [];
    private readonly ParameterExpression LambdaParam = Expression.Parameter(typeof(TModel), "e");


    public void Append<TProperty>(JunctionProperty junction, TProperty value)
    {
        var criterion = junction.Criterion;

        // Build comparison expression: e.Property OP value
        var targetAccess = junction.ModelPropertySelection.GetMemberAccess(LambdaParam);
        var valueExpr = Expression.Constant(value, typeof(TProperty));

        Expression opExpr = ExpressionGenerator.CreateOperatorExpression(
            junction.Operator,
            junction.Criterion.Negation,
            targetAccess,
            valueExpr);

        predicates.Add(opExpr);
    }

    public bool Any() => predicates.Count > 0;

    public IQueryable<TModel> Apply(IQueryable<TModel> query)
    {
        if (!Any())
            return query;

        Expression? lamdaBody = null;
        foreach (var pred in predicates)
        {
            lamdaBody = lamdaBody is null ? pred : Expression.OrElse(lamdaBody, pred);
        }

        var lambda = Expression.Lambda<Func<TModel, bool>>(lamdaBody!, LambdaParam);
        return query.Where(lambda);
    }
}
