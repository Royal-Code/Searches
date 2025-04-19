using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Core.Pipeline;

namespace RoyalCode.SmartSearch.EntityFramework.Internals;

internal sealed class InternalSearch<TDbContext, TEntity> : Search<TEntity>, ISearch<TDbContext, TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    public InternalSearch(IPipelineFactory<TDbContext> factory) : base(factory) { }
}
