# SmartSearch - Documentacao Orientada a IA

SmartSearch implementa busca declarativa para .NET usando filtros, specifiers, sorting, paginacao, selecao para DTO e execucao sobre Entity Framework Core.

## Pacotes

- `RoyalCode.SmartSearch.Abstractions`: contratos como `ICriteria<TEntity>`, `ISearch<TEntity>`, `ISorting`, `IResultList`.
- `RoyalCode.SmartSearch.Core`: implementacoes padrao, `Criteria`, `Search`, `CriteriaOptions`.
- `RoyalCode.SmartSearch.Linq`: geracao de expressoes, specifiers, selectors e order-by.
- `RoyalCode.SmartSearch.EntityFramework`: pipeline EF Core sobre `DbContext`.
- `RoyalCode.SmartSearch.AspNetCore`: helpers para endpoints.

## Setup Basico com EF Core

Registre o `DbContext` e as entidades pesquisaveis:

```csharp
services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

services.AddEntityFrameworkSearches<AppDbContext>(cfg =>
{
    cfg.Add<Order>();
    cfg.Add<Customer>();
});
```

Tambem e possivel registrar entidades dinamicamente por `Type`:

```csharp
services.AddEntityFrameworkSearches<AppDbContext>(cfg =>
{
    foreach (var entityType in discoveredEntityTypes)
        cfg.Add(entityType);
});
```

Resolva `ICriteria<TEntity>` diretamente:

```csharp
var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<Order>>();
```

Ou use `ISearchManager<TDbContext>`:

```csharp
var manager = scope.ServiceProvider.GetRequiredService<ISearchManager<AppDbContext>>();
var criteria = manager.Criteria<Order>();
```

Tambem existe a extensao sobre `DbContext`:

```csharp
var criteria = db.Criteria<Order>();
```

## Fluxo Mental

- Use `FilterBy(filter)` para aplicar criterios declarativos.
- Use `OrderBy(...)` para ordenar.
- Use `UsePages(...)`, `FetchPage(...)`, `Skip(...)`, `Take(...)` ou `SkipTake(...)` para limitar resultados.
- Use `Select<TDto>()` quando o retorno e DTO.
- Use `UseHints(...)` quando o retorno e entidade e voce precisa carregar o grafo do agregado.
- Use `Collect()` para lista simples rastreada pelo EF.
- Use `AsSearch().ToList()` para `ResultList<T>` com metadados de pagina.

## Filtros

Um filtro e uma classe com propriedades que representam criterios de busca. Valores vazios sao normalmente ignorados.

Por convencao, toda propriedade publica do filtro vira um criterio usando o mesmo nome no modelo. Use `[Criterion]`
apenas quando precisar configurar algo: operador, caminho alvo, negacao, ignorar propriedade ou regra de valor vazio.
`[Criterion]` sem configuracao e equivalente a nao colocar atributo.

```csharp
public sealed class OrderFilter
{
    public int? Id { get; set; }

    public string? Number { get; set; }

    [Criterion("Customer.Name")]
    public string? CustomerName { get; set; }
}
```

Uso:

```csharp
var orders = criteria
    .FilterBy(new OrderFilter { CustomerName = "Maria" })
    .Collect();
```

`FilterBy` recebe um objeto filtro. Nao passe lambda/predicate para `FilterBy`.

## Filtros Manuais por Metodo

Quando a regra de filtro nao cabe bem em criterios por propriedade, o proprio filtro pode declarar um metodo
publico que recebe e retorna `IQueryable<TModel>`. O nome recomendado e `Filter`:

```csharp
public sealed class OrderFilter
{
    public string? Text { get; set; }

    public IQueryable<Order> Filter(IQueryable<Order> query)
    {
        if (!string.IsNullOrWhiteSpace(Text))
        {
            query = query.Where(o =>
                o.Number.Contains(Text) ||
                o.Customer.Name.Contains(Text));
        }

        return query;
    }
}
```

Esse metodo e descoberto automaticamente pelo SmartSearch quando nao existe specifier ja registrado ou
resolvido por DI para o par modelo/filtro. Quando ele existe, ele representa o filtro completo; as
propriedades do filtro nao sao processadas novamente pelo gerador por convencao.

## Operadores de Filtro

Use `CriterionAttribute` para controlar operador e caminho alvo:

