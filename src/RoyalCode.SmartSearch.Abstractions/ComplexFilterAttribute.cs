namespace RoyalCode.SmartSearch;

/// <summary>
/// Specifies a filter for a property of the annotated class or struct, enabling advanced filtering scenarios based on
/// the property's value.
/// </summary>
/// <typeparam name="TPropertyType">The type of the property to which the filter applies.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public class ComplexFilterAttribute<TPropertyType> : Attribute
{
    /// <summary>
    /// The name of the property to which the complex filter applies.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="ComplexFilterAttribute{TPropertyType}"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property to which the complex filter applies.</param>
    public ComplexFilterAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}

/// <summary>
/// <para>
///     Specifies that the annotated class or struct represents a complex filter type,
///     which can be used for advanced filtering scenarios.
/// </para>
/// <para>
///     When applied, it indicates that the type contains properties that can be used
///     for filtering purposes within the context of a complex filter.
/// </para>
/// <para>
///     In Entity Framework Core, classes or structs marked with this attribute 
///     can be mapped as a complex property or an owned entity, 
///     depending on how the class or struct is used.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class ComplexFilterAttribute : Attribute { }

