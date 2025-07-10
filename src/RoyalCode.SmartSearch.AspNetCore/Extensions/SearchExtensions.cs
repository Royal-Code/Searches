using Microsoft.AspNetCore.Builder;
using RoyalCode.SmartSearch;
using RoyalCode.SmartSearch.AspNetCore.Internals;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Routing;

public static class SearchExtensions
{
    #region MapSearch

    /// <summary>
    /// Maps a GET endpoint to paginated, sorted and filtered search, returning entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to be consulted.</typeparam>
    /// <typeparam name="TFilter">The type of filter applied.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapSearch<TEntity, TFilter>(this IEndpointRouteBuilder builder, [StringSyntax("Route")] string pattern)
        where TEntity : class
        where TFilter : class
    {
        var search = new SearchEntityEndpoint<TEntity, TFilter>();
        return builder.MapGet(pattern, search.Search);
    }

    /// <summary>
    /// Maps a GET endpoint to paginated, sorted and filtered search, returning entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Custom action to configure the search.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapSearch<TEntity, TFilter>(
        this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var search = new SearchEntityEndpoint<TEntity, TFilter>(searchAction);
        return builder.MapGet(pattern, search.Search);
    }

    /// <summary>
    /// Maps a GET endpoint to paginated, sorted and filtered search, returning entities with an additional identifier.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId">The type of the additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Custom action to configure the search.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapSearch<TEntity, TFilter, TId>(
        this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var search = new SearchEntityEndpoint<TEntity, TFilter, TId>(searchAction);
        return builder.MapGet(pattern, search.Search);
    }

    /// <summary>
    /// Maps a GET endpoint to paginated, sorted and filtered search, returning entities with two additional identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
    /// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Custom action to configure the search.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapSearch<TEntity, TFilter, TId1, TId2>(
        this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId1, TId2, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var search = new SearchEntityEndpoint<TEntity, TFilter, TId1, TId2>(searchAction);
        return builder.MapGet(pattern, search.Search);
    }

    /// <summary>
    /// Maps a GET endpoint to paginated, sorted and filtered search, returning DTOs.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TDto">The type of the DTO to be returned.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapSearch<TEntity, TDto, TFilter>(this IEndpointRouteBuilder builder, [StringSyntax("Route")] string pattern)
        where TEntity : class
        where TDto : class
        where TFilter : class
    {
        var search = new SearchModelEndpoint<TEntity, TDto, TFilter>();
        return builder.MapGet(pattern, search.Search);
    }

    /// <summary>
    /// Maps a GET endpoint to paginated, sorted and filtered search, returning DTOs.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TDto">The type of the DTO to be returned.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapSearch<TEntity, TDto, TFilter>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TDto : class
        where TFilter : class
    {
        var search = new SearchModelEndpoint<TEntity, TDto, TFilter>(searchAction);
        return builder.MapGet(pattern, search.Search);
    }

    /// <summary>
    /// Maps a GET endpoint to paginated, sorted and filtered search, returning DTOs with an additional identifier.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TDto">The type of the DTO to be returned.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId">The type of the additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapSearch<TEntity, TDto, TFilter, TId>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TDto : class
        where TFilter : class
    {
        var search = new SearchModelEndpoint<TEntity, TDto, TFilter, TId>(searchAction);
        return builder.MapGet(pattern, search.Search);
    }

    /// <summary>
    /// Maps a GET endpoint to paginated, sorted and filtered search, returning DTOs with two additional identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TDto">The type of the DTO to be returned.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
    /// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapSearch<TEntity, TDto, TFilter, TId1, TId2>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId1, TId2, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TDto : class
        where TFilter : class
    {
        var search = new SearchModelEndpoint<TEntity, TDto, TFilter, TId1, TId2>(searchAction);
        return builder.MapGet(pattern, search.Search);
    }

    #endregion

    #region MapList

    /// <summary>
    /// Maps a GET endpoint to a filtered and sorted list of entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapList<TEntity, TFilter>(this IEndpointRouteBuilder builder, 
        [StringSyntax("Route")] string pattern)
        where TEntity : class
        where TFilter : class
    {
        var list = new ListEntityEndpoint<TEntity, TFilter>();
        return builder.MapGet(pattern, list.List);
    }

    /// <summary>
    /// Maps a GET endpoint to a filtered and sorted list of entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapList<TEntity, TFilter>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var list = new ListEntityEndpoint<TEntity, TFilter>(searchAction);
        return builder.MapGet(pattern, list.List);
    }

    /// <summary>
    /// Maps a GET endpoint to a filtered and sorted list of entities with an additional identifier.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId">The type of the additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapList<TEntity, TFilter, TId>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var list = new ListEntityEndpoint<TEntity, TFilter, TId>(searchAction);
        return builder.MapGet(pattern, list.List);
    }

    /// <summary>
    /// Maps a GET endpoint to a filtered and sorted list of entities with two additional identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
    /// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapList<TEntity, TFilter, TId1, TId2>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId1, TId2, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var list = new ListEntityEndpoint<TEntity, TFilter, TId1, TId2>(searchAction);
        return builder.MapGet(pattern, list.List);
    }

    /// <summary>
    /// Maps a GET endpoint to a filtered and sorted list of DTOs.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TDto">The type of the DTO to be returned.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapList<TEntity, TDto, TFilter>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern)
        where TEntity : class
        where TDto : class
        where TFilter : class
    {
        var list = new ListModelEndpoint<TEntity, TDto, TFilter>();
        return builder.MapGet(pattern, list.List);
    }

    /// <summary>
    /// Maps a GET endpoint to a filtered and sorted list of DTOs.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TDto">The type of the DTO to be returned.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapList<TEntity, TDto, TFilter>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TDto : class
        where TFilter : class
    {
        var list = new ListModelEndpoint<TEntity, TDto, TFilter>(searchAction);
        return builder.MapGet(pattern, list.List);
    }

    /// <summary>
    /// Maps a GET endpoint to a filtered and sorted list of DTOs with an additional identifier.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TDto">The type of the DTO to be returned.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId">The type of the additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapList<TEntity, TDto, TFilter, TId>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TDto : class
        where TFilter : class
    {
        var list = new ListModelEndpoint<TEntity, TDto, TFilter, TId>(searchAction);
        return builder.MapGet(pattern, list.List);
    }

    /// <summary>
    /// Maps a GET endpoint to a filtered and sorted list of DTOs with two additional identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TDto">The type of the DTO to be returned.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
    /// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapList<TEntity, TDto, TFilter, TId1, TId2>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId1, TId2, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TDto : class
        where TFilter : class
    {
        var list = new ListModelEndpoint<TEntity, TDto, TFilter, TId1, TId2>(searchAction);
        return builder.MapGet(pattern, list.List);
    }

    #endregion

    #region MapFirst

    /// <summary>
    /// Maps a GET endpoint to a first entity based on the applied filter.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapFirst<TEntity, TFilter>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern)
        where TEntity : class
        where TFilter : class
    {
        var first = new FirstEntityEndpoint<TEntity, TFilter>();
        return builder.MapGet(pattern, first.First);
    }

    /// <summary>
    /// Maps a GET endpoint to a first entity based on the applied filter.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapFirst<TEntity, TFilter>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var first = new FirstEntityEndpoint<TEntity, TFilter>(searchAction);
        return builder.MapGet(pattern, first.First);
    }

    /// <summary>
    /// Maps a GET endpoint to a first entity based on the applied filter with an additional identifier.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId">The type of the additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapFirst<TEntity, TFilter, TId>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var first = new FirstEntityEndpoint<TEntity, TFilter, TId>(searchAction);
        return builder.MapGet(pattern, first.First);
    }

    /// <summary>
    /// Maps a GET endpoint to a first entity based on the applied filter with two additional identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
    /// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapFirst<TEntity, TFilter, TId1, TId2>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId1, TId2, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var first = new FirstEntityEndpoint<TEntity, TFilter, TId1, TId2>(searchAction);
        return builder.MapGet(pattern, first.First);
    }

    /// <summary>
    /// Maps a GET endpoint to a first entity based on the applied filter with three additional identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
    /// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
    /// <typeparam name="TId3">The type of the third additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapFirst<TEntity, TFilter, TId1, TId2, TId3>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId1, TId2, TId3, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var first = new FirstEntityEndpoint<TEntity, TFilter, TId1, TId2, TId3>(searchAction);
        return builder.MapGet(pattern, first.First);
    }

    /// <summary>
    /// Maps a GET endpoint to a first DTO based on the applied filter.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapFirstDto<TEntity, TFilter>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern)
        where TEntity : class
        where TFilter : class
    {
        var first = new FirstEntityEndpoint<TEntity, TFilter>();
        return builder.MapGet(pattern, first.First);
    }

    /// <summary>
    /// Maps a GET endpoint to a first DTO based on the applied filter.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapFirstDto<TEntity, TFilter>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var first = new FirstEntityEndpoint<TEntity, TFilter>(searchAction);
        return builder.MapGet(pattern, first.First);
    }

    /// <summary>
    /// Maps a GET endpoint to a first DTO based on the applied filter with an additional identifier.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId">The type of the first additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapFirstDto<TEntity, TFilter, TId>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var first = new FirstEntityEndpoint<TEntity, TFilter, TId>(searchAction);
        return builder.MapGet(pattern, first.First);
    }

    /// <summary>
    /// Maps a GET endpoint to a first DTO based on the applied filter with two additional identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
    /// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapFirstDto<TEntity, TFilter, TId1, TId2>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId1, TId2, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var first = new FirstEntityEndpoint<TEntity, TFilter, TId1, TId2>(searchAction);
        return builder.MapGet(pattern, first.First);
    }

    /// <summary>
    /// Maps a GET endpoint to a first DTO based on the applied filter with three additional identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be queried.</typeparam>
    /// <typeparam name="TFilter">The type of the applied filter.</typeparam>
    /// <typeparam name="TId1">The type of the first additional identifier.</typeparam>
    /// <typeparam name="TId2">The type of the second additional identifier.</typeparam>
    /// <typeparam name="TId3">The type of the third additional identifier.</typeparam>
    /// <param name="builder">The route builder.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="searchAction">Action to apply additional logic to the search criteria.</param>
    /// <returns>A builder for additional endpoint configuration.</returns>
    public static RouteHandlerBuilder MapFirstDto<TEntity, TFilter, TId1, TId2, TId3>(this IEndpointRouteBuilder builder,
        [StringSyntax("Route")] string pattern,
        Action<TId1, TId2, TId3, ICriteria<TEntity>> searchAction)
        where TEntity : class
        where TFilter : class
    {
        var first = new FirstEntityEndpoint<TEntity, TFilter, TId1, TId2, TId3>(searchAction);
        return builder.MapGet(pattern, first.First);
    }

    #endregion
}
