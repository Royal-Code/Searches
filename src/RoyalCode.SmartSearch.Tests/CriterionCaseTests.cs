using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.SmartSearch.Linq;
using RoyalCode.SmartSearch.Linq.Services;
using System.Linq.Expressions;

namespace RoyalCode.SmartSearch.Tests;

/// <summary>
/// Fase 1 do plan-operator-expression-customization: intencao declarativa de case sensitivity
/// (`[Criterion(Case = ...)]`) com fallback portavel via ToUpper() em ambos os lados.
/// </summary>
public class CriterionCaseTests
{
	[Theory]
	[InlineData(CriterionOperator.Like)]
	[InlineData(CriterionOperator.Contains)]
	[InlineData(CriterionOperator.StartsWith)]
	[InlineData(CriterionOperator.EndsWith)]
	public void CreateOperatorExpression_Insensitive_MustNormalizeBothSides_WithToUpper(CriterionOperator @operator)
	{
		var filter = Expression.Parameter(typeof(string), "f");
		var target = Expression.Parameter(typeof(string), "t");

		var expression = ExpressionGenerator.CreateOperatorExpression(
			@operator, false, filter, target, CriterionCase.Insensitive);

		var text = expression.ToString();
		Assert.Equal(2, text.Split("ToUpper()").Length - 1);
	}

	[Theory]
	[InlineData(CriterionCase.Default)]
	[InlineData(CriterionCase.Sensitive)]
	public void CreateOperatorExpression_SensitiveOrDefault_MustNotNormalize(CriterionCase criterionCase)
	{
		var filter = Expression.Parameter(typeof(string), "f");
		var target = Expression.Parameter(typeof(string), "t");

		var expression = ExpressionGenerator.CreateOperatorExpression(
			CriterionOperator.Contains, false, filter, target, criterionCase);

		Assert.DoesNotContain("ToUpper()", expression.ToString());
	}

	[Fact]
	public void CreateOperatorExpression_Insensitive_MustBeIgnored_ForNonStringOperands()
	{
		var filter = Expression.Parameter(typeof(int), "f");
		var target = Expression.Parameter(typeof(int), "t");

		var expression = ExpressionGenerator.CreateOperatorExpression(
			CriterionOperator.GreaterThanOrEqual, false, filter, target, CriterionCase.Insensitive);

		Assert.DoesNotContain("ToUpper()", expression.ToString());
	}

	[Fact]
	public void CreateOperatorExpression_Insensitive_MustBeIgnored_ForEqualOperator()
	{
		// o plano restringe a normalizacao aos operadores de string Like/Contains/StartsWith/EndsWith
		var filter = Expression.Parameter(typeof(string), "f");
		var target = Expression.Parameter(typeof(string), "t");

		var expression = ExpressionGenerator.CreateOperatorExpression(
			CriterionOperator.Equal, false, filter, target, CriterionCase.Insensitive);

		Assert.DoesNotContain("ToUpper()", expression.ToString());
	}

	[Fact]
	public void Specifier_ContainsInsensitive_MustMatch_IgnoringCase_InMemory()
	{
		var specifier = CreateSpecifierFactory().GetSpecifier<CcModel, CcInsensitiveFilter>();
		var query = CcData().AsQueryable();

		var result = specifier.Specify(query, new CcInsensitiveFilter { Nome = "notebook" }).ToList();

		var item = Assert.Single(result);
		Assert.Equal("Notebook Gamer", item.Nome);
	}

	[Fact]
	public void Specifier_ContainsDefault_MustNotMatch_DifferentCase_InMemory()
	{
		// sem declaracao de case, o Contains em memoria e ordinal (case-sensitive)
		var specifier = CreateSpecifierFactory().GetSpecifier<CcModel, CcDefaultFilter>();
		var query = CcData().AsQueryable();

		var result = specifier.Specify(query, new CcDefaultFilter { Nome = "notebook" }).ToList();

		Assert.Empty(result);
	}

	[Fact]
	public void Disjunction_LikeInsensitive_MustMatch_IgnoringCase_InMemory()
	{
		var specifier = CreateSpecifierFactory().GetSpecifier<CcPessoa, CcJunctionFilter>();
		var query = new List<CcPessoa>
		{
			new() { Id = 1, Nome = "Carlos", Apelido = "Kadu" },
			new() { Id = 2, Nome = "Bruno", Apelido = "Bl" },
		}.AsQueryable();

		var result = specifier.Specify(query, new CcJunctionFilter { NomeOrApelido = "KADU" }).ToList();

		var item = Assert.Single(result);
		Assert.Equal(1, item.Id);
	}

	[Fact]
	public async Task Search_ContainsInsensitive_MustMatch_OnSqlite()
	{
		var provider = await CreateSqliteProvider();
		using var scope = provider.CreateScope();
		var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<CcEntity>>();

		var result = await criteria
			.FilterBy(new CcEntityInsensitiveFilter { Nome = "nOtEbOoK" })
			.CollectAsync();

		var item = Assert.Single(result);
		Assert.Equal("Notebook Gamer", item.Nome);
	}

	private static ISpecifierFactory CreateSpecifierFactory()
	{
		ServiceCollection services = new();
		services.AddSmartSearchLinq();
		return services.BuildServiceProvider().GetRequiredService<ISpecifierFactory>();
	}

	private static List<CcModel> CcData() =>
	[
		new() { Id = 1, Nome = "Notebook Gamer" },
		new() { Id = 2, Nome = "Mouse" },
	];

	private static async Task<IServiceProvider> CreateSqliteProvider()
	{
		ServiceCollection services = new();
		var sqlite = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
		await sqlite.OpenAsync();
		services.AddSingleton(sqlite);
		services.AddDbContext<CcDbContext>(b => b.UseSqlite(sqlite));
		services.AddEntityFrameworkSearches<CcDbContext>(s => s.Add<CcEntity>());
		var provider = services.BuildServiceProvider();
		using var scope = provider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<CcDbContext>();
		await db.Database.EnsureCreatedAsync();
		db.AddRange(
			new CcEntity { Id = 1, Nome = "Notebook Gamer" },
			new CcEntity { Id = 2, Nome = "Mouse" });
		await db.SaveChangesAsync();
		return provider;
	}
}

file class CcDbContext : DbContext
{
	public CcDbContext(DbContextOptions<CcDbContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<CcEntity>();
}

public class CcModel
{
	public int Id { get; set; }
	public string Nome { get; set; } = null!;
}

public class CcEntity
{
	public int Id { get; set; }
	public string Nome { get; set; } = null!;
}

public class CcPessoa
{
	public int Id { get; set; }
	public string Nome { get; set; } = null!;
	public string Apelido { get; set; } = null!;
}

public class CcInsensitiveFilter
{
	[Criterion(CriterionOperator.Contains, Case = CriterionCase.Insensitive)]
	public string? Nome { get; set; }
}

public class CcDefaultFilter
{
	[Criterion(CriterionOperator.Contains)]
	public string? Nome { get; set; }
}

public class CcEntityInsensitiveFilter
{
	[Criterion(CriterionOperator.Contains, Case = CriterionCase.Insensitive)]
	public string? Nome { get; set; }
}

public class CcJunctionFilter
{
	[Criterion(Case = CriterionCase.Insensitive)] // operador Auto -> Like (default), via disjuncao Nome/Apelido
	public string? NomeOrApelido { get; set; }
}
