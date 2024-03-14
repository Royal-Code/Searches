using Microsoft.EntityFrameworkCore;
using RoyalCode.Searches.Persistence.Linq;

namespace RoyalCode.Searches.Persistence.EntityFramework;

/// <inheritdoc />
internal sealed class Removable<TEntity> : IRemovable<TEntity>
    where TEntity : class
{
    private readonly DbContext db;

    public Removable(DbContext db)
    {
        this.db = db;
    }

    /// <inheritdoc />
    public void RemoveAll(IQueryable<TEntity> entities)
    {
        db.RemoveRange(entities);
    }

    /// <inheritdoc />
    public async Task RemoveAllAsync(IQueryable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        db.RemoveRange(await entities.ToListAsync(cancellationToken: cancellationToken));
    }
}