using RoyalCode.Extensions.PropertySelection;

namespace RoyalCode.SmartSearch.Linq.Mappings.Converters;

/// <summary>
/// <para>
///     Interface to resolve the properties of the <see cref="IQueryable{T}"/> selector.
/// </para>
/// </summary>
public interface ISelectorPropertyResolver
{
    /// <summary>
    /// Check if the converter can convert the <paramref name="selection"/>.
    /// </summary>
    /// <param name="selection">The selection to be converted.</param>
    /// <param name="resolver">The resolver of the properties.</param>
    /// <param name="converter">A converter that can convert the <paramref name="selection"/>.</param>
    /// <returns>
    ///     <c>true</c> if the converter can convert the <paramref name="selection"/>, otherwise <c>false</c>.
    /// </returns>
    bool CanConvert(PropertyMatch selection, ISelectResolver resolver, out ISelectorPropertyConverter? converter);
}
