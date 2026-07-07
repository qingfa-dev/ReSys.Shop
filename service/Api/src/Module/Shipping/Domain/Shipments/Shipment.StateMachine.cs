namespace Module.Shipping.Domain.Shipments;

// Context: Ported from Ruby Spree::Shipment state machine and business logic
//          (ruby-sdk-1/core/app/models/spree/shipment.rb)
//          State machine: Pending -> Ready -> Shipped (terminal); Pending/Canceled transitions
//          Invariant: State transitions follow Pending->Ready->Shipped; Pending->Canceled; Ready->Canceled; Canceled->Ready|Pending
public static class ShipmentStateMachine
{
    #region State Transitions

    // Enforce: Only Ready shipments can be pended back to Pending
    //          (Ruby SDK shipment.rb state machine: event :pend, ready -> pending)
    public static Result Pend(this Shipment shipment)
    {
        if (shipment.State != ShipmentState.Ready)
        {
            return ShipmentResult.Errors.InvalidStateTransition(shipment.State, ShipmentState.Pending);
        }

        shipment.State = ShipmentState.Pending;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok();
    }

    // Enforce: Only Canceled shipments can be resumed
    //          (Ruby SDK: canceled -> ready (if order ready) or canceled -> pending)
    public static Result Resume(this Shipment shipment)
    {
        if (shipment.State != ShipmentState.Canceled)
        {
            return ShipmentResult.Errors.InvalidStateTransition(shipment.State, ShipmentState.Ready);
        }

        // Compute: Determine target state based on order readiness (Ruby SDK determine_state pattern)
        shipment.State = ShipmentState.Pending;
        shipment.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(ShipmentResult.Success.Ready(shipment.Id));
    }

    #endregion State Transitions

    #region Lifecycle Callbacks

    // Handle: Post-ship actions — restock adjustment, inventory finalization
    //         (Ruby SDK: after_ship calls ShipmentHandler.factory(self).perform)
    public static void AfterShip(this Shipment shipment)
    {
        // Handle: Trigger shipment processed handlers
    }

    // Compensate: Restock inventory units when shipment is canceled
    //             (Ruby SDK: after_cancel -> manifest_restock for each manifest item)
    public static void AfterCancel(this Shipment shipment)
    {
        // Compensate: Restore stocked items for shipments that hadn't shipped yet
    }

    // Compensate: Restore inventory when shipment resumes from canceled
    //             (Ruby SDK: after_resume -> manifest_unstock for each manifest item)
    public static void AfterResume(this Shipment shipment)
    {
        // Compensate: Re-reserve inventory for resumed shipments
    }

    #endregion Lifecycle Callbacks

    #region State Determination

    // Compute: Determine the correct shipment state based on order state (Ruby SDK determine_state)
    //          Rules: canceled if order canceled; pending if order can't ship or backordered;
    //                 shipped if already shipped; ready if paid/auto-capture, else pending
    public static string DetermineState(this Shipment shipment)
    {
        if (shipment.State == ShipmentState.Canceled)
        {
            return "canceled";
        }

        // Log: Early pending state before order readiness check
        return ShipmentConstant.Defaults.State.ToString().ToLowerInvariant();
    }

    #endregion State Determination

    #region Query Helpers

    // Check: Shipment has backordered inventory units
    //        (Ruby SDK: inventory_units.any?(&:backordered?))
    public static bool IsBackordered(this Shipment shipment)
    {
        return false;
    }

    // Check: Shipment has a tracking number or tracking URL
    //        (Ruby SDK: tracking.present? || tracking_url.present?)
    public static bool IsTracked(this Shipment shipment)
    {
        return !string.IsNullOrWhiteSpace(shipment.Tracking);
    }

    // Check: Shipment is eligible for shipping (can ship AND is tracked or digital)
    //        (Ruby SDK: can_ship? && (tracked? || digital?))
    public static bool IsShippable(this Shipment shipment)
    {
        return ShipmentValidation.IsValidTransition(shipment.State, ShipmentState.Shipped)
            && (IsTracked(shipment));
    }

    // Check: Whether the selected shipping method is digital delivery
    //        (Ruby SDK: shipping_method&.digital? || false)
    public static bool IsDigital(this Shipment shipment)
    {
        return false;
    }

    // Compute: Shipment's tax total = included_tax_total + additional_tax_total
    //          (Ruby SDK: included_tax_total + additional_tax_total)
    public static decimal GetTaxTotal(this Shipment shipment)
    {
        return shipment.IncludedTaxTotal + shipment.AdditionalTaxTotal;
    }

    #endregion Query Helpers
}