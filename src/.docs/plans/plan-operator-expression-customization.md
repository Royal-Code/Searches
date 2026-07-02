# Plano: Customizacao da geracao de expressoes de operadores (case-insensitive e EF.Functions)

## Objetivo

Permitir que a geracao das expressoes de filtro do SmartSearch seja customizavel em tempo de configuracao,
separando **intencao** (declarada no filtro, ex.: busca case-insensitive) de **emissao** (qual expressao e gerada,
ex.: `string.Contains`, `ToUpper().Contains`, `EF.Functions.Like`, `EF.Functions.ILike`), sem o core Linq depender
de EF ou de provider especifico.

Motivacao (descoberta pela demo do SmartCommands, laboratorio vivo das libs):

- A traducao de `Contains` diverge por provider: SQLite `LIKE` e case-insensitive para ASCII (mas sensitive para
  acentos), PostgreSQL e case-sensitive (`strpos`/`LIKE`), SQL Server depende da collation (default CI). O mesmo
  filtro passa verde no SQLite e nao encontra nada no PG.
- `citext`/collation no banco resolve parte dos casos no PG, mas ha cenarios com `char`/`varchar` em que o uso de
  `%` (curinga) e case-insensitive **pelo usuario** e desejado — hoje impossivel: o EF escapa curingas na traducao
  de `string.Contains`.
- `CriterionOperator.Like` e `CriterionOperator.Contains` geram hoje a mesma expressao (`string.Contains`) — nao ha
  como expressar semanticas diferentes.
- Nao existe propriedade no `[Criterion]` para declarar case sensitivity, nem seam para trocar a geracao da
  expressao do operador por provider em config-time.

## Status

**PLANEJADO. Nenhuma fase iniciada.** A Fase 3 esta bloqueada pela Questao 1 (semantica de `Like`/`Contains`/novo
operador), que sera discutida antes.

Contexto relacionado (fora deste plano, ja resolvido no repo aguardando release): orderby case-insensitive
(`DefaultOrderByGenerator` + `OrderByHandlersMap`) e falha de traducao de `ORDER BY` relancada como
`OrderByException` (400 em vez de 500) — ver `CaseInsensitiveOrderByTests` e `SearchContractFixesTests`.

## Levantamento do pipeline atual (base do design)

Pontos de extensao existentes, do mais largo ao mais fino:

| Seam | Grao | Trocavel em config-time? |
|---|---|---|
| `ISpecifierFunctionGenerator` (DI) | tudo | sim, mas substituir = reimplementar o gerador inteiro |
| `ISpecifierGenerator` (DI) / `AddSpecifier` | par (TModel, TFilter) | sim, manual |
| `ConfigureSpecifierGenerator(...).Predicate(...)` | propriedade do filtro | sim, manual por propriedade |
| `[FilterExpressionGenerator<TGen>]` | propriedade ou tipo do valor do filtro | nao — gerador fixado no atributo em compile-time |
| `ExpressionGenerator.CreateOperatorExpression` | operador | **nao — estatico e hardcoded** (onde `Like`/`Contains` colapsam) |

`CreateOperatorExpression` tem **dois chamadores com naturezas diferentes**:

- `DefaultOperatorCriterionResolution.CreatePredicateExpression` — generation-time; a expressao vira parte da
  funcao compilada e **cacheada** em `SpecifiersMap.Instance` (estatico de processo, chave `(TModel, TFilter)`).
- `DisjunctionContext.Append` — **runtime**, a cada execucao, com o valor real inline (`Expression.Constant`).
  Qualquer seam novo precisa alcancar os dois, senao filtros `NomeOrApelido` ignoram a customizacao.

## Decisoes de design

- **Separar intencao de emissao.** O filtro declara a intencao (`Case` no `[Criterion]`); quem decide a expressao
  emitida e uma estrategia registrada em config-time (factory via DI). O filtro nao sabe de provider.
