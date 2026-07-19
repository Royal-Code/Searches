namespace RoyalCode.SmartSearch.Demo.Search;

/// <summary>
/// Operation hints for <c>Order</c>. Each value maps (in the search configuration) to an EF include, so that
/// entity-materializing terminals (<c>Collect</c>/<c>Single</c>/<c>FirstOrDefault</c>) load the requested graph.
/// Hints are NOT applied to <c>Select&lt;TDto&gt;()</c> projections nor to <c>Exists</c>.
/// </summary>
[Flags]
public enum OrderHints
{
    None = 0,
    WithCustomer = 1,
    WithItems = 2,
}
