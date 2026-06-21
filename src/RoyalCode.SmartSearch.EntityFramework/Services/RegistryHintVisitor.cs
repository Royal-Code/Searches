using RoyalCode.OperationHint.Abstractions;
using RoyalCode.SmartSearch.Hints;

namespace RoyalCode.SmartSearch.EntityFramework.Services;

/// <summary>
/// <para>
///     Applies per-query criteria hints to an Entity Framework query using the operation hint registry.
/// </para>
/// <para>
///     This is the only place where the criteria hint carrier (provider-neutral, in <c>Core</c>) meets the
///     <see cref="IHintHandlerRegistry"/>: the carrier supplies the closed <c>THint</c> through
///     <see cref="ICriteriaHintVisitor.Visit{THint}"/>, while this visitor supplies the query type
///     <typeparamref name="TQuery"/> and the registry — resolving both generic arguments by double-dispatch.
/// </para>
/// </summary>
/// <typeparam name="TQuery">The query type (e.g. <c>IQueryable&lt;TEntity&gt;</c>).</typeparam>
internal sealed class RegistryHintVisitor<TQuery> : ICriteriaHintVisitor
    where TQuery : class
{
    private readonly IHintHandlerRegistry registry;

    /// <summary>
    /// Creates a new visitor over the given <paramref name="query"/>.
    /// </summary>
    /// <param name="registry">The hint handler registry.</param>
    /// <param name="query">The query to which hints will be applied.</param>
    public RegistryHintVisitor(IHintHandlerRegistry registry, TQuery query)
    {
        this.registry = registry;
        Query = query;
    }

    /// <summary>
    /// The query after the visited hints have been applied.
    /// </summary>
    public TQuery Query { get; private set; }

    /// <inheritdoc />
    public void Visit<THint>(THint hint) where THint : Enum
    {
        foreach (var handler in registry.GetQueryHandlers<TQuery, THint>())
            Query = handler.Handle(Query, hint);
    }
}
