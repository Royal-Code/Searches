using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering.Resolutions;

internal sealed class DisjunctionContext<TModel>
    where TModel : class
{
    private readonly List<Expression> predicates = [];
    private readonly ParameterExpression LambdaParam = Expression.Parameter(typeof(TModel), "e");


    public void Append<TProperty>(JunctionProperty junction, TProperty value)
    {
        var criterion = junction.Criterion;

        // Build comparison expression: e.Property OP value
        var targetAccess = junction.ModelPropertySelection!.GetMemberAccess(LambdaParam);
        var valueExpr = Expression.Constant(value, typeof(TProperty));

        // este caminho executa em runtime (valor real inline): as mesmas customizacoes da geracao
        // (factories, Like portavel, case) devem valer aqui, senao filtros "NomeOrApelido" divergem.
        var context = new CriterionOperatorContext(
            junction.Operator,
            criterion.Case,
            criterion.Wrap,
            criterion.Negation,
            valueExpr,
            targetAccess,
            junction.FilterProperty.Info,
            typeof(TModel));

        var opExpr = junction.Factories.TryCreate(in context);

        if (opExpr is null
            && junction.Operator is CriterionOperator.Like
            && targetAccess.Type == typeof(string)
            && value is string stringValue)
        {
            opExpr = LikeExpressionGenerator.CreatePatternExpression(
                targetAccess,
                stringValue,
                CriterionDefaults.ResolveWrap(criterion.Wrap),
                criterion.Case == CriterionCase.Insensitive);

            if (criterion.Negation)
                opExpr = Expression.Not(opExpr);
        }

        // ordem dos operandos corrigida: antes o valor ia na posicao do alvo (gerava "valor".Contains(e.Prop)
        // e "valor > e.Prop"), invertendo a semantica em relacao ao caminho principal das resolutions.
        opExpr ??= ExpressionGenerator.CreateOperatorExpression(
            junction.Operator,
            criterion.Negation,
            valueExpr,
            targetAccess,
            criterion.Case);

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
