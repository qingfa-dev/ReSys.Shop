==== Order Lifecycle

// Diagram placeholder: Order Lifecycle use case diagram

==== UC-ADM-ORD-01 — View Orders

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ORD-01],
    [*Use Case Name*], [View Orders],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [List orders with filtering by status, date range, and customer; view individual order detail with full transactional context.],
    [*Trigger*], [Administrator navigates to the order management interface.],
    [*Preconditions*], [
      - Administrator is authenticated with order viewing permissions.
    ],
    [*Postconditions*], [
      - Order data displayed with full transactional context including line items, payment state, and shipment state.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the order management interface.
      2. System -- Displays the order list with default sorting (most recent first).
      3. Administrator -- Applies optional filters: order status, date range, customer email.
      4. System -- Refreshes the listing with filtered results and pagination controls.
      5. Administrator -- Selects an individual order to view detail.
      6. System -- Displays the full order detail: line items, pricing breakdown, payment state, shipment state, shipping and billing addresses, and status timeline.
    ],
    [*Alternative Flows*], [
      A1. No orders match the applied filters -- System displays an empty result message with suggestion to broaden the filter criteria.
      A2. Administrator exports the order list -- System generates a downloadable file with the current filtered results.
    ],
    [*Exception Flows*], [
      E1. System fails to retrieve order data -- System displays an error message and offers a retry option.
    ],
    [*Related Requirements*], [ORD-FR-13],
  ),
  caption: [UC-ADM-ORD-01 -- View Orders.],
)

==== UC-ADM-ORD-02 — Update Order

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ORD-02],
    [*Use Case Name*], [Update Order],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Modify order attributes: adjust line items, update shipping and billing addresses, change delivery method.],
    [*Trigger*], [Administrator opens the order edit form from the order detail view.],
    [*Preconditions*], [
      - Administrator is authenticated with order update permissions.
      - The order is in a mutable state.
    ],
    [*Postconditions*], [
      - Order updated and totals recalculated to reflect the changes.
      - Change recorded in the order audit log.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Opens an order from the order listing.
      2. System -- Displays the order detail view.
      3. Administrator -- Initiates the edit action.
      4. System -- Presents the editable order form with current values.
      5. Administrator -- Modifies line items (add, remove, or change quantities), updates shipping or billing addresses, or selects a different shipping method.
      6. Administrator -- Submits the changes.
      7. System -- Validates the modifications (stock availability for changed quantities, address completeness).
      8. System -- Recalculates item totals, shipment total, and order total.
      9. System -- Persists the updated order and logs the change.
      10. System -- Confirms the update.
    ],
    [*Alternative Flows*], [
      A1. Requested quantity exceeds available stock -- System rejects the line item change and displays the maximum available quantity.
      A2. Administrator removes all line items -- System rejects and warns that the order must have at least one line item.
      A3. Administrator changes the shipping address to a zone not served by the current shipping method -- System warns and prompts to select a compatible shipping method.
    ],
    [*Exception Flows*], [
      E1. Order was transitioned to an immutable state by a concurrent operation -- System refreshes the order detail and notifies the administrator that the order is no longer editable.
    ],
    [*Related Requirements*], [ORD-FR-13],
  ),
  caption: [UC-ADM-ORD-02 -- Update Order.],
)

==== UC-ADM-ORD-03 — Approve Order

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ORD-03],
    [*Use Case Name*], [Approve Order],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Approve a pending order for fulfilment after verifying payment and inventory.],
    [*Trigger*], [Administrator selects the approve action on a pending order.],
    [*Preconditions*], [
      - Administrator is authenticated with order approval permissions.
      - The order is in pending state.
      - Payment is verified and captured.
      - Inventory is available for all line items.
    ],
    [*Postconditions*], [
      - Order approved and moved to fulfilment state.
      - Fulfilment process can begin.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Opens a pending order from the order listing.
      2. System -- Displays the order detail including payment status and inventory reservation status.
      3. Administrator -- Verifies payment capture and inventory availability.
      4. Administrator -- Selects the approve action.
      5. System -- Performs final validation of payment state and inventory reservations.
      6. System -- Transitions the order to approved state.
      7. System -- Confirms approval and displays the updated order status.
    ],
    [*Alternative Flows*], [
      A1. Payment has not been captured -- System prevents approval and suggests the administrator capture the payment first.
      A2. Inventory reservation for a line item has expired -- System prevents approval, releases the stale reservation, and notifies the administrator to re-reserve stock.
      A3. Administrator cancels the approval action -- System returns to the order detail view without changes.
    ],
    [*Exception Flows*], [
      E1. Order was modified by a concurrent session -- System detects the conflict, refreshes the order data, and asks the administrator to re-evaluate.
      E2. Payment gateway reports a state mismatch -- System prevents approval and advises the administrator to verify the payment state with the gateway before retrying.
    ],
    [*Related Requirements*], [ORD-FR-13],
  ),
  caption: [UC-ADM-ORD-03 -- Approve Order.],
)

