using RoyalCode.SmartSearch.Linq.Filtering;

namespace RoyalCode.SmartSearch;

/// <summary>
/// <para>
///     A filter expression generator attribute that specifies the expression generator to use for a filter property.
/// </para>
/// <para>
///     The expression generator must implement the <see cref="ISpecifierExpressionGenerator"/> interface.
///     This generator will be used to create the expression for the filter property.
/// </para>
/// </summary>
/// <typeparam name="TExpressionGenerator"></typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = false)]
public class FilterExpressionGeneratorAttribute<TExpressionGenerator> : Attribute 
    where TExpressionGenerator : ISpecifierExpressionGenerator
{ }
    


