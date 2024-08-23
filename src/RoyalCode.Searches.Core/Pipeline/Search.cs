using System.Linq.Expressions;
using RoyalCode.Searches.Abstractions;

namespace RoyalCode.Searches.Core.Pipeline;

/// <inheritdoc />
public class Search<TEntity> : SearchBase<TEntity>
    where TEntity : class
{
    private readonly IPipelineFactory factory;

    /// <summary>
    /// Creates a new search with the <see cref="IPipelineFactory"/> to execute the query.
    /// </summary>
    /// <param name="factory">A search pipeline factory.</param>
    public Search(IPipelineFactory factory)
    {
        this.factory = factory;
    }

    /// <inheritdoc />
    public override ISearch<TEntity, TDto> Select<TDto>()
    {
        return new Search<TEntity, TDto>(factory, criteria);
    }

    /// <inheritdoc />
    public override ISearch<TEntity, TDto> Select<TDto>(Expression<Func<TEntity, TDto>> selectExpression)
    {
        criteria.SetSelectExpression(selectExpression);
        return new Search<TEntity, TDto>(factory, criteria);
    }

    /// <inheritdoc />
    public override IResultList<TEntity> ToList()
    {
        var pipeline = factory.Create<TEntity>();
        return pipeline.Execute(criteria);
    }

    /// <inheritdoc />
    public override Task<IResultList<TEntity>> ToListAsync(CancellationToken token)
    {
        var pipeline = factory.Create<TEntity>();
        return pipeline.ExecuteAsync(criteria, token);
    }

    /// <inheritdoc />
    public override Task<IAsyncResultList<TEntity>> ToAsyncListAsync(CancellationToken token)
    {
        var pipeline = factory.Create<TEntity>();
        return pipeline.AsyncExecuteAsync(criteria, token);
    }
}

/// <inheritdoc />
public class Search<TEntity, TDto> : SearchBase<TEntity, TDto>
    where TEntity : class
    where TDto : class
{
    private readonly IPipelineFactory factory;

    /// <summary>
    /// Creates a new search.
    /// </summary>
    /// <param name="factory">The pipeline factory for create the search pipeline.</param>
    /// <param name="criteria">The previous criteria.</param>
    public Search(IPipelineFactory factory, SearchCriteria criteria) : base(criteria)
    {
        this.factory = factory;
    }

    /// <inheritdoc />
    public override IResultList<TDto> ToList()
    {
        var pipeline = factory.Create<TEntity, TDto>();
        return pipeline.Execute(criteria);
    }

    /// <inheritdoc />
    public override Task<IResultList<TDto>> ToListAsync(CancellationToken token)
    {
        var pipeline = factory.Create<TEntity, TDto>();
        return pipeline.ExecuteAsync(criteria, token);
    }

    /// <inheritdoc />
    public override Task<IAsyncResultList<TDto>> ToAsyncListAsync(CancellationToken token)
    {
        var pipeline = factory.Create<TEntity, TDto>();
        return pipeline.AsyncExecuteAsync(criteria, token);
    }
}