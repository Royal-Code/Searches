using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.SmartSearch.Tests;

public class DisjunctionTests
{
    private static IServiceProvider CreateProvider(string name)
    {
        var services = new ServiceCollection();
        services.AddDbContext<DisjunctionDbContext>(b => b.UseInMemoryDatabase(name));
        services.AddEntityFrameworkSearches<DisjunctionDbContext>(s => s.Add<DisjunctionEntity>());
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Must_NotApplyWhere_WhenAllDisjunctionFiltersAreEmpty()
    {
        // arrange
        var provider = CreateProvider(nameof(Must_NotApplyWhere_WhenAllDisjunctionFiltersAreEmpty));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DisjunctionDbContext>();
        ctx.AddRange(
            new DisjunctionEntity { Id = 1, P1 = "A", P2 = "X" },
            new DisjunctionEntity { Id = 2, P1 = "B", P2 = "Y" },
            new DisjunctionEntity { Id = 3, P1 = "C", P2 = "Z" }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<DisjunctionEntity>>();

        // act
        var filter = new DisjunctionFilter();
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Must_ApplyWhereWithSingleCondition_WhenOneDisjunctionFilterHasValue()
    {
        // arrange
        var provider = CreateProvider(nameof(Must_ApplyWhereWithSingleCondition_WhenOneDisjunctionFilterHasValue));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DisjunctionDbContext>();
        ctx.AddRange(
            new DisjunctionEntity { Id = 1, P1 = "A", P2 = "X" },
            new DisjunctionEntity { Id = 2, P1 = "B", P2 = "Y" },
            new DisjunctionEntity { Id = 3, P1 = "C", P2 = "Z" }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<DisjunctionEntity>>();

        // act
        var filter = new DisjunctionFilter { P1 = "B" };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Single(result);
        Assert.Equal(2, result.Single().Id);
    }

    [Fact]
    public void Must_ApplyWhereWithOrConditions_WhenMultipleDisjunctionFiltersHaveValue()
    {
        // arrange
        var provider = CreateProvider(nameof(Must_ApplyWhereWithOrConditions_WhenMultipleDisjunctionFiltersHaveValue));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DisjunctionDbContext>();
        ctx.AddRange(
            new DisjunctionEntity { Id = 1, P1 = "A", P2 = "X" },
            new DisjunctionEntity { Id = 2, P1 = "B", P2 = "Y" },
            new DisjunctionEntity { Id = 3, P1 = "C", P2 = "Z" }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<DisjunctionEntity>>();

        // act
        var filter = new DisjunctionFilter { P1 = "B", P2 = "Z" };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Id == 2);
        Assert.Contains(result, x => x.Id == 3);
    }
}

public class DisjunctionDbContext : DbContext
{
    public DisjunctionDbContext(DbContextOptions<DisjunctionDbContext> options) : base(options) { }

    public DbSet<DisjunctionEntity> Entities => Set<DisjunctionEntity>();
}

public class DisjunctionEntity
{
    public int Id { get; set; }
    public string P1 { get; set; } = null!;
    public string P2 { get; set; } = null!;
}

public class DisjunctionFilter
{
    [Disjuction("g1")] 
    public string? P1 { get; set; }

    [Disjuction("g1")] 
    public string? P2 { get; set; }
}
