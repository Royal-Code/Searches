using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RoyalCode.SmartSearch.Tests;

public class ComplexTypeTests
{
    private static async Task<IServiceProvider> CreateServiceProvider(string name)
    {
        ServiceCollection services = new();

        var sqliteConnection = new Microsoft.Data.Sqlite.SqliteConnection($"DataSource=:memory:");
        await sqliteConnection.OpenAsync();
        services.AddSingleton(sqliteConnection);

        services.AddDbContext<UserDbContext>(builder => builder.UseSqlite(sqliteConnection));
        services.AddEntityFrameworkSearches<UserDbContext>(s => s.Add<User>());

        ServiceProvider provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await db.Database.EnsureCreatedAsync();

        return provider;
    }
    [Fact]
    public void HasAttribute_WhenComplexFilter_ShouldBeTrue()
    {
        // Arrange
        var type = typeof(Email);

        // Act
        var hasAttribute = ExpressionGenerator.HasAttribute(type, typeof(ComplexFilterAttribute<>), out var attr);

        // Assert
        Assert.True(hasAttribute);

        var genericType = attr!.GetType().GetGenericArguments()[0];
        Assert.Equal(typeof(string), genericType);
    }

    [Fact]
    public async Task Must_FilterBy_ComplexTypePropertyPath_EmailValue()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_ComplexTypePropertyPath_EmailValue));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), FirstName = "John", MiddleName = "A", LastName = "Smith" },
            new User { Id = 2, Email = new Email("mary@site.com"), FirstName = "Mary", MiddleName = "B", LastName = "Jones" }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act
        var filter = new UserFilter { Email = "mary@site.com" };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Single(result);
        Assert.Equal(2, result.Single().Id);
    }

    [Fact]
    public async Task Must_NotApply_Filter_When_Email_IsEmpty()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_NotApply_Filter_When_Email_IsEmpty));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), FirstName = "John", MiddleName = "A", LastName = "Smith" },
            new User { Id = 2, Email = new Email("mary@site.com"), FirstName = "Mary", MiddleName = "B", LastName = "Jones" }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act
        var filter = new UserFilter { Email = null };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
    }
}


[ComplexFilter<string>(nameof(Value))]
internal readonly record struct Email
{
    public string Value { get; }

    public Email(string value)
    {
        Value = value;
    }
}

internal class User
{
    public int Id { get; set; }
    public Email Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}

internal class UserFilter
{
    [Criterion("Email.Value")]
    public string? Email { get; set; }
}

internal class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.ComplexProperty(u => u.Email, email =>
            {
               email.Property(e => e.Value).HasColumnName("Email");
            });
        });

        base.OnModelCreating(modelBuilder);
    }
}