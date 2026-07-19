using Microsoft.AspNetCore.Mvc;
using RoyalCode.SmartProblems;
using RoyalCode.SmartProblems.HttpResults;
using RoyalCode.SmartSearch.AspNetCore.HttpResults;
using RoyalCode.SmartSearch.Demo.Domain;
using RoyalCode.SmartSearch.Demo.Dtos;
using RoyalCode.SmartSearch.Demo.Filters;
using RoyalCode.SmartSearch.Demo.Search;
using RoyalCode.SmartSearch.Exceptions;

namespace RoyalCode.SmartSearch.Demo.Endpoints;

/// <summary>
/// Endpoints that use <see cref="ICriteria{TEntity}"/> directly, without the AspNetCore helpers. They show the
/// full surface: Collect/ToList, Select (explicit and by convention), Exists, Single, FirstOrDefault, In,
/// Skip/Take + UseCount, [ComplexFilter] and per-query hints.
/// <para>
/// Error handling uses SmartProblems: recoverable cases return <see cref="Problems"/> through the
/// <c>OkMatch&lt;T&gt;</c> result type (converted to RFC-9457 ProblemDetails), and the group's
/// <c>WithExceptionFilter</c> turns any unexpected exception into a 500 ProblemDetails.
/// </para>
/// </summary>
public static class ManualSearchEndpoints
{
    public static IEndpointRouteBuilder MapManualSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/manual")
            .WithTags("Manual (ICriteria)")
            .WithExceptionFilter();

        // Customers filtered by a flat filter, projected with an explicit Select expression.
        // Invalid order by -> SmartProblems InvalidParameter (400) via the standardized MatchList result.
        group.MapGet("/customers", async Task<MatchList<CustomerDto>> (
            [AsParameters] CustomerFilter filter,
            [FromQuery] Sorting[]? orderby,
            [FromServices] ICriteria<Customer> criteria,
            CancellationToken ct) =>
        {
            try
            {
                var result = await criteria
                    .OrderBy(orderby)
                    .FilterBy(filter)
                    .Select(c => new CustomerDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        City = c.MainAddress != null ? c.MainAddress.City : null,
                    })
                    .ToListAsync(ct);

                if (result.Count == 0)
                    return TypedResults.NoContent();

                return TypedResults.Ok(result.Items);
            }
            catch (OrderByException ex)
            {
                return Problems.InvalidParameter(ex.Message, "orderby");
            }
        });

        // Customers filtered by a structured [ComplexFilter] over the owned address, projected via the
        // registered selector (Select<CustomerDto>()).
        group.MapGet("/customers/by-address", async (
            string? city,
            string? state,
            [FromServices] ICriteria<Customer> criteria,
            CancellationToken ct) =>
        {
            var filter = new CustomerAddressFilter { Address = new AddressFilter { City = city, State = state } };
            var result = await criteria.FilterBy(filter).Select<CustomerDto>().ToListAsync(ct);
            return result.Count == 0 ? Results.NoContent() : Results.Ok(result.Items);
        });

        // The same query as the mapped GET /api/orders (kitchen-sink filter + paged OrderSummaryDto), built by hand.
        // Returns the same standardized MatchSearch result; invalid order by -> SmartProblems InvalidParameter (400).
        group.MapGet("/orders", async Task<MatchSearch<OrderSummaryDto>> (
            [AsParameters] OrderFilter filter,
            [AsParameters] SearchOptions options,
            [FromQuery] Sorting[]? orderby,
            [FromServices] ICriteria<Order> criteria,
            CancellationToken ct) =>
        {
            try
            {
                var result = await criteria
                    .WithOptions(options)
                    .OrderBy(orderby)
                    .FilterBy(filter)
                    .Select<OrderSummaryDto>()
                    .ToListAsync(ct);

                if (result.Count == 0)
                    return TypedResults.NoContent();

                return TypedResults.Ok(result);
            }
            catch (OrderByException ex)
            {
                return Problems.InvalidParameter(ex.Message, "orderby");
            }
        });

        // In operator: statuses come as an array query param and are assigned to an IEnumerable<> filter property.
        group.MapGet("/orders/by-status", async (
            [FromQuery] OrderStatus[]? statuses,
            [FromServices] ICriteria<Order> criteria,
            CancellationToken ct) =>
        {
            var filter = new OrderStatusesFilter { Statuses = statuses };
            var result = await criteria.FilterBy(filter).Select<OrderSummaryDto>().ToListAsync(ct);
            return result.Count == 0 ? Results.NoContent() : Results.Ok(result.Items);
        });

        // Offset paging (Skip/Take) plus UseCount(false): a "next page" that does not compute the total.
        group.MapGet("/orders/page", async (
            int skip,
            int take,
            bool? count,
            [FromServices] ICriteria<Order> criteria,
            CancellationToken ct) =>
        {
            var result = await criteria
                .FilterBy(new OrderFilter())
                .OrderBy(new Sorting { OrderBy = "createdAt" })
                .SkipTake(skip, take)
                .UseCount(count ?? false)
                .Select<OrderSummaryDto>()
                .ToListAsync(ct);

            return Results.Ok(result);
        });

        // Exists: cheap existence check (hints and projections do not apply).
        group.MapGet("/orders/exists", async (
            [AsParameters] OrderFilter filter,
            [FromServices] ICriteria<Order> criteria,
            CancellationToken ct) =>
        {
            var exists = await criteria.FilterBy(filter).ExistsAsync(ct);
            return Results.Ok(new { exists });
        });

        // Single: exactly-one contract by unique number. No element -> SmartProblems NotFound (404).
        group.MapGet("/orders/by-number/{number}", async Task<OkMatch<Order>> (
            string number,
            [FromServices] ICriteria<Order> criteria,
            CancellationToken ct) =>
        {
            try
            {
                var order = await criteria
                    .UseHints(OrderHints.WithCustomer, OrderHints.WithItems)
                    .FilterBy(new OrderByNumberFilter { Number = number })
                    .SingleAsync(ct);
                return order;
            }
            catch (InvalidOperationException)
            {
                // Single throws when there is no element (or more than one).
                return Problems.NotFound($"Order '{number}' was not found.", "number");
            }
        });

        // FirstOrDefault by id, with per-query hints loading the Customer and Items navigations.
        // Not found -> SmartProblems NotFound (404).
        group.MapGet("/orders/{id:int}", async Task<OkMatch<Order>> (
            int id,
            [FromServices] ICriteria<Order> criteria,
            CancellationToken ct) =>
        {
            var order = await criteria
                .UseHints(OrderHints.WithCustomer, OrderHints.WithItems)
                .FilterBy(new OrderByIdFilter { Id = id })
                .FirstOrDefaultAsync(ct);

            if (order is null)
                return Problems.NotFound($"Order '{id}' was not found.", "id");

            return order;
        });

        return app;
    }
}
