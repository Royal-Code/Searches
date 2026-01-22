using System.Diagnostics.CodeAnalysis;

namespace RoyalCode.SmartSearch.Linq.Filtering;

internal sealed class Lack
{
    public string Description { get; init; }

    public Exception ToException() => new InvalidOperationException(Description);

    public static bool CheckLacks([NotNullWhen(true)] out Lack[]? lacks, params IReadOnlyList<ICriterionResolution> resolutions)
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
}
