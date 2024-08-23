using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace RoyalCode.Searches.Linq.Sorter;

internal sealed class OrderByHandlersMap
{
    public static OrderByHandlersMap Instance { get; } = new();

    private readonly Dictionary<(Type, string), object> handlers = [];

    public object this[(Type, string) key] => handlers[key];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains((Type, string) key) => handlers.ContainsKey(key);

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