```csharp
public sealed class InvoiceFilter
{
    [Criterion("CreatedAt", CriterionOperator.GreaterThanOrEqual)]
    public DateTime? CreatedAtStart { get; set; }

    [Criterion("CreatedAt", CriterionOperator.LessThanOrEqual)]
    public DateTime? CreatedAtEnd { get; set; }
}
```

Caminhos aninhados podem ser passados pelo construtor ou por `TargetPropertyPath`:

```csharp
[Criterion("Customer.Email")]
public string? Email { get; set; }
```

## OR / Disjuncao

Use `[Disjuction("grupo")]` para combinar membros em OR:

```csharp
public sealed class ContactFilter
{
    [Disjuction("contact")]
    public string? Email { get; set; }

    [Disjuction("contact")]
    public string? Phone { get; set; }
}
```

Tambem ha convencao por nome/caminho contendo `Or`:

```csharp
public sealed class PersonFilter
{
    public string? FirstNameOrLastName { get; set; }
}
```

Tambem funciona com caminho alvo:

```csharp
public sealed class PersonFilter
{
    [Criterion(TargetPropertyPath = "FirstNameOrLastName")]
    public string? Query { get; set; }
}
```

Se `Or` faz parte do nome e nao deve indicar disjuncao, use `DisableOrFromName`:

```csharp
public sealed class ProductFilter
{
    [Criterion(DisableOrFromName = true)]
    public string? ColorOrSizePreference { get; set; }
}
```

## Filtros Complexos

Use `[ComplexFilter]` quando uma propriedade do filtro e um objeto de valor ou subfiltro com campos
internos que devem ser aplicados contra uma propriedade complexa do modelo.

```csharp
[ComplexFilter]
public sealed class AddressFilter
{
    public string? City { get; set; }
    public string? State { get; set; }
}

public sealed class CustomerFilter
{
    [Criterion("MainAddress")]
    public AddressFilter? Address { get; set; }
}
```

O atributo pode ficar no tipo complexo ou diretamente na propriedade do filtro. Quando a propriedade
complexa esta nula, nenhum filtro interno e aplicado.

Filtros complexos tambem podem combinar OR:

```csharp
[ComplexFilter]
public struct PersonNameFilter
{
    [Criterion("FirstNameOrMiddleNameOrLastName")]
    public string? Value { get; set; }
}
```

## Geradores Customizados de Expressao

Use `[FilterExpressionGenerator<TGenerator>]` quando uma propriedade de filtro precisa gerar uma expressao
LINQ propria, mas voce ainda quer manter o filtro declarativo.

```csharp
public enum Period
{
    Today,
    Last7Days,
    ThisMonth
}

public sealed class OrderFilter
{
    [Criterion("CreatedAt")]
    [FilterExpressionGenerator<PeriodExpressionGenerator>]
    public Period Period { get; set; }
}

public sealed class PeriodExpressionGenerator : ISpecifierExpressionGenerator
{
    public static DateTime GetStart(Period period)
    {
        var today = DateTime.UtcNow.Date;

        return period switch
        {
            Period.Last7Days => today.AddDays(-7),
            Period.ThisMonth => new DateTime(today.Year, today.Month, 1),
            _ => today
        };
    }

    public static Expression GenerateExpression(ExpressionGeneratorContext context)
    {
        var getStart = typeof(PeriodExpressionGenerator).GetMethod(nameof(GetStart))!;
        var start = Expression.Call(getStart, context.FilterMember);
        var body = Expression.GreaterThanOrEqual(context.ModelMember, start);
        var lambda = Expression.Lambda(body, context.Model);

        var where = ExpressionGenerator.CreateWhereCall(
            context.Model.Type,
            context.Query,
            lambda);

        return Expression.Assign(context.Query, where);
    }
}
```

O generator recebe `Query`, `Filter`, `Model`, `ModelMember` e `FilterMember`. Retorne uma expressao que
atualiza a query, normalmente atribuindo um `Where(...)` de volta para `context.Query`.

## Sorting

Sorting dinamico usa `Sorting`:

```csharp
criteria.OrderBy(new Sorting
{
    OrderBy = "CreatedAt",
    Direction = ListSortDirection.Descending
});
```

Registre order-by nomeado quando quiser mapear nomes estaveis para expressoes:

