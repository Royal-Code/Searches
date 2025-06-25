namespace RoyalCode.SmartSearch.Linq.Services;

/// <summary>
/// Component to provide a <see cref="IQueryable{T}"/> for an entity.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IQueryableProvider<out TEntity>
    where TEntity : class
{
    /// <summary>
    /// Get a new queryable for the entity.
    /// </summary>
    /// <param name="tracking">
    ///     Defines whether the entities should be tracked by the context, session or unit of work.
    /// </param>
    /// <returns>An <see cref="IQueryable{T}"/> instance.</returns>
    IQueryable<TEntity> GetQueryable(bool tracking);
}
