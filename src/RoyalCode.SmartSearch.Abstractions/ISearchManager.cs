namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     A service for persistence components that allow search operations to be carried out.
/// </para>
/// <para>
///     The search component is used to create a search for a specific entity type.
/// </para>
/// </summary>
public interface ISearchManager
{
    /// <summary>
    /// <para>
    ///     Creates a new criteria for the entity.
    /// </para>
    /// <para>
    ///     With the criteria, it is possible to apply filters, sorting, projections,
    ///     and pagination, or collect entities directly from the persistence unit.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A new instance of <see cref="ICriteria{TEntity}"/>.</returns>
    ICriteria<TEntity> Criteria<TEntity>() where TEntity : class;
}

