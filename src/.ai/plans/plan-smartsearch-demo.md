# Plan: Projeto demo WebAPI do SmartSearch (`smartsearch-demo`)

## Status: CONCLUIDO - Fases 1 a 5 concluidas

## Progresso

`#####` **100%** - 5 de 5 fases

| Fase | Estado |
|---|---|
| Fase 1 - Fechar escopo do demo | Concluida |
| Fase 2 - Criar projeto e infraestrutura | Concluida |
| Fase 3 - Modelar dominio, filtros e DTOs | Concluida |
| Fase 4 - Implementar endpoints de exemplo | Concluida |
| Fase 5 - Documentar e validar o demo | Concluida |

> **Manutencao deste plano:** ao concluir as tarefas de uma fase, marque cada tarefa com `- [x]`,
> troque o **Estado** da fase para `Concluida` na tabela acima e atualize a barra de progresso
> (um caractere `#` por fase concluida, `%` e `X de N`).
> Antes de fechar uma fase, confirme que decisoes, criterios de aceite, testes e invariantes relacionados foram aplicados.

---

## Contexto

### Fontes verificadas

- `.ai/references/template-plan/template-ai-implementation-plan.md` - define o formato exigido para planos orientados a IA.
- `SmartSearch.sln` - contem pacotes principais e projeto de testes, mas nao contem projeto demo.
- `Directory.Build.props` - libs miram `net8.0;net9.0;net10.0` via `LibTargets`/`AspTargets`.
- `RoyalCode.SmartSearch.AspNetCore/Extensions/SearchExtensions.cs` - expoe helpers `MapSearch`, `MapList`, `MapFirst` e `MapSelectFirst`.
- `RoyalCode.SmartSearch.EntityFramework/Extensions/EntityFrameworkSearchesServiceCollectionExtensions.cs` - expoe `AddEntityFrameworkSearches` e `AddEntityFrameworkLikeOperator`.
- `RoyalCode.SmartSearch.EntityFramework.Npgsql/Extensions/NpgsqlSearchesServiceCollectionExtensions.cs` - expoe `AddNpgsqlLikeOperators`.
- `smartsearch.md` - documenta uso manual de `ICriteria`, filtros, sorting, DTO, Operation Hint e operadores.
- `RoyalCode.SmartSearch.Tests/CriteriaOperationHintTests.cs` e `CriteriaUseHintsTests.cs` - mostram configuracao de Operation Hint com SQLite (`ConfigureOperationHints`, `AddIncludesHandler<TEntity, THint>` com `IncludeReference`/`IncludeCollection`, e por-query `UseHints`).
- `RoyalCode.SmartSearch.Tests/ComplexTypeTests.cs` - mostra `[ComplexFilter]` sobre owned/complex type, `[Criterion("MainAddress")]`, path aninhado `[Criterion("Email.Value")]` e OR por nome `[Criterion("FirstNameOrMiddleNameOrLastName")]`, com mapeamento EF `ComplexProperty`/`OwnsOne`.
- `RoyalCode.SmartSearch.Abstractions/{ICriteria,ICriteriaOptions,ISearch,CriterionAttribute,CriterionOperator,CriterionCase,LikeWrap,DisjunctionAttribute,ComplexFilterAttribute}.cs` - superficie de API confirmada para o design do demo.
- `RoyalCode.SmartSearch.Linq/ISearchConfigurations` - expoe `AddSelector`, `AddOrderBy` e `AddSpecifier` usados na configuracao do demo.
- `dotnet test SmartSearch.sln --no-restore -v minimal` - passou em 2026-07-08 com 247 testes em `net10.0`.

### Estado atual do codigo (verificado em 2026-07-08)

- **Nao existe demo:** nao ha projeto `RoyalCode.SmartSearch.Demo` na solution.
- **AspNetCore esta subdocumentado:** o pacote tem helpers de endpoints, mas `smartsearch.md` so o resume como "helpers para endpoints".
- **SQLite ja e usado em testes:** o projeto de testes referencia `Microsoft.EntityFrameworkCore.Sqlite` e usa conexoes in-memory.
- **Operation Hint ja tem exemplo testado:** testes cobrem includes por hints ambiente e por `UseHints`.
- **Npgsql nao tem PostgreSQL real nos testes:** `NpgsqlILikeFactoryTests` valida arvore de expressao, nao execucao em banco real.

### Lacunas, conflitos e restricoes

- **Escopo do demo fechado em Fase 1:** usar somente `net10.0`, project references locais, testes HTTP em projeto dedicado e SQLite in-memory por execucao.
- **Demo nao deve virar pacote:** o projeto deve existir para documentacao e experimentacao, nao para publicacao NuGet.
- **Persistencia local:** SQLite in-memory deve ser usado sem exigir servico externo.
- **Custo de manutencao:** endpoints demais podem transformar o demo em segundo produto.

### Superficies impactadas a mapear

- `RoyalCode.SmartSearch.Demo` - novo projeto WebAPI de exemplo.
- `RoyalCode.SmartSearch.Demo.Tests` - novo projeto de testes HTTP do demo.
- `SmartSearch.sln` - inclusao do projeto demo.
- `Directory.Build.props` ou csproj do demo - target framework e referencias.
- `.ai/plans/plan-api-typos-e-documentacao.md` - pode alterar nomes de API usados no demo.
- `smartsearch.md`/README do demo - exemplos canonicos para IA.

---

## Objetivo

