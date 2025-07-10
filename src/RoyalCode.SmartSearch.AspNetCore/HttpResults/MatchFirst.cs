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

public class MatchFirst<TModel> : IResult, INestedHttpResult, IEndpointMetadataProvider
    where TModel : class
{
    /// <summary>
    /// Conversão implícita de <see cref="NoContent"/> para <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="result">Resultado NoContent.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchFirst<TModel>(NoContent result) => new(result);

    /// <summary>
    /// Conversão implícita de <see cref="Ok{T} "/> para <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="result">Resultado Ok com lista de modelos.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchFirst<TModel>(Ok<TModel> result) => new(result);

    /// <summary>
    /// Conversão implícita de <see cref="Ok{T} "/> para <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="result">Resultado Ok com lista de modelos.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchFirst<TModel>(TModel result) => new(result);

    /// <summary>
    /// Conversão implícita de <see cref="Problem"/> para <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="problem">Problema ocorrido na operação.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchFirst<TModel>(Problem problem) => new(problem);

    /// <summary>
    /// Conversão implícita de <see cref="Exception"/> para <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="ex">Exceção lançada na operação.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchFirst<TModel>(Exception ex) => new(ex);

    /// <summary>
    /// Cria um resultado de lista sem conteúdo (204).
    /// </summary>
    public MatchFirst()
    {
        Result = TypedResults.NoContent();
    }

    /// <summary>
    /// Cria um resultado de lista a partir de uma lista de modelos (200).
    /// </summary>
    /// <param name="model">Modelo retornado.</param>
    public MatchFirst(TModel model)
    {
        Result = TypedResults.Ok(model);
    }

    /// <summary>
    /// Cria um resultado de lista a partir de uma exceção (500).
    /// </summary>
    /// <param name="ex">Exceção lançada.</param>
    public MatchFirst(Exception ex)
    {
        MatchErrorResult match = ex;
        Result = match;
    }

    /// <summary>
    /// Cria um resultado de lista a partir de um problema (400/500).
    /// </summary>
    /// <param name="problem">Problema ocorrido.</param>
    public MatchFirst(Problem problem)
    {
        MatchErrorResult match = problem;
        Result = match;
    }

    /// <summary>
    /// Cria um resultado de lista a partir de um resultado Ok com lista de modelos (200).
    /// </summary>
    /// <param name="result">Resultado Ok com lista de modelos.</param>
    public MatchFirst(Ok<TModel> result)
    {
        Result = result;
    }

    /// <summary>
    /// Cria um resultado de lista a partir de um resultado NoContent (204).
    /// </summary>
    /// <param name="result">Resultado NoContent.</param>
    public MatchFirst(NoContent result)
    {
        Result = result;
    }

    /// <summary>
    /// Resultado HTTP encapsulado (Ok, NoContent, Problem, etc).
    /// </summary>
    public IResult Result { get; }

    /// <summary>
    /// Adiciona metadados de resposta para documentação e OpenAPI.
    /// </summary>
    /// <param name="method">Método associado ao endpoint.</param>
    /// <param name="builder">Construtor do endpoint.</param>
    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        OkMatch<TModel>.PopulateMetadata(method, builder);
        builder.Metadata.Add(new ResponseTypeMetadata(StatusCodes.Status204NoContent, MediaTypeNames.Application.Json));
    }

    /// <summary>
    /// Executa o resultado HTTP no contexto da requisição.
    /// </summary>
    /// <param name="httpContext">Contexto HTTP.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ExecuteAsync(HttpContext httpContext) => Result.ExecuteAsync(httpContext);
}
