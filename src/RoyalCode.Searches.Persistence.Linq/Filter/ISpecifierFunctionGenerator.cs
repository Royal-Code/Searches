
namespace RoyalCode.Searches.Persistence.Linq.Filter;

/// <summary>
/// <para>
///     Interface for generate functions to apply a filter in a query, used by <see cref="ISpecifier{TModel, TFilter}"/>.
/// </para>
/// <para>
///     This is used by internal components and should not be used directly from your code.
/// </para>
/// </summary>
public interface ISpecifierFunctionGenerator
{
    /// <summary>
    ///     Generate a function to apply a filter in a query.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TFilter">The type of the filter.</typeparam>
    /// <returns>The function or null if not exists.</returns>
    Func<IQueryable<TModel>, TFilter, IQueryable<TModel>>? Generate<TModel, TFilter>()
        where TModel : class
        where TFilter : class;
}