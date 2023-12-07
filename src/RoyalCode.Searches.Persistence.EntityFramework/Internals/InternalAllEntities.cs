using Microsoft.EntityFrameworkCore;
using RoyalCode.Searches.Persistence.Abstractions.Pipeline;

namespace RoyalCode.Searches.Persistence.EntityFramework.Internals;

internal sealed class InternalAllEntities<TDbContext, TEntity> : AllEntities<TEntity>, IAllEntities<TDbContext, TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    public InternalAllEntities(IPipelineFactory<TDbContext> factory) : base(factory) { }
}