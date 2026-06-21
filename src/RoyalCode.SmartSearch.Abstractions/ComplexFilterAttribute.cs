namespace RoyalCode.SmartSearch;

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
/// <para>
///     It can be applied to classes, structs, or properties to denote that they represent complex filter types or properties.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = false)]
public class ComplexFilterAttribute : Attribute { }

