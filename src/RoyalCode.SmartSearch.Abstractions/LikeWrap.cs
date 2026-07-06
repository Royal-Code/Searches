namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     Declares, per criterion, whether the filter value of a <see cref="CriterionOperator.Like"/> criterion
///     is wrapped with wildcards (<c>%value%</c>) before matching.
/// </para>
/// <para>
///     Used by <see cref="CriterionAttribute.Wrap"/> to override the global default
///     (<c>CriterionDefaults.WrapLikeValue</c>, in the Linq package).
/// </para>
/// </summary>
public enum LikeWrap
{
    /// <summary>
    /// <para>
    ///     Not declared: the global default is used.
    /// </para>
    /// </summary>
    Default = 0,

    /// <summary>
    /// <para>
    ///     The value is wrapped with wildcards (<c>%value%</c>).
    /// </para>
    /// </summary>
    Wrap,

    /// <summary>
    /// <para>
    ///     The value is used as the pattern as-is: without user wildcards the match is exact,
    ///     and leading/trailing segments act as anchors (<c>LIKE</c> semantics).
    /// </para>
    /// </summary>
    None,
}
