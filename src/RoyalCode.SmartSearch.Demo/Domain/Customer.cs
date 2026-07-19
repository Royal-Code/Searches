namespace RoyalCode.SmartSearch.Demo.Domain;

/// <summary>
/// A customer that places orders.
/// </summary>
public sealed class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    /// <summary>Owned/complex address (see <see cref="Address"/>).</summary>
    public Address? MainAddress { get; set; }
}
