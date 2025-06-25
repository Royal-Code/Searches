using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace RoyalCode.SmartSearch.Linq.Sortings;

internal sealed class OrderByHandlersMap
{
    public static OrderByHandlersMap Instance { get; } = new();

    private readonly Dictionary<(Type, string), object> handlers = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<TModel>(string orderBy, [NotNullWhen(true)] out IOrderByHandler<TModel>? handler)
        where TModel : class
    {
        var key = (typeof(TModel), orderBy);
        if (handlers.TryGetValue(key, out var value))
        {
            handler = (IOrderByHandler<TModel>)value;
            return true;
        }
        handler = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add((Type modelType, string orderBy) key, object handler) => handlers.Add(key, handler);

    public void Add<TModel>(string orderBy, IOrderByHandler<TModel> handler)
        where TModel : class
    {
        var key = (typeof(TModel), orderBy);
        if (handlers.ContainsKey(key))
            throw new ArgumentException($"Handler for {key} already exists.");

        handlers.Add(key, handler);
    }

    public void Add<TModel, TProperty>(string orderBy, Expression<Func<TModel, TProperty>> expression)
        where TModel : class
    {
        var key = (typeof(TModel), orderBy);
        if (handlers.ContainsKey(key))
            throw new ArgumentException($"Handler for {key} already exists.");

        handlers.Add(key, new OrderByHandler<TModel, TProperty>(expression));
    }
}
