using RoyalCode.SmartSearch.Core;
using RoyalCode.SmartSearch.Filtering;

namespace RoyalCode.SmartSearch.Linq;

/// <summary>
/// Component that applies the filtering conditions to the query,
/// where the query object is a <see cref="IQueryable{T}"/>.
/// </summary>
/// <typeparam name="TModel">The model of the <see cref="IQueryable{T}"/>.</typeparam>
/// <typeparam name="TFilter">The filter type.</typeparam>
public interface ISpecifier<TModel, in TFilter> : ISpecifier
    where TModel : class
    where TFilter : class
{ }