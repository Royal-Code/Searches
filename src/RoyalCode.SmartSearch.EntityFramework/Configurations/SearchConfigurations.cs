using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.SmartSearch.EntityFramework.Services;
using RoyalCode.SmartSearch.Linq;

namespace RoyalCode.SmartSearch.EntityFramework.Configurations;

/// <inheritdoc />
public sealed class SearchConfigurations<TDbContext> : ISearchConfigurations<TDbContext>
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
    }

    /// <inheritdoc />
    public ISearchConfigurations Add<TEntity>() 
        where TEntity : class
    {
        return Add(typeof(TEntity));
    }

    /// <inheritdoc />
    public ISearchConfigurations Add(Type entityType)
    {
        // add criteria as a service for the respective context
        var serviceType = typeof(ICriteria<>).MakeGenericType(entityType);
        var implType = typeof(InternalCriteria<,>).MakeGenericType(typeof(TDbContext), entityType);

        services.Add(ServiceDescriptor.Describe(
            serviceType,
            implType,
            ServiceLifetime.Transient));

        // add criteria performer as a service for the respective context
        serviceType = typeof(ICriteriaPerformer<,>).MakeGenericType(typeof(TDbContext), entityType);
        implType = typeof(CriteriaPerformer<,>).MakeGenericType(typeof(TDbContext), entityType);

        services.Add(ServiceDescriptor.Describe(
            serviceType,
            implType,
            ServiceLifetime.Transient));

        return this;
    }
}