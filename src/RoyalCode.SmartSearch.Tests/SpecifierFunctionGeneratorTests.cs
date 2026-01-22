using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.SmartSearch.Linq;
using RoyalCode.SmartSearch.Linq.Filtering;
using RoyalCode.SmartSearch.Linq.Services;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace RoyalCode.SmartSearch.Tests;

#pragma warning disable CA1859 // Use of concrete type

public class SpecifierFunctionGeneratorTests
{
    [Fact]
    public void Generate_Must_GenerateTheFilterFunction_For_SimpleModel()
    {
        // arrange
        var generator = new DefaultSpecifierFunctionGenerator();

        // act
        var function = generator.Generate<SimpleModel, SimpleFilter>();

        // assert
        Assert.NotNull(function);
    }

    [Fact]
    public void Generate_Must_GenerateTheFilterFunction_For_NullableProperties()
    {
        // arrange
        var generator = new DefaultSpecifierFunctionGenerator();

        // act
        var function = generator.Generate<SimpleModel, NullablePropertiesFilter>();

        // assert
        Assert.NotNull(function);
    }

    [Fact]
    public void Generate_Must_GenerateTheFilter_When_ConfiguredWithDbContext()
    {
        // arrange
        ServiceCollection services = new();

        services.AddDbContext<LocalDbContext>(builder => builder.UseInMemoryDatabase("test"));

        services.AddEntityFrameworkSearches<LocalDbContext>(s => s.Add<SimpleModel>());

        ServiceProvider provider = services.BuildServiceProvider();

        // act
        var search = provider.GetRequiredService<ICriteria<SimpleModel>>();

        // assert
        Assert.NotNull(search);

        // act
        search!.FilterBy(new SimpleFilter());
        var resultList = search.AsSearch().ToList();

        // assert
        Assert.NotNull(resultList);
    }

    [Fact]
    public void Generate_Must_GenerateTheFilter_For_ExtendedModels()
    {
        // arrange
        var generator = new DefaultSpecifierFunctionGenerator();

        // act
        var function = generator.Generate<ExtendedModel, ExtendedFilter>();

        // assert
        Assert.NotNull(function);
    }

    [Fact]
    public void GetMemberAccess_OnProperty_Must_ReturnTheMemberAccess_WithSameType()
    {
        // arrange
        var property = typeof(SimpleModel).GetProperty(nameof(SimpleModel.Guid))!;
        var parameter = Expression.Parameter(typeof(SimpleModel), "x");

        // act
        var memberAccess = ExpressionGenerator.GetMemberAccess(property, parameter);

        // assert
        Assert.NotNull(memberAccess);
        Assert.Equal(typeof(Guid), memberAccess.Type);
    }

    [Fact]
    public void GetMemberAccess_OnProperty_Must_ReturnTheMemberAccess_WithUnderlyingType()
    {
        // arrange
        var property = typeof(SimpleModel).GetProperty(nameof(SimpleModel.Date))!;
        var parameter = Expression.Parameter(typeof(SimpleModel), "x");

        // act
        var memberAccess = ExpressionGenerator.GetMemberAccess(property, parameter);

        // assert
        Assert.NotNull(memberAccess);
        Assert.Equal(typeof(DateTime), memberAccess.Type);
    }

    [Fact]
    public void GetMemberAccess_PropertySelection_Must_ReturnTheMemberAccess_WithSameType()
    {
        // arrange
        var property = typeof(SimpleModel).SelectProperty(nameof(SimpleModel.Guid))!;
        var parameter = Expression.Parameter(typeof(SimpleModel), "x");

        // act
        var memberAccess = ExpressionGenerator.GetMemberAccess(property, parameter);

        // assert
        Assert.NotNull(memberAccess);
        Assert.Equal(typeof(Guid), memberAccess.Type);
    }

    [Fact]
    public void GetMemberAccess_PropertySelection_Must_ReturnTheMemberAccess_WithUnderlyingType()
    {
        // arrange
        var property = typeof(SimpleModel).SelectProperty(nameof(SimpleModel.Date))!;
        var parameter = Expression.Parameter(typeof(SimpleModel), "x");

        // act
        var memberAccess = ExpressionGenerator.GetMemberAccess(property, parameter);

        // assert
        Assert.NotNull(memberAccess);
        Assert.Equal(typeof(DateTime), memberAccess.Type);
    }

