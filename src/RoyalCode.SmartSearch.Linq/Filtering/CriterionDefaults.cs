namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     Global defaults for the generation of criterion expressions.
/// </para>
/// <para>
///     These values are read when the specifier functions are generated (and cached), therefore they must be
///     configured at application startup, before any search is performed.
/// </para>
/// </summary>
public static class CriterionDefaults
{
    /// <summary>
    /// <para>
    ///     The operator applied to string filter properties when the criterion operator is
    ///     <see cref="CriterionOperator.Auto"/>. The default is <see cref="CriterionOperator.Like"/>,
    ///     where user wildcards (<c>%</c>) are honored.
    /// </para>
    /// <para>
    ///     Set to <see cref="CriterionOperator.Contains"/> to restore literal substring matching
    ///     (wildcards escaped) as the default for strings.
    /// </para>
    /// </summary>
    public static CriterionOperator DefaultStringOperator { get; set; } = CriterionOperator.Like;

    /// <summary>
    /// <para>
    ///     Whether <see cref="CriterionOperator.Like"/> values are wrapped with wildcards (<c>%value%</c>)
    ///     by default. Can be overridden per criterion with <see cref="CriterionAttribute.Wrap"/>.
    /// </para>
    /// <para>
    ///     The default is <see langword="true"/>: values match as substrings, and user wildcards inside the
    ///     value are honored. When <see langword="false"/>, the value is the pattern as-is (LIKE semantics:
    ///     without wildcards the match is exact).
    /// </para>
    /// </summary>
    public static bool WrapLikeValue { get; set; } = true;

    /// <summary>
    ///     Resolves the effective wrap behavior for a criterion, combining the per-criterion override
    ///     with the global default (<see cref="WrapLikeValue"/>).
    /// </summary>
    /// <param name="wrap">The per-criterion override.</param>
    /// <returns>True when the like value must be wrapped with wildcards.</returns>
    public static bool ResolveWrap(LikeWrap wrap) => wrap switch
    {
        LikeWrap.Wrap => true,
        LikeWrap.None => false,
        _ => WrapLikeValue,
    };
}
