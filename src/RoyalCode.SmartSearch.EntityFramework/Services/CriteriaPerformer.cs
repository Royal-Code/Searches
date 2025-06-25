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
/// <typeparam name="TDbContext">
///     The type of the <see cref="DbContext"/> used to access the database.
/// </typeparam>
/// <typeparam name="TEntity">
///     The type of the entity for which queries are prepared. Must be a reference type.
/// </typeparam>
public sealed class CriteriaPerformer<TDbContext, TEntity> : ICriteriaPerformer<TDbContext, TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
    private readonly TDbContext db;
    private readonly ISpecifierFactory specifierFactory;
    private readonly IOrderByProvider orderByProvider;
    private readonly ISelectorFactory selectorFactory;

    /// <summary>
    /// Creates a new instance of <see cref="CriteriaPerformer{TDbContext, TEntity}"/>.
    /// </summary>
    /// <param name="db">The <see cref="DbContext"/> to get the queryable for the entity type.</param>
    /// <param name="specifierFactory">The factory to create specifiers for the query.</param>
    /// <param name="orderByProvider">The provider for ordering the results.</param>
    /// <param name="selectorFactory">The factory for selecting results.</param>
    public CriteriaPerformer(
        TDbContext db,
        ISpecifierFactory specifierFactory,
        IOrderByProvider orderByProvider,
        ISelectorFactory selectorFactory)
    {
        this.db = db;
        this.specifierFactory = specifierFactory;
        this.orderByProvider = orderByProvider;
        this.selectorFactory = selectorFactory;
    }

    /// <inheritdoc />
    public IPreparedQuery<TEntity> Prepare(CriteriaOptions options)
    {
        var entities = options.TrackingEnabled 
            ? db.Set<TEntity>()
            : db.Set<TEntity>().AsNoTracking();

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
}
