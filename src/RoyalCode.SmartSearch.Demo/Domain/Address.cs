using RoyalCode.SmartSearch;

namespace RoyalCode.SmartSearch.Demo.Domain;

/// <summary>
/// <para>
///     Postal address of a <see cref="Customer"/>.
/// </para>
/// <para>
///     Mapped as an owned type (<c>OwnsOne</c>) on the same table as the customer,
///     and annotated with <see cref="ComplexFilterAttribute"/> so it can be targeted by complex filters.
/// </para>
/// </summary>
[ComplexFilter]
public sealed class Address
{
    public string Street { get; set; } = null!;

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string PostalCode { get; set; } = null!;
}
