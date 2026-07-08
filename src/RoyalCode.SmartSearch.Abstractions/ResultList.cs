// Ignore Spelling: Sortings

using System.Text.Json.Serialization;

namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     Default implementation of <see cref="IResultList{TModel}"/>.
/// </para>
/// </summary>
/// <typeparam name="TModel">The result model type.</typeparam>
public class ResultList<TModel> : IResultList<TModel>
{
    /// <inheritdoc />
    public int Page { get; init; }

    /// <inheritdoc />
    public int Count { get; init; }

    /// <inheritdoc />
    public int ItemsPerPage { get; init; }

    /// <inheritdoc />
    public int Pages { get; init; }

    /// <inheritdoc />
    public int Skipped { get; init; }

    /// <inheritdoc />
    public int Taken { get; init; }

    /// <inheritdoc />
    [JsonConverter(typeof(SortingsConverter))]
    public IReadOnlyList<ISorting> Sortings { get; init; } = null!;

    /// <summary>
    /// Reserved for future query-level projections computed during the search.
    /// </summary>
    /// <remarks>
    /// The default search pipeline does not populate this property yet.
    /// </remarks>
    public Dictionary<string, object>? Projections { get; init; } = null!;

    /// <inheritdoc />
    public IReadOnlyList<TModel> Items { get; init; } = null!;
    
    /// <summary>
    /// Reserved for future access to query-level projection values.
    /// </summary>
    /// <remarks>
    /// This implementation is not functional yet.
    /// </remarks>
    /// <typeparam name="T">Projection value type.</typeparam>
    /// <param name="name">Projection name.</param>
    /// <param name="defaultValue">Default value.</param>
    /// <returns>The projection value, or the default value, when projection support is implemented.</returns>
    /// <exception cref="NotImplementedException">
    /// Always thrown until projection support is implemented.
    /// </exception>
    public virtual T GetProjection<T>(string name, T? defaultValue = default)
    {
        throw new NotImplementedException();
    }
}
