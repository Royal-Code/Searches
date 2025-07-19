using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Linq;

namespace RoyalCode.SmartSearch.EntityFramework.Configurations;

/// <summary>
///     Configure searches for the unit of work.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public interface ISearchConfigurations<out TDbContext> : ISearchConfigurations
    where TDbContext : DbContext
{ }
