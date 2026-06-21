using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering;

/// <summary>
/// Defines a custom expression generator used by SmartSearch to build a filter expression for a filter property.
/// </summary>
/// <remarks>
/// Implement this interface when a filter property cannot be translated by the default criterion pipeline.
/// The type is selected with <see cref="FilterExpressionGeneratorAttribute{TExpressionGenerator}"/>.
/// </remarks>
public interface ISpecifierExpressionGenerator
{
    /// <summary>
    /// Creates the expression that applies the filter to the current query.
    /// </summary>
    /// <param name="context">The context containing query, filter, model, and member access expressions.</param>
    /// <returns>
    /// An expression that applies the desired filter operation, usually by assigning a new
    /// <see cref="IQueryable{T}"/> expression back to <see cref="ExpressionGeneratorContext.Query"/>.
    /// </returns>
    public static abstract Expression GenerateExpression(ExpressionGeneratorContext context);
}

/// <summary>
/// Provides the expressions required by a custom SmartSearch filter expression generator.
/// </summary>
public class ExpressionGeneratorContext
{
    /// <summary>
    /// Gets the query parameter expression that stores the current <see cref="IQueryable{T}"/>.
    /// </summary>
    public required ParameterExpression Query { get; init; }

    /// <summary>
    /// Gets the filter instance parameter expression.
    /// </summary>
    public required ParameterExpression Filter { get; init; }

    /// <summary>
    /// Gets the model/entity parameter expression used by the generated predicate.
    /// </summary>
    public required ParameterExpression Model { get; init; }

    /// <summary>
    /// Gets the member access expression for the target model property.
    /// </summary>
    public required MemberExpression ModelMember { get; init; }

    /// <summary>
    /// Gets the member access expression for the source filter property.
    /// </summary>
    public required MemberExpression FilterMember { get; init; }

}
