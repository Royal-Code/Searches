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
    public async Task Must_FilterBy_Names_WithOr_ReturnThree()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_Names_WithOr_ReturnThree));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("u1@corp.com"), Name = new PersonName("John", "A", "Smith") },            // FirstName = John
            new User { Id = 2, Email = new Email("u2@corp.com"), Name = new PersonName("Mary", "John", "Jones") },         // MiddleName = John
            new User { Id = 3, Email = new Email("u3@corp.com"), Name = new PersonName("Alex", "C", "John") },            // LastName = John
            new User { Id = 4, Email = new Email("u4@corp.com"), Name = new PersonName("Maria", "B", "Silva") },
            new User { Id = 5, Email = new Email("u5@corp.com"), Name = new PersonName("Peter", "Q", "Parker") },
            new User { Id = 6, Email = new Email("u6@corp.com"), Name = new PersonName("Julia", "R", "Stone") }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: Name.Value applies OR across FirstName/MiddleName/LastName
        var filter = new UserFilterByName { Name = new PersonNameFilter { Value = "John" } };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x.Id == 1);
        Assert.Contains(result, x => x.Id == 2);
        Assert.Contains(result, x => x.Id == 3);
    }

    [Fact]
    public async Task Must_FilterBy_Email_FirstName_And_State_ReturnTwo()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_Email_FirstName_And_State_ReturnTwo));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john.ca1@corp.com"), Name = new PersonName("John", "A", "Smith"), MainAddress = new Address { Street = "Main 1", City = "LA", State = "CA", PostalCode = "90001" } },
            new User { Id = 2, Email = new Email("john.ca2@corp.com"), Name = new PersonName("John", "B", "Jones"), MainAddress = new Address { Street = "Main 2", City = "SF", State = "CA", PostalCode = "94101" } },
            new User { Id = 3, Email = new Email("john.ny@corp.com"),  Name = new PersonName("John", "C", "Doe"),   MainAddress = new Address { Street = "Main 3", City = "NYC", State = "NY", PostalCode = "10001" } },
            new User { Id = 4, Email = new Email("mary.ca@corp.com"),  Name = new PersonName("Mary", "D", "Brown"), MainAddress = new Address { Street = "Main 4", City = "LA",  State = "CA", PostalCode = "90002" } }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: Email contains "john" AND FirstName = "John" AND State = "CA"
        var filter = new UserFilter
        {
            Email = "john",
            Name = new PersonName("John", string.Empty, string.Empty),
            Address = new Address { State = "CA", Street = string.Empty, City = string.Empty, PostalCode = string.Empty }
        };

        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Id == 1);
        Assert.Contains(result, x => x.Id == 2);
    }

    [Fact]
    public async Task Must_FilterBy_ComplexType_Address_City()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_ComplexType_Address_City));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith"), MainAddress = new Address { Street = "1st Ave", City = "NYC", State = "NY", PostalCode = "10001" } },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones"), MainAddress = new Address { Street = "2nd St", City = "LA", State = "CA", PostalCode = "90001" } },
            new User { Id = 3, Email = new Email("alex@site.com"), Name = new PersonName("Alex", "C", "Doe"), MainAddress = new Address { Street = "3rd Blvd", City = "NYC", State = "NY", PostalCode = "10002" } }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: filter by City = "NYC"
        var filter = new UserFilter { Address = new Address { City = "NYC", Street = string.Empty, State = string.Empty, PostalCode = string.Empty } };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Id == 1);
        Assert.Contains(result, x => x.Id == 3);
    }

    [Fact]
    public async Task Must_FilterBy_ComplexType_Address_Street()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_ComplexType_Address_Street));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith"), MainAddress = new Address { Street = "Main St", City = "NYC", State = "NY", PostalCode = "10001" } },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones"), MainAddress = new Address { Street = "Second St", City = "LA", State = "CA", PostalCode = "90001" } }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: filter by Street = "Second St"
        var filter = new UserFilter { Address = new Address { Street = "Second St", City = string.Empty, State = string.Empty, PostalCode = string.Empty } };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Single(result);
        Assert.Equal(2, result.Single().Id);
    }

    [Fact]
    public async Task Must_FilterBy_ComplexType_Address_State()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_ComplexType_Address_State));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith"), MainAddress = new Address { Street = "Main St", City = "NYC", State = "NY", PostalCode = "10001" } },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones"), MainAddress = new Address { Street = "Second St", City = "LA", State = "CA", PostalCode = "90001" } },
            new User { Id = 3, Email = new Email("alex@site.com"), Name = new PersonName("Alex", "C", "Doe"), MainAddress = new Address { Street = "Third St", City = "LA", State = "CA", PostalCode = "90002" } }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: filter by State = "CA"
        var filter = new UserFilter { Address = new Address { State = "CA", Street = string.Empty, City = string.Empty, PostalCode = string.Empty } };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Id == 2);
        Assert.Contains(result, x => x.Id == 3);
    }

    [Fact]
    public async Task Must_FilterBy_ComplexType_Address_PostalCode()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_ComplexType_Address_PostalCode));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith"), MainAddress = new Address { Street = "Main St", City = "NYC", State = "NY", PostalCode = "10001" } },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones"), MainAddress = new Address { Street = "Second St", City = "LA", State = "CA", PostalCode = "90001" } }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: filter by PostalCode = "90001"
        var filter = new UserFilter { Address = new Address { PostalCode = "90001", Street = string.Empty, City = string.Empty, State = string.Empty } };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Single(result);
        Assert.Equal(2, result.Single().Id);
    }

    [Fact]
    public async Task Must_FilterBy_ComplexType_Address_MultipleFields_WithAnd()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_ComplexType_Address_MultipleFields_WithAnd));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith"), MainAddress = new Address { Street = "Main St", City = "NYC", State = "NY", PostalCode = "10001" } },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones"), MainAddress = new Address { Street = "Main St", City = "LA", State = "CA", PostalCode = "90001" } },
            new User { Id = 3, Email = new Email("alex@site.com"), Name = new PersonName("Alex", "C", "Doe"), MainAddress = new Address { Street = "Main St", City = "LA", State = "CA", PostalCode = "90002" } }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: Street = "Main St" AND City = "LA" AND State = "CA"
        var filter = new UserFilter { Address = new Address { Street = "Main St", City = "LA", State = "CA", PostalCode = string.Empty } };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Id == 2);
        Assert.Contains(result, x => x.Id == 3);
    }

    [Fact]
    public async Task Must_NotApply_Filter_When_Address_IsNull()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_NotApply_Filter_When_Address_IsNull));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith"), MainAddress = new Address { Street = "Main St", City = "NYC", State = "NY", PostalCode = "10001" } },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones"), MainAddress = new Address { Street = "Second St", City = "LA", State = "CA", PostalCode = "90001" } }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act
        var filter = new UserFilter { Address = null };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Must_NotApply_Filter_When_Address_AllFields_Empty()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_NotApply_Filter_When_Address_AllFields_Empty));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith"), MainAddress = new Address { Street = "Main St", City = "NYC", State = "NY", PostalCode = "10001" } },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones"), MainAddress = new Address { Street = "Second St", City = "LA", State = "CA", PostalCode = "90001" } }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: all fields empty -> no WHERE applied
        var filter = new UserFilter { Address = new Address { Street = string.Empty, City = string.Empty, State = string.Empty, PostalCode = string.Empty } };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
    }
    [Fact]
    public async Task Must_FilterBy_ComplexTypePropertyPath_EmailValue()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_ComplexTypePropertyPath_EmailValue));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith") },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones") }
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
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith") },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones") }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act
        var filter = new UserFilter { Email = null };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Must_FilterBy_ComplexType_Name_FirstName()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_ComplexType_Name_FirstName));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith") },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones") },
            new User { Id = 3, Email = new Email("alex@site.com"), Name = new PersonName("Alex", "C", "Doe") }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: filtra por FirstName = "Mary"
        var filter = new UserFilter { Name = new PersonName("Mary", string.Empty, string.Empty) };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Single(result);
        Assert.Equal(2, result.Single().Id);
    }

    [Fact]
    public async Task Must_FilterBy_ComplexType_Name_MiddleName()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_ComplexType_Name_MiddleName));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith") },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones") },
            new User { Id = 3, Email = new Email("alex@site.com"), Name = new PersonName("Alex", "C", "Doe") }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: filtra por MiddleName = "A"
        var filter = new UserFilter { Name = new PersonName(string.Empty, "A", string.Empty) };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Single(result);
        Assert.Equal(1, result.Single().Id);
    }

    [Fact]
    public async Task Must_FilterBy_ComplexType_Name_LastName()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_ComplexType_Name_LastName));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith") },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones") },
            new User { Id = 3, Email = new Email("alex@site.com"), Name = new PersonName("Alex", "C", "Doe") }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: filtra por LastName = "Jones"
        var filter = new UserFilter { Name = new PersonName(string.Empty, string.Empty, "Jones") };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Single(result);
        Assert.Equal(2, result.Single().Id);
    }

    [Fact]
    public async Task Must_FilterBy_ComplexType_Name_MultipleFields_WithAnd()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_ComplexType_Name_MultipleFields_WithAnd));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith") },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones") },
            new User { Id = 3, Email = new Email("alex@site.com"), Name = new PersonName("Alex", "C", "Jones") }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: combina FirstName = "Mary" E LastName = "Jones" (AND)
        var filter = new UserFilter { Name = new PersonName("Mary", string.Empty, "Jones") };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Single(result);
        Assert.Equal(2, result.Single().Id);
    }

    [Fact]
    public async Task Must_NotApply_Filter_When_Name_IsNull()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_NotApply_Filter_When_Name_IsNull));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith") },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones") }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act
        var filter = new UserFilter { Name = null };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Must_NotApply_Filter_When_Name_AllFields_Empty()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_NotApply_Filter_When_Name_AllFields_Empty));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        ctx.AddRange(
            new User { Id = 1, Email = new Email("john@site.com"), Name = new PersonName("John", "A", "Smith") },
            new User { Id = 2, Email = new Email("mary@site.com"), Name = new PersonName("Mary", "B", "Jones") }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<User>>();

        // act: todos campos vazios -> nenhum WHERE aplicado
        var filter = new UserFilter { Name = new PersonName(string.Empty, string.Empty, string.Empty) };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
    }
}


