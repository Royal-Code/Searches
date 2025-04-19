// Ignore Spelling: Sortings

using System.Text.Json.Serialization;

namespace RoyalCode.SmartSearch.Abstractions;

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
    [JsonConverter(typeof(SortingsConverter))]
    public IEnumerable<ISorting> Sortings { get; init; } = null!;

    /// <inheritdoc />
    public Dictionary<string, object> Projections { get; init; } = null!;

    /// <inheritdoc />
    public IEnumerable<TModel> Items { get; init; } = null!;

    /// <inheritdoc />
    public T GetProjection<T>(string name, T? defaultValue = default)
    {
        throw new NotImplementedException();
    }
}
