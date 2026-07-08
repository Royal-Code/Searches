# Plan: Correcao de typos de API e documentacao (`api-typos-e-documentacao`)

## Status: EM ANDAMENTO - Fase 2 concluida

## Progresso

`##--` **50%** - 2 de 4 fases

| Fase | Estado |
|---|---|
| Fase 1 - Renomear Disjuction para Disjunction | Concluida |
| Fase 2 - Renomear FirstDefaultAsync | Concluida |
| Fase 3 - Documentar Projection como reservado | Pendente |
| Fase 4 - Versao, verificacao e fechamento | Pendente |

> **Manutencao deste plano:** ao concluir as tarefas de uma fase, marque cada tarefa com `- [x]`,
> troque o **Estado** da fase para `Concluida` na tabela acima e atualize a barra de progresso
> (um caractere `#` por fase concluida, `%` e `X de N`).
> Antes de fechar uma fase, confirme que decisoes, criterios de aceite, testes e invariantes relacionados foram aplicados.

---

## Contexto

### Fontes verificadas

- `.ai/references/template-plan/template-ai-implementation-plan.md` - define o formato exigido para planos orientados a IA.
- `RoyalCode.SmartSearch.Abstractions/DisjuctionAttribute.cs` - existe API publica `DisjuctionAttribute`.
- `RoyalCode.SmartSearch.Linq/Filtering/CriterionResolutions.cs` - a deteccao de OR por atributo usa `DisjuctionAttribute`.
- `RoyalCode.SmartSearch.Linq/Filtering/Resolutions/DisjuctionCriterionResolution.cs` - existe classe interna com o typo no nome.
- `RoyalCode.SmartSearch.Abstractions/ISearch.cs` - `ISearch<TEntity>` expoe `FirstDefaultAsync`.
- `RoyalCode.SmartSearch.Core/Defaults/Search.cs` - implementa `FirstDefaultAsync` para `ISearch<TEntity>`.
- `RoyalCode.SmartSearch.Abstractions/ResultList.cs` - `GetProjection<T>()` existe, mas lanca `NotImplementedException`.
- `smartsearch.md` e `README.md` - documentam `[Disjuction]`.
- `Directory.Build.props` - `SearchesVer` esta em `0.10.5`.
- `dotnet test SmartSearch.sln --no-restore -v minimal` - passou em 2026-07-08 com 247 testes em `net10.0`.

### Estado atual do codigo (verificado em 2026-07-08)

- **API publica com typo:** `DisjuctionAttribute` esta em `RoyalCode.SmartSearch.Abstractions/DisjuctionAttribute.cs`.
- **Pipeline Linq depende do nome antigo:** `CriterionResolutions` busca `typeof(DisjuctionAttribute)` e le `Alias`.
- **Classe interna com typo:** `DisjuctionCriterionResolution` aplica os grupos OR.
- **Async entity search com typo:** `ISearch<TEntity>` tem `FirstDefaultAsync`, enquanto `ICriteria<TEntity>` e `ISearch<TEntity, TDto>` usam `FirstOrDefaultAsync`.
- **Projection ainda nao implementada:** `ResultList<T>.GetProjection<T>()` lanca `NotImplementedException`.
- **Documentacao atual usa nomes antigos:** `smartsearch.md` e `README.md` citam `[Disjuction]`.
- **Release atual:** `SearchesVer` esta em `0.10.5`.

### Lacunas, conflitos e restricoes

- **Mudanca quebradora de API publica:** remover `DisjuctionAttribute` e `FirstDefaultAsync` quebra consumidores, mas o pacote ainda esta em pre-1.0.
- **Sem alias de compatibilidade:** nao criar outro atributo ou metodo antigo reduz ambiguidade, mas exige atualizacao nos consumidores.
- **Projection sem design fechado:** a intencao existe, mas nao ha contrato implementado para agregacoes/projecoes extras.
- **Testes executam so em `net10.0`:** as libs compilam multi-target, mas a suite atual roda em `net10.0`.

### Superficies impactadas a mapear

- `RoyalCode.SmartSearch.Abstractions` - contrato publico de atributo e `ISearch<TEntity>`.
- `RoyalCode.SmartSearch.Core` - implementacao de `ISearch<TEntity>`.
- `RoyalCode.SmartSearch.Linq` - resolucao de atributos e nomes internos de disjuncao.
- `RoyalCode.SmartSearch.Tests` - usos de `[Disjuction]` e chamadas a `FirstDefaultAsync`, se houver.
- `smartsearch.md` e `README.md` - documentacao para humanos e IA.
- `Directory.Build.props` - versao `0.11.0`.

