using System.Diagnostics.CodeAnalysis;

namespace RoyalCode.SmartSearch.Linq.Filtering;

/// <summary>
/// <para>
///     The result of a specifier function generation attempt.
/// </para>
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TFilter">The type of the filter.</typeparam>
public readonly ref struct SpecifierFunctionGenerationResult<TModel, TFilter>
    where TModel : class
    where TFilter : class
{
    /// <summary>
    /// Implicit conversion from function to result.
    /// </summary>
    /// <param name="function"></param>
    public static implicit operator SpecifierFunctionGenerationResult<TModel, TFilter>(
        Func<IQueryable<TModel>, TFilter, IQueryable<TModel>> function) => new()
        {
            Function = function
        };

    /// <summary>
    /// Implicit conversion from lack array to result.
    /// </summary>
    /// <param name="lacks"></param>
    public static implicit operator SpecifierFunctionGenerationResult<TModel, TFilter>(
        Lack[] lacks) => new()
        {
            Lacks = lacks
        };

    /// <summary>
    /// The Function generated.
    /// </summary>
    public Func<IQueryable<TModel>, TFilter, IQueryable<TModel>>? Function { get; init; }

    /// <summary>
    /// Gets the collection of lack items associated with this instance.
    /// </summary>
    public Lack[]? Lacks { get; init; }

    /// <summary>
    /// Determines whether a valid function is available and provides it if present.
    /// </summary>
    /// <param name="function">
    ///     When this method returns <see langword="true"/>, contains the function to apply a filter to a queryable
    ///     collection of <typeparamref name="TModel"/> objects. When <see langword="false"/>,
    ///     the value is <see langword="null"/>.
    /// </param>
    /// <returns>true if a function is available and no filters are lacking; otherwise, false.</returns>
    [MemberNotNullWhen(true, nameof(Function))]
    [MemberNotNullWhen(false, nameof(Lacks))]
    public bool HasFunction([NotNullWhen(true)] out Func<IQueryable<TModel>, TFilter, IQueryable<TModel>>? function)
    {
        function = Function;
        return Function is not null && (Lacks?.Length ?? 0) == 0;
    }
}