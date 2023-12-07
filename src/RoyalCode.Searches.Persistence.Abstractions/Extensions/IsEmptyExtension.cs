using System.Collections;
using System.Globalization;

#pragma warning disable S6580 // Use a format provider when parsing date and time

namespace RoyalCode.Searches.Persistence.Abstractions.Extensions;

/// <summary>
/// Class to check if some value is empty.
/// If it's null, it's empty.
/// If it's numeric and it's zero, it's empty.
/// If it is string and has no characters or only blanks, it will be empty.
/// If it is an enumerable and has no items, it is empty.
/// If it doesn't meet one of the above rules, it's not empty.
/// </summary>
public static class IsEmptyExtension
{
    /// <summary>
    /// Check if some value is empty.
    /// </summary>
    /// <param name="expression">The value to be checked.</param>
    /// <returns>True if the value is empty, false otherwise.</returns>
    public static bool IsEmpty(this object expression)
    {
        if (expression == null)
            return true;

        if (expression is bool b)
            return b;

        if (expression is DateTime dt)
            return dt.IsBlank();

        if (expression is DateTime?)
            return ((DateTime?)expression).IsBlank();

        if (expression is Guid guid)
            return guid == Guid.Empty;

        bool isNumber = double.TryParse(
            Convert.ToString(expression, CultureInfo.InvariantCulture),
            NumberStyles.Any,
            NumberFormatInfo.InvariantInfo,
            out double number);
        if (isNumber)
            return number == 0;

        bool isDate = DateTime.TryParse(
            Convert.ToString(expression, CultureInfo.InvariantCulture),
            out var date);

        if (isDate)
            return date.IsBlank();

        if (expression is string str)
            return string.IsNullOrWhiteSpace(str);

        if (expression is IEnumerable enumerable)
            return VerifyCollection(enumerable);

        return false;
    }

    /// <summary>
    /// Checks whether an enumerable is empty.
    /// </summary>
    /// <param name="source">IEnumerable</param>
    /// <returns>True if it is empty, false otherwise.</returns>
    public static bool VerifyCollection(IEnumerable source)
    {
        return source == null || !source.GetEnumerator().MoveNext();
    }

    /// <summary>
    /// Checks if a date is blank "01/01/0001" or "01/01/1753" or 01/01/1970.
    /// </summary>
    /// <param name="date">Date to be checked.</param>
    /// <returns>True if it is blank, false otherwise.</returns>
    public static bool IsBlank(this DateTime date)
    {
        return date == SQLDateNull
            || date == CSDateNull
            || date == UnixDateNull;
    }

    /// <summary>
    /// Checks if a nullable date is blank "01/01/0001" or "01/01/1753" or null.
    /// </summary>
    /// <param name="date">Nullable date to be checked.</param>
    /// <returns>True if it is blank, false otherwise.</returns>
    public static bool IsBlank(this DateTime? date)
    {
        // if it has a value, checks if the value is blank, otherwise returns that it is blank (true).
        return !date.HasValue || date.Value.IsBlank();
    }

    /// <summary>
    /// Start data for SQL Server.
    /// </summary>
    public static readonly DateTime SQLDateNull = new(1753, 01, 01);

    /// <summary>
    /// Start date of C#, DotNet.
    /// </summary>
    public static readonly DateTime CSDateNull = new(0001, 01, 01);

    /// <summary>
    /// Start date of Unix
    /// </summary>
    public static readonly DateTime UnixDateNull = new(1970, 01, 01);
}