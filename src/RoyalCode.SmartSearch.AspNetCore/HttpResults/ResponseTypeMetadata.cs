using Microsoft.AspNetCore.Http.Metadata;
using System.Net.Mime;

namespace RoyalCode.SmartSearch.AspNetCore.HttpResults;

/// <summary>
/// Response metadata for HTTP endpoints, used for documentation and OpenAPI/Swagger generation.
/// </summary>
internal sealed class ResponseTypeMetadata : IProducesResponseTypeMetadata
{
    /// <summary>
    /// Initializes a new instance of <see cref="ResponseTypeMetadata"/> with return type, status code, and content types.
    /// </summary>
    /// <param name="type">Type of the returned object.</param>
    /// <param name="statusCode">HTTP status code.</param>
    /// <param name="contentTypes">Supported content types (e.g., application/json).</param>
    public ResponseTypeMetadata(Type? type, int statusCode, params string[]? contentTypes)
    {
        Type = type;
        StatusCode = statusCode;
        ContentTypes = contentTypes ?? [MediaTypeNames.Application.Json];
    }
    /// <summary>
    /// Initializes a new instance of <see cref="ResponseTypeMetadata"/> with status code and content types only.
    /// </summary>
    /// <param name="statusCode">HTTP status code.</param>
    /// <param name="contentTypes">Supported content types (e.g., application/json).</param>
    public ResponseTypeMetadata(int statusCode, params string[]? contentTypes)
    {
        StatusCode = statusCode;
        ContentTypes = contentTypes ?? [MediaTypeNames.Application.Json];
    }
    /// <summary>
    /// Type of the object returned in the HTTP response.
    /// </summary>
    public Type? Type { get; }
    /// <summary>
    /// HTTP status code of the response.
    /// </summary>
    public int StatusCode { get; }
    /// <summary>
    /// Supported content types for the response.
    /// </summary>
    public IEnumerable<string> ContentTypes { get; }
}