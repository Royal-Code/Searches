# RoyalCode SmartSearch

## Overview

RoyalCode SmartSearch is a set of .NET libraries for implementing advanced search, filtering, sorting, and data projection in enterprise applications, following the Specification/Filter-Specifier-Pattern. The goal is to facilitate the creation of reusable, decoupled, and extensible search components, integrating with ORMs such as Entity Framework.

## Applied Pattern

The core pattern is the **Filter-Specifier-Pattern** (Specification Pattern), which allows you to define filters and search criteria declaratively, compose LINQ expressions, and simplify testing and maintenance. The main components are:

- **Filter:** Object representing search criteria.
- **Specifier:** Function or component that applies the filter to a LINQ query.
- **Selector:** Projection from entities to DTOs.
- **Sorting:** Sorting of results.
- **Disjunction (OR groups):** Grouped OR conditions across multiple filter properties.
- **OR by property name/path:** Automatically split properties containing "Or" into OR conditions.
  - Opt-out: set `DisableOrFromName = true` in `Criterion` to treat names/paths with `Or` as a single criterion.
- **ComplexFilter:** Filter complex/owned types and structs by mapping scalar or nested filters to target paths.
- **FilterExpressionGenerator:** Plug custom expression builders for complex filter scenarios via `ISpecifierExpressionGenerator`.

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

### 6. Disjunction filters (grouped OR)
Use `[Disjuction("alias")]` to group multiple filter properties into an OR clause:

```csharp
public class DisjunctionFilter
{
    [Disjuction("g1")] public string? P1 { get; set; }
    [Disjuction("g1")] public string? P2 { get; set; }
}

// When both are empty: no WHERE is applied.
// When one has a value: single condition.
// When multiple have values: OR across P1 and P2.
```

### 7. OR by property name or TargetPropertyPath
If a filter property name or `Criterion.TargetPropertyPath` contains `Or`, it is split and applied as OR across the parts:

```csharp
public class OrFilterNameProperty
{
    [Criterion] // default operator for string is Like
    public string? FirstNameOrMiddleNameOrLastName { get; set; }
}

public class OrFilterTargetPath
{
    [Criterion(TargetPropertyPath = "FirstNameOrLastName")]
    public string? Query { get; set; }
}

// Opt-out (do not split by Or in the name)
public class PreferencesFilter
{
    [Criterion(DisableOrFromName = true)]
    public string? ColorOrSizePreference { get; set; } // treated as a single criterion
}
```

### 8. ComplexFilter for complex/owned types
Use `[ComplexFilter]` on filter properties or complex types to apply nested filtering with automatic resolution of target properties.

Scalar-to-complex mapping (do not require `[ComplexFilter]`):
```csharp
public readonly record struct Email(string Value);

public class UserFilter
{
    [Criterion("Email.Value")]
    public string? Email { get; set; } // maps to User.Email.Value
}
```

Nested complex type filtering:
```csharp
public readonly record struct PersonName(string FirstName, string MiddleName, string LastName);

public class UserFilter
{
    [ComplexFilter]
    public PersonName? Name { get; set; } // filters FirstName/MiddleName/LastName, ignores empty fields
}
```

Owned/complex entity filtering via target path:
```csharp
[ComplexFilter]
public class Address
{
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
}

public class User
{
    public Address? MainAddress { get; set; }
}

public class UserFilter
{
    [Criterion("MainAddress")]
    public Address? Address { get; set; } // applies nested filters to MainAddress
}
```

Behavior notes:
- Empty/null values are ignored when `IgnoreIfIsEmpty` applies (strings blank, nullables not set, empty collections).
- Complex filters support AND across provided subfields; only non-empty subfields are applied.
- OR semantics can be declared via `[Disjuction]` groups or inferred from `Or` in names/paths.

### 9. FilterExpressionGenerator for complex filter logic
Use `[FilterExpressionGenerator<TGenerator>]` to delegate expression creation to a custom generator that implements `ISpecifierExpressionGenerator`.

Attribute:
```csharp
public class OrderByDateFilter
{
    [Criterion(nameof(Order.OrderDate))]
    [FilterExpressionGenerator<PeriodSpecifierExpressionGenerator>]
    public Period Period { get; set; }
}
```

Generator:
```csharp
public class PeriodSpecifierExpressionGenerator : ISpecifierExpressionGenerator
{
    public static Expression GenerateExpression(ExpressionGeneratorContext ctx)
    {
        // Compute range (outside expression) and then apply via expression
        var getRange = typeof(PeriodSpecifierExpressionGenerator).GetMethod("GetRange")!;
        var rangeCall = Expression.Call(getRange, ctx.FilterMember);

        var start = Expression.Property(rangeCall, nameof(PeriodRange.Start));
        var end   = Expression.Property(rangeCall, nameof(PeriodRange.End));

        var ge = Expression.GreaterThanOrEqual(ctx.ModelMember, start);
        var lt = Expression.LessThan(ctx.ModelMember, end);
        var body = Expression.AndAlso(ge, lt);

        var predType = typeof(Func<,>).MakeGenericType(ctx.Model.Type, typeof(bool));
        var lambda = Expression.Lambda(predType, body, ctx.Model);

        var where = Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { ctx.Model.Type }, ctx.Query, lambda);
        return Expression.Assign(ctx.Query, where);
    }
}
```

This allows encapsulating advanced logic (date ranges, business rules) and reusing it across filters while keeping the generated LINQ expression EF-translatable.

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
- Disjunction and OR splitting by name/path
- Complex filter on structs/owned entities (`Email`, `PersonName`, `Address`)

## References
- [Specification Pattern](https://martinfowler.com/apsupp/spec.pdf)
- LINQ, Entity Framework Core

---

For more examples, see the test files in the `RoyalCode.SmartSearch.Tests` folder.

## Documentation Status
- Libraries and patterns are described above with examples of usage.
- More API-level docs and extension points can be expanded (e.g., selector resolvers, specifier generator options).
- Tests illustrate typical usage; a dedicated samples directory could be added in the future.

## Test Coverage
The test projects already reference `coverlet.collector` and `Microsoft.CodeCoverage` (see `src/tests.targets`).

### Local coverage report
1. Run tests and collect coverage:
   - `dotnet test ./src --collect:"XPlat Code Coverage" --results-directory ./TestResults`
2. Generate HTML report:
   - Install tool once: `dotnet tool install --global dotnet-reportgenerator-globaltool`
   - Generate: `reportgenerator -reports:./TestResults/**/coverage.cobertura.xml -targetdir:./TestResults/Report -reporttypes:Html`
3. Open `./TestResults/Report/index.html` in a browser.

### GitHub Actions coverage
The workflow at `.github/workflows/smart-search.yml` runs tests with coverage and publishes an artifact `coverage-report` containing the HTML output. After the run completes:
- Download the `coverage-report` artifact from the job summary.
- Open `index.html` to view coverage.

