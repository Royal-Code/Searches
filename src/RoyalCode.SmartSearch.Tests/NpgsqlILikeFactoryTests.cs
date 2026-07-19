using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.SmartSearch.EntityFramework.Npgsql.Filtering;
using RoyalCode.SmartSearch.Linq;
using RoyalCode.SmartSearch.Linq.Filtering;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Tests;

/// <summary>
/// Fase 5 do plan-operator-expression-customization: pacote Npgsql com ILIKE para Like + Insensitive.
/// Sem PostgreSQL no repo (Questao 4), a verificacao e por assercao da arvore gerada.
/// </summary>
public class NpgsqlILikeFactoryTests
{
	[Fact]
	public void TryCreate_LikeInsensitive_MustEmit_EfFunctionsILike()
	{
		var factory = new NpgsqlILikeExpressionFactory();

		var expression = factory.TryCreate(CreateContext(CriterionOperator.Like, CriterionCase.Insensitive));

		var call = Assert.IsAssignableFrom<MethodCallExpression>(expression);
		Assert.Equal("ILike", call.Method.Name);
		Assert.Equal(typeof(NpgsqlDbFunctionsExtensions), call.Method.DeclaringType);
		// wrap default (true): o pattern e concatenado com "%"
		Assert.Contains("Concat", call.Arguments[2].ToString());
	}

	[Fact]
	public void TryCreate_WrapNone_MustUsePatternAsIs()
	{
		var factory = new NpgsqlILikeExpressionFactory();

		var expression = factory.TryCreate(CreateContext(CriterionOperator.Like, CriterionCase.Insensitive, LikeWrap.None));

		var call = Assert.IsAssignableFrom<MethodCallExpression>(expression);
		Assert.DoesNotContain("Concat", call.Arguments[2].ToString());
	}

	[Fact]
	public void TryCreate_Negation_MustWrapWithNot()
	{
		var factory = new NpgsqlILikeExpressionFactory();

		var expression = factory.TryCreate(CreateContext(CriterionOperator.Like, CriterionCase.Insensitive, negation: true));

		Assert.Equal(ExpressionType.Not, Assert.IsAssignableFrom<UnaryExpression>(expression).NodeType);
	}

	[Theory]
	[InlineData(CriterionOperator.Like, CriterionCase.Default)]
	[InlineData(CriterionOperator.Like, CriterionCase.Sensitive)]
	[InlineData(CriterionOperator.Contains, CriterionCase.Insensitive)]
	public void TryCreate_ForaDoEscopo_MustReturnNull(CriterionOperator @operator, CriterionCase @case)
	{
		var factory = new NpgsqlILikeExpressionFactory();

		Assert.Null(factory.TryCreate(CreateContext(@operator, @case)));
	}

	[Fact]
	public void AddNpgsqlLikeOperators_MustRegister_ILikeBeforeEfLike()
	{
		ServiceCollection services = new();
		services.AddSmartSearchLinq();
		services.AddNpgsqlLikeOperators();
		var factories = services.BuildServiceProvider().GetRequiredService<CriterionOperatorExpressionFactories>();

		// Insensitive -> ILIKE (a factory Npgsql vence por ordem de registro)
		var insensitive = factories.TryCreate(CreateContext(CriterionOperator.Like, CriterionCase.Insensitive));
		Assert.Equal("ILike", Assert.IsAssignableFrom<MethodCallExpression>(insensitive).Method.Name);

		// demais casos de Like -> EF.Functions.Like
		var @default = factories.TryCreate(CreateContext(CriterionOperator.Like, CriterionCase.Default));
		Assert.Equal(nameof(DbFunctionsExtensions.Like), Assert.IsAssignableFrom<MethodCallExpression>(@default).Method.Name);
	}

	private static CriterionOperatorContext CreateContext(
		CriterionOperator @operator,
		CriterionCase @case,
		LikeWrap wrap = LikeWrap.Default,
		bool negation = false)
	{
		var filter = Expression.Parameter(typeof(string), "f");
		var target = Expression.Parameter(typeof(string), "t");
		return new CriterionOperatorContext(@operator, @case, wrap, negation, filter, target, null, typeof(object));
	}
}
