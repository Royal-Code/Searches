
namespace RoyalCode.Searches.Linq.Filter;

/// <summary>
/// <para>
///     Interface for generate a <see cref="ISpecifier{TModel, TFilter}"/>.
/// </para>
/// <para>
///     This is used by internal components and should not be used directly from your code.
/// </para>
/// </summary>
public interface ISpecifierGenerator
{
    /// <summary>
    ///     Generate a <see cref="ISpecifier{TModel, TFilter}"/> for the model and filter.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TFilter">The type of the filter.</typeparam>
    /// <returns>The specifier or null if not exists.</returns>
    ISpecifier<TModel, TFilter>? Generate<TModel, TFilter>()
        where TModel : class
        where TFilter : class;
}