    [Theory]
    [InlineData(CriterionOperator.Equal)]
    [InlineData(CriterionOperator.In)]
    [InlineData(CriterionOperator.LessThan)]
    [InlineData(CriterionOperator.LessThanOrEqual)]
    [InlineData(CriterionOperator.GreaterThan)]
    [InlineData(CriterionOperator.GreaterThanOrEqual)]
    [InlineData(CriterionOperator.Like)]
    public void DiscoveryCriterionOperator_Must_ReturnSameOperator_WhenOperatorIsNotAuto(CriterionOperator inOperator)
    {
        // arrange
        var criterion = new CriterionAttribute
        {
            Operator = inOperator
        };
        var property = typeof(SimpleModel).GetProperty(nameof(SimpleModel.Name))!;

        // act
        var @operator = ExpressionGenerator.DiscoveryCriterionOperator(criterion, property);

        // assert
        Assert.Equal(inOperator, @operator);
    }

    [Theory]
    [InlineData(nameof(MultiFilterTypes.Name), CriterionOperator.Like)]
    [InlineData(nameof(MultiFilterTypes.Age), CriterionOperator.Equal)]
    [InlineData(nameof(MultiFilterTypes.Date), CriterionOperator.Equal)]
    [InlineData(nameof(MultiFilterTypes.Tags), CriterionOperator.In)]
    [InlineData(nameof(MultiFilterTypes.Value), CriterionOperator.Equal)]
    public void DiscoveryCriterionOperator_Must_ReturnDefaultOperator_WhenOperatorIsAuto(
        string propertyName, CriterionOperator expectedOperator)
    {
        // arrange
        var criterion = new CriterionAttribute();
        var property = typeof(MultiFilterTypes).GetProperty(propertyName)!;

        // act
        var @operator = ExpressionGenerator.DiscoveryCriterionOperator(criterion, property);

        // assert
        Assert.Equal(expectedOperator, @operator);
    }

    [Theory]
    [InlineData(nameof(MultiFilterTypes.Name), true, ExpressionType.Call, "IsNullOrWhiteSpace")]
    [InlineData(nameof(MultiFilterTypes.Age), false, ExpressionType.MemberAccess, "HasValue")]
    [InlineData(nameof(MultiFilterTypes.Date), true, ExpressionType.Call, "IsBlank")]
    [InlineData(nameof(MultiFilterTypes.Tags), false, ExpressionType.Call, "Any")]
    [InlineData(nameof(MultiFilterTypes.Value), false, ExpressionType.GreaterThan, null)]
    [InlineData(nameof(MultiFilterTypes.Some), false, ExpressionType.NotEqual, null)]
    [InlineData(nameof(MultiFilterTypes.StructValues), true, ExpressionType.Call, "IsEmpty")]
    public void GetIfIsEmptyConstraintExpression_Must_WrapWithIfExpression(
        string propertyName, bool not, ExpressionType expectedType, string? name)
    {
        // arrange
        var assignExpression = Expression.Assign(Expression.Parameter(typeof(bool)), Expression.Constant(true));
        var filterMemberAccess = Expression.MakeMemberAccess(
            Expression.Parameter(typeof(MultiFilterTypes)),
            typeof(MultiFilterTypes).GetProperty(propertyName)!);

        // act
        var expression = ExpressionGenerator.GetIfIsEmptyConstraintExpression(
            filterMemberAccess,
            assignExpression);

        // assert
        Assert.Equal(ExpressionType.Conditional, expression.NodeType);

        var condition = ((ConditionalExpression)expression).Test;

        if (not)
        {
            Assert.Equal(ExpressionType.Not, condition.NodeType);
            condition = ((UnaryExpression)condition).Operand;
        }

        Assert.Equal(expectedType, condition.NodeType);

        if (name is not null)
        {
            if (expectedType == ExpressionType.Call)
                Assert.Equal(name, ((MethodCallExpression)condition).Method.Name);
            else if (expectedType == ExpressionType.MemberAccess)
                Assert.Equal(name, ((MemberExpression)condition).Member.Name);
        }
    }

