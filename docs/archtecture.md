# Overview and goals
- Purpose: Provide reusable, decoupled search/filter/sort/projection for IQueryable sources and EF Core, following a Specification/Filter-Specifier pattern.
- Key capabilities:
    - Declarative filters using attributes and conventions.
    - Automatic LINQ expression generation for criteria.
    - Sorting and paging with ResultList.
    - DTO projection via selector mapping.
    - Integration with EF Core (ISearchManager<TDbContext>, ICriteria<TEntity>).
    - Extensibility points for custom predicate factories, complex filters, and OR-disjunctions.

# Primary packages and responsibilities
- RoyalCode.SmartSearch.Core: Abstractions for searching, filtering, result composition, sorting, and projection.
- RoyalCode.SmartSearch.Linq:
    - Filter expression generation and resolution (core of building predicates).
    - Selector generation for DTO mapping.
    - Sorting and property-path resolution.
    - Factories for specifiers and selectors.
- RoyalCode.SmartSearch.EntityFramework:
    - EF Core integration: DI registrations, ISearchManager<TDbContext>, ICriteria<TEntity> for EF sets.

# Core concepts
- Filter: Plain object with properties that represent search criteria. Attributes on properties control operator, negation, target path, “ignore if empty”, and grouping for disjunctions.
- Specifier: Applies the filter to an IQueryable<TModel>, composing predicate(s) and returning a filtered query.
- Selector: Maps entities to DTOs via generated expressions (DefaultSelectorExpressionGenerator).
- Sorting: Sorting object(s) parsed from strings or JSON; applied to queries; included in ResultList.
- Criteria: ICriteria<TEntity> orchestrates filter, sorting, paging, projection, sync/async collection.

# How filtering is generated
- Entry points:
    - ISpecifierFunctionGenerator.Generate<TModel, TFilter>(): Produces a function (IQueryable<TModel>, TFilter) -> IQueryable<TModel>.
    - ISpecifierGenerator.Generate<TModel, TFilter>(): Produces an ISpecifier<TModel, TFilter>.
    - ISpecifierExpressionGenerator.GenerateExpression<TModel>(ExpressionGeneratorContext<TModel>): Builds expression trees for a filtering pipeline.
- Resolution pipeline: CriterionResolutions.CreateResolutions<TModel, TFilter>() constructs a list of ICriterionResolution describing how each filter property contributes to the final expression. Steps:
    1.	Predicate factories from SpecifierGeneratorOptions:
        - If a property has a custom predicate factory configured, generate PredicateFactoryCriterionResolution and exclude from default processing.
    2.	Disjunction groups via [Disjuction(alias)]:
        - Properties with the same group alias are combined into a single OR-resolution (DisjuctionCriterionResolution), then excluded from remaining processing.
    3.	“Or” in property name or TargetPropertyPath:
        - Names/path containing “Or” are split into parts; each part becomes an individual criterion, then all are wrapped in disjunction resolution.
        - Example: FirstNameOrMiddleNameOrLastName results in an OR across these target paths.
    4.	Complex filter attribute:
        - Properties with [ComplexFilter] on the property or property type are elected for ComplexFilterCriterionResolution (currently NotImplemented).
    5.	Default resolution:
        - Remaining properties use DefaultOperatorCriterionResolution using attribute/operator defaults (e.g., Equal for numerics, Like for strings, In for enumerables).
        - Operator selection:
        - DiscoveryCriterionOperator(CriterionAttribute, PropertyInfo) chooses operator when Operator=Auto based on property type (validated in tests: strings => Like; enumerables => In; numerics/dates => Equal).
        - Ignore-if-empty semantics:
        - GetIfIsEmptyConstraintExpression(this Expression, Expression) wraps predicate generation with guards so empty values are skipped, ensuring filters aren’t applied when null/blank/empty (tested across strings, nullable types, collections, structs).

# Disjunction and OR semantics
- [Disjuction("g1")]:
    - Tested in DisjunctionTests: when all group members are empty → no where applied; one value → single condition; multiple values → OR between conditions.
- “Or” in property name or target path:
    - Tested in OrTests: property FirstNameOrMiddleNameOrLastName or target FirstNameOrLastName create OR predicates across the listed paths; empty input is ignored.
    - Opt-out: properties with `DisableOrFromName = true` do not split by `Or` and are treated as a single target.

