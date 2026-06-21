namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     Determine that a property of a filter must be used together with other properties,
///     also annotated with this attribute, in a single 'Where' expression applied to a query,
///     using the 'Or' expression to join the various filtered properties.
/// </para>
/// </summary>
/// <remarks>
/// Constructor with the alias.
/// </remarks>
/// <param name="alias">The disjuction alias.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DisjuctionAttribute(string alias) : Attribute
{
    /// <summary>
    /// <para>
    ///     Disjuction alias, used to group various properties in the same disjuction.
    /// </para>
    /// </summary>
    public string Alias { get; set; } = alias ?? throw new ArgumentNullException(nameof(alias));
}