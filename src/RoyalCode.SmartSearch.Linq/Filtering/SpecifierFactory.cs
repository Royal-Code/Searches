using RoyalCode.SmartSearch.Linq.Services;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq.Filtering;

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

    public ISpecifier<TModel, TFilter> GetSpecifier<TModel, TFilter>()
        where TModel : class
        where TFilter : class
    {
        if (specifiers.TryGet<TModel, TFilter>(out var specifier))
            return specifier;

        specifier = specifierGenerator?.Generate<TModel, TFilter>();
        Lack[]? lacks = null;

        if (specifier is null)
        {
            if (TryFindByFilterMethod(out specifier))
            {
                specifiers.Add((typeof(TModel), typeof(TFilter)), specifier);
                return specifier;
            }

            if (functionGenerator is not null)
            {
                var result = functionGenerator.Generate<TModel, TFilter>();
                if (result.HasFunction(out var function))
                    specifier = new InternalSpecifier<TModel, TFilter>(function);
                else
                    lacks = result.Lacks;
            }
        }

        if (specifier is not null)
        {
            specifiers.Add((typeof(TModel), typeof(TFilter)), specifier);
            return specifier;
        }

        throw lacks is not null
            ? Lack.ToException(lacks)
            : new InvalidOperationException("No specifier configured for the model and filter.");
    }

    private static bool TryFindByFilterMethod<TModel, TFilter>([NotNullWhen(true)] out ISpecifier<TModel, TFilter>? specifier)
        where TModel : class
        where TFilter : class
    {
        specifier = null;

        // using reflection, lookup for a method that has IQueryable<TModel> as parameter and return type.
        // if exists, create a lambda function using expression that meets Func<IQueryable<TModel>, TFilter, IQueryable<TModel>>
        // and create a InternalSpecifier<TModel, TFilter>.

        var method = typeof(TFilter).GetMethods()
            .FirstOrDefault(m => m.ReturnType == typeof(IQueryable<TModel>) &&
                                 m.GetParameters().Length == 1 &&
                                 m.GetParameters()[0].ParameterType == typeof(IQueryable<TModel>));
        
        if (method is not null)
        {
            var queryParamter = Expression.Parameter(typeof(IQueryable<TModel>), "query");
            var filterParameter = Expression.Parameter(typeof(TFilter), "filter");
            var callExpression = Expression.Call(filterParameter, method, queryParamter);

            var expression = Expression.Lambda<Func<IQueryable<TModel>, TFilter, IQueryable<TModel>>>(callExpression, queryParamter, filterParameter);

            specifier = new InternalSpecifier<TModel, TFilter>(expression.Compile());
            return true;
        }

        return false;
    }
}
