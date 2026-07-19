namespace RoyalCode.SmartSearch.Demo.Tests;

/// <summary>
/// Shares a single <see cref="DemoApplicationFactory"/> across all test classes. SmartSearch registers selectors
/// and named sortings in process-global static maps, so the host (and therefore <c>AddDemoSearches</c>) must be
/// configured only once per process.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DemoCollection : ICollectionFixture<DemoApplicationFactory>
{
    public const string Name = "demo";
}
