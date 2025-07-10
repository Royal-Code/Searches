namespace RoyalCode.SmartSearch.Exceptions;

/// <summary>
/// Exception thrown when an error occurs related to the sorting of results in a query.
/// </summary>
/// <param name="message">Message about the sorting property with problems.</param>
/// <param name="propertyName">The name of the property that caused the sorting error.</param>
/// <param name="typeName">The type of data to be filtered and sorted.</param>
/// <param name="inner">Internal exception that caused the problem.</param>
public sealed class OrderByException(string message, string propertyName, string typeName, Exception inner) 
    : Exception(message, inner) 
{
    /// <summary>
    /// The name of the property that caused the sorting error.
    /// </summary>
    public string PropertyName { get; } = propertyName;

    /// <summary>
    /// The type of data to be filtered and sorted.
    /// </summary>
    public string TypeName { get; } = typeName;
}
