using RoyalCode.Extensions.PropertySelection;
using RoyalCode.SmartSearch.Linq.Filtering.Resolutions;
using System.Reflection;
using System.Reflection.Emit;

namespace RoyalCode.SmartSearch.Linq.Filtering;

internal static class CriterionResolutions
{
    public static IReadOnlyList<ICriterionResolution> CreateResolutions<TModel, TFilter>(
        PropertySelection? previousFilterProperty = null, FilterTarget? filterTarget = null)
        where TModel : class
        where TFilter : class
    {
        filterTarget ??= new(typeof(TModel));

        List<ICriterionResolution> resolutions = [];
        var available = BuildAvailableFilterProperties(typeof(TFilter), previousFilterProperty);

        ApplyCustomPredicateFactories<TModel, TFilter>(available, filterTarget, resolutions);
        BuildDisjunctionsFromAttributes(available, filterTarget, resolutions);
        BuildDisjunctionsFromNameOrTargetPath(available, filterTarget, resolutions);
        BuildComplexFilterResolutions(available, filterTarget, resolutions);
        BuildFilterExpressionGenerator(available, filterTarget, resolutions);
        BuildDefaultOperatorResolutions(available, filterTarget, resolutions);

        return resolutions;
    }

    public static IReadOnlyList<ICriterionResolution> CreateResolutions(
        PropertySelection previousFilterProperty, FilterTarget filterTarget)
    {
        List<ICriterionResolution> resolutions = [];
        var available = BuildAvailableFilterProperties(previousFilterProperty.Info.PropertyType, previousFilterProperty);

        BuildDisjunctionsFromAttributes(available, filterTarget, resolutions);
        BuildDisjunctionsFromNameOrTargetPath(available, filterTarget, resolutions);
        BuildComplexFilterResolutions(available, filterTarget, resolutions);
        BuildFilterExpressionGenerator(available, filterTarget, resolutions);
        BuildDefaultOperatorResolutions(available, filterTarget, resolutions);

        return resolutions;
    }

    private static List<AvailableFilterProperty> BuildAvailableFilterProperties(Type filterType, PropertySelection? previousFilterProperty)
    {
        var available = filterType.GetProperties()
            .Select(p => new AvailableFilterProperty
            {
                FilterProperty = previousFilterProperty?.SelectChild(p) ?? new PropertySelection(p),
                Criterion = p.GetCustomAttribute<CriterionAttribute>(true) ?? new CriterionAttribute()
            })
            .Where(t => !t.Criterion.Ignore)
            .ToList();

        return available;
    }

    private static void ApplyCustomPredicateFactories<TModel, TFilter>(
        List<AvailableFilterProperty> available,
        FilterTarget filterTarget,
        List<ICriterionResolution> resolutions)
            where TModel : class
            where TFilter : class
    {
        if (SpecifierGeneratorOptions.TryGetOptions<TModel, TFilter>(out var options))
        {
            List<AvailableFilterProperty> toRemove = [];

            foreach (var tuple in available)
            {
                if (options.TryGetPropertyOptions(tuple.FilterProperty.Info, out var propertyOptions)
                    && propertyOptions.PredicateFactory is not null)
                {
                    var resolution = new PredicateFactoryCriterionResolution(
                        tuple.FilterProperty,
                        tuple.Criterion,
                        filterTarget,
                        propertyOptions.PredicateFactory);

                    resolutions.Add(resolution);
                    toRemove.Add(tuple);
                }
            }

            toRemove.ForEach(r => available.Remove(r));
        }
    }

    private static void BuildDisjunctionsFromAttributes(
        List<AvailableFilterProperty> available, 
        FilterTarget filterTarget, 
        List<ICriterionResolution> resolutions)
    {
        var disjuctionsElected = available
            .Where(t => t.FilterProperty.Info.IsDefined(typeof(DisjuctionAttribute), true))
            .Select(t => new
            {
                Property = t,
                Group = t.FilterProperty.Info.GetCustomAttribute<DisjuctionAttribute>(true)!.Alias
            })
            .ToList();

        if (disjuctionsElected.Count is not 0)
        {
            var disjuctions = disjuctionsElected
                .GroupBy(t => t.Group)
                .Select(g => new DisjuctionCriterionResolution(
                    filterTarget,
                    [.. g.Select(t => new JunctionProperty(t.Property.FilterProperty, t.Property.Criterion, filterTarget))]))
                .ToList();

            resolutions.AddRange(disjuctions);
            disjuctionsElected.ForEach(de => available.Remove(de.Property));
        }
    }

