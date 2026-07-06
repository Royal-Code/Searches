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

**IMPLEMENTADO — Fases 1 a 5 CONCLUIDAS na branch `feature/operator-expression-customization`; suite 248/248
verde (net10). Aguarda revisao do mantenedor e release.** Todas as questoes (1-9) foram decididas pelo mantenedor
(ver "Respostas" e as decisoes nas "Novas questoes"). A decisao da Questao 1 (`Like` honra curingas, `Contains`
literal, defaults estaticos configuraveis) expandiu o escopo: a semantica de `Like`/`Contains` no core virou a
Fase 3 propria, deslocando a factory EF para a Fase 4 e o pacote Npgsql para a Fase 5.

Notas de implementacao (desvios e achados; ver tambem "Resultado" das fases):

- **Bug latente corrigido no `DisjunctionContext`:** os operandos eram passados invertidos ao
  `CreateOperatorExpression` (gerava `"valor".Contains(e.Prop)` e `"valor" > e.Prop`), divergindo do caminho
  principal das resolutions. Os testes passavam por simetria dos dados. Corrigido junto com a Fase 2 (o caminho
  runtime da disjuncao agora emite igual ao caminho principal); `OrTests`/`DisjunctionTests` continuam verdes.
- **`Like` sem curinga e sem wrap = igualdade exata** (semantica do LIKE), e nao `Contains`: a frase da Questao 1
  ("se nao tem % usar direto o Contains") vale no default (wrap ligado), onde `%valor%` reduz a `Contains`;
  com `Wrap = None` a fidelidade ao LIKE foi mantida para preservar a paridade com `EF.Functions.Like`.
- **Overload com escape char (`EF.Functions.Like(t, p, escape)`) avaliado e nao adotado:** o proposito do modo
  Like e honrar os curingas do usuario; sem uma sintaxe de escape definida para o usuario final, o overload nao
  agrega — pode ser retomado se surgir o requisito.
- O `ToUpper()` da normalizacao portavel e o sem parametro (traduzivel); `ToUpperInvariant` nao e traduzido.

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
  default (sem `Case`, sem factory) permanece identico ao atual, **exceto** pela decisao da Questao 1: `Like`
  (default de string) passa a honrar curingas digitados pelo usuario. Valvula de escape: propriedade estatica
  publica com o operador default para strings (consumidor pode voltar a `Contains` literal como default).
- **`Like` != `Contains` (Questao 1).** `Contains` = substring literal (comportamento atual, curingas escapados);
  `Like` = pattern com `%` do usuario honrado. Wrap `%valor%` opcional: default em propriedade estatica publica +
  override por criterion.
- **Cache por processo aceito como limitacao (Questao 2).** Specifiers continuam cacheados por processo
  (`SpecifiersMap.Instance`, chave `(TModel, TFilter)`): na pratica, uma estrategia por model por processo.
  Documentar; nada a fazer agora.
- **Composicao de factories via classe encapsuladora (Questao 3).** `IEnumerable<ICriterionOperatorExpressionFactory>`
  com primeira-nao-null-vence, encapsulado em uma classe que faz o loop internamente; as resolutions recebem essa
  classe, nao o `IEnumerable` cru.
- **Alcance documentado como limitacao (Questao 6).** A customizacao cobre criterios gerados (`[Criterion]`,
  disjuncoes). `[WithFilter]`, `AddSpecifier` e `Predicate(...)` sao expressoes do consumidor — competencia dele,
  nao do core.
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

### Resultado - CONCLUIDA

`CriterionCase` e `LikeWrap` em `Abstractions`; `Case`/`Wrap` no `CriterionAttribute`; overload de
`CreateOperatorExpression` com `CriterionCase` normalizando `Like`/`Contains`/`StartsWith`/`EndsWith` via
`ToUpper()` (so operandos string; `Equal` e nao-string ignorados). Nota: a normalizacao no SQLite so vale para
ASCII (o `UPPER()` do SQLite nao cobre acentos), entao o teste E2E usa divergencia de case ASCII (`nOtEbOoK`)
em vez do exemplo com acento do plano; a cobertura insensitive com acentos fica nos testes em memoria.
Testes: `CriterionCaseTests` (unitarios + in-memory + disjuncao + E2E SQLite).

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

- Classe encapsuladora (Questao 3): envolve o `IEnumerable<ICriterionOperatorExpressionFactory>` e implementa
  internamente o loop primeira-nao-null-vence; e ela que flui pelo pipeline (nao o `IEnumerable` cru).
- Fluxo: `DefaultSpecifierFunctionGenerator` recebe a classe encapsuladora pelo ctor (DI, opcional) e repassa a
  `CriterionResolutions.CreateResolutions(...)`; as resolutions guardam e tentam as factories antes do default.
