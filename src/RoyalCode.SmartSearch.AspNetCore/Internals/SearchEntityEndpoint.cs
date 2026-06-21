using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoyalCode.SmartProblems;
using RoyalCode.SmartSearch.AspNetCore.HttpResults;

namespace RoyalCode.SmartSearch.AspNetCore.Internals;

/// <summary>
/// It allows you to customize the paged, sorted and filtered search for entities, applying additional logic via delegate.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
/// <typeparam name="TFilter">The type of the applied filter.</typeparam>
public class SearchEntityEndpoint<TEntity, TFilter>
    where TEntity : class
    where TFilter : class
{
    private readonly Action<ICriteria<TEntity>>? searchAction;

    /// <summary>
    /// Initializes a new instance of <see cref="SearchEntityEndpoint{TEntity, TFilter}"/>.
    /// </summary>
    /// <param name="searchAction">Custom action to configure the search.</param>
    public SearchEntityEndpoint(Action<ICriteria<TEntity>>? searchAction = null)
    {
        this.searchAction = searchAction;
    }

    /// <summary>
    /// Executes the custom search, applying filters, sorting, pagination, and additional logic, returning paginated entities.
    /// </summary>
    /// <param name="filtro">Filter object with search criteria.</param>
    /// <param name="options">Pagination and counting parameters.</param>
    /// <param name="orderby">Sorting criteria.</param>
    /// <param name="criteria">Search service for the entity.</param>
    /// <param name="logger">Logger for logging errors.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP 200 result with paginated list, 204 if empty, or 400 on sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public Task<MatchSearch<TEntity>> Search(
        [AsParameters] TFilter filtro,
        [AsParameters] SearchOptions options,
        [FromQuery] Sorting[]? orderby,
        [FromServices] ICriteria<TEntity> criteria,
        [FromServices] ILogger<ICriteria<TEntity>>? logger,
        CancellationToken ct)
    {
        return Performer.SearchAsync(
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
/// It allows you to customize the paginated, sorted and filtered search for entities, using an additional identifier and custom logic.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
/// <typeparam name="TFilter">The type of the applied filter.</typeparam>
/// <typeparam name="TId">The type of the additional identifier.</typeparam>
public class SearchEntityEndpoint<TEntity, TFilter, TId>
    where TEntity : class
    where TFilter : class
{
    private readonly Action<TId, ICriteria<TEntity>>? searchAction;

    /// <summary>
    /// Initializes a new instance of <see cref="SearchEntityEndpoint{TEntity, TFilter, TId}"/>.
    /// </summary>
    /// <param name="searchAction">Custom action to configure the search with identifier.</param>
    public SearchEntityEndpoint(Action<TId, ICriteria<TEntity>>? searchAction = null)
    {
        this.searchAction = searchAction;
    }

    /// <summary>
    /// Executes the custom search, applying filters, sorting, pagination, and additional logic, returning paginated entities.
    /// </summary>
    /// <param name="id">Additional identifier for the search.</param>
    /// <param name="filtro">Filter object with search criteria.</param>
    /// <param name="options">Pagination and counting parameters.</param>
    /// <param name="orderby">Sorting criteria.</param>
    /// <param name="criteria">Search service for the entity.</param>
    /// <param name="logger">Logger for logging errors.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP 200 result with paginated list, 204 if empty, or 400 on sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public Task<MatchSearch<TEntity>> Search(
        [FromRoute] TId id,
        [AsParameters] TFilter filtro,
        [AsParameters] SearchOptions options,
        [FromQuery] Sorting[]? orderby,
        [FromServices] ICriteria<TEntity> criteria,
        [FromServices] ILogger<ICriteria<TEntity>>? logger,
        CancellationToken ct)
    {
        Action<ICriteria<TEntity>>? action = null;
        if (searchAction is not null)
            action = c => searchAction(id, c);

        return Performer.SearchAsync(
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
/// It allows you to customize the paginated, sorted and filtered search for entities, using two additional identifiers and custom logic.
/// </summary>
/// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
/// <typeparam name="TFilter">The type of the applied filter.</typeparam>
/// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
/// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
public class SearchEntityEndpoint<TEntity, TFilter, TId1, TId2>
    where TEntity : class
    where TFilter : class
{
    private readonly Action<TId1, TId2, ICriteria<TEntity>>? searchAction;

    /// <summary>
    /// Initializes a new instance of <see cref="SearchEntityEndpoint{TEntity, TFilter, TId1, TId2}"/>.
    /// </summary>
    /// <param name="searchAction">Custom action to configure the search with identifiers.</param>
    public SearchEntityEndpoint(Action<TId1, TId2, ICriteria<TEntity>>? searchAction = null)
    {
        this.searchAction = searchAction;
    }

    /// <summary>
    /// Executes the custom search, applying filters, sorting, pagination, and additional logic, returning paginated entities.
    /// </summary>
    /// <param name="id">The first additional identifier for the search.</param>
    /// <param name="relatedId">The second additional identifier for the search.</param>
    /// <param name="filtro">Filter object with search criteria.</param>
    /// <param name="options">Pagination and counting parameters.</param>
    /// <param name="orderby">Sorting criteria.</param>
    /// <param name="criteria">Search service for the entity.</param>
    /// <param name="logger">Logger for logging errors.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP 200 result with paginated list, 204 if empty, or 400 on sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public Task<MatchSearch<TEntity>> Search(
        [FromRoute] TId1 id,
        [FromRoute] TId2 relatedId,
        [AsParameters] TFilter filtro,
        [AsParameters] SearchOptions options,
        [FromQuery] Sorting[]? orderby,
        [FromServices] ICriteria<TEntity> criteria,
        [FromServices] ILogger<ICriteria<TEntity>>? logger,
        CancellationToken ct)
    {
        Action<ICriteria<TEntity>>? action = null;
        if (searchAction is not null)
            action = c => searchAction(id, relatedId, c);

        return Performer.SearchAsync(
            filtro,
            options,
            orderby,
            criteria,
            action,
            logger,
            ct);
    }
}
