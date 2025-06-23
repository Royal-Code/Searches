using RoyalCode.SmartSearch.Mappings;

namespace RoyalCode.SmartSearch.Services;

public interface IPreparedQuery<T> 
    where T : class
{
    bool Exists();

    Task<bool> ExistsAsync(CancellationToken ct = default);

    T? FirstOrDefault();

    Task<T?> FirstOrDefaultAsync(CancellationToken ct = default);

    T Single();

    Task<T> SingleAsync(CancellationToken ct = default);

    IReadOnlyList<T> ToList();

    Task<IReadOnlyList<T>> ToListAsync(CancellationToken ct);

    IResultList<T> ToResultList();

    Task<IResultList<T>> ToResultListAsync(CancellationToken ct);

    Task<IAsyncResultList<T>> ToAsyncListAsync(CancellationToken ct);

    IPreparedQuery<TDto> Select<TDto>(ISearchSelect<T, TDto> select)
        where TDto : class;
}
