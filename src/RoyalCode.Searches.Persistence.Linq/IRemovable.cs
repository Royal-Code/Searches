namespace RoyalCode.Searches.Persistence.Linq;

/// <summary>
/// Componente to provide a queryable and methods to remove entities from the database.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IRemovable<in TEntity>
    where TEntity : class
{
    /// <summary>
    /// Remove all entities from the database filter by the queryable.
    /// </summary>
    /// <param name="entities">The query to filter the entities to be removed.</param>
    void RemoveAll(IQueryable<TEntity> entities);

    /// <summary>
    /// Remove all entities from the database filter by the queryable.
    /// </summary>
    /// <param name="entities">The query to filter the entities to be removed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAllAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken = default);
}