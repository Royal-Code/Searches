using RoyalCode.Extensions.PropertySelection;
using RoyalCode.SmartSearch.Core.Extensions;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.SmartSearch;

/// <summary>
/// Helpers to generate expressions for filtering.
/// </summary>
public static class ExpressionGenerator
{
    #region Static Helpers

    /// <summary>
    /// MethodInfo referring to the generic function that checks whether a value is an empty representation of a type.
    /// </summary>
    private static readonly MethodInfo IsEmptyMethod = typeof(IsEmptyExtension)
        .GetMethod(nameof(IsEmptyExtension.IsEmpty))!;

    /// <summary>
    /// MethodInfo for checks whether a string is an empty.
    /// </summary>
    private static readonly MethodInfo IsNullOrWhiteSpaceMethod =
        typeof(string).GetMethod(nameof(string.IsNullOrWhiteSpace))!;

    /// <summary>
    /// MethodInfo for checks whether a enumerable is empty.
    /// </summary>
    private static readonly MethodInfo AnyMethod = typeof(Enumerable).GetMethods()
        .Where(m => m.Name == nameof(Enumerable.Any))
        .First(m => m.GetParameters().Length == 1);

    private static readonly MethodInfo WhereMethod = typeof(Queryable).GetMethods()
        .Where(m => m.Name == nameof(Queryable.Where))
        .Where(m => m.GetParameters().Length == 2)
        .First(m => m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2);

    /// <summary>
    /// MethodInfo for checks whether a date is empty.
    /// </summary>
    private static readonly MethodInfo IsBlankMethod = typeof(IsEmptyExtension)
        .GetMethod(nameof(IsEmptyExtension.IsBlank), [typeof(DateTime)])!;

    /// <summary>
    /// Contains Method of string.
    /// </summary>
    public static readonly MethodInfo ContainsMethod = typeof(string)
        .GetMethod(nameof(string.Contains), [typeof(string)])!;

    /// <summary>
    /// StartsWith Method of string.
    /// </summary>
    public static readonly MethodInfo StartsWithMethod = typeof(string)
        .GetMethod(nameof(string.StartsWith), [typeof(string)])!;

    /// <summary>
    /// EndsWith Method of string.
    /// </summary>
    public static readonly MethodInfo EndsWithMethod = typeof(string)
        .GetMethod(nameof(string.EndsWith), [typeof(string)])!;

    /// <summary>
    /// Where method of <see cref="Enumerable"/> to call over <see cref="IEnumerable{T}"/>.
    /// </summary>
    internal static readonly MethodInfo InMethod = typeof(Enumerable).GetMethods()
        .Where(m => m.Name == nameof(Enumerable.Contains))
        .First(m => m.GetParameters().Length == 2);

    /// <summary>
    /// Types where "greater than" is applied to check that the value is not empty.
    /// </summary>
    private static readonly Type[] GreaterThenTypes =
    [
        typeof(byte),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(float),
        typeof(double),
        typeof(decimal),
    ];

    #endregion

    /// <summary>
    /// <para>
    ///     Creates the expression that performs the comparison between the model property and the filter property.
    /// </para>
    /// </summary>
    /// <param name="operator">The operator to be used in the comparison.</param>
    /// <param name="negation">Indicates whether the comparison should be negated.</param>
    /// <param name="filterMemberAccess">The expression that represents the filter property.</param>
    /// <param name="targetMemberAccess">The expression that represents the model property.</param>
    /// <returns>The expression that performs the comparison.</returns>
    /// <exception cref="InvalidOperationException">
    ///     The operator is not supported.
    /// </exception>
    public static Expression CreateOperatorExpression(
        CriterionOperator @operator,
        bool negation,
        Expression filterMemberAccess,
        Expression targetMemberAccess)
    {
        Expression? expression = null;

        switch (@operator)
        {
            case CriterionOperator.Equal:
                return negation
                    ? Expression.NotEqual(targetMemberAccess, filterMemberAccess)
                    : Expression.Equal(targetMemberAccess, filterMemberAccess);
            case CriterionOperator.GreaterThan:
                expression = Expression.GreaterThan(targetMemberAccess, filterMemberAccess);
                break;
            case CriterionOperator.GreaterThanOrEqual:
                expression = Expression.GreaterThanOrEqual(targetMemberAccess, filterMemberAccess);
                break;
            case CriterionOperator.LessThan:
                expression = Expression.LessThan(targetMemberAccess, filterMemberAccess);
                break;
            case CriterionOperator.LessThanOrEqual:
                expression = Expression.LessThanOrEqual(targetMemberAccess, filterMemberAccess);
                break;
            case CriterionOperator.Like:
                expression = Expression.Call(
                    targetMemberAccess,
                    typeof(string).GetMethod("Contains", [typeof(string)])!,
                    filterMemberAccess);
                break;
            case CriterionOperator.Contains:
                expression = Expression.Call(targetMemberAccess, ContainsMethod, filterMemberAccess);
                break;
            case CriterionOperator.StartsWith:
                expression = Expression.Call(targetMemberAccess, StartsWithMethod, filterMemberAccess);
                break;
            case CriterionOperator.EndsWith:
                expression = Expression.Call(targetMemberAccess, EndsWithMethod, filterMemberAccess);
                break;
            case CriterionOperator.In:
                // check if filter type is IEnumerable and has a generic type.
                if (filterMemberAccess.Type.IsGenericType
                    && filterMemberAccess.Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    // get the generic type of the filter.
                    var filterGenericType = filterMemberAccess.Type.GetGenericArguments()[0];

                    // get the method to check if the target is in the filter.
                    var method = InMethod.MakeGenericMethod(filterGenericType);

                    // create the expression to check if the target is in the filter.
                    expression = Expression.Call(method, filterMemberAccess, targetMemberAccess);
                }
                else
                {
                    throw new InvalidOperationException("The filter property must be an IEnumerable.");
                }
                break;
        }

        if (expression is null)
            throw new InvalidOperationException("The operator is not supported.");

        if (negation)
            expression = Expression.Not(expression);

        return expression;
    }

