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
    [*Goal*], [View orders with filtering by status, date, and customer; inspect full order detail.],
    [*Trigger*], [Administrator navigates to order management.],
    [*Preconditions*], [
      - Authenticated with order viewing permissions.
    ],
    [*Postconditions*], [
      - Order data displayed with transactional context.
    ],
    [*Main Success Scenario*], [
      1. Navigates to order management.
      2. System displays order list sorted by most recent.
      3. Applies optional filters: status, date range, customer email.
      4. System refreshes listing with pagination.
      5. Selects an order to view detail.
      6. System displays full order detail: line items, pricing, payment state, shipment state, addresses, and status timeline.
    ],
    [*Alternative Flows*], [
      A1. No orders match: system displays empty message with suggestion to broaden filters.
      A2. Exports list: system generates downloadable file of filtered results.
    ],
    [*Exception Flows*], [
      E1. Retrieval failure: system displays error and offers retry.
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
    [*Goal*], [Modify line items, addresses, or delivery method on an editable order.],
    [*Trigger*], [Administrator opens the order edit form from the order detail view.],
    [*Preconditions*], [
      - Authenticated with order update permissions.
      - Order is in a mutable state.
    ],
    [*Postconditions*], [
      - Order updated with recalculated totals. Change logged.
    ],
    [*Main Success Scenario*], [
      1. Opens an order from the listing.
      2. System displays order detail.
      3. Initiates the edit action.
      4. System presents the editable form with current values.
      5. Modifies line items, addresses, or shipping method.
      6. Submits the changes.
      7. System validates modifications (stock, address completeness).
      8. System recalculates totals.
      9. System persists the updated order and logs the change.
      10. System confirms the update.
    ],
    [*Alternative Flows*], [
      A1. Quantity exceeds stock: system rejects and shows max available.
      A2. All line items removed: system rejects and warns at least one is required.
      A3. Address incompatible with shipping method: system warns and prompts method change.
    ],
    [*Exception Flows*], [
      E1. Order became immutable concurrently: system refreshes and notifies order is no longer editable.
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
      - Authenticated with order approval permissions.
      - Order is pending.
      - Payment verified and captured.
      - Inventory available.
    ],
    [*Postconditions*], [
      - Order approved and moved to fulfilment.
    ],
    [*Main Success Scenario*], [
      1. Opens a pending order.
      2. System displays order detail with payment and inventory status.
      3. Verifies payment capture and inventory availability.
      4. Selects the approve action.
      5. System performs final validation of payment and inventory.
      6. System transitions order to approved state.
      7. System confirms approval and displays updated status.
    ],
    [*Alternative Flows*], [
      A1. Payment not captured: system prevents and suggests capturing first.
      A2. Inventory reservation expired: system prevents, releases stale reservation, notifies to re-reserve.
      A3. Cancels approval: system returns to order detail without changes.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system refreshes and asks to re-evaluate.
      E2. Payment gateway state mismatch: system prevents and advises verifying with gateway.
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
    [*Trigger*], [Administrator selects the complete action on an order in fulfilment.],
    [*Preconditions*], [
      - Authenticated with order completion permissions.
      - Order is in fulfilment state.
      - Shipment dispatched.
    ],
    [*Postconditions*], [
      - Order completed and immutable. Inventory decremented.
    ],
    [*Main Success Scenario*], [
      1. Opens an order in fulfilment state.
      2. System displays order detail with tracking information.
      3. Verifies shipment dispatched with tracking details.
      4. Selects the complete action.
      5. System displays confirmation prompt.
      6. Confirms completion.
      7. System decrements on-hand inventory for each line item.
      8. System transitions order to completed and marks as immutable.
      9. System records completion in audit log.
      10. System confirms completion.
    ],
    [*Alternative Flows*], [
      A1. No tracking number: system warns but allows if shipment confirmed.
      A2. Complete before shipment: system warns and recommends after dispatch; allows override.
    ],
    [*Exception Flows*], [
      E1. Inventory decrement data conflict: system reports and suggests verifying stock levels.
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
    [*Goal*], [Cancel an order and release reserved inventory.],
    [*Trigger*], [Administrator selects the cancel action on an order.],
    [*Preconditions*], [
      - Authenticated with cancellation permissions.
      - Order is cancellable (not completed or already cancelled).
    ],
    [*Postconditions*], [
      - Order cancelled. Inventory released. Payment voided or refunded.
    ],
    [*Main Success Scenario*], [
      1. Opens the order detail view.
      2. Selects the cancel action.
      3. System displays confirmation with consequences summary.
      4. Provides a cancellation reason.
      5. Confirms the cancellation.
      6. System releases all reserved inventory.
      7. System voids payment if uncaptured or initiates refund if captured.
      8. System transitions order to cancelled state.
      9. System confirms cancellation and displays updated status.
    ],
    [*Alternative Flows*], [
      A1. Void fails (already captured): system proceeds with refund.
      A2. Partial refund due to gateway fees: system records partial refund.
      A3. Cancels operation: system returns to order detail without changes.
    ],
    [*Exception Flows*], [
      E1. Payment gateway unreachable: system cancels order, releases inventory, queues payment action, notifies to verify.
      E2. Concurrent state change: system refreshes and notifies.
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
    [*Goal*], [Resume a paused or stalled order, returning it to active workflow.],
    [*Trigger*], [Administrator selects the resume action on a paused or stalled order.],
    [*Preconditions*], [
      - Authenticated with order management permissions.
      - Order is paused or stalled.
    ],
    [*Postconditions*], [
      - Order returned to pending state.
    ],
    [*Main Success Scenario*], [
      1. Locates a paused or stalled order.
      2. System displays order detail with pause/stall reason.
      3. Resolves the underlying issue.
      4. Selects the resume action.
      5. System validates prerequisites are met.
      6. System transitions order back to pending state.
      7. System confirms resumption and displays updated status.
    ],
    [*Alternative Flows*], [
      A1. Prerequisites not met: system prevents and displays issues to resolve.
      A2. Payment hold released: system verifies and allows.
      A3. Cancels instead: proceeds with cancel order flow (UC-ADM-ORD-05).
    ],
    [*Exception Flows*], [
      E1. Concurrent state change: system refreshes and notifies.
    ],
    [*Related Requirements*], [ORD-FR-13],
  ),
  caption: [UC-ADM-ORD-06 -- Resume Order.],
)