1. Criar um projeto `RoyalCode.SmartSearch.Demo` WebAPI com SQLite e seed local.
2. Demonstrar uso manual de `ICriteria` e uso dos helpers AspNetCore (`MapSearch`, `MapList`, `MapFirst`/`MapSelectFirst`).
3. Demonstrar filtros declarativos cobrindo: igualdade, range numerico/de data, `CriterionOperator.In`, `Like`/`Contains` com `CriterionCase.Insensitive` e `LikeWrap.None`, o trio de OR (OR por nome, `[Disjunction]` e a armadilha `DisableOrFromName`), `[ComplexFilter]` sobre owned type, `TargetPropertyPath` aninhado, `Negation`/`IgnoreIfIsEmpty`, sorting nomeado, os dois modelos de paginacao (`UsePages`/`FetchPage` vs `Skip`/`Take`/`SkipTake`), `UseCount(false)`, as duas formas de projecao (`Select<TDto>()` por convencao + `AddSelector` e `Select<TDto>(expr)`), os terminais `Exists`/`Single`/`FirstOrDefault` e Operation Hint (`ConfigureOperationHints`/`AddIncludesHandler` + `UseHints`).
4. Incluir documentacao executavel para humanos e IA, com uma **matriz de cobertura** (recurso -> filtro/endpoint -> query string -> resultado esperado) e endpoints de exemplo.
5. Validar que o demo compila e sobe sem afetar os pacotes.

## Fora de escopo

- UI frontend.
- Autenticacao/autorizacao.
- PostgreSQL real, Aspire ou Testcontainers.
- Migracoes EF formais para producao.
- Benchmark de performance.
- Implementacao de `GetProjection<T>()`.
- Publicacao do demo como NuGet.

---

## Perguntas ao humano

- **Q1 - Target framework do demo:** qual TFM o projeto demo deve usar?
  - **Opcoes:**
    - **A)** `net10.0`, alinhado ao projeto de testes atual e SDK local.
    - **B)** `net8.0`, mais conservador para consumidores LTS.
  - **Impacto se nao decidir:** a Fase 2 nao deve fechar o csproj.
  - **Resposta:** usar apenas `net10.0`.
  - **Status:** Fechada; ver DF5.

- **Q2 - Referencias do demo:** o demo deve referenciar projetos locais ou pacotes NuGet publicados?
  - **Opcoes:**
    - **A)** Project references locais, util para validar a branch atual.
    - **B)** Package references, util para mostrar consumo real de release.
  - **Impacto se nao decidir:** muda o csproj e o papel do demo na solucao.
  - **Resposta:** usar project references locais.
  - **Status:** Fechada; ver DF6.

- **Q3 - Testes do demo:** adicionar testes com `WebApplicationFactory` para endpoints principais?
  - **Opcoes:**
    - **A)** Sim, criar/estender um projeto de teste para smoke tests HTTP.
    - **B)** Nao agora, validar apenas build e execucao manual documentada.
  - **Impacto se nao decidir:** a Fase 5 fica sem criterio automatizado de endpoint.
  - **Resposta:** sim, criar projeto dedicado `RoyalCode.SmartSearch.Demo.Tests`.
  - **Status:** Fechada; ver DF7.

- **Q4 - Seed do banco:** o demo pode recriar o SQLite local no startup?
  - **Opcoes:**
    - **A)** Sim, apagar/recriar banco demo em ambiente Development.
    - **B)** Nao, apenas criar se nao existir e aplicar seed idempotente.
  - **Impacto se nao decidir:** risco de perda de dados locais ou seed duplicado.
  - **Resposta:** usar sempre banco zerado com SQLite in-memory, mantendo uma conexao unica aberta por execucao para preservar o banco durante a execucao.
  - **Status:** Fechada; ver DF8.

- **Q5 - Mapeamento do `Address`:** como mapear e filtrar o `Address` para demonstrar filtro complexo?
  - **Opcoes:**
    - **A)** Owned type (`OwnsOne`) com `[ComplexFilter]`, filtro por path aninhado na mesma tabela.
    - **B)** Entidade relacionada em tabela separada com join.
  - **Impacto se nao decidir:** muda o que o exemplo de `[ComplexFilter]` demonstra e a complexidade do seed.
  - **Resposta:** usar owned type com `[ComplexFilter]`, seguindo o padrao de `ComplexTypeTests.cs`.
  - **Status:** Fechada; ver DF9.

---

## Decisoes fechadas

- **DF1 - Criar demo WebAPI:** criar um projeto `RoyalCode.SmartSearch.Demo` para exemplos executaveis. Fonte: decisao humana nesta conversa.
- **DF2 - Usar SQLite:** o demo deve usar SQLite como persistencia local. Fonte: decisao humana nesta conversa.
- **DF3 - Usar dominio pequeno de vendas:** usar entidades como `Customer`, `Order`, `OrderItem`, `Product` e `Address` para cobrir filtros, relacionamentos e DTOs. Fonte: proposta aceita nesta conversa.
- **DF4 - Manter demo fora dos pacotes:** o demo deve entrar na solucao como exemplo, nao como pacote NuGet. Fonte: objetivo do demo e estrutura atual de pacotes.
- **DF5 - Target framework do demo:** usar apenas `net10.0`. Fonte: decisao humana nesta conversa.
- **DF6 - Referencias locais:** o demo deve referenciar os projetos locais da solucao, nao pacotes NuGet publicados. Fonte: decisao humana nesta conversa.
- **DF7 - Testes HTTP dedicados:** criar `RoyalCode.SmartSearch.Demo.Tests` para smoke tests dos endpoints com `WebApplicationFactory`. Fonte: decisao humana nesta conversa.
- **DF8 - SQLite in-memory zerado:** usar SQLite in-memory, com banco zerado por execucao e uma conexao unica aberta durante a execucao para manter o banco em memoria. Fonte: decisao humana nesta conversa.
- **DF9 - `Address` como owned type com `[ComplexFilter]`:** mapear `Address` via `OwnsOne` e anotar o tipo/filtro com `[ComplexFilter]`, filtrando por path aninhado (`MainAddress.City` etc.), conforme `ComplexTypeTests.cs`. Fonte: decisao humana nesta conversa (Q5).
- **DF10 - Estrategia pedagogica do demo:** o demo otimiza para "um filtro/endpoint demonstra N recursos" em vez de "um endpoint por recurso", e adota tres artefatos-chave: (a) uma **matriz de cobertura** (recurso -> filtro/endpoint -> query string -> resultado esperado) como entregavel de 1a classe no README; (b) pelo menos uma consulta implementada nas **duas formas** (manual via `ICriteria` e via helper AspNetCore) lado a lado; (c) um **filtro "kitchen-sink"** (`OrderFilter`) fortemente comentado como referencia canonica de copy-paste, mantendo os demais filtros minimos. Fonte: revisao de design nesta conversa. Restringe o risco "demo grande demais".

