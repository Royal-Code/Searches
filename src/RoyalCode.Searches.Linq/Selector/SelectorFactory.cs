
namespace RoyalCode.Searches.Linq.Selector;

/// <summary>
/// Default implementation of <see cref="ISelectorFactory"/>.
/// </summary>
internal class SelectorFactory : ISelectorFactory
{
    private readonly SelectorsMap selectors;
    private readonly ISelectorGenerator? selectorGenerator;
    private readonly ISelectorExpressionGenerator? expressionGenerator;

    public SelectorFactory(
        SelectorsMap selectors,
        ISelectorGenerator? selectorGenerator = null,
        ISelectorExpressionGenerator? expressionGenerator = null)
    {
        this.selectors = selectors;
        this.selectorGenerator = selectorGenerator;
        this.expressionGenerator = expressionGenerator;
    }

    /// <inheritdoc />
    public ISelector<TEntity, TDto> Create<TEntity, TDto>()
        where TEntity : class
        where TDto : class
    {
        if (selectors.ContainsKey((typeof(TEntity), typeof(TDto))))
            return (ISelector<TEntity, TDto>)selectors[(typeof(TEntity), typeof(TDto))];

        var selector = selectorGenerator?.Generate<TEntity, TDto>();
        if (selector is null)
        {
            var expression = expressionGenerator?.Generate<TEntity, TDto>();
            if (expression is not null)
                selector = new InternalSelector<TEntity, TDto>(expression);
        }

        if (selector is not null)
        {
            selectors.Add((typeof(TEntity), typeof(TDto)), selector);
            return selector;
        }

        throw new SelectorNotFoundException($"No selector found for {typeof(TEntity)} to {typeof(TDto)}.");
    }
}