using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.SmartSearch.Linq;
using RoyalCode.SmartSearch.Linq.Sortings;

namespace RoyalCode.SmartSearch.Tests;

/// <summary>
/// O nome do order by nao deve depender do case: "nome" deve resolver a propriedade "Nome".
/// Cobre o fallback case-insensitive do <see cref="DefaultOrderByGenerator"/> e o lookup
/// case-insensitive do mapa de handlers (registros manuais via AddOrderBy).
/// </summary>
public class CaseInsensitiveOrderByTests
{
	[Theory]
	[InlineData("Nome")]
	[InlineData("nome")]
	[InlineData("NOME")]
	public void Generate_MustResolveProperty_IgnoringCase(string orderBy)
	{
		var generator = new DefaultOrderByGenerator();

		var expression = generator.Generate<CiModel>(orderBy);

		Assert.NotNull(expression);
	}

	[Theory]
	[InlineData("Filho.Nome")]
	[InlineData("filho.nome")]
	[InlineData("FILHO.NOME")]
	public void Generate_MustResolveNestedPath_IgnoringCase(string orderBy)
	{
		var generator = new DefaultOrderByGenerator();

		var expression = generator.Generate<CiModel>(orderBy);

		Assert.NotNull(expression);
	}

	[Fact]
	public void Generate_MustReturnNull_WhenPropertiesDifferOnlyByCase()
	{
		// "vAlOr" e ambiguo entre Valor e VALOR -> order by nao suportado
		var generator = new DefaultOrderByGenerator();

		var expression = generator.Generate<CiAmbiguousModel>("vAlOr");

		Assert.Null(expression);
	}

	[Fact]
	public void Generate_MustResolveExactMatch_WhenPropertiesDifferOnlyByCase()
	{
		var generator = new DefaultOrderByGenerator();

		var expression = generator.Generate<CiAmbiguousModel>("Valor");

		Assert.NotNull(expression);
	}

	[Theory]
	[InlineData(ListSortDirection.Ascending, new[] { "Alfa", "Bravo", "Charlie" })]
	[InlineData(ListSortDirection.Descending, new[] { "Charlie", "Bravo", "Alfa" })]
	public async Task Search_MustOrder_WhenOrderByCaseDiffers(ListSortDirection direction, string[] expected)
	{
		var provider = await CreateProvider();
		using var scope = provider.CreateScope();
		var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<CiEntity>>();

		var result = await criteria
			.WithOptions(new SearchOptions())
			.OrderBy(new Sorting { OrderBy = "nome", Direction = direction })
			.Select<CiEntityDto>()
			.ToListAsync();

		Assert.Equal(expected, result.Items.Select(x => x.Nome).ToArray());
	}

	[Fact]
	public async Task Search_MustOrder_WhenManualHandlerRegisteredWithDifferentCase()
	{
		// "NomeCliente" nao e propriedade da entidade: so resolve pelo handler manual,
		// que deve ser encontrado mesmo com o case diferente no order by.
		var provider = await CreateProvider(cfg => cfg.AddOrderBy<CiEntity, string>("NomeCliente", x => x.Nome));
		using var scope = provider.CreateScope();
		var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<CiEntity>>();

		var result = await criteria
			.WithOptions(new SearchOptions())
			.OrderBy(new Sorting { OrderBy = "nomecliente", Direction = ListSortDirection.Descending })
			.Select<CiEntityDto>()
			.ToListAsync();

		Assert.Equal(["Charlie", "Bravo", "Alfa"], result.Items.Select(x => x.Nome).ToArray());
	}

	private static async Task<IServiceProvider> CreateProvider(Action<ISearchConfigurations>? configure = null)
	{
		ServiceCollection services = new();
		var sqlite = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
		await sqlite.OpenAsync();
		services.AddSingleton(sqlite);
		services.AddDbContext<CiDbContext>(b => b.UseSqlite(sqlite));
		services.AddEntityFrameworkSearches<CiDbContext>(s =>
		{
			s.Add<CiEntity>();
			configure?.Invoke(s);
		});
		var provider = services.BuildServiceProvider();
		using var scope = provider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<CiDbContext>();
		await db.Database.EnsureCreatedAsync();
		db.AddRange(
			new CiEntity { Id = 1, Nome = "Bravo" },
			new CiEntity { Id = 2, Nome = "Alfa" },
			new CiEntity { Id = 3, Nome = "Charlie" });
		await db.SaveChangesAsync();
		return provider;
	}
}

file class CiDbContext : DbContext
{
	public CiDbContext(DbContextOptions<CiDbContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<CiEntity>();
}

public class CiEntity
{
	public int Id { get; set; }
	public string Nome { get; set; } = null!;
}

public class CiEntityDto
{
	public int Id { get; set; }
	public string Nome { get; set; } = null!;
}

public class CiModel
{
	public int Id { get; set; }
	public string Nome { get; set; } = null!;
	public CiChildModel Filho { get; set; } = null!;
}

public class CiChildModel
{
	public string Nome { get; set; } = null!;
}

#pragma warning disable S101, IDE1006 // proposital: propriedades diferindo apenas por case
public class CiAmbiguousModel
{
	public decimal Valor { get; set; }
	public decimal VALOR { get; set; }
}
#pragma warning restore S101, IDE1006