---

## Historico de decisoes

**Fase 0 (ideia do demo):**

- **Q0 - Criar um projeto demo antes de ampliar docs?** Opcoes consideradas: documentar apenas em Markdown ou criar demo executavel.
  - **Resposta Q0.1:** criar plano para um `RoyalCode.SmartSearch.Demo` WebAPI com entidades, mapeamento, SQLite e endpoints de exemplo.
  - **Conclusao Q0:** DF1, DF2, DF3.

**Fase 1 (fechamento de escopo):**

- **Q1 - Target framework do demo:** usar apenas `net10.0`.
  - **Conclusao Q1:** DF5.
- **Q2 - Referencias do demo:** usar project references locais.
  - **Conclusao Q2:** DF6.
- **Q3 - Testes do demo:** criar projeto dedicado `RoyalCode.SmartSearch.Demo.Tests`.
  - **Conclusao Q3:** DF7.
- **Q4 - Seed do banco:** usar SQLite in-memory zerado por execucao com conexao unica aberta durante a execucao.
  - **Conclusao Q4:** DF8.

**Fase 1.1 (refinamento de design apos revisao da superficie de API):**

- **Q5 - Mapeamento do `Address`:** usar owned type com `[ComplexFilter]`.
  - **Conclusao Q5:** DF9.
- **Revisao de design:** ampliar cobertura de recursos (trio de OR, `In`, `[ComplexFilter]`, `TargetPropertyPath`, dois modelos de paginacao, duas formas de projecao, `Exists`/`Single`, hints via `ConfigureOperationHints`/`AddIncludesHandler`) sem inflar o escopo, via matriz de cobertura, comparacao manual/mapped e filtro kitchen-sink.
  - **Conclusao:** DF10.

---

## Design alvo

### Contratos e bordas

- `RoyalCode.SmartSearch.Demo`: projeto WebAPI de exemplo, incluido na solution.
- `RoyalCode.SmartSearch.Demo.Tests`: projeto de testes HTTP do demo, incluido na solution.
- `AppDbContext`: DbContext SQLite do demo.
- `SearchExtensions` do pacote AspNetCore: usados em endpoints declarativos com `MapSearch`, `MapList`, `MapFirst` e `MapSelectFirst`.
- `ICriteria<TEntity>`: usado em endpoints manuais para demonstrar fluxo sem helpers, incluindo `Exists`/`ExistsAsync`, `Single`/`SingleAsync`, `FirstOrDefault`, `Collect`/`CollectAsync` e `AsSearch().ToListAsync()`.
- `ICriteriaOptions`: `UsePages`/`FetchPage` (paginacao por pagina) vs `Skip`/`Take`/`SkipTake` (paginacao por offset) e `UseCount(false)` (proxima pagina sem total).
- `[Criterion]`: demonstrar `Operator` (`In`, `Contains`, `GreaterThanOrEqual`/`LessThanOrEqual`, `Like`), `Case=Insensitive`, `Wrap=None`, `TargetPropertyPath` aninhado, `Negation` e `IgnoreIfIsEmpty`.
- OR: os tres comportamentos - split automatico por token "Or" no nome, `[Disjunction("alias")]` explicito e `DisableOrFromName` (armadilha com nomes que contem "Or").
- `[ComplexFilter]`: filtro sobre owned type `Address` (path aninhado `MainAddress.City`), conforme DF9.
- Projecao: `Select<TDto>()` por convencao (com `AddSelector` registrado) e `Select<TDto>(expr)` por expressao explicita; contraste com os helpers `MapSearch<TEntity,TDto,TFilter>` que projetam internamente.
- Operation Hint: registro via `ConfigureOperationHints` + `AddIncludesHandler<Order, OrderHints>` (`IncludeReference(o => o.Customer)`, `IncludeCollection(o => o.Items)`) e uso por-query via `UseHints(OrderHints.WithCustomer, ...)`; hints nao se aplicam a `Select<TDto>()` nem a `Exists`.
- `SearchExtensions` de configuracao (`ISearchConfigurations`): `Add<TEntity>()`, `AddSelector<TEntity,TDto>()` e `AddOrderBy<TEntity,TKey>(nome, expr)`.
- `SearchOptions` e `Sorting[]`: usados via query string para paginacao e ordenacao; `OrderBy` invalido resulta em `OrderByException` tratada pelo pipeline como problema HTTP 400.

### Modelo, dados e persistencia

```text
Customer
  Id int key
  Name string required
  Email string required
  MainAddress Address owned (OwnsOne) + [ComplexFilter]

Address  ([ComplexFilter], mapeado via OwnsOne na mesma tabela do Customer)
  Street string
  City string
  State string
  PostalCode string

Product
  Id int key
  Sku string required
  Name string required
  Price decimal required
  Active bool required

Order
  Id int key
  Number string required
  CreatedAt DateTime required
  Status enum required
  CustomerId int required
  Customer navigation
  Items collection<OrderItem>

OrderItem
  Id int key
  OrderId int required
  ProductId int required
  Quantity int required
  UnitPrice decimal required
```

Persistencia do demo:

```text
SQLite in-memory
  Banco zerado por execucao
  Uma conexao SQLite aberta durante a vida do host/test host
  Seed deterministico aplicado no startup da aplicacao
```

