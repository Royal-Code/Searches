using Microsoft.EntityFrameworkCore;
using RoyalCode.Searches.Persistence.Abstractions;
using RoyalCode.Searches.Persistence.Abstractions.Pipeline;
using RoyalCode.Searches.Persistence.Linq;
using RoyalCode.Searches.Persistence.Linq.Filter;

namespace RoyalCode.Searches.Persistence.EntityFramework;

/// <summary>
/// Default implementation of <see cref="IAllEntitiesPipeline{TEntity}"/>.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public sealed class AllEntitiesPipeline<TEntity> : SearchPipelineBase<TEntity>, IAllEntitiesPipeline<TEntity>
    where TEntity : class
{
    /// <inheritdoc />
    public AllEntitiesPipeline(
        IQueryableProvider<TEntity> queryableProvider,
        ISpecifierFactory specifierFactory,
        ISorter<TEntity> sorter)
        : base(queryableProvider, specifierFactory, sorter)
    { }

    /// <inheritdoc />
    public ICollection<TEntity> Execute(SearchCriteria searchCriteria)
    {
        return PrepareQuery(searchCriteria).ToList();
    }

    /// <inheritdoc />
    public async Task<ICollection<TEntity>> ExecuteAsync(
        SearchCriteria searchCriteria,
        CancellationToken cancellationToken = default)
    {
        return await PrepareQuery(searchCriteria).ToListAsync(cancellationToken);
    }
}
