using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Mappings;

/// <summary>
/// <para>
///     Defines a component capable of configuring a select projection for an entity type.
/// </para>
/// <para>
///     Used to specify how an entity should be projected to a DTO in search operations.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type to select from.</typeparam>
public interface ISearchSelector<TEntity>
    where TEntity : class
{
    /// <summary>
    /// <para>
    ///     Configures the selector to use the default mapping from <typeparamref name="TEntity"/> to <typeparamref name="TDto"/>.
    /// </para>
    /// <para>
    ///     The mapping will be resolved by the search engine or selector factory.
    /// </para>
    /// </summary>
    /// <typeparam name="TDto">The DTO type to project to.</typeparam>
    void Select<TDto>()
        where TDto : class;

    /// <summary>
    /// <para>
    ///     Configures the selector to use a custom select expression from <typeparamref name="TEntity"/> to <typeparamref name="TDto"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TDto">The DTO type to project to.</typeparam>
    /// <param name="selectExpression">The expression to select from the entity to the DTO.</param>
    void Select<TDto>(Expression<Func<TEntity, TDto>> selectExpression)
        where TDto : class;
}
