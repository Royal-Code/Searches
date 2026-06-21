# Searches

Search / Filter / Specifier pattern for .NET, with an Entity Framework Core implementation.

Packages:
- `RoyalCode.SmartSearch.Abstractions` — contracts (`ICriteria<TEntity>`, `ISearch<…>`, `ISorting`, …).
- `RoyalCode.SmartSearch.Core` — default implementations (`Criteria`, `Search`, `CriteriaOptions`).
- `RoyalCode.SmartSearch.Linq` — LINQ specifiers/selectors/order-by.
- `RoyalCode.SmartSearch.EntityFramework` — EF Core pipeline (`ICriteria` over a `DbContext`).
- `RoyalCode.SmartSearch.AspNetCore` — ASP.NET Core helpers.

## Criteria

Register entities and resolve `ICriteria<TEntity>` (directly or via `ISearchManager<TDbContext>`):

```csharp
services.AddEntityFrameworkSearches<MyDbContext>(cfg => cfg.Add<MyEntity>());

var criteria = provider.GetRequiredService<ICriteria<MyEntity>>();
var items = criteria.FilterBy(new MyFilter { Name = "abc" }).Collect();
```

`ICriteria<TEntity>` exposes `FilterBy`, `OrderBy`, `UseHints`, `Select<TDto>()`, `AsSearch()`, and the terminals
`Collect`/`CollectAsync`, `Exists`/`ExistsAsync`, `FirstOrDefault`/`Single` (+ async).

## Loading the aggregate graph (Include) via Operation Hints

`ICriteria` does **not** take a raw `Include(...)` expression. To load navigations (children, owned, 1:1) you declare
the graph **once** per `(entity, hint)` using [`RoyalCode.OperationHint`](https://github.com/Royal-Code/OperationHint),
then reference it by hint. This keeps the contract provider-neutral and centralizes include knowledge.

> Rule of thumb: a read that returns a **DTO** uses `Select<TDto>()`; loading the **aggregate** (to mutate or expose its
> graph) uses **hints**.

### 1. Register the includes once

```csharp
services.AddEntityFrameworkSearches<MyDbContext>(cfg => cfg.Add<Blog>());

services.ConfigureOperationHints(registry =>
    registry.AddIncludesHandler<Blog, BlogHints>((hint, includes) =>
    {
        if (hint is BlogHints.WithPosts) includes.IncludeCollection(b => b.Posts);
        if (hint is BlogHints.WithOwner) includes.IncludeReference(b => b.Owner);
    }));

public enum BlogHints { WithPosts, WithOwner }
```

### 2a. Per-query hints — `UseHints` (isolated to this criteria)

```csharp
var blog = criteria
    .FilterBy(new BlogFilter { Id = 5 })
    .UseHints(BlogHints.WithPosts, BlogHints.WithOwner)
    .Single();           // blog.Posts and blog.Owner are loaded
```

`UseHints` is local: a sibling criteria in the same scope is unaffected.

### 2b. Ambient hints — for every criteria in the scope

```csharp
scope.ServiceProvider.GetRequiredService<IHintsContainer>().AddHint(BlogHints.WithPosts);
var blogs = criteria.Collect();   // every criteria in this scope includes Posts
```

`UseHints` and ambient hints **union**.

### Where hints apply

- ✅ Entity terminals: `Collect`/`ToList`, `FirstOrDefault`, `Single`, and `AsSearch().ToList()`.
- ❌ `Exists()` (it is an `Any()`), and ❌ after `Select<TDto>()` (projection — the type changes).
- 🔁 The same registration also drives the post-load `Find` path (`IHintPerformer.Perform(entity, source)`).
- 🟢 Without `OperationHint` registered, criteria behave exactly as before (no include applied).
