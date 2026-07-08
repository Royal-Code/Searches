# Plan: Projeto demo WebAPI do SmartSearch (`smartsearch-demo`)

## Status: RASCUNHO - aguardando decisoes de escopo

## Progresso

`-----` **0%** - 0 de 5 fases

| Fase | Estado |
|---|---|
| Fase 1 - Fechar escopo do demo | Pendente |
| Fase 2 - Criar projeto e infraestrutura | Pendente |
| Fase 3 - Modelar dominio, filtros e DTOs | Pendente |
| Fase 4 - Implementar endpoints de exemplo | Pendente |
| Fase 5 - Documentar e validar o demo | Pendente |

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
- `RoyalCode.SmartSearch.Tests/CriteriaOperationHintTests.cs` e `CriteriaUseHintsTests.cs` - mostram configuracao de Operation Hint com SQLite.
- `dotnet test SmartSearch.sln --no-restore -v minimal` - passou em 2026-07-08 com 247 testes em `net10.0`.

### Estado atual do codigo (verificado em 2026-07-08)

- **Nao existe demo:** nao ha projeto `RoyalCode.SmartSearch.Demo` na solution.
- **AspNetCore esta subdocumentado:** o pacote tem helpers de endpoints, mas `smartsearch.md` so o resume como "helpers para endpoints".
- **SQLite ja e usado em testes:** o projeto de testes referencia `Microsoft.EntityFrameworkCore.Sqlite` e usa conexoes in-memory.
- **Operation Hint ja tem exemplo testado:** testes cobrem includes por hints ambiente e por `UseHints`.
- **Npgsql nao tem PostgreSQL real nos testes:** `NpgsqlILikeFactoryTests` valida arvore de expressao, nao execucao em banco real.

### Lacunas, conflitos e restricoes

- **Escopo do demo ainda nao fechado:** falta confirmar target framework, referencias por projeto/pacote, cobertura por testes e comportamento de seed.
- **Demo nao deve virar pacote:** o projeto deve existir para documentacao e experimentacao, nao para publicacao NuGet.
- **Persistencia local:** SQLite deve ser usado sem exigir servico externo.
- **Custo de manutencao:** endpoints demais podem transformar o demo em segundo produto.

### Superficies impactadas a mapear

- `RoyalCode.SmartSearch.Demo` - novo projeto WebAPI de exemplo.
- `SmartSearch.sln` - inclusao do projeto demo.
- `Directory.Build.props` ou csproj do demo - target framework e referencias.
- `.ai/plans/plan-api-typos-e-documentacao.md` - pode alterar nomes de API usados no demo.
- `smartsearch.md`/README do demo - exemplos canonicos para IA.

---

## Objetivo

1. Criar um projeto `RoyalCode.SmartSearch.Demo` WebAPI com SQLite e seed local.
2. Demonstrar uso manual de `ICriteria` e uso dos helpers AspNetCore (`MapSearch`, `MapList`, `MapFirst`/`MapSelectFirst`).
3. Demonstrar filtros declarativos, OR/disjunction, filtros complexos, sorting, paginacao, selectors, DTOs, `Like`/`Contains`/case-insensitive e Operation Hint.
4. Incluir documentacao executavel para humanos e IA, com endpoints e query strings de exemplo.
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
  - **Status:** Aberta.

- **Q2 - Referencias do demo:** o demo deve referenciar projetos locais ou pacotes NuGet publicados?
  - **Opcoes:**
    - **A)** Project references locais, util para validar a branch atual.
    - **B)** Package references, util para mostrar consumo real de release.
  - **Impacto se nao decidir:** muda o csproj e o papel do demo na solucao.
  - **Status:** Aberta.

- **Q3 - Testes do demo:** adicionar testes com `WebApplicationFactory` para endpoints principais?
  - **Opcoes:**
    - **A)** Sim, criar/estender um projeto de teste para smoke tests HTTP.
    - **B)** Nao agora, validar apenas build e execucao manual documentada.
  - **Impacto se nao decidir:** a Fase 5 fica sem criterio automatizado de endpoint.
  - **Status:** Aberta.

