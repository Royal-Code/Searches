using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Core.Pipeline;

namespace RoyalCode.SmartSearch.EntityFramework.Internals;

internal sealed class InternalAllEntities<TDbContext, TEntity> : AllEntities<TEntity>, IAllEntities<TDbContext, TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    public InternalAllEntities(IPipelineFactory<TDbContext> factory) : base(factory) { }
}