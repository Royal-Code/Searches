using RoyalCode.OperationHint.Abstractions;
using RoyalCode.SmartSearch.Demo.Data;
using RoyalCode.SmartSearch.Demo.Domain;
using RoyalCode.SmartSearch.Demo.Dtos;
using RoyalCode.SmartSearch.Demo.Search;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Wires up SmartSearch for the demo: criteria services, selectors, named sortings, the native Like operator
/// and operation hints.
/// </summary>
public static class SearchSetup
{
    public static IServiceCollection AddDemoSearches(this IServiceCollection services)
    {
        services.AddEntityFrameworkSearches<AppDbContext>(cfg =>
        {
            // Register ICriteria<T> for the searchable entities.
            cfg.Add<Customer>();
            cfg.Add<Product>();
            cfg.Add<Order>();

            // Registered selector: Select<CustomerDto>() (and MapSelectFirst) pick it up.
            cfg.AddSelector<Customer, CustomerDto>(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                City = c.MainAddress != null ? c.MainAddress.City : null,
            });

            // Registered selector: computes Total from the items and flattens the customer name.
            cfg.AddSelector<Order, OrderSummaryDto>(o => new OrderSummaryDto
            {
                Id = o.Id,
                Number = o.Number,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                CustomerName = o.Customer.Name,
                Total = o.Items.Sum(i => i.Quantity * i.UnitPrice),
            });

            // Named sortings usable via ?orderby=<name> or ?orderby=<name>-desc.
            cfg.AddOrderBy<Order, DateTime>("createdAt", o => o.CreatedAt);
            cfg.AddOrderBy<Order, string>("number", o => o.Number);
            cfg.AddOrderBy<Order, string>("customer", o => o.Customer.Name);
            cfg.AddOrderBy<Product, decimal>("price", p => p.Price);
            cfg.AddOrderBy<Customer, string>("name", c => c.Name);
        });

        // Opt-in: emit Like as native SQL LIKE, so user wildcards (e.g. LikeWrap.None with 'ABC%') are honored.
        services.AddEntityFrameworkLikeOperator();

        // Map operation hints to EF includes for Order (applied on entity terminals, not on projections/Exists).
        services.ConfigureOperationHints(registry =>
        {
            registry.AddIncludesHandler<Order, OrderHints>((hint, includes) =>
            {
                switch (hint)
                {
                    case OrderHints.WithCustomer:
                        includes.IncludeReference(o => o.Customer);
                        break;
                    case OrderHints.WithItems:
                        includes.IncludeCollection(o => o.Items);
                        break;
                }
            });
        });

        return services;
    }
}
