
namespace RoyalCode.SmartSearch.Linq.Selector;

/// <summary>
/// A factory to create <see cref="ISelector{TEntity, TDto}"/>
/// </summary>
public interface ISelectorFactory
{
    /// <summary>
    /// Create a selector for the entity type <typeparamref name="TEntity"/> and the DTO type <typeparamref name="TDto"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TDto">The DTO type.</typeparam>
    /// <returns>The selector.</returns>
    ISelector<TEntity, TDto> Create<TEntity, TDto>()
        where TEntity : class
        where TDto : class;
}
