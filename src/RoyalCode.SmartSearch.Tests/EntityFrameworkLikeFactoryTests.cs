using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.SmartSearch.EntityFramework.Filtering;
using RoyalCode.SmartSearch.Linq.Filtering;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Tests;

/// <summary>
/// Fase 4 do plan-operator-expression-customization: emissao do Like como EF.Functions.Like
/// (opt-in via AddEntityFrameworkLikeOperator), com paridade de resultados com o helper portavel.
/// </summary>
public class EntityFrameworkLikeFactoryTests
{
	[Fact]
	public void TryCreate_Like_MustEmit_EfFunctionsLike()
	{
		var factory = new EntityFrameworkLikeExpressionFactory();

		var expression = factory.TryCreate(CreateContext(CriterionOperator.Like, CriterionCase.Default));

		var call = Assert.IsAssignableFrom<MethodCallExpression>(expression);
		Assert.Equal(nameof(DbFunctionsExtensions.Like), call.Method.Name);
		Assert.Equal(typeof(DbFunctionsExtensions), call.Method.DeclaringType);
		// wrap default (true): o pattern e concatenado com "%"
		Assert.Contains("Concat", call.Arguments[2].ToString());
	}

	[Fact]
	public void TryCreate_Insensitive_MustNormalizeBothSides_WithToUpper()
	{
		var factory = new EntityFrameworkLikeExpressionFactory();

		var expression = factory.TryCreate(CreateContext(CriterionOperator.Like, CriterionCase.Insensitive));

		Assert.NotNull(expression);
		Assert.Equal(2, expression.ToString().Split("ToUpper()").Length - 1);
	}

	[Fact]
	public void TryCreate_WrapNone_MustUsePatternAsIs()
	{
		var factory = new EntityFrameworkLikeExpressionFactory();

		var expression = factory.TryCreate(CreateContext(CriterionOperator.Like, CriterionCase.Default, LikeWrap.None));

		var call = Assert.IsAssignableFrom<MethodCallExpression>(expression);
		Assert.DoesNotContain("Concat", call.Arguments[2].ToString());
	}

	[Fact]
	public void TryCreate_Negation_MustWrapWithNot()
	{
		var factory = new EntityFrameworkLikeExpressionFactory();

		var expression = factory.TryCreate(CreateContext(CriterionOperator.Like, CriterionCase.Default, negation: true));

		Assert.Equal(ExpressionType.Not, Assert.IsAssignableFrom<UnaryExpression>(expression).NodeType);
	}

	[Theory]
	[InlineData(CriterionOperator.Contains)]
	[InlineData(CriterionOperator.StartsWith)]
	[InlineData(CriterionOperator.Equal)]
	public void TryCreate_OutrosOperadores_MustReturnNull(CriterionOperator @operator)
	{
		var factory = new EntityFrameworkLikeExpressionFactory();

		Assert.Null(factory.TryCreate(CreateContext(@operator, CriterionCase.Default)));
	}

	[Fact]
	public void TryCreate_OperandosNaoString_MustReturnNull()
	{
		var factory = new EntityFrameworkLikeExpressionFactory();
		var filter = Expression.Parameter(typeof(int), "f");
		var target = Expression.Parameter(typeof(int), "t");
		var context = new CriterionOperatorContext(
			CriterionOperator.Like, CriterionCase.Default, LikeWrap.Default, false,
			filter, target, null, typeof(object));

		Assert.Null(factory.TryCreate(context));
	}

	// --- paridade com o helper portavel (mesmos casos da LikeSearchTests, com a factory registrada) ---

	[Fact]
	public async Task Search_ComEfLike_TemParidadeComOHelperPortavel()
	{
		var provider = await CreateProvider();
		using var scope = provider.CreateScope();

		// ICriteria e fluente/stateful: uma instancia nova por consulta
		ICriteria<EflProduto> Criteria() => scope.ServiceProvider.GetRequiredService<ICriteria<EflProduto>>();

		// curinga do usuario honrado
		var curinga = await Criteria().FilterBy(new EflFiltroAuto { Nome = "Jo%o" }).CollectAsync();
		Assert.Equal(2, curinga.Count);
		Assert.Contains(curinga, p => p.Nome == "Joao");
		Assert.Contains(curinga, p => p.Nome == "Jono");

		// "100%" via Like honra o curinga (2); via Contains e literal (1) — Contains nao passa pela factory
		var like = await Criteria().FilterBy(new EflFiltroAuto { Nome = "100%" }).CollectAsync();
		Assert.Equal(2, like.Count);

		var contains = await Criteria().FilterBy(new EflFiltroContains { Nome = "100%" }).CollectAsync();
		Assert.Single(contains);

		// sem wrap e sem curinga: match exato
		var exato = await Criteria().FilterBy(new EflFiltroSemWrap { Nome = "oao" }).CollectAsync();
		Assert.Empty(exato);

		// insensitive: UPPER(...) LIKE UPPER(...)
		var insensitive = await Criteria().FilterBy(new EflFiltroInsensitive { Nome = "jOnO" }).CollectAsync();
		var item = Assert.Single(insensitive);
		Assert.Equal("Jono", item.Nome);
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

	private static async Task<IServiceProvider> CreateProvider()
	{
		ServiceCollection services = new();
		var sqlite = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
		await sqlite.OpenAsync();
		services.AddSingleton(sqlite);
		services.AddDbContext<EflDbContext>(b => b.UseSqlite(sqlite));
		services.AddEntityFrameworkSearches<EflDbContext>(s => s.Add<EflProduto>());
		services.AddEntityFrameworkLikeOperator();
		var provider = services.BuildServiceProvider();
		using var scope = provider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<EflDbContext>();
		await db.Database.EnsureCreatedAsync();
		db.AddRange(
			new EflProduto { Id = 1, Nome = "Joao" },
			new EflProduto { Id = 2, Nome = "Jono" },
			new EflProduto { Id = 3, Nome = "Joa" },
			new EflProduto { Id = 4, Nome = "100% algodao" },
			new EflProduto { Id = 5, Nome = "100 metros" });
		await db.SaveChangesAsync();
		return provider;
	}
}

file class EflDbContext : DbContext
{
	public EflDbContext(DbContextOptions<EflDbContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<EflProduto>();
}

public class EflProduto
{
	public int Id { get; set; }
	public string Nome { get; set; } = null!;
}

public class EflFiltroAuto
{
	[Criterion]
	public string? Nome { get; set; }
}

public class EflFiltroContains
{
	[Criterion(CriterionOperator.Contains)]
	public string? Nome { get; set; }
}

public class EflFiltroSemWrap
{
	[Criterion(Wrap = LikeWrap.None)]
	public string? Nome { get; set; }
}

public class EflFiltroInsensitive
{
	[Criterion(Case = CriterionCase.Insensitive)]
	public string? Nome { get; set; }
}