# Selector generation (DTO projection)
- DefaultSelectorExpressionGenerator.Generate<TEntity, TDto>():
    - Maps same-named properties, handles nested objects, nullables, enums, sub-selects, collections, and multi-level nests.
    - Tests verify:
        - Same-name mapping (SimpleEntity -> SimpleDto).
        - Navigation properties (ComplexEntity.Complex.Value -> DtoForComplex.ComplexValue).
        - Nullables normalization (EntityWithNullable to non-null DTO property).
        - Enum mapping across different enum types by value conversion.
        - Nested sub-selects and multi-level sub-selects (including complex substructures).
        - Collections of ints and item objects preserve element mapping with appropriate transformations.

# Search orchestration and results
- ICriteria<TEntity>:
    - Methods: FilterBy<TFilter>(TFilter), OrderBy(ISorting), Select<TDto>, Collect()/CollectAsync(), existence checks (Exists()), single-result queries (FirstOrDefault(), Single()), and search mode (AsSearch().ToList()/ToListAsync()).
    - Tests validate:
        - Collect all with no filter or sorting (sync/async).
    - Filter by simple property (Name = "B") for collect, exists, first, single, exceptions when invalid cardinality.
- ResultList<T>:
    - Contains paging metadata: Page, ItemsPerPage, Count, Pages, Sortings, and items.
    - JSON serialization/deserialization compatibility validated in SortingTests.
- Search defaults:
    - SearchTests show AsSearch().ToListAsync() applies defaults: page=1, itemsPerPage=10, default sortings added (likely by configuration), and supported filter attributes on date ranges.

# EF Core integration
- Tests create in-memory SQLite connections, register DbContext, then call services.AddEntityFrameworkSearches<TDbContext>(s => s.Add<TEntity>()).
- ISearchManager<TDbContext> is used to create criteria (manager.Criteria<TEntity>()), while convenience in tests also gets ICriteria<TEntity> directly from DI.
- Ensures DB is created and then queries are composed against EF with dynamic LINQ expressions.

# Factories and DI surface
- ISpecifierFactory.GetSpecifier<TModel, TFilter>():
    - Returns specifier capable of applying filter (can dispatch to Filter method on filter if present, e.g., SimpleFilterManual.Filter()).
- ISelectorFactory.Create<TEntity, TDto>():
    - Provides DTO selector implementation that leverages generated expressions.

# Extension points for customization
- ISearchConfigurations:
    - Used to add entities, configure sortings, add selectors, set predicate factories for specific filter properties. Example in README shows:
        - cfg.AddOrderBy<TEntity, string>("Name", x => x.Name)
        - cfg.AddSelector<TEntity, TDto>(...)
    - ConfigureSpecifierGenerator<TEntity, TFilter>(opt => opt.For(f => f.SomeProperty).Predicate(val => e => e.Collection.Any(x => x.Id == val)))
- Specifier Generator Options:
    - Allows mapping of filter properties to custom predicate factories, overriding default operator-based behavior.

# Complex types and attributes
- Complex filter attribute detection:
    - ExpressionGenerator.HasAttribute(type, typeof(ComplexFilterAttribute<>), out var attr) is used to identify complex filters (tested in HasAttribute_WhenComplexFilter_ShouldBeTrue()).
- Complex filter behavior (expected):
    - Email as string matching complex type Value indicates a complex filter attribute with generic argument string to guide mapping.
    - Tests validate:
        - Filtering by complex type property path (e.g., Email.Value) when filter provides a scalar value.
        - No predicate applied when the filter value is empty (null).

# Design summary
- Pipeline-based resolution:
    - Build a set of resolutions for every filter property, respecting configurations and attributes, then compose a single query predicate by merging all applicable resolutions.
- Convention over configuration:
    - Defaults work across common types and operators; attributes enable switching operators, target paths, negation, and grouping.
- Expression-first:
    - All behavior funnels into LINQ expression trees that EF Core can translate to SQL or operate on in-memory LINQ providers.
- Extensible:
    - Predicate factories, complex filters, custom selector mapping, and sort definitions can be added via configuration APIs.