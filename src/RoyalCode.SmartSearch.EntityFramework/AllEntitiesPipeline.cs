using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Core.Pipeline;
using RoyalCode.SmartSearch.Defaults;
using RoyalCode.SmartSearch.Linq;
using RoyalCode.SmartSearch.Linq.Services;
using System.Runtime.CompilerServices;

namespace RoyalCode.SmartSearch.EntityFramework;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Any(CriteriaOptions searchCriteria)
    {
        return PrepareQuery(searchCriteria).Any();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<bool> AnyAsync(CriteriaOptions searchCriteria, CancellationToken cancellationToken = default)
    {
        return PrepareQuery(searchCriteria).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ICollection<TEntity> Execute(CriteriaOptions searchCriteria)
    {
        return PrepareQuery(searchCriteria).ToList();
    }

    /// <inheritdoc />
    public async Task<ICollection<TEntity>> ExecuteAsync(
        CriteriaOptions searchCriteria,
        CancellationToken cancellationToken = default)
    {
        return await PrepareQuery(searchCriteria).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TEntity? First(CriteriaOptions searchCriteria)
    {
        return PrepareQuery(searchCriteria).FirstOrDefault();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TEntity?> FirstAsync(CriteriaOptions searchCriteria, CancellationToken cancellationToken = default)
    {
        return PrepareQuery(searchCriteria).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAll(CriteriaOptions searchCriteria)
    {
        var query = PrepareQuery(searchCriteria);
        var removable = queryableProvider.GetRemovable();
        removable.RemoveAll(query);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task RemoveAllAsync(CriteriaOptions searchCriteria, CancellationToken cancellationToken = default)
    {
        var query = PrepareQuery(searchCriteria);
        var removable = queryableProvider.GetRemovable();
        return removable.RemoveAllAsync(query, cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TEntity Single(CriteriaOptions searchCriteria)
    {
        return PrepareQuery(searchCriteria).Single();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TEntity> SingleAsync(CriteriaOptions searchCriteria, CancellationToken cancellationToken = default)
    {
        return PrepareQuery(searchCriteria).SingleAsync(cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateAll(CriteriaOptions searchCriteria, Action<TEntity> updateAction)
    {
        var query = PrepareQuery(searchCriteria);
        foreach(var entity in query)
        {
            updateAction(entity);
        }
    }

    /// <inheritdoc />
    public async Task UpdateAllAsync(
        CriteriaOptions searchCriteria,
        Action<TEntity> updateAction,
        CancellationToken cancellationToken = default)
    {
        var query = PrepareQuery(searchCriteria);
        await foreach (var entity in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            updateAction(entity);
        }
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateAll<TData>(CriteriaOptions searchCriteria, TData data, Action<TEntity, TData> updateAction)
    {
        var query = PrepareQuery(searchCriteria);
        foreach(var entity in query)
        {
            updateAction(entity, data);
        }
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task UpdateAllAsync<TData>(
        CriteriaOptions searchCriteria,
        TData data,
        Action<TEntity, TData> updateAction,
        CancellationToken cancellationToken = default)
    {
        var query = PrepareQuery(searchCriteria);
        await foreach (var entity in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            updateAction(entity, data);
        }
    }

    /// <inheritdoc />
    public void UpdateAll<TData, TId>(
        CriteriaOptions searchCriteria,
        ICollection<TData> collection,
        Func<TEntity, TId> entityIdGet,
        Func<TData, TId> dataIdGet,
        Action<TEntity, TData> updateAction)
        where TData : class
    {
        var query = PrepareQuery(searchCriteria);
        foreach (var entity in query)
        {
            var entityId = entityIdGet(entity);
            var data = collection.FirstOrDefault(d => Equals(dataIdGet(d), entityId));
            if (data is not null)
                updateAction(entity, data);
            else
                throw new ArgumentOutOfRangeException(
                    nameof(collection), 
                    $"The collection does not contain any data with the id '{entityId}'");
        }
    }

    /// <inheritdoc />
    public async Task UpdateAllAsync<TData, TId>(
        CriteriaOptions searchCriteria,
        ICollection<TData> collection,
        Func<TEntity, TId> entityIdGet,
        Func<TData, TId> dataIdGet,
        Action<TEntity, TData> updateAction,
        CancellationToken cancellationToken = default)
        where TData : class
    {
        var query = PrepareQuery(searchCriteria);
        await foreach (var entity in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            var entityId = entityIdGet(entity);
            var data = collection.FirstOrDefault(d => Equals(dataIdGet(d), entityId));
            if (data is not null)
                updateAction(entity, data);
            else
                throw new ArgumentOutOfRangeException(
                    nameof(collection), 
                    $"The collection does not contain any data with the id '{entityId}'");
        }
    }
}
