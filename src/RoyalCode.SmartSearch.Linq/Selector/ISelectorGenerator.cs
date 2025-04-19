
namespace RoyalCode.SmartSearch.Linq.Selector;

/// <summary>
/// <para>
///     A generator of selectors.
/// </para>
/// </summary>
public interface ISelectorGenerator
{
    /// <summary>
    /// <para>
    ///     Generate a selector for the specified models types.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The type of the model.</typeparam>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <returns>The selector.</returns>
    ISelector<TEntity, TDto>? Generate<TEntity, TDto>()
        where TEntity : class
        where TDto : class;
}
