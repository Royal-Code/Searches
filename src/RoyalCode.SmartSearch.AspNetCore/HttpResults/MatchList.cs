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
///     Representa um resultado HTTP para uma listagem de entidades ou DTOs em operações de busca.
/// </para>
/// <para>
///     Permite retornar resultados padronizados (200, 204, 400, 500) em Minimal APIs e Controllers,
///     encapsulando listas, erros e exceções de forma consistente.
/// </para>
/// </summary>
/// <typeparam name="TModel">Tipo do modelo dos itens retornados na lista.</typeparam>
public class MatchList<TModel> : IResult, INestedHttpResult, IEndpointMetadataProvider
    where TModel : class
{
    /// <summary>
    /// Conversão implícita de <see cref="NoContent"/> para <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="result">Resultado NoContent.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchList<TModel>(NoContent result) => new(result);

    /// <summary>
    /// Conversão implícita de <see cref="Ok{T} "/> para <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="result">Resultado Ok com lista de modelos.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchList<TModel>(Ok<IReadOnlyList<TModel>> result) => new(result);

    /// <summary>
    /// Conversão implícita de <see cref="Problem"/> para <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="problem">Problema ocorrido na operação.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchList<TModel>(Problem problem) => new(problem);

    /// <summary>
    /// Conversão implícita de <see cref="Exception"/> para <see cref="MatchList{TModel}"/>.
    /// </summary>
    /// <param name="ex">Exceção lançada na operação.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator MatchList<TModel>(Exception ex) => new(ex);

    /// <summary>
    /// Cria um resultado de lista sem conteúdo (204).
    /// </summary>
    public MatchList()
    {
        Result = TypedResults.NoContent();
    }

    /// <summary>
    /// Cria um resultado de lista a partir de uma lista de modelos (200).
    /// </summary>
    /// <param name="result">Lista de modelos retornados.</param>
    public MatchList(IReadOnlyList<TModel> result)
    {
        Result = TypedResults.Ok(result);
    }

    /// <summary>
    /// Cria um resultado de lista a partir de uma exceção (500).
    /// </summary>
    /// <param name="ex">Exceção lançada.</param>
    public MatchList(Exception ex)
    {
        MatchErrorResult match = ex;
        Result = match;
    }

    /// <summary>
    /// Cria um resultado de lista a partir de um problema (400/500).
    /// </summary>
    /// <param name="problem">Problema ocorrido.</param>
    public MatchList(Problem problem)
    {
        MatchErrorResult match = problem;
        Result = match;
    }

    /// <summary>
    /// Cria um resultado de lista a partir de um resultado Ok com lista de modelos (200).
    /// </summary>
    /// <param name="result">Resultado Ok com lista de modelos.</param>
    public MatchList(Ok<IReadOnlyList<TModel>> result)
    {
        Result = result;
    }

    /// <summary>
    /// Cria um resultado de lista a partir de um resultado NoContent (204).
    /// </summary>
    /// <param name="result">Resultado NoContent.</param>
    public MatchList(NoContent result)
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
        builder.Metadata.Add(new ResponseTypeMetadata(typeof(IReadOnlyList<TModel>), StatusCodes.Status200OK, MediaTypeNames.Application.Json));
        builder.Metadata.Add(new ResponseTypeMetadata(StatusCodes.Status204NoContent, MediaTypeNames.Application.Json));
        MatchErrorResult.PopulateMetadata(method, builder);
    }

    /// <summary>
    /// Executa o resultado HTTP no contexto da requisição.
    /// </summary>
    /// <param name="httpContext">Contexto HTTP.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ExecuteAsync(HttpContext httpContext) => Result.ExecuteAsync(httpContext);
}