
using RoyalCode.Searches.Linq.Selector;

namespace RoyalCode.Persistence.Tests.Selectors;

public class SelectorExpressionGeneratorTests
{
    [Fact]
    public void Generate_Must_GenerateExpression_For_SameProperties()
    {
        // Arrange
        var generator = new DefaultSelectorExpressionGenerator();

        // Act
        var expression = generator.Generate<SimpleEntity, SimpleDto>();

        // Assert
        Assert.NotNull(expression);

        // Arrange
        var entity = new SimpleEntity { Id = 1, Name = "Name" };

        // Act
        var dto = expression!.Compile()(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Id, dto!.Id);
        Assert.Equal(entity.Name, dto.Name);
    }

    [Fact]
    public void Generate_Must_GenerateExpression_For_NavegationProperties()
    {
        // Arrange
        var generator = new DefaultSelectorExpressionGenerator();

        // Act
        var expression = generator.Generate<ComplexEntity, DtoForComplex>();

        // Assert
        Assert.NotNull(expression);

        // Arrange
        var entity = new ComplexEntity()
        {
            Name = "Name",
            Complex = new ComplexType()
            {
                Value = 1
            }
        };

        // Act
        var dto = expression!.Compile()(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Name, dto.Name);
        Assert.Equal(entity.Complex.Value, dto.ComplexValue);
    }

    [Fact]
    public void Generate_Must_GenerateExpression_For_NullableProperties()
    {
        // Arrange
        var generator = new DefaultSelectorExpressionGenerator();

        // Act
        var expression = generator.Generate<EntityWithNullable, DtoWithoutNullable>();

        // Assert
        Assert.NotNull(expression);

        // Arrange
        var entity = new EntityWithNullable() { Id = 1 };

        // Act
        var dto = expression!.Compile()(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Id, dto!.Id);

        var q = new List<EntityWithNullable>().AsQueryable();
        q.Select(e => new DtoWithoutNullable()
        {
            Id = e.Id ?? default,
        });
    }

    [Fact]
    public void Generate_Must_GenerateExpression_For_Different_Enums()
    {
        // Arrange
        var generator = new DefaultSelectorExpressionGenerator();

        // Act
        var expression = generator.Generate<EntityWithEnum, DtoWithEnum>();

        // Assert
        Assert.NotNull(expression);

        // Arrange
        var entity = new EntityWithEnum() { Id = 1, Enum = DomainValues.Value1 };

        // Act
        var dto = expression!.Compile()(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Id, dto!.Id);
        Assert.Equal((int)entity.Enum, (int)dto.Enum);
    }

    [Fact]
    public void Generate_Must_GenerateExpression_For_SubSelect()
    {
        // Arrange
        var generator = new DefaultSelectorExpressionGenerator();

        // Act
        var expression = generator.Generate<EntityForSubSelect, DtoForSubSelect>();

        // Assert
        Assert.NotNull(expression);

        // Arrange
        var entity = new EntityForSubSelect()
        {
            Id = 1,
            SubValue = new EntitySubType()
            {
                Id = 2,
                Name = "Name"
            }
        };

        // Act
        var dto = expression!.Compile()(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Id, dto!.Id);
        Assert.Equal(entity.SubValue.Id, dto.SubValue.Id);
        Assert.Equal(entity.SubValue.Name, dto.SubValue.Name);
    }

    [Fact]
    public void Generate_Must_GenerateExpression_For_MultiLevelSubSelect()
    {
        // Arrange
        var generator = new DefaultSelectorExpressionGenerator();

        // Act
        var expression = generator.Generate<EntityForMultiLevelSubSelect, DtoForMultiLevelSubSelect>();

        // Assert
        Assert.NotNull(expression);

        // Arrange
        var entity = new EntityForMultiLevelSubSelect()
        {
            Id = 1,
            Value = new EntityForSubSelect()
            {
                Id = 2,
                SubValue = new EntitySubType()
                {
                    Id = 3,
                    Name = "Name"
                }
            }
        };

        // Act
        var dto = expression!.Compile()(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Id, dto!.Id);
        Assert.Equal(entity.Value.Id, dto.Value.Id);
        Assert.Equal(entity.Value.SubValue.Id, dto.Value.SubValue.Id);
        Assert.Equal(entity.Value.SubValue.Name, dto.Value.SubValue.Name);
    }

