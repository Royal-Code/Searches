using Microsoft.EntityFrameworkCore.Infrastructure;
using RoyalCode.SmartSearch;
using RoyalCode.SmartSearch.Defaults;
using RoyalCode.SmartSearch.EntityFramework.Services;
using RoyalCode.SmartSearch.Linq.Services;
using RoyalCode.SmartSearch.Linq.Sortings;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Extensions methods for <see cref="DbContext"/> to create <see cref="ICriteria{TEntity}"/>.
/// </summary>
public static class EFSearchesExtensions
{
    /// <summary>
    /// <para>
    ///     Creates a new <see cref="ICriteria{TEntity}"/> for the entity <typeparamref name="TEntity"/>
    ///     using the <see cref="DbContext"/> used by the unit of work.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The entity type to create the criteria for.</typeparam>
    /// <param name="db">The <see cref="DbContext"/> to use for performing the searches.</param>
    /// <returns>A new <see cref="ICriteria{TEntity}"/> instance.</returns>
    public static ICriteria<TEntity> Criteria<TEntity>(this DbContext db)
        where TEntity : class
    {
        var specifierFactory = db.GetService<ISpecifierFactory>();
        var orderByFactory = db.GetService<IOrderByProvider>();
        var selectorFactory = db.GetService<ISelectorFactory>();

        var preparer = new CriteriaPerformer<DbContext, TEntity>(
            db,
            specifierFactory,
            orderByFactory,
            selectorFactory);

        var criteria = new Criteria<TEntity>(preparer);

        return criteria;
    }
}
