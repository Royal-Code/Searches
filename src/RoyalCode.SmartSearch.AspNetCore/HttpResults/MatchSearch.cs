using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using RoyalCode.SmartProblems;
using RoyalCode.SmartProblems.HttpResults;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RoyalCode.SmartSearch.AspNetCore.HttpResults;

/// <summary>
/// <para>
///     Represents an HTTP result for a paginated search of entities or DTOs, 
///     including pagination and sorting information.
/// </para>
/// <para>
///     Enables standardized results (200, 204, 400, 500) in Minimal APIs and Controllers,
///     encapsulating paginated lists, errors, and exceptions consistently.
/// </para>
/// </summary>
/// <typeparam name="TModel">Type of the returned items in the search.</typeparam>
public class MatchSearch<TModel> : IResult, INestedHttpResult, IEndpointMetadataProvider
    where TModel : class
{
    /// <summary>
    /// Implicit conversion from <see cref="NoContent"/> to <see cref="MatchSearch{TModel}"/>.
    /// </summary>
    /// <param name="result">NoContent result.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchSearch<TModel>(NoContent result) => new(result);

    /// <summary>
    /// Implicit conversion from <see cref="Ok{T}"/> to <see cref="MatchSearch{TModel}"/>.
    /// </summary>
    /// <param name="result">Ok result with a paginated list of models.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchSearch<TModel>(Ok<IResultList<TModel>> result) => new(result);

    /// <summary>
    /// Implicit conversion from <see cref="Problem"/> to <see cref="MatchSearch{TModel}"/>.
    /// </summary>
    /// <param name="problem">Problem occurred during the operation.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchSearch<TModel>(Problem problem) => new(problem);

    /// <summary>
    /// Implicit conversion from <see cref="Exception"/> to <see cref="MatchSearch{TModel}"/>.
    /// </summary>
    /// <param name="ex">Exception thrown during the operation.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchSearch<TModel>(Exception ex) => new(ex);

    /// <summary>
    /// Creates a search result with no content (204).
    /// </summary>
    public MatchSearch()
    {
        Result = TypedResults.NoContent();
    }

    /// <summary>
    /// Creates a search result from a paginated list of models (200).
    /// </summary>
    /// <param name="result">Paginated list of returned models.</param>
    public MatchSearch(IResultList<TModel> result)
    {
        Result = TypedResults.Ok(result);
    }

    /// <summary>
    /// Creates a search result from an exception (500).
    /// </summary>
    /// <param name="ex">Exception thrown.</param>
    public MatchSearch(Exception ex)
    {
        MatchErrorResult match = ex;
        Result = match;
    }

    /// <summary>
    /// Creates a search result from a problem (400/500).
    /// </summary>
    /// <param name="problem">Problem occurred.</param>
    public MatchSearch(Problem problem)
    {
        MatchErrorResult match = problem;
        Result = match;
    }

    /// <summary>
    /// Creates a search result from an Ok result with a paginated list of models (200).
    /// </summary>
    /// <param name="result">Ok result with a paginated list of models.</param>
    public MatchSearch(Ok<IResultList<TModel>> result)
    {
        Result = result;
    }

    /// <summary>
    /// Creates a search result from a NoContent result (204).
    /// </summary>
    /// <param name="result">NoContent result.</param>
    public MatchSearch(NoContent result)
    {
        Result = result;
    }

    /// <summary>
    /// Encapsulated HTTP result (Ok, NoContent, Problem, etc).
    /// </summary>
    public IResult Result { get; }

    /// <summary>
    /// Adds response metadata for documentation and OpenAPI.
    /// </summary>
    /// <param name="method">Endpoint method.</param>
    /// <param name="builder">Endpoint builder.</param>
    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        builder.Metadata.Add(new ResponseTypeMetadata(typeof(IResultList<TModel>), StatusCodes.Status200OK, MediaTypeNames.Application.Json));
        builder.Metadata.Add(new ResponseTypeMetadata(StatusCodes.Status204NoContent, MediaTypeNames.Application.Json));
        MatchErrorResult.PopulateMetadata(method, builder);
    }

    /// <summary>
    /// Executes the HTTP result in the request context.
    /// </summary>
    /// <param name="httpContext">HTTP context.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ExecuteAsync(HttpContext httpContext) => Result.ExecuteAsync(httpContext);
}
