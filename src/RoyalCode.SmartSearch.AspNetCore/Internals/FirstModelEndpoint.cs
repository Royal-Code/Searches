﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalCode.SmartProblems;
using RoyalCode.SmartSearch.AspNetCore.HttpResults;
using RoyalCode.SmartSearch.Exceptions;

namespace RoyalCode.SmartSearch.AspNetCore.Internals;

/// <summary>
/// Endpoint to get the first item in the entity that meets the filter and sort criteria.
/// </summary>
/// <typeparam name="TEntity">The type of entity to be queried.</typeparam>
/// <typeparam name="TDto">The type of DTO to be returned.</typeparam>
/// <typeparam name="TFilter">The type of filter applied.</typeparam>
public class FirstModelEndpoint<TEntity, TDto, TFilter>
    where TEntity : class
    where TDto : class
    where TFilter : class
{
    private readonly Action<ICriteria<TEntity>>? searchAction;

    /// <summary>
    /// Initializes a new instance of <see cref="FirstModelEndpoint{TEntity, TDto, TFilter}"/>.
    /// </summary>
    /// <param name="searchAction">Custom action to configure the search.</param>
    public FirstModelEndpoint(Action<ICriteria<TEntity>>? searchAction = null)
    {
        this.searchAction = searchAction;
    }

    /// <summary>
    /// Executes the search for the first item in the entity that meets the filter and sort criteria.
    /// </summary>
    /// <param name="filtro">Filter object with search criteria.</param>
    /// <param name="orderby">Sorting criteria.</param>
    /// <param name="searchable">Search service for the entity.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP 200 result with the item, 204 if not found, or 400 in case of sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public async Task<MatchFirst<TDto>> First(
        [AsParameters] TFilter filtro,
        [FromQuery] Sorting[]? orderby,
        ICriteria<TEntity> searchable,
        CancellationToken ct)
    {
        try
        {
            var search = searchable
                .OrderBy(orderby)
                .FilterBy(filtro);

            if (searchAction is not null)
                searchAction(search);

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
            return Problems.InternalError(ex);
        }
    }
}

/// <summary>
/// Endpoint to get the first entity item with additional identifier.
/// </summary>
/// <typeparam name="TEntity">The type of entity to be queried.</typeparam>
/// <typeparam name="TDto"> The type of DTO to be returned.</typeparam>
/// <typeparam name="TFilter">The type of the applied filter.</typeparam>
/// <typeparam name="TId">The type of the additional identifier.</typeparam>
public class FirstModelEndpoint<TEntity, TDto, TFilter, TId>
    where TEntity : class
    where TDto : class
    where TFilter : class
{
    private readonly Action<TId, ICriteria<TEntity>>? searchAction;

    /// <summary>
    /// Initializes a new instance of <see cref="FirstModelEndpoint{TEntity, TDto, TFilter, TId}"/>.
    /// </summary>
    /// <param name="searchAction">Custom action to configure the search with identifier.</param>
    public FirstModelEndpoint(Action<TId, ICriteria<TEntity>>? searchAction = null)
    {
        this.searchAction = searchAction;
    }

    /// <summary>
    /// Executes the search for the first item in the entity with additional identifier.
    /// </summary>
    /// <param name="id">Additional identifier for the search.</param>
    /// <param name="filtro">Filter object with search criteria.</param>
    /// <param name="orderby">Sorting criteria.</param>
    /// <param name="searchable">Search service for the entity.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP 200 result with the item, 204 if not found, or 400 in case of sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public async Task<MatchFirst<TDto>> First(
        [FromRoute] TId id,
        [AsParameters] TFilter filtro,
        [FromQuery] Sorting[]? orderby,
        ICriteria<TEntity> searchable,
        CancellationToken ct)
    {
        try
        {
            var search = searchable
                .OrderBy(orderby)
                .FilterBy(filtro);

            if (searchAction is not null)
                searchAction(id, search);

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
            return Problems.InternalError(ex);
        }
    }
}

