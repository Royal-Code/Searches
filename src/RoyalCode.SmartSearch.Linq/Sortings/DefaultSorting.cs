using System.ComponentModel;

namespace RoyalCode.SmartSearch.Linq.Sortings;

/// <summary>
/// Represents the default sorting configuration, typically by the "Id" property in ascending order.
/// </summary>
public sealed class DefaultSorting : ISorting
{
    /// <summary>
    /// The default property name used for sorting.
    /// </summary>
    public const string DefaultOrderBy = "Id";

    /// <summary>
    /// Singleton instance of <see cref="DefaultSorting"/>.
    /// </summary>
    public static readonly DefaultSorting Instance = new();

    /// <inheritdoc />
    public string OrderBy => DefaultOrderBy;

    /// <inheritdoc />
    public ListSortDirection Direction => ListSortDirection.Ascending;
}