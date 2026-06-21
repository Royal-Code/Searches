using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoyalCode.OperationHint.Abstractions;
using RoyalCode.SmartSearch.Tests.OperationHints;

namespace RoyalCode.SmartSearch.Tests;

/// <summary>
/// <para>
///     Integration tests for Phase 1 of the "Include via Operation Hints" feature: ambient hints applied to the
///     EF <see cref="ICriteria{TEntity}"/> pipeline.
/// </para>
/// <para>
///     Hints declared in the scope's <see cref="IHintsContainer"/> must drive EF includes on the entity-materializing
///     terminals (<c>Collect</c>/<c>First</c>/<c>Single</c>), but must NOT affect <c>Exists</c> (an <c>Any</c>) nor
///     <c>Select&lt;TDto&gt;</c> (a projection). Without <c>OperationHint</c> registered, behavior is unchanged.
/// </para>
/// </summary>
public class CriteriaOperationHintTests
{
    private readonly List<string> sqlLog = [];

    private IServiceProvider CreateProvider(bool withOperationHints)
    {
        var services = new ServiceCollection();

        // a single shared in-memory connection kept open for the provider lifetime
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        services.AddSingleton(connection);

        services.AddDbContext<HintsDbContext>(builder => builder
            .UseSqlite(connection)
            .LogTo(sqlLog.Add, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information));

        services.AddEntityFrameworkSearches<HintsDbContext>(s => s.Add<RootEntity>());

        if (withOperationHints)
        {
            services.ConfigureOperationHints(registry =>
            {
                registry.AddIncludesHandler<RootEntity, SampleHints>((hint, includes) =>
                {
                    switch (hint)
                    {
                        case SampleHints.IncludeSingle:
                            includes.IncludeReference(e => e.SingleRelation!);
                            break;
                        case SampleHints.IncludeMultiple:
                            includes.IncludeCollection(e => e.MultipleRelation!);
                            break;
                    }
                });
            });
        }

        var provider = services.BuildServiceProvider();
        Seed(provider);
        return provider;
    }

    // Seeds one aggregate in its own scope so query scopes start with an empty change tracker
    // (otherwise navigation fixup could populate navigations without an actual include).
    private static void Seed(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HintsDbContext>();
        db.Database.EnsureCreated();

        db.Add(new RootEntity
        {
            Name = "root",
            SingleRelation = new SampleEntity { Name = "single" },
            MultipleRelation = [new SampleEntity { Name = "multi-1" }, new SampleEntity { Name = "multi-2" }],
        });

        db.SaveChanges();
    }

    private bool LoggedJoin() => sqlLog.Any(s => s.Contains("JOIN", StringComparison.OrdinalIgnoreCase));

    [Fact]
    public void Collect_Should_Apply_AmbientHint_Include_SingleRelation()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IHintsContainer>().AddHint(SampleHints.IncludeSingle);
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = criteria.Collect();

        result.Should().ContainSingle();
        result[0].SingleRelation.Should().NotBeNull();
        result[0].MultipleRelation.Should().BeNull();
    }

    [Fact]
    public void FirstOrDefault_Should_Apply_AmbientHint_Include()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IHintsContainer>().AddHint(SampleHints.IncludeSingle);
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = criteria.FirstOrDefault();

        result.Should().NotBeNull();
        result!.SingleRelation.Should().NotBeNull();
    }

    [Fact]
    public void Single_Should_Apply_AmbientHint_Include()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IHintsContainer>().AddHint(SampleHints.IncludeSingle);
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = criteria.Single();

        result.SingleRelation.Should().NotBeNull();
    }

    [Fact]
    public void MultipleHints_Should_Union_Includes()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(SampleHints.IncludeSingle);
        container.AddHint(SampleHints.IncludeMultiple);
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = criteria.Collect();

        result.Should().ContainSingle();
        result[0].SingleRelation.Should().NotBeNull();
        result[0].MultipleRelation.Should().NotBeNull().And.HaveCount(2);
    }

    [Fact]
    public void DbContextCriteria_Should_Apply_AmbientHint_Include()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IHintsContainer>().AddHint(SampleHints.IncludeSingle);
        var db = scope.ServiceProvider.GetRequiredService<HintsDbContext>();
        var criteria = db.Criteria<RootEntity>();

        var result = criteria.Collect();

        result.Should().ContainSingle();
        result[0].SingleRelation.Should().NotBeNull();
    }

    [Fact]
    public void DbContextCriteria_WithoutOperationHint_Registered_Query_Is_Unchanged()
    {
        var provider = CreateProvider(withOperationHints: false);
        using var scope = provider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<HintsDbContext>();
        var criteria = db.Criteria<RootEntity>();

        var result = criteria.Collect();

        result.Should().ContainSingle();
        result[0].SingleRelation.Should().BeNull("no OperationHint is registered, so no include is applied");
        result[0].MultipleRelation.Should().BeNull();
    }

    [Fact]
    public void AsSearch_ToList_Should_Apply_AmbientHint_Include_ForEntityResults()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IHintsContainer>().AddHint(SampleHints.IncludeSingle);
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = criteria.UsePages().AsSearch().ToList();

        result.Items.Should().ContainSingle();
        result.Items[0].SingleRelation.Should().NotBeNull();
    }

    [Fact]
    public async Task AsSearch_ToListAsync_Should_Apply_AmbientHint_Include_ForEntityResults()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IHintsContainer>().AddHint(SampleHints.IncludeSingle);
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = await criteria.UsePages().AsSearch().ToListAsync(CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].SingleRelation.Should().NotBeNull();
    }

    [Fact]
    public async Task AsSearch_ToAsyncListAsync_Should_Apply_AmbientHint_Include_ForEntityResults()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IHintsContainer>().AddHint(SampleHints.IncludeSingle);
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = await criteria.UsePages().AsSearch().ToAsyncListAsync(CancellationToken.None);
        var items = new List<RootEntity>();
        await foreach (var item in result.Items)
            items.Add(item);

        items.Should().ContainSingle();
        items[0].SingleRelation.Should().NotBeNull();
    }

    [Fact]
    public void Exists_Should_Not_Apply_Includes()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IHintsContainer>().AddHint(SampleHints.IncludeSingle);
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        // a materializing terminal applies the include (anchors the contrast)
        sqlLog.Clear();
        criteria.Collect();
        LoggedJoin().Should().BeTrue("Collect must honor the ambient include hint");

        // Exists is an Any(): the include would be wasted, so it must NOT be applied
        sqlLog.Clear();
        var exists = criteria.Exists();

        exists.Should().BeTrue();
        LoggedJoin().Should().BeFalse("Exists must not apply entity includes");
    }

    [Fact]
    public void Select_Dto_Should_Not_Apply_Includes()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IHintsContainer>().AddHint(SampleHints.IncludeSingle);
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        sqlLog.Clear();
        var dto = criteria
            .Select(r => new RootDto { Id = r.Id, Name = r.Name })
            .FirstOrDefault();

        dto.Should().NotBeNull();
        dto!.Name.Should().Be("root");
        LoggedJoin().Should().BeFalse("entity includes must be ignored under a Select<TDto> projection");
    }

    [Fact]
    public void Without_OperationHint_Registered_Query_Is_Unchanged()
    {
        var provider = CreateProvider(withOperationHints: false);
        using var scope = provider.CreateScope();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = criteria.Collect();

        result.Should().ContainSingle();
        result[0].SingleRelation.Should().BeNull("no OperationHint is registered, so no include is applied");
        result[0].MultipleRelation.Should().BeNull();
    }
}
