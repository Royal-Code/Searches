using RoyalCode.SmartSearch.Filtering;
using RoyalCode.SmartSearch.Linq.Services;
using RoyalCode.SmartSearch.Linq.Sortings;
using RoyalCode.SmartSearch.Mappings;
using RoyalCode.SmartSearch.Services;

namespace RoyalCode.SmartSearch.Linq;

public abstract class CriteriaQuery<TEntity> : IPreparedQuery<TEntity>, IFilterHandler
    where TEntity : class
{
    private readonly ISpecifierFactory specifierFactory;
    private readonly IOrderByProvider orderByProvider;
    private readonly ISelectorFactory selectorFactory;

    private IQueryable<TEntity> query;
    private Func<int>? countQuery;

    private int pageNumber;
    private int skip;
    private int take;
    private bool count;
    private int lastCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="CriteriaQuery{TEntity}"/> class, 
    /// providing the necessary components for building and executing queries with specified criteria.
    /// </summary>
    /// <remarks>
    ///     This constructor is designed to facilitate the creation of complex queries by combining
    ///     filtering,  ordering, and projection capabilities. Ensure that all dependencies are properly 
    ///     initialized  before passing them to this constructor.
    /// </remarks>
    /// <param name="queryable">The source queryable representing the data set to query.</param>
    /// <param name="specifierFactory">The factory used to create specifiers for filtering criteria.</param>
    /// <param name="orderByProvider">The provider used to define ordering rules for the query results.</param>
    /// <param name="selectorFactory">The factory used to create selectors for projecting query results.</param>
    protected CriteriaQuery(
        IQueryable<TEntity> queryable,
        ISpecifierFactory specifierFactory,
        IOrderByProvider orderByProvider,
        ISelectorFactory selectorFactory)
    {
        query = queryable;
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

            orderByBuilder.CurrentDirection = sorting.Direction;
            orderByBuilder = handler.Handle(orderByBuilder);
        }
    }

    internal void SetPageSkipTakeCount(int pageNumber, int skip, int take, bool count, int lastCount)
    {
        this.pageNumber = pageNumber;
        this.skip = skip;
        this.take = take;
        this.count = count;
        this.lastCount = lastCount;
    }

    internal void UseCountQuery(Func<int> countQuery)
    {
        this.countQuery = countQuery;
    }

    /// <inheritdoc />
    public IPreparedQuery<TDto> Select<TDto>(ISearchSelect<TEntity, TDto> select)
         where TDto : class
    {
        var expression = select.SelectExpression ?? selectorFactory.Create<TEntity, TDto>().GetSelectExpression();
        var queryable = query.Select(expression);
        var criteriaQuery = CreateCriteriaQuery(queryable, specifierFactory, orderByProvider, selectorFactory);
        criteriaQuery.SetPageSkipTakeCount(pageNumber, skip, take, count, lastCount);
        criteriaQuery.UseCountQuery(query.Count);
        return criteriaQuery;
    }

    /// <summary>
    /// Creates a criteria query for filtering, ordering, and selecting data from the specified queryable source.
    /// </summary>
    /// <typeparam name="TDto">The type of the data transfer object (DTO) being queried. Must be a reference type.</typeparam>
    /// <param name="queryable">The queryable source to apply criteria to. Cannot be null.</param>
    /// <param name="specifierFactory">The factory used to create specifiers for filtering the query. Cannot be null.</param>
    /// <param name="orderByProvider">The provider used to define ordering rules for the query. Cannot be null.</param>
    /// <param name="selectorFactory">The factory used to create selectors for projecting the query results. Cannot be null.</param>
    /// <returns>A <see cref="CriteriaQuery{TDto}"/> representing the configured query.</returns>
    protected abstract CriteriaQuery<TDto> CreateCriteriaQuery<TDto>(
        IQueryable<TDto> queryable,
        ISpecifierFactory specifierFactory,
        IOrderByProvider orderByProvider,
        ISelectorFactory selectorFactory)
        where TDto : class;

    
    protected IQueryable<TEntity> GetQueryableWithSkip()
    {
        var queryable = query;
        if (skip > 0)
            queryable = queryable.Skip(skip);
        return queryable;
    }

    protected IQueryable<TEntity> GetQueryableWithSkipAndTake(bool count)
    {
        var queryable = GetQueryableWithSkip();
        if (take > 0)
            queryable = queryable.Take(take + (count ? 1 : 0));
        return queryable;
    }

    /// <inheritdoc />
    public bool Exists() => GetQueryableWithSkip().Any();

    /// <inheritdoc />
    public abstract Task<bool> ExistsAsync(CancellationToken ct = default);

    /// <inheritdoc />
    public TEntity? FirstOrDefault()
    {
        return GetQueryableWithSkip().FirstOrDefault();
    }

    /// <inheritdoc />
    public abstract Task<TEntity?> FirstOrDefaultAsync(CancellationToken ct = default);

    /// <inheritdoc />
    public TEntity Single()
    {
        var entities = GetQueryableWithSkip().Take(2).ToList();
        if (entities.Count == 0)
            throw new InvalidOperationException("No entity found matching the criteria.");
        if (entities.Count > 1)
            throw new InvalidOperationException("More than one entity found matching the criteria.");
        return entities[0];
    }

    /// <inheritdoc />
    public abstract Task<TEntity> SingleAsync(CancellationToken ct = default);

    public IReadOnlyList<TEntity> ToList() => GetQueryableWithSkipAndTake(false).ToList();

    /// <inheritdoc />
    public abstract Task<IReadOnlyList<TEntity>> ToListAsync(CancellationToken ct);

    /// <inheritdoc />
    public IResultList<TEntity> ToResultList()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IResultList<TEntity>> ToResultListAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IAsyncResultList<TEntity>> ToAsyncListAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