[ComplexFilter]
internal readonly record struct Email
{
    public string Value { get; }

    public Email(string value)
    {
        Value = value;
    }
}

internal readonly record struct PersonName
{
    public string FirstName { get; }
    public string MiddleName { get; }
    public string LastName { get; }
    
    public PersonName(string firstName, string middleName, string lastName)
    {
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
    }

    public override string ToString()
    {
        return $"{FirstName} {MiddleName} {LastName}";
    }
}

internal class User
{
    public int Id { get; set; }

    public Email Email { get; set; }
    
    public PersonName Name { get; set; }

    public Address? MainAddress { get; set; }
}

[ComplexFilter]
internal class Address
{
    public string Street { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public string PostalCode { get; set; }
}

internal class UserFilter
{
    [Criterion("Email.Value")]
    public string? Email { get; set; }

    [ComplexFilter]
    public PersonName? Name { get; set; }

    [Criterion("MainAddress")]
    public Address? Address { get; set; }
}

internal class UserFilterByName
{
    public PersonNameFilter Name { get; set; }
}

[ComplexFilter]
internal struct PersonNameFilter
{
    [Criterion("FirstNameOrMiddleNameOrLastName")]
    public string? Value { get; set; }
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
            b.ComplexProperty(u => u.Name, name =>
            {
                name.Property(n => n.FirstName).HasColumnName("FirstName");
                name.Property(n => n.MiddleName).HasColumnName("MiddleName");
                name.Property(n => n.LastName).HasColumnName("LastName");
            });
            b.OwnsOne(u => u.MainAddress, address =>
            {
                address.Property(a => a.Street).HasColumnName("Street");
                address.Property(a => a.City).HasColumnName("City");
                address.Property(a => a.State).HasColumnName("State");
                address.Property(a => a.PostalCode).HasColumnName("PostalCode");
            });

            base.OnModelCreating(modelBuilder);
        });
    }
}