    [Theory]
    [InlineData(typeof(Guid?), typeof(Guid), true)]
    [InlineData(typeof(Guid), typeof(Guid?), true)]
    [InlineData(typeof(int), typeof(long), false)]
    [InlineData(typeof(int?), typeof(long), false)]
    [InlineData(typeof(int), typeof(long?), false)]
    [InlineData(typeof(int?), typeof(long?), false)]
    [InlineData(typeof(IEnumerable<int>), typeof(int), true)]
    [InlineData(typeof(IEnumerable<int>), typeof(int?), true)]
    [InlineData(typeof(IEnumerable<int?>), typeof(int?), true)]
    [InlineData(typeof(IEnumerable<int>), typeof(long), false)]
    [InlineData(typeof(IEnumerable), typeof(int), false)]
    [InlineData(typeof(IEnumerable), typeof(int?), false)]
    [InlineData(typeof(IEnumerable), typeof(object), false)]
    [InlineData(typeof(IEnumerable<IEnumerable<int>>), typeof(int), false)]
    public void CheckTypes_Must_ValidateDifferentTypeAbleToFilter(
        Type filterPropertyType, Type modelPropertyType, bool expected)
    {
        // act
        var match = ExpressionGenerator.CheckTypes(filterPropertyType, modelPropertyType);

        // assert
        Assert.Equal(expected, match);
    }

    [Theory]
    [InlineData(CriterionOperator.Equal, false, "Age", ExpressionType.Equal, false)]
    [InlineData(CriterionOperator.Equal, true, "Age", ExpressionType.NotEqual, false)]
    [InlineData(CriterionOperator.GreaterThanOrEqual, false, "Age", ExpressionType.GreaterThanOrEqual, false)]
    [InlineData(CriterionOperator.GreaterThanOrEqual, true, "Age", ExpressionType.GreaterThanOrEqual, true)]
    [InlineData(CriterionOperator.GreaterThan, false, "Age", ExpressionType.GreaterThan, false)]
    [InlineData(CriterionOperator.GreaterThan, true, "Age", ExpressionType.GreaterThan, true)]
    [InlineData(CriterionOperator.LessThanOrEqual, false, "Age", ExpressionType.LessThanOrEqual, false)]
    [InlineData(CriterionOperator.LessThanOrEqual, true, "Age", ExpressionType.LessThanOrEqual, true)]
    [InlineData(CriterionOperator.LessThan, false, "Age", ExpressionType.LessThan, false)]
    [InlineData(CriterionOperator.LessThan, true, "Age", ExpressionType.LessThan, true)]
    [InlineData(CriterionOperator.Like, false, "Name", ExpressionType.Call, false)]
    [InlineData(CriterionOperator.Like, true, "Name", ExpressionType.Call, true)]
    [InlineData(CriterionOperator.Contains, false, "Name", ExpressionType.Call, false)]
    [InlineData(CriterionOperator.Contains, true, "Name", ExpressionType.Call, true)]
    [InlineData(CriterionOperator.StartsWith, false, "Name", ExpressionType.Call, false)]
    [InlineData(CriterionOperator.StartsWith, true, "Name", ExpressionType.Call, true)]
    [InlineData(CriterionOperator.EndsWith, false, "Name", ExpressionType.Call, false)]
    [InlineData(CriterionOperator.EndsWith, true, "Name", ExpressionType.Call, true)]
    [InlineData(CriterionOperator.In, false, "Tag", ExpressionType.Call, false)]
    [InlineData(CriterionOperator.In, true, "Tag", ExpressionType.Call, true)]
    public void CreateOperatorExpression_Must_GenerateTheRelatedExpression(
        CriterionOperator @operator, bool negation, string propertyName, ExpressionType expected, bool not)
    {
        // arrange
        var filterMemberAccess = Expression.MakeMemberAccess(
            Expression.Parameter(typeof(OperatorsFilter)),
            typeof(OperatorsFilter).GetProperty(propertyName)!);
        var modelMemberAccess = Expression.MakeMemberAccess(
            Expression.Parameter(typeof(OperatorsModel)),
            typeof(OperatorsModel).GetProperty(propertyName)!);

        // act
        var expression = ExpressionGenerator.CreateOperatorExpression(
            @operator,
            negation,
            filterMemberAccess,
            modelMemberAccess);

        // assert
        if (not)
        {
            Assert.Equal(ExpressionType.Not, expression.NodeType);
            expression = ((UnaryExpression)expression).Operand;
        }

        Assert.Equal(expected, expression.NodeType);
    }

