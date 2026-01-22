using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering;

public interface ISpecifierExpressionGenerator
{

    public Expression GenerateExpression<TModel>(ExpressionGeneratorContext<TModel> context);
}

public class ExpressionGeneratorContext<TModel>
{
    public ParameterExpression Query { get; init; }

    public ParameterExpression Filter { get; init; }

    public ParameterExpression Model { get; init; }

    public MemberExpression ModelMember { get; init; }

    public MemberExpression FilterMember { get; init; }

}
