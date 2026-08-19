==== Order Lifecycle

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-order-lifecycle.png",
    width: 100%
  ),
  caption: [Use case diagram for Order Lifecycle (UC-ADM-ORD, UC-ADM-ORD-ITEMS).],
) <fig-uc-adm-ord-d>

==== UC-ADM-ORD: Manage Orders

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-ORD — Manage Orders],
    [*Actor*], [Administrator],
    [*Support*], [Payment Gateway],
    [*Goal*], [View, modify, and manage the lifecycle of customer orders.],
    [*Pre/Post*], [
      Pre: authenticated with order management permissions.
      Post: order state transitions performed; status logged for audit.
    ],
    [*Scenario*], [
      *View Orders*
      + Navigates to order management.
      + System displays order list sorted by most recent.
      + Applies optional filters (status, date range, customer email).
      + Selects order to view detail with line items, pricing, payment, shipment, addresses, timeline.
      ,
      *Update Order*
      + Opens order, initiates edit.
      + System presents editable form with current values.
      + Modifies line items, addresses, or shipping method.
      + Submits; system validates modifications, recalculates totals, persists, confirms.
      ,
      *Approve Order*
      + Opens pending order, verifies payment capture and inventory availability.
      + Selects approve; system validates, transitions order to approved, confirms.
      ,
      *Complete Order*
      + Opens order in fulfilment state.
      + Verifies shipment dispatched.
      + Selects complete.
      + System displays confirmation.
      + Confirms; system decrements on-hand inventory, transitions to completed, logs, confirms.
      ,
      *Cancel Order*
      + Opens order, selects cancel.
      + System displays confirmation with consequences summary.
      + Provides cancellation reason, confirms.
      + System releases reserved inventory, voids or refunds payment, transitions to cancelled, confirms.
      ,
      *Resume Order*
      + Locates paused or stalled order.
      + Resolves underlying issue.
      + Selects resume; system validates prerequisites, transitions back to pending, confirms.
      ,
    ],
    [*Alternatives*], [
      + A1. No orders match → system displays empty message, suggests broadening filters.
      + A2. Payment not captured (Approve) → system prevents, suggests capturing first.
      + A3. Payment gateway unreachable (Cancel) → system cancels order, releases inventory, queues payment action.
      + A4. Prerequisites not met (Resume) → system prevents, displays issues to resolve.
      + A5. Concurrent state change → system refreshes, notifies.
    ],
    [*Exceptions*], [
      + E1. Order became immutable concurrently → system refreshes, notifies order is no longer editable.
      + E2. Payment gateway state mismatch → system prevents, advises verifying with gateway.
      + E3. Inventory decrement data conflict → system reports, suggests verifying stock levels.
    ],
    [*Requirements*], [ORD-FR-04, ORD-FR-05, ORD-FR-06, ORD-FR-07, ORD-FR-09, ORD-FR-13],
  ),
    kind: table,
  caption: [Manage Orders.],
)

==== UC-ADM-ORD-ITEMS: Manage Order Details

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-ORD-ITEMS — Manage Order Details],
    [*Actor*], [Administrator],
    [*Goal*], [Manage line items, shipping address, and billing address on existing orders.],
    [*Pre/Post*], [
      Pre: authenticated with order update permissions; order is in a mutable state.
      Post: order details updated with recalculated totals; change logged.
    ],
    [*Scenario*], [
      + Opens order from listing.
      + System displays order detail.
      + Initiates edit action.
      + System presents editable form with current values.
      + Modifies line items (add, update, remove), shipping address, or billing address.
      + Submits; system validates modifications (stock, address completeness), recalculates totals, persists, confirms.
    ],
    [*Alternatives*], [
      + A1. Quantity exceeds stock → system rejects, shows max available.
      + A2. All line items removed → system rejects, warns at least one is required.
      + A3. Address incompatible with shipping method → system warns, prompts method change.
    ],
    [*Exceptions*], [
      + E1. Order became immutable concurrently → system refreshes, notifies order is no longer editable.
    ],
    [*Requirements*], [ORD-FR-13],
  ),
    kind: table,
  caption: [Manage Order Details.],
)
