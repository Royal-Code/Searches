using System.Linq.Expressions;
using RoyalCode.Searches.Abstractions;

namespace RoyalCode.Searches.Core;

/// <inheritdoc />
public abstract class SearchBase<TEntity> : ISearch<TEntity>
    where TEntity : class
{
    /// <summary>
    /// The criteria for performing the search.
    /// </summary>
    protected readonly SearchCriteria criteria = new();

    /// <inheritdoc />
    public ISearch<TEntity> UsePages(int itemsPerPage = 10)
    {
        criteria.ItemsPerPage = itemsPerPage;
        return this;
    }

    /// <inheritdoc />
    public ISearch<TEntity> FetchPage(int pageNumber)
    {
        criteria.Page = pageNumber;
        return this;
    }

    /// <inheritdoc />
    public ISearch<TEntity> UseLastCount(int lastCount)
    {
        criteria.LastCount = lastCount;
        return this;
    }

    /// <inheritdoc />
    public ISearch<TEntity> UseCount(bool useCount = true)
    {
        criteria.UseCount = useCount;
        return this;
    }

    /// <inheritdoc />
    public ISearch<TEntity> FilterBy<TFilter>(TFilter filter)
        where TFilter : class
    {
        criteria.AddFilter(typeof(TEntity), filter);
        return this;
    }

    /// <inheritdoc />
    public ISearch<TEntity> OrderBy(ISorting sorting)
    {
        criteria.AddSorting(sorting);
        return this;
    }

    /// <inheritdoc />
    public abstract ISearch<TEntity, TDto> Select<TDto>()
        where TDto : class;

    /// <inheritdoc />
    public abstract ISearch<TEntity, TDto> Select<TDto>(Expression<Func<TEntity, TDto>> selectExpression)
        where TDto : class;

    /// <inheritdoc />
    public abstract IResultList<TEntity> ToList();

    /// <inheritdoc />
    public abstract Task<IResultList<TEntity>> ToListAsync(CancellationToken token);

    /// <inheritdoc />
    public abstract Task<IAsyncResultList<TEntity>> ToAsyncListAsync(CancellationToken token);
}

/// <inheritdoc />
public abstract class SearchBase<TEntity, TDto> : ISearch<TEntity, TDto>
    where TEntity : class
    where TDto : class
{
    /// <summary>
    /// The criteria for performing the search.
    /// </summary>
    protected readonly SearchCriteria criteria;

    /// <summary>
    /// Initialize the base criteria with the <see cref="SearchCriteria"/>.
    /// </summary>
    /// <param name="criteria"></param>
    protected SearchBase(SearchCriteria criteria)
    {
        this.criteria = criteria;
    }

    /// <inheritdoc />
    public ISearch<TEntity, TDto> UsePages(int itemsPerPage = 10)
    {
        criteria.ItemsPerPage = itemsPerPage;
        return this;
    }

    /// <inheritdoc />
    public ISearch<TEntity, TDto> FetchPage(int pageNumber)
    {
        criteria.Page = pageNumber;
        return this;
    }

    /// <inheritdoc />
    public ISearch<TEntity, TDto> UseLastCount(int lastCount)
    {
        criteria.LastCount = lastCount;
        return this;
    }

    /// <inheritdoc />
    public ISearch<TEntity, TDto> UseCount(bool useCount = true)
    {
        criteria.UseCount = useCount;
        return this;
    }

    /// <inheritdoc />
    public ISearch<TEntity, TDto> FilterBy<TFilter>(TFilter filter)
        where TFilter : class
    {
        criteria.AddFilter(typeof(TDto), filter);
        return this;
    }

    /// <inheritdoc />
    public abstract IResultList<TDto> ToList();

    /// <inheritdoc />
    public abstract Task<IResultList<TDto>> ToListAsync(CancellationToken token);

    /// <inheritdoc />
    public abstract Task<IAsyncResultList<TDto>> ToAsyncListAsync(CancellationToken token);
}