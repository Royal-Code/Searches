using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.SmartSearch.Linq.Filtering;

/// <summary>
/// <para>
///     Factory that customizes the expression emitted for a criterion operator.
/// </para>
/// <para>
///     Implementations are registered in the service collection and tried, in registration order, before the
///     default emission. Returning <see langword="null"/> means the factory does not customize the given
///     context, and the next factory (or the default emission) is used.
/// </para>
/// <para>
///     The customization covers generated criteria (<see cref="CriterionAttribute"/> and disjunctions).
///     Hand-written expressions (manual specifiers, predicate factories) are the consumer's responsibility
///     and are not affected.
/// </para>
/// <para>
///     Note: generated specifier functions are cached per process, keyed by model and filter types. In
///     practice this means one emission strategy per model per process — distinct service providers in the
///     same process share the generated specifiers.
/// </para>
/// </summary>
public interface ICriterionOperatorExpressionFactory
{
    /// <summary>
    ///     Tries to create the operator expression for the given context.
    /// </summary>
    /// <param name="context">The criterion operator context.</param>
    /// <returns>
    ///     The boolean expression that performs the comparison, or <see langword="null"/> when this factory
    ///     does not customize the given context.
    /// </returns>
    Expression? TryCreate(in CriterionOperatorContext context);
}

/// <summary>
///     The context of a criterion operator expression to be created.
/// </summary>
public readonly struct CriterionOperatorContext
{
    /// <summary>
    ///     Creates a new context.
    /// </summary>
    /// <param name="operator">The resolved criterion operator (never <see cref="CriterionOperator.Auto"/>).</param>
    /// <param name="case">The declared case sensitivity.</param>
    /// <param name="wrap">The per-criterion like wrap override.</param>
    /// <param name="negation">Whether the comparison is negated.</param>
    /// <param name="filterMemberAccess">
    ///     The expression of the filter value: a member access at generation time, or a constant with the
    ///     actual value in the disjunction (runtime) path.
    /// </param>
    /// <param name="targetMemberAccess">The expression that accesses the target model property.</param>
    /// <param name="filterProperty">The filter property, when available.</param>
    /// <param name="modelType">The query source model type.</param>
    public CriterionOperatorContext(
        CriterionOperator @operator,
        CriterionCase @case,
        LikeWrap wrap,
        bool negation,
        Expression filterMemberAccess,
        Expression targetMemberAccess,
        PropertyInfo? filterProperty,
        Type modelType)
    {
        Operator = @operator;
        Case = @case;
        Wrap = wrap;
        Negation = negation;
        FilterMemberAccess = filterMemberAccess;
        TargetMemberAccess = targetMemberAccess;
        FilterProperty = filterProperty;
        ModelType = modelType;
    }

    /// <summary>
    ///     The resolved criterion operator (never <see cref="CriterionOperator.Auto"/>).
    /// </summary>
    public CriterionOperator Operator { get; }

    /// <summary>
    ///     The declared case sensitivity.
    /// </summary>
    public CriterionCase Case { get; }

    /// <summary>
    ///     The per-criterion like wrap override. Resolve the effective value with
    ///     <see cref="CriterionDefaults.ResolveWrap(LikeWrap)"/>.
    /// </summary>
    public LikeWrap Wrap { get; }

    /// <summary>
    ///     Whether the comparison is negated.
    /// </summary>
    public bool Negation { get; }

    /// <summary>
    ///     The expression of the filter value: a member access at generation time, or a constant with the
    ///     actual value in the disjunction (runtime) path.
    /// </summary>
    public Expression FilterMemberAccess { get; }

    /// <summary>
    ///     The expression that accesses the target model property.
    /// </summary>
    public Expression TargetMemberAccess { get; }

    /// <summary>
    ///     The filter property, when available.
    /// </summary>
    public PropertyInfo? FilterProperty { get; }

    /// <summary>
    ///     The query source model type.
    /// </summary>
    public Type ModelType { get; }
}
