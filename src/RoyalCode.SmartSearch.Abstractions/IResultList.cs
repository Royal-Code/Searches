using System.Text.Json.Serialization;

namespace RoyalCode.SmartSearch.Abstractions;

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
    /// The sort objects applied to the search.
    /// </summary>
    [JsonConverter(typeof(SortingsConverter))]
    IEnumerable<ISorting> Sortings { get; }

    /// <summary>
    /// Projections carried out during the research.
    /// </summary>
    Dictionary<string, object> Projections { get; }
}

/// <summary>
/// Component interface for listing the result of a search.
/// </summary>
/// <typeparam name="TModel">Type of data listed by the result.</typeparam>
public interface IResultList<out TModel> : IResultList
{
    /// <summary>
    /// Collection of the searched models.
    /// </summary>
    IEnumerable<TModel> Items { get; }

    /// <summary>
    /// Gets a value from the projection if it exists and is of the type entered,
    /// or returns the default value if the value does not exist or the type is different.
    /// </summary>
    /// <typeparam name="T">Projection value type.</typeparam>
    /// <param name="name">Projection name.</param>
    /// <param name="defaultValue">Default value.</param>
    /// <returns>The projection value, or default value.</returns>
    T GetProjection<T>(string name, T? defaultValue = default);
}