- Alcance da disjuncao: `DisjuctionCriterionResolution`/`JunctionProperty` carregam a encapsuladora ate
  `DisjunctionContext.Append` (via `Expression.Constant` embutido no codigo gerado ou campo no `JunctionProperty`).
- Limitacao documentada (Questao 2): specifiers sao cacheados por processo (`SpecifiersMap.Instance`); na pratica,
  uma estrategia por model por processo — containers distintos no mesmo processo compartilham o specifier gerado.

### Testes

- Factory fake que troca `Like` por expressao marcada: unitario provando que a funcao gerada usa a factory, que
  `null` cai no default, e que a ordem de registro decide (primeira-nao-null-vence via encapsuladora).
- Disjuncao usa a factory (runtime path).

### Resultado - CONCLUIDA

`ICriterionOperatorExpressionFactory` + `CriterionOperatorContext` (readonly struct com `Operator`, `Case`,
`Wrap`, `Negation`, acessos e `ModelType`) e a encapsuladora `CriterionOperatorExpressionFactories` no core
Linq; registrada no DI por `AddSmartSearchLinq` (via `GetServices`, ordem de registro preservada);
`DefaultSpecifierFunctionGenerator` recebe a encapsuladora e a repassa por `CriterionResolutions` ate
`DefaultOperatorCriterionResolution`, `ComplexFilterCriterionResolution` (recursao) e `JunctionProperty` →
`DisjunctionContext.Append` (runtime). De quebra, corrigiu-se a inversao de operandos do caminho da disjuncao
(ver Notas de implementacao no Status). Testes: `CriterionOperatorFactoriesTests` (customizacao, fallback null,
ordem, disjuncao runtime).

## Fase 3 - Semantica de `Like` e `Contains` no core (Questao 1)

### Objetivo

`Like` e `Contains` deixarem de ser sinonimos: `Contains` = substring literal (atual); `Like` = pattern com `%`
do usuario honrado, com comportamento portavel (sem EF) e defaults configuraveis.

### Entregas

- Propriedade estatica publica com o operador default para strings, com `Like` como default; lida por
  `DiscoveryCriterionOperator` (valvula de escape para consumidores que preferem `Contains` literal como default).
- Wrap `%valor%` opcional: default (true/false) em propriedade estatica publica + propriedade no
  `CriterionAttribute` para sobrescrever por criterion (enum tri-state, ex.: `LikeWrap { Default, Wrap, None }` —
  Questao 8; o enum mora em `Abstractions`, junto do atributo).
- Gerador dinamico portavel do `Like` (sem EF): como o valor so existe na execucao e a funcao do specifier e
  compilada e cacheada por `(TModel, TFilter)`, a resolution emite chamada a um helper de runtime que inspeciona o
  valor: sem `%`, aplica `Contains` direto; com `%`, compoe o **casamento guloso** decidido na Questao 7 —
  ancoras por `StartsWith`/`EndsWith` + guarda de `Length`, segmentos intermediarios em ordem via fatiamento
  (`s.Substring(s.IndexOf(seg) + seg.Length)`), todas as pecas traduziveis pelos providers relacionais e
  executaveis em memoria (a mesma arvore serve para os dois mundos). Corte em **5 `%`**: curingas excedentes tem
  seus segmentos verificados por `Contains` sem fatiamento (aproximacao documentada). O corpo do specifier ja e
  imperativo (`Expression.Block` com `Assign`), o que comporta a chamada.
- Nota de implementacao (so do caminho portavel): os segmentos entram como `Constant` na arvore, entao viram
  literais no SQL (um plano por padrao distinto). Aceito e documentado; no caminho EF (Fase 4) nao se aplica — o
  pattern e um parametro unico do `EF.Functions.Like`.
- Interacao com `Case = Insensitive` (Fase 1): a normalizacao portavel (`ToUpper`) e aplicada uma vez na base e
  nos segmentos, antes da composicao.
- Casa dos defaults estaticos (operador default de string + wrap): tipo unico de opcoes no pacote de uso —
  `RoyalCode.SmartSearch.Linq` (Questao 9); se algum default for consumido por outro pacote, mora no outro.

### Testes

- Unitarios do helper de runtime: valor sem `%` = `Contains`; ancoras (`jo%`, `%jo`, `jo%o` com guarda de
  comprimento — "jo" nao passa em `jo%o`); ordem dos segmentos (`%b%a%` nao da match em "ab"); segmento repetido
  (`a%a` exige duas ocorrencias); corte em 5 `%` degrada para `Contains` nos excedentes; wrap on/off; override por
  criterion vence o default estatico; `Insensitive` normaliza.
