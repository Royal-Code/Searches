using System.Text.Json.Serialization;

namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     Default implementation of <see cref="IAsyncResultList{TModel}"/>.
/// </para>
/// </summary>
/// <typeparam name="TModel">The result model type.</typeparam>
public sealed class AsyncResultList<TModel> : IAsyncResultList<TModel>
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

    /// <inheritdoc />
    public Dictionary<string, object> Projections { get; init; } = null!;

    /// <inheritdoc />
    public IAsyncEnumerable<TModel> Items { get; init; } = null!;
}