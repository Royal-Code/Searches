using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RoyalCode.SmartSearch;
using RoyalCode.SmartSearch.EntityFramework.Configurations;
using RoyalCode.SmartSearch.EntityFramework.Services;
using RoyalCode.SmartSearch.Linq;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class EntityFrameworkSearchesServiceCollectionExtensions
{
    /// <summary>
    /// <para>
    ///     Add services for <see cref="ICriteria{TEntity}"/>
    ///     using entity framework and defining the <see cref="DbContext"/> for performing the searches.
    /// </para>
    /// <para>
    ///     You can also use a <see cref="ISearchManager{TDbContext}"/> to create 
    ///     <see cref="ICriteria{TEntity}"/>
    ///     of entities related to the <typeparamref name="TDbContext"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TDbContext">The <see cref="DbContext"/> to use for performing the searches.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureAction">The action to configure the <see cref="ISearchConfigurations{TDbContext}"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     When <paramref name="configureAction"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddEntityFrameworkSearches<TDbContext>(this IServiceCollection services,
        Action<ISearchConfigurations<TDbContext>> configureAction)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(configureAction);

        services.AddSearchManager<TDbContext>();

        configureAction(new SearchConfigurations<TDbContext>(services));
        return services;
    }

    /// <summary>
    /// <para>
    ///     Adds services to work with <see cref="ICriteria{TEntity}"/> and <see cref="ISearch{TEntity}"/>
    ///     using the entity framework.
    /// </para>
    /// <para>
    ///     It will be necessary to use <see cref="ISearchManager"/> or <see cref="ISearchManager{TDbContext}"/>
    ///     to create <see cref="ICriteria{TEntity}"/> and <see cref="ISearch{TEntity}"/>
    ///     of entities related to the <typeparamref name="TDbContext"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TDbContext">The <see cref="DbContext"/> to use for performing the searches.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddSearchManager<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        if (services.Any(d => d.ServiceType == typeof(ISearchManager<TDbContext>)))
            return services;

        services.AddSmartSearchLinq();
        services.TryAddTransient<ISearchManager<TDbContext>, SearchManager<TDbContext>>();
        services.TryAddTransient<ISearchManager, SearchManager<TDbContext>>();

        return services;
    }
}
