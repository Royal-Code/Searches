using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Linq;

namespace RoyalCode.SmartSearch.EntityFramework.Configurations;

/// <summary>
///     Configure searches for the unit of work.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public interface ISearchConfigurations<out TDbContext> : ISearchConfigurations
    where TDbContext : DbContext
{
    /// <summary>
    /// Add a search for an entity as a service, related to <see cref="DbContext"/> used by the unit of work.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>The same instance.</returns>
    ISearchConfigurations<TDbContext> Add<TEntity>()
        where TEntity : class;
}
