using System.ComponentModel;

namespace RoyalCode.SmartSearch.Abstractions;

/// <summary>
/// Component that contains information about the sorting applied to a search/query.
/// </summary>
public interface ISorting
{
    /// <summary>
    /// One or more property names on which the sorting should be done.
    /// </summary>
    string OrderBy { get; }

    /// <summary>
    /// <para>
    ///     Specifies the direction of a sort operation.
    /// </para>
    /// <para>
    ///     Ascending = 0, Descending = 1.
    /// </para>
    /// </summary>
    ListSortDirection Direction { get; }
}
