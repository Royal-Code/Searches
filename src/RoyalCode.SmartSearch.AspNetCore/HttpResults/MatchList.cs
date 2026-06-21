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
///     Represents an HTTP result for listing entities or DTOs in search operations.
/// </para>
/// <para>
///     Enables standardized results (200, 204, 400, 500) in Minimal APIs and Controllers,
///     encapsulating lists, errors, and exceptions consistently.
/// </para>
/// </summary>
/// <typeparam name="TModel">Type of the returned items in the list.</typeparam>
public class MatchList<TModel> : IResult, INestedHttpResult, IEndpointMetadataProvider
    where TModel : class
{
    /// <summary>
    /// Implicit conversion from <see cref="NoContent"/> to <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="result">NoContent result.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchList<TModel>(NoContent result) => new(result);

    /// <summary>
    /// Implicit conversion from <see cref="Ok{T}"/> to <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="result">Ok result with a list of models.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchList<TModel>(Ok<IReadOnlyList<TModel>> result) => new(result);

    /// <summary>
    /// Implicit conversion from <see cref="Problem"/> to <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="problem">Problem occurred during the operation.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchList<TModel>(Problem problem) => new(problem);

    /// <summary>
    /// Implicit conversion from <see cref="Exception"/> to <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="ex">Exception thrown during the operation.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchList<TModel>(Exception ex) => new(ex);

    /// <summary>
    /// Creates a result with no content (204).
    /// </summary>
    public MatchList()
    {
        Result = TypedResults.NoContent();
    }

    /// <summary>
    /// Creates a result from a list of models (200).
    /// </summary>
    /// <param name="result">Returned list of models.</param>
    public MatchList(IReadOnlyList<TModel> result)
    {
        Result = TypedResults.Ok(result);
    }

    /// <summary>
    /// Creates a result from an exception (500).
    /// </summary>
    /// <param name="ex">Exception thrown.</param>
    public MatchList(Exception ex)
    {
        MatchErrorResult match = ex;
        Result = match;
    }

    /// <summary>
    /// Creates a result from a problem (400/500).
    /// </summary>
    /// <param name="problem">Problem occurred.</param>
    public MatchList(Problem problem)
    {
        MatchErrorResult match = problem;
        Result = match;
    }

    /// <summary>
    /// Creates a result from an Ok result with a list of models (200).
    /// </summary>
    /// <param name="result">Ok result with a list of models.</param>
    public MatchList(Ok<IReadOnlyList<TModel>> result)
    {
        Result = result;
    }

    /// <summary>
    /// Creates a result from a NoContent result (204).
    /// </summary>
    /// <param name="result">NoContent result.</param>
    public MatchList(NoContent result)
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
        builder.Metadata.Add(new ResponseTypeMetadata(typeof(IReadOnlyList<TModel>), StatusCodes.Status200OK, MediaTypeNames.Application.Json));
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