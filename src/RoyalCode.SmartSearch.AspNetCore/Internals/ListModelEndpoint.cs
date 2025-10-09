using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoyalCode.SmartProblems;
using RoyalCode.SmartSearch.AspNetCore.HttpResults;

namespace RoyalCode.SmartSearch.AspNetCore.Internals;

/// <summary>
/// Endpoint for filtered and sorted listing of DTOs.
/// </summary>
/// <typeparam name="TEntity">The type of entity to be queried.</typeparam>
/// <typeparam name="TDto">The type of DTO to be returned.</typeparam>
/// <typeparam name="TFilter">The type of filter applied.</typeparam>
public class ListModelEndpoint<TEntity, TDto, TFilter>
    where TEntity : class
    where TDto : class
    where TFilter : class
{
    private readonly Action<ICriteria<TEntity>>? listAction;

    /// <summary>
    /// Initializes a new instance of <see cref="ListModelEndpoint{TEntity, TDto, TFilter}"/>.
    /// </summary>
    /// <param name="listAction">Action to customize the listing criteria.</param>
    public ListModelEndpoint(Action<ICriteria<TEntity>>? listAction = null)
    {
        this.listAction = listAction;
    }

    /// <summary>
    /// Performs a filtered and sorted list of DTOs.
    /// </summary>
    /// <param name="filtro">A filter object with search criteria.</param>
    /// <param name="orderby">Ordering criteria.</param>
    /// <param name="criteria">A search service for the entity.</param>
    /// <param name="logger">Logger for logging errors.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>HTTP 200 result with list, 204 if empty, or 400 on sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public Task<MatchList<TDto>> List(
        [AsParameters] TFilter filtro,
        [FromQuery] Sorting[]? orderby,
        [FromServices] ICriteria<TEntity> criteria,
        [FromServices] ILogger<ICriteria<TEntity>>? logger,
        CancellationToken ct)
    {
        return Performer.ListAsync<TEntity, TDto, TFilter>(
            filtro,
            orderby,
            criteria,
            listAction,
            logger,
            ct);
    }
}

/// <summary>
/// Endpoint for filtered and sorted listing of DTOs with additional identifier.
/// </summary>
/// <typeparam name="TEntity">The type of entity to be queried.</typeparam>
/// <typeparam name="TDto">The type of DTO to be returned.</typeparam>
/// <typeparam name="TFilter">The type of filter applied.</typeparam>
/// <typeparam name="TId">The type of the additional identifier.</typeparam>
public class ListModelEndpoint<TEntity, TDto, TFilter, TId>
    where TEntity : class
    where TDto : class
    where TFilter : class
{
    private readonly Action<TId, ICriteria<TEntity>>? listAction;

    /// <summary>
    /// Initializes a new instance of <see cref="ListModelEndpoint{TEntity, TDto, TFilter, TId}"/>.
    /// </summary>
    /// <param name="listAction">Action to customize the listing criteria with an additional identifier.</param>
    public ListModelEndpoint(Action<TId, ICriteria<TEntity>>? listAction = null)
    {
        this.listAction = listAction;
    }

    /// <summary>
    /// Performs a filtered and sorted list of DTOs with an additional identifier.
    /// </summary>
    /// <param name="id">The additional identifier for the list.</param>
    /// <param name="filtro">A filter object with search criteria.</param>
    /// <param name="orderby">Ordering criteria.</param>
    /// <param name="criteria">A search service for the entity.</param>
    /// <param name="logger">Logger for logging errors.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>HTTP 200 result with list, 204 if empty, or 400 on sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public Task<MatchList<TDto>> List(
        [FromRoute] TId id,
        [AsParameters] TFilter filtro,
        [FromQuery] Sorting[]? orderby,
        [FromServices] ICriteria<TEntity> criteria,
        [FromServices] ILogger<ICriteria<TEntity>>? logger,
        CancellationToken ct)
    {
        Action<ICriteria<TEntity>>? action = null;
        if (listAction is not null)
            action = c => listAction(id, c);

        return Performer.ListAsync<TEntity, TDto, TFilter>(
            filtro,
            orderby,
            criteria,
            action,
            logger,
            ct);
    }
}

/// <summary>
/// Endpoint for filtered and sorted listing of DTOs with two additional identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entity to be queried.</typeparam>
/// <typeparam name="TDto">The type of DTO to be returned.</typeparam>
/// <typeparam name="TFilter">The type of filter applied.</typeparam>
/// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
/// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
public class ListModelEndpoint<TEntity, TDto, TFilter, TId1, TId2>
    where TEntity : class
    where TDto : class
    where TFilter : class
{
    private readonly Action<TId1, TId2, ICriteria<TEntity>>? listAction;

    /// <summary>
    /// Initializes a new instance of <see cref="ListModelEndpoint{TEntity, TDto, TFilter, TId1, TId2}"/>.
    /// </summary>
    /// <param name="listAction">Action to customize the listing criteria with two additional identifiers.</param>
    public ListModelEndpoint(Action<TId1, TId2, ICriteria<TEntity>>? listAction = null)
    {
        this.listAction = listAction;
    }

    /// <summary>
    /// Performs a filtered and sorted list of DTOs with two additional identifiers.
    /// </summary>
    /// <param name="id">The first additional identifier for the list.</param>
    /// <param name="relatedId">The second additional identifier for the list.</param>
    /// <param name="filtro">A filter object with search criteria.</param>
    /// <param name="orderby">Ordering criteria.</param>
    /// <param name="criteria">A search service for the entity.</param>
    /// <param name="logger">Logger for logging errors.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>HTTP 200 result with list, 204 if empty, or 400 on sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public Task<MatchList<TDto>> List(
        [FromRoute] TId1 id,
        [FromRoute] TId2 relatedId,
        [AsParameters] TFilter filtro,
        [FromQuery] Sorting[]? orderby,
        [FromServices] ICriteria<TEntity> criteria,
        [FromServices] ILogger<ICriteria<TEntity>>? logger,
        CancellationToken ct)
    {
        Action<ICriteria<TEntity>>? action = null;
        if (listAction is not null)
            action = c => listAction(id, relatedId, c);

        return Performer.ListAsync<TEntity, TDto, TFilter>(
            filtro,
            orderby,
            criteria,
            action,
            logger,
            ct);
    }
}
