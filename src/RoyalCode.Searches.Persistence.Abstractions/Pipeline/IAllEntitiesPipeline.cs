
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
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of the entities.</returns>
    Task<ICollection<TEntity>> ExecuteAsync(
        SearchCriteria searchCriteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute the query and verify if there are any entities.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <returns>True if there are any entities, otherwise false.</returns>
    bool Any(SearchCriteria searchCriteria);

    /// <summary>
    /// Execute the query and verify if there are any entities.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if there are any entities, otherwise false.</returns>
    Task<bool> AnyAsync(SearchCriteria searchCriteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute the query and get the first entity.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <returns>The first entity that meets the criteria.</returns>
    TEntity? First(SearchCriteria searchCriteria);

    /// <summary>
    /// Execute the query and get the first entity.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first entity that meets the criteria.</returns>
    Task<TEntity?> FirstAsync(SearchCriteria searchCriteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute the query and get the first entity or throw an exception if there are no entities or more than one.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <returns>The first entity that meets the criteria, or an exception if there are no entities or more than one.</returns>
    TEntity Single(SearchCriteria searchCriteria);

    /// <summary>
    /// Execute the query and get the first entity or throw an exception if there are no entities or more than one.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The first entity that meets the criteria, or an exception if there are no entities or more than one.</returns>
    Task<TEntity> SingleAsync(SearchCriteria searchCriteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute the query and remove all entities that meet the criteria.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    void RemoveAll(SearchCriteria searchCriteria);

    /// <summary>
    /// Execute the query and remove all entities that meet the criteria.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAllAsync(SearchCriteria searchCriteria, CancellationToken cancellationToken = default);
}
