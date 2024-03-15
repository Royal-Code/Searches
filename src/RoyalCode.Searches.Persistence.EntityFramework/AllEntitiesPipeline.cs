using Microsoft.EntityFrameworkCore;
using RoyalCode.Searches.Persistence.Abstractions;
using RoyalCode.Searches.Persistence.Abstractions.Pipeline;
using RoyalCode.Searches.Persistence.Linq;
using RoyalCode.Searches.Persistence.Linq.Filter;
using System.Runtime.CompilerServices;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Any(SearchCriteria searchCriteria)
    {
        return PrepareQuery(searchCriteria).Any();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<bool> AnyAsync(SearchCriteria searchCriteria, CancellationToken cancellationToken = default)
    {
        return PrepareQuery(searchCriteria).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TEntity? First(SearchCriteria searchCriteria)
    {
        return PrepareQuery(searchCriteria).FirstOrDefault();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TEntity?> FirstAsync(SearchCriteria searchCriteria, CancellationToken cancellationToken = default)
    {
        return PrepareQuery(searchCriteria).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAll(SearchCriteria searchCriteria)
    {
        var query = PrepareQuery(searchCriteria);
        var removable = queryableProvider.GetRemovable();
        removable.RemoveAll(query);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task RemoveAllAsync(SearchCriteria searchCriteria, CancellationToken cancellationToken = default)
    {
        var query = PrepareQuery(searchCriteria);
        var removable = queryableProvider.GetRemovable();
        return removable.RemoveAllAsync(query, cancellationToken);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TEntity Single(SearchCriteria searchCriteria)
    {
        return PrepareQuery(searchCriteria).Single();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TEntity> SingleAsync(SearchCriteria searchCriteria, CancellationToken cancellationToken = default)
    {
        return PrepareQuery(searchCriteria).SingleAsync(cancellationToken);
    }

    public void UpdateAll(SearchCriteria searchCriteria, Action<TEntity> updateAction)
    {
        var query = PrepareQuery(searchCriteria);
        foreach(var entity in query)
        {
            updateAction(entity);
        }
    }

    public async Task UpdateAllAsync(
        SearchCriteria searchCriteria,
        Action<TEntity> updateAction,
        CancellationToken cancellationToken = default)
    {
        var query = PrepareQuery(searchCriteria);
        await foreach (var entity in query.AsAsyncEnumerable())
        {
            cancellationToken.ThrowIfCancellationRequested();
            updateAction(entity);
        }
    }

    public void UpdateAll<TData>(SearchCriteria searchCriteria, TData data, Action<TEntity, TData> updateAction)
    {
        var query = PrepareQuery(searchCriteria);
        foreach(var entity in query)
        {
            updateAction(entity, data);
        }
    }

    public async Task UpdateAllAsync<TData>(
        SearchCriteria searchCriteria,
        TData data,
        Action<TEntity, TData> updateAction,
        CancellationToken cancellationToken = default)
    {
        var query = PrepareQuery(searchCriteria);
        await foreach (var entity in query.AsAsyncEnumerable())
        {
            cancellationToken.ThrowIfCancellationRequested();
            updateAction(entity, data);
        }
    }

    public void UpdateAll<TData, TId>(
        SearchCriteria searchCriteria,
        IEnumerable<TData> collection,
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
        }
    }

    public async Task UpdateAllAsync<TData, TId>(
        SearchCriteria searchCriteria,
        IEnumerable<TData> collection,
        Func<TEntity, TId> entityIdGet,
        Func<TData, TId> dataIdGet,
        Action<TEntity, TData> updateAction,
        CancellationToken cancellationToken = default)
        where TData : class
    {
        var query = PrepareQuery(searchCriteria);
        await foreach (var entity in query.AsAsyncEnumerable())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entityId = entityIdGet(entity);
            var data = collection.FirstOrDefault(d => Equals(dataIdGet(d), entityId));
            if (data is not null)
                updateAction(entity, data);
        }
    }
}
