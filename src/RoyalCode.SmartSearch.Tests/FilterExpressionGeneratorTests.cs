using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.SmartSearch.Linq.Filtering;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RoyalCode.SmartSearch.Tests;

public class FilterExpressionGeneratorTests
{
    private static async Task<IServiceProvider> CreateServiceProvider(string name)
    {
        ServiceCollection services = new();

        var sqliteConnection = new Microsoft.Data.Sqlite.SqliteConnection($"DataSource=:memory:");
        await sqliteConnection.OpenAsync();
        services.AddSingleton(sqliteConnection);

        services.AddDbContext<OrderDbContext>(builder => builder.UseSqlite(sqliteConnection));
        services.AddEntityFrameworkSearches<OrderDbContext>(s => s.Add<Order>());

        ServiceProvider provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await db.Database.EnsureCreatedAsync();

        return provider;
    }

    [Fact]
    public async Task Must_FilterBy_Period_ThisWeek_OnlyCurrentWeek()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_Period_ThisWeek_OnlyCurrentWeek));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var now = DateTime.UtcNow;
        // Helper to get Monday start
        int diff = ((int)now.DayOfWeek + 6) % 7;
        var weekStart = now.Date.AddDays(-diff);
        var weekEnd = weekStart.AddDays(7);

        ctx.AddRange(
            new Order { Id = 1, ClientName = "A", Description = "d1", TotalAmount = 10, OrderDate = weekStart.AddHours(1) },
            new Order { Id = 2, ClientName = "B", Description = "d2", TotalAmount = 20, OrderDate = weekEnd.AddHours(1) }, // outside week (>= end)
            new Order { Id = 3, ClientName = "C", Description = "d3", TotalAmount = 30, OrderDate = weekStart.AddDays(3).AddHours(2) }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<Order>>();

        // act
        var filter = new OrderByDateFilter { Period = Period.ThisWeek };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, o => o.Id == 1);
        Assert.Contains(result, o => o.Id == 3);
    }

    [Fact]
    public async Task Must_FilterBy_Period_ThisMonth_OnlyCurrentMonth()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_Period_ThisMonth_OnlyCurrentMonth));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        ctx.AddRange(
            new Order { Id = 1, ClientName = "A", Description = "d1", TotalAmount = 10, OrderDate = monthStart.AddDays(10) },
            new Order { Id = 2, ClientName = "B", Description = "d2", TotalAmount = 20, OrderDate = monthEnd.AddDays(1) }, // outside month
            new Order { Id = 3, ClientName = "C", Description = "d3", TotalAmount = 30, OrderDate = monthStart.AddDays(20) }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<Order>>();

        // act
        var filter = new OrderByDateFilter { Period = Period.ThisMonth };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, o => o.Id == 1);
        Assert.Contains(result, o => o.Id == 3);
    }

    [Fact]
    public async Task Must_FilterBy_Period_Today_OnlyToday()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_Period_Today_OnlyToday));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var now = DateTime.UtcNow;
        ctx.AddRange(
            new Order { Id = 1, ClientName = "A", Description = "d1", TotalAmount = 10, OrderDate = now.Date.AddHours(10) },
            new Order { Id = 2, ClientName = "B", Description = "d2", TotalAmount = 20, OrderDate = now.Date.AddHours(5) },
            new Order { Id = 3, ClientName = "C", Description = "d3", TotalAmount = 30, OrderDate = now.Date.AddDays(-1).AddHours(12) }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<Order>>();

        // act
        var filter = new OrderByDateFilter { Period = Period.Today };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, o => o.Id == 1);
        Assert.Contains(result, o => o.Id == 2);
    }

    [Fact]
    public async Task Must_FilterBy_Period_ThisYear_ExcludeLastYear()
    {
        // arrange
        var provider = await CreateServiceProvider(nameof(Must_FilterBy_Period_ThisYear_ExcludeLastYear));
        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        var now = DateTime.UtcNow;
        var lastYearDate = new DateTime(now.Year - 1, Math.Min(6, now.Month), 15, 12, 0, 0, DateTimeKind.Utc);

        ctx.AddRange(
            new Order { Id = 1, ClientName = "A", Description = "d1", TotalAmount = 10, OrderDate = now.Date.AddHours(10) },
            new Order { Id = 2, ClientName = "B", Description = "d2", TotalAmount = 20, OrderDate = now.Date.AddDays(-10) }, // should still be this year
            new Order { Id = 3, ClientName = "C", Description = "d3", TotalAmount = 30, OrderDate = lastYearDate }
        );
        ctx.SaveChanges();

        var criteria = scope.ServiceProvider.GetRequiredService<ICriteria<Order>>();

        // act
        var filter = new OrderByDateFilter { Period = Period.ThisYear };
        var result = criteria.FilterBy(filter).Collect();

        // assert
        Assert.DoesNotContain(result, o => o.Id == 3);
        Assert.Contains(result, o => o.Id == 1);
        Assert.Contains(result, o => o.Id == 2);
    }
}


internal class Order
{
    public int Id { get; set; }

    public required string ClientName { get; set; }

    public required string Description { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
}

internal enum Period
{
    Today,
    ThisWeek,
    ThisMonth,
    ThisYear
}

internal class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
}

internal class PeriodSpecifierExpressionGenerator : ISpecifierExpressionGenerator
{
    public readonly struct PeriodRange
    {
        public readonly DateTime Start { get; }
        public readonly DateTime End { get; }

        public PeriodRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
    }

    public static PeriodRange GetRange(Period period)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1);

        int diff = ((int)now.DayOfWeek + 6) % 7;
        var weekStart = todayStart.AddDays(-diff);
        var weekEnd = weekStart.AddDays(7);

        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var yearStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = yearStart.AddYears(1);

        return period switch
        {
            Period.Today => new PeriodRange(todayStart, todayEnd),
            Period.ThisWeek => new PeriodRange(weekStart, weekEnd),
            Period.ThisMonth => new PeriodRange(monthStart, monthEnd),
            Period.ThisYear => new PeriodRange(yearStart, yearEnd),
            _ => new PeriodRange(todayStart, todayEnd)
        };
    }

    public static Expression GenerateExpression(ExpressionGeneratorContext context)
    {
        // Call static GetRange(period) and apply range Start/End once
        var getRangeMethod = typeof(PeriodSpecifierExpressionGenerator).GetMethod(nameof(GetRange))!;
        var periodRangeCall = Expression.Call(getRangeMethod, context.FilterMember);

        var startProp = typeof(PeriodRange).GetProperty(nameof(PeriodRange.Start))!;
        var endProp = typeof(PeriodRange).GetProperty(nameof(PeriodRange.End))!;
        var startExpr = Expression.Property(periodRangeCall, startProp);
        var endExpr = Expression.Property(periodRangeCall, endProp);

        var ge = Expression.GreaterThanOrEqual(context.ModelMember, startExpr);
        var lt = Expression.LessThan(context.ModelMember, endExpr);
        var body = Expression.AndAlso(ge, lt);

        var predicateType = typeof(Func<,>).MakeGenericType(context.Model.Type, typeof(bool));
        var lambda = Expression.Lambda(predicateType, body, context.Model);

        var whereCall = Expression.Call(
            typeof(Queryable), nameof(Queryable.Where), new[] { context.Model.Type }, context.Query, lambda);

        return Expression.Assign(context.Query, whereCall);
    }
}

internal class OrderByDateFilter
{
    [Criterion(nameof(Order.OrderDate))]
    [FilterExpressionGenerator<PeriodSpecifierExpressionGenerator>]
    public Period Period { get; set; }
}