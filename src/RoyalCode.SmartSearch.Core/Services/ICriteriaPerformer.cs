using RoyalCode.SmartSearch.Defaults;

namespace RoyalCode.SmartSearch.Services;

/// <summary>
/// A service capable of accessing the database and running a query using the Criteria options.
/// </summary>
/// <typeparam name="TEntity">The entity type to query.</typeparam>
public interface ICriteriaPerformer<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Prepares the query with the criteria options for execution.
    /// </summary>
    /// <param name="options">The criteria options for performing the search.</param>
    /// <returns>A prepared query to perform.</returns>
    IPreparedQuery<TEntity> Prepare(CriteriaOptions options);
}