### Arquitetura alvo

```text
RoyalCode.SmartSearch.Demo/
  Program.cs
  appsettings.json
  Data/AppDbContext.cs
  Data/DemoSeeder.cs
  Domain/Customer.cs
  Domain/Product.cs
  Domain/Order.cs
  Domain/OrderItem.cs
  Domain/Address.cs
  Filters/CustomerFilter.cs      (Name Contains+Insensitive, NameOrEmail split, [Disjunction], [ComplexFilter] AddressFilter)
  Filters/AddressFilter.cs       ([ComplexFilter] sobre owned Address)
  Filters/ProductFilter.cs       (Active equal, PriceMin/PriceMax range, Sku Like+Wrap=None)
  Filters/OrderFilter.cs         (kitchen-sink: In statuses, range de data, Customer.Name via TargetPropertyPath, Negation, DisableOrFromName)
  Dtos/CustomerDto.cs
  Dtos/ProductDto.cs
  Dtos/OrderSummaryDto.cs
  Search/OrderHints.cs           (enum de hints)
  Search/OperationHintsSetup.cs  (ConfigureOperationHints + AddIncludesHandler<Order, OrderHints>)
  Endpoints/ManualSearchEndpoints.cs   (ICriteria: Exists, Single, Skip/Take, UseCount(false), Select(expr), UseHints)
  Endpoints/MappedSearchEndpoints.cs   (MapSearch/MapList/MapFirst/MapSelectFirst; mesma consulta que um endpoint manual, p/ comparacao)
  README.md                      (inclui a matriz de cobertura de recursos)
  RoyalCode.SmartSearch.Demo.http

RoyalCode.SmartSearch.Demo.Tests/
  DemoApplicationFactory.cs
  CustomersEndpointsTests.cs
  OrdersEndpointsTests.cs
  ProductsEndpointsTests.cs
```

### Seguranca, concorrencia e confiabilidade

- O demo nao deve expor autenticacao falsa nem regras de seguranca de producao.
- O seed deve ser deterministico.
- O SQLite in-memory deve usar uma unica conexao aberta durante a execucao do host para manter o banco vivo.
- O demo nao deve alterar arquivos fora da propria pasta, exceto solution/csproj quando necessario.

### Compatibilidade, migracao e rollout

- O demo nao deve alterar contratos publicos dos pacotes.
- O demo deve acompanhar os nomes corrigidos do plano `api-typos-e-documentacao`, como `[Disjunction]` e `FirstOrDefaultAsync`.
- O demo deve usar referencias locais para validar a branch atual.
- O demo deve ser excluido de empacotamento.

---

## Matriz de cobertura de recursos (alvo)

Entregavel de 1a classe (DF10), materializado no README na Fase 5. As chaves exatas de query string
sao confirmadas na Fase 4 (binding de `SearchOptions`/`Sorting[]`/filtros); ate la sao ilustrativas.

| Recurso | Como demonstrar | Endpoint (alvo) | Query string (ilustrativa) |
|---|---|---|---|
| Igualdade | `[Criterion]` em `ProductFilter.Active` | `GET /products` | `?active=true` |
| Range numerico | `PriceMin`/`PriceMax` (`>=`/`<=`) | `GET /products` | `?priceMin=10&priceMax=100` |
| Range de data | `CreatedAtFrom`/`CreatedAtTo` em `OrderFilter` | `GET /orders` | `?createdAtFrom=2026-01-01&createdAtTo=2026-06-30` |
| `In` (lista) | `CriterionOperator.In` em `OrderFilter.Statuses` | `GET /orders` | `?statuses=Pending&statuses=Paid` |
| Like/Contains + case-insensitive | `[Criterion(Contains, Case=Insensitive)]` em `CustomerFilter.Name` | `GET /customers` | `?name=maria` |
| LikeWrap.None (ancorado) | `[Criterion(Like, Wrap=None)]` em `Sku` | `GET /products` | `?sku=ABC%` |
| OR por nome | `NameOrEmail` (split automatico) | `GET /customers` | `?nameOrEmail=maria` |
| OR por `[Disjunction]` | `[Disjunction("contato")]` em duas props | `GET /customers` | `?...` |
| Armadilha `DisableOrFromName` | prop com "Or" no nome, nao-disjuncao | README (nota) | - |
| `[ComplexFilter]` (owned Address) | `AddressFilter` -> `MainAddress` | `GET /customers` | `?city=NYC` |
| TargetPropertyPath aninhado | `[Criterion("Customer.Name")] CustomerName` | `GET /orders` | `?customerName=maria` |
| Negation | `[Criterion(Negation=true)]` | `GET /orders` | `?...` |
| Sorting nomeado | `AddOrderBy("total", ...)` | qualquer | `?sort=total desc` |
| Paginacao (UsePages) | helper padrao | `GET /orders` | `?page=1&itemsPerPage=2` |
| Paginacao (Skip/Take) | endpoint manual | `GET /manual/orders` | `?skip=20&take=20` |
| UseCount(false) | endpoint manual sem total | `GET /manual/orders` | `?count=false` |
| Projecao convencao + `AddSelector` | `MapSearch<Order,OrderSummaryDto,OrderFilter>` | `GET /orders/summary` | `?...` |
| Projecao `Select<TDto>(expr)` | endpoint manual | `GET /manual/orders/summary` | `?...` |
| `Exists` | endpoint manual | `GET /manual/orders/exists` | `?number=...` |
| `Single` | endpoint manual por id | `GET /manual/orders/{id}` | - |
| `FirstOrDefault` | `MapFirst`/`MapSelectFirst` | `GET /orders/first` | `?...` |
| Hint carrega navegacao | `UseHints(OrderHints.WithCustomer)` | `GET /manual/orders/{id}` | `?hints=WithCustomer` |
| Hint ignorado em DTO | `MapSearch` de DTO nao usa hint | `GET /orders/summary` | - |
| Manual vs mapped (mesma consulta) | orders por `CustomerName` + data | `GET /manual/orders` e `GET /orders` | (mesma) |
| OrderBy invalido -> 400 | pipeline `OrderByException` | qualquer | `?sort=campoInexistente` |

