using System.Net;
using FluentAssertions;

namespace RoyalCode.SmartSearch.Demo.Tests;

[Collection(DemoCollection.Name)]
public sealed class ProductsEndpointsTests(DemoApplicationFactory factory)
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Equality_And_Numeric_Range()
    {
        var response = await client.GetAsync("/api/products?active=true&priceMin=50&priceMax=200");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Webcam");
        body.Should().NotContain("Monitor"); // 899.00 is out of range
        body.Should().NotContain("Headset"); // inactive
    }

    [Fact]
    public async Task Anchored_Like_With_Wrap_None()
    {
        // 'ABC%' is used as-is (no %value% wrapping): matches SKUs that start with ABC.
        var response = await client.GetAsync("/api/products?sku=ABC%25");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("ABC-001").And.Contain("ABC-002").And.Contain("ABC-003");
        body.Should().NotContain("XYZ-100");
    }

    [Fact]
    public async Task Disjunction_Matches_Name_Or_Sku()
    {
        var response = await client.GetAsync("/api/products?textInName=mo&textInSku=xyz");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        // Mouse & Monitor by name ("mo"); Headset by sku (XYZ-200).
        body.Should().Contain("Mouse").And.Contain("Monitor").And.Contain("Headset");
        body.Should().NotContain("Keyboard");
    }

    [Fact]
    public async Task First_Returns_Single_Entity()
    {
        var response = await client.GetAsync("/api/products/first?active=true");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().StartWith("{").And.Contain("\"sku\"");
    }
}
