using System.Threading.Channels;
using Module.Ordering.Domain.Orders.Contracts;
using Module.Ordering.Infrastructure.Events;

namespace Module.UnitTests.Ordering;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
public class InProcessOrderEventPublisherTests
{
    [Fact(DisplayName = "Publish: enqueues event readable from the channel")]
    public async Task Publish_ReaderReceivesEvent()
    {
        var sut = new InProcessOrderEventPublisher();
        var payload = "test-payload";

        await sut.PublishAsync("order.placed", payload, TestContext.Current.CancellationToken);

        var read = await sut.Reader.ReadAsync(TestContext.Current.CancellationToken);
        read.EventName.Should().Be("order.placed");
        read.Payload.Should().Be(payload);
    }

    [Fact(DisplayName = "Publish: ordering is preserved for sequential writes")]
    public async Task Publish_OrderingPreserved()
    {
        var sut = new InProcessOrderEventPublisher();

        for (var i = 0; i < 100; i++)
        {
            await sut.PublishAsync("order.placed", i, TestContext.Current.CancellationToken);
        }

        for (var i = 0; i < 100; i++)
        {
            var read = await sut.Reader.ReadAsync(TestContext.Current.CancellationToken);
            read.EventName.Should().Be("order.placed");
            read.Payload.Should().Be(i);
        }
    }
}