```csharp
services.AddEntityFrameworkSearches<AppDbContext>(cfg =>
{
    cfg.Add<Order>();
    cfg.AddOrderBy<Order, string>("CustomerName", o => o.Customer.Name);
});
```

Uso:

```csharp
criteria.OrderBy(new Sorting { OrderBy = "CustomerName" });
```

## Paginacao e Limites

`ICriteria<TEntity>` herda opcoes comuns:

```csharp
criteria.UsePages(itemsPerPage: 20, pageNumber: 1);
criteria.FetchPage(2);
criteria.Skip(10);
criteria.Take(50);
criteria.SkipTake(skip: 10, take: 50);
criteria.UseCount();
criteria.UseLastCount(lastCount);
```

Para `AsSearch().ToList()`, informe pagina ou limite quando espera itens no `ResultList`:

```csharp
var page = criteria
    .UsePages(itemsPerPage: 20, pageNumber: 1)
    .AsSearch()
    .ToList();
```

`Collect()` nao retorna metadados de pagina; ele retorna apenas os itens.

## Terminais Comuns

### Collect

Materializa entidades em lista simples. No EF Core, preserva tracking.

```csharp
IReadOnlyList<Order> orders = criteria
    .FilterBy(new OrderFilter { CustomerName = "Maria" })
    .Collect();
```

Async:

```csharp
var orders = await criteria.CollectAsync(ct);
```

### AsSearch().ToList

Retorna `IResultList<TEntity>` ou `IResultList<TDto>` com metadados:

```csharp
var result = criteria
    .UsePages(20, 1)
    .AsSearch()
    .ToList();

var items = result.Items;
var total = result.Count;
```

Async:

```csharp
var result = await criteria
    .UsePages(20, 1)
    .AsSearch()
    .ToListAsync(ct);
```

### Exists

Executa como existencia (`Any`). Nao aplica includes/hints.

```csharp
var exists = criteria
    .FilterBy(new OrderFilter { Id = 10 })
    .Exists();
```

### FirstOrDefault

Retorna o primeiro item ou `null`.

```csharp
var order = criteria
    .FilterBy(new OrderFilter { Number = "A-001" })
    .FirstOrDefault();
```

### Single

Retorna exatamente um item. Lanca se nao houver nenhum ou se houver mais de um.

```csharp
var order = criteria
    .FilterBy(new OrderFilter { Id = 10 })
    .Single();
```

## Projecao para DTO

Use `Select<TDto>()` quando quer retorno em DTO.

```csharp
public sealed class OrderDto
{
    public int Id { get; set; }
    public string Number { get; set; } = "";
}
```

Uso por selector configurado ou convencao:

```csharp
var result = criteria
    .FilterBy(new OrderFilter { CustomerName = "Maria" })
    .Select<OrderDto>()
    .UsePages(20, 1)
    .AsSearch()
    .ToList();
```

Uso com expressao:

```csharp
var dto = criteria
    .Select(o => new OrderDto { Id = o.Id, Number = o.Number })
    .FirstOrDefault();
```

Registre selector quando quiser uma expressao centralizada:

```csharp
services.AddEntityFrameworkSearches<AppDbContext>(cfg =>
{
    cfg.Add<Order>();
    cfg.AddSelector<Order, OrderDto>(o => new OrderDto
    {
        Id = o.Id,
        Number = o.Number
    });
});
```

## Operation Hint no SmartSearch

SmartSearch nao expoe `Include(Expression<...>)` no contrato `ICriteria`. Para carregar navegacoes do agregado, use Operation Hint.

Use hints quando o retorno e entidade e voce precisa carregar o grafo. Use `Select<TDto>()` quando o retorno e DTO.

### Pacotes

No projeto EF/infra, use:

```csharp
dotnet add package RoyalCode.OperationHint.EntityFramework
```

`RoyalCode.SmartSearch.EntityFramework` ja referencia `RoyalCode.OperationHint.Abstractions`.

### Registrar Includes

Registre o grafo uma vez por `(entidade, hint)`:

```csharp
public enum OrderHints
{
    WithCustomer,
    WithItems
}

services.AddEntityFrameworkSearches<AppDbContext>(cfg => cfg.Add<Order>());

services.ConfigureOperationHints(registry =>
    registry.AddIncludesHandler<Order, OrderHints>((hint, includes) =>
    {
        if (hint is OrderHints.WithCustomer)
            includes.IncludeReference(o => o.Customer);

        if (hint is OrderHints.WithItems)
            includes.IncludeCollection(o => o.Items);
    }));
```

