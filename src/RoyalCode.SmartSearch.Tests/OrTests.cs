using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.SmartSearch.Tests;

public class OrTests
{
    private static async Task<IServiceProvider> CreateProvider(string name)
    {
        var services = new ServiceCollection();

        var sqliteConnection = new Microsoft.Data.Sqlite.SqliteConnection($"DataSource=:memory:");
        await sqliteConnection.OpenAsync();
        services.AddSingleton(sqliteConnection);

        services.AddDbContext<OrDbContext>(b => b.UseSqlite(sqliteConnection));
        services.AddEntityFrameworkSearches<OrDbContext>(s => s.Add<OrEntity>());

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrDbContext>();
        await db.Database.EnsureCreatedAsync();

        return provider;
    }

    [Fact]
    public async Task Must_NotApplyWhere_WhenOrFilterPropertyIsEmpty()
    {
        // arrange
        var provider = await CreateProvider(nameof(Must_NotApplyWhere_WhenOrFilterPropertyIsEmpty));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrDbContext>();
        ctx.AddRange(
            new OrEntity { Id = 1, FirstName = "John", MiddleName = "A", LastName = "Smith" },
            new OrEntity { Id = 2, FirstName = "Mary", MiddleName = "B", LastName = "Jones" }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<OrEntity>>();

        // act
        var filter = new OrFilterNameProperty();
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Must_ApplyWhere_WithSingleCondition_WhenOneValueMatchesAnyName()
    {
        // arrange
        var provider = await CreateProvider(nameof(Must_ApplyWhere_WithSingleCondition_WhenOneValueMatchesAnyName));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrDbContext>();
        ctx.AddRange(
            new OrEntity { Id = 1, FirstName = "John", MiddleName = "A", LastName = "Smith" },
            new OrEntity { Id = 2, FirstName = "Mary", MiddleName = "B", LastName = "Jones" },
            new OrEntity { Id = 3, FirstName = "Alex", MiddleName = "John", LastName = "Doe" }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<OrEntity>>();

        // act
        var filter = new OrFilterNameProperty { FirstNameOrMiddleNameOrLastName = "Mary" };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Single(result);
        Assert.Equal(2, result.Single().Id);
    }

    [Fact]
    public async Task Must_ApplyWhere_WithOrConditions_WhenSameValueAppearsInDifferentFields()
    {
        // arrange
        var provider = await CreateProvider(nameof(Must_ApplyWhere_WithOrConditions_WhenSameValueAppearsInDifferentFields));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrDbContext>();
        ctx.AddRange(
            new OrEntity { Id = 1, FirstName = "John", MiddleName = "A", LastName = "Smith" },
            new OrEntity { Id = 2, FirstName = "Mary", MiddleName = "B", LastName = "John" },
            new OrEntity { Id = 3, FirstName = "Alex", MiddleName = "John", LastName = "Doe" }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<OrEntity>>();

        // act
        var filter = new OrFilterNameProperty { FirstNameOrMiddleNameOrLastName = "John" };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x.Id == 1);
        Assert.Contains(result, x => x.Id == 2);
        Assert.Contains(result, x => x.Id == 3);
    }

    [Fact]
    public async Task Must_ApplyWhere_UsingTargetPropertyPathWithOr()
    {
        // arrange
        var provider = await CreateProvider(nameof(Must_ApplyWhere_UsingTargetPropertyPathWithOr));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrDbContext>();
        ctx.AddRange(
            new OrEntity { Id = 1, FirstName = "John", MiddleName = "A", LastName = "Smith" },
            new OrEntity { Id = 2, FirstName = "Mary", MiddleName = "B", LastName = "John" },
            new OrEntity { Id = 3, FirstName = "Alex", MiddleName = "John", LastName = "Doe" }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<OrEntity>>();

        // act
        var filter = new OrFilterTargetPath { Query = "John" };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Id == 1);
        Assert.Contains(result, x => x.Id == 2);
    }
}

public class OrDbContext(DbContextOptions<OrDbContext> options) : DbContext(options)
{
    public DbSet<OrEntity> Entities => Set<OrEntity>();
}

public class OrEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}

public class OrFilterNameProperty
{
    [Criterion] // default operator for string is Like
    public string? FirstNameOrMiddleNameOrLastName { get; set; }
}

public class OrFilterTargetPath
{
    [Criterion(TargetPropertyPath = "FirstNameOrLastName")] // use Or in target property path
    public string? Query { get; set; }
}
