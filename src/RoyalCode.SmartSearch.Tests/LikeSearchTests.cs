using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.SmartSearch.Linq;
using RoyalCode.SmartSearch.Linq.Services;

namespace RoyalCode.SmartSearch.Tests;

/// <summary>
/// Fase 3 do plan-operator-expression-customization: semantica de Like (curingas honrados)
/// vs Contains (substring literal) de ponta a ponta no SQLite, com o helper portavel
/// (as pecas Contains/StartsWith/EndsWith/IndexOf/Substring sao traduzidas pelo provider).
/// </summary>
public class LikeSearchTests
{
	[Fact]
	public async Task Like_ComCuringaDoUsuario_HonraOPattern()
	{
		var provider = await CreateProvider();
		using var scope = provider.CreateScope();
		var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<LkProduto>>();

		var result = await criteria.FilterBy(new LkFiltroAuto { Nome = "Jo%o" }).CollectAsync();

		Assert.Equal(2, result.Count);
		Assert.Contains(result, p => p.Nome == "Joao");
		Assert.Contains(result, p => p.Nome == "Jono");
	}

	[Fact]
	public async Task Like_EContains_DivergemParaPercentLiteral()
	{
		var provider = await CreateProvider();
		using var scope = provider.CreateScope();

		// ICriteria e fluente/stateful: uma instancia nova por consulta
		ICriteria<LkProduto> Criteria() => scope.ServiceProvider.GetRequiredService<ICriteria<LkProduto>>();

		// mesmo valor "100%": Like honra o curinga (2 matches), Contains trata literal (1 match)
		var like = await Criteria().FilterBy(new LkFiltroAuto { Nome = "100%" }).CollectAsync();
		Assert.Equal(2, like.Count);

		var contains = await Criteria().FilterBy(new LkFiltroContains { Nome = "100%" }).CollectAsync();
		var item = Assert.Single(contains);
		Assert.Equal("100% algodao", item.Nome);
	}

	[Fact]
	public async Task Like_SemWrap_ExigeMatchExato_QuandoNaoHaCuringa()
	{
		var provider = await CreateProvider();
		using var scope = provider.CreateScope();

		// ICriteria e fluente/stateful: uma instancia nova por consulta
		ICriteria<LkProduto> Criteria() => scope.ServiceProvider.GetRequiredService<ICriteria<LkProduto>>();

		// "oao" sem wrap: igualdade exata -> nada; com wrap (default) seria substring -> "Joao"
		var exato = await Criteria().FilterBy(new LkFiltroSemWrap { Nome = "oao" }).CollectAsync();
		Assert.Empty(exato);

		var comWrap = await Criteria().FilterBy(new LkFiltroAuto { Nome = "oao" }).CollectAsync();
		var item = Assert.Single(comWrap);
		Assert.Equal("Joao", item.Nome);
	}

	[Fact]
	public async Task Like_Insensitive_MustMatch_IgnoringCase()
	{
		var provider = await CreateProvider();
		using var scope = provider.CreateScope();
		var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<LkProduto>>();

		var result = await criteria.FilterBy(new LkFiltroInsensitive { Nome = "jOnO" }).CollectAsync();

		var item = Assert.Single(result);
		Assert.Equal("Jono", item.Nome);
	}

	private static async Task<IServiceProvider> CreateProvider()
	{
		ServiceCollection services = new();
		var sqlite = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
		await sqlite.OpenAsync();
		services.AddSingleton(sqlite);
		services.AddDbContext<LkDbContext>(b => b.UseSqlite(sqlite));
		services.AddEntityFrameworkSearches<LkDbContext>(s => s.Add<LkProduto>());
		var provider = services.BuildServiceProvider();
		using var scope = provider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<LkDbContext>();
		await db.Database.EnsureCreatedAsync();
		db.AddRange(
			new LkProduto { Id = 1, Nome = "Joao" },
			new LkProduto { Id = 2, Nome = "Jono" },
			new LkProduto { Id = 3, Nome = "Joa" },
			new LkProduto { Id = 4, Nome = "100% algodao" },
			new LkProduto { Id = 5, Nome = "100 metros" });
		await db.SaveChangesAsync();
		return provider;
	}
}

/// <summary>
/// Fase 3: a valvula de escape — trocar o operador default de string para Contains restaura
/// o comportamento literal por completo. Classe separada com tipos proprios (cache global de specifiers).
/// </summary>
public class LikeDefaultStringOperatorTests
{
	[Fact]
	public void DefaultStringOperator_Contains_RestauraComportamentoLiteral()
	{
		ServiceCollection services = new();
		services.AddSmartSearchLinq();
		var specifierFactory = services.BuildServiceProvider().GetRequiredService<ISpecifierFactory>();

		var original = CriterionDefaults.DefaultStringOperator;
		try
		{
			CriterionDefaults.DefaultStringOperator = CriterionOperator.Contains;

			// a geracao ocorre aqui (tipos exclusivos deste teste), lendo o default trocado
			var specifier = specifierFactory.GetSpecifier<LkSwapModel, LkSwapFilter>();

			var query = new List<LkSwapModel>
			{
				new() { Id = 1, Nome = "50% off" },
				new() { Id = 2, Nome = "5000 off" },
			}.AsQueryable();

			// com Like (default da lib), "50%" honraria o curinga e daria 2 matches; literal da 1
			var result = specifier.Specify(query, new LkSwapFilter { Nome = "50%" }).ToList();

			var item = Assert.Single(result);
			Assert.Equal(1, item.Id);
		}
		finally
		{
			CriterionDefaults.DefaultStringOperator = original;
		}
	}
}

file class LkDbContext : DbContext
{
	public LkDbContext(DbContextOptions<LkDbContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<LkProduto>();
}

public class LkProduto
{
	public int Id { get; set; }
	public string Nome { get; set; } = null!;
}

public class LkSwapModel
{
	public int Id { get; set; }
	public string Nome { get; set; } = null!;
}

public class LkFiltroAuto
{
	[Criterion] // Auto -> Like (default): curingas honrados, wrap default
	public string? Nome { get; set; }
}

public class LkFiltroSemWrap
{
	[Criterion(Wrap = LikeWrap.None)]
	public string? Nome { get; set; }
}

public class LkFiltroContains
{
	[Criterion(CriterionOperator.Contains)]
	public string? Nome { get; set; }
}

public class LkFiltroInsensitive
{
	[Criterion(Case = CriterionCase.Insensitive)]
	public string? Nome { get; set; }
}

public class LkSwapFilter
{
	[Criterion] // Auto: le CriterionDefaults.DefaultStringOperator na geracao
	public string? Nome { get; set; }
}
