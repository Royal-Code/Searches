// Ignore Spelling: sortings

namespace RoyalCode.SmartSearch.Abstractions;

/// <summary>
/// <para>
///     Extensions methods for <see cref="ISearchOptions{TSearch}"/>, <see cref="ISearch{TEntity}"/>
///     and <see cref="ISearch{TEntity, TDto}"/>.
/// </para>
/// </summary>
public static class SearchExtensions
{
    /// <summary>
    /// Applies the <see cref="SearchOptions"/> to the <see cref="ISearchOptions{TSearch}"/>.
    /// </summary>
    /// <typeparam name="T">The search object type.</typeparam>
    /// <param name="search">The search.</param>
    /// <param name="options">The options.</param>
    /// <returns>The search with the options applied.</returns>
    public static ISearch<T> WithOptions<T>(this ISearch<T> search, SearchOptions options)
        where T : class
    {
        if (options.ItemsPerPage.HasValue)
            search = search.UsePages(options.ItemsPerPage.Value);

        if (options.Page.HasValue)
            search = search.FetchPage(options.Page.Value);

        if (options.LastCount.HasValue)
            search = search.UseLastCount(options.LastCount.Value);

        if (options.Count.HasValue)
            search = search.UseCount(options.Count.Value);

        OrderBy(search, options.Sortings);

        return search;
    }

    /// <summary>
    /// Applies a set of sorting to the search.
    /// </summary>
    /// <typeparam name="T">The search object type.</typeparam>
    /// <param name="search">The search.</param>
    /// <param name="sortings">The sortings.</param>
    /// <returns>The search with the sortings applied.</returns>
    public static ISearch<T> OrderBy<T>(this ISearch<T> search, ISorting[]? sortings)
        where T : class
    {
        if (sortings is not null)
            foreach (var sorting in sortings)
                search.OrderBy(sorting);

        return search;
    }
}