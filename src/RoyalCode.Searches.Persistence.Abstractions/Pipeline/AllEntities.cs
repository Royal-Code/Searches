using RoyalCode.Searches.Abstractions;
using System.Runtime.CompilerServices;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ICollection<TEntity> Collect()
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.Execute(criteria);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<ICollection<TEntity>> CollectAsync(CancellationToken cancellationToken = default)
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.ExecuteAsync(criteria, cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exists()
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.Any(criteria);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.AnyAsync(criteria, cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TEntity? First()
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.First(criteria);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TEntity?> FirstAsync(CancellationToken cancellationToken = default)
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.FirstAsync(criteria, cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TEntity Single()
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.Single(criteria);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TEntity> SingleAsync(CancellationToken cancellationToken = default)
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.SingleAsync(criteria, cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DeleteAll()
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        pipeline.RemoveAll(criteria);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.RemoveAllAsync(criteria, cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateAll(Action<TEntity> updateAction)
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        pipeline.UpdateAll(criteria, updateAction);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task UpdateAllAsync(Action<TEntity> updateAction, CancellationToken cancellationToken = default)
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.UpdateAllAsync(criteria, updateAction, cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateAll<TData>(TData data, Action<TEntity, TData> updateAction)
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        pipeline.UpdateAll(criteria, data, updateAction);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task UpdateAllAsync<TData>(TData data, Action<TEntity, TData> updateAction,
        CancellationToken cancellationToken = default)
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.UpdateAllAsync(criteria, data, updateAction, cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateAll<TData, TId>(
        ICollection<TData> collection,
        Func<TEntity, TId> entityIdGet,
        Func<TData, TId> dataIdGet,
        Action<TEntity, TData> updateAction) where TData : class
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        pipeline.UpdateAll(criteria, collection, entityIdGet, dataIdGet, updateAction);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task UpdateAllAsync<TData, TId>(
        ICollection<TData> collection,
        Func<TEntity, TId> entityIdGet,
        Func<TData, TId> dataIdGet,
        Action<TEntity, TData> updateAction,
        CancellationToken cancellationToken = default) where TData : class
    {
        var pipeline = factory.CreateAllEntities<TEntity>();
        return pipeline.UpdateAllAsync(criteria, collection, entityIdGet, dataIdGet, updateAction, cancellationToken);
    }
}