---

## Ordem de execucao

1. **Fase 1 (Fechar escopo do demo)** - resolve perguntas que bloqueiam csproj, seed e testes.
2. **Fase 2 (Criar projeto e infraestrutura)** - cria WebAPI, DbContext, SQLite, Swagger e wiring de DI.
3. **Fase 3 (Modelar dominio, filtros e DTOs)** - cria entidades e configuracoes SmartSearch.
4. **Fase 4 (Implementar endpoints de exemplo)** - demonstra usos manual e AspNetCore.
5. **Fase 5 (Documentar e validar o demo)** - README, `.http`, build/test e smoke checks.

Build/test padrao:

```powershell
dotnet build SmartSearch.sln --no-restore
dotnet test SmartSearch.sln --no-restore -v minimal
dotnet run --project .\RoyalCode.SmartSearch.Demo\RoyalCode.SmartSearch.Demo.csproj
```

---

## Fase 1 - Fechar escopo do demo

**Depende de:** Q1, Q2, Q3, Q4.

**Escopo:** plano e decisoes de implementacao.

**O que/como:** obter respostas humanas para target framework, tipo de referencia, testes HTTP e seed. Atualizar `Decisoes fechadas`, `Historico de decisoes`, fases dependentes e matriz.

**Tarefas:**

- [x] Registrar resposta de Q1 como decisao fechada.
- [x] Registrar resposta de Q2 como decisao fechada.
- [x] Registrar resposta de Q3 como decisao fechada.
- [x] Registrar resposta de Q4 como decisao fechada.
- [x] Atualizar fases 2 a 5 se alguma decisao alterar escopo.

**Criterios de aceite:** nao ha perguntas abertas que bloqueiem criacao do projeto, seed ou validacao.

**Testes:** nao aplicavel; fase de decisao.

### Resultado da Fase 1

Concluida em 2026-07-08.

Entregaveis:

- Q1 fechada: demo usara somente `net10.0`.
- Q2 fechada: demo usara project references locais.
- Q3 fechada: sera criado projeto dedicado `RoyalCode.SmartSearch.Demo.Tests`.
- Q4 fechada: demo usara SQLite in-memory, banco zerado por execucao e conexao unica aberta durante a execucao.
- Fases dependentes atualizadas para refletir target framework, referencias, testes e persistencia.

Arquivos alterados:

- `.ai/plans/plan-smartsearch-demo.md`.

Decisoes aplicadas:

- DF5.
- DF6.
- DF7.
- DF8.

Verificacao:

- Nao aplicavel; fase de decisao.

Desvios:

- Q4 foi fechada com alternativa mais especifica que as opcoes originais: SQLite in-memory, sem arquivo local.

Pendencias:

- Nenhuma pergunta bloqueante permanece aberta.

---

## Fase 2 - Criar projeto e infraestrutura

**Depende de:** Fase 1, DF1, DF2, DF4, DF5, DF6, DF8.

**Escopo:** novo projeto `RoyalCode.SmartSearch.Demo`, solution, configuracao de DI e SQLite.

**O que/como:** criar WebAPI, adicionar ao `SmartSearch.sln`, configurar SQLite, Swagger/OpenAPI, EF Core e SmartSearch.

**Tarefas:**

- [x] Criar projeto WebAPI `RoyalCode.SmartSearch.Demo` em `net10.0`.
- [x] Adicionar o projeto ao `SmartSearch.sln`.
- [x] Configurar project references locais para os projetos SmartSearch necessarios.
- [x] Adicionar dependencias de SQLite e Swagger se necessario.
- [x] Criar `AppDbContext` e configuracao de connection string.
- [x] Configurar `AddDbContext<AppDbContext>()` com SQLite in-memory.
- [x] Configurar conexao SQLite unica aberta durante a vida do host.
- [x] Configurar `AddEntityFrameworkSearches<AppDbContext>()`.
- [x] Configurar `AddEntityFrameworkLikeOperator()`.
- [x] Configurar seed deterministico em banco zerado por execucao.
- [x] Garantir que o demo nao gere pacote NuGet.

**Criterios de aceite:** `dotnet build SmartSearch.sln --no-restore` compila com o novo projeto; `dotnet run --project .\RoyalCode.SmartSearch.Demo\RoyalCode.SmartSearch.Demo.csproj` sobe localmente.

**Testes:** `dotnet build SmartSearch.sln --no-restore`.

### Resultado da Fase 2

Concluida em 2026-07-08.

Entregaveis:

- Projeto `RoyalCode.SmartSearch.Demo` (WebAPI, `Microsoft.NET.Sdk.Web`) criado e adicionado ao `SmartSearch.sln` (pasta de solucao `Samples`).
- SQLite in-memory com conexao unica aberta (singleton) e `AddDbContext<AppDbContext>` sobre ela; `EnsureCreated` + seed deterministico no startup (DF8).
- DI de busca via `AddDemoSearches()`: `AddEntityFrameworkSearches<AppDbContext>`, `AddEntityFrameworkLikeOperator`, `ConfigureOperationHints`.
- OpenAPI via `AddOpenApi`/`MapOpenApi`; `AddProblemDetails`; raiz `/` redireciona para o documento OpenAPI.
- `IsPackable=false` (DF4); project references locais (DF6).

Verificacao:

- `dotnet build RoyalCode.SmartSearch.Demo` -> 0 erros, 0 avisos.
- `dotnet run` sobe em `http://localhost:5080` e serve `/openapi/v1.json` (200).

Desvios:

