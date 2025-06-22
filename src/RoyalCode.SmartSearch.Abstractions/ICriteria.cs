using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Abstractions;

/// <summary>
/// <para>
///     Represents a set of criteria for querying entities, allowing the application of multiple filters,
///     sorting, projections, and selection of specific data. The <see cref="ICriteria{TEntity}"/> interface
///     is designed for scenarios where entities are to be collected or obtained directly, and these entities
///     will be tracked by the change tracker (e.g., in an ORM context such as Entity Framework).
/// </para>
/// <para>
///     When using methods like <see cref="Collect"/>, <see cref="First"/>, or <see cref="Single"/>,
///     the returned entities are tracked by the change tracker, enabling change detection and persistence.
/// </para>
/// <para>
///     To perform a search where entities are not tracked (i.e., detached from the change tracker),
///     convert the criteria to a search using <see cref="AsSearch"/> or <see cref="Select{TDto}"/>.
///     The resulting <see cref="ISearch{TEntity}"/> or <see cref="ISearch{TEntity, TDto}"/> will return
///     entities or DTOs that are not tracked by the change tracker.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface ICriteria<TEntity> : ICriteriaOptions<ICriteria<TEntity>>
    where TEntity : class
{
    /// <summary>
    /// <para>
    ///     Adds a filter object to the criteria.
    /// </para>
    /// <para>
    ///     The search engine must be able to apply this filter, otherwise an exception will be throwed.
    /// </para>
    /// </summary>
    /// <typeparam name="TFilter">The filter type.</typeparam>
    /// <param name="filter">The filter object.</param>
    /// <returns>The same instance for chaining calls.</returns>
    ICriteria<TEntity> FilterBy<TFilter>(TFilter filter)
        where TFilter : class;

    /// <summary>
    /// <para>
    ///     Adds a sorting object to be applied to the criteria.
    /// </para>
    /// </summary>
    /// <param name="sorting">The sorting object.</param>
    /// <returns>The same instance for chaining calls.</returns>
    ICriteria<TEntity> OrderBy(ISorting sorting);

    /// <summary>
    /// <para>
    ///     Adds multiple sorting objects to be applied to the criteria.
    /// </para>
    /// </summary>
    /// <param name="sorting">The array of sorting objects.</param>
    /// <returns>The same instance for chaining calls.</returns>
    ICriteria<TEntity> OrderBy(ISorting[]? sorting);

    /// <summary>
    /// <para>
    ///     Convert the criteria to a search.
    /// </para>
    /// <para>
    ///     When this method is called, the criteria will be converted to a search,
    ///     and the entities will not be tracked by a unit of work or a context.
    /// </para>
    /// </summary>
    /// <returns>new instance of <see cref="ISearch{TEntity}"/> with the same filters.</returns>
    ISearch<TEntity> AsSearch();

    /// <summary>
    /// <para>
    ///     Requires a Select, adapting the Entity to a DTO.
    /// </para>
    /// <para>
    ///     In this method the adaptation of the entity to the DTO will be done by the search engine.
    /// </para>
    /// <para>
    ///     When this method is called, the criteria will be converted to a search,
    ///     and the entities will not be tracked by a unit of work or a context.
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
    /// <para>
    ///     When this method is called, the criteria will be converted to a search,
    ///     and the entities will not be tracked by a unit of work or a context.
    /// </para>
    /// </summary>
    /// <typeparam name="TDto">The Data Transfer Object type.</typeparam>
    /// <param name="selectExpression">Expression to adapt the entity to the DTO.</param>
    /// <returns>new instance of <see cref="ISearch{TEntity, TDto}"/> with the same filters.</returns>
    ISearch<TEntity, TDto> Select<TDto>(Expression<Func<TEntity, TDto>> selectExpression)
        where TDto : class;

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
}
