// Ignore Spelling: Sortings

using System.Linq.Expressions;
using RoyalCode.SmartSearch.Abstractions;

namespace RoyalCode.SmartSearch.Core;

/// <summary>
/// The criteria for performing the search.
/// </summary>
public class SearchCriteria
{
    private List<SearchFilter>? filters;
    private List<ISorting>? sortings;

    /// <summary>
    /// Get all filters.
    /// </summary>
    public IEnumerable<SearchFilter> Filters => filters ?? Enumerable.Empty<SearchFilter>();

    /// <summary>
    /// Get all sortings.
    /// </summary>
    public IEnumerable<ISorting> Sortings => sortings ?? Enumerable.Empty<ISorting>();

    /// <summary>
    /// Information about the select expression.
    /// </summary>
    public SearchSelect? Select { get; private set; }

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
    /// Adds a new filter to specify the search.
    /// </summary>
    /// <param name="modelType">The query model type.</param>
    /// <param name="filter">The filter instance.</param>
    public void AddFilter<TFilter>(Type modelType, TFilter filter)
        where TFilter : class
    {
        filters ??= [];
        filters.Add(new SearchFilter<TFilter>(modelType, filter));
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
    /// Set the select expression.
    /// </summary>
    /// <param name="selectExpression">The select expression.</param>
    /// <typeparam name="TEntity">The query entity type.</typeparam>
    /// <typeparam name="TDto">The select type.</typeparam>
    /// <exception cref="ArgumentNullException">If expression is null.</exception>
    public void SetSelectExpression<TEntity, TDto>(Expression<Func<TEntity, TDto>> selectExpression)
    {
        ArgumentNullException.ThrowIfNull(selectExpression);

        Select = new SearchSelect(typeof(TEntity), typeof(TDto), selectExpression);
    }

    /// <summary>
    /// Whether the query should be paginated.
    /// </summary>
    public bool Paginate => ItemsPerPage > 0;

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
    public int GetSkipCount() => Paginate ? ItemsPerPage * (GetPageNumber() - 1) : 0;

    /// <summary>
    /// Default values for each new <see cref="SearchCriteria"/> created.
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