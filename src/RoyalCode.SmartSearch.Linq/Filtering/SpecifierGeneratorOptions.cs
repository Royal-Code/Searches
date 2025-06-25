using System.Diagnostics.CodeAnalysis;

namespace RoyalCode.SmartSearch.Linq.Filtering;

internal static class SpecifierGeneratorOptions
{
    private static Dictionary<(Type, Type), object>? options;

    internal static ISpecifierGeneratorOptions<TModel, TFilter> GetOptions<TModel, TFilter>()
        where TModel : class
        where TFilter : class
    {
        options ??= [];

        if (options.TryGetValue((typeof(TModel), typeof(TFilter)), out var value))
            return (ISpecifierGeneratorOptions<TModel, TFilter>)value;

        value = new InternalSpecifierGeneratorOptions<TModel, TFilter>();
        options.Add((typeof(TModel), typeof(TFilter)), value);
        return (ISpecifierGeneratorOptions<TModel, TFilter>)value;
    }

    internal static bool TryGetOptions<TModel, TFilter>(
        [NotNullWhen(true)] out InternalSpecifierGeneratorOptions<TModel, TFilter>? specifierOptions)
        where TModel : class
        where TFilter : class
    {
        if (options is not null && options.TryGetValue((typeof(TModel), typeof(TFilter)), out var value))
        {
            specifierOptions = (InternalSpecifierGeneratorOptions<TModel, TFilter>)value;
            return true;
        }

        specifierOptions = null;
        return false;
    }
}
