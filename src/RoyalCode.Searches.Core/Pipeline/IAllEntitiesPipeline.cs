
namespace RoyalCode.Searches.Core.Pipeline;

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

    /// <summary>
    /// Execute the query and update all entities that meet the criteria.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <param name="updateAction">The action to update the entities.</param>
    void UpdateAll(SearchCriteria searchCriteria, Action<TEntity> updateAction);

    /// <summary>
    /// Execute the query and update all entities that meet the criteria.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <param name="updateAction">The action to update the entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAllAsync(
        SearchCriteria searchCriteria,
        Action<TEntity> updateAction,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Execute the query and update all entities that meet the criteria.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <param name="data">The data used to update the entities.</param>
    /// <param name="updateAction">The action to update the entities.</param>
    /// <typeparam name="TData">The type of the data used to update the entities.</typeparam>
    void UpdateAll<TData>(SearchCriteria searchCriteria, TData data, Action<TEntity, TData> updateAction);

    /// <summary>
    /// Execute the query and update all entities that meet the criteria.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <param name="data">The data used to update the entities.</param>
    /// <param name="updateAction">The action to update the entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TData">The type of the data used to update the entities.</typeparam>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAllAsync<TData>(
        SearchCriteria searchCriteria,
        TData data,
        Action<TEntity, TData> updateAction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update all entities that meet the criteria.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <param name="collection">A collection of data used to update the entities.</param>
    /// <param name="entityIdGet">A function to get the entity id.</param>
    /// <param name="dataIdGet">A function to get the data id.</param>
    /// <param name="updateAction">The action to update the entities.</param>
    /// <typeparam name="TData">The type of the data used to update the entities.</typeparam>
    /// <typeparam name="TId">The type of the id.</typeparam>
    void UpdateAll<TData, TId>(
        SearchCriteria searchCriteria,
        ICollection<TData> collection,
        Func<TEntity, TId> entityIdGet,
        Func<TData, TId> dataIdGet,
        Action<TEntity, TData> updateAction)
        where TData : class;

    /// <summary>
    /// Update all entities that meet the criteria.
    /// </summary>
    /// <param name="searchCriteria">The criteria for the query.</param>
    /// <param name="collection">The collection of data used to update the entities.</param>
    /// <param name="entityIdGet">The function to get the entity id.</param>
    /// <param name="dataIdGet">The function to get the data id.</param>
    /// <param name="updateAction">The action to update the entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TData">The type of the data used to update the entities.</typeparam>
    /// <typeparam name="TId">The type of the id.</typeparam>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAllAsync<TData, TId>(
        SearchCriteria searchCriteria,
        ICollection<TData> collection,
        Func<TEntity, TId> entityIdGet,
        Func<TData, TId> dataIdGet,
        Action<TEntity, TData> updateAction,
        CancellationToken cancellationToken = default)
        where TData : class;
}
