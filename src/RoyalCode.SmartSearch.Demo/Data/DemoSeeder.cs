using RoyalCode.SmartSearch.Demo.Domain;

namespace RoyalCode.SmartSearch.Demo.Data;

/// <summary>
/// Deterministic seed for the demo. The first records (customers 1-5, the 5 products, orders 1-5) are the
/// "named" ones the documentation and tests refer to; the remaining rows are generated deterministically to
/// provide enough volume to paginate. Generated rows deliberately avoid the values the documented filters key on
/// (no "maria"/"mario" names, no NYC/NY addresses, dates after the documented range, never Cancelled).
/// </summary>
public static class DemoSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Customers.Any())
            return;

        db.Customers.AddRange(BuildCustomers());
        db.Products.AddRange(BuildProducts());
        db.SaveChanges();

        db.Orders.AddRange(BuildOrders());
        db.SaveChanges();
    }

    private static List<Customer> BuildCustomers()
    {
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "Maria Silva",    Email = "maria@shop.com", MainAddress = new Address { Street = "1st Ave",  City = "NYC",    State = "NY", PostalCode = "10001" } },
            new() { Id = 2, Name = "Mario Souza",    Email = "mario@shop.com", MainAddress = new Address { Street = "2nd St",   City = "NYC",    State = "NY", PostalCode = "10002" } },
            new() { Id = 3, Name = "John Appleseed", Email = "john@corp.com",  MainAddress = new Address { Street = "3rd Blvd", City = "LA",     State = "CA", PostalCode = "90001" } },
            new() { Id = 4, Name = "Ana Pereira",    Email = "ana@corp.com",   MainAddress = new Address { Street = "4th Rd",   City = "SF",     State = "CA", PostalCode = "94101" } },
            new() { Id = 5, Name = "Bruno Costa",    Email = "bruno@shop.com", MainAddress = new Address { Street = "5th Ln",   City = "Austin", State = "TX", PostalCode = "73301" } },
        };

        // 20 more customers, none in NYC/NY and none matching "maria"/"mario".
        string[] first = ["Carla", "Diego", "Elena", "Felipe", "Gabriela", "Hugo", "Isabela", "Rafael", "Julia", "Lucas",
                          "Marina", "Nelson", "Olivia", "Paulo", "Renata", "Sergio", "Tatiana", "Vitor", "Wanda", "Xavier"];
        string[] last = ["Dias", "Melo", "Rocha", "Nunes", "Barros", "Campos", "Freitas", "Gomes", "Lima", "Moraes"];
        (string City, string State)[] places = [("Rio", "RJ"), ("Miami", "FL"), ("Denver", "CO"), ("Boston", "MA"), ("Seattle", "WA")];

        for (var j = 0; j < 20; j++)
        {
            var id = 6 + j;
            var (city, state) = places[j % places.Length];
            customers.Add(new Customer
            {
                Id = id,
                Name = $"{first[j]} {last[j % last.Length]}",
                Email = $"{first[j].ToLowerInvariant()}.{last[j % last.Length].ToLowerInvariant()}@mail.com",
                MainAddress = new Address
                {
                    Street = $"{id} Market St",
                    City = city,
                    State = state,
                    PostalCode = $"{20000 + id}",
                },
            });
        }

        return customers;
    }

    private static List<Product> BuildProducts() =>
    [
        new() { Id = 1, Sku = "ABC-001", Name = "Keyboard", Price = 49.90m,  Active = true },
        new() { Id = 2, Sku = "ABC-002", Name = "Mouse",    Price = 19.90m,  Active = true },
        new() { Id = 3, Sku = "XYZ-100", Name = "Monitor",  Price = 899.00m, Active = true },
        new() { Id = 4, Sku = "XYZ-200", Name = "Headset",  Price = 129.00m, Active = false },
        new() { Id = 5, Sku = "ABC-003", Name = "Webcam",   Price = 79.00m,  Active = true },
    ];

    private static List<Order> BuildOrders()
    {
        var orders = new List<Order>
        {
            new() { Id = 1, Number = "ORD-1001", CreatedAt = new DateTime(2026, 1, 15), Status = OrderStatus.Paid,      CustomerId = 1, Items = [ new() { Id = 1, ProductId = 1, Quantity = 1, UnitPrice = 49.90m }, new() { Id = 2, ProductId = 2, Quantity = 2, UnitPrice = 19.90m } ] },
            new() { Id = 2, Number = "ORD-1002", CreatedAt = new DateTime(2026, 2, 10), Status = OrderStatus.Pending,   CustomerId = 2, Items = [ new() { Id = 3, ProductId = 3, Quantity = 1, UnitPrice = 899.00m } ] },
            new() { Id = 3, Number = "ORD-1003", CreatedAt = new DateTime(2026, 3, 5),  Status = OrderStatus.Shipped,   CustomerId = 1, Items = [ new() { Id = 4, ProductId = 5, Quantity = 3, UnitPrice = 79.00m } ] },
            new() { Id = 4, Number = "ORD-1004", CreatedAt = new DateTime(2026, 4, 20), Status = OrderStatus.Cancelled, CustomerId = 3, Items = [ new() { Id = 5, ProductId = 4, Quantity = 1, UnitPrice = 129.00m } ] },
            new() { Id = 5, Number = "ORD-1005", CreatedAt = new DateTime(2026, 6, 1),  Status = OrderStatus.Paid,      CustomerId = 4, Items = [ new() { Id = 6, ProductId = 3, Quantity = 2, UnitPrice = 899.00m }, new() { Id = 7, ProductId = 1, Quantity = 1, UnitPrice = 49.90m } ] },
        };

        // 25 more orders: dates after the documented Feb-May range, never Cancelled, assigned to the generated customers.
        OrderStatus[] cycle = [OrderStatus.Paid, OrderStatus.Pending, OrderStatus.Shipped];
        decimal[] prices = [49.90m, 19.90m, 899.00m, 129.00m, 79.00m];
        var itemId = 8;

        for (var j = 0; j < 25; j++)
        {
            var id = 6 + j;
            var productId = (j % 5) + 1;
            orders.Add(new Order
            {
                Id = id,
                Number = $"ORD-{1000 + id}",
                CreatedAt = new DateTime(2026, 7, 1).AddDays(j * 3),
                Status = cycle[j % cycle.Length],
                CustomerId = 6 + (j % 20),
                Items = [new OrderItem { Id = itemId++, ProductId = productId, Quantity = (j % 3) + 1, UnitPrice = prices[productId - 1] }],
            });
        }

        return orders;
    }
}
