using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.UnitTests.Ordering.Domain.Orders;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Entity", "Order")]
public class OrderDiscontinuedTests
{
    [Fact]
    public void EnsureLineItemVariantsAreNotDiscontinued_Should_Return_False_When_Variant_Discontinued()
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid(), Guid.NewGuid()).Value;
        var variantId = Guid.NewGuid();
        order.LineItems.Add(LineItemMethod.Create(order.Id, variantId, 1, 10.00m).Value);

        var result = order.EnsureLineItemVariantsAreNotDiscontinued(new HashSet<Guid> { variantId });

        result.Should().BeFalse();
    }

    [Fact]
    public void EnsureLineItemVariantsAreNotDiscontinued_Should_Return_True_When_No_Variant_Discontinued()
    {
        var order = OrderMethod.Create("USD", Guid.NewGuid(), Guid.NewGuid()).Value;
        var variantId = Guid.NewGuid();
        order.LineItems.Add(LineItemMethod.Create(order.Id, variantId, 1, 10.00m).Value);

        var result = order.EnsureLineItemVariantsAreNotDiscontinued(new HashSet<Guid>());

        result.Should().BeTrue();
    }
}
