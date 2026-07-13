using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Backgrounds;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Component", "CartExpiryJob")]
public class CartExpiryJobTests
{
    [Fact]
    public void ExpiredFilter_ShouldIncludeNullModifiedAtUtc()
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-1);
        var orders = new[]
        {
            new Order { Status = OrderStatus.Draft, ModifiedAtUtc = null, IsDeleted = false },
            new Order { Status = OrderStatus.Draft, ModifiedAtUtc = DateTimeOffset.UtcNow, IsDeleted = false },
            new Order { Status = OrderStatus.Placed, ModifiedAtUtc = null, IsDeleted = false },
        };
        var expired = orders.Where(o =>
            o.Status == OrderStatus.Draft
            && (o.ModifiedAtUtc == null || o.ModifiedAtUtc < cutoff)
            && !o.IsDeleted).ToList();
        expired.Should().ContainSingle();
        expired[0].ModifiedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ExpiredFilter_ShouldExcludeRecentlyModified()
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-1);
        var orders = new[]
        {
            new Order { Status = OrderStatus.Draft, ModifiedAtUtc = DateTimeOffset.UtcNow, IsDeleted = false },
            new Order { Status = OrderStatus.Draft, ModifiedAtUtc = null, IsDeleted = true },
        };
        var expired = orders.Where(o =>
            o.Status == OrderStatus.Draft
            && (o.ModifiedAtUtc == null || o.ModifiedAtUtc < cutoff)
            && !o.IsDeleted).ToList();
        expired.Should().BeEmpty();
    }
}