- E2E SQLite: `?nome=jo%o` encontra "Joao" e "Jono", nao encontra "Joa"; `%b%a%` nao encontra "ab"; `Contains`
  com `%` no valor segue literal (escapado).
- Troca do default estatico para `Contains` restaura o comportamento anterior por completo.

### Resultado - CONCLUIDA

`CriterionDefaults` (`DefaultStringOperator` = `Like`, `WrapLikeValue` = `true`, `ResolveWrap`) no Linq
(Questao 9); `DiscoveryCriterionOperator` le o default configuravel; `LikeExpressionGenerator` com o casamento
guloso decidido na Questao 7: ancora inicial via `StartsWith` + fatiamento, segmentos do meio em ordem via
`Substring(IndexOf(seg) + len)`, ancora final via `EndsWith` **sobre a fatia restante** (o que subsume a guarda
de comprimento e impede sobreposicao), `MaxSliceOperations = 5` com degradacao para `Contains`; sem curinga e
sem wrap = igualdade exata (ver Notas de implementacao). `DefaultOperatorCriterionResolution` emite chamada ao
helper de runtime (`Apply`) para `Like` string-string quando nenhuma factory customiza; a disjuncao usa
`CreatePatternExpression` direto com o valor real. Testes: `LikePatternExpressionTests` (matriz do padrao,
corte, negacao, valor vazio), `LikeSearchTests` (E2E SQLite: curinga do usuario, `Like` vs `Contains` com "100%",
wrap None = exato, insensitive) e `LikeDefaultStringOperatorTests` (valvula de escape).

## Fase 4 - Emissao EF relacional (`EF.Functions.Like`)

### Objetivo

Nos providers relacionais, o `Like` ser emitido como `EF.Functions.Like(target, pattern)` (traduz para `LIKE`
nativo), via factory em `RoyalCode.SmartSearch.EntityFramework` (opt-in no `AddEntityFrameworkSearches`).

### Entregas

- Factory baseada em `EF.Functions.Like(target, pattern)` (pacote relacional, provider-neutro), respeitando o wrap
  (default estatico + override do criterion) e o `Case` (com `Insensitive`, `UPPER(...) LIKE UPPER(...)` na
  ausencia de suporte nativo).
- Avaliar o overload com escape char (`EF.Functions.Like(target, pattern, escapeChar)`) para permitir literal
  `%`/`_` dentro do pattern.
- Registro opt-in documentado (nao muda comportamento de quem nao registrar).

### Testes

- E2E SQLite in-memory (o `LIKE` do SQLite honra `%`): usuario busca `jo%o` e encontra; paridade de semantica com
  o helper portavel da Fase 3 (mesmos casos, mesmos resultados).

### Resultado - CONCLUIDA

`EntityFrameworkLikeExpressionFactory` no pacote EntityFramework: `Like` string-string vira
`EF.Functions.Like(target, pattern)` respeitando wrap (concat `%` em expressao, traduzivel) e `Case`
(`Insensitive` → `UPPER(...) LIKE UPPER(...)`); registro opt-in `AddEntityFrameworkLikeOperator()`
(`TryAddEnumerable`, sem duplicar). Overload com escape char avaliado e nao adotado (ver Notas). Testes:
`EntityFrameworkLikeFactoryTests` — assercoes de arvore (metodo `Like`, `Concat` no wrap, `ToUpper` no
insensitive, `Not` na negacao, null fora do escopo) + E2E SQLite de paridade com os mesmos casos da Fase 3
(SQL confirmado: `WHERE "x"."Nome" LIKE @Concat` com `@Concat = '%Jo%o%'`). Achado de uso registrado:
`ICriteria` e fluente/stateful — uma instancia nova por consulta nos testes.

## Fase 5 - Pacote `RoyalCode.SmartSearch.EntityFramework.Npgsql` (`EF.Functions.ILike`)

### Objetivo

`Case = Insensitive` em PG emitir `ILIKE` (nativo, melhor que normalizacao `ToUpper`), em pacote novo (Questao 5).

### Entregas

- Novo projeto/pacote `RoyalCode.SmartSearch.EntityFramework.Npgsql` com a factory `EF.Functions.ILike` e extensao
  de registro.
- Documentacao comparando as tres estrategias para PG: `citext`/collation no banco (preserva indice, recomendada
  quando o schema e controlado), `ILike` (factory), normalizacao `ToUpper` (portavel, sem indice).

### Testes

- Assercao da arvore gerada (sem PG no repo — Questao 4); e2e real com PG (ex.: .NET Aspire) adiado para iteracao
  futura, sem complicar agora.

### Resultado - CONCLUIDA