    private static void BuildDisjunctionsFromNameOrTargetPath(
        List<AvailableFilterProperty> available,
        FilterTarget filterTarget,
        List<ICriterionResolution> resolutions)
    {
        var junctionsElected = available
            .Select(t => new
            {
                Property = t,
                Parts = (t.Criterion.TargetPropertyPath ?? t.FilterProperty.PropertyName)
                    .Split(["Or"], StringSplitOptions.RemoveEmptyEntries)
            })
            // Only consider as junction when split produced more than one part (e.g., FirstNameOrLastName)
            .Where(x => x.Parts.Length > 1)
            .ToList();

        if (junctionsElected.Count is not 0)
        {
            var junctions = junctionsElected
                .Select(j => new DisjuctionCriterionResolution(
                    filterTarget,
                    [.. j.Parts.Select(part => new JunctionProperty(
                        j.Property.FilterProperty,
                        new CriterionAttribute
                        {
                            Operator = j.Property.Criterion.Operator,
                            Negation = j.Property.Criterion.Negation,
                            IgnoreIfIsEmpty = j.Property.Criterion.IgnoreIfIsEmpty,
                            TargetPropertyPath = part,
                        },
                        filterTarget))
                    ]))
                .ToList();

            resolutions.AddRange(junctions);
            junctionsElected.ForEach(j => available.Remove(j.Property));
        }
    }

    private static void BuildComplexFilterResolutions(
        List<AvailableFilterProperty> available,
        FilterTarget filterTarget, 
        List<ICriterionResolution> resolutions)
    {
        var complexElected = available
            .Where(t =>
                t.FilterProperty.Info.IsDefined(typeof(ComplexFilterAttribute), true)
                || t.FilterProperty.Info.PropertyType.IsDefined(typeof(ComplexFilterAttribute), true))
            .ToList();

        if (complexElected.Count is not 0)
        {
            var complexResolutions = complexElected
                .Select(t => new ComplexFilterCriterionResolution(
                    t.FilterProperty,
                    t.Criterion,
                    filterTarget))
                .ToList();

            resolutions.AddRange(complexResolutions);
            complexElected.ForEach(c => available.Remove(c));
        }
    }

    private static void BuildFilterExpressionGenerator(
        List<AvailableFilterProperty> available, 
        FilterTarget filterTarget, 
        List<ICriterionResolution> resolutions)
    {
        var generatorElected = available
            .Where(t =>
                t.FilterProperty.Info.IsDefined(typeof(FilterExpressionGeneratorAttribute<>), true)
                || t.FilterProperty.Info.PropertyType.IsDefined(typeof(FilterExpressionGeneratorAttribute<>), true))
            .ToList();

        if (generatorElected.Count is not 0)
        {
            var generatorResolutions = generatorElected
                .Select(t =>
                {
                    // obter tipo genérico do atributo
                    var attr = t.FilterProperty.Info.GetCustomAttribute(typeof(FilterExpressionGeneratorAttribute<>), true)
                        ?? t.FilterProperty.Info.PropertyType.GetCustomAttribute(typeof(FilterExpressionGeneratorAttribute<>), true);

                    // extrair o tipo genérico TExpressionGenerator do atributo
                    var genericAttrType = attr?.GetType().GetGenericArguments()[0];

                    return new FilterExpressionGeneratorResolution(
                        t.FilterProperty,
                        t.Criterion,
                        filterTarget,
                        genericAttrType);
                })
                .ToList();

            resolutions.AddRange(generatorResolutions);
            generatorElected.ForEach(g => available.Remove(g));
        }
    }

    private static void BuildDefaultOperatorResolutions(
        List<AvailableFilterProperty> available,
        FilterTarget filterTarget, 
        List<ICriterionResolution> resolutions)
    {
        foreach (var t in available)
        {
            resolutions.Add(new DefaultOperatorCriterionResolution(t.FilterProperty, t.Criterion, filterTarget));
        }
    }

    private sealed class AvailableFilterProperty
    {
        public required PropertySelection FilterProperty { get; init; }
        public required CriterionAttribute Criterion { get; init; }
    }
}