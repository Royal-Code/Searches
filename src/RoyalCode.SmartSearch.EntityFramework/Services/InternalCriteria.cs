using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Defaults;

namespace RoyalCode.SmartSearch.EntityFramework.Services;

internal sealed class InternalCriteria<TDbContext, TEntity> : Criteria<TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
    public InternalCriteria(ICriteriaPerformer<TDbContext, TEntity> performer) : base(performer) { }
}
