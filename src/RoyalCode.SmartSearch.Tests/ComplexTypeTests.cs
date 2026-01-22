using RoyalCode.SmartSearch.Linq.Filtering;

namespace RoyalCode.SmartSearch.Tests;

public class ComplexTypeTests
{
    [Fact]
    public void HasAttribute_WhenComplexFilter_ShouldBeTrue()
    {
        // Arrange
        var type = typeof(Email);

        // Act
        var hasAttribute = ExpressionGenerator.HasAttribute(type, typeof(ComplexFilterAttribute<>), out var attr);

        // Assert
        Assert.True(hasAttribute);

        var genericType = attr!.GetType().GetGenericArguments()[0];
        Assert.Equal(typeof(string), genericType);
    }

}


[ComplexFilter<string>(nameof(Value))]
file readonly record struct Email
{
    public string Value { get; }

    public Email(string value)
    {
        Value = value;
    }
}

[ComplexFilter<string>(nameof(FirstName))]
file readonly record struct PersonName
{
    public string FirstName { get; }
    public string LastName { get; }
    public PersonName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}