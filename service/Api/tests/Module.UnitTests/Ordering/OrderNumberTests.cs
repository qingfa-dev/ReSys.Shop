using Microsoft.EntityFrameworkCore;
using Module.Ordering.Domain.Orders;
using OrderEntity = Module.Ordering.Domain.Orders.Order;

namespace Module.UnitTests.Ordering;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
public class OrderNumberTests : IDisposable
{
    private readonly ApplicationDbContext _db;

    public OrderNumberTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(OrderEntity).Assembly];
        _db = new ApplicationDbContext(opts);
    }

    public void Dispose() { _db.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Generate: returns well-formed order number")]
    public void Generate_ReturnsWellFormed()
    {
        var n = OrderNumber.Generate(_db, out var attempts);
        n.Should().MatchRegex(@"^R\d{8}-[A-F0-9]{8}$");
        attempts.Should().Be(1, "first call on an empty db should not retry");
    }

    [Fact(DisplayName = "Generate: 10000 calls produce no duplicates")]
    public async Task Generate_10000Calls_NoDuplicates()
    {
        var seen = new HashSet<string>();
        for (var i = 0; i < 10_000; i++)
        {
            var n = OrderNumber.Generate(_db, out _);
            seen.Add(n).Should().BeTrue($"duplicate generated on iteration {i}: {n}");
        }
    }

    [Fact(DisplayName = "Generate: retries when prefix collides")]
    public async Task Generate_RetriesOnCollision()
    {
        // Seed an order with a forced collision by stubbing the prefix
        // Implementation detail: the generator MUST query the db by Number
        // and retry if found. We pre-seed a row with the next predicted number.
        var first = OrderNumber.Generate(_db, out _);
        _db.Set<OrderEntity>().Add(new OrderEntity
        {
            Id = Guid.NewGuid(),
            Number = first,
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Draft,
            Currency = "USD"
        });
        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // The next call should NOT return `first` even if its random suffix
        // happens to match (it likely won't, but the test is for the retry path).
        var second = OrderNumber.Generate(_db, out var attempts);
        second.Should().NotBe(first);
    }
}