`AddIncludesHandler<TEntity, THint>` tambem registra o handler de entidade usado por `IHintPerformer.Perform(entity, db)` no caminho pos-carga.

### Hints por Consulta: UseHints

`UseHints` e local da criteria e nao vaza para outras criterias no mesmo escopo.

```csharp
var order = criteria
    .FilterBy(new OrderFilter { Id = 10 })
    .UseHints(OrderHints.WithCustomer, OrderHints.WithItems)
    .Single();
```

Tambem funciona com `Collect`, `FirstOrDefault`, `Single`, `CollectAsync`, `FirstOrDefaultAsync`, `SingleAsync` e caminhos de entidade via `AsSearch()`:

```csharp
var page = criteria
    .UseHints(OrderHints.WithCustomer)
    .UsePages(20, 1)
    .AsSearch()
    .ToList();
```

`UseHints` exige ao menos um hint. `null` gera `ArgumentNullException`; chamada vazia gera `ArgumentException`.

### Hints Ambiente: IHintsContainer

Hints ambiente valem para as criterias executadas no mesmo escopo.

```csharp
var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
container.AddHint(OrderHints.WithCustomer);

var orders = criteria.Collect();
```

Hints ambiente e `UseHints` sao combinados.

### Onde Hints Aplicam

Aplicam em terminais que materializam entidade:

- `Collect()` / `CollectAsync()`
- `FirstOrDefault()` / `FirstOrDefaultAsync()`
- `Single()` / `SingleAsync()`
- `AsSearch().ToList()` / `ToListAsync()` / `ToAsyncListAsync()` quando o resultado ainda e entidade

Nao aplicam em:

- `Exists()` / `ExistsAsync()`
- Depois de `Select<TDto>()`
- Projecoes para DTO

Sem OperationHint registrado, o comportamento e no-op: nenhuma navegacao e incluida.

### Find / Pos-Carga

SmartSearch nao fornece uma API `Find`. A paridade vem do Operation Hint: o mesmo `AddIncludesHandler` cobre query e entidade. Em um repository externo:

```csharp
public Order? FindOrder(int id, IHintsContainer container, IHintPerformer performer, AppDbContext db)
{
    container.AddHint(OrderHints.WithItems);

    var order = db.Set<Order>().Find(id);
    if (order is not null)
        performer.Perform(order, db);

    return order;
}
```

## Exemplos Completos

### Lista simples rastreada com filtro, sorting e hints

```csharp
var orders = criteria
    .FilterBy(new OrderFilter { CustomerName = "Maria" })
    .OrderBy(new Sorting { OrderBy = "CreatedAt", Direction = ListSortDirection.Descending })
    .UseHints(OrderHints.WithCustomer)
    .Collect();
```

### Pagina para UI/API

```csharp
var page = criteria
    .FilterBy(new OrderFilter { CustomerName = "Maria" })
    .UseHints(OrderHints.WithCustomer)
    .UsePages(itemsPerPage: 20, pageNumber: 1)
    .AsSearch()
    .ToList();
```

### DTO sem hints

```csharp
var page = criteria
    .FilterBy(new OrderFilter { CustomerName = "Maria" })
    .Select<OrderDto>()
    .UsePages(20, 1)
    .AsSearch()
    .ToList();
```

## Boas Praticas

- Modele filtros como classes pequenas e declarativas.
- Nao passe lambda para `FilterBy`; use classe filtro e atributos.
- Use `Select<TDto>()` para leitura em DTO.
- Use `UseHints(...)` para carregar agregado quando o retorno e entidade.
- Para `AsSearch().ToList()`, configure pagina com `UsePages(...)` ou limite com `Take(...)`.
- Configure sortings e selectors nomeados no startup.
- Evite strings magicas de sorting espalhadas; use nomes registrados.
- Teste `Exists` e `Select<TDto>` quando adicionar hints, pois eles devem permanecer sem includes.

## Antipadroes

- Espalhar `.Include(...)` pelos call sites em vez de registrar hints.
- Usar `UseHints` antes de `Select<TDto>()` esperando carregar navegacoes.
- Usar `AsSearch().ToList()` sem pagina/limite quando espera itens.
- Criar predicates manuais quando `Criterion` cobre o caso.
