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
///     Represents an HTTP result for retrieving the first entity or DTO in search operations.
/// </para>
/// <para>
///     Enables standardized results (200, 204, 400, 500) in Minimal APIs and Controllers,
///     encapsulating single items, errors, and exceptions consistently.
/// </para>
/// </summary>
/// <typeparam name="TModel">Type of the returned model.</typeparam>
public class MatchFirst<TModel> : IResult, INestedHttpResult, IEndpointMetadataProvider
    where TModel : class
{
    /// <summary>
    /// Implicit conversion from <see cref="NoContent"/> to <see cref="MatchFirst{TModel}"/>.
    /// </summary>
    /// <param name="result">NoContent result.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchFirst<TModel>(NoContent result) => new(result);

    /// <summary>
    /// Implicit conversion from <see cref="Ok{T}"/> to <see cref="MatchFirst{TModel}"/>.
    /// </summary>
    /// <param name="result">Ok result with a model.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchFirst<TModel>(Ok<TModel> result) => new(result);

    /// <summary>
    /// Implicit conversion from <typeparamref name="TModel"/> to <see cref="MatchFirst{TModel}"/>.
    /// </summary>
    /// <param name="result">Returned model.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchFirst<TModel>(TModel result) => new(result);

    /// <summary>
    /// Implicit conversion from <see cref="Problem"/> to <see cref="MatchFirst{TModel}"/>.
    /// </summary>
    /// <param name="problem">Problem occurred during the operation.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchFirst<TModel>(Problem problem) => new(problem);

    /// <summary>
    /// Implicit conversion from <see cref="Exception"/> to <see cref="MatchFirst{TModel}"/>.
    /// </summary>
    /// <param name="ex">Exception thrown during the operation.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchFirst<TModel>(Exception ex) => new(ex);

    /// <summary>
    /// Creates a result with no content (204).
    /// </summary>
    public MatchFirst()
    {
        Result = TypedResults.NoContent();
    }

    /// <summary>
    /// Creates a result with a returned model (200).
    /// </summary>
    /// <param name="model">Returned model.</param>
    public MatchFirst(TModel model)
    {
        Result = TypedResults.Ok(model);
    }

    /// <summary>
    /// Creates a result from an exception (500).
    /// </summary>
    /// <param name="ex">Exception thrown.</param>
    public MatchFirst(Exception ex)
    {
        MatchErrorResult match = ex;
        Result = match;
    }

    /// <summary>
    /// Creates a result from a problem (400/500).
    /// </summary>
    /// <param name="problem">Problem occurred.</param>
    public MatchFirst(Problem problem)
    {
        MatchErrorResult match = problem;
        Result = match;
    }

    /// <summary>
    /// Creates a result from an Ok result with a model (200).
    /// </summary>
    /// <param name="result">Ok result with a model.</param>
    public MatchFirst(Ok<TModel> result)
    {
        Result = result;
    }

    /// <summary>
    /// Creates a result from a NoContent result (204).
    /// </summary>
    /// <param name="result">NoContent result.</param>
    public MatchFirst(NoContent result)
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
        OkMatch<TModel>.PopulateMetadata(method, builder);
        builder.Metadata.Add(new ResponseTypeMetadata(StatusCodes.Status204NoContent, MediaTypeNames.Application.Json));
    }

    /// <summary>
    /// Executes the HTTP result in the request context.
    /// </summary>
    /// <param name="httpContext">HTTP context.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ExecuteAsync(HttpContext httpContext) => Result.ExecuteAsync(httpContext);
}
