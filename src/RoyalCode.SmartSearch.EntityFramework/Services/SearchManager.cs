using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Linq.Services;
using RoyalCode.SmartSearch.Linq.Sortings;

#pragma warning disable S2326 // Unused type parameters should be removed

namespace RoyalCode.SmartSearch.EntityFramework.Services;

/// <summary>
/// Default implementation for <see cref="ISearchManager{TDbContext}"/>.
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public sealed class SearchManager<TDbContext> : ISearchManager<TDbContext>
    where TDbContext : DbContext
{
    private readonly TDbContext db;
    private readonly ISpecifierFactory specifierFactory;
    private readonly IOrderByProvider orderByProvider;
    private readonly ISelectorFactory selectorFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchManager{TDbContext}"/> class,  
    ///     providing the necessary dependencies for performing database searches.
    /// </summary>
    /// <param name="db">
    ///     The database context used to query the underlying data source. Must not be null.
    /// </param>
    /// <param name="specifierFactory">
    ///     The factory responsible for creating specifiers that define search criteria. Must not be null.
    /// </param>
    /// <param name="orderByProvider">
    ///     The provider used to specify ordering rules for search results. Must not be null.
    /// </param>
    /// <param name="selectorFactory">
    ///     The factory responsible for creating selectors that determine which fields to include in the search results.
    ///     Must not be null.
    /// </param>
    public SearchManager(
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
    public ICriteria<TEntity> Criteria<TEntity>() where TEntity : class
    {
        var performer = new CriteriaPerformer<TDbContext, TEntity>(
            db,
            specifierFactory,
            orderByProvider,
            selectorFactory);

        return new InternalCriteria<TDbContext, TEntity>(performer);
    }
}