namespace RoyalCode.Searches.Linq.Selector;

/// <summary>
/// <para>
///     Default implementation of <see cref="ISelectorGenerator"/>.
/// </para>
/// </summary>
public sealed class DefaultSelectorGenerator : ISelectorGenerator
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// <para>
    ///     Create a new instance of <see cref="DefaultSelectorGenerator"/>.
    /// </para>
    /// </summary>
    /// <param name="serviceProvider">The service provider for locate the selectors.</param>
    public DefaultSelectorGenerator(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public ISelector<TEntity, TDto>? Generate<TEntity, TDto>()
        where TEntity : class
        where TDto : class
    {
        var selectorType = typeof(ISelector<,>).MakeGenericType(typeof(TEntity), typeof(TDto));
        var selector = serviceProvider.GetService(selectorType);
        return selector is null ? null : (ISelector<TEntity, TDto>?)selector;
    }
}
