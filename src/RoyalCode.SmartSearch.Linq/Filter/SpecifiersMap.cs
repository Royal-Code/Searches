using System.Runtime.CompilerServices;

namespace RoyalCode.SmartSearch.Linq.Filter;

/// <summary>
/// <para>
///     A class that maps the types of the model and filter to the specifier.
/// </para>
/// </summary>
internal sealed class SpecifiersMap
{
    public static SpecifiersMap Instance { get; } = new();

    private readonly Dictionary<(Type, Type), object> specifiers = new();

    public object this[(Type, Type) key] => specifiers[key];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey((Type, Type) key) => specifiers.ContainsKey(key);

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
