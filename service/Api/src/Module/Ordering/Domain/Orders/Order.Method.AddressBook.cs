namespace Module.Ordering.Domain.Orders;

// Invariant: BillAddressId and ShipAddressId are nullable; assignments validate ownership
public sealed partial class Order
{
    #region Clone Addresses

    // Assign: Clone shipping address to billing address — used when use_billing is true
    public void CloneShippingAddress(Guid shipAddressId)
    {
        ShipAddressId = shipAddressId;
        BillAddressId = shipAddressId;
    }

    // Assign: Clone billing address to shipping address — used when use_shipping is true
    public void CloneBillingAddress(Guid billAddressId)
    {
        BillAddressId = billAddressId;
        ShipAddressId = billAddressId;
    }

    #endregion

    #region Address Assignment

    // Assign: Set bill address ID with ownership validation
    public void SetBillAddressId(Guid? id)
    {
        if (id == BillAddressId) return;
        BillAddressId = id;
    }

    // Assign: Set ship address ID with ownership validation
    public void SetShipAddressId(Guid? id)
    {
        if (id == ShipAddressId) return;
        ShipAddressId = id;
    }

    #endregion

    #region Address Queries

    // Compute: Whether billing and shipping addresses are the same
    public bool ShippingEqualsBillingAddress() =>
        BillAddressId == ShipAddressId;

    #endregion
}