---

## Objetivo

1. Substituir `Disjuction` por `Disjunction` na API publica, codigo interno, testes e documentacao.
2. Substituir `FirstDefaultAsync` por `FirstOrDefaultAsync` em `ISearch<TEntity>` e na implementacao.
3. Documentar `GetProjection`/`Projections` como API reservada para funcionalidade futura, sem implementar agregacoes.
4. Preparar a alteracao para release `0.11.0` com build e testes verdes.

## Fora de escopo

- Implementar `GetProjection`, `Projections` ou agregacoes extras em query.
- Manter alias de compatibilidade para `[Disjuction]`.
- Manter alias de compatibilidade para `FirstDefaultAsync`.
- Corrigir outros typos publicos nao decididos, como `SearchOptions.AllItens()`.
- Criar o projeto demo. Destino: `.ai/plans/plan-smartsearch-demo.md`.

---

## Decisoes fechadas

- **DF1 - Corrigir `Disjuction` sem alias:** substituir o typo por `Disjunction`, sem criar outro atributo paralelo. Fonte: decisao humana nesta conversa.
- **DF2 - Corrigir `FirstDefaultAsync`:** trocar para `FirstOrDefaultAsync`, com `Or`, no contrato e implementacao de `ISearch<TEntity>`. Fonte: decisao humana nesta conversa.
- **DF3 - Projection reservada para o futuro:** documentar `GetProjection`/`Projections` como intencao futura, sem implementar comportamento nesta iteracao. Fonte: decisao humana nesta conversa.
- **DF4 - Versao de release:** preparar a mudanca como `0.11.0`. Fonte: decisao humana nesta conversa.
- **DF5 - Aceitar breaking changes pre-1.0:** aplicar as correcoes como quebra controlada por ainda estar antes da versao 1.0. Fonte: decisao humana nesta conversa.

---

## Historico de decisoes

**Fase 0 (triagem de API):**

- **Q1 - `Disjuction` deve ser mantido com alias?** Opcoes consideradas: manter alias obsoleto ou substituir sem alias.
  - **Resposta Q1.1:** substituir sem criar outro atributo, pois a versao ainda e pre-1.0.
  - **Conclusao Q1:** DF1 e DF5.
- **Q2 - `GetProjection` deve ser implementado agora?** Opcoes consideradas: implementar agregacoes ou documentar como reservado.
  - **Resposta Q2.1:** documentar como reservado para futuro.
  - **Conclusao Q2:** DF3.
- **Q3 - `FirstDefaultAsync` deve ser corrigido?** Opcoes consideradas: manter nome atual ou trocar para `FirstOrDefaultAsync`.
  - **Resposta Q3.1:** trocar para `FirstOrDefaultAsync`.
  - **Conclusao Q3:** DF2.

---

## Design alvo

### Contratos e bordas

- `DisjunctionAttribute(string alias)`: atributo publico em `RoyalCode.SmartSearch` para agrupar propriedades de filtro em OR.
- `DisjunctionCriterionResolution`: classe interna no Linq para aplicar grupos OR por atributo e por convencao de nome.
- `ISearch<TEntity>.FirstOrDefaultAsync(CancellationToken cancellationToken = default)`: metodo async publico para obter o primeiro item ou `null`.
- `IResultList.Projections` e `IResultList<T>.GetProjection<T>()`: API reservada; a documentacao deve avisar que nao ha implementacao funcional nesta versao.

### Modelo, dados e persistencia

```text
Nao ha mudanca de modelo, dados ou persistencia.
```

### Arquitetura alvo

```text
RoyalCode.SmartSearch.Abstractions/
  DisjunctionAttribute.cs
  ISearch.cs
  IResultList.cs
  ResultList.cs

RoyalCode.SmartSearch.Core/
  Defaults/Search.cs

RoyalCode.SmartSearch.Linq/
  Filtering/CriterionResolutions.cs
  Filtering/Resolutions/DisjunctionCriterionResolution.cs

RoyalCode.SmartSearch.Tests/
  testes atualizados para nomes novos

Documentacao/
  smartsearch.md
  README.md
```

### Seguranca, concorrencia e confiabilidade

