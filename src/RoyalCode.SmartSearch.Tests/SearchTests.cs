using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.SmartSearch.Tests;

public class SearchTests
{
    private static async Task<IServiceProvider> CreateServiceProvider(string name)
    {
        ServiceCollection services = new();

        var sqliteConnection = new Microsoft.Data.Sqlite.SqliteConnection($"DataSource=:memory:");
        await sqliteConnection.OpenAsync();
        services.AddSingleton(sqliteConnection);

        services.AddDbContext<SearchDbContext>(builder => builder.UseSqlite(sqliteConnection));
        services.AddEntityFrameworkSearches<SearchDbContext>(s => s.Add<SimpleModel>());

        ServiceProvider provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
        await db.Database.EnsureCreatedAsync();
        await db.Seed();

        return provider;
    }

    [Fact]
    public async Task Search_ShouldReturnFirstPage_WhenNoFilterAndNoOptionsApplied()
    {
        // Arrange
        var provider = await CreateServiceProvider(nameof(Search_ShouldReturnFirstPage_WhenNoFilterAndNoOptionsApplied));
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
        var criteria = db.Criteria<SearchEntity>();

        // Act
        var filter = new SearchTestsFilter();
        var options = new SearchOptions();
        var resultList = await criteria
            .WithOptions(options)
            .FilterBy(filter)
            .AsSearch()
            .ToListAsync();

        // Assert
        Assert.NotEmpty(resultList.Items);
        Assert.Equal(1, resultList.Page);
        Assert.Equal(10, resultList.ItemsPerPage);
        Assert.Equal(0, resultList.Skipped);
        Assert.Equal(10, resultList.Taken);
        Assert.NotEmpty(resultList.Sortings);
    }
}

public class SearchDbContext : DbContext
{
    public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options) { }

    public DbSet<SearchEntity> SearchEntities { get; set; } = null!;

    public async Task Seed()
    {
        var faker = new Faker();

        // cria 50 entidades de pesquisa com nomes aleatórios
        for (int i = 0; i < 50; i++)
        {
            SearchEntity entity = new(faker.Name.FullName());
            SearchEntities.Add(entity);
        }

        await SaveChangesAsync();
    }
}

public class SearchEntity
{
    public SearchEntity(string name)
    {
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

#nullable disable
    public SearchEntity() { }
#nullable enable

    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SearchTestsFilter
{
    public string? Name { get; set; }

    [Criterion(TargetProperty = nameof(SearchEntity.CreatedAt), Operator = CriterionOperator.GreaterThanOrEqual)]
    public DateTime? CreatedAtFrom { get; set; }

    [Criterion(TargetProperty = nameof(SearchEntity.CreatedAt), Operator = CriterionOperator.LessThanOrEqual)]
    public DateTime? CreatedAtTo { get; set; }
}