- **TFM plural obrigatorio:** `Directory.Build.props` define `AspVer`/`EFVer` condicionais a `TargetFramework`, que so resolve no inner build por-TFM. Usar `<TargetFramework>` singular quebrou o restore (NU1015); a solucao foi `<TargetFrameworks>net10.0</TargetFrameworks>` (plural), como os demais projetos.
- **Advisory Microsoft.OpenApi (NU1903):** o transitivo `Microsoft.OpenApi` 2.0.0 tem advisory; foi promovido para `2.4.0` (ainda no range) e por fim `2.10.0` (dentro da major 2.x, sem quebra), zerando avisos.

---

## Fase 3 - Modelar dominio, filtros e DTOs

**Depende de:** Fase 2, DF3, DF9, DF10.

**Escopo:** entidades, DbContext mappings, filtros, DTOs, selectors, order-bys e hints.

**O que/como:** criar dominio pequeno de vendas com dados suficientes para demonstrar os recursos do SmartSearch, seguindo DF10 (um filtro/endpoint demonstra N recursos).

**Tarefas:**

- [x] Criar entidades `Customer`, `Address`, `Product`, `Order` e `OrderItem` e o enum `OrderStatus`.
- [x] Mapear entidades no `AppDbContext`; mapear `Address` como owned via `OwnsOne` e anota-lo com `[ComplexFilter]` (DF9).
- [x] `CustomerFilter`: `Name` com `[Criterion(Contains, Case=Insensitive)]`; OR por nome (`NameOrEmail`); um par `[Disjunction("contato")]`; `[ComplexFilter] AddressFilter Address`.
- [x] `AddressFilter`: `[ComplexFilter]` sobre owned `Address`, filtrando por `City`/`State` (path aninhado).
- [x] `ProductFilter`: `Active` (igualdade), `PriceMin`/`PriceMax` (range via `GreaterThanOrEqual`/`LessThanOrEqual`), `Sku` com `[Criterion(Like, Wrap=None)]`.
- [x] `OrderFilter` (kitchen-sink, fortemente comentado): `Statuses` com `CriterionOperator.In`; `CreatedAtFrom`/`CreatedAtTo` (range de data); `CustomerName` com `[Criterion("Customer.Name")]` (TargetPropertyPath aninhado); um exemplo de `Negation`; um exemplo de `DisableOrFromName` documentando a armadilha do token "Or".
- [x] Criar DTOs `CustomerDto`, `ProductDto` e `OrderSummaryDto`.
- [x] Registrar selector por convencao via `AddSelector` (para `OrderSummaryDto`) e demonstrar `Select<TDto>(expr)` explicito em um endpoint manual.
- [x] Registrar sortings nomeados via `AddOrderBy` (ex.: `total`, `createdAt`).
- [x] Criar `Search/OrderHints.cs` (enum) e `Search/OperationHintsSetup.cs` com `ConfigureOperationHints` + `AddIncludesHandler<Order, OrderHints>` usando `IncludeReference(o => o.Customer)` e `IncludeCollection(o => o.Items)`.
- [x] Popular seed deterministico com casos que provem: OR (nome e disjunction), case-insensitive, `In` de status, ranges, `[ComplexFilter]` de Address, sorting e paginacao.

**Criterios de aceite:** o demo tem exemplos compilaveis para cada recurso da matriz de cobertura; seed contem dados que retornam resultados distintos para os filtros documentados.

**Testes:** `dotnet build SmartSearch.sln --no-restore`.

### Resultado da Fase 3

Concluida em 2026-07-08.

Entregaveis:

- Entidades `Customer`, `Address`, `Product`, `Order`, `OrderItem` e enum `OrderStatus`; `Address` owned via `OwnsOne` + `[ComplexFilter]` (DF9).
- Filtros: `CustomerFilter` (Contains+Insensitive, OR por nome, path aninhado), `AddressFilter`/`CustomerAddressFilter` (`[ComplexFilter]`), `ProductFilter` (igualdade, range, Like `Wrap=None`, `[Disjunction]`), `OrderFilter` (kitchen-sink: range de data, `Customer.Name` aninhado, Negation, `DisableOrFromName`), `OrderStatusesFilter` (`In` com `IEnumerable<>`), `OrderLookupFilters` (Equal).
- DTOs: `ProductDto` (convencao), `CustomerDto` e `OrderSummaryDto` (selectors registrados via `AddSelector`).
- `AddOrderBy` nomeados (`createdAt`, `number`, `customer`, `price`, `name`); `OrderHints` + `ConfigureOperationHints`/`AddIncludesHandler` com `IncludeReference`/`IncludeCollection`.
- Seed deterministico com 5 clientes, 5 produtos, 5 pedidos, cobrindo OR, case-insensitive, In, ranges, complex filter, sorting e paginacao.

Verificacao:

- `dotnet build` do demo -> 0 erros. Comportamento confirmado em runtime na Fase 4.

Desvios:

- **`In` exige `IEnumerable<T>` exato:** `List<T>`/`T[]` lancam `InvalidOperationException`. Por isso a propriedade e `IEnumerable<OrderStatus>?` e o endpoint manual recebe um array e atribui a ela.

---

## Fase 4 - Implementar endpoints de exemplo

**Depende de:** Fase 3, DF10.

**Escopo:** endpoints Minimal API manuais e endpoints via helpers AspNetCore.

**O que/como:** criar endpoints pequenos e nomeados, separados por grupos, com exemplos que uma IA possa copiar para outros projetos.

**Tarefas:**

