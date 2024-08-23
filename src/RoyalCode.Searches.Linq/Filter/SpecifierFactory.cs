
namespace RoyalCode.Searches.Linq.Filter;

/// <summary>
/// <para>
///     Default implementation of <see cref="ISpecifierFactory"/>.
/// </para>
/// <para>
///     This is a internal singleton service.
/// </para>
/// </summary>
internal sealed class SpecifierFactory : ISpecifierFactory
{
    private readonly SpecifiersMap specifiers;
    private readonly ISpecifierGenerator? specifierGenerator;
    private readonly ISpecifierFunctionGenerator? functionGenerator;

    public SpecifierFactory(SpecifiersMap specifiers,
        ISpecifierGenerator? specifierGenerator = null,
        ISpecifierFunctionGenerator? functionGenerator = null)
    {
        this.specifiers = specifiers;
        this.specifierGenerator = specifierGenerator;
        this.functionGenerator = functionGenerator;
    }

    public ISpecifier<TModel, TFilter>? GetSpecifier<TModel, TFilter>()
        where TModel : class
        where TFilter : class
    {
        var key = (typeof(TModel), typeof(TFilter));
        if (specifiers.ContainsKey(key))
            return (ISpecifier<TModel, TFilter>)specifiers[key];

        var specifier = specifierGenerator?.Generate<TModel, TFilter>();

        if (specifier is null)
        {
            var function = functionGenerator?.Generate<TModel, TFilter>();
            if (function is not null)
                specifier = new InternalSpecifier<TModel, TFilter>(function);
        }

        if (specifier is not null)
        {
            specifiers.Add(key, specifier);
            return specifier;
        }

        throw new InvalidOperationException("No specifier configured for the model and filter.");
    }
}
