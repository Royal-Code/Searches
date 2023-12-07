namespace RoyalCode.Searches.Persistence.EntityFramework;

/// <summary>
/// Exception for when a selector is not informed for the search and also there is none configured for a given entity and DTO.
/// </summary>
public sealed class SelectorNotFoundException : InvalidOperationException
{
    /// <summary>
    /// Creates a new exception.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="dtoType">The DTO type.</param>
    public SelectorNotFoundException(Type entityType, Type dtoType)
        : base($"A search selector is missing for select '{dtoType.Name}' from {entityType.Name}")
    { }
}