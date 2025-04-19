namespace RoyalCode.SmartSearch.Abstractions;

/// <summary>
/// Options that can be applied into searches.
/// </summary>
/// <typeparam name="TSearch">The search component type.</typeparam>
public interface ISearchOptions<out TSearch>
{
    /// <summary>
    /// <para>
    ///     Defines that the query will be paged and determines the number of items per page.
    /// </para>
    /// <para>
    ///     The default value is 10 items per page.
    /// </para>
    /// <para>
    ///     When zero (0) is entered, it will not be paged.
    /// </para>
    /// </summary>
    /// <param name="itemsPerPage">Items per page.</param>
    /// <returns>The same instance of the search for chaining calls.</returns>
    TSearch UsePages(int itemsPerPage = 10);

    /// <summary>
    /// The number of the page to be searched.
    /// </summary>
    /// <returns>The same instance of the search for chaining calls.</returns>
    TSearch FetchPage(int pageNumber);

    /// <summary>
    /// <para>
    ///     Updates the last record count.
    /// </para>
    /// <para>
    ///     Used to not count the records again.
    /// </para>
    /// </summary>
    /// <returns>The same instance of the search for chaining calls.</returns>
    TSearch UseLastCount(int lastCount);

    /// <summary>
    /// Whether to apply record counting.
    /// </summary>
    /// <param name="useCount">Whether to apply record counting.</param>
    /// <returns>The same instance of the search for chaining calls.</returns>
    TSearch UseCount(bool useCount = true);
}
