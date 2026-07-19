using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Linq.Filtering;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.SmartSearch.EntityFramework.Filtering;

/// <summary>
/// <para>
///     Emits <see cref="CriterionOperator.Like"/> criteria as <c>EF.Functions.Like(target, pattern)</c>,
///     translated to the native <c>LIKE</c> by relational providers. User wildcards (<c>%</c> and <c>_</c>)
///     are honored by the provider.
/// </para>
/// <para>
///     With <see cref="CriterionCase.Insensitive"/>, both sides are normalized with <c>ToUpper()</c>
///     (<c>UPPER(...) LIKE UPPER(...)</c>), since <c>ILIKE</c> is provider-specific (see the Npgsql package).
/// </para>
/// <para>
///     Opt-in: register with <c>AddEntityFrameworkLikeOperator</c>. Only queries executed by a provider that
///     translates <c>EF.Functions</c> are supported (the expression is not executable in memory).
/// </para>
/// </summary>
public sealed class EntityFrameworkLikeExpressionFactory : ICriterionOperatorExpressionFactory
{
    private static readonly MethodInfo LikeMethod = typeof(DbFunctionsExtensions)
        .GetMethod(nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private static readonly MethodInfo ConcatMethod = typeof(string)
        .GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)])!;

    private static readonly MethodInfo ToUpperMethod = typeof(string)
        .GetMethod(nameof(string.ToUpper), Type.EmptyTypes)!;

    private static readonly MemberExpression EfFunctions =
        Expression.Property(null, typeof(EF).GetProperty(nameof(EF.Functions))!);

    /// <inheritdoc />
    public Expression? TryCreate(in CriterionOperatorContext context)
    {
        if (context.Operator is not CriterionOperator.Like
            || context.TargetMemberAccess.Type != typeof(string)
            || context.FilterMemberAccess.Type != typeof(string))
            return null;

        var target = context.TargetMemberAccess;
        var pattern = context.FilterMemberAccess;

        if (context.Case == CriterionCase.Insensitive)
        {
            target = Expression.Call(target, ToUpperMethod);
            pattern = Expression.Call(pattern, ToUpperMethod);
        }

        if (CriterionDefaults.ResolveWrap(context.Wrap))
            pattern = Expression.Call(ConcatMethod, Expression.Constant("%"), pattern, Expression.Constant("%"));

        Expression expression = Expression.Call(LikeMethod, EfFunctions, target, pattern);
        return context.Negation ? Expression.Not(expression) : expression;
    }
}
