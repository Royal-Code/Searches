using RoyalCode.SmartSearch.Demo.Domain;
using RoyalCode.SmartSearch.Demo.Dtos;
using RoyalCode.SmartSearch.Demo.Filters;

namespace RoyalCode.SmartSearch.Demo.Endpoints;

/// <summary>
/// Endpoints built with the AspNetCore helpers. Each helper wires filtering, sorting, paging and the standardized
/// HTTP results (200/204/400/500) for you. The filter type is bound from the query string via <c>[AsParameters]</c>.
/// </summary>
public static class MappedSearchEndpoints
{
    public static IEndpointRouteBuilder MapMappedSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api")
            .WithExceptionFilter();

        // Paged DTO search. Same filter, DTO and result as the manual GET /manual/orders.
        group.MapSearch<Order, OrderSummaryDto, OrderFilter>("/orders").WithTags("Mapped (helpers)");

        // Paged DTO search for customers (uses the registered CustomerDto selector).
        group.MapSearch<Customer, CustomerDto, CustomerFilter>("/customers").WithTags("Mapped (helpers)");

        // Simple (non-paged) list of DTOs. ProductDto is projected by convention.
        group.MapList<Product, ProductDto, ProductFilter>("/products").WithTags("Mapped (helpers)");

        // First entity matching the filter.
        group.MapFirst<Product, ProductFilter>("/products/first").WithTags("Mapped (helpers)");

        // First DTO matching the filter (uses the registered CustomerDto selector).
        group.MapSelectFirst<Customer, CustomerDto, CustomerFilter>("/customers/first").WithTags("Mapped (helpers)");

        return group;
    }
}
