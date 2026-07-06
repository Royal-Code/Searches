using Microsoft.Extensions.DependencyInjection.Extensions;
using RoyalCode.SmartSearch;
using RoyalCode.SmartSearch.EntityFramework.Npgsql.Filtering;
using RoyalCode.SmartSearch.Linq.Filtering;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class NpgsqlSearchesServiceCollectionExtensions
{
    /// <summary>
    /// <para>
    ///     Adds the PostgreSQL emission of <see cref="CriterionOperator.Like"/> criteria:
    ///     <c>EF.Functions.ILike</c> for <see cref="CriterionCase.Insensitive"/> (native <c>ILIKE</c>)
    ///     and <c>EF.Functions.Like</c> for the remaining cases.
    /// </para>
    /// <para>
    ///     The registration order matters (first-non-null-wins): the ILike factory is registered before the
    ///     EF Like factory, so insensitive criteria are handled by <c>ILIKE</c>.
    /// </para>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddNpgsqlLikeOperators(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor
            .Singleton<ICriterionOperatorExpressionFactory, NpgsqlILikeExpressionFactory>());
        services.AddEntityFrameworkLikeOperator();
        return services;
    }
}