    /// <summary>
    /// Gets the operator of a condition for a filter from the <see cref="CriterionAttribute"/>.
    /// </summary>
    /// <param name="criterion"><see cref="CriterionAttribute"/>.</param>
    /// <param name="filterProperty">The filter property.</param>
    /// <returns>The operator of the condition that should be applied in the filter.</returns>
    public static CriterionOperator DiscoveryCriterionOperator(
        CriterionAttribute criterion,
        PropertyInfo filterProperty)
    {
        if (criterion.Operator != CriterionOperator.Auto)
        {
            return criterion.Operator;
        }
        if (filterProperty.PropertyType == typeof(string))
        {
            return CriterionOperator.Like;
        }
        else if (typeof(IEnumerable).IsAssignableFrom(filterProperty.PropertyType))
        {
            return CriterionOperator.In;
        }

        // think about this
        ////else if (PropertyFilter.GreaterThanOrEqualSuffix.Any(filterProperty.PropertyName.EndsWith))
        ////{
        ////    return CriterionOperator.GreaterThanOrEqual;
        ////}
        ////else if (PropertyFilter.GreaterThanSuffix.Any(filterProperty.PropertyName.EndsWith))
        ////{
        ////    return CriterionOperator.GreaterThan;
        ////}
        ////else if (PropertyFilter.LessThanOrEqualSuffix.Any(filterProperty.PropertyName.EndsWith))
        ////{
        ////    return CriterionOperator.LessThanOrEqual;
        ////}
        ////else if (PropertyFilter.LessThanSuffix.Any(filterProperty.PropertyName.EndsWith))
        ////{
        ////    return CriterionOperator.LessThan;
        ////}

        return CriterionOperator.Equal;
    }

    /// <summary>
    /// Creates a MethodCallExpression for a Where call on a queryable.
    /// </summary>
    /// <param name="modelType">The model type.</param>
    /// <param name="queryParam">The query parameter expression.</param>
    /// <param name="predicateExpression">The predicate expression.</param>
    /// <returns>The MethodCallExpression representing the Where call.</returns>
    public static MethodCallExpression CreateWhereCall(Type modelType, ParameterExpression queryParam, Expression predicateExpression)
    {
        // create the method call to apply the filter in the query.
        return Expression.Call(
            WhereMethod.MakeGenericMethod(modelType),
            queryParam,
            predicateExpression);
    }

    /// <summary>
    /// Gets the expression to access the member, checking if it is a Nullable to get the Value if it is.
    /// </summary>
    /// <param name="propertySelection">The property selection.</param>
    /// <param name="parameterExpression">The parameter expression.</param>
    /// <returns>The expression to access the member.</returns>
    public static MemberExpression GetMemberAccess(
        this PropertySelection propertySelection,
        Expression parameterExpression)
    {
        // expressão da propriedade
        return Nullable.GetUnderlyingType(propertySelection.PropertyType) == null
            ? propertySelection.GetAccessExpression(parameterExpression)
            : propertySelection.SelectChild("Value")!.GetAccessExpression(parameterExpression);
    }

    /// <summary>
    /// Gets the expression to access the member, checking if it is a Nullable to get the Value if it is.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="parameterExpression">The parameter expression.</param>
    /// <returns>The expression to access the member.</returns>
    public static MemberExpression GetMemberAccess(
        this PropertyInfo property,
        Expression parameterExpression)
    {
        // expressão da propriedade
        return Nullable.GetUnderlyingType(property.PropertyType) == null
            ? Expression.MakeMemberAccess(parameterExpression, property)
            : new PropertySelection(property).SelectChild("Value")!.GetAccessExpression(parameterExpression);
    }

