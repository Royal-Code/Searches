using System.Linq.Expressions;

namespace RoyalCode.Searches.Core;

/// <summary>
/// Information about the select to be applied to the query.
/// </summary>
/// <param name="EntityType">The query entity type.</param>
/// <param name="DtoType">The DTO type to be selected.</param>
/// <param name="SelectExpression">The select expression.</param>
public record SearchSelect(Type EntityType, Type DtoType, Expression SelectExpression);