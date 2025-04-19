using RoyalCode.SmartSearch.Linq.Filter;
using RoyalCode.SmartSearch.Linq.Selector;
using RoyalCode.SmartSearch.Linq.Sorter;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Linq;

/// <summary>
/// <para>
///     Configure search components like specifier, order by and selector.
/// </para>
/// <para>
///     The configurations are global and will be used by all searches.
/// </para>
/// </summary>
public interface ISearchConfigurations
{
    /// <summary>
    /// Add a specifier function for the model and filter.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TFilter">The type of the filter.</typeparam>
    /// <param name="specifier">The specifier function that applies the filter over the model query.</param>
    /// <returns>The same instance of the configuration.</returns>
    ISearchConfigurations AddSpecifier<TModel, TFilter>(
        Func<IQueryable<TModel>, TFilter, IQueryable<TModel>> specifier)
        where TModel : class
        where TFilter : class
    {
        SpecifiersMap.Instance.Add(specifier);
        return this;
    }

    /// <summary>
    /// Add a specifier for the model and filter.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TFilter">The type of the filter.</typeparam>
    /// <param name="specifier">The specifier that applies the filter over the model query.</param>
    /// <returns>The same instance of the configuration.</returns>
    ISearchConfigurations AddSpecifier<TModel, TFilter>(ISpecifier<TModel, TFilter> specifier)
        where TModel : class
        where TFilter : class
    {
        SpecifiersMap.Instance.Add(specifier);
        return this;
    }

    /// <summary>
    /// Configure the options for the specifier generator of the model and filter.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TFilter">The type of the filter.</typeparam>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The same instance of the configuration.</returns>
    ISearchConfigurations ConfigureSpecifierGenerator<TModel, TFilter>(
        Action<ISpecifierGeneratorOptions<TModel, TFilter>> configure)
        where TModel : class
        where TFilter : class
    {
        var options = SpecifierGeneratorOptions.GetOptions<TModel, TFilter>();
        configure(options);
        return this;
    }

    /// <summary>
    /// Add a Order By expression for the model and property (<paramref name="orderBy"/>).
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="orderBy">The name of the property.</param>
    /// <param name="expression">The expression that represents the property.</param>
    /// <returns>The same instance of the configuration.</returns>
    ISearchConfigurations AddOrderBy<TModel, TProperty>(
        string orderBy, Expression<Func<TModel, TProperty>> expression)
        where TModel : class
    {
        OrderByHandlersMap.Instance.Add(orderBy, expression);
        return this;
    }

    /// <summary>
    /// Add a Order By handler for the model and property (<paramref name="orderBy"/>).
    /// </summary>
    ///<typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="orderBy">The name of the property.</param>
    /// <param name="handler">The handler that represents the property.</param>
    /// <returns>The same instance of the configuration.</returns>
    ISearchConfigurations AddOrderBy<TModel, TProperty>(string orderBy, IOrderByHandler<TModel> handler)
        where TModel : class
    {
        OrderByHandlersMap.Instance.Add(orderBy, handler);
        return this;
    }

    /// <summary>
    /// Add a selector function expression for select values from the model to the DTO.
    /// </summary>
    /// <typeparam name="TEntity">The type of the model.</typeparam>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <param name="selector">The selector function.</param>
    /// <returns>The same instance of the configuration.</returns>
    ISearchConfigurations AddSelector<TEntity, TDto>(
        Expression<Func<TEntity, TDto>> selector)
        where TEntity : class
        where TDto : class
    {
        SelectorsMap.Instance.Add(selector);
        return this;
    }

    /// <summary>
    /// Add a selector for select values from the model to the DTO.
    /// </summary>
    /// <typeparam name="TEntity">The type of the model.</typeparam>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <param name="selector">The selector.</param>
    /// <returns>The same instance of the configuration.</returns>
    ISearchConfigurations AddSelector<TEntity, TDto>(
        ISelector<TEntity, TDto> selector)
        where TEntity : class
        where TDto : class
    {
        SelectorsMap.Instance.Add(selector);
        return this;
    }
}

