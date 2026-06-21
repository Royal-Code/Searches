namespace RoyalCode.SmartSearch.Hints;

/// <summary>
/// <para>
///     A provider-agnostic carrier for a per-query hint captured by <c>ICriteria.UseHints</c>.
/// </para>
/// <para>
///     It holds the hint enum value(s) at the call site and exposes them through double-dispatch
///     (<see cref="Accept"/>), so the closed hint type can be recovered by a provider-specific
///     <see cref="ICriteriaHintVisitor"/> (e.g. the Entity Framework registry visitor) without coupling the
///     abstraction to any hint mechanism.
/// </para>
/// </summary>
public interface ICriteriaHint
{
    /// <summary>
    /// Dispatches the captured hint(s) to the given <paramref name="visitor"/>.
    /// </summary>
    /// <param name="visitor">The visitor that will receive each captured hint value.</param>
    void Accept(ICriteriaHintVisitor visitor);
}

/// <summary>
/// <para>
///     A visitor that receives the closed hint enum value(s) captured by an <see cref="ICriteriaHint"/>.
/// </para>
/// <para>
///     Implemented by provider-specific code (e.g. Entity Framework) to apply the hint to a query, recovering the
///     concrete <c>THint</c> type that was erased when the hint was stored.
/// </para>
/// </summary>
public interface ICriteriaHintVisitor
{
    /// <summary>
    /// Visits a single hint value with its concrete enum type recovered.
    /// </summary>
    /// <typeparam name="THint">The hint enum type.</typeparam>
    /// <param name="hint">The hint value.</param>
    void Visit<THint>(THint hint) where THint : Enum;
}
