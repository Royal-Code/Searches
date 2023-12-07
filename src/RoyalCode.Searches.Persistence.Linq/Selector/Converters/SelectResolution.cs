using RoyalCode.Extensions.PropertySelection;

namespace RoyalCode.Searches.Persistence.Linq.Selector.Converters;

/// <summary>
/// It contains the resolved properties for creating a select expression of the <see cref="IQueryable{T}"/>.
/// </summary>
/// <param name="Match">The mapped properties.</param>
/// <param name="Converter">The converter, which generates the readout of the source type property.</param>
public record SelectResolution(
    PropertyMatch Match,
    ISelectorPropertyConverter Converter);
