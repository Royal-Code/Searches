namespace RoyalCode.Searches.Persistence.Linq.Sorter;

/// <summary>
/// <para>
///     Manages and provides the query ordering handlers of a given data model.
/// </para>
/// </summary>
public interface IOrderByProvider

{
    /// <summary>
    /// <para>
    ///     It tries to get an ordering handler configured for the data model,
    ///     according to a sort name that identifies how the query should be sorted.
    /// </para>
    /// <para>
    ///     May be returned null if no handler is configured for the order by identification, or throw an exception.
    /// </para>
    /// </summary>
    /// <typeparam name="TModel">The query source model type.</typeparam>
    /// <param name="orderBy">A sort name that identifies how the query should be sorted.</param>
    /// <returns>An ordering handler or null if not exists.</returns>
    /// <exception cref="Exception">Optional, if no handler is configured for the order by identification.</exception>
    IOrderByHandler<TModel>? GetHandler<TModel>(string orderBy) where TModel : class;

    /// <summary>
    /// <para>
    ///     It tries to get the default ordering handler configured for the data model.
    /// </para>
    /// <para>
    ///     The common property used as default is the "Id" property.
    /// </para>
    /// <para>
    ///     May be returned null if no default handler is configured for the data model, or throw an exception.
    /// </para>
    /// </summary>
    /// <typeparam name="TModel">The query source model type.</typeparam>
    /// <returns>The default ordering handler or null if not exists.</returns>
    /// <exception cref="Exception">Optional, if no handler is configured for the order by identification.</exception>
    IOrderByHandler<TModel> GetDefaultHandler<TModel>() where TModel : class;
}