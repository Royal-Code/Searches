using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.SmartSearch.Tests;

/// <summary>
/// Integration tests for the <see cref="IAllEntities{TEntity}"/> search.
/// </summary>
public class AllEntitiesTests
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
        result.Should().HaveCount(3);
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
        result.Should().HaveCount(3);
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
        result.Should().HaveCount(1);
        result.Should().ContainSingle(x => x.Id == 2);
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
        result.Should().HaveCount(1).And.ContainSingle(x => x.Id == 2);
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
        result.Should().BeTrue();
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
        result.Should().BeTrue();
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
        result.Should().BeFalse();
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
        result.Should().BeFalse();
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
        result.Should().NotBeNull().And.Match<SimpleModel>(x => x.Id == 2);
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
        SimpleModel? result = await all.FilterBy(filter).FirstDefaultAsync();

        // assert
        result.Should().NotBeNull().And.Match<SimpleModel>(x => x.Id == 2);
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
        result.Should().BeNull();
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
        SimpleModel? result = await all.FilterBy(filter).FirstDefaultAsync();

        // assert
        result.Should().BeNull();
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
        result.Id.Should().Be(2);
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
        result.Id.Should().Be(2);
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
        act.Should().Throw<InvalidOperationException>();
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
        await act.Should().ThrowAsync<InvalidOperationException>();
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
        act.Should().Throw<InvalidOperationException>();
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
        await act.Should().ThrowAsync<InvalidOperationException>();
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