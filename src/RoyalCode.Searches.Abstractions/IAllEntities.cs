
namespace RoyalCode.Searches.Abstractions;

/// <summary>
/// <para>
///     A search that returns all entities with purpose to edit them.
/// </para>
/// <para>
///     Filters and sorting can be applied, but the search engine must be able to apply them.
/// </para>
/// <para>
///     When used with a unit of work, all changes made to the entities must be saved.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IAllEntities<TEntity>
    where TEntity : class
{
    /// <summary>
    /// <para>
    ///     Adds a filter object to the search.
    /// </para>
    /// <para>
    ///     The search engine must be able to apply this filter, otherwise an exception will be throwed.
    /// </para>
    /// </summary>
    /// <typeparam name="TFilter">The filter type.</typeparam>
    /// <param name="filter">The filter object.</param>
    /// <returns>The same instance for chaining calls.</returns>
    IAllEntities<TEntity> FilterBy<TFilter>(TFilter filter)
        where TFilter : class;

    /// <summary>
    /// <para>
    ///     Adds a sorting object to be applied to the search.
    /// </para>
    /// </summary>
    /// <param name="sorting">The sorting object.</param>
    /// <returns>The same instance for chaining calls.</returns>
    IAllEntities<TEntity> OrderBy(ISorting sorting);

    /// <summary>
    /// Apply the filters and sorting and get all the entities that meet the criteria.
    /// </summary>
    /// <returns>
    ///     A collection of the entities.
    /// </returns>
    ICollection<TEntity> Collect();

    /// <summary>
    /// Apply the filters and sorting and get all the entities that meet the criteria.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A collection of the entities.
    /// </returns>
    Task<ICollection<TEntity>> CollectAsync(CancellationToken cancellationToken = default);
}
