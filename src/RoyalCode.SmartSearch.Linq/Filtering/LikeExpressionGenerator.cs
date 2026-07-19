using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.SmartSearch.Linq.Filtering;

/// <summary>
/// <para>
///     Portable generation of <see cref="CriterionOperator.Like"/> expressions, honoring user wildcards
///     (<c>%</c>) without depending on EF or provider-specific functions.
/// </para>
/// <para>
///     The pattern is matched with a greedy algorithm composed only of translatable pieces
///     (<c>StartsWith</c>, <c>EndsWith</c>, <c>Contains</c>, <c>IndexOf</c>, <c>Substring</c>):
///     anchors when the pattern does not start/end with <c>%</c>, and in-order middle segments by slicing
///     the remaining string after each match. The same expression tree is translatable by relational
///     providers and executable in memory (LINQ to Objects).
/// </para>
/// <para>
///     The <c>_</c> wildcard is not supported in the portable mode (it is honored by provider factories such
///     as <c>EF.Functions.Like</c>).
/// </para>
/// </summary>
public static class LikeExpressionGenerator
{
    /// <summary>
    /// <para>
    ///     Maximum number of slice operations (in-order middle segments). Each slice doubles the size of the
    ///     generated expression, so the composition is cut here; segments beyond the cut are checked with a
    ///     plain <c>Contains</c> over the whole value (documented approximation: order is not guaranteed for
    ///     the excess segments).
    /// </para>
    /// </summary>
    public const int MaxSliceOperations = 5;

    private static readonly MethodInfo IndexOfMethod = typeof(string)
        .GetMethod(nameof(string.IndexOf), [typeof(string)])!;

    private static readonly MethodInfo SubstringMethod = typeof(string)
        .GetMethod(nameof(string.Substring), [typeof(int)])!;

    private static readonly MethodInfo ApplyMethod = typeof(LikeExpressionGenerator)
        .GetMethod(nameof(Apply))!;

    internal static MethodInfo GetApplyMethod(Type modelType) => ApplyMethod.MakeGenericMethod(modelType);

    /// <summary>
    /// <para>
    ///     Applies the like pattern to the query. Called at runtime from generated specifier code, because
    ///     the shape of the predicate depends on the value (wildcards), which only exists at execution time.
    /// </para>
    /// </summary>
    /// <typeparam name="TModel">The query source model type.</typeparam>
    /// <param name="query">The query to filter.</param>
    /// <param name="value">The filter value (the pattern, before the optional wrap).</param>
    /// <param name="target">The expression that selects the target string property.</param>
    /// <param name="wrap">Whether the value is wrapped with wildcards (<c>%value%</c>).</param>
    /// <param name="ignoreCase">Whether both sides are normalized with <c>ToUpper()</c>.</param>
    /// <param name="negation">Whether the comparison is negated.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<TModel> Apply<TModel>(
        IQueryable<TModel> query,
        string? value,
        Expression<Func<TModel, string>> target,
        bool wrap,
        bool ignoreCase,
        bool negation)
    {
        // defensivo: criterios com IgnoreIfIsEmpty ja evitam chegar aqui com valor vazio
        if (string.IsNullOrEmpty(value))
            return query;

        var parameter = target.Parameters[0];
        var predicate = CreatePatternExpression(target.Body, value, wrap, ignoreCase);
        if (negation)
            predicate = Expression.Not(predicate);

        var lambda = Expression.Lambda<Func<TModel, bool>>(predicate, parameter);
        return query.Where(lambda);
    }

    /// <summary>
    /// <para>
    ///     Creates the boolean expression that matches the like pattern against the target string expression.
    /// </para>
    /// </summary>
    /// <param name="targetAccess">The expression that accesses the target string value.</param>
    /// <param name="value">The filter value (the pattern, before the optional wrap).</param>
    /// <param name="wrap">Whether the value is wrapped with wildcards (<c>%value%</c>).</param>
    /// <param name="ignoreCase">Whether both sides are normalized with <c>ToUpper()</c>.</param>
    /// <returns>The boolean expression of the pattern match.</returns>
    public static Expression CreatePatternExpression(
        Expression targetAccess,
        string value,
        bool wrap,
        bool ignoreCase)
    {
        if (ignoreCase)
        {
            targetAccess = Expression.Call(targetAccess, ExpressionGenerator.ToUpperMethod);
            value = value.ToUpper();
        }

        var pattern = wrap ? "%" + value + "%" : value;
        var leading = pattern.Length > 0 && pattern[0] == '%';
        var trailing = pattern.Length > 0 && pattern[^1] == '%';
        var segments = pattern.Split('%', StringSplitOptions.RemoveEmptyEntries);

        // pattern feito so de curingas: tudo da match; pattern vazio: igualdade com vazio
        if (segments.Length == 0)
            return leading || trailing
                ? Expression.Constant(true)
                : Expression.Equal(targetAccess, Expression.Constant(string.Empty));

        // sem curingas: igualdade exata (semantica do LIKE; com o wrap default nao ocorre)
        if (!leading && !trailing && segments.Length == 1)
            return Expression.Equal(targetAccess, Expression.Constant(segments[0]));

        List<Expression> conditions = [];
        var current = targetAccess;
        var slices = 0;
        var first = 0;
        var last = segments.Length - 1;

        if (!leading)
        {
            // ancora inicial: o primeiro segmento e prefixo; consome-o fatiando o restante
            conditions.Add(Expression.Call(current, ExpressionGenerator.StartsWithMethod, Expression.Constant(segments[first])));
            current = Expression.Call(current, SubstringMethod, Expression.Constant(segments[first].Length));
            first++;
        }

        // segmentos "do meio": todos, exceto a ancora final quando o pattern nao termina com %
        var middleEnd = trailing ? last : last - 1;

        for (var i = first; i <= middleEnd; i++)
        {
            var segment = Expression.Constant(segments[i]);

            if (slices >= MaxSliceOperations)
            {
                // corte: excedentes verificados sobre o valor inteiro, sem garantia de ordem
                conditions.Add(Expression.Call(targetAccess, ExpressionGenerator.ContainsMethod, segment));
                continue;
            }

            // o segmento deve ocorrer na fatia restante (ordem garantida pelo fatiamento anterior)
            conditions.Add(Expression.Call(current, ExpressionGenerator.ContainsMethod, segment));

            // fatia apenas se ainda ha segmentos a consumir depois deste
            if (i < middleEnd || !trailing)
            {
                var next = Expression.Add(
                    Expression.Call(current, IndexOfMethod, segment),
                    Expression.Constant(segments[i].Length));
                current = Expression.Call(current, SubstringMethod, next);
                slices++;
            }
        }

        if (!trailing)
        {
            // ancora final: o ultimo segmento deve encerrar a fatia restante
            // (EndsWith sobre a fatia impede sobreposicao com os segmentos ja consumidos)
            conditions.Add(Expression.Call(current, ExpressionGenerator.EndsWithMethod, Expression.Constant(segments[last])));
        }

        var result = conditions[0];
        for (var i = 1; i < conditions.Count; i++)
            result = Expression.AndAlso(result, conditions[i]);

        return result;
    }
}
