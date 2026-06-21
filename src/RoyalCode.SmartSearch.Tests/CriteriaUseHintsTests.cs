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
///     Integration tests for Phase 2: per-query hints declared via <see cref="ICriteria{TEntity}.UseHints"/>.
/// </para>
/// <para>
///     Local hints must drive EF includes on entity-materializing terminals, must be isolated to the criteria that
///     declared them (never leaking to sibling criterias in the same scope), must union with ambient hints, must NOT
///     affect <c>Exists</c> or <c>Select&lt;TDto&gt;</c>, and must be a safe no-op when no handler/registry exists.
/// </para>
/// </summary>
public class CriteriaUseHintsTests
{
    private readonly List<string> sqlLog = [];

    private IServiceProvider CreateProvider(bool withOperationHints)
    {
        var services = new ServiceCollection();

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
    public void UseHints_Should_Apply_Include_On_Collect()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = criteria.UseHints(SampleHints.IncludeSingle).Collect();

        result.Should().ContainSingle();
        result[0].SingleRelation.Should().NotBeNull();
        result[0].MultipleRelation.Should().BeNull();
    }

    [Fact]
    public void UseHints_Should_Apply_Include_On_FirstOrDefault()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = criteria.UseHints(SampleHints.IncludeSingle).FirstOrDefault();

        result.Should().NotBeNull();
        result!.SingleRelation.Should().NotBeNull();
    }

    [Fact]
    public void UseHints_Should_Apply_Include_On_Single()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = criteria.UseHints(SampleHints.IncludeSingle).Single();

        result.SingleRelation.Should().NotBeNull();
    }

    [Fact]
    public void UseHints_Multiple_Values_Should_Union_Includes()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = criteria
            .UseHints(SampleHints.IncludeSingle, SampleHints.IncludeMultiple)
            .Collect();

        result.Should().ContainSingle();
        result[0].SingleRelation.Should().NotBeNull();
        result[0].MultipleRelation.Should().NotBeNull().And.HaveCount(2);
    }

    [Fact]
    public void UseHints_With_Null_Hints_Should_Throw()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var act = () => criteria.UseHints<SampleHints>(null!);

        var exception = act.Should().Throw<ArgumentNullException>().Which;
        exception.ParamName.Should().Be("hints");
    }

    [Fact]
    public void UseHints_With_Empty_Hints_Should_Throw()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var act = () => criteria.UseHints<SampleHints>();

        var exception = act.Should().Throw<ArgumentException>().Which;
        exception.ParamName.Should().Be("hints");
    }

    [Fact]
    public void UseHints_Is_Isolated_Between_Criterias_In_Same_Scope()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();

        var criteriaWithHint = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();
        var criteriaWithoutHint = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        sqlLog.Clear();
        criteriaWithHint.UseHints(SampleHints.IncludeSingle).Collect();
        LoggedJoin().Should().BeTrue("the criteria that declared UseHints must include");

        sqlLog.Clear();
        criteriaWithoutHint.Collect();
        LoggedJoin().Should().BeFalse("a local UseHint must not leak to a sibling criteria in the same scope");
    }

    [Fact]
    public void UseHints_Combined_With_Ambient_Hint_Unions_Includes()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();

        // ambient hint in the container...
        scope.ServiceProvider.GetRequiredService<IHintsContainer>().AddHint(SampleHints.IncludeMultiple);
        // ...combined with a per-query hint
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        var result = criteria.UseHints(SampleHints.IncludeSingle).Collect();

        result.Should().ContainSingle();
        result[0].SingleRelation.Should().NotBeNull("from the per-query UseHints");
        result[0].MultipleRelation.Should().NotBeNull("from the ambient hint").And.HaveCount(2);
    }

    [Fact]
    public void UseHints_Should_Not_Affect_Exists()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        sqlLog.Clear();
        var exists = criteria.UseHints(SampleHints.IncludeSingle).Exists();

        exists.Should().BeTrue();
        LoggedJoin().Should().BeFalse("Exists must not apply per-query includes");
    }

    [Fact]
    public void UseHints_Should_Not_Affect_Select_Dto()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        sqlLog.Clear();
        var dto = criteria
            .UseHints(SampleHints.IncludeSingle)
            .Select(r => new RootDto { Id = r.Id, Name = r.Name })
            .FirstOrDefault();

        dto.Should().NotBeNull();
        dto!.Name.Should().Be("root");
        LoggedJoin().Should().BeFalse("per-query includes must be ignored under a Select<TDto> projection");
    }

    [Fact]
    public void UseHints_Without_OperationHint_Registered_Is_Safe_NoOp()
    {
        var provider = CreateProvider(withOperationHints: false);
        using var scope = provider.CreateScope();
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        // no registry available: the hint is silently ignored, no exception
        var result = criteria.UseHints(SampleHints.IncludeSingle).Collect();

        result.Should().ContainSingle();
        result[0].SingleRelation.Should().BeNull();
    }

    [Fact]
    public void UseHints_With_Unregistered_Hint_Is_Safe_NoOp()
    {
        var provider = CreateProvider(withOperationHints: true);
        using var scope = provider.CreateScope();
        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<RootEntity>>();

        // OperationHint is registered, but no handler exists for this enum type: no-op, no exception
        var result = criteria.UseHints(UnregisteredHints.Something).Collect();

        result.Should().ContainSingle();
        result[0].SingleRelation.Should().BeNull();
    }
}
