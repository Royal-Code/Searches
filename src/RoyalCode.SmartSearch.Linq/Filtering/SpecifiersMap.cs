using System.Diagnostics.CodeAnalysis;
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

    private readonly Dictionary<(Type, Type), object> specifiers = new();

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
    public void Add((Type, Type) key, object value) => specifiers.Add(key, value);

    public void Add<TModel, TFilter>(ISpecifier<TModel, TFilter> specifier)
        where TModel : class
        where TFilter : class
    {
        var key = (typeof(TModel), typeof(TFilter));
        if (specifiers.ContainsKey(key))
            throw new ArgumentException($"Specifier for {key} already exists.");

        specifiers.Add(key, specifier);
    }

    public void Add<TModel, TFilter>(Func<IQueryable<TModel>, TFilter, IQueryable<TModel>> specifier)
        where TModel : class
        where TFilter : class
    {
        var key = (typeof(TModel), typeof(TFilter));
        if (specifiers.ContainsKey(key))
            throw new ArgumentException($"Specifier for {key} already exists.");

        specifiers.Add(key, new InternalSpecifier<TModel, TFilter>(specifier));
    }
}