- **Q4 - Seed do banco:** o demo pode recriar o SQLite local no startup?
  - **Opcoes:**
    - **A)** Sim, apagar/recriar banco demo em ambiente Development.
    - **B)** Nao, apenas criar se nao existir e aplicar seed idempotente.
  - **Impacto se nao decidir:** risco de perda de dados locais ou seed duplicado.
  - **Status:** Aberta.

---

## Decisoes fechadas

- **DF1 - Criar demo WebAPI:** criar um projeto `RoyalCode.SmartSearch.Demo` para exemplos executaveis. Fonte: decisao humana nesta conversa.
- **DF2 - Usar SQLite:** o demo deve usar SQLite como persistencia local. Fonte: decisao humana nesta conversa.
- **DF3 - Usar dominio pequeno de vendas:** usar entidades como `Customer`, `Order`, `OrderItem`, `Product` e `Address` para cobrir filtros, relacionamentos e DTOs. Fonte: proposta aceita nesta conversa.
- **DF4 - Manter demo fora dos pacotes:** o demo deve entrar na solucao como exemplo, nao como pacote NuGet. Fonte: objetivo do demo e estrutura atual de pacotes.

---

## Historico de decisoes

**Fase 0 (ideia do demo):**

- **Q0 - Criar um projeto demo antes de ampliar docs?** Opcoes consideradas: documentar apenas em Markdown ou criar demo executavel.
  - **Resposta Q0.1:** criar plano para um `RoyalCode.SmartSearch.Demo` WebAPI com entidades, mapeamento, SQLite e endpoints de exemplo.
  - **Conclusao Q0:** DF1, DF2, DF3.

---

## Design alvo

### Contratos e bordas

- `RoyalCode.SmartSearch.Demo`: projeto WebAPI de exemplo, incluido na solution.
- `AppDbContext`: DbContext SQLite do demo.
- `SearchExtensions` do pacote AspNetCore: usados em endpoints declarativos com `MapSearch`, `MapList`, `MapFirst` e `MapSelectFirst`.
- `ICriteria<TEntity>`: usado em endpoints manuais para demonstrar fluxo sem helpers.
- `OperationHint`: usado para mostrar includes por hint em retorno de entidade.
- `SearchOptions` e `Sorting[]`: usados via query string para paginacao e ordenacao.

### Modelo, dados e persistencia

```text
Customer
  Id int key
  Name string required
  Email string required
  MainAddress Address owned/complex

Address
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
  Filters/CustomerFilter.cs
  Filters/ProductFilter.cs
  Filters/OrderFilter.cs
  Dtos/CustomerDto.cs
  Dtos/ProductDto.cs
  Dtos/OrderSummaryDto.cs
  Search/OrderHints.cs
  Endpoints/ManualSearchEndpoints.cs
  Endpoints/MappedSearchEndpoints.cs
  README.md
  RoyalCode.SmartSearch.Demo.http
```

### Seguranca, concorrencia e confiabilidade

- O demo nao deve expor autenticacao falsa nem regras de seguranca de producao.
- O seed deve ser deterministico.
- O SQLite local deve ficar dentro da pasta do demo ou em caminho configuravel.
- O demo nao deve alterar arquivos fora da propria pasta, exceto solution/csproj quando necessario.

### Compatibilidade, migracao e rollout

- O demo nao deve alterar contratos publicos dos pacotes.
- O demo deve acompanhar os nomes corrigidos do plano `api-typos-e-documentacao`, como `[Disjunction]` e `FirstOrDefaultAsync`.
- O demo deve usar referencias locais ou pacotes conforme Q2.
- O demo deve ser excluido de empacotamento.

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

- [ ] Registrar resposta de Q1 como decisao fechada.
- [ ] Registrar resposta de Q2 como decisao fechada.
- [ ] Registrar resposta de Q3 como decisao fechada.
- [ ] Registrar resposta de Q4 como decisao fechada.
- [ ] Atualizar fases 2 a 5 se alguma decisao alterar escopo.

**Criterios de aceite:** nao ha perguntas abertas que bloqueiem criacao do projeto, seed ou validacao.

**Testes:** nao aplicavel; fase de decisao.

### Resultado da Fase 1

*a preencher*

---

## Fase 2 - Criar projeto e infraestrutura

