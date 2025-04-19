namespace RoyalCode.SmartSearch.Core;

/// <summary>
/// <para>
///     A handler for applying filters to queries.
/// </para>
/// <para>
///     This component is used by the <see cref="SearchFilter"/>,
///     which stores the filter that will be used in the query specification.
/// </para>
/// </summary>
public interface ISpecifierHandler
{
    /// <summary>
    /// Receives the filter object that will be used to specify the query.
    /// </summary>
    /// <param name="filter">The filter object.</param>
    /// <typeparam name="TFilter">The filter type.</typeparam>
    void Handle<TFilter>(TFilter filter) where TFilter : class;
}