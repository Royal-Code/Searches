using Microsoft.EntityFrameworkCore;
using RoyalCode.OperationHint.Abstractions;
using RoyalCode.SmartSearch.Filtering;
using RoyalCode.SmartSearch.Hints;
using RoyalCode.SmartSearch.Linq.Services;
using RoyalCode.SmartSearch.Linq.Sortings;
using RoyalCode.SmartSearch.Mappings;
using RoyalCode.SmartSearch.Services;
using System.Runtime.CompilerServices;

#pragma warning disable S3358 // ifs ternaries should not be nested

namespace RoyalCode.SmartSearch.EntityFramework.Services;

/// <summary>
/// <para>
///     Represents a query builder for constructing and executing queries 
///     with filtering, sorting,  pagination, and projection capabilities for a specified entity type.
/// </para>
/// </summary>
/// <remarks>
/// <para>
///     The <see cref="CriteriaQuery{TEntity}"/> class provides a flexible and extensible way to build 
///     queries by combining filtering criteria, sorting rules, pagination settings, and projection  expressions.
///     <br />
///     It is designed to work with LINQ and supports both synchronous and asynchronous query execution.
///     <br />    
///     This class is particularly useful for scenarios where dynamic query construction is required,
///     such as implementing search functionality or handling complex data retrieval requirements.
/// </para>
/// <para>    
///     Thread safety: Instances of this class are not guaranteed to be thread-safe. 
///     If multiple threads need to access the same instance, synchronization mechanisms should be used.
/// </para>
/// </remarks>
/// <typeparam name="TEntity">The type of the entity being queried. Must be a reference type.</typeparam>
public class CriteriaQuery<TEntity> : IPreparedQuery<TEntity>, IFilterHandler
    where TEntity : class
{
    private readonly ISpecifierFactory specifierFactory;
    private readonly IOrderByProvider orderByProvider;
    private readonly ISelectorFactory selectorFactory;
    private readonly IHintPerformer? hintPerformer;
    private readonly IHintHandlerRegistry? hintRegistry;
    private readonly IReadOnlyList<ICriteriaHint>? localHints;

    private IQueryable<TEntity> query;
    private IQueryable<TEntity>? hintedQuery;

    private int pageNumber;
    private int skip;
    private int take;
    private bool count;
    private int lastCount;

    private readonly List<ISorting> appliedSorting = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="CriteriaQuery{TEntity}"/> class, 
    /// providing the necessary components for building and executing queries with specified criteria.
    /// </summary>
    /// <remarks>
    ///     This constructor is designed to facilitate the creation of complex queries by combining
    ///     filtering,  ordering, and projection capabilities. Ensure that all dependencies are properly 
    ///     initialized  before passing them to this constructor.
    /// </remarks>
    /// <param name="query">The initial queryable instance representing the data source for the entity type.</param>
    /// <param name="specifierFactory">The factory used to create specifiers for filtering criteria.</param>
    /// <param name="orderByProvider">The provider used to define ordering rules for the query results.</param>
    /// <param name="selectorFactory">The factory used to create selectors for projecting query results.</param>
    /// <param name="hintPerformer">
    ///     The optional operation hint performer. When provided (i.e. <c>OperationHint</c> is registered),
    ///     ambient hints are applied to the query at the entity-materializing terminals. When <see langword="null"/>,
    ///     the query behaves exactly as before (no-op).
    /// </param>
    /// <param name="hintRegistry">
    ///     The optional hint handler registry, used to apply per-query (local) hints. When <see langword="null"/>
    ///     (or there are no local hints), no local hint is applied.
    /// </param>
    /// <param name="localHints">
    ///     The optional per-query hints declared via <c>ICriteria.UseHints</c>. Applied alongside the ambient hints
    ///     at the entity-materializing terminals, but isolated to this query (never via the ambient container).
    /// </param>
    public CriteriaQuery(
        IQueryable<TEntity> query,
        ISpecifierFactory specifierFactory,
        IOrderByProvider orderByProvider,
        ISelectorFactory selectorFactory,
        IHintPerformer? hintPerformer = null,
        IHintHandlerRegistry? hintRegistry = null,
        IReadOnlyList<ICriteriaHint>? localHints = null)
    {
        this.query = query;
        this.specifierFactory = specifierFactory;
        this.orderByProvider = orderByProvider;
        this.selectorFactory = selectorFactory;
        this.hintPerformer = hintPerformer;
        this.hintRegistry = hintRegistry;
        this.localHints = localHints;
    }

    /// <inheritdoc />
    public void Specify<TFilter>(TFilter filter)
        where TFilter : class
    {
        var specifier = specifierFactory.GetSpecifier<TEntity, TFilter>();
        query = specifier.Specify(query, filter);
    }

    internal void OrderBy(IReadOnlyList<ISorting> sortings)
    {
        var orderByBuilder = new OrderByBuilder<TEntity>(query);

        foreach (var sorting in sortings)
        {
            var handler = orderByProvider.GetHandler<TEntity>(sorting.OrderBy);
            if (handler is null)
                continue;

            appliedSorting.Add(sorting);

            orderByBuilder.CurrentDirection = sorting.Direction;
            orderByBuilder = handler.Handle(orderByBuilder);
        }

        query = orderByBuilder.OrderedQueryable;

        CheckSorting();
    }

    internal void SetPageSkipTakeCount(int pageNumber, int skip, int take, bool count, int lastCount)
    {
        this.pageNumber = pageNumber;
        this.skip = skip;
        this.take = take;
        this.count = count;
        this.lastCount = lastCount;

        CheckSorting();
    }

    /// <inheritdoc />
    public IPreparedQuery<TDto> Select<TDto>(ISearchSelect<TEntity, TDto> select)
         where TDto : class
    {
        var expression = select.SelectExpression ?? selectorFactory.Create<TEntity, TDto>().GetSelectExpression();
        var queryable = query.Select(expression);
        var criteriaQuery = new CriteriaQuery<TDto>(
            queryable,
            specifierFactory,
            orderByProvider,
            selectorFactory);

        // A ordenacao ja foi aplicada a `query` antes do Select e permanece embutida na consulta projetada.
        // Propaga o estado de ordenacao para a query projetada; sem isso, CheckSorting veria appliedSorting vazio
        // e (quando ha paginacao) aplicaria a ordenacao default (ex.: Id) por cima da projecao, sobrescrevendo a
        // ordem real. Tambem garante que o resultado reporte os Sortings efetivamente aplicados.
        criteriaQuery.appliedSorting.AddRange(appliedSorting);
        criteriaQuery.SetPageSkipTakeCount(pageNumber, skip, take, count, lastCount);
        return criteriaQuery;
    }

    private void CheckSorting()
    {
        if (appliedSorting.Count is 0 && (skip > 0 || take > 0))
        {
            var orderByBuilder = new OrderByBuilder<TEntity>(query);
            var handler = orderByProvider.GetDefaultHandler<TEntity>();
            orderByBuilder = handler.Handle(orderByBuilder);
            query = orderByBuilder.OrderedQueryable;
            appliedSorting.Add(DefaultSorting.Instance);
        }
    }

    /// <summary>
    ///     Returns the base query with operation hints applied (e.g. EF includes): both ambient hints (from the
    ///     hint container, via <see cref="IHintPerformer"/>) and per-query hints (declared via
    ///     <c>ICriteria.UseHints</c>, applied through the registry visitor).
    /// </summary>
    /// <remarks>
    ///     The hints are applied <b>once</b> and cached (idempotent). When neither source is available, the original
    ///     query is returned unchanged. This is only used by the entity-materializing terminals;
    ///     <see cref="Exists()"/> and the record counting must not apply hints.
    /// </remarks>
    private IQueryable<TEntity> GetEntityQuery()
    {
        if (hintedQuery is not null)
            return hintedQuery;

        var entityQuery = hintPerformer is null ? query : hintPerformer.Perform(query);

        if (hintRegistry is not null && localHints is { Count: > 0 })
        {
            var visitor = new RegistryHintVisitor<IQueryable<TEntity>>(hintRegistry, entityQuery);
            foreach (var hint in localHints)
                hint.Accept(visitor);
            entityQuery = visitor.Query;
        }

        return hintedQuery = entityQuery;
    }

    private IQueryable<TEntity> GetQueryableWithSkip(bool applyHints = false)
    {
        var queryable = applyHints ? GetEntityQuery() : query;
        if (skip > 0)
            queryable = queryable.Skip(skip);
        return queryable;
    }

    private IQueryable<TEntity> GetQueryableWithSkipAndTake(bool count, bool applyHints = false)
    {
        var queryable = GetQueryableWithSkip(applyHints);
        if (take > 0)
            queryable = queryable.Take(take + (count ? 1 : 0));
        return queryable;
    }

    private int CountPages(int count)
    {
        return count == 0 || take < 1
            ? 0
            : (int)Math.Ceiling((double)count / take);
    }

    /// <inheritdoc />
    public bool Exists()
        => GetQueryableWithSkip().Any();

    /// <inheritdoc />
    public Task<bool> ExistsAsync(CancellationToken ct = default) 
        => GetQueryableWithSkip().AnyAsync(ct);

    /// <inheritdoc />
    public TEntity? FirstOrDefault()
        => GetQueryableWithSkip(applyHints: true).FirstOrDefault();

    /// <inheritdoc />
    public Task<TEntity?> FirstOrDefaultAsync(CancellationToken ct = default)
        => GetQueryableWithSkip(applyHints: true).FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public TEntity Single()
        => GetQueryableWithSkip(applyHints: true).Single();

    /// <inheritdoc />
    public Task<TEntity> SingleAsync(CancellationToken ct = default)
        => GetQueryableWithSkip(applyHints: true).SingleAsync(ct);

    /// <inheritdoc />
    public IReadOnlyList<TEntity> ToList() => GetQueryableWithSkipAndTake(false, applyHints: true).ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> ToListAsync(CancellationToken ct)
        => await GetQueryableWithSkipAndTake(false, applyHints: true).ToListAsync(ct);

    /// <inheritdoc />
    public IResultList<TEntity> ToResultList()
    {
        var executableQuery = GetQueryableWithSkipAndTake(true, applyHints: true);

        var items = executableQuery.ToList();
        var hasNextPage = take > 0 && items.Count > take;
        if (hasNextPage)
            items = items.Take(take).ToList();

        var queryCount = lastCount > 0
            ? lastCount
            : count
                ? hasNextPage
                    ? query.Count()
                    : skip + items.Count
                : 0;

        var pages = CountPages(queryCount);

        var result = new ResultList<TEntity>()
        {
            Page = pageNumber,
            Count = queryCount,
            ItemsPerPage = take,
            Pages = pages,
            Skipped = skip,
            Taken = hasNextPage ? take : items.Count,
            Sortings = appliedSorting,
            Items = items
        };

        return result;
    }

    /// <inheritdoc />
    public async Task<IResultList<TEntity>> ToResultListAsync(CancellationToken ct)
    {
        var executableQuery = GetQueryableWithSkipAndTake(true, applyHints: true);

        var items = await executableQuery.ToListAsync(ct);
        var hasNextPage = take > 0 && items.Count > take;
        if (hasNextPage)
            items = items.Take(take).ToList();

        var queryCount = lastCount > 0
            ? lastCount
            : count
                ? hasNextPage
                    ? await query.CountAsync(ct)
                    : skip + items.Count
                : 0;

        var pages = CountPages(queryCount);

        var result = new ResultList<TEntity>()
        {
            Page = pageNumber,
            Count = queryCount,
            ItemsPerPage = take,
            Pages = pages,
            Skipped = skip,
            Taken = hasNextPage ? take : items.Count,
            Sortings = appliedSorting,
            Items = items
        };

        return result;
    }

    /// <inheritdoc />
    public async Task<IAsyncResultList<TEntity>> ToAsyncListAsync(CancellationToken ct)
    {
        var executableQuery = GetQueryableWithSkipAndTake(false, applyHints: true);

        var queryCount = lastCount > 0
            ? lastCount
            : count
                ? await query.CountAsync(ct)
                : 0;

        var items = executableQuery.AsAsyncEnumerable();

        var pages = CountPages(queryCount);

        var result = new AsyncResultList<TEntity>()
        {
            Page = pageNumber,
            Count = queryCount,
            ItemsPerPage = take,
            Pages = pages,
            Skipped = skip,
            Taken = take,
            Sortings = appliedSorting,
            Items = items
        };

        return result;
    }
}
