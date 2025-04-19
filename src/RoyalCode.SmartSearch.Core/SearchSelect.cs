using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Core;

/// <summary>
/// Information about the select to be applied to the query.
/// </summary>
public sealed class SearchSelect
{
    /// <summary>
    /// Creates a new instance of <see cref="SearchSelect"/>.
    /// </summary>
    /// <param name="selectExpression"></param>
    public SearchSelect(Expression selectExpression)
    {
        SelectExpression = selectExpression;
    }

    /// <summary>
    /// The expression to be used in the select operation.
    /// </summary>
    public Expression SelectExpression { get; }
}