namespace RoyalCode.SmartSearch.Hints;

/// <summary>
/// Default <see cref="ICriteriaHint"/> carrier that captures one or more hint values of a single enum type.
/// </summary>
/// <typeparam name="THint">The hint enum type captured at the call site.</typeparam>
internal sealed class CriteriaHint<THint> : ICriteriaHint
    where THint : Enum
{
    private readonly THint[] hints;

    public CriteriaHint(THint[] hints) => this.hints = [.. hints];

    public void Accept(ICriteriaHintVisitor visitor)
    {
        foreach (var hint in hints)
            visitor.Visit(hint);
    }
}
