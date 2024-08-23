using System.Runtime.CompilerServices;

namespace RoyalCode.Searches.Linq.Filter;

internal sealed class InternalSpecifier<TModel, TFilter> : ISpecifier<TModel, TFilter>
    where TModel : class
    where TFilter : class
{
    private readonly Func<IQueryable<TModel>, TFilter, IQueryable<TModel>> specifierFunction;

    public InternalSpecifier(Func<IQueryable<TModel>, TFilter, IQueryable<TModel>> specifierFunction)
    {
        this.specifierFunction = specifierFunction;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IQueryable<TModel> Specify(IQueryable<TModel> query, TFilter filter)
    {
        return specifierFunction(query, filter);
    }
}