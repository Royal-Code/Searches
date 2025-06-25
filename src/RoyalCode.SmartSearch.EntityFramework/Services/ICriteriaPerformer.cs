using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Services;

namespace RoyalCode.SmartSearch.EntityFramework.Services;

/// <summary>
/// Defines functionality for performing criteria-based operations on entities within a specific database context.
/// </summary>
/// <typeparam name="TDbContext">
///     The type of the database context, which must derive from <see cref="DbContext"/>.
/// </typeparam>
/// <typeparam name="TEntity">
///     The type of the entity on which criteria-based operations are performed, which must be a reference type.
/// </typeparam>
public interface ICriteriaPerformer<TDbContext, TEntity> : ICriteriaPerformer<TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
}
