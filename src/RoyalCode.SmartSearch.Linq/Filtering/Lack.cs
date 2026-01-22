using System.Diagnostics.CodeAnalysis;

namespace RoyalCode.SmartSearch.Linq.Filtering;

/// <summary>
/// Represents a missing requirement, unsupported capability, or unmet precondition detected while resolving
/// filtering criteria within the SmartSearch LINQ pipeline.
/// </summary>
/// <remarks>
/// A <see cref="Lack"/> conveys why a criterion or operation cannot be fulfilled. Implementations of
/// <c>ICriterionResolution</c> can produce instances to describe what is missing so that callers
/// can aggregate and present meaningful diagnostics to users or logs.
/// </remarks>
public sealed class Lack
{
    /// <summary>
    /// Human-readable description of what is lacking or why the requested filtering operation cannot be performed.
    /// </summary>
    /// <value>
    /// A non-empty string that explains the missing requirement in a way suitable for end-user messages or logs.
    /// </value>
    public required string Description { get; init; }

    /// <summary>
    /// Creates an <see cref="InvalidOperationException"/> that encapsulates this lack's <see cref="Description"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="InvalidOperationException"/> initialized with <see cref="Description"/>.
    /// </returns>
    public Exception ToException() => new InvalidOperationException(Description);

    internal static bool CheckLacks([NotNullWhen(true)] out Lack[]? lacks, params IReadOnlyList<ICriterionResolution> resolutions)
    {
        List<Lack>? lackingList = null;

        foreach (var resolution in resolutions)
        {
            if (resolution.IsLacking(out var lack))
            {
                lackingList ??= [];
                lackingList.Add(lack);
            }
        }

        lacks = lackingList?.ToArray();
        return lacks is not null;
    }

    internal static Exception ToException(Lack[] lacks)
    {
        var messages = lacks.Select(l => l.Description);
        var fullMessage = "Filtering cannot be applied due to the following lacks: " + string.Join("; ", messages);
        return new InvalidOperationException(fullMessage);
    }
}
