
using System.Linq.Expressions;

namespace RoyalCode.Searches.Abstractions;

/// <summary>
/// <para>
///     Component to perform entity searches, being able to apply multiple filters,
///     sorting, define paging, include projections, and determine which data must be selected.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface ISearch<TEntity> : ISearchOptions<ISearch<TEntity>>
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
    ISearch<TEntity> FilterBy<TFilter>(TFilter filter)
        where TFilter : class;

    /// <summary>
    /// <para>
    ///     Adds a sorting object to be applied to the search.
    /// </para>
    /// </summary>
    /// <param name="sorting">The sorting object.</param>
    /// <returns>The same instance for chaining calls.</returns>
    ISearch<TEntity> OrderBy(ISorting sorting);

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
    Task<IResultList<TEntity>> ToListAsync(CancellationToken token);

    /// <summary>
    /// Creates a result list that will return the searched entities asynchronously.
    /// </summary>
    /// <param name="token">The task cancellation token.</param>
    /// <returns>A task to wait for the async list of results.</returns>
    Task<IAsyncResultList<TEntity>> ToAsyncListAsync(CancellationToken token);
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
public interface ISearch<TEntity, TDto> : ISearchOptions<ISearch<TEntity, TDto>>
    where TEntity : class
    where TDto : class
{
    /// <summary>
    /// <para>
    ///     Adds a filter object to the search.
    /// </para>
    /// <para>
    ///     The search engine must be able to apply this filter, otherwise an exception will be throwed.
    /// </para>
    /// <para>
    ///     This filter will be applied against the <typeparamref name="TDto"/> query type.
    /// </para>
    /// </summary>
    /// <typeparam name="TFilter">The filter type.</typeparam>
    /// <param name="filter">The filter object.</param>
    /// <returns>The same instance for chaining calls.</returns>
    ISearch<TEntity, TDto> FilterBy<TFilter>(TFilter filter)
        where TFilter : class;
    
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
    Task<IResultList<TDto>> ToListAsync(CancellationToken token);

    /// <summary>
    /// Creates a result list that will return the searched entities asynchronously.
    /// </summary>
    /// <param name="token">The task cancellation token.</param>
    /// <returns>A task to wait for the async list of results.</returns>
    Task<IAsyncResultList<TDto>> ToAsyncListAsync(CancellationToken token);
}