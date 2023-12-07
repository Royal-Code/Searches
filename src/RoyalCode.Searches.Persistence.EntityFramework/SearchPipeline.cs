using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using RoyalCode.Searches.Abstractions;
using RoyalCode.Searches.Persistence.Abstractions;
using RoyalCode.Searches.Persistence.Abstractions.Pipeline;
using RoyalCode.Searches.Persistence.Linq;
using RoyalCode.Searches.Persistence.Linq.Filter;

namespace RoyalCode.Searches.Persistence.EntityFramework;

/// <summary>
/// <para>
///     Default implementation of <see cref="ISearchPipeline{TModel}"/>.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public sealed class SearchPipeline<TEntity> : SearchPipelineBase<TEntity>, ISearchPipeline<TEntity>
    where TEntity : class
{
    /// <inheritdoc />
    public SearchPipeline(
        IQueryableProvider<TEntity> queryableProvider,
        ISpecifierFactory specifierFactory,
        ISorter<TEntity> sorter)
        : base(
            queryableProvider,
            specifierFactory,
            sorter)
    { }

    /// <inheritdoc />
    public IResultList<TEntity> Execute(SearchCriteria criteria)
    {
        var sortedQuery = PrepareQuery(criteria);

        var executableQuery = criteria.Paginate
            ? sortedQuery
                .Skip(criteria.GetSkipCount())
                .Take(criteria.ItemsPerPage + 1)
            : sortedQuery;

        var list = executableQuery.ToList();
        var hasNextPage = list.Count > criteria.ItemsPerPage;
        var items = hasNextPage && criteria.Paginate ? list.Take(criteria.ItemsPerPage) : list;

        var count = criteria.LastCount > 0
            ? criteria.LastCount
            : criteria.UseCount
                ? hasNextPage
                    ? sortedQuery.Count()
                    : criteria.GetSkipCount() + list.Count
                : 0;

        var pages = CountPages(count, criteria.ItemsPerPage);

        var result = new ResultList<TEntity>()
        {
            Page = criteria.GetPageNumber(),
            Count = count,
            ItemsPerPage = criteria.ItemsPerPage,
            Pages = pages,
            Sortings = criteria.Sortings,
            Projections = [],
            Items = items
        };

        return result;
    }

    /// <inheritdoc />
    public async Task<IResultList<TEntity>> ExecuteAsync(SearchCriteria criteria, CancellationToken token)
    {
        var sortedQuery = PrepareQuery(criteria);

        var executableQuery = criteria.Paginate
            ? sortedQuery
                .Skip(criteria.GetSkipCount())
                .Take(criteria.ItemsPerPage + 1)
            : sortedQuery;

        var list = await executableQuery.ToListAsync(token);
        var hasNextPage = list.Count > criteria.ItemsPerPage;
        var items = hasNextPage && criteria.Paginate ? list.Take(criteria.ItemsPerPage) : list;

        var count = criteria.LastCount > 0
            ? criteria.LastCount
            : criteria.UseCount
                ? hasNextPage
                    ? await sortedQuery.CountAsync(token)
                    : criteria.GetSkipCount() + list.Count
                : 0;

        var pages = CountPages(count, criteria.ItemsPerPage);

        var result = new ResultList<TEntity>()
        {
            Page = criteria.GetPageNumber(),
            Count = count,
            ItemsPerPage = criteria.ItemsPerPage,
            Pages = pages,
            Sortings = criteria.Sortings,
            Projections = [],
            Items = items
        };

        return result;
    }

    /// <inheritdoc />
    public async Task<IAsyncResultList<TEntity>> AsyncExecuteAsync(SearchCriteria criteria, CancellationToken token)
    {
        var sortedQuery = PrepareQuery(criteria);

        var executableQuery = criteria.Paginate
            ? sortedQuery
                .Skip(criteria.GetSkipCount())
                .Take(criteria.ItemsPerPage)
            : sortedQuery;

        var items = executableQuery.AsAsyncEnumerable();

        var count = criteria.LastCount > 0
            ? criteria.LastCount
            : criteria.UseCount
                ? await sortedQuery.CountAsync(token)
                : 0;

        var pages = CountPages(count, criteria.ItemsPerPage);

        var result = new AsyncResultList<TEntity>()
        {
            Page = criteria.GetPageNumber(),
            Count = count,
            ItemsPerPage = criteria.ItemsPerPage,
            Pages = pages,
            Sortings = criteria.Sortings,
            Projections = [],
            Items = items
        };

        return result;
    }
}

