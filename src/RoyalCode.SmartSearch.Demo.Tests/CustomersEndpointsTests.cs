using System.Net;
using FluentAssertions;

namespace RoyalCode.SmartSearch.Demo.Tests;

[Collection(DemoCollection.Name)]
public sealed class CustomersEndpointsTests(DemoApplicationFactory factory)
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Name_Contains_CaseInsensitive_Returns_Maria()
    {
        var response = await client.GetAsync("/api/customers?name=maria");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Maria Silva");
        body.Should().Contain("\"count\":1");
    }

    [Fact]
    public async Task NameOrEmail_Splits_Into_Disjunction()
    {
        var response = await client.GetAsync("/api/customers?nameOrEmail=mario");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Mario Souza");
    }

    [Fact]
    public async Task State_Filters_Owned_Address_By_Nested_Path()
    {
        var response = await client.GetAsync("/api/customers?state=NY");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"count\":2");
    }

    [Fact]
    public async Task ComplexFilter_Over_Owned_Address_By_City()
    {
        var response = await client.GetAsync("/manual/customers/by-address?city=NYC");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Maria Silva").And.Contain("Mario Souza");
        body.Should().NotContain("John Appleseed");
    }

    [Fact]
    public async Task Paging_Uses_Page_And_ItemsPerPage_Keys()
    {
        var response = await client.GetAsync("/api/customers?page=1&itemsPerPage=2");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"itemsPerPage\":2");
    }

    [Fact]
    public async Task No_Match_Returns_204()
    {
        var response = await client.GetAsync("/api/customers?name=nobody-here");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
