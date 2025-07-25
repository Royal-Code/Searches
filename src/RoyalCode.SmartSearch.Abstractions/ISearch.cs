using System.Linq.Expressions;

namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     Component to perform entity searches, being able to apply multiple filters,
///     sorting, define paging, include projections, and determine which data must be selected.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface ISearch<TEntity>
    where TEntity : class
{
    /// <summary>
    /// <para>
    ///     Requires a Select, adapting the Entity to a DTO.
    /// </para>
    /// <para>
    ///     In this method the adaptation of the entity to the DTO will be done by the search engine.
    /// </para>
    /// </summary>
    /// <typeparam name="TDto">The Data Transfer Object type.</typeparam>
    /// <returns>new instance of <see cref="ISearch{TEntity, TDto}"/> with the same filters.</returns>
    ISearch<TEntity, TDto> Select<TDto>()
        where TDto : class;

    /// <summary>
    /// <para>
    ///     Requires a Select, adapting the Entity to a DTO.
    /// </para>
    /// <para>
    ///     In this method, the expression is required to adapt the entity to the DTO.
    /// </para>
    /// </summary>
    /// <typeparam name="TDto">The Data Transfer Object type.</typeparam>
    /// <param name="selectExpression">Expression to adapt the entity to the DTO.</param>
    /// <returns>new instance of <see cref="ISearch{TEntity, TDto}"/> with the same filters.</returns>
    ISearch<TEntity, TDto> Select<TDto>(Expression<Func<TEntity, TDto>> selectExpression)
        where TDto : class;

    /// <summary>
    /// It searches for the entities and returns them in a list of results.
    /// </summary>
    /// <returns>The result list.</returns>
    IResultList<TEntity> ToList();

    /// <summary>
    /// It async searches for the entities and returns them in a list of results.
    /// </summary>
    /// <param name="token">The task cancellation token.</param>
    /// <returns>A task to wait for the list of results.</returns>
    Task<IResultList<TEntity>> ToListAsync(CancellationToken token = default);

    /// <summary>
    /// Creates a result list that will return the searched entities asynchronously.
    /// </summary>
    /// <param name="token">The task cancellation token.</param>
    /// <returns>A task to wait for the async list of results.</returns>
    Task<IAsyncResultList<TEntity>> ToAsyncListAsync(CancellationToken token = default);

    /// <summary>
    /// Apply the filters and sorting and get the first entity that meets the criteria.
    /// </summary>
    /// <returns>
    ///     The entity or null if there are no entities that meet the criteria.
    /// </returns>
    TEntity? FirstOrDefault();

    /// <summary>
    /// Apply the filters and sorting and get the first entity that meets the criteria.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The entity or null if there are no entities that meet the criteria.
    /// </returns>
    Task<TEntity?> FirstDefaultAsync(CancellationToken cancellationToken = default);

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
}

/// <summary>
/// <para>
///     Component to perform entity searches, being able to apply multiple filters,
///     sorting, define paging, include projections, and determine which data must be selected.
/// </para>
/// <para>
///     Objects of this class is created by the <see cref="ISearch{TEntity}"/>.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TDto">The Data Transfer Object type.</typeparam>
public interface ISearch<TEntity, TDto>
    where TEntity : class
    where TDto : class
{
    /// <summary>
    /// It searches for the entities and returns them in a list of results.
    /// </summary>
    /// <returns>The result list.</returns>
    IResultList<TDto> ToList();

    /// <summary>
    /// It async searches for the entities and returns them in a list of results.
    /// </summary>
    /// <param name="token">The task cancellation token.</param>
    /// <returns>A task to wait for the list of results.</returns>
    Task<IResultList<TDto>> ToListAsync(CancellationToken token = default);

    /// <summary>
    /// Creates a result list that will return the searched entities asynchronously.
    /// </summary>
    /// <param name="token">The task cancellation token.</param>
    /// <returns>A task to wait for the async list of results.</returns>
    Task<IAsyncResultList<TDto>> ToAsyncListAsync(CancellationToken token = default);

    /// <summary>
    /// Apply the filters and sorting and get the first entity that meets the criteria.
    /// </summary>
    /// <returns>
    ///     The entity or null if there are no entities that meet the criteria.
    /// </returns>
    TDto? FirstOrDefault();

    /// <summary>
    /// Apply the filters and sorting and get the first entity that meets the criteria.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The entity or null if there are no entities that meet the criteria.
    /// </returns>
    Task<TDto?> FirstOrDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply the filters and sorting and get the first entity that meets the criteria,
    /// or throw an exception if there are no entities that meet the criteria or more than one.
    /// </summary>
    /// <returns>
    ///     The entity that meets the criteria 
    ///     or throw an exception if there are no entities that meet the criteria or more than one.
    /// </returns>
    TDto Single();

    /// <summary>
    /// Apply the filters and sorting and get the first entity that meets the criteria,
    /// or throw an exception if there are no entities that meet the criteria or more than one.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The entity that meets the criteria
    ///     or throw an exception if there are no entities that meet the criteria or more than one.
    /// </returns>
    Task<TDto> SingleAsync(CancellationToken cancellationToken = default);
}