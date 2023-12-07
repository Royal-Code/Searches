
namespace RoyalCode.Searches.Persistence.Linq.Filter;

/// <summary>
/// A factory to create <see cref="ISpecifier{TModel,TFilter}"/> for a given model type and filter type.
/// </summary>
public interface ISpecifierFactory
{
    /// <summary>
    /// <para>
    ///     Creates a new specifier for a given model type and filter type.
    /// </para>
    /// <para>
    ///     May be returned null if no specifier is configured for the model and filter, or throw an exception.
    /// </para>
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TFilter">The filter type.</typeparam>
    /// <returns>A new specifier or null if not exists.</returns>
    /// <exception cref="Exception">Optional, if no specifier is configured for the model and filter.</exception>
    ISpecifier<TModel, TFilter>? GetSpecifier<TModel, TFilter>()
        where TModel : class
        where TFilter : class;
}