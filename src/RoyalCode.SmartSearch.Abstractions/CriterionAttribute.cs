namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     Attribute that indicates that a property is a criterion.
/// </para>
/// <para>
///     This can be use in property of filters to automatic generate a filter function.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CriterionAttribute : Attribute
{
    /// <summary>
    /// <para>
    ///     The operator used to filter.
    /// </para>
    /// </summary>
    public CriterionOperator Operator { get; set; }

    /// <summary>
    /// <para>
    ///     Requires the use of the Not operator
    /// </para>
    /// </summary>
    public bool Negation { get; set; }

    /// <summary>
    /// <para>
    ///     The name of the target property path.
    /// </para>
    /// <para>
    ///     Optional, when not informed then the property name will be used.
    /// </para>
    /// <para>
    ///     It can be a nested property path using dot notation or CamelCase.
    /// </para>
    /// </summary>
    public string? TargetPropertyPath { get; set; }

    /// <summary>
    /// <para>
    ///     Ignore the property. Do not use to apply filters.
    /// </para>
    /// </summary>
    public bool Ignore { get; set; }

    /// <summary>
    /// <para>
    ///     Ignore the property if the value is null or empty.
    /// </para>
    /// </summary>
    public bool IgnoreIfIsEmpty { get; set; } = true;
}
