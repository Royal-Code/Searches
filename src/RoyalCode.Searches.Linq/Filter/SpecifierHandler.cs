using RoyalCode.Searches.Core;

namespace RoyalCode.Searches.Linq.Filter;

/// <summary>
/// <para>
///     Default implementation of <see cref="ISpecifierHandler"/>
///     that applies the filters to a <see cref="IQueryable{T}"/>,
///     retrieving the filters specifiers from the <see cref="ISpecifierFactory"/>,
///     which in turn knows how to apply the filter specification to the query.
/// </para>
/// </summary>
/// <typeparam name="TModel">The query model type.</typeparam>
public class SpecifierHandler<TModel> : ISpecifierHandler
    where TModel : class
{
    private readonly ISpecifierFactory factory;

    /// <summary>
    /// Creates a new specifier handler.
    /// </summary>
    /// <param name="factory">The specifier factory to create the specifiers.</param>
    /// <param name="query">The query to apply the filters.</param>
    public SpecifierHandler(ISpecifierFactory factory, IQueryable<TModel> query)
    {
        this.factory = factory;
        Query = query;
    }

    /// <summary>
    /// The query to apply the filters, always updating it with the last filtered query.
    /// </summary>
    public IQueryable<TModel> Query { get; private set; }

    /// <inheritdoc />
    public void Handle<TFilter>(TFilter filter) where TFilter : class
    {
        var specifier = factory.GetSpecifier<TModel, TFilter>();
        if (specifier is not null)
            Query = specifier.Specify(Query, filter);
    }
}