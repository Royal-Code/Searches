using FluentAssertions;
using RoyalCode.SmartSearch.Abstractions;
using System.ComponentModel;
using System.Text.Json;

namespace RoyalCode.SmartSearch.Tests;

public class SortingTests
{
    private readonly static JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Deserialize_ResultList_With_Sorting_MustBeEquivalent()
    {
        // arrange
        var expected = new ResultList<TestModel>
        {
            Page = 1,
            Count = 1,
            ItemsPerPage = 1,
            Pages = 1,
            Sortings =
            [
                new Sorting
                {
                    OrderBy = "Name",
                    Direction = ListSortDirection.Ascending
                },
                new Sorting
                {
                    OrderBy = "Id",
                    Direction = ListSortDirection.Descending
                }
            ],
            Items =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Mateus"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Marcos"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Lucas"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "João"
                }
            ]
        };

        var json = JsonSerializer.Serialize(expected, jsonOptions);

        // act
        var result = JsonSerializer.Deserialize<ResultList<TestModel>>(json, jsonOptions);

        // assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("Name asc", true, "Name", ListSortDirection.Ascending)]
    [InlineData("Name desc", true, "Name", ListSortDirection.Descending)]
    [InlineData("Name", true, "Name", ListSortDirection.Ascending)]
    [InlineData("Name-asc", true, "Name", ListSortDirection.Ascending)]
    [InlineData("Name-desc", true, "Name", ListSortDirection.Descending)]
    [InlineData(null, false, null, ListSortDirection.Ascending)]
    [InlineData("", false, null, ListSortDirection.Ascending)]
    [InlineData(" ", false, null, ListSortDirection.Ascending)]
    public void TryParse_NotJsonStrings(
        string? orderby,
        bool expectedResult,
        string? expectedOrderBy,
        ListSortDirection expectedDirection)
    {
        // arrange
        // act
        var result = Sorting.TryParse(orderby, out var sorting);

        // assert
        result.Should().Be(expectedResult);
        if (expectedResult)
        {
            sorting.Direction.Should().Be(expectedDirection);
            sorting.OrderBy.Should().Be(expectedOrderBy);
        }
    }
}

public class TestModel
{
    public Guid Id { get; set; }

    public string? Name { get; set; }
}