- **Tri-state para case.** Atributos nao aceitam `bool?`; um enum `CriterionCase { Default, Sensitive, Insensitive }`
  preserva a distincao entre "nao declarado" (segue politica global/estrategia) e declaracao explicita.
- **Seam no nivel do operador, nao da resolution.** A variacao esta so na expressao do operador; trocar a
  `ICriterionResolution` inteira duplicaria null-check, `IgnoreIfIsEmpty` e a chamada `Where`. O caso "resolution
  custom por propriedade/tipo" ja e coberto por `[FilterExpressionGenerator<TGen>]`.
- **Fallback portavel no core.** `Case = Insensitive` sem factory registrada emite normalizacao via `ToUpper()`
  em ambos os lados (`ToUpper` sem parametro e traduzivel pelos providers relacionais; `ToUpperInvariant` nao e).
  Custo documentado: nao usa indice comum.
- **Implementacoes EF fora do core.** `EF.Functions.Like` e do pacote relacional (provider-neutro) e mora em
  `RoyalCode.SmartSearch.EntityFramework`; `EF.Functions.ILike` e Npgsql-especifico e mora em pacote proprio ou no
  app consumidor (Questao 5).
- **Compatibilidade.** `CreateOperatorExpression` atual permanece; a nova assinatura e overload. Comportamento
  default (sem `Case`, sem factory) permanece identico ao atual.
- **Rewriter de query descartado.** A alternativa de reescrever `query.Expression` no pacote EF (visitor trocando
  `Contains` por `ILike`) e nao-invasiva, mas nao enxerga o `[Criterion(Case = ...)]` — nao respeita intencao
  declarada por propriedade. Fica registrada como alternativa rejeitada.

## Fase 1 - Intencao declarativa (`CriterionCase`)

### Objetivo

O filtro poder declarar case sensitivity por propriedade, com comportamento portavel mesmo sem nenhuma factory.

### Entregas

- Enum `CriterionCase { Default = 0, Sensitive, Insensitive }` em `RoyalCode.SmartSearch.Abstractions`.
- Propriedade `Case` no `CriterionAttribute`.
- Overload de `ExpressionGenerator.CreateOperatorExpression` recebendo `CriterionCase` (ou o contexto da Fase 2):
  para operadores de string (`Like`, `Contains`, `StartsWith`, `EndsWith`) com `Insensitive`, emite
  `target.ToUpper().op(filter.ToUpper())`; `Sensitive`/`Default` mantem a emissao atual.
- `DefaultOperatorCriterionResolution` e `DisjunctionContext.Append` repassam o `Case` do criterion.
- `Case` em operadores nao-string: ignorado (documentar no XML doc).

### Testes

- Unitarios de arvore de expressao (estilo `SpecifierFunctionGeneratorTests`): `Insensitive` gera `ToUpper` nos dois
  lados para cada operador de string; `Sensitive`/`Default` nao muda nada.
- E2E SQLite: filtro `Insensitive` encontra valor com acento/case divergente que o `LIKE` ASCII do SQLite nao
  encontraria (ex.: "JOSE" vs "josé" — caso que hoje falha mesmo no SQLite).
- Disjuncao (`NomeOrApelido`) com `Insensitive` tambem normaliza.

## Fase 2 - Seam de emissao (`ICriterionOperatorExpressionFactory`)

### Objetivo

Trocar a geracao da expressao do operador em config-time (por DI), com decisao no grao que a implementacao quiser
(por operador, por tipo do alvo, por propriedade, ou tudo).

### Entregas

- Contrato no core Linq (sem dependencia de EF):

```csharp
public interface ICriterionOperatorExpressionFactory
{
    // null = nao customiza; cai na emissao default (Fase 1)
    Expression? TryCreate(in CriterionOperatorContext context);
}

public readonly struct CriterionOperatorContext
{
    public CriterionOperator Operator { get; }
    public CriterionCase Case { get; }
    public bool Negation { get; }
    public Expression FilterMemberAccess { get; }   // MemberExpression (geracao) ou Constant (disjuncao/runtime)
    public Expression TargetMemberAccess { get; }
    public PropertyInfo? FilterProperty { get; }
    public Type ModelType { get; }
}
```

