using System.Net;
using FluentAssertions;

namespace RoyalCode.SmartSearch.Demo.Tests;

[Collection(DemoCollection.Name)]
public sealed class OrdersEndpointsTests(DemoApplicationFactory factory)
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Registered_Selector_Computes_Total_And_CustomerName()
    {
        var response = await client.GetAsync("/api/orders?status=Paid&orderby=createdAt-desc");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"customerName\"").And.Contain("\"total\"");
        body.Should().Contain("\"count\":11"); // 2 seeded Paid orders + 9 generated
    }

    [Fact]
    public async Task Date_Range_Filter()
    {
        var response = await client.GetAsync("/api/orders?createdAtFrom=2026-02-01&createdAtTo=2026-05-31");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"count\":3");
    }

    [Fact]
    public async Task Negation_Excludes_Status()
    {
        var response = await client.GetAsync("/api/orders?notStatus=Cancelled");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"count\":29"); // 30 orders minus the single Cancelled one
        body.Should().NotContain("ORD-1004"); // the only Cancelled order
    }

    [Fact]
    public async Task In_Operator_Over_Statuses()
    {
        var response = await client.GetAsync("/manual/orders/by-status?statuses=Paid&statuses=Shipped");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("ORD-1001").And.Contain("ORD-1003").And.Contain("ORD-1005");
        body.Should().NotContain("ORD-1002"); // Pending
        body.Should().NotContain("ORD-1004"); // Cancelled
    }

    [Fact]
    public async Task Exists_True_And_False()
    {
        (await client.GetStringAsync("/manual/orders/exists?number=1002")).Should().Contain("\"exists\":true");
        (await client.GetStringAsync("/manual/orders/exists?number=9999")).Should().Contain("\"exists\":false");
    }

    [Fact]
    public async Task Single_By_Number_Loads_Navigations_Via_Hints()
    {
        var response = await client.GetAsync("/manual/orders/by-number/ORD-1001");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        // Hints WithCustomer + WithItems load the navigations...
        body.Should().Contain("\"customer\"").And.Contain("Maria Silva");
        body.Should().Contain("\"items\"");
        // ...but the un-hinted Product navigation stays null.
        body.Should().Contain("\"product\":null");
    }

    [Fact]
    public async Task FirstOrDefault_Unknown_Id_Returns_SmartProblems_404()
    {
        var response = await client.GetAsync("/manual/orders/999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // The manual endpoint returns a SmartProblems NotFound converted to RFC-9457 ProblemDetails.
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"status\":404").And.Contain("was not found");
    }

    [Fact]
    public async Task Single_Unknown_Number_Returns_SmartProblems_404()
    {
        var response = await client.GetAsync("/manual/orders/by-number/NOPE");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"status\":404").And.Contain("NOPE");
    }

    [Fact]
    public async Task Invalid_OrderBy_Returns_400_ProblemDetails()
    {
        var response = await client.GetAsync("/api/orders?orderby=bogusField");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("bogusField").And.Contain("Order");
    }

    [Fact]
    public async Task Manual_Invalid_OrderBy_Returns_SmartProblems_400()
    {
        var response = await client.GetAsync("/manual/orders?orderby=bogusField");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // The manual endpoint returns Problems.InvalidParameter as ProblemDetails (400).
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"status\":400").And.Contain("bogusField");
    }

    [Fact]
    public async Task Manual_And_Mapped_Same_Query_Return_Identical_Results()
    {
        const string query = "?status=Paid&orderby=createdAt-desc";

        var mapped = await client.GetStringAsync("/api/orders" + query);
        var manual = await client.GetStringAsync("/manual/orders" + query);

        manual.Should().Be(mapped);
    }
}
