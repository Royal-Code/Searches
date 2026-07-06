using Microsoft.Extensions.DependencyInjection;
using RoyalCode.SmartSearch.Linq;
using RoyalCode.SmartSearch.Linq.Filtering;
using RoyalCode.SmartSearch.Linq.Services;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Tests;

/// <summary>
/// Fase 2 do plan-operator-expression-customization: seam de emissao via
/// ICriterionOperatorExpressionFactory (DI, primeira-nao-null-vence pela encapsuladora),
/// alcancando tambem o caminho runtime das disjuncoes.
/// </summary>
public class CriterionOperatorFactoriesTests
{
	[Fact]
	public void Factory_MustCustomizeOperatorExpression()
	{
		// intercepta Contains com "false" constante: nenhum registro deve passar pelo filtro
		var specifier = CreateSpecifierFactory(new OperatorInterceptor(CriterionOperator.Contains, static () => Expression.Constant(false)))
			.GetSpecifier<FacModelA, FacFilterA>();

		var query = new List<FacModelA> { new() { Id = 1, Nome = "abc" } }.AsQueryable();
		var result = specifier.Specify(query, new FacFilterA { Nome = "a" }).ToList();

		Assert.Empty(result);
	}

	[Fact]
	public void Factory_ReturningNull_MustFallBackToDefaultEmission()
	{
		var specifier = CreateSpecifierFactory(new OperatorInterceptor(CriterionOperator.StartsWith, static () => Expression.Constant(false)))
			.GetSpecifier<FacModelB, FacFilterB>();

		var query = new List<FacModelB> { new() { Id = 1, Nome = "abc" }, new() { Id = 2, Nome = "zzz" } }.AsQueryable();
		var result = specifier.Specify(query, new FacFilterB { Nome = "a" }).ToList();

		// a factory intercepta outro operador (StartsWith): o Contains cai na emissao default e filtra normalmente
		var item = Assert.Single(result);
		Assert.Equal(1, item.Id);
	}

	[Fact]
	public void Factories_MustBeTried_InRegistrationOrder_FirstNonNullWins()
	{
		var alwaysFalse = new OperatorInterceptor(CriterionOperator.Contains, static () => Expression.Constant(false));
		var alwaysTrue = new OperatorInterceptor(CriterionOperator.Contains, static () => Expression.Constant(true));

		// false primeiro: nada passa
		var specifier = CreateSpecifierFactory(alwaysFalse, alwaysTrue).GetSpecifier<FacModelC, FacFilterC>();
		var query = new List<FacModelC> { new() { Id = 1, Nome = "abc" }, new() { Id = 2, Nome = "zzz" } }.AsQueryable();
		Assert.Empty(specifier.Specify(query, new FacFilterC { Nome = "a" }).ToList());

		// true primeiro (tipos distintos por causa do cache global de specifiers): tudo passa
		var specifier2 = CreateSpecifierFactory(alwaysTrue, alwaysFalse).GetSpecifier<FacModelD, FacFilterD>();
		var query2 = new List<FacModelD> { new() { Id = 1, Nome = "abc" }, new() { Id = 2, Nome = "zzz" } }.AsQueryable();
		Assert.Equal(2, specifier2.Specify(query2, new FacFilterD { Nome = "a" }).ToList().Count);
	}

	[Fact]
	public void DisjunctionRuntimePath_MustUseFactory()
	{
		// a disjuncao ("NomeOrApelido") monta a expressao em runtime: a factory deve valer la tambem
		var specifier = CreateSpecifierFactory(new OperatorInterceptor(CriterionOperator.Contains, static () => Expression.Constant(false)))
			.GetSpecifier<FacModelE, FacFilterE>();

		var query = new List<FacModelE> { new() { Id = 1, Nome = "abc", Apelido = "zzz" } }.AsQueryable();
		var result = specifier.Specify(query, new FacFilterE { NomeOrApelido = "a" }).ToList();

		Assert.Empty(result);
	}

	private static ISpecifierFactory CreateSpecifierFactory(params ICriterionOperatorExpressionFactory[] factories)
	{
		ServiceCollection services = new();
		services.AddSmartSearchLinq();
		foreach (var factory in factories)
			services.AddSingleton(factory);
		return services.BuildServiceProvider().GetRequiredService<ISpecifierFactory>();
	}

	private sealed class OperatorInterceptor : ICriterionOperatorExpressionFactory
	{
		private readonly CriterionOperator @operator;
		private readonly Func<Expression> create;

		public OperatorInterceptor(CriterionOperator @operator, Func<Expression> create)
		{
			this.@operator = @operator;
			this.create = create;
		}

		public Expression? TryCreate(in CriterionOperatorContext context)
			=> context.Operator == @operator ? create() : null;
	}
}

public class FacModelA { public int Id { get; set; } public string Nome { get; set; } = null!; }
public class FacModelB { public int Id { get; set; } public string Nome { get; set; } = null!; }
public class FacModelC { public int Id { get; set; } public string Nome { get; set; } = null!; }
public class FacModelD { public int Id { get; set; } public string Nome { get; set; } = null!; }
public class FacModelE { public int Id { get; set; } public string Nome { get; set; } = null!; public string Apelido { get; set; } = null!; }

public class FacFilterA
{
	[Criterion(CriterionOperator.Contains)]
	public string? Nome { get; set; }
}

public class FacFilterB
{
	[Criterion(CriterionOperator.Contains)]
	public string? Nome { get; set; }
}

public class FacFilterC
{
	[Criterion(CriterionOperator.Contains)]
	public string? Nome { get; set; }
}

public class FacFilterD
{
	[Criterion(CriterionOperator.Contains)]
	public string? Nome { get; set; }
}

public class FacFilterE
{
	[Criterion(CriterionOperator.Contains)]
	public string? NomeOrApelido { get; set; }
}
