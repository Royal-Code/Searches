using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace RoyalCode.Searches.Persistence.Linq.Sorter;

internal sealed class OrderByHandler<TModel, TProperty> : IOrderByHandler<TModel>
    where TModel : class
{
    private readonly Expression<Func<TModel, TProperty>> expression;

    public OrderByHandler(Expression<Func<TModel, TProperty>> expression)
    {
        this.expression = expression;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Handle(IOrderByBuilder<TModel> builder) => builder.Add(expression);
}

internal static class OrderByHandler
{
    public static IOrderByHandler<TModel> Create<TModel>(Expression expression)
        where TModel : class
    {
        var expressionType = expression.GetType();
        var expressionGenerics = expressionType.GetGenericArguments();
        if (expressionGenerics is null or { Length: not 1 })
            throw new InvalidOrderByExpressionException($"The expression type {expressionType} is not supported.", nameof(expression));

        var delegateType = expressionGenerics[0];
        var delegateGenerics = delegateType.GetGenericArguments();
        if (delegateGenerics is null or { Length: not 2 })
            throw new InvalidOrderByExpressionException($"The delegate type {delegateType} is not supported.", nameof(expression));

        var modelType = delegateGenerics[0];
        var propertyType = delegateGenerics[1];

        var handlerType = typeof(OrderByHandler<,>).MakeGenericType(modelType, propertyType);
        var handler = handlerType.GetConstructors().First().Invoke(new[] { expression });
        return (IOrderByHandler<TModel>)handler;
    }
}
