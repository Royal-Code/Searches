// Ignore Spelling: Searchable

namespace RoyalCode.Searches.Abstractions;

/// <summary>
/// <para>
///     Represents a search component for a persistence unit.
/// </para>
/// <para>
///     The search component is used to create a search for a specific entity type.
/// </para>
/// </summary>
public interface ISearchable
{
    /// <summary>
    /// <para>
    ///     Creates a new search for the entity.
    /// </para>
    /// <para>
    ///     There must be a search component for the persistence unit, otherwise an exception will be thrown.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>A new instance of <see cref="ISearch{TEntity}"/>.</returns>
    /// <exception cref="InvalidOperationException">
    ///     If entity is not part of the persistence unit or there is no search component for it.
    /// </exception>
    ISearch<TEntity> CreateSearch<TEntity>() where TEntity : class;

    /// <summary>
    /// <para>
    ///     Gets all entities of the persistence unit, where it is possible to add filters and sorters.
    /// </para>
    /// <para>
    ///     The purpose of <see cref="IAllEntities{TEntity}"/> is to query entities for update or delete.
    /// </para>
    /// <para>
    ///     When used with a unit of work, all changes made to the entities must be saved.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>
    ///     A new instance of <see cref="IAllEntities{TEntity}"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     If entity is not part of the persistence unit or there is no search component for it.
    /// </exception>
    IAllEntities<TEntity> All<TEntity>() where TEntity : class;
}

