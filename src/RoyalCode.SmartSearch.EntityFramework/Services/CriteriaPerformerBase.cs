using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Defaults;
using RoyalCode.SmartSearch.Linq.Services;
using RoyalCode.SmartSearch.Linq.Sortings;
using RoyalCode.SmartSearch.Services;

namespace RoyalCode.SmartSearch.EntityFramework.Services;

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
public abstract class CriteriaPerformerBase<TEntity> : ICriteriaPerformer<TEntity>
    where TEntity : class
{
    private readonly ISpecifierFactory specifierFactory;
    private readonly IOrderByProvider orderByProvider;
    private readonly ISelectorFactory selectorFactory;

    /// <summary>
    /// Creates a new instance of <see cref="CriteriaPerformer{TDbContext, TEntity}"/>.
    /// </summary>
    /// <param name="specifierFactory">The factory to create specifiers for the query.</param>
    /// <param name="orderByProvider">The provider for ordering the results.</param>
    /// <param name="selectorFactory">The factory for selecting results.</param>
    protected CriteriaPerformerBase(
        ISpecifierFactory specifierFactory,
        IOrderByProvider orderByProvider,
        ISelectorFactory selectorFactory)
    {
        this.specifierFactory = specifierFactory;
        this.orderByProvider = orderByProvider;
        this.selectorFactory = selectorFactory;
    }

    /// <inheritdoc />
    public IPreparedQuery<TEntity> Prepare(CriteriaOptions options)
    {
        var entities = GetQueryable(options.TrackingEnabled);

        var criteriaQuery = new CriteriaQuery<TEntity>(
            entities,
            specifierFactory,
            orderByProvider,
            selectorFactory);

        foreach (var filter in options.Filters)
            filter.ApplyFilter(criteriaQuery);

        criteriaQuery.OrderBy(options.Sortings);
        criteriaQuery.SetPageSkipTakeCount(
            options.GetPageNumber(), 
            options.GetSkipCount(),
            options.GetTakeCount(), 
            options.UseCount,
            options.LastCount);

        return criteriaQuery;
    }

    /// <summary>
    /// Provides an <see cref="IQueryable{T}"/> for querying entities of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <remarks>
    ///     Use this method to retrieve a queryable source for entities, optionally enabling or disabling
    ///     change tracking. Disabling tracking is recommended for scenarios where entities are only read and not
    ///     modified.
    /// </remarks>
    /// <param name="trackingEnabled">
    ///     A value indicating whether change tracking is enabled for the returned query. 
    ///     <br />
    ///     <see langword="true"/> enables change tracking, allowing entity modifications to be tracked by the context.
    ///     <br />    
    ///     <see langword="false"/> disables change tracking, which can improve performance for read-only operations.
    /// </param>
    /// <returns>
    ///     An <see cref="IQueryable{TEntity}"/> that can be used to query entities of type <typeparamref name="TEntity"/>.
    /// </returns>
    protected abstract IQueryable<TEntity> GetQueryable(bool trackingEnabled);
}
