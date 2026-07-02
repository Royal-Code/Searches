using RoyalCode.SmartSearch.Exceptions;

namespace RoyalCode.SmartSearch.Linq.Sortings;

/// <summary>
/// Exception thrown when the order by is not supported for the type.
/// </summary>
/// <remarks>
/// Derives from <see cref="OrderByException"/> so that HTTP layers that already handle sorting errors
/// (e.g. the AspNetCore <c>Performer</c>) translate an unsupported order-by into a client error
/// (<c>400 InvalidParameter</c>) instead of leaking it as an internal error (<c>500</c>).
/// </remarks>
public sealed class OrderByNotSupportedException : OrderByException
{
    /// <summary>
    /// Creates a new instance of <see cref="OrderByNotSupportedException"/>.
    /// </summary>
    /// <param name="orderBy">The order by parameter that is not supported.</param>
    /// <param name="typeName">The type name that not supports the order by.</param>
    public OrderByNotSupportedException(string orderBy, string typeName)
        : base(string.Format("The order by '{0}' is not supported for the type '{1}'.", orderBy, typeName), orderBy, typeName)
    { }
}