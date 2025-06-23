using RoyalCode.SmartSearch.Filtering;

namespace RoyalCode.SmartSearch.Defaults;

/// <summary>
/// The criteria options for performing the search.
/// </summary>
public sealed class CriteriaOptions
{
    private List<IFilter>? filters;
    private List<ISorting>? sortings;
    private bool trackingEnabled = true;

    /// <summary>
    /// Get all filters.
    /// </summary>
    public IReadOnlyList<IFilter> Filters => filters ?? [];

    /// <summary>
    /// Get all sortings.
    /// </summary>
    public IReadOnlyList<ISorting> Sortings => sortings ?? [];

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
    public int ItemsPerPage { get; set; } = Defaults.DefaultItemsPerPage;

    /// <summary>
    /// The number of the page to be searched.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of records to be skipped in the search.
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// The number of records to be returned in the search.
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// <para>
    ///     Updates the last record count.
    /// </para>
    /// <para>
    ///     Used to not count the records again.
    /// </para>
    /// </summary>
    public int LastCount { get; set; }

    /// <summary>
    /// Whether to apply record counting.
    /// </summary>
    public bool UseCount { get; set; } = Defaults.DefaultUseCount;

    /// <summary>
    /// Disables entity tracking for the query, which can improve performance for read-only operations.
    /// </summary>
    public void NoTracking()
    {
        trackingEnabled = false;
    }

    /// <summary>
    /// Adds a new filter to specify the search.
    /// </summary>
    /// <param name="filter">The filter instance.</param>
    public void AddFilter<TFilter>(TFilter filter)
        where TFilter : class
    {
        filters ??= [];
        filters.Add(new EntityFilter<TFilter>(filter));
    }

    /// <summary>
    /// Add a sorting definition.
    /// </summary>
    /// <param name="sorting">The sorting definition.</param>
    public void AddSorting(ISorting sorting)
    {
        sortings ??= [];
        sortings.Add(sorting);
    }

    /// <summary>
    /// Adds a sorting definition.
    /// </summary>
    /// <param name="sorting">The sorting definitions.</param>
    public void AddSorting(IEnumerable<ISorting>? sorting)
    {
        if (sorting is null)
            return;
        sortings ??= [];
        sortings.AddRange(sorting);
    }

    /// <summary>
    /// Whether the query should be paginated.
    /// </summary>
    public bool Paginate => Page > 0;

    /// <summary>
    /// The number of the page that should be listed.
    /// </summary>
    /// <returns>The number of the page.</returns>
    public int GetPageNumber() => Page > 0 ? Page : 1;

    /// <summary>
    /// <para>
    ///     The number of records that must be skipped in the query because of pagination.
    /// </para>
    /// <para>
    ///     This calculation is performed using the page number and the quantity of items per page. 
    /// </para>
    /// <para>
    ///     When the query should not be paged, this value will always be zero.
    /// </para>
    /// </summary>
    /// <returns>The number of records that must be skipped.</returns>
    public int GetSkipCount() => Paginate ? ItemsPerPage * (GetPageNumber() - 1) : Skip;

    /// <summary>
    /// Determines the number of items to retrieve in a paginated query.
    /// </summary>
    /// <returns>
    ///     The number of items to retrieve. 
    ///     <br />
    ///     If pagination is enabled, returns the value of <see cref="ItemsPerPage"/>. 
    ///     <br />
    ///     If pagination is disabled and <see cref="Take"/> is greater than 0, returns the value of <see cref="Take"/>. 
    ///     <br />
    ///     Otherwise, returns <c>0</c>.
    /// </returns>
    public int GetTakeCount() => Paginate ? ItemsPerPage : Take > 0 ? Take : 0;

    /// <summary>
    /// Default values for each new <see cref="CriteriaOptions"/> created.
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// The default value of <see cref="ItemsPerPage"/>.
        /// </summary>
        public static int DefaultItemsPerPage { get; set; } = 10;

        /// <summary>
        /// The default value of <see cref="UseCount"/>.
        /// </summary>
        public static bool DefaultUseCount { get; set; } = true;
    }
}