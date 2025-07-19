namespace RoyalCode.SmartSearch.Linq.Sortings;

/// <summary>
/// Exception thrown when the order by is not supported for the type.
/// </summary>
public sealed class OrderByNotSupportedException : ArgumentException
{
    /// <summary>
    /// Creates a new instance of <see cref="OrderByNotSupportedException"/>.
    /// </summary>
    /// <param name="orderBy">The order by parameter that is not supported.</param>
    /// <param name="typeName">The type name that not supports the order by.</param>
    public OrderByNotSupportedException(string orderBy, string typeName)
        : base(string.Format("The order by '{0}' is not supported for the type '{1}'.", orderBy, typeName), orderBy)
    { }
}