- [x] Criar grupo `/manual/customers` usando `ICriteria<Customer>` diretamente (FilterBy + Collect/ToListAsync).
- [x] Criar grupo `/manual/orders` demonstrando `UseHints`, `Select<TDto>(expr)`, paginacao por `Skip`/`Take`, `UseCount(false)`, `OrderBy` e `AsSearch().ToListAsync()`.
- [x] Criar `/manual/orders/exists` (`ExistsAsync`) e `/manual/orders/{id}` (`SingleAsync`) para demonstrar os terminais e contrastar com `FirstOrDefault`.
- [x] Escolher uma consulta (ex.: orders por `CustomerName` + range de data) e implementa-la nas **duas formas** (manual e via helper) para comparacao lado a lado (DF10).
- [x] Criar endpoints via `MapSearch` para lista paginada de DTO (`UsePages`).
- [x] Criar endpoints via `MapList` para lista simples.
- [x] Criar endpoints via `MapFirst` ou `MapSelectFirst` para primeiro item.
- [x] Demonstrar query string de `SearchOptions` e `Sorting[]` e confirmar as chaves exatas de binding (fecha a coluna "query string" da matriz de cobertura).
- [x] Garantir que endpoints de DTO nao dependem de `UseHints` (hint ignorado em projecao).
- [x] Garantir que endpoints de entidade com hints carregam navegacoes esperadas (`Customer`, `Items`).
- [x] Tratar casos de 204 e erro de order by invalido (`OrderByException` -> ProblemDetails 400) pelo pipeline existente.

**Criterios de aceite:** cada familia de endpoint tem ao menos um exemplo que retorna 200 com dados seeded; a consulta espelhada manual/mapped retorna o mesmo resultado; order by invalido retorna problema 400 quando passar pelo `Performer`.

**Testes:** `dotnet run --project .\RoyalCode.SmartSearch.Demo\RoyalCode.SmartSearch.Demo.csproj` e chamadas manuais documentadas no `.http`.

### Resultado da Fase 4

Concluida em 2026-07-08.

Entregaveis:

- Grupo manual `/manual/*` (`ICriteria`): `customers` (Select expr), `customers/by-address` (`[ComplexFilter]` + `Select<CustomerDto>()`), `orders` (espelho do mapeado), `orders/by-status` (`In`), `orders/page` (`Skip`/`Take` + `UseCount(false)`), `orders/exists` (`Exists`), `orders/by-number/{number}` (`Single` + hints), `orders/{id}` (`FirstOrDefault` + hints).
- Grupo mapeado (helpers): `MapSearch<Order,OrderSummaryDto,OrderFilter>` (`/orders`), `MapSearch<Customer,CustomerDto,CustomerFilter>` (`/customers`), `MapList<Product,ProductDto,ProductFilter>` (`/products`), `MapFirst<Product,ProductFilter>` (`/products/first`), `MapSelectFirst<Customer,CustomerDto,CustomerFilter>` (`/customers/first`).
- Query strings de `SearchOptions` (`page`/`itemsPerPage`/`skip`/`take`/`count`) e `Sorting[]` (`orderby=<nome>-desc`) confirmadas.

Verificacao (smoke via curl em `http://localhost:5080`, todos 200/esperado):

- Contains+Insensitive, OR por nome, path aninhado owned, range numerico/de data, `In`, Like `Wrap=None` ancorado, `[Disjunction]`, Negation, sorting nomeado.
- `[ComplexFilter]` (`by-address`) retorna owned filtrado; hints carregam `customer`+`items` e deixam `items[].product` = null (nao pedido); `Exists` true/false; `Single` por numero unico; `FirstOrDefault` id inexistente -> 404.
- `Skip/Take` + `UseCount(false)` retorna `count:0` (nao computado).
- Order by invalido -> 400 ProblemDetails (SmartProblems) com `propertyName`/`typeName`/`pointer`.
- Endpoint manual `/manual/orders` e mapeado `/orders` retornam JSON identico para a mesma query (DF10).

Desvios:

- **Binding de filtros complexos:** `[AsParameters]` nao popula objetos aninhados; o exemplo de `[ComplexFilter]` fica no endpoint manual (constroi o filtro a mao). Os endpoints mapeados usam filtros planos.

---

## Fase 5 - Documentar e validar o demo

**Depende de:** Fase 4, DF7.

**Escopo:** README do demo, arquivo `.http`, possiveis smoke tests.

**O que/como:** criar documentacao operacional curta e validacao automatica com testes HTTP dedicados, conforme DF7.

**Tarefas:**

- [x] Criar `RoyalCode.SmartSearch.Demo/README.md` com objetivo, setup, endpoints e exemplos.
- [x] Incluir no README a **matriz de cobertura** (recurso -> filtro/endpoint -> query string -> resultado esperado) preenchida com as chaves confirmadas na Fase 4.
- [x] Documentar a forma do `ProblemDetails` para `OrderBy` invalido (`OrderByException` -> 400).
- [x] Criar `RoyalCode.SmartSearch.Demo/RoyalCode.SmartSearch.Demo.http` com chamadas principais (incluindo a consulta espelhada manual/mapped).
- [x] Documentar quais endpoints mostram cada recurso do SmartSearch.
- [x] Criar projeto `RoyalCode.SmartSearch.Demo.Tests`.
- [x] Adicionar testes `WebApplicationFactory` para smoke tests HTTP dos endpoints principais.
- [x] Executar `dotnet build SmartSearch.sln --no-restore`.
- [x] Executar `dotnet test SmartSearch.sln --no-restore -v minimal`.
- [x] Executar o demo e validar ao menos uma chamada por familia de endpoint.
- [x] Atualizar `smartsearch.md` para apontar o demo como referencia executavel, se desejado.

**Criterios de aceite:** uma pessoa ou IA consegue subir o demo e chamar endpoints documentados sem ler codigo interno; build e testes passam.

**Testes:** comandos padrao e smoke checks definidos no README/.http.

### Resultado da Fase 5

Concluida em 2026-07-08.

Entregaveis:

