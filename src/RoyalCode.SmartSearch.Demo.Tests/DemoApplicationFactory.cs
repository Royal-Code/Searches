using Microsoft.AspNetCore.Mvc.Testing;

namespace RoyalCode.SmartSearch.Demo.Tests;

/// <summary>
/// Boots the demo host in-memory (its own SQLite in-memory database, seeded per factory instance) so the
/// endpoints can be driven over HTTP with <see cref="WebApplicationFactory{TEntryPoint}"/>.
/// </summary>
public sealed class DemoApplicationFactory : WebApplicationFactory<Program>;
