using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Abstractions;

/// <summary>
/// Extension methods for <see cref="IAllEntities{TEntity}"/>.
/// </summary>
public static class AllEntitiesExtensions
{
    /// <summary>
    /// Apply the filters and sorting and update all entities that meet the criteria.
    /// </summary>
    /// <param name="allEntities"></param>
    /// <param name="data"></param>
    /// <param name="updateAction"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public static void UpdateAll<TEntity, TData>(
        this IAllEntities<TEntity> allEntities,
        ICollection<TData> data,
        Action<TEntity, TData> updateAction)
        where TEntity : class
        where TData : class
    {
        UpdateAllWithHelper<TEntity, TData>.Execute(allEntities, data, updateAction);
    }
    
    /// <summary>
    /// Apply the filters and sorting and update all entities that meet the criteria.
    /// </summary>
    /// <param name="allEntities"></param>
    /// <param name="data"></param>
    /// <param name="updateAction"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TData"></typeparam>
    /// <returns></returns>
    public static Task UpdateAllWith<TEntity, TData>(
        this IAllEntities<TEntity> allEntities,
        ICollection<TData> data,
        Action<TEntity, TData> updateAction)
        where TEntity : class
        where TData : class
    {
        return UpdateAllWithHelper<TEntity, TData>.ExecuteAsync(allEntities, data, updateAction);
    }
    
    private static class UpdateAllWithHelper<TEntity, TData>
        where TEntity : class
        where TData : class
    {
        private static Action<IAllEntities<TEntity>, ICollection<TData>, Action<TEntity, TData>>? execute;
        private static Func<IAllEntities<TEntity>, ICollection<TData>, Action<TEntity, TData>, Task>? executeAsync;
        
        public static void Execute(
            IAllEntities<TEntity> allEntities,
            ICollection<TData> data,
            Action<TEntity, TData> updateAction)
        {
            execute ??= Create();
            execute(allEntities, data, updateAction);
        }
        
        public static Task ExecuteAsync(
            IAllEntities<TEntity> allEntities,
            ICollection<TData> data,
            Action<TEntity, TData> updateAction)
        {
            executeAsync ??= CreateAsync();
            return executeAsync(allEntities, data, updateAction);
        }

        private static Action<IAllEntities<TEntity>, ICollection<TData>, Action<TEntity,TData>> Create()
        {
            // get the property Id from TEntity
            var idProperty = typeof(TEntity).GetProperty("Id")
                ?? throw new InvalidOperationException("The entity does not have a property Id.");

            // get the property Id from TData
            var dataIdProperty = typeof(TData).GetProperty("Id")
                ?? throw new InvalidOperationException("The data does not have a property Id.");

            // validate types of the id properties
            if (idProperty.PropertyType != dataIdProperty.PropertyType)
                throw new InvalidOperationException("The type of the id properties are different.");
            
            // create a function expression to get the id from TEntity
            var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
            var lambdaType = typeof(Func<,>).MakeGenericType(typeof(TEntity), idProperty.PropertyType);
            var entityLambda = Expression.Lambda(lambdaType,
                Expression.Property(entityParameter, idProperty),
                entityParameter);
            
            // create a function expression to get the id from TData
            var dataParameter = Expression.Parameter(typeof(TData), "data");
            var dataLambda = Expression.Lambda(lambdaType,
                Expression.Property(dataParameter, dataIdProperty),
                dataParameter);
            
            // create expression paramters for allEntities, data, and updateAction
            var allEntitiesParameter = Expression.Parameter(typeof(IAllEntities<TEntity>), "allEntities");
            dataParameter = Expression.Parameter(typeof(ICollection<TData>), "data");
            var updateActionParameter = Expression.Parameter(typeof(Action<TEntity, TData>), "updateAction");
            
            // create a call to the UpdateAll method
            var updateAllMethod = typeof(IAllEntities<TEntity>).GetMethods()
                .First(m => m.Name == nameof(IAllEntities<object>.UpdateAll) && m.GetParameters().Length == 4);
            var updateAllCall = Expression.Call(
                allEntitiesParameter,
                updateAllMethod,
                dataParameter,
                entityLambda,
                dataLambda,
                updateActionParameter);
            
            // create a lambda expression
            var lambda = Expression.Lambda<Action<IAllEntities<TEntity>, ICollection<TData>, Action<TEntity, TData>>>(
                updateAllCall,
                allEntitiesParameter,
                dataParameter,
                updateActionParameter);
            
            // compile the lambda expression
            return lambda.Compile();
        }
        
        private static Func<IAllEntities<TEntity>,ICollection<TData>,Action<TEntity,TData>,Task> CreateAsync()
        {
            // get the property Id from TEntity
            var idProperty = typeof(TEntity).GetProperty("Id")
                ?? throw new InvalidOperationException("The entity does not have a property Id.");

            // get the property Id from TData
            var dataIdProperty = typeof(TData).GetProperty("Id")
                ?? throw new InvalidOperationException("The data does not have a property Id.");

            // validate types of the id properties
            if (idProperty.PropertyType != dataIdProperty.PropertyType)
                throw new InvalidOperationException("The type of the id properties are different.");
            
            // create a function expression to get the id from TEntity
            var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
            var lambdaType = typeof(Func<,>).MakeGenericType(typeof(TEntity), idProperty.PropertyType);
            var entityLambda = Expression.Lambda(lambdaType,
                Expression.Property(entityParameter, idProperty),
                entityParameter);
            
            // create a function expression to get the id from TData
            var dataParameter = Expression.Parameter(typeof(TData), "data");
            var dataLambda = Expression.Lambda(lambdaType,
                Expression.Property(dataParameter, dataIdProperty),
                dataParameter);
            
            // create expression paramters for allEntities, data, and updateAction
            var allEntitiesParameter = Expression.Parameter(typeof(IAllEntities<TEntity>), "allEntities");
            dataParameter = Expression.Parameter(typeof(ICollection<TData>), "data");
            var updateActionParameter = Expression.Parameter(typeof(Action<TEntity, TData>), "updateAction");
            
            // create a call to the UpdateAllAsync method
            var updateAllMethod = typeof(IAllEntities<TEntity>).GetMethods()
                .First(m => m.Name == nameof(IAllEntities<object>.UpdateAllAsync) && m.GetParameters().Length == 5);
            var updateAllCall = Expression.Call(
                allEntitiesParameter,
                updateAllMethod,
                dataParameter,
                entityLambda,
                dataLambda,
                updateActionParameter);
            
            // create a lambda expression
            var lambda = Expression.Lambda<Func<IAllEntities<TEntity>,ICollection<TData>,Action<TEntity,TData>,Task>>(
                updateAllCall,
                allEntitiesParameter,
                dataParameter,
                updateActionParameter);
            
            // compile the lambda expression
            return lambda.Compile();
        }
    }
}