Novo projeto/pacote `RoyalCode.SmartSearch.EntityFramework.Npgsql` (multi-target net8/9/10;
`Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.11/9.0.4/10.0.0 por TFM; adicionado a solution):
`NpgsqlILikeExpressionFactory` emite `EF.Functions.ILike` para `Like` + `Insensitive` (respeitando wrap e
negacao; devolve null fora desse escopo) e `AddNpgsqlLikeOperators()` registra ILike **antes** do EF Like
(ordem primeira-nao-null). Documentacao em `smartsearch.md` (nova secao "Like, Contains e Case-Insensitive"
com a comparacao das tres estrategias para PG: `citext`/collation, `ILIKE`, `ToUpper`). Testes:
`NpgsqlILikeFactoryTests` (arvore: metodo `ILike`, wrap, negacao, null fora do escopo, ordem de registro
via encapsuladora).

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

### Novas questoes (implementacao — decorrentes da resposta da Questao 1)

7. **Semantica exata do split portavel do `Like` (Fase 3).** Quebrar `jo%o` em `Contains("jo") && Contains("o")`
   nao equivale ao `LIKE`: (a) perde a **ordem** dos segmentos (`b%a` daria match em "ab", que o `LIKE '%b%a%'`
   nao daria); (b) segmento repetido (`a%a`) exige duas ocorrencias no `LIKE`, mas `Contains("a") && Contains("a")`
   aceita uma; (c) sem wrap, o primeiro/ultimo segmento viram ancoras (`StartsWith`/`EndsWith`). Alternativa fiel e
   ainda portavel: cadeia de `IndexOf` com posicao crescente (EF traduz `IndexOf` para `CHARINDEX`/`instr`),
   garantindo ordem. Decidir: aproximacao por `Contains` documentada, ou fidelidade via `IndexOf`? O curinga `_`
   fica fora do modo portavel em ambas (documentar).

   - Decidido: fidelidade via casamento guloso — ancoras por `StartsWith`/`EndsWith` + guarda de `Length`,
     segmentos intermediarios em ordem via fatiamento (`Substring(IndexOf(seg) + seg.Length)`). A mesma arvore e
     traduzivel pelos providers e executavel em memoria. **Corte em 5 `%`**: excedentes verificados por `Contains`
     sem fatiamento (controla o crescimento exponencial da expressao). A nota de cache de plano do EF vale so para
     o caminho portavel (segmentos como literais); com `EF.Functions.Like` (Fase 4) nao se aplica — o pattern e um
     parametro unico.
8. **Tri-state do override do wrap no `CriterionAttribute` (Fase 3).** Atributos nao aceitam `bool?`; para o
   override distinguir "nao declarado" (segue o default estatico) de declaracao explicita, precisa de enum (ex.:
   `LikeWrap { Default, Wrap, None }`) — nome a definir.

   - Decidido: enum como sugerido (`LikeWrap { Default, Wrap, None }`); mora em `Abstractions`, junto do
     `CriterionAttribute` que o expoe.
9. **Casa unica dos defaults estaticos (Fase 3).** `DefaultStringOperator` e o default do wrap devem morar juntos
   em um unico tipo de opcoes estaticas (evitar estaticos espalhados) — nome a definir (ex.: `CriterionDefaults`),
   e definir se em `Abstractions` (onde esta o `CriterionAttribute`) ou no core Linq (onde esta o
   `DiscoveryCriterionOperator`).

   - Decidido: no pacote de uso — `RoyalCode.SmartSearch.Linq` (onde estao `DiscoveryCriterionOperator` e o helper
     do `Like`); se um default vier a ser consumido por outro pacote, mora no outro.

## Fora de escopo

- Trocar a semantica de traducao de filtros de tipos nao-string.
- Collation/citext no banco (decisao de schema do consumidor; sera apenas documentada como alternativa).
- Ordenacao case-insensitive e `OrderByException` 400 (ja resolvidos, aguardando release).
- Rewriter de `query.Expression` no pacote EF (alternativa rejeitada — nao ve a intencao declarada).
- Isolamento do cache de specifiers por container (limitacao aceita — Questao 2).
- E2E com PostgreSQL real (adiado — Questao 4; avaliar .NET Aspire em iteracao futura, sem complicar agora).
- Curinga `_` no modo portavel do `Like` (Fase 3) — apenas `%`; `_` funciona no caminho EF (`EF.Functions.Like`).

## Ordem recomendada

1. Fase 1 (intencao declarativa + fallback portavel).
2. Fase 2 (seam com a classe encapsuladora).
3. Fase 3 (semantica `Like`/`Contains` no core, com o casamento guloso da Questao 7).
4. Fase 4 (factory `EF.Functions.Like`).
5. Fase 5 (pacote Npgsql com `ILike`).
