using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Linq.Filtering;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.SmartSearch.EntityFramework.Npgsql.Filtering;

/// <summary>
/// <para>
///     Emits case-insensitive <see cref="CriterionOperator.Like"/> criteria
///     (<see cref="CriterionCase.Insensitive"/>) as <c>EF.Functions.ILike(target, pattern)</c>,
///     the native PostgreSQL <c>ILIKE</c> operator — preferable to <c>UPPER(...) LIKE UPPER(...)</c>.
/// </para>
/// <para>
///     Criteria that are not <c>Like</c> + <c>Insensitive</c> are not customized (returns null), letting the
///     next factory (e.g. <c>EntityFrameworkLikeExpressionFactory</c>) or the default emission handle them.
///     Register with <c>AddNpgsqlLikeOperators</c>, which puts this factory before the EF Like factory.
/// </para>
/// </summary>
public sealed class NpgsqlILikeExpressionFactory : ICriterionOperatorExpressionFactory
{
    private static readonly MethodInfo ILikeMethod = typeof(NpgsqlDbFunctionsExtensions)
        .GetMethod(nameof(NpgsqlDbFunctionsExtensions.ILike), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private static readonly MethodInfo ConcatMethod = typeof(string)
        .GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)])!;

    private static readonly MemberExpression EfFunctions =
        Expression.Property(null, typeof(EF).GetProperty(nameof(EF.Functions))!);

    /// <inheritdoc />
    public Expression? TryCreate(in CriterionOperatorContext context)
    {
        if (context.Operator is not CriterionOperator.Like
            || context.Case is not CriterionCase.Insensitive
            || context.TargetMemberAccess.Type != typeof(string)
            || context.FilterMemberAccess.Type != typeof(string))
            return null;

        var pattern = context.FilterMemberAccess;

        if (CriterionDefaults.ResolveWrap(context.Wrap))
            pattern = Expression.Call(ConcatMethod, Expression.Constant("%"), pattern, Expression.Constant("%"));

        Expression expression = Expression.Call(ILikeMethod, EfFunctions, context.TargetMemberAccess, pattern);
        return context.Negation ? Expression.Not(expression) : expression;
    }
}
