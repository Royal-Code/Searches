using Microsoft.EntityFrameworkCore;
using RoyalCode.Searches.Abstractions;
using RoyalCode.Searches.EntityFramework.Internals;

#pragma warning disable S2326 // Unused type parameters should be removed

namespace RoyalCode.Searches.EntityFramework;

/// <summary>
/// Default implementation for <see cref="ISearchManager{TDbContext}"/>.
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public class SearchManager<TDbContext> : ISearchManager<TDbContext>
    where TDbContext : DbContext
{
    private readonly IPipelineFactory<TDbContext> pipelineFactory;

    /// <summary>
    /// Creates a new search manager with the pipeline factory for the <typeparamref name="TDbContext"/>.
    /// </summary>
    /// <param name="pipelineFactory"></param>
    public SearchManager(IPipelineFactory<TDbContext> pipelineFactory)
    {
        this.pipelineFactory = pipelineFactory;
    }

    /// <inheritdoc />
    public IAllEntities<TEntity> All<TEntity>() where TEntity : class
    {
        return new InternalAllEntities<TDbContext, TEntity>(pipelineFactory);
    }

    /// <inheritdoc />
    public ISearch<TEntity> Search<TEntity>() where TEntity : class
    {
        return new InternalSearch<TDbContext, TEntity>(pipelineFactory);
    }
}