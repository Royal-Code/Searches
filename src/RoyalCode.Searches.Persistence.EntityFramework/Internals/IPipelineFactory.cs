using Microsoft.EntityFrameworkCore;
using RoyalCode.Searches.Persistence.Abstractions.Pipeline;

#pragma warning disable S2326 // Unused type parameters should be removed

namespace RoyalCode.Searches.Persistence.EntityFramework.Internals;

/// <summary>
///     Factory to create search pipelines and searches for all entities using a specific <see cref="DbContext"/>.
/// </summary>
/// <typeparam name="TDbContext">
///     The <see cref="DbContext"/> type to use for the search pipelines and searches for all entities.
/// </typeparam>
public interface IPipelineFactory<TDbContext> : IPipelineFactory
    where TDbContext : DbContext
{ }
