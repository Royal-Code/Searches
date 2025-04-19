
namespace RoyalCode.SmartSearch.Linq.Sorter;

/// <summary>
/// Exception thrown when the expression is not valid for the <see cref="OrderByHandler{TModel, TProperty}"/>.
/// </summary>
public sealed class InvalidOrderByExpressionException : ArgumentException
{
    /// <summary>
    /// Creates a new instance of <see cref="InvalidOrderByExpressionException"/>.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    public InvalidOrderByExpressionException(string? message, string? paramName)
        : base(message, paramName)
    { }
}