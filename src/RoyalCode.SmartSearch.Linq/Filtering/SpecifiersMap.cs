using System.Diagnostics.CodeAnalysis;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace RoyalCode.SmartSearch.Linq.Filtering;

/// <summary>
/// <para>
///     A class that maps the types of the model and filter to the specifier.
/// </para>
/// </summary>
internal sealed class SpecifiersMap
{
    public static SpecifiersMap Instance { get; } = new();

    private readonly ConcurrentDictionary<(Type, Type), object> specifiers = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<TModel, TFilter>([NotNullWhen(true)] out ISpecifier<TModel, TFilter>? specifier)
        where TModel : class
        where TFilter : class
    {
        var key = (typeof(TModel), typeof(TFilter));
        if (specifiers.TryGetValue(key, out var value))
        {
            specifier = (ISpecifier<TModel, TFilter>)value;
            return true;
        }
        specifier = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add((Type, Type) key, object value) => specifiers.TryAdd(key, value);

    public void Add<TModel, TFilter>(ISpecifier<TModel, TFilter> specifier)
        where TModel : class
        where TFilter : class
    {
        var key = (typeof(TModel), typeof(TFilter));
        if (!specifiers.TryAdd(key, specifier))
            throw new ArgumentException($"Specifier for {key} already exists.");
    }

    public void Add<TModel, TFilter>(Func<IQueryable<TModel>, TFilter, IQueryable<TModel>> specifier)
        where TModel : class
        where TFilter : class
    {
        var key = (typeof(TModel), typeof(TFilter));
        if (!specifiers.TryAdd(key, new InternalSpecifier<TModel, TFilter>(specifier)))
            throw new ArgumentException($"Specifier for {key} already exists.");
    }
}