    [Fact]
    public void Generate_Must_GenerateExpression_For_CollectionOfInt()
    {
        // Arrange
        var generator = new DefaultSelectorExpressionGenerator();

        // Act
        var expression = generator.Generate<EntityWithCollectionOfInt, DtoWithCollectionOfInt>();

        // Assert
        Assert.NotNull(expression);

        // Arrange
        var entity = new EntityWithCollectionOfInt()
        {
            Id = 1,
            Values = new List<int>() { 1, 2, 3 }
        };

        // Act
        var dto = expression!.Compile()(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Values, dto.Values);
    }

    [Fact]
    public void Generate_Must_GenerateExpression_For_CollectionOfItem()
    {
        // Arrange
        var generator = new DefaultSelectorExpressionGenerator();

        // Act
        var expression = generator.Generate<EntityWithCollectionOfItem, DtoWithCollectionOfItem>();

        // Assert
        Assert.NotNull(expression);

        // Arrange
        var entity = new EntityWithCollectionOfItem()
        {
            Id = 1,
            Values = new List<EntityItem>()
            {
                new EntityItem() { Id = 1, Name = "Name1" },
                new EntityItem() { Id = 2, Name = "Name2" },
                new EntityItem() { Id = 3, Name = "Name3" }
            }
        };

        // Act
        var dto = expression!.Compile()(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.Values.Select(e => e.Id), dto.Values.Select(e => e.Id));
        Assert.Equal(entity.Values.Select(e => e.Name), dto.Values.Select(e => e.Name));
    }

    [Fact]
    public void Generate_Must_GenerateExpression_For_SubTypesAndCollection()
    {
        // Arrange
        var generator = new DefaultSelectorExpressionGenerator();

        // Act
        var expression = generator.Generate<EntityWithSubSelectsAndCollections, DtoWithSubSelectsAndCollections>();

        // Assert
        Assert.NotNull(expression);

        // Arrange
        var entity = new EntityWithSubSelectsAndCollections()
        {
            Id = 1,
            SubValue = new EntitySubTypeWithCollection()
            {
                Id = 2,
                CollectionOfSubItems = new List<EntityItemWithSubType>()
                {
                    new EntityItemWithSubType() 
                    { 
                        Id = 3,
                        PropertyWithInt = new EntityWithCollectionOfInt() { Id = 4, Values = new List<int>() { 1, 2, 3 } },
                        PropertyWithItem = new EntityWithCollectionOfItem()
                        {
                            Id = 5, 
                            Values = new List<EntityItem>()
                            {
                                new EntityItem() { Id = 6, Name = "Name1" },
                                new EntityItem() { Id = 7, Name = "Name2" },
                                new EntityItem() { Id = 8, Name = "Name3" }
                            }
                        },
                    },
                    new EntityItemWithSubType()
                    {
                        Id = 9,
                        PropertyWithInt = new EntityWithCollectionOfInt() { Id = 10, Values = new List<int>() { 4, 5, 6 } },
                        PropertyWithItem = new EntityWithCollectionOfItem()
                        {
                            Id = 11, 
                            Values = new List<EntityItem>()
                            {
                                new EntityItem() { Id = 12, Name = "Name4" },
                                new EntityItem() { Id = 13, Name = "Name5" },
                                new EntityItem() { Id = 14, Name = "Name6" }
                            }
                        },
                    },
                }
            },
            SubTypes = new List<EntitySubType>()
            {
                new EntitySubType() { Id = 15, Name = "Name1" },
                new EntitySubType() { Id = 16, Name = "Name2" },
                new EntitySubType() { Id = 17, Name = "Name3" }
            }
        };

        // Act
        var dto = expression!.Compile()(entity);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(entity.SubValue.Id, dto.SubValue.Id);
        Assert.Equal(entity.SubValue.CollectionOfSubItems.Select(e => e.Id), dto.SubValue.CollectionOfSubItems.Select(e => e.Id));
        Assert.Equal(entity.SubValue.CollectionOfSubItems.Select(e => e.PropertyWithInt.Values), dto.SubValue.CollectionOfSubItems.Select(e => e.PropertyWithInt.Values));
        Assert.Equal(entity.SubValue.CollectionOfSubItems.Select(e => e.PropertyWithItem.Id), dto.SubValue.CollectionOfSubItems.Select(e => e.PropertyWithItem.Id));
        Assert.Equal(entity.SubValue.CollectionOfSubItems.Select(e => e.PropertyWithItem.Values.Select(v => v.Id)), dto.SubValue.CollectionOfSubItems.Select(e => e.PropertyWithItem.Values.Select(v => v.Id)));
        Assert.Equal(entity.SubValue.CollectionOfSubItems.Select(e => e.PropertyWithItem.Values.Select(v => v.Name)), dto.SubValue.CollectionOfSubItems.Select(e => e.PropertyWithItem.Values.Select(v => v.Name)));
        Assert.Equal(entity.SubTypes.Select(e => e.Id), dto.SubTypes.Select(e => e.Id));
        Assert.Equal(entity.SubTypes.Select(e => e.Name), dto.SubTypes.Select(e => e.Name));
    }
}

