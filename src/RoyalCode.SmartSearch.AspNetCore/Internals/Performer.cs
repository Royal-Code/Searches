using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoyalCode.SmartProblems;
using RoyalCode.SmartSearch.AspNetCore.HttpResults;
using RoyalCode.SmartSearch.Exceptions;

namespace RoyalCode.SmartSearch.AspNetCore.Internals;

/// <summary>
/// Generic performer for executing searches with filtering and sorting, returning standardized HTTP results.
/// </summary>
public class Performer
{
    /// <summary>
    /// <para>
    ///     Asynchronously retrieves the first entity that matches the specified filter and sorting criteria,
    ///     or returns a result indicating no content if no entity is found.
    /// </para>
    /// </summary>
    /// <remarks>
    ///     If the sorting criteria are invalid, the method returns a result containing problem details
    ///     with information about the invalid parameter. If an unexpected error occurs during query execution,
    ///     the method returns a result with internal error details.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type to be queried.</typeparam>
    /// <typeparam name="TFilter">The filter type to be applied.</typeparam>
    /// <param name="filtro">
    ///     The filter criteria used to select entities.
    ///     Specifies the conditions that entities must satisfy to be considered for retrieval.
    /// </param>
    /// <param name="orderby">
    ///     An array of sorting instructions that determines the order in which entities are evaluated.
    ///     Can be null to use the default ordering.
    /// </param>
    /// <param name="criteria">
    ///     An object that provides query capabilities over the entity type.
    ///     Used to apply filtering and sorting operations.
    /// </param>
    /// <param name="configure">
    ///     An optional delegate that allows additional configuration of the query before execution.
    ///     Can be null if no further customization is needed.
    /// </param>
    /// <param name="logger">
    ///     An optional logger instance for logging errors and important events.
    /// </param>
    /// <param name="ct">
    ///     A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// 
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The result contains the first matching entity if found; otherwise, a result indicating no content.
    ///     If an error occurs, the result contains problem details describing the error.
    /// </returns>
    public static async Task<MatchFirst<TEntity>> FirstAsync<TEntity, TFilter>(
        TFilter filtro,
        Sorting[]? orderby,
        ICriteria<TEntity> criteria,
        Action<ICriteria<TEntity>>? configure,
        ILogger? logger,
        CancellationToken ct)
        where TEntity : class
        where TFilter : class
    {
        try
        {
            var search = criteria
                .OrderBy(orderby)
                .FilterBy(filtro);

            if (configure is not null)
                configure(search);

            var entity = await search.FirstOrDefaultAsync(ct);

            if (entity is null)
            {
                return TypedResults.NoContent();
            }

            return entity;
        }
        catch (OrderByException obex)
        {
            return Problems.InvalidParameter(obex.Message, nameof(orderby))
                .With("propertyName", obex.PropertyName)
                .With("typeName", obex.TypeName)
                .With("orderby", orderby);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex,
                "An error occurred while executing the query for the first entity of type {EntityType} with filter of type {FilterType}.",
                typeof(TEntity).Name, typeof(TFilter).Name);

            return Problems.InternalError(ex);
        }
    }

    /// <summary>
    /// <para>
    ///     Asynchronously retrieves the first entity matching the specified filter and sorting criteria,
    ///     projecting it to the specified DTO type.
    /// </para>
    /// </summary>
    /// <remarks>
    ///     If no entity matches the filter, the result indicates no content.
    ///     If an invalid sorting property is specified, an error result is returned.
    ///     Any other exceptions are logged and returned as internal errors.
    ///     This method does not throw exceptions for query errors; instead, it returns a result describing the error.
    /// </remarks>
    /// <typeparam name="TEntity">The type of the entity to query.</typeparam>
    /// <typeparam name="TDto">The type to which the entity is projected in the result.</typeparam>
    /// <typeparam name="TFilter">The type of the filter used to constrain the query.</typeparam>
    /// <param name="filtro">
    ///     The filter criteria used to select entities. Cannot be null.
    /// </param>
    /// <param name="orderby">
    ///     An array of sorting instructions that determines the order of the query results.
    ///     Can be null to use default ordering.
    /// </param>
    /// <param name="searchable">
    ///     An implementation of criteria for querying entities of type TEntity. Cannot be null.
    /// </param>
    /// <param name="configure">
    ///     An optional action to further configure the query criteria before execution.
    /// </param>
    /// <param name="logger">
    ///     An optional logger used to record errors that occur during query execution.
    /// </param>
    /// <param name="ct">
    ///     A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    ///     A MatchFirst{TDto} containing the first projected DTO if found;
    ///     otherwise, a result indicating no content or an error.
    /// </returns>
    public static async Task<MatchFirst<TDto>> FirstAsync<TEntity, TDto, TFilter>(
        [AsParameters] TFilter filtro,
        [FromQuery] Sorting[]? orderby,
        [FromServices] ICriteria<TEntity> searchable,
        Action<ICriteria<TEntity>>? configure,
        ILogger? logger,
        CancellationToken ct)
        where TEntity : class
        where TDto : class
        where TFilter : class
    {
        try
        {
            var search = searchable
                .OrderBy(orderby)
                .FilterBy(filtro);

            if (configure is not null)
                configure(search);

            var dto = await search.Select<TDto>().FirstOrDefaultAsync(ct);

            if (dto is null)
            {
                return TypedResults.NoContent();
            }

            return dto;
        }
        catch (OrderByException obex)
        {
            return Problems.InvalidParameter(obex.Message, nameof(orderby))
                .With("propertyName", obex.PropertyName)
                .With("typeName", obex.TypeName)
                .With("orderby", orderby);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex,
                "An error occurred while executing the query for the first entity of type {EntityType} with filter of type {FilterType}.",
                typeof(TEntity).Name, typeof(TFilter).Name);

            return Problems.InternalError(ex);
        }
    }

    /// <summary>
    /// <para>
    ///     Asynchronously retrieves a list of entities that match the specified filter and sorting criteria.
    /// </para>
    /// </summary>
    /// <remarks>
    ///     Returns 204 when no entities are found, 400 when an invalid sorting is provided and 500 on unexpected errors.
    /// </remarks>
    /// <typeparam name="TEntity">Entity type to be queried.</typeparam>
    /// <typeparam name="TFilter">Filter type applied to the query.</typeparam>
    /// <param name="filtro">Filter object with search criteria.</param>
    /// <param name="orderby">Sorting array or null for defaults.</param>
    /// <param name="criteria">Criteria component to build the query.</param>
    /// <param name="configure">Optional extra configuration delegate.</param>
    /// <param name="logger">Optional logger for errors.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="MatchList{TModel}"/> result.</returns>
    public static async Task<MatchList<TEntity>> ListAsync<TEntity, TFilter>(
        TFilter filtro,
        Sorting[]? orderby,
        ICriteria<TEntity> criteria,
        Action<ICriteria<TEntity>>? configure,
        ILogger? logger,
        CancellationToken ct)
        where TEntity : class
        where TFilter : class
    {
        try
        {
            var search = criteria
                .OrderBy(orderby)
                .FilterBy(filtro);

            configure?.Invoke(search);

            var result = await search.AsSearch().ToListAsync(ct);

            if (result.Count == 0)
                return TypedResults.NoContent();

            return TypedResults.Ok(result.Items);
        }
        catch (OrderByException obex)
        {
            return Problems.InvalidParameter(obex.Message, nameof(orderby))
                .With("propertyName", obex.PropertyName)
                .With("typeName", obex.TypeName)
                .With("orderby", orderby);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex,
                "An error occurred while listing entities of type {EntityType} with filter of type {FilterType}.",
                typeof(TEntity).Name, typeof(TFilter).Name);
            return Problems.InternalError(ex);
        }
    }

    /// <summary>
    /// <para>
    ///     Asynchronously retrieves a list of DTOs (projection) that match the specified filter and sorting criteria.
    /// </para>
    /// </summary>
    /// <remarks>
    ///     Returns 204 when no items are found, 400 when sorting is invalid and 500 on unexpected errors.
    /// </remarks>
    /// <typeparam name="TEntity">Entity type to be queried.</typeparam>
    /// <typeparam name="TDto">DTO projection type.</typeparam>
    /// <typeparam name="TFilter">Filter type applied to the query.</typeparam>
    /// <param name="filtro">Filter object with search criteria.</param>
    /// <param name="orderby">Sorting array or null for defaults.</param>
    /// <param name="criteria">Criteria component to build the query.</param>
    /// <param name="configure">Optional extra configuration delegate.</param>
    /// <param name="logger">Optional logger for errors.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="MatchList{TModel}"/> result.</returns>
    public static async Task<MatchList<TDto>> ListAsync<TEntity, TDto, TFilter>(
        TFilter filtro,
        Sorting[]? orderby,
        ICriteria<TEntity> criteria,
        Action<ICriteria<TEntity>>? configure,
        ILogger? logger,
        CancellationToken ct)
        where TEntity : class
        where TDto : class
        where TFilter : class
    {
        try
        {
            var search = criteria
                .OrderBy(orderby)
                .FilterBy(filtro);

            configure?.Invoke(search);

            var select = search.Select<TDto>();
            var result = await select.ToListAsync(ct);

            if (result.Count == 0)
                return TypedResults.NoContent();

            return TypedResults.Ok(result.Items);
        }
        catch (OrderByException obex)
        {
            return Problems.InvalidParameter(obex.Message, nameof(orderby))
                .With("propertyName", obex.PropertyName)
                .With("typeName", obex.TypeName)
                .With("orderby", orderby);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex,
                "An error occurred while listing DTOs ({DtoType}) for entity {EntityType} with filter {FilterType}.",
                typeof(TDto).Name, typeof(TEntity).Name, typeof(TFilter).Name);
            return Problems.InternalError(ex);
        }
    }

    /// <summary>
    /// <para>
    ///     Asynchronously performs a paged search returning entities.
    /// </para>
    /// </summary>
    /// <remarks>
    ///     Applies pagination, sorting and filtering. Returns 204 when empty, 400 for invalid sorting, 500 for unexpected errors.
    /// </remarks>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TFilter">Filter type.</typeparam>
    /// <param name="filtro">Filter instance.</param>
    /// <param name="options">Search/pagination options.</param>
    /// <param name="orderby">Sorting definitions.</param>
    /// <param name="criteria">Criteria component.</param>
    /// <param name="configure">Optional extra configuration delegate.</param>
    /// <param name="logger">Optional logger.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="MatchSearch{TModel}"/> result.</returns>
    public static async Task<MatchSearch<TEntity>> SearchAsync<TEntity, TFilter>(
        TFilter filtro,
        SearchOptions options,
        Sorting[]? orderby,
        ICriteria<TEntity> criteria,
        Action<ICriteria<TEntity>>? configure,
        ILogger? logger,
        CancellationToken ct)
        where TEntity : class
        where TFilter : class
    {
        try
        {
            var search = criteria
                .WithOptions(options)
                .OrderBy(orderby)
                .FilterBy(filtro);

            configure?.Invoke(search);

            var result = await search.AsSearch().ToListAsync(ct);

            if (result.Count == 0)
                return TypedResults.NoContent();

            return TypedResults.Ok(result);
        }
        catch (OrderByException obex)
        {
            return Problems.InvalidParameter(obex.Message, nameof(orderby))
                .With("propertyName", obex.PropertyName)
                .With("typeName", obex.TypeName)
                .With("orderby", orderby);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex,
                "An error occurred while performing paged search for entity {EntityType} with filter {FilterType}.",
                typeof(TEntity).Name, typeof(TFilter).Name);
            return Problems.InternalError(ex);
        }
    }

    /// <summary>
    /// <para>
    ///     Asynchronously performs a paged search returning DTOs (projection).
    /// </para>
    /// </summary>
    /// <remarks>
    ///     Applies pagination, sorting and filtering. Returns 204 when empty, 400 for invalid sorting, 500 for unexpected errors.
    /// </remarks>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TDto">DTO projection type.</typeparam>
    /// <typeparam name="TFilter">Filter type.</typeparam>
    /// <param name="filtro">Filter instance.</param>
    /// <param name="options">Search/pagination options.</param>
    /// <param name="orderby">Sorting definitions.</param>
    /// <param name="criteria">Criteria component.</param>
    /// <param name="configure">Optional extra configuration delegate.</param>
    /// <param name="logger">Optional logger.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="MatchSearch{TModel}"/> result.</returns>
    public static async Task<MatchSearch<TDto>> SearchAsync<TEntity, TDto, TFilter>(
        TFilter filtro,
        SearchOptions options,
        Sorting[]? orderby,
        ICriteria<TEntity> criteria,
        Action<ICriteria<TEntity>>? configure,
        ILogger? logger,
        CancellationToken ct)
        where TEntity : class
        where TDto : class
        where TFilter : class
    {
        try
        {
            var search = criteria
                .WithOptions(options)
                .OrderBy(orderby)
                .FilterBy(filtro);

            configure?.Invoke(search);

            var select = search.Select<TDto>();
            var result = await select.ToListAsync(ct);

            if (result.Count == 0)
                return TypedResults.NoContent();

            return TypedResults.Ok(result);
        }
        catch (OrderByException obex)
        {
            return Problems.InvalidParameter(obex.Message, nameof(orderby))
                .With("propertyName", obex.PropertyName)
                .With("typeName", obex.TypeName)
                .With("orderby", orderby);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex,
                "An error occurred while performing paged search for DTO {DtoType} of entity {EntityType} with filter {FilterType}.",
                typeof(TDto).Name, typeof(TEntity).Name, typeof(TFilter).Name);
            return Problems.InternalError(ex);
        }
    }
}
