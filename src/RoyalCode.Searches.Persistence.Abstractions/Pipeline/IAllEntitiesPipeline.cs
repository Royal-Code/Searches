using RoyalCode.Searches.Persistence.Abstractions;

namespace RoyalCode.Searches.Persistence.Abstractions.Pipeline;

/// <summary>
/// <para>
///     A search pipeline for executing queries from the input criteria and get all entities.
/// </para>
/// <para>
///     This component will perform the various steps necessary to execute the query.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type to query.</typeparam>
public interface IAllEntitiesPipeline<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Execute the query and collect all entities.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <returns>A collection of the entities.</returns>
    ICollection<TEntity> Execute(SearchCriteria searchCriteria);

    /// <summary>
    /// Execute the query and collect all entities.
    /// </summary>
    /// <param name="searchCriteria"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A collection of the entities.</returns>
    Task<ICollection<TEntity>> ExecuteAsync(
        SearchCriteria searchCriteria,
        CancellationToken cancellationToken = default);
}
