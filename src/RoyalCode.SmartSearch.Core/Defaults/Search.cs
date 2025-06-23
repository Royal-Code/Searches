
using RoyalCode.SmartSearch.Mappings;
using RoyalCode.SmartSearch.Services;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Defaults;

/// <summary>
/// Default implementation of <see cref="ISearch{TEntity}"/>.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class Search<TEntity> : ISearch<TEntity>
    where TEntity : class
{
    private readonly ICriteriaPerformer<TEntity> performer;
    private readonly CriteriaOptions options;

    /// <summary>
    /// Creates a new search.
    /// </summary>
    /// <param name="performer"></param>
    /// <param name="options"></param>
    public Search(ICriteriaPerformer<TEntity> performer, CriteriaOptions options)
    {
        this.performer = performer;
        this.options = options;
        options.NoTracking();
    }

    /// <inheritdoc />
    public ISearch<TEntity, TDto> Select<TDto>()
        where TDto : class
    {
        var select = new SearchSelect<TEntity, TDto>();
        return new Search<TEntity, TDto>(performer, options, select);
    }

    /// <inheritdoc />
    public ISearch<TEntity, TDto> Select<TDto>(Expression<Func<TEntity, TDto>> selectExpression)
        where TDto : class
    {
        var select = new SearchSelect<TEntity, TDto>(selectExpression);
        return new Search<TEntity, TDto>(performer, options, select);
    }

    /// <inheritdoc />
    public IResultList<TEntity> ToList() => performer.Prepare(options).ToResultList();

    /// <inheritdoc />
    public Task<IResultList<TEntity>> ToListAsync(CancellationToken token)
        => performer.Prepare(options).ToResultListAsync(token);

    /// <inheritdoc />
    public Task<IAsyncResultList<TEntity>> ToAsyncListAsync(CancellationToken token)
        => performer.Prepare(options).ToAsyncListAsync(token);

    /// <inheritdoc />
    public TEntity? FirstOrDefault() => performer.Prepare(options).FirstOrDefault();

    /// <inheritdoc />
    public Task<TEntity?> FirstDefaultAsync(CancellationToken cancellationToken = default)
        => performer.Prepare(options).FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public TEntity Single() => performer.Prepare(options).Single();

    /// <inheritdoc />
    public Task<TEntity> SingleAsync(CancellationToken cancellationToken = default)
        => performer.Prepare(options).SingleAsync(cancellationToken);
}

public class Search<TEntity, TDto> : ISearch<TEntity, TDto>
    where TEntity : class
    where TDto : class
{
    private readonly ICriteriaPerformer<TEntity> performer;
    private readonly CriteriaOptions options;
    private readonly ISearchSelect<TEntity, TDto> searchSelect;

    public Search(
        ICriteriaPerformer<TEntity> performer,
        CriteriaOptions options,
        ISearchSelect<TEntity, TDto> searchSelect)
    {
        this.performer = performer;
        this.options = options;
        this.searchSelect = searchSelect;
        options.NoTracking();
    }

    /// <inheritdoc />
    public IResultList<TDto> ToList() => performer.Prepare(options).Select(searchSelect).ToResultList();

    /// <inheritdoc />
    public Task<IResultList<TDto>> ToListAsync(CancellationToken token)
        => performer.Prepare(options).Select(searchSelect).ToResultListAsync(token);

    /// <inheritdoc />
    public Task<IAsyncResultList<TDto>> ToAsyncListAsync(CancellationToken token)
        => performer.Prepare(options).Select(searchSelect).ToAsyncListAsync(token);

    /// <inheritdoc />
    public TDto? FirstOrDefault() => performer.Prepare(options).Select(searchSelect).FirstOrDefault();

    /// <inheritdoc />
    public Task<TDto?> FirstDefaultAsync(CancellationToken cancellationToken = default)
        => performer.Prepare(options).Select(searchSelect).FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public TDto Single() => performer.Prepare(options).Select(searchSelect).Single();

    /// <inheritdoc />
    public Task<TDto> SingleAsync(CancellationToken cancellationToken = default)
        => performer.Prepare(options).Select(searchSelect).SingleAsync(cancellationToken);
}