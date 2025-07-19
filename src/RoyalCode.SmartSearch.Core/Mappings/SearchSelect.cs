using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Mappings;

/// <summary>
/// <para>
///     Represents a select mapping between an entity and a DTO for search projections.
/// </para>
/// <para>
///     Implements <see cref="ISearchSelect{TEntity, TDto}"/> to provide a way to apply a select expression
///     or use the default mapping for the given types.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type to select from.</typeparam>
/// <typeparam name="TDto">The DTO type to project to.</typeparam>
public sealed class SearchSelect<TEntity, TDto> : ISearchSelect<TEntity, TDto>
    where TEntity : class
    where TDto : class
{
    /// <summary>
    /// The select expression to project from <typeparamref name="TEntity"/> to <typeparamref name="TDto"/>.
    /// </summary>
    private readonly Expression<Func<TEntity, TDto>>? selectExpression;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchSelect{TEntity, TDto}"/> class with a select expression.
    /// </summary>
    /// <param name="selectExpression">The expression to select from the entity to the DTO.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="selectExpression"/> is null.</exception>
    public SearchSelect(Expression<Func<TEntity, TDto>> selectExpression)
    {
        ArgumentNullException.ThrowIfNull(selectExpression);
        this.selectExpression = selectExpression;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchSelect{TEntity, TDto}"/> class using the default mapping.
    /// </summary>
    public SearchSelect() { }

    /// <inheritdoc />
    public Expression<Func<TEntity, TDto>>? SelectExpression => selectExpression;
}
