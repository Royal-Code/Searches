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

    /// <summary>
    /// Apply the filters and sorting and verify if there are any entities that meet the criteria.
    /// </summary>
    /// <returns>
    ///     True if there are entities that meet the criteria, otherwise false.
    /// </returns>
    bool Exists();

    /// <summary>
    /// Apply the filters and sorting and verify if there are any entities that meet the criteria.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     True if there are entities that meet the criteria, otherwise false.
    /// </returns>
    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);

    // Opções First, FirstAsync, Single, SingleAsync, e OrDefault.

    /// <summary>
    /// Apply the filters and sorting and get the first entity that meets the criteria.
    /// </summary>
    /// <returns>
    ///     The entity or null if there are no entities that meet the criteria.
    /// </returns>
    TEntity? First();

    /// <summary>
    /// Apply the filters and sorting and get the first entity that meets the criteria.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The entity or null if there are no entities that meet the criteria.
    /// </returns>
    Task<TEntity?> FirstAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply the filters and sorting and get the first entity that meets the criteria,
    /// or throw an exception if there are no entities that meet the criteria or more than one.
    /// </summary>
    /// <returns>
    ///     The entity that meets the criteria 
    ///     or throw an exception if there are no entities that meet the criteria or more than one.
    /// </returns>
    TEntity Single();

    /// <summary>
    /// Apply the filters and sorting and get the first entity that meets the criteria,
    /// or throw an exception if there are no entities that meet the criteria or more than one.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The entity that meets the criteria
    ///     or throw an exception if there are no entities that meet the criteria or more than one.
    /// </returns>
    Task<TEntity> SingleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply the filters and sorting and delete all the entities that meet the criteria.
    /// </summary>
    void DeleteAll();

    /// <summary>
    /// Apply the filters and sorting and delete all the entities that meet the criteria.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply the filters and sorting and update all the entities that meet the criteria.
    /// </summary>
    /// <param name="updateAction">The action to update the entities.</param>
    void UpdateAll(Action<TEntity> updateAction);

    /// <summary>
    /// Apply the filters and sorting and update all entities that meet the criteria.
    /// </summary>
    /// <param name="data">The data used to update the entities.</param>
    /// <param name="updateAction">The action to update the entities.</param>
    /// <typeparam name="TData">The type of the data used to update the entities.</typeparam>
    void UpdateAll<TData>(TData data, Action<TEntity, TData> updateAction);

    /// <summary>
    /// Apply the filters and sorting and update all entities that meet the criteria.
    /// </summary>
    /// <param name="collection">A collection of data used to update the entities.</param>
    /// <param name="entityIdGet">The function to get the entity id.</param>
    /// <param name="dataIdGet">The function to get the data id.</param>
    /// <param name="updateAction">The action to update the entities.</param>
    /// <typeparam name="TData">The type of the data used to update the entities.</typeparam>
    /// <typeparam name="TId">The type of the id.</typeparam>
    void UpdateAll<TData, TId>(
        ICollection<TData> collection,
        Func<TEntity, TId> entityIdGet,
        Func<TData, TId> dataIdGet,
        Action<TEntity, TData> updateAction)
        where TData : class;

    /// <summary>
    /// Apply the filters and sorting and update all the entities that meet the criteria.
    /// </summary>
    /// <param name="updateAction">The action to update the entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAllAsync(
        Action<TEntity> updateAction,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Apply the filters and sorting and update all entities that meet the criteria.
    /// </summary>
    /// <param name="data">The data used to update the entities.</param>
    /// <param name="updateAction">The action to update the entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TData">The type of the data used to update the entities.</typeparam>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAllAsync<TData>(
        TData data,
        Action<TEntity, TData> updateAction,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Apply the filters and sorting and update all entities that meet the criteria.
    /// </summary>
    /// <param name="collection">The collection of data used to update the entities.</param>
    /// <param name="entityIdGet">The function to get the entity id.</param>
    /// <param name="dataIdGet">The function to get the data id.</param>
    /// <param name="updateAction">The action to update the entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="TData">The type of the data used to update the entities.</typeparam>
    /// <typeparam name="TId">The type of the id.</typeparam>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAllAsync<TData, TId>(
        ICollection<TData> collection,
        Func<TEntity, TId> entityIdGet,
        Func<TData, TId> dataIdGet,
        Action<TEntity, TData> updateAction,
        CancellationToken cancellationToken = default)
        where TData : class;
}