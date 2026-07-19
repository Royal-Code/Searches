using RoyalCode.SmartSearch.Linq.Filtering;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Tests;

/// <summary>
/// Fase 3 do plan-operator-expression-customization: casamento guloso do Like portavel
/// (ancoras StartsWith/EndsWith, segmentos em ordem via fatiamento, corte de fatiamentos).
/// A mesma arvore e traduzivel pelos providers e executavel em memoria — aqui, executada em memoria.
/// </summary>
public class LikePatternExpressionTests
{
	[Theory]
	// ancoras nas duas pontas: "Jo%o" = comeca com "Jo" e termina com "o" (sem sobreposicao)
	[InlineData("Joao", "Jo%o", true)]
	[InlineData("Jono", "Jo%o", true)]
	[InlineData("Joa", "Jo%o", false)]
	[InlineData("Jo", "Jo%o", false)] // o "o" final nao pode sobrepor o prefixo "Jo"
	// ordem dos segmentos do meio: "%b%a%" exige "b" antes de "a"
	[InlineData("xbya", "%b%a%", true)]
	[InlineData("ab", "%b%a%", false)]
	// segmento repetido: "a%a" exige duas ocorrencias
	[InlineData("aa", "a%a", true)]
	[InlineData("aba", "a%a", true)]
	[InlineData("a", "a%a", false)]
	// ancora simples
	[InlineData("abcd", "abc%", true)]
	[InlineData("zabc", "abc%", false)]
	[InlineData("zabc", "%abc", true)]
	[InlineData("abcz", "%abc", false)]
	// sem curinga, sem wrap: igualdade exata (semantica do LIKE)
	[InlineData("abc", "abc", true)]
	[InlineData("zabcz", "abc", false)]
	public void Match_SemWrap(string candidate, string pattern, bool expected)
	{
		Assert.Equal(expected, Match(candidate, pattern, wrap: false));
	}

	[Theory]
	// com wrap, o valor flutua como substring; curingas internos continuam honrados
	[InlineData("zabcz", "abc", true)]
	[InlineData("zaXbcz", "abc", false)]
	[InlineData("xx100% de algodao", "100%", true)]
	[InlineData("Joao", "Jo%o", true)]
	[InlineData("Joa", "Jo%o", false)]
	public void Match_ComWrap(string candidate, string pattern, bool expected)
	{
		Assert.Equal(expected, Match(candidate, pattern, wrap: true));
	}

	[Theory]
	[InlineData("JOAO", "joao", true)]
	[InlineData("Joao", "jO%o", true)]
	[InlineData("Joao", "z%o", false)]
	public void Match_Insensitive(string candidate, string pattern, bool expected)
	{
		Assert.Equal(expected, Match(candidate, pattern, wrap: true, ignoreCase: true));
	}

	[Fact]
	public void Match_ComCorteDeFatiamentos_ContinuaFuncional()
	{
		// mais segmentos que o corte (MaxSliceOperations = 5): os excedentes degradam para Contains
		Assert.True(Match("abcdefgh", "%a%b%c%d%e%f%g%h%", wrap: false));
		Assert.False(Match("abcdefg", "%a%b%c%d%e%f%g%h%", wrap: false));
		// ordem garantida ate o corte: "b" antes de "a" nos primeiros segmentos nao da match
		Assert.False(Match("bacdefgh", "%a%b%c%d%e%f%g%h%", wrap: false));
	}

	[Fact]
	public void Match_PatternSoDeCuringas_DaMatchEmTudo()
	{
		Assert.True(Match("qualquer", "%", wrap: false));
		Assert.True(Match("", "%%", wrap: false));
	}

	[Fact]
	public void Apply_ComNegacao_InverteOResultado()
	{
		var query = new List<LkPessoa>
		{
			new() { Id = 1, Nome = "Joao" },
			new() { Id = 2, Nome = "Maria" },
		}.AsQueryable();

		var result = LikeExpressionGenerator.Apply<LkPessoa>(
			query, "Jo%o", p => p.Nome, wrap: false, ignoreCase: false, negation: true)
			.ToList();

		var item = Assert.Single(result);
		Assert.Equal(2, item.Id);
	}

	[Fact]
	public void Apply_ComValorVazio_NaoFiltra()
	{
		var query = new List<LkPessoa> { new() { Id = 1, Nome = "Joao" } }.AsQueryable();

		var result = LikeExpressionGenerator.Apply<LkPessoa>(
			query, "", p => p.Nome, wrap: true, ignoreCase: false, negation: false)
			.ToList();

		Assert.Single(result);
	}

	private static bool Match(string candidate, string pattern, bool wrap, bool ignoreCase = false)
	{
		var parameter = Expression.Parameter(typeof(string), "s");
		var expression = LikeExpressionGenerator.CreatePatternExpression(parameter, pattern, wrap, ignoreCase);
		var predicate = Expression.Lambda<Func<string, bool>>(expression, parameter).Compile();
		return predicate(candidate);
	}
}

public class LkPessoa
{
	public int Id { get; set; }
	public string Nome { get; set; } = null!;
}
