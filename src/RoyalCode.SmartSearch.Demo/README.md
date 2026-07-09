# RoyalCode.SmartSearch.Demo

Executable WebAPI demo for **RoyalCode.SmartSearch**. It exists for documentation and experimentation only
(it is never published as a NuGet package) and is meant to be copied from: every filter, DTO and endpoint is a
small, self-contained example of one or more SmartSearch features.

It uses **SQLite in-memory** with a single connection kept open for the process lifetime, so the database is
created empty on every run and seeded deterministically. Nothing external is required. The seed has 25 customers,
5 products and 30 orders, so the list/search endpoints paginate (default 10 items per page).

## Run

```powershell
dotnet run --project .\RoyalCode.SmartSearch.Demo\RoyalCode.SmartSearch.Demo.csproj
```

The app listens on `http://localhost:5080`. The home page (`/`) opens the **Scalar** API reference UI at
`/scalar`, backed by the OpenAPI document at `/openapi/v1.json`. Ready-to-run calls are in
[`RoyalCode.SmartSearch.Demo.http`](./RoyalCode.SmartSearch.Demo.http).

## Two ways to search

The demo shows the same capability through two surfaces:

- **Manual** (`/manual/*`, [`Endpoints/ManualSearchEndpoints.cs`](./Endpoints/ManualSearchEndpoints.cs)) — uses
  `ICriteria<TEntity>` directly. You control filtering, sorting, paging, projection and terminals by hand.
- **Mapped** (`/api/*`, [`Endpoints/MappedSearchEndpoints.cs`](./Endpoints/MappedSearchEndpoints.cs)) — uses the
  AspNetCore helpers `MapSearch` / `MapList` / `MapFirst` / `MapSelectFirst`, which wire the filter (bound from
  the query string via `[AsParameters]`), sorting, paging and standardized HTTP results (200/204/400/500) for you.

`GET /api/orders` (mapped) and `GET /manual/orders` (manual) implement the **same query** so you can compare them
side by side.

## Coverage matrix

Every row is a feature, where it lives, and a call that exercises it against the seed. Base URL omitted
(`http://localhost:5080`).

| Feature | Where | Endpoint + query string | Expected |
|---|---|---|---|
| Equality | `ProductFilter.Active` | `GET /api/products?active=true` | active products only |
| Numeric range | `ProductFilter.PriceMin/PriceMax` | `GET /api/products?priceMin=50&priceMax=200` | Webcam (79.00) |
| Date range | `OrderFilter.CreatedAtFrom/CreatedAtTo` | `GET /api/orders?createdAtFrom=2026-02-01&createdAtTo=2026-05-31` | orders 2,3,4 |
| `In` (list) | `OrderStatusesFilter.Statuses` (`IEnumerable<>`) | `GET /manual/orders/by-status?statuses=Paid&statuses=Shipped` | Paid/Shipped orders (incl. 1,3,5) |
| Contains + case-insensitive | `CustomerFilter.Name` | `GET /api/customers?name=maria` | Maria Silva |
| `Like` anchored (`Wrap=None`) | `ProductFilter.Sku` | `GET /api/products?sku=ABC%25` | SKUs starting `ABC` |
| OR by name (token split) | `CustomerFilter.NameOrEmail` | `GET /api/customers?nameOrEmail=mario` | Mario Souza |
| `[Disjunction]` | `ProductFilter.TextInName/TextInSku` | `GET /api/products?textInName=mo&textInSku=xyz` | Mouse, Monitor, Headset |
| `DisableOrFromName` (trap) | `OrderFilter.NumberOrCode` | `GET /api/orders?numberOrCode=1001` | order 1 (see note) |
| `[ComplexFilter]` (owned) | `AddressFilter` via `CustomerAddressFilter` | `GET /manual/customers/by-address?city=NYC` | Maria, Mario |
| Nested target path | `CustomerFilter.State` / `OrderFilter.CustomerName` | `GET /api/orders?customerName=maria` | orders 1,3 |
| Negation | `OrderFilter.NotStatus` | `GET /api/orders?notStatus=Cancelled` | all but order 4 |
| Named sorting | `AddOrderBy("createdAt", ...)` | `GET /api/orders?orderby=createdAt-desc` | newest first |
| Paging (`UsePages`) | `SearchOptions` (mapped) | `GET /api/orders?page=1&itemsPerPage=2` | page 1, 2 items |
| Paging (`Skip`/`Take`) | manual | `GET /manual/orders/page?skip=2&take=2` | 3rd–4th orders |
| `UseCount(false)` | manual | `GET /manual/orders/page?skip=2&take=2&count=false` | `count` not computed (0) |
| Projection by convention | `ProductDto` (`Select<ProductDto>()`) | `GET /api/products` | product DTOs |
| Projection by registered selector | `OrderSummaryDto` (`AddSelector`) | `GET /api/orders` | `customerName` + `total` |
| Projection by explicit expression | `Select(expr)` | `GET /manual/customers?name=maria` | customer DTOs |
| `Exists` | manual | `GET /manual/orders/exists?number=1002` | `{ "exists": true }` |
| `Single` | manual | `GET /manual/orders/by-number/ORD-1001` | order 1 (throws if 0/2+) |
| `FirstOrDefault` | mapped / manual | `GET /api/products/first?active=true` | first active product |
| Hint loads navigation | `UseHints(WithCustomer, WithItems)` | `GET /manual/orders/1` | `customer` + `items` populated |
| Hint ignored by projection | `MapSearch` DTO | `GET /api/orders` | hints do not apply to DTOs |
| Manual vs mapped (same query) | `OrderFilter` | `GET /api/orders` == `GET /manual/orders` | identical results |
| Invalid order by → 400 | pipeline (`OrderByException`) | `GET /api/orders?orderby=bogusField` | ProblemDetails 400 |

