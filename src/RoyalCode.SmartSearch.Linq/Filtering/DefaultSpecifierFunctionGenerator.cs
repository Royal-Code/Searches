using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.SmartSearch.Linq.Filtering;

/// <summary>
/// <para>
///     Generates a function that apply filters in a query.
/// </para>
/// </summary>
public sealed class DefaultSpecifierFunctionGenerator : ISpecifierFunctionGenerator
{
    /// <inheritdoc />
    public Func<IQueryable<TModel>, TFilter, IQueryable<TModel>>? Generate<TModel, TFilter>()
        where TModel : class
        where TFilter : class
    {
        List<ICriterionResolution> resolutions = [];

        var elected = typeof(TFilter).GetProperties()
            .Select(p => new
            {
                Property = p,
                Criterion = p.GetCustomAttribute<CriterionAttribute>(true) ?? new CriterionAttribute()
            })
            .Where(t => !t.Criterion.Ignore)
            .ToList();

        // try add predicate factories from custom specifier generator options.
        if (SpecifierGeneratorOptions.TryGetOptions<TModel, TFilter>(out var options))
        {
            List<PredicateFactoryCriterionResolution> predicateResolutions = [];
            foreach (var pair in elected)
            {
                if (options.TryGetPropertyOptions(pair.Property, out var propertyOptions)
                    && propertyOptions.PredicateFactory is not null)
                {
                    var resolution = new PredicateFactoryCriterionResolution(pair.Property, pair.Criterion, typeof(TModel), propertyOptions.PredicateFactory);
                    predicateResolutions.Add(resolution);
                }
            }

            resolutions.AddRange(predicateResolutions);
            elected = elected
                .Where(t => !predicateResolutions.Exists(pr => pr.FilterPropertyInfo.Name == t.Property.Name))
                .ToList();
        }

        // get the disjunction groups from the remaining elected properties.
        var disjuctionsElected = elected
            .Where(t => t.Property.IsDefined(typeof(DisjuctionAttribute), true))
            .Select(t => new
            {
                t.Property,
                t.Criterion,
                Group = t.Property.GetCustomAttribute<DisjuctionAttribute>(true)!.Alias
            })
            .ToList();

        if (disjuctionsElected.Count is not 0)
        {
            var disjuctions = disjuctionsElected
                .GroupBy(t => t.Group)
                .Select(g => new DisjuctionCriterionResolution(
                    typeof(TModel),
                    [.. g.Select(t => new JunctionProperty(t.Property, t.Criterion, typeof(TModel)))]))
                .ToList();

            resolutions.AddRange(disjuctions);
            elected = elected
                .Where(t => !disjuctionsElected.Exists(de => de.Property.Name == t.Property.Name))
                .ToList();
        }

        // for each remaining elected property, creates a criterion resolution.
        var remainingResolutions = elected
            .Select(t => new DefaultOperatorCriterionResolution(t.Property, t.Criterion, typeof(TModel)))
            .ToList();

        resolutions.AddRange(remainingResolutions);

        // check if all resolution are satisfied, if any lack, then return.
        if (Lack.CheckLacks(out var lacks, resolutions))
            return null;

        // creates a function to apply the filter in a query.
        return Create<TModel, TFilter>(resolutions);
    }

    private static Func<IQueryable<TModel>, TFilter, IQueryable<TModel>>? Create<TModel, TFilter>(
        IReadOnlyList<ICriterionResolution> resolvedProperties)
        where TModel : class
        where TFilter : class
    {
        var filterParam = Expression.Parameter(typeof(TFilter), "filter");
        var queryParam = Expression.Parameter(typeof(IQueryable<TModel>), "query");
        List<Expression> body = [];

        foreach (var resolution in resolvedProperties)
        {
            // create the assign expression to apply the filter to the query.
            var assign = resolution.CreateExpression(queryParam, filterParam);

            // add the assign expression to the body.
            body.Add(assign);
        }

        // build the expression of the lambda function to apply the filters.
        var funcType = typeof(Func<,,>).MakeGenericType(
            typeof(IQueryable<TModel>),
            typeof(TFilter),
            typeof(IQueryable<TModel>));

        body.Add(queryParam);
        var bodyBlock = Expression.Block(body);

        var lambdaFunc = Expression.Lambda(funcType, bodyBlock, queryParam, filterParam);

        // compile the lambda function.
        return (Func<IQueryable<TModel>, TFilter, IQueryable<TModel>>?)lambdaFunc.Compile();
    }
}
