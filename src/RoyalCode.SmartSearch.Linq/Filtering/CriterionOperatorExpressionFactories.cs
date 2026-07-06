using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering;

/// <summary>
/// <para>
///     Encapsulates the registered <see cref="ICriterionOperatorExpressionFactory"/> instances and applies
///     the first-non-null-wins policy over them, in registration order.
/// </para>
/// <para>
///     This is the component that flows through the specifier generation pipeline; the raw enumerable of
///     factories is never passed around.
/// </para>
/// </summary>
public sealed class CriterionOperatorExpressionFactories
{
    /// <summary>
    ///     An instance without factories: <see cref="TryCreate"/> always returns <see langword="null"/>.
    /// </summary>
    public static CriterionOperatorExpressionFactories Empty { get; } = new([]);

    private readonly ICriterionOperatorExpressionFactory[] factories;

    /// <summary>
    ///     Creates a new instance encapsulating the given factories.
    /// </summary>
    /// <param name="factories">The factories, in the order they must be tried.</param>
    public CriterionOperatorExpressionFactories(IEnumerable<ICriterionOperatorExpressionFactory> factories)
    {
        this.factories = [.. factories];
    }

    /// <summary>
    ///     Tries to create the operator expression using the registered factories, in order.
    ///     The first non-null result wins.
    /// </summary>
    /// <param name="context">The criterion operator context.</param>
    /// <returns>
    ///     The customized expression, or <see langword="null"/> when no factory customizes the given context
    ///     (the default emission must be used).
    /// </returns>
    public Expression? TryCreate(in CriterionOperatorContext context)
    {
        foreach (var factory in factories)
        {
            var expression = factory.TryCreate(in context);
            if (expression is not null)
                return expression;
        }

        return null;
    }
}
