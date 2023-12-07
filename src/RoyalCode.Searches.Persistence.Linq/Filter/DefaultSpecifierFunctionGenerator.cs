using RoyalCode.Extensions.PropertySelection;
using RoyalCode.Searches.Abstractions;
using RoyalCode.Searches.Persistence.Abstractions.Extensions;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.Searches.Persistence.Linq.Filter;

/// <summary>
/// <para>
///     Generates a function that apply filters in a query.
/// </para>
/// </summary>
public class DefaultSpecifierFunctionGenerator : ISpecifierFunctionGenerator
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

    /// <inheritdoc />
    public Func<IQueryable<TModel>, TFilter, IQueryable<TModel>>? Generate<TModel, TFilter>()
        where TModel : class
        where TFilter : class
    {
        // creates criterion resolution for each filter type properties.
        var criterionResolutions = typeof(TFilter).GetProperties()
            .Select(p => new CriterionResolution(p, p.GetCustomAttribute<CriterionAttribute>(true)))
            .Where(cr => !cr.Criterion.Ignore)
            .ToList();

        // try add predicate factories from custom specifier generator options.
        if (SpecifierGeneratorOptions.TryGetOptions<TModel, TFilter>(out var options))
        {
            foreach (var cr in criterionResolutions)
            {
                if (options.TryGetPropertyOptions(cr.FilterPropertyInfo, out var propertyOptions)
                    && propertyOptions.PredicateFactory is not null)
                {
                    cr.AddPredicateFactory(propertyOptions.PredicateFactory);
                }
            }
        }

        // for each criterion resolution where the criterion attribute has a property name,
        // creates a property selection for the model type.
        var configuredProperties = criterionResolutions
            .Where(cr => cr.IsPending && cr.Criterion.TargetProperty is not null);
        foreach (var resolution in configuredProperties)
        {
            var propertySelection = typeof(TModel).TrySelectProperty(resolution.Criterion.TargetProperty!);
            if (propertySelection is null)
                return null;

            resolution.TargetSelection = propertySelection;
        }

        // match properties of filter type with properties of model type.
        var selectionMatch = typeof(TFilter).MatchProperties(typeof(TModel));

        // join the criterion resolutions with the selection match,
        // where the criterion resolution is pending.
        var pendingProperties = criterionResolutions
            .Where(cr => cr.IsPending);
        foreach (var criterionResolution in pendingProperties)
            foreach (var match in selectionMatch.PropertyMatches)
                if (match.OriginProperty.Name == criterionResolution.FilterPropertyInfo.Name
                    && match.Match
                    && (match.TypeMatch
                        || CheckTypes(criterionResolution.FilterPropertyInfo.PropertyType, match.TargetSelection!.PropertyType)))
                {
                    criterionResolution.TargetSelection = match.TargetSelection;
                }

        // check if all resolution are satisfied, if any pending, then return.
        if (criterionResolutions.Exists(cr => cr.IsPending))
            return null;

        // creates a function to apply the filter in a query.
        return Create<TModel, TFilter>(criterionResolutions);
    }

    private static Func<IQueryable<TModel>, TFilter, IQueryable<TModel>>? Create<TModel, TFilter>(
        List<CriterionResolution> resolvedProperties)
        where TModel : class
        where TFilter : class
    {
        var filterParam = Expression.Parameter(typeof(TFilter), "filter");
        var queryParam = Expression.Parameter(typeof(IQueryable<TModel>), "query");
        List<Expression> body = [];

        foreach (var resolution in resolvedProperties)
        {
            // the predicate expression to pass to the queryable (to where method).
            Expression predicateExpression;

            // check if resolution has a predicate factory
            var predicateFactory = resolution.TryGetPredicateFactory<TModel>(resolution.FilterPropertyInfo.PropertyType);
            if (predicateFactory is not null)
            {
                // create a expression to call the predicate factory for create the predicate expression.
                var predicateFactoryCall = Expression.Call(
                    Expression.Constant(predicateFactory.Target),
                    predicateFactory.Method,
                    GetMemberAccess(resolution.FilterPropertyInfo, filterParam));

                predicateExpression = predicateFactoryCall;
            }
            else
            {
                // the predicate function parameter, the entity/model of the query.
                var targetParam = Expression.Parameter(typeof(TModel), "e");

                var operatorExpression = CreateOperatorExpression(
                    DiscoveryCriterionOperator(resolution.Criterion, resolution.FilterPropertyInfo),
                    resolution.Criterion.Negation,
                    GetMemberAccess(resolution.FilterPropertyInfo, filterParam),
                    GetMemberAccess(resolution.TargetSelection!, targetParam));

                // generate the type of the predicate.
                var predicateType = typeof(Func<,>).MakeGenericType(
                    typeof(TModel),
                    typeof(bool));

                // create the lambda expression for the queryable
                var lambda = Expression.Lambda(predicateType, operatorExpression, targetParam);

                predicateExpression = lambda;
            }

            // create the method call to apply the filter in the query.
            var methodCall = Expression.Call(
                WhereMethod.MakeGenericMethod(typeof(TModel)),
                queryParam,
                predicateExpression);

            // assign the query with the value of the call.
            var assign = Expression.Assign(queryParam, methodCall);

            // create an expression to check if the filter property is empty
            body.Add(resolution.Criterion.IgnoreIfIsEmpty
                ? GetIfIsEmptyConstraintExpression(
                    Expression.MakeMemberAccess(filterParam, resolution.FilterPropertyInfo),
                    assign)
                : assign);
        }

        // build the expression of the lambda function to apply the filters.
        var funcType = typeof(Func<,,>).MakeGenericType(
            typeof(IQueryable<TModel>),
            typeof(TFilter),
            typeof(IQueryable<TModel>));

        body.Add(queryParam);
        var bodyBlock = Expression.Block(body);

        var lambdaFunc = Expression.Lambda(funcType, bodyBlock, queryParam, filterParam);

        // compile the lambda function.
        return (Func<IQueryable<TModel>, TFilter, IQueryable<TModel>>?)lambdaFunc.Compile();
    }

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
        MemberExpression filterMemberAccess,
        MemberExpression targetMemberAccess)
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
    /// Gets the expression to access the member, checking if it is a Nullable to get the Value if it is.
    /// </summary>
    /// <param name="propertySelection">The property selection.</param>
    /// <param name="parameterExpression">The parameter expression.</param>
    /// <returns>The expression to access the member.</returns>
    public static MemberExpression GetMemberAccess(
        PropertySelection propertySelection,
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
        PropertyInfo property,
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
        Expression filterMemberAccess,
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
    public static bool CheckTypes(Type filterPropertyType, Type modelPropertyType)
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
}
