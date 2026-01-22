using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.SmartSearch.Tests;

public class OrTests
{
    private static IServiceProvider CreateProvider(string name)
    {
        var services = new ServiceCollection();
        services.AddDbContext<OrDbContext>(b => b.UseInMemoryDatabase(name));
        services.AddEntityFrameworkSearches<OrDbContext>(s => s.Add<OrEntity>());
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Must_NotApplyWhere_WhenOrFilterPropertyIsEmpty()
    {
        // arrange
        var provider = CreateProvider(nameof(Must_NotApplyWhere_WhenOrFilterPropertyIsEmpty));
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
    public void Must_ApplyWhere_WithSingleCondition_WhenOneValueMatchesAnyName()
    {
        // arrange
        var provider = CreateProvider(nameof(Must_ApplyWhere_WithSingleCondition_WhenOneValueMatchesAnyName));
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
    public void Must_ApplyWhere_WithOrConditions_WhenSameValueAppearsInDifferentFields()
    {
        // arrange
        var provider = CreateProvider(nameof(Must_ApplyWhere_WithOrConditions_WhenSameValueAppearsInDifferentFields));
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
    public void Must_ApplyWhere_UsingTargetPropertyPathWithOr()
    {
        // arrange
        var provider = CreateProvider(nameof(Must_ApplyWhere_UsingTargetPropertyPathWithOr));
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
