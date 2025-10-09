// Ignore Spelling: sortings

namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     A class that contains the options for a criteria and search.
///     It is used to define the search parameters.
/// </para>
/// <para>
///     It is designed to retrieve the options from a query string and apply them to the search.
/// </para>
/// </summary>
public sealed class SearchOptions
{
    private List<Sorting>? sortings;

    /// <summary>
    /// The number of the page to be searched.
    /// </summary>
    public int? Page { get; set; }

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
    public int? ItemsPerPage { get; set; }

    /// <summary>
    /// <para>
    ///     Defines the number of records to be skipped in the search.
    ///     When zero (0) is entered, it will not skip any records.
    /// </para>
    /// <para>
    ///     When <see cref="Page"/> is informed, this property is ignored.
    /// </para>
    /// </summary>
    public int? Skip { get; set; }

    /// <summary>
    /// <para>
    ///     Defines the number of records to be returned in the search.
    /// </para>
    /// <para>
    ///     This property must be use with <see cref="Skip"/> to define the number of records to be returned
    ///     after skipping the defined number of records.
    /// </para>
    /// <para>
    ///     When <see cref="Page"/> is informed, this property is ignored.
    /// </para>
    /// </summary>
    public int? Take { get; set; }

    /// <summary>
    /// <para>
    ///     Updates the last record count.
    /// </para>
    /// <para>
    ///     Used to not count the records again.
    /// </para>
    /// </summary>
    public int? LastCount { get; set; }

    /// <summary>
    /// Whether to apply record counting.
    /// </summary>
    public bool? Count { get; set; }

    /// <summary>
    /// The order by instructions for the search.
    /// </summary>
    internal Sorting[]? Sortings
    {
        get => sortings?.ToArray() ?? null;
        set => sortings = value?.ToList();
    }

    /// <summary>
    /// Adds a new order by instruction.
    /// </summary>
    /// <param name="orderBy">The property name to be ordered.</param>
    /// <returns>The same instance for chaining calls.</returns>
    public SearchOptions OrderBy(string orderBy)
    {
        sortings ??= [];
        sortings.Add(new Sorting
        {
            OrderBy = orderBy,
            Direction = System.ComponentModel.ListSortDirection.Ascending
        });
        return this;
    }

    /// <summary>
    /// Adds a new order by instruction.
    /// </summary>
    /// <param name="orderBy">The property name to be ordered.</param>
    /// <returns>The same instance for chaining calls.</returns>
    public SearchOptions OrderByDesc(string orderBy)
    {
        sortings ??= [];
        sortings.Add(new Sorting
        {
            OrderBy = orderBy,
            Direction = System.ComponentModel.ListSortDirection.Descending
        });
        return this;
    }

    /// <summary>
    /// Set properties to return all items.
    /// </summary>
    /// <returns>The same instance for chaining calls.</returns>
    public SearchOptions AllItens()
    {
        Count = false;
        ItemsPerPage = 0;
        Page = 0;
        LastCount = 0;
        return this;
    }

    /// <summary>
    /// Update the current options properties with values from the <see cref="IResultList"/>.
    /// </summary>
    /// <param name="resultList">A result list returned by an search execution.</param>
    /// <returns>The same instance for chaining calls.</returns>
    public SearchOptions UpdateFromResult(IResultList resultList)
    {
        LastCount = resultList.Count;
        ItemsPerPage = resultList.ItemsPerPage;
        Page = resultList.Page;
        return this;
    }

    /// <summary>
    /// Ensures that the search options are not empty.
    /// </summary>
    public void AvoidEmpty()
    {
        if (Page is null && ItemsPerPage is null && Skip is null && Take is null)
        {
            Page = 1;
            ItemsPerPage = 10;
        }
    }
}
