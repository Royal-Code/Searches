using RoyalCode.SmartSearch.Linq.Filtering.Resolutions;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering;

/// <summary>
/// <para>
///     Generates a function that apply filters in a query.
/// </para>
/// </summary>
public sealed class DefaultSpecifierFunctionGenerator : ISpecifierFunctionGenerator
{
    /// <inheritdoc />
    public SpecifierFunctionGenerationResult<TModel, TFilter> Generate<TModel, TFilter>()
        where TModel : class
        where TFilter : class
    {
        var resolutions = CriterionResolutions.CreateResolutions<TModel, TFilter>();

        // check if all resolution are satisfied, if any lack, then return.
        if (Lack.CheckLacks(out var lacks, resolutions))
            return lacks;

        // creates a function to apply the filter in a query.
        return Create<TModel, TFilter>(resolutions);
    }

    private static Func<IQueryable<TModel>, TFilter, IQueryable<TModel>> Create<TModel, TFilter>(
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
        return (Func<IQueryable<TModel>, TFilter, IQueryable<TModel>>)lambdaFunc.Compile();
    }
}