**Depende de:** Fase 1, DF1, DF2, DF4.

**Escopo:** novo projeto `RoyalCode.SmartSearch.Demo`, solution, configuracao de DI e SQLite.

**O que/como:** criar WebAPI, adicionar ao `SmartSearch.sln`, configurar SQLite, Swagger/OpenAPI, EF Core e SmartSearch.

**Tarefas:**

- [ ] Criar projeto WebAPI `RoyalCode.SmartSearch.Demo`.
- [ ] Adicionar o projeto ao `SmartSearch.sln`.
- [ ] Configurar referencias conforme Q2.
- [ ] Adicionar dependencias de SQLite e Swagger se necessario.
- [ ] Criar `AppDbContext` e configuracao de connection string.
- [ ] Configurar `AddDbContext<AppDbContext>()`.
- [ ] Configurar `AddEntityFrameworkSearches<AppDbContext>()`.
- [ ] Configurar `AddEntityFrameworkLikeOperator()`.
- [ ] Configurar seed conforme Q4.
- [ ] Garantir que o demo nao gere pacote NuGet.

**Criterios de aceite:** `dotnet build SmartSearch.sln --no-restore` compila com o novo projeto; `dotnet run --project .\RoyalCode.SmartSearch.Demo\RoyalCode.SmartSearch.Demo.csproj` sobe localmente.

**Testes:** `dotnet build SmartSearch.sln --no-restore`.

### Resultado da Fase 2

*a preencher*

---

## Fase 3 - Modelar dominio, filtros e DTOs

**Depende de:** Fase 2, DF3.

**Escopo:** entidades, DbContext mappings, filtros, DTOs, selectors, order-bys e hints.

**O que/como:** criar dominio pequeno de vendas com dados suficientes para demonstrar os recursos do SmartSearch.

**Tarefas:**

- [ ] Criar entidades `Customer`, `Address`, `Product`, `Order` e `OrderItem`.
- [ ] Mapear entidades no `AppDbContext`, incluindo owned/complex `Address` se aplicavel.
- [ ] Criar enum `OrderStatus`.
- [ ] Criar filtros `CustomerFilter`, `ProductFilter` e `OrderFilter`.
- [ ] Demonstrar `[Disjunction]` em pelo menos um filtro.
- [ ] Demonstrar filtro complexo para `Address`.
- [ ] Demonstrar range de data e range numerico.
- [ ] Demonstrar `CriterionOperator.Contains`, `LikeWrap.None` e `CriterionCase.Insensitive`.
- [ ] Criar DTOs `CustomerDto`, `ProductDto` e `OrderSummaryDto`.
- [ ] Registrar selectors via `AddSelector`.
- [ ] Registrar sortings nomeados via `AddOrderBy`.
- [ ] Registrar hints de entidade para `Order` com `WithCustomer` e `WithItems`.
- [ ] Popular seed deterministico com casos que provem OR, case-insensitive, sorting e paginacao.

**Criterios de aceite:** o demo tem exemplos compilaveis para cada recurso listado; seed contem dados que retornam resultados distintos para os filtros documentados.

**Testes:** `dotnet build SmartSearch.sln --no-restore`.

### Resultado da Fase 3

*a preencher*

---

## Fase 4 - Implementar endpoints de exemplo

**Depende de:** Fase 3.

**Escopo:** endpoints Minimal API manuais e endpoints via helpers AspNetCore.

**O que/como:** criar endpoints pequenos e nomeados, separados por grupos, com exemplos que uma IA possa copiar para outros projetos.

**Tarefas:**

- [ ] Criar grupo `/manual/customers` usando `ICriteria<Customer>` diretamente.
- [ ] Criar grupo `/manual/orders` demonstrando `UseHints`, `Select<TDto>()`, `UsePages`, `OrderBy` e `AsSearch().ToListAsync()`.
- [ ] Criar endpoints via `MapSearch` para lista paginada de DTO.
- [ ] Criar endpoints via `MapList` para lista simples.
- [ ] Criar endpoints via `MapFirst` ou `MapSelectFirst` para primeiro item.
- [ ] Demonstrar query string de `SearchOptions` e `Sorting[]`.
- [ ] Garantir que endpoints de DTO nao dependem de `UseHints`.
- [ ] Garantir que endpoints de entidade com hints carregam navegacoes esperadas.
- [ ] Tratar casos de 204 e erro de order by invalido pelo pipeline existente.

