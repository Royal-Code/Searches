using System.Linq.Expressions;

namespace RoyalCode.Searches.Persistence.Linq.Selector;

/// <summary>
/// <para>
///     A generator of expressions for the selection of the model (DTO) from other model (Entity).
/// </para>
/// </summary>
public interface ISelectorExpressionGenerator
{
    /// <summary>
    /// <para>
    ///     Generate an expression for the selection of the model (DTO) from other model (Entity).
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <returns>The expression for the selection of the model (DTO) from other model (Entity).</returns>
    Expression<Func<TEntity, TDto>>? Generate<TEntity, TDto>()
        where TEntity : class
        where TDto : class;
}