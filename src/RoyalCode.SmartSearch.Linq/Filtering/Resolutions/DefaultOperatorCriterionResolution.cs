using RoyalCode.Extensions.PropertySelection;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering.Resolutions;

internal class DefaultOperatorCriterionResolution : AbstractCriterionResolution
{
    private readonly CriterionOperatorExpressionFactories factories;
    private readonly PropertySelection? targetSelection;
    private readonly CriterionOperator @operator;

    public DefaultOperatorCriterionResolution(
        PropertySelection property,
        CriterionAttribute criterionAttribute,
        FilterTarget filterTarget,
        CriterionOperatorExpressionFactories factories)
        : base(property, criterionAttribute, filterTarget)
    {
        this.factories = factories;

        var targetProperty = Criterion.TargetPropertyPath ?? FilterPropertySelection.PropertyName;
        targetSelection = filterTarget.TrySelectProperty(targetProperty);

        if (targetSelection is null)
        {
            Lack = new Lack
            {
                Description = $"The target property path '{targetProperty}' could not be resolved in type '{filterTarget.TargetType.FullName}'."
            };
        }
        else if (!(targetSelection.PropertyType.IsAssignableFrom(FilterPropertySelection.PropertyType)
                || FilterPropertySelection.PropertyType.CheckTypes(targetSelection.PropertyType)))
        {
            Lack = new Lack
            {
                Description = $"The target property '{targetSelection}' for filter property '{FilterPropertySelection}' has incompatible type '{targetSelection.PropertyType.FullName}' (filter property type: '{FilterPropertySelection.PropertyType.FullName}')."
            };
        }

        @operator = ExpressionGenerator.DiscoveryCriterionOperator(criterionAttribute, property.Info);
    }

    public override Expression CreateExpression(ParameterExpression queryParam, ParameterExpression filterParam)
    {
        if (IsLacking(out var lack))
            throw lack.ToException();

        // the predicate function parameter, the entity/model of the query.
        var targetParam = Expression.Parameter(FilterTarget.ModelType, "e");
        var filterMemberAccess = FilterPropertySelection.GetMemberAccess(filterParam);
        var targetMemberAccess = targetSelection!.GetMemberAccess(targetParam);

        var context = new CriterionOperatorContext(
            @operator,
            Criterion.Case,
            Criterion.Wrap,
            Criterion.Negation,
            filterMemberAccess,
            targetMemberAccess,
            FilterPropertySelection.Info,
            FilterTarget.ModelType);

        // factories registradas tem a primeira chance de customizar a expressao do operador
        var operatorExpression = factories.TryCreate(in context);

        Expression assign;
        if (operatorExpression is null
            && @operator is CriterionOperator.Like
            && targetMemberAccess.Type == typeof(string)
            && filterMemberAccess.Type == typeof(string))
        {
            // Like portavel: a forma do predicado depende do valor (curingas), que so existe na execucao;
            // emite chamada ao helper de runtime em vez de um predicado fixo.
            var targetLambda = Expression.Lambda(
                typeof(Func<,>).MakeGenericType(FilterTarget.ModelType, typeof(string)),
                targetMemberAccess,
                targetParam);

            assign = Expression.Assign(queryParam, Expression.Call(
                LikeExpressionGenerator.GetApplyMethod(FilterTarget.ModelType),
                queryParam,
                filterMemberAccess,
                Expression.Constant(targetLambda),
                Expression.Constant(CriterionDefaults.ResolveWrap(Criterion.Wrap)),
                Expression.Constant(Criterion.Case == CriterionCase.Insensitive),
                Expression.Constant(Criterion.Negation)));
        }
        else
        {
            operatorExpression ??= ExpressionGenerator.CreateOperatorExpression(
                @operator,
                Criterion.Negation,
                filterMemberAccess,
                targetMemberAccess,
                Criterion.Case);

            // generate the type of the predicate.
            var predicateType = typeof(Func<,>).MakeGenericType(FilterTarget.ModelType, typeof(bool));

            // create the lambda expression for the queryable
            var lambda = Expression.Lambda(predicateType, operatorExpression, targetParam);

            assign = Expression.Assign(
                queryParam,
                ExpressionGenerator.CreateWhereCall(FilterTarget.ModelType, queryParam, lambda));
        }

        // create an expression to check if the filter property is empty
        if (Criterion.IgnoreIfIsEmpty)
            assign = ExpressionGenerator.GetIfIsEmptyConstraintExpression(
                FilterPropertySelection.GetAccessExpression(filterParam),
                assign);

        return assign;
    }

    // nao usado: CreateExpression e totalmente substituido porque o Like portavel
    // nao produz um predicado fixo (a aplicacao e delegada ao helper de runtime).
    protected override Expression CreatePredicateExpression(ParameterExpression filterParam)
        => throw new NotSupportedException("CreateExpression is fully overridden by this resolution.");
}
