using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.SmartSearch.Tests;

/// <summary>
/// Integration tests for the <see cref="IAllEntities{TEntity}"/> search.
/// </summary>
public class CriteriaAndCollectTests
{
    // configure test container
    private static IServiceProvider CreateServiceProvider(string name)
    {
        ServiceCollection services = new();

        services.AddDbContext<AllDbContext>(builder => builder.UseInMemoryDatabase(name));

        services.AddEntityFrameworkSearches<AllDbContext>(s => s.Add<SimpleModel>());

        ServiceProvider provider = services.BuildServiceProvider();

        return provider;
    }

    [Fact]
    public void Must_CollectAll_WhenNoFilterOrSorting()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_CollectAll_WhenNoFilterOrSorting));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        context.SaveChanges();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        IReadOnlyList<SimpleModel> result = all.Collect();

        // assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Must_CollectAll_WhenNoFilterOrSortingAsync()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_CollectAll_WhenNoFilterOrSortingAsync));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        await context.SaveChangesAsync();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        IReadOnlyList<SimpleModel> result = await all.CollectAsync();

        // assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Must_CollectOne_WhenFilterByName()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_CollectOne_WhenFilterByName));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        context.SaveChanges();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "B" };
        IReadOnlyList<SimpleModel> result = all.FilterBy(filter).Collect();

        // assert
        Assert.Single(result);
        Assert.Equal(2, result.Single().Id);
    }

    [Fact]
    public async Task Must_CollectOne_WhenFilterByNameAsync()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_CollectOne_WhenFilterByNameAsync));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        await context.SaveChangesAsync();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "B" };
        IReadOnlyList<SimpleModel> result = await all.FilterBy(filter).CollectAsync();

        // assert
        Assert.Single(result);
        Assert.Equal(2, result.Single().Id);
    }

    [Fact]
    public void Must_Exists_WhenFilterByName()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_Exists_WhenFilterByName));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        context.SaveChanges();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "B" };
        bool result = all.FilterBy(filter).Exists();

        // assert
        Assert.True(result);
    }

    [Fact]
    public async Task Must_Exists_WhenFilterByNameAsync()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_Exists_WhenFilterByNameAsync));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        await context.SaveChangesAsync();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "B" };
        bool result = await all.FilterBy(filter).ExistsAsync();

        // assert
        Assert.True(result);
    }

    [Fact]
    public void Must_NotExists_WhenFilterByName()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_NotExists_WhenFilterByName));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        context.SaveChanges();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "D" };
        bool result = all.FilterBy(filter).Exists();

        // assert
        Assert.False(result);
    }

    [Fact]
    public async Task Must_NotExists_WhenFilterByNameAsync()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_NotExists_WhenFilterByNameAsync));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        await context.SaveChangesAsync();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "D" };
        bool result = await all.FilterBy(filter).ExistsAsync();

        // assert
        Assert.False(result);
    }

    [Fact]
    public void Must_First_WhenFilterByName()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_First_WhenFilterByName));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        context.SaveChanges();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "B" };
        SimpleModel? result = all.FilterBy(filter).FirstOrDefault();

        // assert

        // assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public async Task Must_First_WhenFilterByNameAsync()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_First_WhenFilterByNameAsync));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        await context.SaveChangesAsync();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "B" };
        SimpleModel? result = await all.FilterBy(filter).FirstOrDefaultAsync();

        // assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public void Must_FirstBeNull_WhenFilterByName()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_FirstBeNull_WhenFilterByName));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        context.SaveChanges();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "D" };
        SimpleModel? result = all.FilterBy(filter).FirstOrDefault();

        // assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Must_FirstBeNull_WhenFilterByNameAsync()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_FirstBeNull_WhenFilterByNameAsync));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        await context.SaveChangesAsync();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "D" };
        SimpleModel? result = await all.FilterBy(filter).FirstOrDefaultAsync();

        // assert
        Assert.Null(result);
    }

    [Fact]
    public void Must_Single_WhenFilterByName()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_Single_WhenFilterByName));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        context.SaveChanges();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "B" };
        SimpleModel result = all.FilterBy(filter).Single();

        // assert
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public async Task Must_Single_WhenFilterByNameAsync()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_Single_WhenFilterByNameAsync));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        await context.SaveChangesAsync();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "B" };
        SimpleModel result = await all.FilterBy(filter).SingleAsync();

        // assert
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public void Must_Throw_WhenSingle_HasMoreThenOne()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_Throw_WhenSingle_HasMoreThenOne));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        context.SaveChanges();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        Action act = () => all.Single();

        // assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public async Task Must_Throw_WhenSingle_HasMoreThenOneAsync()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_Throw_WhenSingle_HasMoreThenOneAsync));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        await context.SaveChangesAsync();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        Func<Task> act = () => all.SingleAsync();

        // assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }

    [Fact]
    public void Must_Throw_WhenSingle_HasNoOne()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_Throw_WhenSingle_HasNoOne));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        context.SaveChanges();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "D" };
        Action act = () => all.FilterBy(filter).Single();

        // assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public async Task Must_Throw_WhenSingle_HasNoOneAsync()
    {
        // arrange
        IServiceProvider provider = CreateServiceProvider(nameof(Must_Throw_WhenSingle_HasNoOneAsync));

        using IServiceScope scope = provider.CreateScope();
        AllDbContext context = scope.ServiceProvider.GetRequiredService<AllDbContext>();

        context.Add(new SimpleModel { Id = 1, Name = "A" });
        context.Add(new SimpleModel { Id = 2, Name = "B" });
        context.Add(new SimpleModel { Id = 3, Name = "C" });
        await context.SaveChangesAsync();

        var all = scope.ServiceProvider.GetRequiredService<ICriteria<SimpleModel>>();

        // act
        var filter = new SimpleFilter { Name = "D" };
        Func<Task> act = () => all.FilterBy(filter).SingleAsync();

        // assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }
}

file class AllDbContext : DbContext
{
    public AllDbContext(DbContextOptions<AllDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SimpleModel>();
    }
}