- `RoyalCode.SmartSearch.Demo/README.md` com objetivo, setup, os dois modos de busca, a **matriz de cobertura** preenchida (recurso -> endpoint -> query string -> resultado, com chaves confirmadas), e notas sobre o trio de OR, `[ComplexFilter]`, hints e o `ProblemDetails` de order by invalido.
- `RoyalCode.SmartSearch.Demo.http` com chamadas para todas as familias (manual e mapeado), incluindo a consulta espelhada.
- Projeto `RoyalCode.SmartSearch.Demo.Tests` (`WebApplicationFactory<Program>`) com 19 smoke tests (customers, products, orders), incluindo a igualdade manual-vs-mapped e o 400 de order by invalido; adicionado ao `SmartSearch.sln`.

Verificacao:

- `dotnet build SmartSearch.sln --no-restore` -> 0 erros (7 avisos CS8618 pre-existentes no projeto de testes da lib).
- `dotnet test SmartSearch.sln --no-build` -> **266 aprovados** (247 lib + 19 demo), 0 falhas.

Desvios:

- **Maps estaticos globais:** `SelectorsMap`/`OrderByHandlersMap` sao singletons de processo; configurar dois hosts no mesmo processo lanca "Selector already exists". Os testes usam uma unica factory compartilhada via `ICollectionFixture` (um host por processo), o que tambem reflete o uso real (configurar uma vez).

---

## Matriz de rastreabilidade

| Objetivo | Fase(s) | Decisao(es) | Criterio(s) de aceite | Teste(s) |
|---|---|---|---|---|
| Objetivo 1 | Fase 2 | DF1, DF2, DF4, DF5, DF6, DF8 | projeto demo compila e sobe com SQLite in-memory | `dotnet build`, `dotnet run` |
| Objetivo 2 | Fase 4 | DF1 | endpoints manuais e helpers existem e retornam dados | smoke checks HTTP |
| Objetivo 3 | Fase 3, Fase 4 | DF3, DF9, DF10 | cada recurso da matriz de cobertura tem filtro/endpoint compilavel | `dotnet build`, smoke checks |
| Objetivo 4 | Fase 5 | DF7 | README, `.http` e testes HTTP cobrem os endpoints | revisao dos arquivos, smoke checks, `dotnet test` |
| Objetivo 5 | Fase 5 | DF4 | demo nao altera empacotamento dos pacotes | `dotnet build`, `dotnet test` |

---

## Invariantes a preservar

1. O demo nao pode alterar contratos publicos dos pacotes SmartSearch.
2. O demo nao pode exigir banco externo ou credenciais.
3. O demo nao pode ser empacotado como NuGet.
4. O seed deve ser deterministico e suficiente para validar os exemplos.
5. O demo deve usar nomes de API corrigidos pelo plano de typos quando esse plano for aplicado.

---

## Criterios globais de conclusao

- `RoyalCode.SmartSearch.Demo` existe na solution e compila.
- `RoyalCode.SmartSearch.Demo.Tests` existe na solution e valida endpoints principais.
- O demo sobe com SQLite in-memory e Swagger/OpenAPI.
- O README do demo explica setup e exemplos de chamadas.
- O `.http` tem chamadas para manual criteria, `MapSearch`, `MapList`, `MapFirst`/`MapSelectFirst`.
- `dotnet build SmartSearch.sln --no-restore` passa.
- `dotnet test SmartSearch.sln --no-restore -v minimal` passa.

---

## Riscos

| Risco | Gatilho | Impacto | Mitigacao | Estado |
|---|---|---|---|---|
| Demo grande demais | Muitas entidades/endpoints sem criterio | Manutencao cara | Dominio de vendas fixo (5 entidades); "um filtro/endpoint demonstra N recursos" + matriz de cobertura em vez de um endpoint por recurso | Mitigado por DF10 |
| Seed destrutivo | Startup apaga SQLite com dados locais | Perda de dados de experimentos | Usar SQLite in-memory zerado por execucao, sem arquivo local | Mitigado por DF8 |
| Demo diverge da API pre-1.0 | Plano de typos altera nomes depois do demo | Exemplos quebram | Plano de typos ja concluido antes da implementacao do demo | Mitigado |
| Sem teste HTTP | Endpoints mudam sem validacao automatica | Regressao nos endpoints nao detectada por CI | Criar `RoyalCode.SmartSearch.Demo.Tests` com `WebApplicationFactory` | Mitigado por DF7 |
| Referencias por pacote ficam desatualizadas | Pacote local nao publicado ou versao externa diverge da branch | Demo nao compila na branch | Usar project references locais | Mitigado por DF6 |

---

## Diferidos e backlog

- Adicionar demo PostgreSQL/Npgsql com `ILIKE` real - destino: plano futuro.
- Adicionar Aspire/Testcontainers para demo com multiplos providers - destino: plano futuro.
- Demonstrar `GetProjection<T>()` quando a funcionalidade existir - destino: plano futuro de projections.
- Publicar exemplos de curl no `smartsearch.md` principal - destino: revisao de docs apos demo.

---

## Referencias

- `.ai/references/template-plan/template-ai-implementation-plan.md`.
- `SmartSearch.sln`.
- `Directory.Build.props`.
- `smartsearch.md`.
- `RoyalCode.SmartSearch.AspNetCore/Extensions/SearchExtensions.cs`.
- `RoyalCode.SmartSearch.EntityFramework/Extensions/EntityFrameworkSearchesServiceCollectionExtensions.cs`.
- `RoyalCode.SmartSearch.EntityFramework.Npgsql/Extensions/NpgsqlSearchesServiceCollectionExtensions.cs`.
- `RoyalCode.SmartSearch.Tests/CriteriaOperationHintTests.cs`.
- `RoyalCode.SmartSearch.Tests/CriteriaUseHintsTests.cs`.
- `RoyalCode.SmartSearch.Tests/ComplexTypeTests.cs`.
- `RoyalCode.SmartSearch.Abstractions/{ICriteria,ICriteriaOptions,ISearch,CriterionAttribute,CriterionOperator,CriterionCase,LikeWrap,DisjunctionAttribute,ComplexFilterAttribute}.cs`.
