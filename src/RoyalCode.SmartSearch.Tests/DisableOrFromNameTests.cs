using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.SmartSearch.Tests;

public class DisableOrFromNameTests
{
    private static async Task<IServiceProvider> CreateProvider(string name)
    {
        var services = new ServiceCollection();

        var sqliteConnection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        await sqliteConnection.OpenAsync();
        services.AddSingleton(sqliteConnection);

        services.AddDbContext<OrNamesDbContext>(b => b.UseSqlite(sqliteConnection));
        services.AddEntityFrameworkSearches<OrNamesDbContext>(s => s.Add<OrNamesEntity>());

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrNamesDbContext>();
        await db.Database.EnsureCreatedAsync();

        return provider;
    }

    [Fact]
    public async Task Must_ApplyDefaultResolution_When_NameStartsWithOr_And_DisableOrFromName()
    {
        // arrange
        var provider = await CreateProvider(nameof(Must_ApplyDefaultResolution_When_NameStartsWithOr_And_DisableOrFromName));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrNamesDbContext>();
        ctx.AddRange(
            new OrNamesEntity { Id = 1, Ordem = "Pedido-001", TempoOrdinario = "Morning", NomeOrApelido = "John" },
            new OrNamesEntity { Id = 2, Ordem = "Pedido-002", TempoOrdinario = "Afternoon", NomeOrApelido = "Mary" },
            new OrNamesEntity { Id = 3, Ordem = "Pedido-003", TempoOrdinario = "Evening", NomeOrApelido = "Alex" }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<OrNamesEntity>>();

        // act: filter by property that starts with "Or"; should NOT split by Or and should target the entity property itself
        var filter = new FiltroOrdem { Ordem = "Pedido-002" };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Single(result);
        Assert.Equal(2, result.Single().Id);
    }

    [Fact]
    public async Task Must_ApplyDefaultResolution_When_NameHasOrInMiddle_And_DisableOrFromName()
    {
        // arrange
        var provider = await CreateProvider(nameof(Must_ApplyDefaultResolution_When_NameHasOrInMiddle_And_DisableOrFromName));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrNamesDbContext>();
        ctx.AddRange(
            new OrNamesEntity { Id = 1, Ordem = "001", TempoOrdinario = "Morning", NomeOrApelido = "John" },
            new OrNamesEntity { Id = 2, Ordem = "002", TempoOrdinario = "Afternoon", NomeOrApelido = "Mary" },
            new OrNamesEntity { Id = 3, Ordem = "003", TempoOrdinario = "Evening", NomeOrApelido = "Alex" }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<OrNamesEntity>>();

        // act: property with "Or" in the middle should not be split; target the same property
        var filter = new FiltroTempoOrdinario { TempoOrdinario = "Afternoon" };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Single(result);
        Assert.Equal(2, result.Single().Id);
    }

    [Fact]
    public async Task Must_NotSplitByOr_When_DisableOrFromName_IsTrue_In_MiddleOfName()
    {
        // arrange
        var provider = await CreateProvider(nameof(Must_NotSplitByOr_When_DisableOrFromName_IsTrue_In_MiddleOfName));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrNamesDbContext>();
        ctx.AddRange(
            new OrNamesEntity { Id = 1, Ordem = "A", TempoOrdinario = "Morning", NomeOrApelido = "John" },
            new OrNamesEntity { Id = 2, Ordem = "B", TempoOrdinario = "Afternoon", NomeOrApelido = "John" },
            new OrNamesEntity { Id = 3, Ordem = "C", TempoOrdinario = "Evening", NomeOrApelido = "Alex" }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<OrNamesEntity>>();

        // act: property with "Or" and DisableOrFromName should not create disjunction; match exactly on the property
        var filter = new FiltroNomeOrApelido { NomeOrApelido = "John" };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Id == 1);
        Assert.Contains(result, x => x.Id == 2);
    }
}

public class OrNamesDbContext(DbContextOptions<OrNamesDbContext> options) : DbContext(options)
{
    public DbSet<OrNamesEntity> Entities => Set<OrNamesEntity>();
}

public class OrNamesEntity
{
    public int Id { get; set; }
    public string Ordem { get; set; } = null!;           // starts with "Or"
    public string TempoOrdinario { get; set; } = null!;  // contains "Or" in the middle
    public string NomeOrApelido { get; set; } = null!;   // contains "Or" in the middle
}

public class FiltroOrdem
{
    [Criterion(DisableOrFromName = true)]
    public string? Ordem { get; set; }
}

public class FiltroTempoOrdinario
{
    [Criterion(DisableOrFromName = true)]
    public string? TempoOrdinario { get; set; }
}

public class FiltroNomeOrApelido
{
    [Criterion(DisableOrFromName = true)]
    public string? NomeOrApelido { get; set; }
}
