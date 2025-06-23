namespace RoyalCode.SmartSearch.Mappings;

/// <summary>
/// <para>
///     Defines a select mapping between an entity and a DTO for search projections.
/// </para>
/// <para>
///     Used to apply a select expression or use the default mapping for the given types.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type to select from.</typeparam>
/// <typeparam name="TDto">The DTO type to project to.</typeparam>
public interface ISearchSelect<TEntity, TDto>
    where TEntity : class
    where TDto : class
{
    /// <summary>
    /// Applies the select mapping to the specified <see cref="ISearchSelector{TEntity}"/>.
    /// </summary>
    /// <param name="searchSelector">The search selector to apply the select mapping to.</param>
    /// <remarks>
    /// If a select expression was provided, it will be used; otherwise, the default mapping will be applied.
    /// </remarks>
    public void ApplySelect(ISearchSelector<TEntity> searchSelector);
}
