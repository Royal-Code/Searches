// Ignore Spelling: sortings

namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     Extensions methods for <see cref="ICriteriaOptions{TSearch}"/>, <see cref="ISearch{TEntity}"/>
///     and <see cref="ISearch{TEntity, TDto}"/>.
/// </para>
/// </summary>
public static class SearchExtensions
{
    /// <summary>
    /// Applies the <see cref="SearchOptions"/> to the <see cref="ICriteriaOptions{TSearch}"/>.
    /// </summary>
    /// <typeparam name="T">The search object type.</typeparam>
    /// <param name="criteria">The criteria.</param>
    /// <param name="options">The options.</param>
    /// <returns>The search with the options applied.</returns>
    public static ICriteria<T> WithOptions<T>(this ICriteria<T> criteria, SearchOptions options)
        where T : class
    {
        options.AvoidEmpty();

        if (options.ItemsPerPage.HasValue)
            criteria = criteria.UsePages(options.ItemsPerPage.Value);

        if (options.Page.HasValue)
            criteria = criteria.FetchPage(options.Page.Value);

        if (options.LastCount.HasValue)
            criteria = criteria.UseLastCount(options.LastCount.Value);

        if (options.Count.HasValue)
            criteria = criteria.UseCount(options.Count.Value);

        if (options.Skip.HasValue)
            criteria = criteria.Skip(options.Skip.Value);

        if (options.Take.HasValue)
            criteria = criteria.Take(options.Take.Value);

        criteria.OrderBy(options.Sortings);

        return criteria;
    }
}