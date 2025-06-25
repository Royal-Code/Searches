using RoyalCode.SmartSearch.Defaults;
using RoyalCode.SmartSearch.Linq.Sortings;
using RoyalCode.SmartSearch.Services;

namespace RoyalCode.SmartSearch.Linq.Services;

/// <summary>
/// Provides functionality to prepare queries based on specified criteria for a given entity type.
/// </summary>
/// <remarks>
///     This class is designed to facilitate the creation of queries by applying filters, sorting, and
///     selection criteria to an underlying data source.
///     It uses various providers and factories to construct the query dynamically.
/// </remarks>
/// <typeparam name="TEntity">
///     The type of the entity for which queries are prepared. Must be a reference type.
/// </typeparam>
public class CriteriaPerformer<TEntity> : ICriteriaPerformer<TEntity>
    where TEntity : class
{
    private readonly IQueryableProvider<TEntity> queryableProvider;
    private readonly ISpecifierFactory specifierFactory;
    private readonly IOrderByProvider orderByProvider;
    private readonly ISelectorFactory selectorFactory;

    /// <summary>
    /// Creates a new instance of <see cref="CriteriaPerformer{TEntity}"/>.
    /// </summary>
    /// <param name="queryableProvider">The provider to get the queryable for the entity type.</param>
    /// <param name="specifierFactory">The factory to create specifiers for the query.</param>
    /// <param name="orderByProvider">The provider for ordering the results.</param>
    /// <param name="selectorFactory">The factory for selecting results.</param>
    public CriteriaPerformer(
        IQueryableProvider<TEntity> queryableProvider,
        ISpecifierFactory specifierFactory,
        IOrderByProvider orderByProvider,
        ISelectorFactory selectorFactory)
    {
        this.queryableProvider = queryableProvider;
        this.specifierFactory = specifierFactory;
        this.orderByProvider = orderByProvider;
        this.selectorFactory = selectorFactory;
    }

    /// <inheritdoc />
    public IPreparedQuery<TEntity> Prepare(CriteriaOptions options)
    {
        var queryable = queryableProvider.GetQueryable(options.TrackingEnabled);
        var criteriaQuery = new CriteriaQuery<TEntity>(queryable, specifierFactory, orderByProvider, selectorFactory);

        foreach (var filter in options.Filters)
            criteriaQuery.Specify(filter);

        criteriaQuery.OrderBy(options.Sortings);
        criteriaQuery.SetPageSkipTakeCount(
            options.GetPageNumber(), 
            options.GetSkipCount(),
            options.GetTakeCount(), 
            options.UseCount,
            options.LastCount);

        return criteriaQuery;
    }
}
