namespace RoyalCode.SmartSearch.Linq.Mappings;

/// <summary>
/// Exception thrown when a selector is not found.
/// </summary>
public sealed class SelectorNotFoundException : Exception
{
    /// <summary>
    /// Create a new instance of <see cref="SelectorNotFoundException"/>.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    public SelectorNotFoundException(string? message) : base(message) { }
}