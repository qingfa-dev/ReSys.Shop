==== Order Lifecycle

// Diagram placeholder: Order Lifecycle use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-ORD-01], [View orders], [Admin], [List orders with filtering by status, date range, and customer; view individual order detail.], [Admin is authenticated with order viewing permissions.], [Order data displayed with full transactional context.],
  [UC-ADM-ORD-02], [Update order], [Admin], [Modify order attributes: adjust line items, update shipping and billing addresses, change delivery method.], [Admin is authenticated with order update permissions. The order is in a mutable state.], [Order updated and totals recalculated to reflect the changes.],
  [UC-ADM-ORD-03], [Approve order], [Admin], [Approve a pending order for fulfilment after verifying payment and inventory.], [Admin is authenticated. The order is pending. Payment is verified and inventory is available.], [Order approved and moved to fulfilment queue.],
  [UC-ADM-ORD-04], [Complete order], [Admin], [Mark an order as fulfilled after shipment confirmation.], [Admin is authenticated. The order is in fulfilment state.], [Order completed and locked against further modification. Inventory on-hand quantities decremented.],
  [UC-ADM-ORD-05], [Cancel order], [Admin], [Cancel an order at any pre-confirmation stage and release reserved inventory.], [Admin is authenticated with cancellation permissions. The order is in a cancellable state.], [Order cancelled. Inventory returned to availability. Payment voided.],
  [UC-ADM-ORD-06], [Resume order], [Admin], [Resume a previously paused or stalled order, returning it to the active workflow.], [Admin is authenticated. The order is in a paused or stalled state.], [Order returned to processing state.],
)