**Criterios de aceite:** cada familia de endpoint tem ao menos um exemplo que retorna 200 com dados seeded; order by invalido retorna problema 400 quando passar pelo `Performer`.

**Testes:** `dotnet run --project .\RoyalCode.SmartSearch.Demo\RoyalCode.SmartSearch.Demo.csproj` e chamadas manuais documentadas no `.http`.

### Resultado da Fase 4

*a preencher*

---

## Fase 5 - Documentar e validar o demo

**Depende de:** Fase 4, Q3.

**Escopo:** README do demo, arquivo `.http`, possiveis smoke tests.

**O que/como:** criar documentacao operacional curta e validacao automatica ou manual, conforme Q3.

**Tarefas:**

- [ ] Criar `RoyalCode.SmartSearch.Demo/README.md` com objetivo, setup, endpoints e exemplos.
- [ ] Criar `RoyalCode.SmartSearch.Demo/RoyalCode.SmartSearch.Demo.http` com chamadas principais.
- [ ] Documentar quais endpoints mostram cada recurso do SmartSearch.
- [ ] Adicionar testes `WebApplicationFactory` se Q3 = A.
- [ ] Executar `dotnet build SmartSearch.sln --no-restore`.
- [ ] Executar `dotnet test SmartSearch.sln --no-restore -v minimal`.
- [ ] Executar o demo e validar ao menos uma chamada por familia de endpoint.
- [ ] Atualizar `smartsearch.md` para apontar o demo como referencia executavel, se desejado.

**Criterios de aceite:** uma pessoa ou IA consegue subir o demo e chamar endpoints documentados sem ler codigo interno; build e testes passam.

**Testes:** comandos padrao e smoke checks definidos no README/.http.

### Resultado da Fase 5

*a preencher*

---

## Matriz de rastreabilidade

| Objetivo | Fase(s) | Decisao(es) | Criterio(s) de aceite | Teste(s) |
|---|---|---|---|---|
| Objetivo 1 | Fase 2 | DF1, DF2, DF4, Q1, Q2, Q4 | projeto demo compila e sobe com SQLite | `dotnet build`, `dotnet run` |
| Objetivo 2 | Fase 4 | DF1 | endpoints manuais e helpers existem e retornam dados | smoke checks HTTP |
| Objetivo 3 | Fase 3, Fase 4 | DF3 | filtros, DTOs, sorting, hints e operators tem exemplos | `dotnet build`, smoke checks |
| Objetivo 4 | Fase 5 | Q3 | README e `.http` cobrem os endpoints | revisao dos arquivos, smoke checks |
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
- O demo sobe com SQLite local e Swagger/OpenAPI.
- O README do demo explica setup e exemplos de chamadas.
- O `.http` tem chamadas para manual criteria, `MapSearch`, `MapList`, `MapFirst`/`MapSelectFirst`.
- `dotnet build SmartSearch.sln --no-restore` passa.
- `dotnet test SmartSearch.sln --no-restore -v minimal` passa.

---

## Riscos

| Risco | Gatilho | Impacto | Mitigacao | Estado |
|---|---|---|---|---|
| Demo grande demais | Muitas entidades/endpoints sem criterio | Manutencao cara | Limitar ao dominio de vendas e aos recursos do objetivo | Aberto |
| Seed destrutivo | Startup apaga SQLite com dados locais | Perda de dados de experimentos | Fechar Q4 e restringir reset ao banco demo | Aberto |
| Demo diverge da API pre-1.0 | Plano de typos altera nomes depois do demo | Exemplos quebram | Executar plano de typos antes ou atualizar demo na Fase 5 | Aberto |
| Sem teste HTTP | Q3 = B | Regressao nos endpoints nao detectada por CI | Manter `.http` e documentar smoke manual | Aberto |
| Referencias por pacote ficam desatualizadas | Q2 = B e pacote local nao publicado | Demo nao compila na branch | Preferir project references se objetivo for validar repo | Aberto |

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
