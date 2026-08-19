using System.Text.Json;
using System.Text.Json.Serialization;

using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Admin.Shared.Models;
using Module.Inventory.Features.Admin.StockItems.LowStock;
using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Shared.Models;

namespace Module.UnitTests.Serialization;

[Trait("Category", "Unit")]
[Trait("Feature", "EnumWireFormatSerialization")]
public class EnumWireFormatSerializationTests
{
    // Options: Mirror the global JsonStringEnumConverter registered in Program.cs
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact(DisplayName = "Serialize: OrderTimelineEvent.Type emits PascalCase member names")]
    public void OrderTimelineEvent_Type_EmitsPascalCaseMemberNames()
    {
        var timeline = new[]
        {
            new OrderTimelineEvent { Type = OrderTimelineEventType.Created, Label = "Order created" },
            new OrderTimelineEvent { Type = OrderTimelineEventType.PaymentCompleted, Label = "Payment completed" }
        };

        var json = JsonSerializer.Serialize(timeline, JsonOptions);

        json.Should().Contain("\"Type\":\"Created\"");
        json.Should().Contain("\"Type\":\"PaymentCompleted\"");
    }

    [Fact(DisplayName = "Serialize: PaymentDetailResponse.State emits member names")]
    public void PaymentDetailResponse_State_EmitsMemberNames()
    {
        var pending = new PaymentDetailResponse { State = PaymentRecordState.Pending, Currency = "USD" };
        var completed = new PaymentDetailResponse { State = PaymentRecordState.Completed, Currency = "USD" };

        JsonSerializer.Serialize(pending, JsonOptions).Should().Contain("\"State\":\"Pending\"");
        JsonSerializer.Serialize(completed, JsonOptions).Should().Contain("\"State\":\"Completed\"");
    }

    [Fact(DisplayName = "Serialize: LowStockStatus emits member names")]
    public void LowStockStatus_EmitsMemberNames()
    {
        var low = new GetLowStockItems.Response { Status = LowStockStatus.Low };
        var outOfStock = new GetLowStockItems.Response { Status = LowStockStatus.OutOfStock };

        JsonSerializer.Serialize(low, JsonOptions).Should().Contain("\"Status\":\"Low\"");
        JsonSerializer.Serialize(outOfStock, JsonOptions).Should().Contain("\"Status\":\"OutOfStock\"");
    }
}