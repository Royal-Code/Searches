using RoyalCode.SmartSearch.Linq.Filtering;

namespace RoyalCode.SmartSearch.Linq.Services;

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
    ///     Will throw an exception if no specifier is configured for the model and filter.
    /// </para>
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TFilter">The filter type.</typeparam>
    /// <returns>A new specifier.</returns>
    /// <exception cref="Exception">Thrown if no specifier is configured for the model and filter.</exception>
    ISpecifier<TModel, TFilter> GetSpecifier<TModel, TFilter>()
        where TModel : class
        where TFilter : class;
}