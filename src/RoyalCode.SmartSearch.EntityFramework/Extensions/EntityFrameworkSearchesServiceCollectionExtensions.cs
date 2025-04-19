using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RoyalCode.SmartSearch.Abstractions;
using RoyalCode.SmartSearch.EntityFramework;
using RoyalCode.SmartSearch.EntityFramework.Configurations;
using RoyalCode.SmartSearch.EntityFramework.Internals;
using RoyalCode.SmartSearch.Linq;
using RoyalCode.SmartSearch.Linq.Filter;
using RoyalCode.SmartSearch.Linq.Selector;
using RoyalCode.SmartSearch.Linq.Sorter;

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
    /// <para>
    ///     You can also use a <see cref="ISearchManager{TDbContext}"/> to create 
    ///     <see cref="ISearch{TEntity}"/> and <see cref="IAllEntities{TEntity}"/>
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

        configureAction(new SearchConfigurations<TDbContext>(services));
        return services;
    }

    /// <summary>
    /// <para>
    ///     Adds services to work with <see cref="ISearch{TEntity}"/> and <see cref="IAllEntities{TEntity}"/>
    ///     using the entity framework.
    /// </para>
    /// <para>
    ///     It will be necessary to use <see cref="ISearchManager"/> or <see cref="ISearchManager{TDbContext}"/>
    ///     to create <see cref="ISearch{TEntity}"/> and <see cref="IAllEntities{TEntity}"/>
    ///     of entities related to the <typeparamref name="TDbContext"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TDbContext">The <see cref="DbContext"/> to use for performing the searches.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddSearchManager<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddSmartSearchLinq();
        services.TryAddTransient<IPipelineFactory<TDbContext>, PipelineFactory<TDbContext>>();
        services.TryAddTransient<ISearchManager<TDbContext>, SearchManager<TDbContext>>();
        services.TryAddTransient<ISearchManager, SearchManager<TDbContext>>();

        return services;
    }

    /// <summary>
    /// <para>
    ///     Creates a new <see cref="ISearch{TEntity}"/> for the entity <typeparamref name="TEntity"/>
    ///     using the <see cref="DbContext"/> used by the unit of work.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="db"></param>
    /// <returns></returns>
    public static ISearch<TEntity> Search<TEntity>(this DbContext db)
        where TEntity : class
    {
        var specifierFactory = db.GetService<ISpecifierFactory>();
        var orderByFactory = db.GetService<IOrderByProvider>();
        var selectorFactory = db.GetService<ISelectorFactory>();

        var pipelineFacotry = new PipelineFactory<DbContext>(db, specifierFactory, orderByFactory, selectorFactory);

        var search = new InternalSearch<DbContext, TEntity>(pipelineFacotry);

        return search;
    }
}
