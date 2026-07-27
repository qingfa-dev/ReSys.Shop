==== Order Lifecycle

==== UC-ADM-ORD: Manage Orders

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ORD],
    [*Use Case Name*], [Manage Orders],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [View, modify, and manage the lifecycle of customer orders.],
    [*Trigger*], [Administrator navigates to order management.],
    [*Preconditions*], [
      - Authenticated with order management permissions.
    ],
    [*Postconditions*], [
      - Order state transitions performed. Status logged for audit.
    ],
    [*Main Success Scenario*], [
      *View Orders*
      1. Navigates to order management.
      2. System displays order list sorted by most recent.
      3. Applies optional filters: status, date range, customer email.
      4. Selects an order to view detail with line items, pricing, payment, shipment, addresses, and timeline.
      ,
      *Update Order*
      1. Opens an order and initiates edit.
      2. System presents editable form with current values.
      3. Modifies line items, addresses, or shipping method.
      4. Submits. System validates modifications, recalculates totals, persists, and confirms.
      ,
      *Approve Order*
      1. Opens a pending order and verifies payment capture and inventory availability.
      2. Selects approve. System validates and transitions order to approved. Confirms.
      ,
      *Complete Order*
      1. Opens an order in fulfilment state.
      2. Verifies shipment dispatched.
      3. Selects complete. System displays confirmation.
      4. Confirms. System decrements on-hand inventory, transitions to completed, and logs. Confirms.
      ,
      *Cancel Order*
      1. Opens an order and selects cancel.
      2. System displays confirmation with consequences summary.
      3. Provides cancellation reason and confirms.
      4. System releases reserved inventory, voids or refunds payment, transitions to cancelled. Confirms.
      ,
      *Resume Order*
      1. Locates a paused or stalled order.
      2. Resolves the underlying issue.
      3. Selects resume. System validates prerequisites, transitions back to pending. Confirms.
    ],
    [*Alternative Flows*], [
      A1. No orders match: system displays empty message with suggestion to broaden filters.
      A2. Payment not captured (Approve): system prevents and suggests capturing first.
      A3. Payment gateway unreachable (Cancel): system cancels order, releases inventory, queues payment action.
      A4. Prerequisites not met (Resume): system prevents and displays issues to resolve.
      A5. Concurrent state change: system refreshes and notifies.
    ],
    [*Exception Flows*], [
      E1. Order became immutable concurrently: system refreshes and notifies order is no longer editable.
      E2. Payment gateway state mismatch: system prevents and advises verifying with gateway.
      E3. Inventory decrement data conflict: system reports and suggests verifying stock levels.
    ],
    [*Related Requirements*], [ORD-FR-04, ORD-FR-05, ORD-FR-06, ORD-FR-07, ORD-FR-09, ORD-FR-13],
  ),
    kind: table,
  caption: [Manage Orders.],
)

#figure(
  image(
    "../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-order-lifecycle.png",
    width: 100%
  ),
  caption: [Use case diagram for Order Lifecycle (UC-ADM-ORD, UC-ADM-ORD-ITEMS).],
) <fig-uc-adm-ord-d>

==== UC-ADM-ORD-ITEMS: Manage Order Details

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-ORD-ITEMS],
    [*Use Case Name*], [Manage Order Details],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Manage line items, shipping address, and billing address on existing orders.],
    [*Trigger*], [Administrator opens the order edit form from the order detail view.],
    [*Preconditions*], [
      - Authenticated with order update permissions.
      - Order is in a mutable state.
    ],
    [*Postconditions*], [
      - Order details updated with recalculated totals. Change logged.
    ],
    [*Main Success Scenario*], [
      1. Opens an order from the listing.
      2. System displays order detail.
      3. Initiates the edit action.
      4. System presents the editable form with current values.
      5. Modifies line items (add, update, remove), shipping address, or billing address.
      6. Submits. System validates modifications (stock, address completeness), recalculates totals, persists, and confirms.
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
    kind: table,
  caption: [Manage Order Details.],
)