## Notes

### The three OR behaviors

1. **OR by name** — a filter property whose name (or target path) contains the token `Or` is split into a
   disjunction. `CustomerFilter.NameOrEmail` becomes `Name` OR `Email`.
2. **`[Disjunction("alias")]`** — group several filter properties (each with its own value and target) into one
   OR clause. `ProductFilter.TextInName`/`TextInSku` match `Name` contains X **OR** `Sku` contains Y.
3. **`DisableOrFromName`** — the escape hatch. A property named `NumberOrCode` would be split into `Number` OR
   `Code`; if `Code` is not a real member the filter would break. Setting
   `[Criterion("Number", DisableOrFromName = true)]` keeps it as a single criterion over `Number`. Use it for
   natural names that merely contain the substring `Or` (e.g. `ColorOrSize`, `NomeOrApelido`).

### Complex filter over the owned Address

`Address` is mapped as an owned type (`OwnsOne`) on the customer table and annotated with `[ComplexFilter]`.
You can filter it two ways:

- a **nested target path** on a flat, query-friendly property: `CustomerFilter.State` →
  `[Criterion("MainAddress.State")]` (`GET /api/customers?state=NY`);
- a **structured `[ComplexFilter]` object**: `AddressFilter` inside `CustomerAddressFilter`
  (`GET /manual/customers/by-address?city=NYC&state=NY`), built by hand in the manual endpoint because a nested
  object does not bind from a flat query string.

### Operation hints

Hints map to EF includes (`ConfigureOperationHints` + `AddIncludesHandler<Order, OrderHints>`). They apply only to
entity-materializing terminals (`Collect`/`Single`/`FirstOrDefault`) — `GET /manual/orders/1` returns the order
with its `customer` and `items` loaded (but `items[].product` is `null`, since that navigation was not hinted).
Hints do **not** apply to `Select<TDto>()` projections nor to `Exists`, so the DTO endpoints never depend on them.

### Invalid order by

When `orderby` names an unknown property, the pipeline throws `OrderByException`, which the AspNetCore helpers turn
into an RFC-7807 `ProblemDetails` with HTTP 400:

```json
{
  "type": "about:blank",
  "title": "The input parameters are invalid",
  "status": 400,
  "detail": "The order by 'bogusField' is not supported for the type 'Order'.",
  "propertyName": "bogusField",
  "typeName": "Order",
  "pointer": "#/orderby"
}
```

### SmartProblems in the manual endpoints

The manual endpoints ([`Endpoints/ManualSearchEndpoints.cs`](./Endpoints/ManualSearchEndpoints.cs)) use
[SmartProblems](../.ai/references/problems/problems.md) for error handling: an invalid order by returns
`Problems.InvalidParameter(...)` (400) and a missing order returns `Problems.NotFound(...)` (404), both surfaced as
RFC-9457 ProblemDetails through the `OkMatch<T>` / `MatchList<T>` / `MatchSearch<T>` result types. The endpoint
group also adds `WithExceptionFilter()`, which turns any unexpected exception into a 500 ProblemDetails.

## Layout

```text
Domain/     Customer, Address ([ComplexFilter], owned), Product, Order, OrderItem, OrderStatus
Data/       AppDbContext (SQLite + OwnsOne mapping), DemoSeeder (deterministic seed)
Filters/    CustomerFilter, AddressFilter (+CustomerAddressFilter), ProductFilter, OrderFilter (kitchen-sink),
            OrderStatusesFilter (In), OrderLookupFilters (Equal)
Dtos/       CustomerDto, ProductDto (convention), OrderSummaryDto (registered selector)
Search/     OrderHints, SearchSetup (Add<>, AddSelector, AddOrderBy, Like operator, ConfigureOperationHints)
Endpoints/  ManualSearchEndpoints (ICriteria), MappedSearchEndpoints (helpers)
```