    /// <summary>
    /// <para>
    ///     Creates a conditional expression to check whether the filter value is not empty.
    /// </para>
    /// <para>
    ///     The result is an IfThen expression.
    /// </para>
    /// </summary>
    /// <param name="filterMemberAccess">Expression that returns the value of the filter property.</param>
    /// <param name="assignExpression">Expression that will be executed if the condition is true.</param>
    /// <returns>New IfThen expression with the generated condition.</returns>
    public static Expression GetIfIsEmptyConstraintExpression(
        this Expression filterMemberAccess,
        Expression assignExpression)
    {
        Expression constraint;
        var type = filterMemberAccess.Type;
        if (type == typeof(string))
        {
            constraint = Expression.Not(Expression.Call(IsNullOrWhiteSpaceMethod, filterMemberAccess));
        }
        else if (typeof(IEnumerable).IsAssignableFrom(type) && type.GetGenericArguments().Length == 1)
        {
            constraint = Expression.Call(
                AnyMethod.MakeGenericMethod(type.GetGenericArguments()[0]),
                filterMemberAccess);
        }
        else if (Nullable.GetUnderlyingType(type) != null)
        {
            constraint = Expression.MakeMemberAccess(filterMemberAccess, type.GetProperty("HasValue")!);
        }
        else if (GreaterThenTypes.Contains(type))
        {
            constraint = Expression.GreaterThan(filterMemberAccess, Expression.Default(type));
        }
        else if (type == typeof(DateTime))
        {
            constraint = Expression.Not(Expression.Call(IsBlankMethod, filterMemberAccess));
        }
        else if (type.IsClass)
        {
            constraint = Expression.NotEqual(filterMemberAccess, Expression.Constant(null, type));
        }
        else
        {
            constraint = Expression.Not(Expression.Call(IsEmptyMethod, Expression.Convert(filterMemberAccess, typeof(object))));
        }

        return Expression.IfThen(constraint, assignExpression);
    }

    /// <summary>
    /// It checks two types of data, the first from the filter property, the second, from the model property,
    /// if they are compatible for applying a filter.
    /// </summary>
    /// <param name="filterPropertyType">The filter property type.</param>
    /// <param name="modelPropertyType">The model property type.</param>
    /// <returns>True if the types are compatible, otherwise false.</returns>
    public static bool CheckTypes(this Type filterPropertyType, Type modelPropertyType)
        => InnerCheckTypes(filterPropertyType, modelPropertyType, false);

    private static bool InnerCheckTypes(Type filterPropertyType, Type modelPropertyType, bool genericArg)
    {
        // check if filter type is nullable and compare with the model type
        if (Nullable.GetUnderlyingType(filterPropertyType) == modelPropertyType)
        {
            return true;
        }
        // check if model type is nullable and compare with the filter type
        if (Nullable.GetUnderlyingType(modelPropertyType) == filterPropertyType)
        {
            return true;
        }
        // check if the model type has ComplexFilter attribute and get the attribute generic type to compare with the filter type
        if (HasAttribute(Nullable.GetUnderlyingType(modelPropertyType), typeof(ComplexFilterAttribute<>), out var complexFilterAttr))
        {
            var complexFilterType = complexFilterAttr.GetType().GetGenericArguments()[0];
            if (filterPropertyType == complexFilterType)
            {
                return true;
            }
        }

        // when generic arg, is required to check same types, when not, is required to check if is IEnumerable
        if (genericArg)
        {
            if (filterPropertyType == modelPropertyType)
            {
                return true;
            }
        }
        else
        {
            // check if filter type is IEnumerable and generic with one arg and compare the arg with the model type
            if (filterPropertyType.IsGenericType
                && typeof(IEnumerable).IsAssignableFrom(filterPropertyType)
                && InnerCheckTypes(filterPropertyType.GetGenericArguments()[0], modelPropertyType, true))
            {
                return true;
            }
        }

        ////// check if both types are assignable to IEnumerable, and generic, and compare the args
        ////if (typeof(IEnumerable).IsAssignableFrom(filterPropertyType)
        ////    && typeof(IEnumerable).IsAssignableFrom(modelPropertyType)
        ////    && filterPropertyType.IsGenericType
        ////    && modelPropertyType.IsGenericType
        ////    && filterPropertyType.GetGenericArguments()[0] == modelPropertyType.GetGenericArguments()[0])
        ////{
        ////    return true;
        ////} ---> for now, it's not possible to apply a condition for this case.

        return false;
    }

    /// <summary>
    /// Check if the complex type has the specified attribute.
    /// </summary>
    /// <param name="complexType"></param>
    /// <param name="attributeType"></param>
    /// <param name="attr"></param>
    /// <returns></returns>
    public static bool HasAttribute(this Type? complexType, Type attributeType, [NotNullWhen(true)] out object? attr)
    {
        var attributes = complexType?.GetCustomAttributes(attributeType, true);
        attr = attributes?.FirstOrDefault() ?? null;
        return attr != null;
    }
}