- A alteracao nao deve mudar semantica de filtragem OR.
- A alteracao nao deve mudar tracking, paginacao, sorting, selector, hints ou factories.
- O cache de specifiers deve continuar usando os mesmos criterios de chave existentes.

### Compatibilidade, migracao e rollout

- Esta e uma mudanca quebradora para consumidores que usam `[Disjuction]` ou `FirstDefaultAsync`.
- A release deve ser `0.11.0`.
- A documentacao deve chamar a mudanca como correcao de typo pre-1.0.
- Os consumidores devem migrar para `[Disjunction]` e `FirstOrDefaultAsync`.

---

## Ordem de execucao

1. **Fase 1 (Renomear Disjuction para Disjunction)** - corrige atributo, resolucao, testes e docs de OR.
2. **Fase 2 (Renomear FirstDefaultAsync)** - corrige contrato async antes da verificacao global.
3. **Fase 3 (Documentar Projection como reservado)** - evita uso indevido sem implementar agregacoes.
4. **Fase 4 (Versao, verificacao e fechamento)** - atualiza versao e valida a solucao.

Build/test padrao:

```powershell
dotnet build SmartSearch.sln --no-restore
dotnet test SmartSearch.sln --no-restore -v minimal
```

---

## Fase 1 - Renomear Disjuction para Disjunction

**Depende de:** DF1, DF5.

**Escopo:** `Abstractions`, `Linq`, `Tests`, `smartsearch.md`, `README.md`.

**O que/como:** renomear o atributo publico e os tipos internos relacionados. Atualizar todos os usos por busca textual. Nao criar alias do nome antigo.

**Tarefas:**

- [x] Renomear `DisjuctionAttribute.cs` para `DisjunctionAttribute.cs`.
- [x] Renomear `DisjuctionAttribute` para `DisjunctionAttribute` e atualizar XML docs.
- [x] Renomear `DisjuctionCriterionResolution` para `DisjunctionCriterionResolution`.
- [x] Atualizar `CriterionResolutions` para buscar `DisjunctionAttribute`.
- [x] Atualizar testes que usam `[Disjuction]` para `[Disjunction]`.
- [x] Atualizar `smartsearch.md` e `README.md` para usar `[Disjunction]`.
- [x] Executar `rg -n "Disjuction"` e tratar todo resultado dentro do escopo.

**Criterios de aceite:** nao existe ocorrencia de `Disjuction` em codigo fonte ou documentacao, exceto se houver changelog/migracao explicitando o nome removido; testes de disjuncao continuam verdes.

**Testes:** `dotnet test SmartSearch.sln --no-restore -v minimal`.

### Resultado da Fase 1

Concluida em 2026-07-08.

Entregaveis:

- Atributo publico renomeado para `DisjunctionAttribute`.
- Resolution interna renomeada para `DisjunctionCriterionResolution`.
- Pipeline de `CriterionResolutions` atualizado para detectar `DisjunctionAttribute`.
- Teste `DisjunctionTests` atualizado para `[Disjunction]`.
- `smartsearch.md` e `README.md` atualizados para `[Disjunction]`.

Arquivos alterados:

- `RoyalCode.SmartSearch.Abstractions/DisjunctionAttribute.cs`.
- `RoyalCode.SmartSearch.Linq/Filtering/CriterionResolutions.cs`.
- `RoyalCode.SmartSearch.Linq/Filtering/Resolutions/DisjunctionCriterionResolution.cs`.
- `RoyalCode.SmartSearch.Tests/DisjunctionTests.cs`.
- `smartsearch.md`.
- `README.md`.

Decisoes aplicadas:

- DF1.
- DF5.

Verificacao:

- `rg -n "Disjuction" --glob "!.ai/plans/plan-api-typos-e-documentacao.md"` nao retornou ocorrencias.
- `dotnet test SmartSearch.sln --no-restore -v minimal` passou: 247 aprovados, 0 falhas, 0 ignorados.

Warnings observados:

- `NU5104` em `RoyalCode.SmartSearch.AspNetCore` por pacote estavel depender de `RoyalCode.SmartProblems.ApiResults` preview.
- `CS8618` em modelos de teste de `ComplexTypeTests` para propriedades nao anulaveis sem inicializacao.

Desvios:

- Nenhum.

Pendencias:

- Fases 2 a 4 permanecem pendentes.

---

## Fase 2 - Renomear FirstDefaultAsync

**Depende de:** DF2, DF5.

