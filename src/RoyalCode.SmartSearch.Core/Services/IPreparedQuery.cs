using RoyalCode.SmartSearch.Mappings;

namespace RoyalCode.SmartSearch.Services;

/// <summary>
/// <para>
///     Represents a prepared query for a specific entity type, allowing execution of search operations
///     such as existence checks, retrieval of single or multiple results, and projection to DTOs.
/// </para>
/// <para>
///     This interface provides both synchronous and asynchronous methods for querying, as well as support
///     for result list abstractions and custom select projections.
/// </para>
/// </summary>
/// <typeparam name="T">The entity type for which the query is prepared.</typeparam>
public interface IPreparedQuery<T> 
    where T : class
{
    /// <summary>
    /// Determines whether any entities exist that match the prepared query.
    /// </summary>
    /// <returns>True if any entities exist; otherwise, false.</returns>
    bool Exists();

    /// <summary>
    /// Asynchronously determines whether any entities exist that match the prepared query.
    /// </summary>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if any entities exist; otherwise, false.</returns>
    Task<bool> ExistsAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the first entity that matches the prepared query, or null if no entity is found.
    /// </summary>
    /// <returns>The first entity found, or null if no entity matches the query.</returns>
    T? FirstOrDefault();

    /// <summary>
    /// Asynchronously returns the first entity that matches the prepared query, or null if no entity is found.
    /// </summary>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first entity found, or null if no entity matches the query.</returns>
    Task<T?> FirstOrDefaultAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the single entity that matches the prepared query.
    /// </summary>
    /// <returns>The single entity found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no entity or more than one entity matches the query.</exception>
    T Single();

    /// <summary>
    /// Asynchronously returns the single entity that matches the prepared query.
    /// </summary>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the single entity found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no entity or more than one entity matches the query.</exception>
    Task<T> SingleAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns a read-only list of all entities that match the prepared query.
    /// </summary>
    /// <returns>A read-only list of entities.</returns>
    IReadOnlyList<T> ToList();

    /// <summary>
    /// Asynchronously returns a read-only list of all entities that match the prepared query.
    /// </summary>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of entities.</returns>
    Task<IReadOnlyList<T>> ToListAsync(CancellationToken ct);

    /// <summary>
    /// Returns a result list abstraction containing all entities that match the prepared query.
    /// </summary>
    /// <returns>An <see cref="IResultList{T}"/> containing the entities and additional result metadata.</returns>
    IResultList<T> ToResultList();

    /// <summary>
    /// Asynchronously returns a result list abstraction containing all entities that match the prepared query.
    /// </summary>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IResultList{T}"/>.</returns>
    Task<IResultList<T>> ToResultListAsync(CancellationToken ct);

    /// <summary>
    /// Asynchronously returns an asynchronous result list abstraction containing all entities that match the prepared query.
    /// </summary>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IAsyncResultList{T}"/>.</returns>
    Task<IAsyncResultList<T>> ToAsyncListAsync(CancellationToken ct);

    /// <summary>
    /// Projects the prepared query to a different DTO type using the specified select mapping.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to project to.</typeparam>
    /// <param name="select">The select mapping to apply for the projection.</param>
    /// <returns>A new <see cref="IPreparedQuery{TDto}"/> representing the projected query.</returns>
    IPreparedQuery<TDto> Select<TDto>(ISearchSelect<T, TDto> select)
        where TDto : class;
}
