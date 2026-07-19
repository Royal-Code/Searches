using System.Text.Json.Serialization;

namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     Component interface for listing the result of a search.
/// </para>
/// <para>
///     This interface is an abstraction for the component that contains the returned search items:
///     <see cref="IResultList{TModel}"/>.
/// </para>
/// </summary>
public interface IResultList
{
    /// <summary>
    /// Number of the page displayed.
    /// </summary>
    int Page { get; }

    /// <summary>
    /// Total number of records.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Number of items displayed per page.
    /// </summary>
    int ItemsPerPage { get; }

    /// <summary>
    /// Number of pages.
    /// </summary>
    int Pages { get; }

    /// <summary>
    /// Number of items skipped in the result set.
    /// </summary>
    int Skipped { get; }

    /// <summary>
    /// Number of items taken from the result set.
    /// </summary>
    int Taken { get; }

    /// <summary>
    /// The sort objects applied to the search.
    /// </summary>
    [JsonConverter(typeof(SortingsConverter))]
    IReadOnlyList<ISorting> Sortings { get; }

    /// <summary>
    /// Reserved for future query-level projections computed during the search.
    /// </summary>
    /// <remarks>
    /// Current built-in searches do not populate this value. It is intended for future extra values,
    /// such as aggregates over the filtered query before paging is applied.
    /// </remarks>
    Dictionary<string, object>? Projections { get; }
}

/// <summary>
/// Component interface for listing the result of a search.
/// </summary>
/// <typeparam name="TModel">Type of data listed by the result.</typeparam>
public interface IResultList<out TModel> : IResultList
{
    /// <summary>
    /// List of the searched models.
    /// </summary>
    IReadOnlyList<TModel> Items { get; }

    /// <summary>
    /// Reserved for future access to query-level projection values.
    /// </summary>
    /// <remarks>
    /// Current built-in result lists do not provide functional projection lookup. Do not rely on this
    /// method until projection support is implemented.
    /// </remarks>
    /// <typeparam name="T">Projection value type.</typeparam>
    /// <param name="name">Projection name.</param>
    /// <param name="defaultValue">Default value.</param>
    /// <returns>The projection value, or the default value, when projection support is implemented.</returns>
    T GetProjection<T>(string name, T? defaultValue = default);
}
