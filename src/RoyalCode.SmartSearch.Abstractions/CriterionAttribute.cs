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
    /// Initializes a new instance of the <see cref="CriterionAttribute"/> class.
    /// </summary>
    public CriterionAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CriterionAttribute"/> class with the specified criterion operator.
    /// </summary>
    /// <param name="criterionOperator">The operator that defines how the criterion will be evaluated.</param>
    public CriterionAttribute(CriterionOperator criterionOperator)
    {
        Operator = criterionOperator;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CriterionAttribute"/> class with the specified target property path.
    /// </summary>
    /// <param name="targetPropertyPath">The dot-delimited path to the property that this criterion targets. Cannot be null or empty.</param>
    public CriterionAttribute(string targetPropertyPath)
    {
        TargetPropertyPath = targetPropertyPath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CriterionAttribute"/> class with the specified target property path and criterion
    /// operator.
    /// </summary>
    /// <param name="targetPropertyPath">The dot-delimited path to the property that the criterion applies to. Cannot be null or empty.</param>
    /// <param name="criterionOperator">The operator to use when evaluating the criterion.</param>
    public CriterionAttribute(string targetPropertyPath, CriterionOperator criterionOperator)
    {
        TargetPropertyPath = targetPropertyPath;
        Operator = criterionOperator;
    }

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

    /// <summary>
    /// <para>
    ///     Disables the automatic OR disjunction inferred from the filter property name or from
    ///     the <see cref="TargetPropertyPath"/> when they contain the token "Or".
    /// </para>
    /// <para>
    ///     By default, when a filter property name (or its <see cref="TargetPropertyPath"/>) contains
    ///     the token "Or" (e.g., <c>FirstNameOrLastName</c>), the engine splits it into multiple parts
    ///     and builds a disjunction (OR) across those target members. When this flag is set to <c>true</c>,
    ///     that split is suppressed and the property is treated as a single criterion, using the
    ///     property name (or the entire <see cref="TargetPropertyPath"/>) as-is.
    /// </para>
    /// <remarks>
    /// <para>
    ///     Use this when natural words include the substring "Or" but are not intended to express
    ///     disjunctions. Examples include names like <c>Ordem</c>, <c>TempoOrdinario</c>, or
    ///     <c>NomeOrApelido</c>. With this flag enabled, these properties will no longer trigger
    ///     automatic OR behavior.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class CustomerFilter
    /// {
    ///     [Criterion(DisableOrFromName = true)]
    ///     public string? ColorOrSizePreference { get; set; } // treated as a single criterion
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public bool DisableOrFromName { get; set; }
 
}
