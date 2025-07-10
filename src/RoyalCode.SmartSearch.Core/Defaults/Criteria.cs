
using RoyalCode.SmartSearch.Mappings;
using RoyalCode.SmartSearch.Services;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Defaults;

/// <summary>
/// Default implementation of the <see cref="ICriteria{TEntity}"/> interface.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class Criteria<TEntity> : ICriteria<TEntity>
    where TEntity : class
{
    private readonly ICriteriaPerformer<TEntity> performer;

    /// <summary>
    /// <para>
    ///     Creates a new <see cref="Criteria{TEntity}"/> with the service to perform the criteria.
    /// </para>
    /// </summary>
    /// <param name="performer">
    ///     A service capable of accessing the database and running a query using the Criteria options.
    /// </param>
    public Criteria(ICriteriaPerformer<TEntity> performer)
    {
        this.performer = performer;
    }

    private readonly CriteriaOptions options = new();

    /// <inheritdoc />
    public ICriteria<TEntity> UsePages(int itemsPerPage = 10, int pageNumber = 1)
    {
        options.ItemsPerPage = itemsPerPage;
        options.Page = pageNumber;
        return this;
    }

    /// <inheritdoc />
    public ICriteria<TEntity> FetchPage(int pageNumber)
    {
        options.Page = pageNumber;
        return this;
    }

    /// <inheritdoc />
    public ICriteria<TEntity> Skip(int skip)
    {
        options.Skip = skip;
        return this;
    }

    /// <inheritdoc />
    public ICriteria<TEntity> Take(int take)
    {
        options.Take = take;
        return this;
    }

    /// <inheritdoc />
    public ICriteria<TEntity> SkipTake(int skip, int take)
    {
        options.Skip = skip;
        options.Take = take;
        return this;
    }

    /// <inheritdoc />
    public ICriteria<TEntity> UseLastCount(int lastCount)
    {
        options.LastCount = lastCount;
        options.UseCount = true;
        return this;
    }

    /// <inheritdoc />
    public ICriteria<TEntity> UseCount(bool useCount = true)
    {
        options.UseCount = useCount;
        return this;
    }

    /// <inheritdoc />
    public ICriteria<TEntity> FilterBy<TFilter>(TFilter filter)
        where TFilter : class
    {
        options.AddFilter(filter);
        return this;
    }

    /// <inheritdoc />
    public ICriteria<TEntity> OrderBy(ISorting sorting)
    {
        options.AddSorting(sorting);
        return this;
    }

    /// <inheritdoc />
    public ICriteria<TEntity> OrderBy(ISorting[]? sorting)
    {
        options.AddSorting(sorting);
        return this;
    }

    /// <inheritdoc />
    public ISearch<TEntity> AsSearch()
    {
        return new Search<TEntity>(performer, options);
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
    public IReadOnlyList<TEntity> Collect() => performer.Prepare(options).ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> CollectAsync(CancellationToken cancellationToken = default)
        => await performer.Prepare(options).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public bool Exists() => performer.Prepare(options).Exists();

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        => await performer.Prepare(options).ExistsAsync(cancellationToken);

    /// <inheritdoc />
    public TEntity? FirstOrDefault() => performer.Prepare(options).FirstOrDefault();

    /// <inheritdoc />
    public async Task<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        => await performer.Prepare(options).FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public TEntity Single() => performer.Prepare(options).Single();

    /// <inheritdoc />
    public async Task<TEntity> SingleAsync(CancellationToken cancellationToken = default)
        => await performer.Prepare(options).SingleAsync(cancellationToken);
}
