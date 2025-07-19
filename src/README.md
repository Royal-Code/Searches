# RoyalCode SmartSearch

## Overview

RoyalCode SmartSearch is a set of .NET libraries for implementing advanced search, filtering, sorting, and data projection in enterprise applications, following the Specification/Filter-Specifier-Pattern. The goal is to facilitate the creation of reusable, decoupled, and extensible search components, integrating with ORMs such as Entity Framework.

## Applied Pattern

The core pattern is the **Filter-Specifier-Pattern** (Specification Pattern), which allows you to define filters and search criteria declaratively, compose LINQ expressions, and simplify testing and maintenance. The main components are:

- **Filter:** Object representing search criteria.
- **Specifier:** Function or component that applies the filter to a LINQ query.
- **Selector:** Projection from entities to DTOs.
- **Sorting:** Sorting of results.

## Libraries

### RoyalCode.SmartSearch.Core
Abstract components for search and filtering using LINQ and the Specification Pattern.

### RoyalCode.SmartSearch.Linq
Implements property resolution, expression generation, selector mapping, and dynamic sorting.

### RoyalCode.SmartSearch.EntityFramework
Integration with Entity Framework Core, adding services and extensions to register entities and perform persistent searches.

### RoyalCode.SmartSearch.Abstractions
Interfaces and contracts for results, sorting, projection, and search criteria.

## Main Components

### ICriteria<TEntity>
Interface for defining search criteria, applying filters, sorting, and collecting results:

```csharp
var criteria = provider.GetRequiredService<ICriteria<SimpleModel>>();
criteria.FilterBy(new SimpleFilter { Name = "B" });
var results = criteria.Collect(); // Returns only records with Name = "B"
```

### ISearchManager<TDbContext>
Search manager for a DbContext, allowing you to create criteria for entities:

```csharp
var manager = provider.GetRequiredService<ISearchManager<MyDbContext>>();
var criteria = manager.Criteria<MyEntity>();
```

### Sorting and ResultList
Allows sorting and paging of results, as well as projection to DTOs:

```csharp
var sorting = new Sorting { OrderBy = "Name", Direction = ListSortDirection.Ascending };
criteria.OrderBy(sorting);
ResultList<MyEntity> result = ...;
var items = result.Items;
```

### Search Configuration
Use ISearchConfigurations to configure filters, sorting, and selectors:

```csharp
services.AddEntityFrameworkSearches<MyDbContext>(cfg =>
{
    cfg.Add<MyEntity>();
    cfg.AddOrderBy<MyEntity, string>("Name", x => x.Name);
    cfg.AddSelector<MyEntity, MyDto>(x => new MyDto { Id = x.Id, Name = x.Name });
});
```

## Usage Examples

### 1. Simple Search with Filter
```csharp
public class SimpleModel { public int Id; public string Name; }
public class SimpleFilter { public string Name; }

var criteria = provider.GetRequiredService<ICriteria<SimpleModel>>();
criteria.FilterBy(new SimpleFilter { Name = "B" });
var results = criteria.Collect(); // Returns only records with Name = "B"
```

### 2. Asynchronous Search
```csharp
var results = await criteria.FilterBy(new SimpleFilter { Name = "A" }).CollectAsync();
```

### 3. Dynamic Sorting
```csharp
criteria.OrderBy(new Sorting { OrderBy = "Name", Direction = ListSortDirection.Descending });
var results = criteria.Collect();
```

### 4. Projection to DTO
```csharp
criteria.Select<MyDto>(); // Projects to the configured DTO
```

### 5. Advanced Filter Configuration
```csharp
cfg.ConfigureSpecifierGenerator<MyEntity, MyFilter>(opt =>
{
    opt.For(f => f.SomeProperty).Predicate(val => e => e.Collection.Any(x => x.Id == val));
});
```

## Tests and Examples
Tests in `RoyalCode.SmartSearch.Tests` demonstrate usage scenarios such as:
- Filtering by simple and complex properties
- Sorting and paging
- Projection to DTOs
- Custom filter configuration

## References
- [Specification Pattern](https://martinfowler.com/apsupp/spec.pdf)
- LINQ, Entity Framework Core

---

For more examples, see the test files in the `RoyalCode.SmartSearch.Tests` folder.
