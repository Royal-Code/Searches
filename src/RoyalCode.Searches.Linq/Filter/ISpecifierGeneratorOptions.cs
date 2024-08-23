using System.Linq.Expressions;

namespace RoyalCode.Searches.Linq.Filter;

/// <summary>
/// <para>
///     Options used by the default <see cref="ISpecifierFunctionGenerator"/> 
///     to generate the function for apply the filter into the model.
/// </para>
/// <para>
///     It is used for auto generated <see cref="ISpecifier{TModel, TFilter}"/>.
/// </para>
/// </summary>
/// <typeparam name="TModel"></typeparam>
/// <typeparam name="TFilter"></typeparam>
public interface ISpecifierGeneratorOptions<TModel, TFilter>
    where TModel : class
    where TFilter : class
{
    /// <summary>
    /// <para>
    ///     Determines the property of the filter to be configured.
    /// </para>
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="selector">A expression that select the property.</param>
    /// <returns>The options for the property.</returns>
    SpecifierGeneratorPropertyOptions<TModel, TFilter, TProperty> For<TProperty>(
        Expression<Func<TFilter, TProperty>> selector);

    /// <summary>
    /// <para>
    ///     Determines the property of the filter to be configured.
    /// </para>
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="selector">A expression that select the property.</param>
    /// <returns>The options for the property.</returns>
    SpecifierGeneratorPropertyOptions<TModel, TFilter, TProperty> For<TProperty>(
        Expression<Func<TFilter, TProperty?>> selector)
        where TProperty : struct;
}
