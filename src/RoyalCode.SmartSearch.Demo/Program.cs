using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RoyalCode.SmartSearch.Demo.Data;
using RoyalCode.SmartSearch.Demo.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// A single in-memory SQLite connection kept open for the host lifetime: the database is created empty per run
// and stays alive while the connection is open (DF8).
var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();
builder.Services.AddSingleton(connection);
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));

// SmartSearch: ICriteria<T> services, selectors, named sortings, native Like operator and operation hints.
builder.Services.AddDemoSearches();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

// Create the schema on the shared connection and apply the deterministic seed.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DemoSeeder.Seed(db);
}

app.MapOpenApi();

// Scalar API reference UI (Swagger-like) served at /scalar, backed by the OpenAPI document.
app.MapScalarApiReference(options => options.WithTitle("RoyalCode.SmartSearch.Demo"));

app.MapManualSearchEndpoints();
app.MapMappedSearchEndpoints();

// Home page: the Scalar API reference.
app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

app.Run();

// Exposed so the tests project can drive the app with WebApplicationFactory<Program>.
public partial class Program;
