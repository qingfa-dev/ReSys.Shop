using Microsoft.EntityFrameworkCore;

using Module.Ordering.Backgrounds;
using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Backgrounds;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Component", "CartExpiryJob")]
public class CartExpiryJobTests
{
    [Fact(DisplayName = "RunAsync: expires drafts past cutoff in batches, uses Delete() domain method")]
    public async Task RunAsync_ShouldExpireCartsInBatches()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        await using var db = new ApplicationDbContext(options);

        var oldDate = DateTimeOffset.UtcNow.AddDays(-10);
        for (var i = 0; i < 3; i++)
        {
            db.Set<Order>().Add(new Order
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Draft,
                CreatedAtUtc = oldDate,
                ModifiedAtUtc = null,
                IsDeleted = false
            });
        }
        db.Set<Order>().Add(new Order
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ModifiedAtUtc = DateTimeOffset.UtcNow,
            IsDeleted = false
        });
        await db.SaveChangesAsync();

        var job = new CartExpiryJob(db, new Mock<ILogger<CartExpiryJob>>().Object, afterDays: 1);
        await job.RunAsync();

        var expired = await db.Set<Order>().IgnoreQueryFilters().Where(o => o.Status == OrderStatus.Expired).ToListAsync();
        expired.Should().HaveCount(3);
        expired.Should().AllSatisfy(o =>
        {
            o.IsDeleted.Should().BeTrue();
            o.DeletedBy.Should().Be("System");
            o.DeletedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        });

        var recent = await db.Set<Order>().Where(o => o.Status == OrderStatus.Draft).ToListAsync();
        recent.Should().HaveCount(1);
    }

    [Fact(DisplayName = "RunAsync: processes in batches of 500")]
    public async Task RunAsync_ShouldProcessInBatches()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        await using var db = new ApplicationDbContext(options);

        var oldDate = DateTimeOffset.UtcNow.AddDays(-10);
        for (var i = 0; i < 750; i++)
        {
            db.Set<Order>().Add(new Order
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Draft,
                CreatedAtUtc = oldDate,
                ModifiedAtUtc = null,
                IsDeleted = false
            });
        }
        await db.SaveChangesAsync();

        var job = new CartExpiryJob(db, new Mock<ILogger<CartExpiryJob>>().Object, afterDays: 1);
        await job.RunAsync();

        var expired = await db.Set<Order>().IgnoreQueryFilters().Where(o => o.Status == OrderStatus.Expired).ToListAsync();
        expired.Should().HaveCount(750);
    }
}