==== UC-ADM-ORD-04 — Complete Order

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ORD-04],
    [*Use Case Name*], [Complete Order],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Mark an order as fulfilled after shipment confirmation.],
    [*Trigger*], [Administrator selects the complete action on an order in fulfilment state.],
    [*Preconditions*], [
      - Administrator is authenticated with order completion permissions.
      - The order is in fulfilment state.
      - Shipment has been dispatched.
    ],
    [*Postconditions*], [
      - Order completed and locked against further modification.
      - Inventory on-hand quantities decremented.
      - Order marked as immutable.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Opens an order in fulfilment state.
      2. System -- Displays the order detail including shipment tracking information.
      3. Administrator -- Verifies that the shipment has been dispatched with valid tracking details.
      4. Administrator -- Selects the complete action.
      5. System -- Displays a confirmation prompt.
      6. Administrator -- Confirms completion.
      7. System -- Decrements on-hand inventory quantities for each line item.
      8. System -- Transitions the order to completed state and marks it as immutable.
      9. System -- Records the completion in the audit log.
      10. System -- Confirms successful completion.
    ],
    [*Alternative Flows*], [
      A1. No tracking number has been assigned -- System warns the administrator but allows completion if shipment is confirmed.
      A2. Administrator attempts to complete an order before shipment -- System warns and recommends completing after dispatch; allows override for hand-delivery or pickup scenarios.
    ],
    [*Exception Flows*], [
      E1. Inventory decrement fails due to a data conflict -- System reports the failure and suggests the administrator verify current stock levels before retrying.
    ],
    [*Related Requirements*], [ORD-FR-13],
  ),
  caption: [UC-ADM-ORD-04 -- Complete Order.],
)

==== UC-ADM-ORD-05 — Cancel Order

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ORD-05],
    [*Use Case Name*], [Cancel Order],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [Cancel an order at any pre-completion stage and release reserved inventory.],
    [*Trigger*], [Administrator selects the cancel action on an order.],
    [*Preconditions*], [
      - Administrator is authenticated with cancellation permissions.
      - The order is in a cancellable state (not completed or already cancelled).
    ],
    [*Postconditions*], [
      - Order cancelled.
      - Reserved inventory returned to availability.
      - Payment voided if not yet captured.
      - Refund issued if payment was captured.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Opens the order detail view.
      2. Administrator -- Selects the cancel action.
      3. System -- Displays a confirmation prompt with a summary of consequences (inventory release, payment action).
      4. Administrator -- Provides a cancellation reason.
      5. Administrator -- Confirms the cancellation.
      6. System -- Releases all reserved inventory back to available stock.
      7. System -- Voids the payment if authorised but not captured, or initiates a refund if already captured.
      8. System -- Transitions the order to cancelled state.
      9. System -- Confirms the cancellation and displays the updated order status.
    ],
    [*Alternative Flows*], [
      A1. Payment void fails (already captured) -- System proceeds with a refund instead and reports the refund transaction to the administrator.
      A2. Refund amount is partial due to gateway fees -- System records the partial refund and displays the refunded amount in the order detail.
      A3. Administrator cancels the operation -- System returns to the order detail view without changes.
    ],
    [*Exception Flows*], [
      E1. Payment gateway is unreachable -- System cancels the order and releases inventory; the payment action is queued for retry and the administrator is notified to verify the payment state when the gateway is operational.
      E2. Concurrent state change detected -- System refreshes the order and notifies the administrator that the order state has changed.
    ],
    [*Related Requirements*], [ORD-FR-07, ORD-FR-13],
  ),
  caption: [UC-ADM-ORD-05 -- Cancel Order.],
)

==== UC-ADM-ORD-06 — Resume Order

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ORD-06],
    [*Use Case Name*], [Resume Order],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Resume a previously paused or stalled order, returning it to the active workflow.],
    [*Trigger*], [Administrator selects the resume action on a paused or stalled order.],
    [*Preconditions*], [
      - Administrator is authenticated with order management permissions.
      - The order is in a paused or stalled state.
    ],
    [*Postconditions*], [
      - Order returned to pending processing state.
      - Order continues through the normal workflow.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Locates a paused or stalled order from the order listing.
      2. System -- Displays the order detail including the reason for the pause or stall.
      3. Administrator -- Resolves the underlying issue (e.g. confirms inventory, verifies payment).
      4. Administrator -- Selects the resume action.
      5. System -- Validates that all prerequisites for processing are met.
      6. System -- Transitions the order back to pending state.
      7. System -- Confirms the resumption and displays the updated order status.
    ],
    [*Alternative Flows*], [
      A1. Prerequisites still not met -- System prevents resumption and displays the specific issues that must be resolved first.
      A2. Underlying issue was a payment hold that has now been released -- System verifies the new payment state and allows resumption.
      A3. Administrator cancels instead of resuming -- System proceeds with the cancel order flow (see UC-ADM-ORD-05).
    ],
    [*Exception Flows*], [
      E1. Order state changed by a concurrent session -- System refreshes the order data and notifies the administrator.
    ],
    [*Related Requirements*], [ORD-FR-13],
  ),
  caption: [UC-ADM-ORD-06 -- Resume Order.],
)
