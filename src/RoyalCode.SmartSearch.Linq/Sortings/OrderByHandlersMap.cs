using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace RoyalCode.SmartSearch.Linq.Sortings;

internal sealed class OrderByHandlersMap
{
    public static OrderByHandlersMap Instance { get; } = new();

    // Cache de handlers de ordenacao por (tipo, propriedade). E um singleton de processo e o caminho de runtime
    // (OrderByProvider.GetHandler) escreve nele de forma concorrente; por isso usa ConcurrentDictionary. A geracao
    // do handler e deterministica, entao o caminho de runtime e idempotente (last-write-wins, sem lancar em duplicado).
    // O nome do order by e comparado ignorando case: "preco" e "Preco" resolvem para o mesmo handler,
    // tanto para registros manuais (AddOrderBy) quanto para o cache de runtime (evita entradas duplicadas).
    private readonly ConcurrentDictionary<(Type, string), object> handlers = new(KeyComparer.Instance);

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
    public void Add((Type modelType, string orderBy) key, object handler) => handlers[key] = handler;

    public void Add<TModel>(string orderBy, IOrderByHandler<TModel> handler)
        where TModel : class
    {
        var key = (typeof(TModel), orderBy);
        if (!handlers.TryAdd(key, handler))
            throw new ArgumentException($"Handler for {key} already exists.");
    }

    public void Add<TModel, TProperty>(string orderBy, Expression<Func<TModel, TProperty>> expression)
        where TModel : class
    {
        var key = (typeof(TModel), orderBy);
        if (!handlers.TryAdd(key, new OrderByHandler<TModel, TProperty>(expression)))
            throw new ArgumentException($"Handler for {key} already exists.");
    }

    private sealed class KeyComparer : IEqualityComparer<(Type, string)>
    {
        public static KeyComparer Instance { get; } = new();

        public bool Equals((Type, string) x, (Type, string) y)
            => x.Item1 == y.Item1 && StringComparer.OrdinalIgnoreCase.Equals(x.Item2, y.Item2);

        public int GetHashCode((Type, string) obj)
            => HashCode.Combine(obj.Item1, StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item2));
    }
}
