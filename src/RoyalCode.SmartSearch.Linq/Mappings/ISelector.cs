using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Mappings;

/// <summary>
/// <para>
///     Component interface to select one object from other, used in the <c>Select</c> method of a linq query.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type, the object to select values.</typeparam>
/// <typeparam name="TDto">The DTO type, the object to create and set values.</typeparam>
public interface ISelector<TEntity, TDto>
    where TEntity : class
    where TDto : class
{
    /// <summary>
    /// <para>
    ///     Get a lambda function expression to select one object from other,
    ///     usually applicable to select a DTO from an entity.
    /// </para>
    /// <para>
    ///     This expression will be used in the <c>Select</c> method of a linq query.
    /// </para>
    /// </summary>
    /// <returns>An expression to select a data model from another object type.</returns>
    Expression<Func<TEntity, TDto>> GetSelectExpression();
}