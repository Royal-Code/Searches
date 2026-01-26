using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering;

public interface ISpecifierExpressionGenerator
{
    public static abstract Expression GenerateExpression(ExpressionGeneratorContext context);
}

public class ExpressionGeneratorContext
{
    public required ParameterExpression Query { get; init; }

    public required ParameterExpression Filter { get; init; }

    public required ParameterExpression Model { get; init; }
    public required MemberExpression ModelMember { get; init; }

    public required MemberExpression FilterMember { get; init; }

}