- Fluxo: `DefaultSpecifierFunctionGenerator` recebe as factories pelo ctor (DI, opcional) e repassa a
  `CriterionResolutions.CreateResolutions(...)`; as resolutions guardam e tentam as factories antes do default.
- Alcance da disjuncao: `DisjuctionCriterionResolution`/`JunctionProperty` carregam a(s) factory(ies) ate
  `DisjunctionContext.Append` (via `Expression.Constant` embutido no codigo gerado ou campo no `JunctionProperty`).
- Mitigacao do cache (conforme Questao 2): no minimo, incluir a identidade da(s) factory(ies) na chave do
  `SpecifiersMap` para evitar contaminacao entre containers com estrategias diferentes no mesmo processo.

### Testes

- Factory fake que troca `Like` por expressao marcada: unitario provando que a funcao gerada usa a factory, que
  `null` cai no default, e que a ordem de registro decide (se multiplas — Questao 3).
- Disjuncao usa a factory (runtime path).
- Dois containers com factories diferentes no mesmo processo nao compartilham specifier cacheado (prova da
  mitigacao do cache).

## Fase 3 - Emissao EF relacional (`EF.Functions.Like`) — BLOQUEADA pela Questao 1

### Objetivo

Honrar curingas (`%`, `_`) digitados pelo usuario em providers relacionais, via factory em
`RoyalCode.SmartSearch.EntityFramework` (opt-in no `AddEntityFrameworkSearches`).

### Entregas

- Factory baseada em `EF.Functions.Like(target, pattern)` (pacote relacional, provider-neutro).
- Definicao do pattern conforme decisao da Questao 1 (wrap `%valor%` vs valor cru como pattern vs operador novo).
- Registro opt-in documentado (nao muda comportamento de quem nao registrar).

### Testes

- E2E SQLite in-memory (o `LIKE` do SQLite honra `%`): usuario busca `jo%o` e encontra; comportamento de escape
  conforme decisao da Questao 1.

## Fase 4 - Emissao Npgsql (`EF.Functions.ILike`)

### Objetivo

`Case = Insensitive` em PG emitir `ILIKE` (nativo, melhor que normalizacao `ToUpper`).

### Entregas

- Implementacao da factory com `EF.Functions.ILike` — local conforme Questao 5 (pacote novo
  `RoyalCode.SmartSearch.EntityFramework.Npgsql` vs doc/receita para o app consumidor).
- Documentacao comparando as tres estrategias para PG: `citext`/collation no banco (preserva indice, recomendada
  quando o schema e controlado), `ILike` (factory), normalizacao `ToUpper` (portavel, sem indice).

### Testes

- Conforme Questao 4 (sem PG no repo hoje): no minimo assercao da arvore gerada; e2e real depende de decisao sobre
  infra (ex.: Testcontainers).

## Verificacao geral

- Suite completa do repo verde (`RoyalCode.SmartSearch.Tests`).
- Comportamento default inalterado sem `Case` e sem factory registrada (garantia de nao-regressao dos consumidores
  atuais, incluindo a demo do SmartCommands via WorkContext).
- Demo do SmartCommands como laboratorio: apos release, exercitar `Case = Insensitive` no `ProdutoFiltro` e
  registrar o resultado no plano da demo.

## Questoes em aberto

1. **Semantica de `Like` vs `Contains` vs operador novo (bloqueia a Fase 3).** Hoje sao identicos e o EF escapa
   curingas. Opcoes: (a) `Like` passa a honrar curingas (`EF.Functions.Like` com wrap `%valor%`) e `Contains` fica
   literal — da significado real aos nomes, mas muda comportamento observavel do default de string; (b) operador
   novo (`Pattern`/`Matches`) com semantica "o valor e o pattern" e `Like` intocado — conservador, nomes menos
   naturais; (c) variacao de (b): valor cru como pattern sem wrap (`LIKE 'abc'` = igualdade se o usuario nao usar
   curinga). Discussao marcada com o mantenedor.
