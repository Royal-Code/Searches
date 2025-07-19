using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Filtering;
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

    private IQueryable<TEntity> query;

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
    public CriteriaQuery(
        IQueryable<TEntity> query,
        ISpecifierFactory specifierFactory,
        IOrderByProvider orderByProvider,
        ISelectorFactory selectorFactory)
    {
        this.query = query;
        this.specifierFactory = specifierFactory;
        this.orderByProvider = orderByProvider;
        this.selectorFactory = selectorFactory;
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

        criteriaQuery.SetPageSkipTakeCount(pageNumber, skip, take, count, lastCount);
        return criteriaQuery;
    }

    private void CheckSorting()
    {
        if (appliedSorting.Count is 0 && skip > 0)
        {
            var orderByBuilder = new OrderByBuilder<TEntity>(query);
            var handler = orderByProvider.GetDefaultHandler<TEntity>();
            orderByBuilder = handler.Handle(orderByBuilder);
            query = orderByBuilder.OrderedQueryable;
            appliedSorting.Add(DefaultSorting.Instance);
        }
    }

    private IQueryable<TEntity> GetQueryableWithSkip()
    {
        var queryable = query;
        if (skip > 0)
            queryable = queryable.Skip(skip);
        return queryable;
    }

    private IQueryable<TEntity> GetQueryableWithSkipAndTake(bool count)
    {
        var queryable = GetQueryableWithSkip();
        if (take > 0)
            queryable = queryable.Take(take + (count ? 1 : 0));
        return queryable;
    }

    private int CountPages(int count)
    {
        return count == 0 || take < 1
            ? 0
            : (int)Math.Floor((double)count / take);
    }

    /// <inheritdoc />
    public bool Exists()
        => GetQueryableWithSkip().Any();

    /// <inheritdoc />
    public Task<bool> ExistsAsync(CancellationToken ct = default) 
        => GetQueryableWithSkip().AnyAsync(ct);

    /// <inheritdoc />
    public TEntity? FirstOrDefault()
        => GetQueryableWithSkip().FirstOrDefault();

    /// <inheritdoc />
    public Task<TEntity?> FirstOrDefaultAsync(CancellationToken ct = default) 
        => GetQueryableWithSkip().FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public TEntity Single()
        => GetQueryableWithSkip().Single();
    
    /// <inheritdoc />
    public Task<TEntity> SingleAsync(CancellationToken ct = default)
        => GetQueryableWithSkip().SingleAsync(ct);

    /// <inheritdoc />
    public IReadOnlyList<TEntity> ToList() => GetQueryableWithSkipAndTake(false).ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> ToListAsync(CancellationToken ct)
        => await GetQueryableWithSkipAndTake(false).ToListAsync(ct);

    /// <inheritdoc />
    public IResultList<TEntity> ToResultList()
    {
        var executableQuery = GetQueryableWithSkipAndTake(true);

        var items = executableQuery.ToList();
        var hasNextPage = items.Count > take;
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
        var executableQuery = GetQueryableWithSkipAndTake(true);

        var items = await executableQuery.ToListAsync(ct);
        var hasNextPage = items.Count > take;
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
        var executableQuery = GetQueryableWithSkipAndTake(false);

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
