using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.Searches.Persistence.Linq.Filter;

internal class InternalSpecifierGeneratorOptions<TModel, TFilter> : ISpecifierGeneratorOptions<TModel, TFilter>
    where TModel : class
    where TFilter : class
{
    private readonly List<SpecifierGeneratorPropertyOptions<TModel, TFilter>> propertyOptions = [];

    public SpecifierGeneratorPropertyOptions<TModel, TFilter, TProperty> For<TProperty>(
        Expression<Func<TFilter, TProperty>> selector)
    {
        // get selected property
        PropertyInfo property = (selector.Body as MemberExpression)?.Member as PropertyInfo
            ?? throw new ArgumentException("The selector must be a property selector.", nameof(selector));

        // check if exists the options in propertyOptions
        var previous = propertyOptions.Find(p => p.PropertyInfo == property);
        if (previous is not null)
            return (SpecifierGeneratorPropertyOptions<TModel, TFilter, TProperty>)previous;

        var newOptions = new SpecifierGeneratorPropertyOptions<TModel, TFilter, TProperty>(property);
        propertyOptions.Add(newOptions);
        return newOptions;
    }

    public SpecifierGeneratorPropertyOptions<TModel, TFilter, TProperty> For<TProperty>(
        Expression<Func<TFilter, TProperty?>> selector)
        where TProperty : struct
    {
        // get selected property
        PropertyInfo property = (selector.Body as MemberExpression)?.Member as PropertyInfo
            ?? throw new ArgumentException("The selector must be a property selector.", nameof(selector));

        // check if exists the options in propertyOptions
        var previous = propertyOptions.Find(p => p.PropertyInfo == property);
        if (previous is not null)
            return (SpecifierGeneratorPropertyOptions<TModel, TFilter, TProperty>)previous;

        var newOptions = new SpecifierGeneratorPropertyOptions<TModel, TFilter, TProperty>(property);
        propertyOptions.Add(newOptions);
        return newOptions;
    }

    public bool TryGetPropertyOptions(PropertyInfo filterProperty,
        [NotNullWhen(true)] out SpecifierGeneratorPropertyOptions<TModel, TFilter>? options)
    {
        options = propertyOptions.Find(p => p.PropertyInfo == filterProperty);
        return options is not null;
    }
}