**Escopo:** `RoyalCode.SmartSearch.Abstractions/ISearch.cs`, `RoyalCode.SmartSearch.Core/Defaults/Search.cs`, testes e docs.

**O que/como:** substituir o metodo async entity-search pelo nome correto `FirstOrDefaultAsync`, mantendo semantica e assinatura de cancellation token.

**Tarefas:**

- [x] Renomear `ISearch<TEntity>.FirstDefaultAsync` para `FirstOrDefaultAsync`.
- [x] Renomear a implementacao em `Search<TEntity>`.
- [x] Atualizar chamadas internas e testes, se existirem.
- [x] Atualizar documentacao para citar o nome correto quando falar de `ISearch<TEntity>`.
- [x] Executar `rg -n "FirstDefaultAsync"` e tratar todo resultado dentro do escopo.

**Criterios de aceite:** `FirstDefaultAsync` nao aparece mais no codigo; `ISearch<TEntity>`, `ICriteria<TEntity>` e `ISearch<TEntity, TDto>` usam nomenclatura consistente para `FirstOrDefaultAsync`.

**Testes:** `dotnet test SmartSearch.sln --no-restore -v minimal`.

### Resultado da Fase 2

Concluida em 2026-07-08.

Entregaveis:

- `ISearch<TEntity>` expoe `FirstOrDefaultAsync(CancellationToken cancellationToken = default)`.
- `Search<TEntity>` implementa `FirstOrDefaultAsync` e delega para `IPreparedQuery<TEntity>.FirstOrDefaultAsync`.
- `ISearch<TEntity>`, `ICriteria<TEntity>` e `ISearch<TEntity, TDto>` usam nomenclatura consistente para `FirstOrDefaultAsync`.

Arquivos alterados:

- `RoyalCode.SmartSearch.Abstractions/ISearch.cs`.
- `RoyalCode.SmartSearch.Core/Defaults/Search.cs`.

Decisoes aplicadas:

- DF2.
- DF5.

Verificacao:

- `rg -n "FirstDefaultAsync" --glob "!.ai/plans/plan-api-typos-e-documentacao.md"` nao retornou ocorrencias.
- `dotnet test SmartSearch.sln --no-restore -v minimal` passou: 247 aprovados, 0 falhas, 0 ignorados.

Warnings observados:

- `NU5104` em `RoyalCode.SmartSearch.AspNetCore` por pacote estavel depender de `RoyalCode.SmartProblems.ApiResults` preview.
- `CS8618` em modelos de teste de `ComplexTypeTests` para propriedades nao anulaveis sem inicializacao.

Desvios:

- Nenhum.

Pendencias:

- Fases 3 e 4 permanecem pendentes.

---

## Fase 3 - Documentar Projection como reservado

**Depende de:** DF3.

**Escopo:** `smartsearch.md`, `README.md`, XML docs de `IResultList`/`ResultList`.

**O que/como:** documentar que `Projections`/`GetProjection<T>()` sao reservados para uma funcionalidade futura de agregacoes/projecoes extras sobre a consulta filtrada, sem paginação.

**Tarefas:**

- [ ] Atualizar XML docs de `IResultList.Projections` e `GetProjection<T>()` para avisar que a API ainda nao tem implementacao funcional.
- [ ] Atualizar XML docs de `ResultList<T>.GetProjection<T>()` para registrar o estado atual.
- [ ] Adicionar secao curta no `smartsearch.md` sobre "Projections reservadas para futuro".
- [ ] Atualizar `README.md` com o mesmo aviso ou remover incentivo implicito ao uso.
- [ ] Garantir que nenhum exemplo novo chame `GetProjection<T>()` como funcional.

**Criterios de aceite:** uma IA lendo `smartsearch.md` entende que `GetProjection<T>()` nao deve ser usado ainda; o build continua verde.

**Testes:** `dotnet build SmartSearch.sln --no-restore`.

### Resultado da Fase 3

*a preencher*

---

## Fase 4 - Versao, verificacao e fechamento

**Depende de:** Fase 1, Fase 2, Fase 3, DF4.

**Escopo:** `Directory.Build.props`, solucao completa.

**O que/como:** atualizar versao para `0.11.0`, executar verificacoes finais e registrar resultado.

**Tarefas:**

