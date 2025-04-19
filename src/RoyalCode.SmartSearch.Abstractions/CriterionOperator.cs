namespace RoyalCode.SmartSearch.Abstractions;

/// <summary>
/// <para>
///     The operator used to filter.
/// </para>
/// </summary>
public enum CriterionOperator
{
    /// <summary>
    ///     Automatic choice the operator.
    /// </summary>
    Auto,
    
    /// <summary>
    ///     The criterion is equal to the value.
    /// </summary>
    Equal,

    /// <summary>
    ///     The criterion is greater than the value.
    /// </summary>
    GreaterThan,

    /// <summary>
    ///     The criterion is greater than or equal to the value.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    ///     The criterion is less than the value.
    /// </summary>
    LessThan,

    /// <summary>
    ///     The criterion is less than or equal to the value.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    ///     The value is in the criterion.
    /// </summary>
    In,

    /// <summary>
    ///     The criterion is like the value.
    /// </summary>
    Like,

    /// <summary>
    ///    The criterion is contained in the value.
    /// </summary>
    Contains,

    /// <summary>
    ///     The value starts with the criterion.
    /// </summary>
    StartsWith,

    /// <summary>
    ///     The value ends with the criterion.
    /// </summary>
    EndsWith,

    ///// <summary>
    /////     The criterion is null.
    ///// </summary>
    //IsNull,
}