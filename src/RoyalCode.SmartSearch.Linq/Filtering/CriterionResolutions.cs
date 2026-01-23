using RoyalCode.Extensions.PropertySelection;
using RoyalCode.SmartSearch.Linq.Filtering.Resolutions;
using System.Reflection;

namespace RoyalCode.SmartSearch.Linq.Filtering;

internal static class CriterionResolutions
{
    public static IReadOnlyList<ICriterionResolution> CreateResolutions<TModel, TFilter>(PropertySelection? previousFilterProperty = null)
        where TModel : class
        where TFilter : class
    {
        List<ICriterionResolution> resolutions = [];

        var available = typeof(TFilter).GetProperties()
            .Select(p => new
            {
                FilterProperty = previousFilterProperty?.SelectChild(p) ?? new PropertySelection(p),
                Criterion = p.GetCustomAttribute<CriterionAttribute>(true) ?? new CriterionAttribute()
            })
            .Where(t => !t.Criterion.Ignore)
            .ToList();

        // 1 - For each available property, check if there is a predicate factory defined in the specifier options.
        //     If so, create a PredicateFactoryCriterionResolution for it,
        //     and remove it from the available properties.

        // try add predicate factories from custom specifier generator options.
        if (SpecifierGeneratorOptions.TryGetOptions<TModel, TFilter>(out var options))
        {
            List<PredicateFactoryCriterionResolution> predicateResolutions = [];
            foreach (var tuple in available)
            {
                if (options.TryGetPropertyOptions(tuple.FilterProperty.Info, out var propertyOptions)
                    && propertyOptions.PredicateFactory is not null)
                {
                    var resolution = new PredicateFactoryCriterionResolution(
                        tuple.FilterProperty,
                        tuple.Criterion,
                        typeof(TModel),
                        propertyOptions.PredicateFactory);

                    predicateResolutions.Add(resolution);
                }
            }

            resolutions.AddRange(predicateResolutions);
            available = available
                .Where(t => !predicateResolutions.Exists(pr => pr.FilterPropertySelection.PropertyName == t.FilterProperty.PropertyName))
                .ToList();
        }

        // 2 - for each available property, check if it has DisjuctionAttribute defined,
        //     and group them by the DisjuctionAttribute.Alias value,
        //     creating a DisjuctionCriterionResolution for each group,
        //     and removing them from the available properties.

        // get the disjunction groups from the remaining elected properties.
        var disjuctionsElected = available
            .Where(t => t.FilterProperty.Info.IsDefined(typeof(DisjuctionAttribute), true))
            .Select(t => new
            {
                t.FilterProperty,
                t.Criterion,
                Group = t.FilterProperty.Info.GetCustomAttribute<DisjuctionAttribute>(true)!.Alias
            })
            .ToList();

        if (disjuctionsElected.Count is not 0)
        {
            var disjuctions = disjuctionsElected
                .GroupBy(t => t.Group)
                .Select(g => new DisjuctionCriterionResolution(
                    typeof(TModel),
                    [.. g.Select(t => new JunctionProperty(t.FilterProperty, t.Criterion, typeof(TModel)))]))
                .ToList();

            resolutions.AddRange(disjuctions);
            available = available
                .Where(t => !disjuctionsElected.Exists(de => de.FilterProperty.PropertyName == t.FilterProperty.PropertyName))
                .ToList();
        }

        // 3 - for each available property, check if its name (or the TargetPropertyPath) contains "Or",
        //     splitting it into parts, and creating a DisjuctionCriterionResolution for each,
        //     and removing them from the available properties.

        // Search for properties that have Or in the middle of the name, e.g., FirstNameOrMiddleNameOrLastName.
        var junctionsElected = available
            .Where(t =>
                (t.Criterion.TargetPropertyPath is not null && t.Criterion.TargetPropertyPath.Contains("Or"))
                || t.FilterProperty.PropertyName.Contains("Or"))
            .Select(t => new
            {
                t.FilterProperty,
                t.Criterion,
                Parts = (t.Criterion.TargetPropertyPath ?? t.FilterProperty.PropertyName).Split(["Or"], StringSplitOptions.RemoveEmptyEntries)
            })
            .ToList();

        if (junctionsElected.Count is not 0)
        {
            // For each part of the name, create a JunctionProperty, using the same FilterProperty,
            // but creating a Criterion for each and assigning the TargetPropertyPath with the value of the part.
            var junctions = junctionsElected
                .Select(j => new DisjuctionCriterionResolution(
                    typeof(TModel),
                        [.. j.Parts.Select(part => new JunctionProperty(
                            j.FilterProperty,
                            new CriterionAttribute
                            {
                                Operator = j.Criterion.Operator,
                                Negation = j.Criterion.Negation,
                                IgnoreIfIsEmpty = j.Criterion.IgnoreIfIsEmpty,
                                TargetPropertyPath = part,
                            },
                            typeof(TModel)))
                        ]))
                .ToList();

            resolutions.AddRange(junctions);
            available = available
                .Where(t => !junctionsElected.Exists(je => je.FilterProperty.PropertyName == t.FilterProperty.PropertyName))
                .ToList();
        }

        // 4 - for each available property, Check for ComplexFilterAttribute,
        //     creating a ComplexFilterCriterionResolution for each,
        //     and removing them from the available properties.

        // search for properties that have the ComplexFilter attribute or whose type has the ComplexFilter attribute.
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
                    typeof(TModel)))
                .ToList();
            resolutions.AddRange(complexResolutions);
            available = available
                .Where(t => !complexElected.Exists(ce => ce.FilterProperty.PropertyName == t.FilterProperty.PropertyName))
                .ToList();
        }

        // Finally - for each remaining available property, create a DefaultOperatorCriterionResolution.

        // for each remaining elected property, creates a criterion resolution.
        var remainingResolutions = available
        .Select(t => new DefaultOperatorCriterionResolution(t.FilterProperty, t.Criterion, typeof(TModel)))
        .ToList();

        resolutions.AddRange(remainingResolutions);

        return resolutions;
    }
}