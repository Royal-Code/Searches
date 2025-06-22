namespace RoyalCode.SmartSearch.Abstractions;

/// <summary>
/// Options that can be applied into criteria or search components.
/// </summary>
/// <typeparam name="TSearch">The search component type.</typeparam>
public interface ICriteriaOptions<out TSearch>
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
    ///     Defines the number of records to be skipped in the search.
    /// </para>
    /// <para>
    ///     When zero (0) is entered, it will not skip any records.
    /// </para>
    /// <para>
    ///     When <see cref="UsePages(int)"/> is informed, this property is ignored.
    /// </para>
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <returns>The same instance of the search for chaining calls.</returns>
    TSearch Skip(int skip);

    /// <summary>
    /// <para>
    ///     Limits the number of results returned by the search query.
    /// </para>
    /// <para>
    ///     When <see cref="UsePages(int)"/> is informed, this property is ignored.
    /// </para>
    /// </summary>
    /// <param name="take">The maximum number of results to return. Must be a positive integer.</param>
    /// <returns>The updated search query with the specified limit applied.</returns>
    TSearch Take(int take);

    /// <summary>
    /// <para>
    ///     Defines the number of records to be skipped and the number of records to be returned in the search.
    /// </para>
    /// <para>
    ///     When <see cref="UsePages(int)"/> is informed, this property is ignored.
    /// </para>
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The maximum number of results to return. Must be a positive integer.</param>
    /// <returns>The same instance of the search for chaining calls.</returns>
    TSearch SkipTake(int skip, int take);

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