    [Fact]
    public void ConfigureSpecifierGenerator_TwoAutoFields_OnePredicateFactory()
    {
        // arrange configuration
        ISearchConfigurations configurations = new SearchConfigurer();
        configurations.ConfigureSpecifierGenerator<ConfigurableEntity, ConfigurableFilter>(cfg =>
        {
            cfg.For(f => f.ModelId).Predicate(id => e => e.Models.Any(m => m.Id == id));
        });
        // arrange generator
        var generator = new DefaultSpecifierFunctionGenerator();

        // act
        var function = generator.Generate<ConfigurableEntity, ConfigurableFilter>();

        // assert
        Assert.NotNull(function);
    }

    [Fact]
    public void ConfigureSpecifierGenerator_OnlyPredicateFactory_NotNullProperty()
    {
        // arrange configuration
        ISearchConfigurations configurer = new SearchConfigurer();
        configurer.ConfigureSpecifierGenerator<ConfigurableEntity, ConfigurableFilterNotNull>(cfg =>
        {
            cfg.For(f => f.ModelId).Predicate(id => e => e.Models.Any(m => m.Id == id));
        });
        // arrange generator
        var generator = new DefaultSpecifierFunctionGenerator();

        // act
        var function = generator.Generate<ConfigurableEntity, ConfigurableFilterNotNull>();

        // assert
        Assert.NotNull(function);
    }

    [Fact]
    public void ConfigureSpecifierGenerator_RunningWithoutFilterValue_Must_ReturnAll()
    {
        // arrange
        ISearchConfigurations configurer = new SearchConfigurer();
        configurer.ConfigureSpecifierGenerator<ConfigurableEntity, ConfigurableFilter>(cfg =>
        {
            cfg.For(f => f.ModelId).Predicate(id => e => e.Models.Any(m => m.Id == id));
        });

        var generator = new DefaultSpecifierFunctionGenerator();
        var function = generator.Generate<ConfigurableEntity, ConfigurableFilter>()!;
        Assert.NotNull(function);

        var query = ConfigurableEntity.List.AsQueryable();
        var filter = new ConfigurableFilter();

        // act
        query = function(query, filter);

        // assert
        Assert.Equal(3, query.Count());
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 1)]
    [InlineData(5, 1)]
    [InlineData(6, 1)]
    [InlineData(7, 0)]
    public void ConfigureSpecifierGenerator_RunningWithFilter_Must_ApplyPredicates(int id, int expectedCount)
    {
        // arrange
        ISearchConfigurations configurer = new SearchConfigurer();
        configurer.ConfigureSpecifierGenerator<ConfigurableEntity, ConfigurableFilter>(cfg =>
        {
            cfg.For(f => f.ModelId).Predicate(id => e => e.Models.Any(m => m.Id == id));
        });

        var generator = new DefaultSpecifierFunctionGenerator();
        var function = generator.Generate<ConfigurableEntity, ConfigurableFilter>()!;
        Assert.NotNull(function);

        var query = ConfigurableEntity.List.AsQueryable();
        var filter = new ConfigurableFilter()
        {
            ModelId = id
        };

        // act
        query = function(query, filter);

        // assert
        Assert.Equal(expectedCount, query.Count());
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(1, 1)]
    [InlineData(7, 0)]
    public void ConfigureSpecifierGenerator_RunningWithFilter_NotNullProperty_Must_ApplyPredicates(int id, int expectedCount)
    {
        // arrange
        ISearchConfigurations configurer = new SearchConfigurer();
        configurer.ConfigureSpecifierGenerator<ConfigurableEntity, ConfigurableFilterNotNull>(cfg =>
        {
            cfg.For(f => f.ModelId).Predicate(id => e => e.Models.Any(m => m.Id == id));
        });

        var generator = new DefaultSpecifierFunctionGenerator();
        var function = generator.Generate<ConfigurableEntity, ConfigurableFilterNotNull>()!;
        Assert.NotNull(function);

        var query = ConfigurableEntity.List.AsQueryable();
        var filter = new ConfigurableFilterNotNull()
        {
            ModelId = id
        };

        // act
        query = function(query, filter);

        // assert
        Assert.Equal(expectedCount, query.Count());
    }

    [Fact]
    public void Factory_Must_UseTheSpecifierFilterMethod()
    {
        // arrange
        var services = new ServiceCollection();
        services.AddSmartSearchLinq();
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<ISpecifierFactory>();

        var data = new List<SimpleModel>
        {
            new() { Id = 1, Name = "A" },
            new() { Id = 2, Name = "B" },
            new() { Id = 3, Name = "AB" }
        }.AsQueryable();

        var filter = new SimpleFilterManual { Name = "A" };

        // act
        var specifier = factory.GetSpecifier<SimpleModel, SimpleFilterManual>();

        // assert
        Assert.NotNull(specifier);

        // act
        var result = specifier.Specify(data, filter);

        // assert
        Assert.True(SimpleFilterManual.Called);
        Assert.Equal(2, result.Count());
        Assert.All(result, m => Assert.Contains("A", m.Name));
    }
}