- [ ] Atualizar `SearchesVer` para `0.11.0`.
- [ ] Executar busca final por nomes removidos: `rg -n "Disjuction|FirstDefaultAsync"`.
- [ ] Executar `dotnet build SmartSearch.sln --no-restore`.
- [ ] Executar `dotnet test SmartSearch.sln --no-restore -v minimal`.
- [ ] Registrar warnings conhecidos e novos warnings no resultado da fase.

**Criterios de aceite:** versao `0.11.0`; build verde; testes verdes; nenhum uso indevido dos nomes removidos.

**Testes:** comandos de build/test padrao.

### Resultado da Fase 4

*a preencher*

---

## Matriz de rastreabilidade

| Objetivo | Fase(s) | Decisao(es) | Criterio(s) de aceite | Teste(s) |
|---|---|---|---|---|
| Objetivo 1 | Fase 1 | DF1, DF5 | `Disjuction` removido do codigo e docs; OR continua funcionando | `dotnet test SmartSearch.sln --no-restore -v minimal` |
| Objetivo 2 | Fase 2 | DF2, DF5 | `FirstOrDefaultAsync` exposto em `ISearch<TEntity>`; `FirstDefaultAsync` removido | `dotnet test SmartSearch.sln --no-restore -v minimal` |
| Objetivo 3 | Fase 3 | DF3 | docs avisam que Projection e reservada/futura | `dotnet build SmartSearch.sln --no-restore` |
| Objetivo 4 | Fase 4 | DF4 | `SearchesVer` = `0.11.0`; build/test verdes | comandos padrao |

---

## Invariantes a preservar

1. A semantica de OR por atributo e por nome/caminho contendo `Or` nao pode mudar.
2. Hints, selectors, sorting, pagination e factories de operadores nao podem mudar de comportamento.
3. `GetProjection<T>()` nao deve ser apresentado como funcional enquanto lancar `NotImplementedException`.
4. As mudancas quebradoras devem ficar limitadas aos typos decididos.

---

## Criterios globais de conclusao

- `rg -n "Disjuction|FirstDefaultAsync"` nao retorna ocorrencias em codigo/documentacao corrente, exceto historico de migracao se criado.
- `smartsearch.md` e `README.md` orientam uso de `[Disjunction]`, `FirstOrDefaultAsync` e Projection reservada.
- `Directory.Build.props` usa `SearchesVer` = `0.11.0`.
- `dotnet test SmartSearch.sln --no-restore -v minimal` passa.

---

## Riscos

| Risco | Gatilho | Impacto | Mitigacao | Estado |
|---|---|---|---|---|
| Consumidor externo quebra ao usar `[Disjuction]` | Build de app consumidor falha apos atualizar pacote | Migracao manual obrigatoria | Documentar breaking change pre-1.0 e nome novo | Aberto |
| Consumidor externo quebra ao usar `FirstDefaultAsync` | Build de app consumidor falha apos atualizar pacote | Migracao manual obrigatoria | Documentar troca para `FirstOrDefaultAsync` | Aberto |
| Typos antigos permanecem em docs | `rg` encontra ocorrencias nao historicas | IA continua copiando nome errado | Busca final obrigatoria | Aberto |
| Projection parece funcional | Exemplo usa `GetProjection<T>()` | Runtime com `NotImplementedException` | Aviso explicito em XML docs e Markdown | Aberto |

---

## Diferidos e backlog

- Implementar `Projections`/`GetProjection<T>()` para agregacoes sobre consulta filtrada antes da paginacao - destino: plano futuro.
- Avaliar correcao de `SearchOptions.AllItens()` para `AllItems()` - destino: backlog de breaking changes pre-1.0.
- Criar demo WebAPI - destino: `.ai/plans/plan-smartsearch-demo.md`.

---

## Referencias

- `.ai/references/template-plan/template-ai-implementation-plan.md`.
- `smartsearch.md`.
- `README.md`.
- `RoyalCode.SmartSearch.Abstractions/DisjuctionAttribute.cs`.
- `RoyalCode.SmartSearch.Abstractions/ISearch.cs`.
- `RoyalCode.SmartSearch.Abstractions/IResultList.cs`.
- `RoyalCode.SmartSearch.Abstractions/ResultList.cs`.
- `RoyalCode.SmartSearch.Core/Defaults/Search.cs`.
- `RoyalCode.SmartSearch.Linq/Filtering/CriterionResolutions.cs`.
- `RoyalCode.SmartSearch.Linq/Filtering/Resolutions/DisjuctionCriterionResolution.cs`.
- `Directory.Build.props`.
