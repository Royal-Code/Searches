using Microsoft.EntityFrameworkCore;
using RoyalCode.OperationHint.Abstractions;
using RoyalCode.SmartSearch.Core.Pipeline;
using RoyalCode.SmartSearch.EntityFramework.Internals;
using RoyalCode.SmartSearch.Linq.Filter;
using RoyalCode.SmartSearch.Linq.Selector;
using RoyalCode.SmartSearch.Linq.Sorter;

namespace RoyalCode.SmartSearch.EntityFramework;

/// <inheritdoc />
public sealed class PipelineFactory<TDbContext> : IPipelineFactory<TDbContext>
    where TDbContext : DbContext
{
    private readonly TDbContext db;
    private readonly ISpecifierFactory specifierFactory;
    private readonly IOrderByProvider orderByProvider;
    private readonly ISelectorFactory selectorFactory;
    private readonly IHintPerformer? hintPerformer;

    /// <summary>
    /// <para>
    ///     Create a new instance of <see cref="PipelineFactory{TDbContext}"/>.
    /// </para>
    /// </summary>
    /// <param name="db">The database context.</param>
    /// <param name="specifierFactory">The specifier factory.</param>
    /// <param name="orderByProvider">The order by provider.</param>
    /// <param name="selectorFactory">The selector factory.</param>
    /// <param name="hintPerformer">Optional, the hint performer.</param>
    public PipelineFactory(
        TDbContext db,
        ISpecifierFactory specifierFactory,
        IOrderByProvider orderByProvider,
        ISelectorFactory selectorFactory,
        IHintPerformer? hintPerformer = null)
    {
        this.db = db;
        this.specifierFactory = specifierFactory;
        this.orderByProvider = orderByProvider;
        this.selectorFactory = selectorFactory;
        this.hintPerformer = hintPerformer;
    }

    /// <inheritdoc />
    public ISearchPipeline<TEntity> Create<TEntity>() where TEntity : class
    {
        var queryableProvider = new QueryableProvider<TDbContext, TEntity>(db);
        var sorter = new DefaultSorter<TEntity>(orderByProvider);
        return new SearchPipeline<TEntity>(queryableProvider, specifierFactory, sorter);
    }

    /// <inheritdoc />
    public ISearchPipeline<TDto> Create<TEntity, TDto>() where TEntity : class where TDto : class
    {
        var queryableProvider = new QueryableProvider<TDbContext, TEntity>(db);
        var sorter = new DefaultSorter<TEntity>(orderByProvider);
        var selector = selectorFactory.Create<TEntity, TDto>();
        return new SearchPipeline<TEntity, TDto>(queryableProvider, specifierFactory, sorter, selector);
    }

    /// <inheritdoc />
    public IAllEntitiesPipeline<TEntity> CreateAllEntities<TEntity>() where TEntity : class
    {
        var queryableProvider = new QueryableProvider<TDbContext, TEntity>(db, true, hintPerformer);
        var sorter = new DefaultSorter<TEntity>(orderByProvider);
        return new AllEntitiesPipeline<TEntity>(queryableProvider, specifierFactory, sorter);
    }
}