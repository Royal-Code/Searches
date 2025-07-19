using Microsoft.AspNetCore.Http.Metadata;
using System.Net.Mime;

namespace RoyalCode.SmartSearch.AspNetCore.HttpResults;

/// <summary>
/// Metadados de resposta para endpoints HTTP, utilizados para documentação e geração de OpenAPI/Swagger.
/// </summary>
internal sealed class ResponseTypeMetadata : IProducesResponseTypeMetadata
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="ResponseTypeMetadata"/> com tipo de retorno, status e tipos de conteúdo.
    /// </summary>
    /// <param name="type">Tipo do objeto retornado.</param>
    /// <param name="statusCode">Código de status HTTP.</param>
    /// <param name="contentTypes">Tipos de conteúdo suportados (ex: application/json).</param>
    public ResponseTypeMetadata(Type? type, int statusCode, params string[]? contentTypes)
    {
        Type = type;
        StatusCode = statusCode;
        ContentTypes = contentTypes ?? [MediaTypeNames.Application.Json];
    }
    /// <summary>
    /// Inicializa uma nova instância de <see cref="ResponseTypeMetadata"/> apenas com status e tipos de conteúdo.
    /// </summary>
    /// <param name="statusCode">Código de status HTTP.</param>
    /// <param name="contentTypes">Tipos de conteúdo suportados (ex: application/json).</param>
    public ResponseTypeMetadata(int statusCode, params string[]? contentTypes)
    {
        StatusCode = statusCode;
        ContentTypes = contentTypes ?? [MediaTypeNames.Application.Json];
    }
    /// <summary>
    /// Tipo do objeto retornado na resposta HTTP.
    /// </summary>
    public Type? Type { get; }
    /// <summary>
    /// Código de status HTTP da resposta.
    /// </summary>
    public int StatusCode { get; }
    /// <summary>
    /// Tipos de conteúdo suportados pela resposta.
    /// </summary>
    public IEnumerable<string> ContentTypes { get; }
}