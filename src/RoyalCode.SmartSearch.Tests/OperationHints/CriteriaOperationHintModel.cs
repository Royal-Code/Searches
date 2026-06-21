using Microsoft.EntityFrameworkCore;

namespace RoyalCode.SmartSearch.Tests.OperationHints;

/// <summary>Hints used by the Phase 1 criteria/include integration tests.</summary>
public enum SampleHints
{
    IncludeSingle,
    IncludeMultiple,
}

/// <summary>Navigation target for both the reference and the collection relations of <see cref="RootEntity"/>.</summary>
public class SampleEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}

/// <summary>Aggregate root with a single (reference) and a multiple (collection) navigation to <see cref="SampleEntity"/>.</summary>
public class RootEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public SampleEntity? SingleRelation { get; set; }

    public ICollection<SampleEntity>? MultipleRelation { get; set; }
}

/// <summary>Projection target used to assert that <c>Select&lt;TDto&gt;</c> ignores entity includes.</summary>
public class RootDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}

/// <summary>
///     Test context. Both navigations target <see cref="SampleEntity"/>; relationships are configured by convention
///     (mirroring the OperationHint test context).
/// </summary>
public class HintsDbContext : DbContext
{
    public HintsDbContext(DbContextOptions<HintsDbContext> options) : base(options) { }

    public DbSet<RootEntity> Roots { get; set; } = null!;

    public DbSet<SampleEntity> Samples { get; set; } = null!;
}
