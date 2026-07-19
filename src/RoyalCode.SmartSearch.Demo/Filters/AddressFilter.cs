using RoyalCode.SmartSearch;

namespace RoyalCode.SmartSearch.Demo.Filters;

/// <summary>
/// A structured, complex filter over the owned <c>Address</c> type. Marked with <see cref="ComplexFilterAttribute"/>,
/// its inner properties map by name to the target address members (e.g. <c>City</c> -&gt; <c>MainAddress.City</c>).
/// Used by <see cref="CustomerAddressFilter"/>.
/// </summary>
[ComplexFilter]
public sealed class AddressFilter
{
    public string? City { get; set; }

    public string? State { get; set; }
}

/// <summary>
/// Customer filter that carries a nested <see cref="AddressFilter"/> targeting the owned <c>MainAddress</c>.
/// Because the value is a structured object, it is built by hand in the manual endpoint rather than bound
/// from a flat query string.
/// </summary>
public sealed class CustomerAddressFilter
{
    [Criterion("MainAddress")]
    public AddressFilter? Address { get; set; }
}
