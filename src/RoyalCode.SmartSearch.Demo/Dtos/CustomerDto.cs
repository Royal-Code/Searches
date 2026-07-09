namespace RoyalCode.SmartSearch.Demo.Dtos;

/// <summary>
/// Projection of a customer. <c>City</c> is flattened from the owned <c>MainAddress</c>.
/// </summary>
public sealed class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? City { get; set; }
}
