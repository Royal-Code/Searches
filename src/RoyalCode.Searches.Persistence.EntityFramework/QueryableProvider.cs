using Microsoft.EntityFrameworkCore;
using RoyalCode.OperationHint.Abstractions;
using RoyalCode.Searches.Persistence.Linq;

namespace RoyalCode.Searches.Persistence.EntityFramework;

internal sealed class QueryableProvider<TDbContext, TEntity> : IQueryableProvider<TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
    private readonly TDbContext db;
    private readonly bool tracking;
    private readonly IHintPerformer? hintPerformer;

    public QueryableProvider(TDbContext db, bool tracking = false, IHintPerformer? hintPerformer = null)
    {
        this.db = db;
        this.tracking = tracking;
        this.hintPerformer = hintPerformer;
    }

    public IQueryable<TEntity> GetQueryable()
    {
        if (tracking)
        {
            IQueryable<TEntity> query = db.Set<TEntity>();
            return hintPerformer is null
                ? query
                : hintPerformer.Perform(query);
        }

        return db.Set<TEntity>().AsNoTracking();
    }
}