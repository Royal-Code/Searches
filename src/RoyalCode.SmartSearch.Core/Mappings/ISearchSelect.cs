using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Mappings;

/// <summary>
/// <para>
///     Defines a select mapping between an entity and a DTO for search projections.
/// </para>
/// <para>
///     Used to apply a select expression or use the default mapping for the given types.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type to select from.</typeparam>
/// <typeparam name="TDto"> The data transfer object (DTO) type to project to.</typeparam>
public interface ISearchSelect<TEntity, TDto>
    where TEntity : class
    where TDto : class
{
    /// <summary>
    /// <para>
    ///     Gets the expression used to project an entity of type <typeparamref name="TEntity"/>  into a data transfer
    ///     object (DTO) of type <typeparamref name="TDto"/>.
    /// </para>
    /// <para>
    ///     This expression is optional and can be used to customize the selection of properties from the entity
    ///     to the DTO. When not provided, the default mapping will be used if available.
    /// </para>
    /// </summary>
    Expression<Func<TEntity, TDto>>? SelectExpression { get; }
}