public class SimpleModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid Guid { get; set; }

    public DateTime? Date { get; set; }
}

public class SimpleFilter
{
    public string Name { get; set; } = null!;
}

public class SimpleFilterManual
{
    public static bool Called { get; private set; } = false;

    public string Name { get; set; } = null!;

    public IQueryable<SimpleModel> Filter(IQueryable<SimpleModel> query)
    {
        Called = true;

        if (Name is not null)
            query = query.Where(m => m.Name.Contains(Name));

        return query;
    }
}

file class NullablePropertiesFilter
{
    public string? Name { get; set; } = null!;

    public Guid? Guid { get; set; }
}

file class MultiFilterTypes
{
    public string Name { get; set; } = null!;

    public int? Age { get; set; }

    public DateTime Date { get; set; }

    public IEnumerable<string> Tags { get; set; } = null!;

    public decimal Value { get; set; }

    public SomeValueObject? Some { get; set; }

    public SomeStructValues StructValues { get; set; }
}

file record SomeValueObject(string Name, int Age);

file struct SomeStructValues
{
    public string Name { get; set; }

    public int Age { get; set; }
}

file class OperatorsModel
{
    public string Name { get; set; } = null!;

    public int Age { get; set; }

    public string Tag { get; set; } = null!;
}

file class OperatorsFilter
{
    public string Name { get; set; } = null!;

    public int Age { get; set; }

    public IEnumerable<string> Tag { get; set; } = null!;
}

file class LocalDbContext(DbContextOptions<LocalDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SimpleModel>();
    }
}

file class SearchConfigurer : ISearchConfigurations
{
    public ISearchConfigurations Add<TEntity>() where TEntity : class
    {
        throw new NotImplementedException();
    }

    public ISearchConfigurations Add(Type entityType)
    {
        throw new NotImplementedException();
    }
}

file class ConfigurableEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public List<SimpleModel> Models { get; set; } = null!;

    public static List<ConfigurableEntity> List =
    [
        new ConfigurableEntity()
        {
            Id = 1,
            Name = "test 1",
            Models =
            [
                new()
                {
                    Id = 1,
                    Name = "Simple Model 1"
                },
                new()
                {
                    Id = 2,
                    Name = "Simple Model 2"
                }
            ]
        },
        new ConfigurableEntity()
        {
            Id = 2,
            Name = "test 2",
            Models =
            [
                new()
                {
                    Id = 3,
                    Name = "Simple Model 3"
                },
                new()
                {
                    Id = 4,
                    Name = "Simple Model 4"
                }
            ]
        },
        new ConfigurableEntity()
        {
            Id = 3,
            Name = "test 3",
            Models =
            [
                new()
                {
                    Id = 5,
                    Name = "Simple Model 5"
                },
                new()
                {
                    Id = 6,
                    Name = "Simple Model 6"
                }
            ]
        }
    ];
}

file class ConfigurableFilter
{
    public int? Id { get; set; }

    public string? Name { get; set; }

    public int? ModelId { get; set; }
}

file class ConfigurableFilterNotNull
{
    public int ModelId { get; set; }
}


file class ModelBase
{
    public int Id { get; set; }
}

file class ExtendedModel : ModelBase
{
    public string Name { get; set; } = null!;
}

file class ExtendedFilter
{
    public int? Id { get; set; }

    public string? Name { get; set; }
}