2. **Cache estatico vs estrategia por container (afeta a Fase 2).** `SpecifiersMap.Instance` e estatico de processo.
   Paliativo: chave composta com a identidade da factory. Direcao maior: mapa por container com fallback ao global
   para registros manuais (`AddSpecifier`) — refactor que tocaria tambem `OrderByHandlersMap` e
   `SpecifierGeneratorOptions`. Decidir o quanto atacar agora.
3. **Uma factory ou varias (Fase 2).** Recomendacao: `IEnumerable<ICriterionOperatorExpressionFactory>` com
   primeira-nao-null-vence (compoe por tipo/operador sem inventar composite). Confirmar.
4. **Como testar ILike sem PG no repo (Fase 4).** Assercao de arvore apenas, ou infra de integracao real
   (Testcontainers/postgres)? O repo hoje so usa SQLite in-memory.
5. **Onde mora a implementacao Npgsql (Fase 4).** Pacote novo (`RoyalCode.SmartSearch.EntityFramework.Npgsql`,
   custo de release/manutencao) vs receita documentada para o app (que ja referencia o provider Npgsql). 
6. **Escopo do alcance.** A factory cobre criterios gerados (`[Criterion]`, disjuncoes). Nao alcanca expressoes
   escritas a mao (`[WithFilter]`, `AddSpecifier`, `Predicate(...)`) — documentar como limitacao ou tratar em
   iteracao futura?

### Respostas

#### Questão 1

- Decidido: (a) `Like` passa a honrar curingas e `Contains` fica literal — da significado real aos nomes,
  mas muda comportamento observavel do default de string.
  Criar uma variável/propriedade `static` pública para determinar o `default` para strings,
  deixando like como default.
  Para o `Like` sem EF, o like deverá ter um gerador dinâmico da expression, se conter `%` deverá quebrar a `string`
  e criar vários `Contains`, se não tem `%` usar direto o `Contains`.
  Para o `Like` com EF, deverá aplicar `EF.Functions.Like`. Acho que o wrap `%valor%` deverá ser opcional,
  com um valor default (true/false) em uma propriedade estática pública,
  mas o CriterionAttribute deverá ter uma propriedade para sobrescrever o default.

#### Questão 2

- Decidido: neste momento não vamos fazer nada em relação a isso. Considerar uma limitação por hora.
  Dar a entender que é uma entidade/model por "base" ou "container".

#### Questão 3

- Decidido: usar `IEnumerable<ICriterionOperatorExpressionFactory>` com primeira-nao-null-vence.
  No entanto, criar uma classe que emcapsule o `IEnumerable<ICriterionOperatorExpressionFactory>`
  e faça a lógica de loop internamente.

#### Questão 4

- Decidido: podemos pensar em algo como .Net Aspire. Mas não complicar demais, talvez deixar para uma interação futura.

#### Questão 5

- Decidido: criar um pacote novo `RoyalCode.SmartSearch.EntityFramework.Npgsql` para manter a implementação do ILike.

#### Questão 6

- Decidido: documentar como limitação, não será tratado `[WithFilter]`, `AddSpecifier`, `Predicate(...)`.
  Estas customizações são de competência do consumidor, não do core.

## Fora de escopo

- Trocar a semantica de traducao de filtros de tipos nao-string.
- Collation/citext no banco (decisao de schema do consumidor; sera apenas documentada como alternativa).
- Ordenacao case-insensitive e `OrderByException` 400 (ja resolvidos, aguardando release).
- Rewriter de `query.Expression` no pacote EF (alternativa rejeitada — nao ve a intencao declarada).

## Ordem recomendada

1. Fase 1 (intencao declarativa + fallback portavel) — independente das questoes abertas.
2. Fase 2 (seam) — decidir Questoes 2 e 3 antes de iniciar.
3. Discussao da Questao 1 -> Fase 3.
4. Fase 4 apos decidir Questoes 4 e 5.
