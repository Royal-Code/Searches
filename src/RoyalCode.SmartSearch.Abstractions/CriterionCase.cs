namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     Declares the intended case sensitivity of a criterion applied over string values.
/// </para>
/// <para>
///     Used by <see cref="CriterionAttribute.Case"/> for the string operators
///     (<see cref="CriterionOperator.Like"/>, <see cref="CriterionOperator.Contains"/>,
///     <see cref="CriterionOperator.StartsWith"/>, <see cref="CriterionOperator.EndsWith"/>).
///     For non-string operators the value is ignored.
/// </para>
/// </summary>
public enum CriterionCase
{
    /// <summary>
    /// <para>
    ///     No case handling is declared: the comparison behavior is determined by the emission strategy
    ///     and by the query provider (e.g. database collation).
    /// </para>
    /// </summary>
    Default = 0,

    /// <summary>
    /// <para>
    ///     The comparison is intended to be case-sensitive. The default emission applies no normalization,
    ///     therefore the effective behavior still depends on the provider collation.
    /// </para>
    /// </summary>
    Sensitive,

    /// <summary>
    /// <para>
    ///     The comparison is intended to be case-insensitive. The default (portable) emission normalizes both
    ///     sides with <c>ToUpper()</c>; registered expression factories may emit provider-native alternatives
    ///     (e.g. <c>ILIKE</c> on PostgreSQL).
    /// </para>
    /// </summary>
    Insensitive,
}
