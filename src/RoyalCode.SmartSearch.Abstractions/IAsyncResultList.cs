namespace RoyalCode.SmartSearch;

/// <summary>
/// Component interface for async listing the result of a search.
/// </summary>
/// <typeparam name="TModel">Type of data listed by the result.</typeparam>
public interface IAsyncResultList<out TModel> : IResultList
{
    /// <summary>
    /// Async collection of the searched models.
    /// </summary>
    IAsyncEnumerable<TModel> Items { get; }
}