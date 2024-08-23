using Microsoft.EntityFrameworkCore;
using RoyalCode.Searches.Core.Pipeline;

namespace RoyalCode.Searches.EntityFramework.Internals;

internal sealed class InternalSearch<TDbContext, TEntity> : Search<TEntity>, ISearch<TDbContext, TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    public InternalSearch(IPipelineFactory<TDbContext> factory) : base(factory) { }
}