/// <summary>
/// <para>
///     Default implementation of <see cref="ISearchPipeline{TModel}"/> that selects a DTO from the entity.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TDto">The selected DTO type.</typeparam>
public sealed class SearchPipeline<TEntity, TDto> : SearchPipelineBase<TEntity>, ISearchPipeline<TDto>
    where TEntity : class
    where TDto : class
{
    private readonly ISelector<TEntity, TDto>? selector;

    /// <inheritdoc />
    public SearchPipeline(
        IQueryableProvider<TEntity> queryableProvider,
        ISpecifierFactory specifierFactory,
        ISorter<TEntity> sorter,
        ISelector<TEntity, TDto>? selector = null)
        : base(
            queryableProvider,
            specifierFactory,
            sorter)
    {
        this.selector = selector;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IQueryable<TDto> Select(IQueryable<TEntity> query, SearchCriteria criteria)
    {
        var selectExpression = (Expression<Func<TEntity, TDto>>?)criteria.Select?.SelectExpression
                               ?? selector?.GetSelectExpression()
                               ?? throw new SelectorNotFoundException(typeof(TEntity), typeof(TDto));

        return query.Select(selectExpression);
    }

    /// <inheritdoc />
    public IResultList<TDto> Execute(SearchCriteria criteria)
    {
        var sortedQuery = PrepareQuery(criteria);
        var selectQuery = Select(sortedQuery, criteria);
        var executableQuery = criteria.Paginate
            ? selectQuery.Skip(criteria.GetSkipCount()).Take(criteria.ItemsPerPage + 1)
            : selectQuery;

        var list = executableQuery.ToList();
        var hasNextPage = list.Count > criteria.ItemsPerPage;
        var items = hasNextPage && criteria.Paginate ? list.Take(criteria.ItemsPerPage) : list;

        var count = criteria.LastCount > 0
            ? criteria.LastCount
            : criteria.UseCount
                ? hasNextPage
                    ? sortedQuery.Count()
                    : criteria.GetSkipCount() + list.Count
                : 0;

        var pages = CountPages(count, criteria.ItemsPerPage);

        var result = new ResultList<TDto>()
        {
            Page = criteria.GetPageNumber(),
            Count = count,
            ItemsPerPage = criteria.ItemsPerPage,
            Pages = pages,
            Sortings = criteria.Sortings,
            Projections = [],
            Items = items
        };

        return result;
    }

    /// <inheritdoc />
    public async Task<IResultList<TDto>> ExecuteAsync(SearchCriteria criteria, CancellationToken token)
    {
        var sortedQuery = PrepareQuery(criteria);
        var selectQuery = Select(sortedQuery, criteria);
        var executableQuery = criteria.Paginate
            ? selectQuery.Skip(criteria.GetSkipCount()).Take(criteria.ItemsPerPage + 1)
            : selectQuery;

        var list = await executableQuery.ToListAsync(token);
        var hasNextPage = list.Count > criteria.ItemsPerPage;
        var items = hasNextPage && criteria.Paginate ? list.Take(criteria.ItemsPerPage) : list;

        var count = criteria.LastCount > 0
            ? criteria.LastCount
            : criteria.UseCount
                ? hasNextPage
                    ? await sortedQuery.CountAsync(token)
                    : criteria.GetSkipCount() + list.Count
                : 0;

        var pages = CountPages(count, criteria.ItemsPerPage);

        var result = new ResultList<TDto>()
        {
            Page = criteria.GetPageNumber(),
            Count = count,
            ItemsPerPage = criteria.ItemsPerPage,
            Pages = pages,
            Sortings = criteria.Sortings,
            Projections = [],
            Items = items
        };

        return result;
    }

    /// <inheritdoc />
    public async Task<IAsyncResultList<TDto>> AsyncExecuteAsync(SearchCriteria criteria, CancellationToken token)
    {
        var sortedQuery = PrepareQuery(criteria);
        var selectQuery = Select(sortedQuery, criteria);
        var executableQuery = criteria.Paginate
            ? selectQuery.Skip(criteria.GetSkipCount()).Take(criteria.ItemsPerPage)
            : selectQuery;

        var items = executableQuery.AsAsyncEnumerable();

        var count = criteria.LastCount > 0
            ? criteria.LastCount
            : criteria.UseCount
                ? await sortedQuery.CountAsync(token)
                : 0;

        var pages = CountPages(count, criteria.ItemsPerPage);

        var result = new AsyncResultList<TDto>()
        {
            Page = criteria.GetPageNumber(),
            Count = count,
            ItemsPerPage = criteria.ItemsPerPage,
            Pages = pages,
            Sortings = criteria.Sortings,
            Projections = [],
            Items = items
        };

        return result;
    }
}