/// <summary>
/// Endpoint to get the first entity item with two additional identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entity to be queried.</typeparam>
/// <typeparam name="TDto">The type of DTO to be returned.</typeparam>
/// <typeparam name="TFilter">The type of the applied filter.</typeparam>
/// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
/// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
public class FirstModelEndpoint<TEntity, TDto, TFilter, TId1, TId2>
    where TEntity : class
    where TDto : class
    where TFilter : class
{
    private readonly Action<TId1, TId2, ICriteria<TEntity>>? searchAction;

    /// <summary>
    /// Initializes a new instance of <see cref="FirstModelEndpoint{TEntity, TDto, TFilter, TId1, TId2}"/>.
    /// </summary>
    /// <param name="searchAction">Custom action to configure the search with identifiers.</param>
    public FirstModelEndpoint(Action<TId1, TId2, ICriteria<TEntity>>? searchAction = null)
    {
        this.searchAction = searchAction;
    }

    /// <summary>
    /// Executes the search for the first item in the entity with two additional identifiers.
    /// </summary>
    /// <param name="id">Additional primary identifier for the search.</param>
    /// <param name="relatedId">Additional identifier, related to the first, for the search.</param>
    /// <param name="filtro">Filter object with search criteria.</param>
    /// <param name="orderby">Sorting criteria.</param>
    /// <param name="searchable">Search service for the entity.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP 200 result with the item, 204 if not found, or 400 in case of sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public async Task<MatchFirst<TDto>> First(
        [FromRoute] TId1 id,
        [FromRoute] TId2 relatedId,
        [AsParameters] TFilter filtro,
        [FromQuery] Sorting[]? orderby,
        ICriteria<TEntity> searchable,
        CancellationToken ct)
    {
        try
        {
            var search = searchable
                .OrderBy(orderby)
                .FilterBy(filtro);

            if (searchAction is not null)
                searchAction(id, relatedId, search);

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
            return Problems.InternalError(ex);
        }
    }
}

/// <summary>
/// Endpoint to get the first entity item with three additional identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entity to be queried.</typeparam>
/// <typeparam name="TDto">The type of DTO to be returned.</typeparam>
/// <typeparam name="TFilter">The type of the applied filter.</typeparam>
/// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
/// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
/// <typeparam name="TId3">The type of the third additional identifier.</typeparam>
public class FirstModelEndpoint<TEntity, TDto, TFilter, TId1, TId2, TId3>
    where TEntity : class
    where TDto : class
    where TFilter : class
{
    private readonly Action<TId1, TId2, TId3, ICriteria<TEntity>>? searchAction;

    /// <summary>
    /// Initializes a new instance of <see cref="FirstModelEndpoint{TEntity, TDto, TFilter, TId1, TId2, TId3}"/>.
    /// </summary>
    /// <param name="searchAction">Custom action to configure the search with identifiers.</param>
    public FirstModelEndpoint(Action<TId1, TId2, TId3, ICriteria<TEntity>>? searchAction = null)
    {
        this.searchAction = searchAction;
    }

    /// <summary>
    /// Executes the search for the first item in the entity with three additional identifiers.
    /// </summary>
    /// <param name="id">Additional primary identifier for the search.</param>
    /// <param name="relatedId">Additional identifier, related to the first, for the search.</param>
    /// <param name="subRelatedId">Third additional identifier for the search.</param>
    /// <param name="filtro">Filter object with search criteria.</param>
    /// <param name="orderby">Sorting criteria.</param>
    /// <param name="searchable">Search service for the entity.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP 200 result with the item, 204 if not found, or 400 in case of sorting error.</returns>
    [ProduceProblems(ProblemCategory.InvalidParameter, ProblemCategory.InternalServerError)]
    public async Task<MatchFirst<TDto>> First(
        [FromRoute] TId1 id,
        [FromRoute] TId2 relatedId,
        [FromRoute] TId3 subRelatedId,
        [AsParameters] TFilter filtro,
        [FromQuery] Sorting[]? orderby,
        ICriteria<TEntity> searchable,
        CancellationToken ct)
    {
        try
        {
            var search = searchable
                .OrderBy(orderby)
                .FilterBy(filtro);

            if (searchAction is not null)
                searchAction(id, relatedId, subRelatedId, search);

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
            return Problems.InternalError(ex);
        }
    }
}
