using RoyalCode.Searches.Abstractions;

namespace RoyalCode.Searches.Persistence.Abstractions.Pipeline;

/// <inheritdoc />
public class AllEntities<TEntity> : IAllEntities<TEntity>
    where TEntity : class
{
    private readonly IPipelineFactory factory;
    private readonly SearchCriteria criteria = new();

    /// <summary>
    /// Creates a new search with the <see cref="IAllEntitiesPipeline{TEntity}"/> to execute the query.
    /// </summary>
    /// <param name="factory">The pipeline factory for create the all entities pipeline.</param>
    public AllEntities(IPipelineFactory factory)
    {
        this.factory = factory;
    }

    /// <inheritdoc />
    public IAllEntities<TEntity> FilterBy<TFilter>(TFilter filter) where TFilter : class
    {
        criteria.AddFilter(typeof(TEntity), filter);
        return this;
    }

    /// <inheritdoc />
    public IAllEntities<TEntity> OrderBy(ISorting sorting)
    {
        criteria.AddSorting(sorting);
        return this;
    }

    /// <inheritdoc />
    public ICollection<TEntity> Collect()
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.Execute(criteria);
    }

    /// <inheritdoc />
    public Task<ICollection<TEntity>> CollectAsync(CancellationToken cancellationToken = default)
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.ExecuteAsync(criteria, cancellationToken);
    }
}
