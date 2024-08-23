using Microsoft.EntityFrameworkCore;
using RoyalCode.Searches.Core.Pipeline;

namespace RoyalCode.Searches.EntityFramework.Internals;

internal sealed class InternalAllEntities<TDbContext, TEntity> : AllEntities<TEntity>, IAllEntities<TDbContext, TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    public InternalAllEntities(IPipelineFactory<TDbContext> factory) : base(factory) { }
}