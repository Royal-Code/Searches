using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.SmartSearch.Tests;

/// <summary>
/// Regressao: a ordenacao deve ser preservada quando a busca projeta para um DTO (Select) E aplica paginacao.
/// Bug anterior: CriteriaQuery.Select nao propagava o estado de ordenacao ja aplicado; com paginacao, CheckSorting
/// reaplicava a ordenacao default (Id) sobre a projecao, sobrescrevendo a ordem pedida.
/// </summary>
public class DtoProjectionSortingTests
{
	private static async Task<IServiceProvider> CreateProvider()
	{
		ServiceCollection services = new();
		var sqlite = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
		await sqlite.OpenAsync();
		services.AddSingleton(sqlite);
		services.AddDbContext<ProjDbContext>(b => b.UseSqlite(sqlite));
		services.AddEntityFrameworkSearches<ProjDbContext>(s => s.Add<SimpleModel>());
		var provider = services.BuildServiceProvider();
		using var scope = provider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ProjDbContext>();
		await db.Database.EnsureCreatedAsync();
		db.AddRange(
			new SimpleModel { Id = 1, Name = "Bravo" },
			new SimpleModel { Id = 2, Name = "Alfa" },
			new SimpleModel { Id = 3, Name = "Charlie" });
		await db.SaveChangesAsync();
		return provider;
	}

	[Fact]
	public async Task Select_Dto_WithPagination_PreservesDescendingOrder()
	{
		var provider = await CreateProvider();
		using var scope = provider.CreateScope();
		var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

		var result = await criteria
			.WithOptions(new SearchOptions()) // aplica paginacao (take padrao)
			.OrderBy(new Sorting { OrderBy = "Name", Direction = ListSortDirection.Descending })
			.Select<SimpleModelDto>()
			.ToListAsync();

		Assert.Equal(["Charlie", "Bravo", "Alfa"], result.Items.Select(x => x.Name).ToArray());
		Assert.Contains(result.Sortings, s => s.OrderBy == "Name" && s.Direction == ListSortDirection.Descending);
	}

	[Fact]
	public async Task Select_Dto_WithPagination_PreservesAscendingOrder()
	{
		var provider = await CreateProvider();
		using var scope = provider.CreateScope();
		var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

		var result = await criteria
			.WithOptions(new SearchOptions())
			.OrderBy(new Sorting { OrderBy = "Name", Direction = ListSortDirection.Ascending })
			.Select<SimpleModelDto>()
			.ToListAsync();

		Assert.Equal(["Alfa", "Bravo", "Charlie"], result.Items.Select(x => x.Name).ToArray());
	}
}

file class ProjDbContext : DbContext
{
	public ProjDbContext(DbContextOptions<ProjDbContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<SimpleModel>();
}

public class SimpleModelDto
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;
}
