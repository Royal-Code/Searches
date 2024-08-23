using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RoyalCode.Searches.Abstractions;
using RoyalCode.Searches.EntityFramework.Internals;
using RoyalCode.Searches.Linq;

namespace RoyalCode.Searches.EntityFramework.Configurations;

/// <inheritdoc />
public class SearchConfigurations<TDbContext> : ISearchConfigurations<TDbContext>
    where TDbContext : DbContext
{
    private readonly IServiceCollection services;

    /// <summary>
    /// Creates a new instance of <see cref="SearchConfigurations{TDbContext}"/>.
    /// </summary>
    /// <param name="services">The service collection to register the searches as a service.</param>
    public SearchConfigurations(IServiceCollection services)
    {
        this.services = services;

        services.AddSearchesLinq();
        services.TryAddTransient<IPipelineFactory<TDbContext>, PipelineFactory<TDbContext>>();
        services.TryAddTransient<ISearchManager<TDbContext>, SearchManager<TDbContext>>();
    }

    /// <inheritdoc />
    public ISearchConfigurations<TDbContext> Add<TEntity>() where TEntity : class
    {
        // add search as a service for the respective context
        var searchType = typeof(ISearch<>).MakeGenericType(typeof(TEntity));
        var dbSearchType = typeof(Internals.ISearch<,>).MakeGenericType(typeof(TDbContext), typeof(TEntity));
        var searchImplType = typeof(InternalSearch<,>).MakeGenericType(typeof(TDbContext), typeof(TEntity));

        services.Add(ServiceDescriptor.Describe(
            dbSearchType,
            searchImplType,
            ServiceLifetime.Transient));

        services.Add(ServiceDescriptor.Describe(
            searchType,
            sp => sp.GetService(dbSearchType)!,
            ServiceLifetime.Transient));

        // add all entities as a service for the respective context
        var allType = typeof(IAllEntities<>).MakeGenericType(typeof(TEntity));
        var dbAllType = typeof(IAllEntities<,>).MakeGenericType(typeof(TDbContext), typeof(TEntity));
        var allImplType = typeof(InternalAllEntities<,>).MakeGenericType(typeof(TDbContext), typeof(TEntity));

        services.Add(ServiceDescriptor.Describe(
            dbAllType,
            allImplType,
            ServiceLifetime.Transient));

        services.Add(ServiceDescriptor.Describe(
            allType,
            sp => sp.GetService(dbAllType)!,
            ServiceLifetime.Transient));

        return this;
    }
}