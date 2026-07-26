==== Order Lifecycle

// Diagram placeholder: Order Lifecycle use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-ORD-01], [View orders], [Administrator],
    [List orders with filtering by status, date range, and customer. View individual order detail.],
    [Order data displayed with full transactional context.],
    [ORD-FR-05, ORD-FR-06, ORD-FR-13],
    [UC-ADM-ORD-02], [Update order], [Administrator],
    [Modify order attributes: adjust line items, update shipping and billing addresses, change delivery method.],
    [Order updated and totals recalculated to reflect the changes.],
    [ORD-FR-05, ORD-FR-13],
    [UC-ADM-ORD-03], [Approve order], [Administrator],
    [Approve a pending order for fulfilment after verifying payment status and inventory availability.],
    [Order approved and moved to fulfilment queue.],
    [ORD-FR-04, ORD-FR-13],
    [UC-ADM-ORD-04], [Complete order], [Administrator],
    [Mark an order as fulfilled after shipment confirmation.],
    [Order completed and locked against further modification. Inventory on-hand quantities decremented.],
    [ORD-FR-09, ORD-FR-13],
    [UC-ADM-ORD-05], [Cancel order], [Administrator],
    [Cancel an order at any pre-confirmation stage, providing a reason. Release reserved inventory.],
    [Order cancelled. Inventory returned to availability. Payment voided.],
    [ORD-FR-07],
    [UC-ADM-ORD-06], [Resume order], [Administrator],
    [Resume a previously paused or stalled order, returning it to the active workflow.],
    [Order returned to processing state.],
    [ORD-FR-13],
  ),
  caption: [Administrator use cases — Order Lifecycle.],
)
