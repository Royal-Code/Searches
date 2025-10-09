using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoyalCode.SmartProblems;
using RoyalCode.SmartSearch.AspNetCore.HttpResults;

namespace RoyalCode.SmartSearch.AspNetCore.Internals;

/// <summary>
/// It allows you to customize the paginated, sorted and filtered search of entities, with projection to DTO, applying additional logic via delegate.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
/// <typeparam name="TDto">The type of the DTO to be returned.</typeparam>
/// <typeparam name="TFilter">The type of the applied filter.</typeparam>
public class SearchModelEndpoint<TEntity, TDto, TFilter>
    where TEntity : class
    where TDto : class
    where TFilter : class
{
    private readonly Action<ICriteria<TEntity>>? searchAction;

    /// <summary>
    /// Initializes a new instance of <see cref="SearchModelEndpoint{TEntity, TDto, TFilter}"/>.
    /// </summary>
    /// <param name="searchAction">Custom action to configure the search.</param>
    public SearchModelEndpoint(Action<ICriteria<TEntity>>? searchAction = null)
    {
        this.searchAction = searchAction;
    }

    /// <summary>
    /// Performs customized searches, applying filters, sorting, pagination and additional logic, returning paginated DTOs.
    /// </summary>
    /// <param name="filtro">Filter object with search criteria.</param>
    /// <param name="options">Paging and counting parameters.</param>
    /// <param name="orderby">Sorting criteria.</param>
    /// <param name="criteria">Search service for the entity.</param>
    /// <param name="logger">Logger for logging errors.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP result 200 with paginated list, 204 if empty, or 400 in sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public Task<MatchSearch<TDto>> Search(
        [AsParameters] TFilter filtro,
        [AsParameters] SearchOptions options,
        [FromQuery] Sorting[]? orderby,
        [FromServices] ICriteria<TEntity> criteria,
        [FromServices] ILogger<ICriteria<TEntity>>? logger,
        CancellationToken ct)
    {
        return Performer.SearchAsync<TEntity, TDto, TFilter>(
            filtro,
            options,
            orderby,
            criteria,
            searchAction,
            logger,
            ct);
    }
}

/// <summary>
/// It allows you to customize the paginated, sorted and filtered search for entities, with projection to DTO, using an additional identifier and custom logic.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
/// <typeparam name="TDto">The type of the DTO to be returned.</typeparam>
/// <typeparam name="TFilter">The type of the applied filter.</typeparam>
/// <typeparam name="TId">The type of the additional identifier.</typeparam>
public class SearchModelEndpoint<TEntity, TDto, TFilter, TId>
    where TEntity : class
    where TDto : class
    where TFilter : class
{
    private readonly Action<TId, ICriteria<TEntity>> searchAction;

    /// <summary>
    /// Initializes a new instance of <see cref="SearchModelEndpoint{TEntity, TDto, TFilter, TId}"/>.
    /// </summary>
    /// <param name="searchAction">Custom action to configure the search with identifier.</param>
    public SearchModelEndpoint(Action<TId, ICriteria<TEntity>> searchAction)
    {
        this.searchAction = searchAction;
    }

    /// <summary>
    /// Executes the customized search, applying filters, sorting, pagination and additional logic, returning paginated DTOs.
    /// </summary>
    /// <param name="id">The additional identifier for the search.</param>
    /// <param name="filtro">Filter object with search criteria.</param>
    /// <param name="options">Paging and counting parameters.</param>
    /// <param name="orderby">Sorting criteria.</param>
    /// <param name="criteria">Search service for the entity.</param>
    /// <param name="logger">Logger for logging errors.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP result 200 with paginated list, 204 if empty, or 400 in sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public Task<MatchSearch<TDto>> Search(
        [FromRoute] TId id,
        [AsParameters] TFilter filtro,
        [AsParameters] SearchOptions options,
        [FromQuery] Sorting[]? orderby,
        [FromServices] ICriteria<TEntity> criteria,
        [FromServices] ILogger<ICriteria<TEntity>>? logger,
        CancellationToken ct)
    {
        Action<ICriteria<TEntity>> action = c => searchAction(id, c);

        return Performer.SearchAsync<TEntity, TDto, TFilter>(
            filtro,
            options,
            orderby,
            criteria,
            action,
            logger,
            ct);
    }
}

/// <summary>
/// It allows you to customize the paginated, sorted and filtered search for entities, with projection to DTO, using an additional identifier and custom logic.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
/// <typeparam name="TDto">The type of the DTO to be returned.</typeparam>
/// <typeparam name="TFilter">The type of the applied filter.</typeparam>
/// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
/// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
public class SearchModelEndpoint<TEntity, TDto, TFilter, TId1, TId2>
    where TEntity : class
    where TDto : class
    where TFilter : class
{
    private readonly Action<TId1, TId2, ICriteria<TEntity>> searchAction;

    /// <summary>
    /// Initializes a new instance of <see cref="SearchModelEndpoint{TEntity, TDto, TFilter, TId}"/>.
    /// </summary>
    /// <param name="searchAction">Custom action to configure the search with identifiers.</param>
    public SearchModelEndpoint(Action<TId1, TId2, ICriteria<TEntity>> searchAction)
    {
        this.searchAction = searchAction;
    }

    /// <summary>
    /// Executes the customized search, applying filters, sorting, pagination and additional logic, returning paginated DTOs.
    /// </summary>
    /// <param name="id">The primary additional identifier for the search.</param>
    /// <param name="relatedId">The additional identifier, related to the first, for the search.</param>
    /// <param name="filtro">Filter object with search criteria.</param>
    /// <param name="options">Paging and counting parameters.</param>
    /// <param name="orderby">Sorting criteria.</param>
    /// <param name="criteria">Search service for the entity.</param>
    /// <param name="logger">Logger for logging errors.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP result 200 with paginated list, 204 if empty, or 400 in sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public Task<MatchSearch<TDto>> Search(
        [FromRoute] TId1 id,
        [FromRoute] TId2 relatedId,
        [AsParameters] TFilter filtro,
        [AsParameters] SearchOptions options,
        [FromQuery] Sorting[]? orderby,
        [FromServices] ICriteria<TEntity> criteria,
        [FromServices] ILogger<ICriteria<TEntity>>? logger,
        CancellationToken ct)
    {
        Action<ICriteria<TEntity>> action = c => searchAction(id, relatedId, c);

        return Performer.SearchAsync<TEntity, TDto, TFilter>(
            filtro,
            options,
            orderby,
            criteria,
            action,
            logger,
            ct);
    }
}