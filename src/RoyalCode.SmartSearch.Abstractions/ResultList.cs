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
    public int Skipped { get; }

    /// <inheritdoc />
    public int Taken { get; }

    /// <inheritdoc />
    [JsonConverter(typeof(SortingsConverter))]
    public IReadOnlyList<ISorting> Sortings { get; init; } = null!;

    /// <inheritdoc />
    public Dictionary<string, object> Projections { get; init; } = null!;

    /// <inheritdoc />
    public IReadOnlyList<TModel> Items { get; init; } = null!;
    
    /// <inheritdoc />
    public virtual T GetProjection<T>(string name, T? defaultValue = default)
    {
        throw new NotImplementedException();
    }
}
