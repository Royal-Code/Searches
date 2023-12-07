using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace RoyalCode.Searches.Persistence.Linq.Selector.Converters;

/// <summary>
/// A component that solves the mapping between properties of two types to create a select expression 
/// of <see cref="IQueryable{T}"/>.
/// </summary>
public interface ISelectResolver
{
    /// <summary>
    /// Get the resolutions for the mapping between properties of two types.
    /// </summary>
    /// <param name="entityType">The type of the entity, the source of the select expression.</param>
    /// <param name="dtoType">The type of the DTO, the target of the select expression.</param>
    /// <param name="ctor">
    ///     The constructor of the DTO, or null if the mapping cannot be resolved.
    /// </param>
    /// <param name="resolutions">
    ///     A list of resolutions for the mapping between properties of two types,
    ///     or null if the mapping cannot be resolved.
    /// </param>
    /// <returns>
    ///     Returns <c>true</c> when the mapping can be resolved, otherwise <c>false</c>.
    /// </returns>
    public bool GetResolutions(Type entityType, Type dtoType,
        [NotNullWhen(true)] out IEnumerable<SelectResolution>? resolutions,
        [NotNullWhen(true)] out ConstructorInfo? ctor);
}