file class SimpleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

file class SimpleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

file class ComplexEntity
{
    public string Name { get; set; } = null!;

    public ComplexType Complex { get; set; } = null!;
}

file class ComplexType
{
    public int Value { get; set; }
}

file class DtoForComplex
{
    public string Name { get; set; } = null!;

    public int ComplexValue { get; set; }
}

file class EntityWithNullable
{
    public int? Id { get; set; }
}

file class DtoWithoutNullable
{
    public int Id { get; set; }
}

file class EntityWithEnum
{
    public int Id { get; set; }
    public DomainValues Enum { get; set; }
}

file enum DomainValues
{
    Value1,
    Value2
}

file class DtoWithEnum
{
    public int Id { get; set; }
    public DtosValues Enum { get; set; }
}

file enum DtosValues
{
    DtoValue1,
    DtoValue2
}

file class EntityForSubSelect
{
    public int Id { get; set; }

    public EntitySubType SubValue { get; set; } = null!;
}

file class EntitySubType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    // a nullable entity property and a non-nullable dto property
    public int? Money { get; set; }

    public string OtherProperty { get; set; } = null!;
}

file class DtoForSubSelect
{
    public int Id { get; set; }

    public DtoSubType SubValue { get; set; } = null!;
}

file class DtoSubType
{
    public int Id { get; set; }

    // a dto property with ? and a non-nullable entity property
    public string? Name { get; set; }

    public int Money { get; set; }
}

file class EntityForMultiLevelSubSelect
{
    public int Id { get; set; }

    // a nullable entity property and a non-nullable dto property
    public EntityForSubSelect? Value { get; set; }
}

file class DtoForMultiLevelSubSelect
{
    public int Id { get; set; }

    public DtoForSubSelect Value { get; set; } = null!;
}

file class EntityWithCollectionOfInt
{
    public int Id { get; set; }

    public ICollection<int> Values { get; set; } = null!;
}

file class DtoWithCollectionOfInt
{
    public IEnumerable<int> Values { get; set; } = null!;
}

file class EntityWithCollectionOfItem
{
    public int Id { get; set; }

    public ICollection<EntityItem> Values { get; set; } = null!;
}

file class EntityItem
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}

file class DtoWithCollectionOfItem
{
    public int Id { get; set; }

    public IEnumerable<DtoItem> Values { get; set; } = null!;
}

file class DtoItem
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}

file class EntityWithSubSelectsAndCollections
{
    public int Id { get; set; }

    public EntitySubTypeWithCollection SubValue { get; set; } = null!;

    public ICollection<EntitySubType> SubTypes { get; set; } = null!;
}

file class EntitySubTypeWithCollection
{
    public int Id { get; set; }

    public ICollection<EntityItemWithSubType> CollectionOfSubItems { get; set; } = null!;
}

file class EntityItemWithSubType
{
    public int Id { get; set; }

    public EntityWithCollectionOfItem PropertyWithItem { get; set; } = null!;

    public EntityWithCollectionOfInt PropertyWithInt { get; set; } = null!;
}

file class DtoWithSubSelectsAndCollections
{
    public int Id { get; set; }

    public DtoSubTypeWithCollection SubValue { get; set; } = null!;

    public IEnumerable<DtoSubType> SubTypes { get; set; } = null!;
}

file class DtoSubTypeWithCollection
{
    public int Id { get; set; }

    public IEnumerable<DtoItemWithSubType> CollectionOfSubItems { get; set; } = null!;
}

file class DtoItemWithSubType
{
    public int Id { get; set; }

    public DtoWithCollectionOfItem PropertyWithItem { get; set; } = null!;

    public DtoWithCollectionOfInt PropertyWithInt { get; set; } = null!;
}