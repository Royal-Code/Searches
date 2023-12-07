using Microsoft.EntityFrameworkCore;
using RoyalCode.Searches.Abstractions;
using RoyalCode.Searches.Persistence.EntityFramework.Configurations;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class EntityFrameworkSearchesServiceCollectionExtensions

{
    /// <summary>
    /// <para>
    ///     Add services for <see cref="ISearch{TEntity}"/> and <see cref="IAllEntities{TEntity}"/>
    ///     using entity framework and defining the <see cref="DbContext"/> for performing the searches.
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
        if (configureAction is null)
            throw new ArgumentNullException(nameof(configureAction));

        configureAction(new SearchConfigurations<TDbContext>(services));
        return services;
    }
}
