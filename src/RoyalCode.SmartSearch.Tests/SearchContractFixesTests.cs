using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.SmartSearch.Exceptions;
using RoyalCode.SmartSearch.Linq.Sortings;

namespace RoyalCode.SmartSearch.Tests;

public class SearchContractFixesTests
{
	private static async Task<IServiceProvider> CreateProvider(int items)
	{
		ServiceCollection services = new();
		var sqlite = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
		await sqlite.OpenAsync();
		services.AddSingleton(sqlite);
		services.AddDbContext<FixesDbContext>(b => b.UseSqlite(sqlite));
		services.AddEntityFrameworkSearches<FixesDbContext>(s =>
		{
			s.Add<SimpleModel>();
			s.Add<TemporalModel>();
		});
		var provider = services.BuildServiceProvider();
		using var scope = provider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<FixesDbContext>();
		await db.Database.EnsureCreatedAsync();
		for (var i = 1; i <= items; i++)
		{
			db.Add(new SimpleModel { Id = i, Name = $"N{i:00}" });
			db.Add(new TemporalModel { Id = i, CriadoEm = DateTimeOffset.UtcNow.AddDays(-i) });
		}
		await db.SaveChangesAsync();
		return provider;
	}

	[Fact]
	public async Task Pages_UsesCeiling_NotFloor()
	{
		// 3 itens, 2 por pagina => 2 paginas (antes o Floor devolvia 1).
		var provider = await CreateProvider(3);
		using var scope = provider.CreateScope();
		var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

		var result = await criteria
			.WithOptions(new SearchOptions { ItemsPerPage = 2, Page = 1 })
			.AsSearch().ToListAsync();

		Assert.Equal(3, result.Count);
		Assert.Equal(2, result.ItemsPerPage);
		Assert.Equal(2, result.Pages);
	}

	[Fact]
	public async Task OrderByInvalido_Lanca_OrderByNotSupported_QueEhOrderByException()
	{
		var provider = await CreateProvider(2);
		using var scope = provider.CreateScope();
		var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

		var ex = await Assert.ThrowsAsync<OrderByNotSupportedException>(async () =>
			await criteria
				.OrderBy(new Sorting { OrderBy = "PropriedadeInexistente" })
				.CollectAsync());

		// Contrato que permite ao Performer (AspNetCore) traduzir para 400 InvalidParameter em vez de 500:
		// a excecao de ordenacao invalida deve ser um OrderByException.
		Assert.IsAssignableFrom<OrderByException>(ex);
	}

	[Fact]
	public async Task OrderByNaoTraduzivel_Lanca_OrderByException_ComInnerDoProvider()
	{
		// O SQLite nao suporta ORDER BY em DateTimeOffset; a NotSupportedException do provider so estoura
		// na materializacao (traducao lazy). Com ordenacao pedida pelo usuario, o erro deve ser atribuido
		// a ela e relancado como OrderByException, permitindo ao Performer responder 400 em vez de 500.
		var provider = await CreateProvider(3);
		using var scope = provider.CreateScope();
		var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<TemporalModel>>();

		var ex = await Assert.ThrowsAsync<OrderByException>(async () =>
			await criteria
				.OrderBy(new Sorting { OrderBy = "CriadoEm" })
				.CollectAsync());

		Assert.Equal("CriadoEm", ex.PropertyName);
		Assert.IsType<NotSupportedException>(ex.InnerException);
	}
}

file class FixesDbContext : DbContext
{
	public FixesDbContext(DbContextOptions<FixesDbContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<SimpleModel>();
		modelBuilder.Entity<TemporalModel>();
	}
}

public class TemporalModel
{
	public int Id { get; set; }
	public DateTimeOffset CriadoEm { get; set; }
}
