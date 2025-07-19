using RoyalCode.SmartSearch.Linq.Services;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.SmartSearch.Linq.Mappings;

/// <summary>
/// Default implementation of <see cref="ISelectorFactory"/>.
/// </summary>
internal sealed class SelectorFactory : ISelectorFactory
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
        if (selectors.TryGet<TEntity, TDto>(out var selector))
            return selector;

        selector = selectorGenerator?.Generate<TEntity, TDto>();
        if (selector is null)
        {
            if (TryFindByStaticProperty(out selector))
            {
                selectors.Add((typeof(TEntity), typeof(TDto)), selector);
                return selector;
            }

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

    private static bool TryFindByStaticProperty<TEntity, TDto>([NotNullWhen(true)] out ISelector<TEntity, TDto>? selector)
        where TEntity : class
        where TDto : class
    {
        selector = null;
        // Lookup for static properties in TDto type where the type is Expression<Func<TEntity, TDto>>
        var properties = typeof(TDto).GetProperties(BindingFlags.Public | BindingFlags.Static);
        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(Expression<Func<TEntity, TDto>>))
            {
                if (property.GetValue(null) is Expression<Func<TEntity, TDto>> value)
                {
                    selector = new InternalSelector<TEntity, TDto>(value);
                    return true;
                }
            }
        }
        return false;
    }
}