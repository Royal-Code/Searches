namespace RoyalCode.SmartSearch.Linq.Filtering;

/// <summary>
/// <para>
///     Default implementation of <see cref="ISpecifierGenerator"/>.
/// </para>
/// </summary>
internal sealed class DefaultSpecifierGenerator : ISpecifierGenerator
{
    private readonly IServiceProvider serviceProvider;

    public DefaultSpecifierGenerator(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public ISpecifier<TModel, TFilter>? Generate<TModel, TFilter>()
        where TModel : class
        where TFilter : class
    {
        var type = typeof(ISpecifier<,>).MakeGenericType(typeof(TModel), typeof(TFilter));
        return (ISpecifier<TModel, TFilter>?)serviceProvider.GetService(type);
    }
}