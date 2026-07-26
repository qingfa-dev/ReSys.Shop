==== Cart Management
// Diagram placeholder for Cart Management

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-STR-ORD-01], [Manage cart], [Customer],
    [Create a new cart or access the current session cart. Add product variants with desired quantity. Update item quantities or remove items. View cart summary with item totals.],
    [Cart persisted across page navigation. Guest carts survive browser sessions via signed cookie.],
    [ORD-FR-01, ORD-FR-10],
    [UC-STR-ORD-02], [Associate cart with account], [Customer],
    [Upon login or registration, the existing guest cart is promoted to the authenticated user context. Cart contents are merged without data loss.],
    [Cart now associated with user account; available across devices via authentication.],
    [ORD-FR-02],
  ),
  caption: [Customer use cases — Cart Management.],
)

==== Checkout Flow
// Diagram placeholder for Checkout Flow

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-STR-ORD-03], [Select shipping address], [Customer],
    [Select a saved address or enter a new shipping address. System validates address completeness and zone applicability.],
    [Shipping address set on the order; shipping zone determined for rate calculation.],
    [ORD-FR-04, PRF-FR-01],
    [UC-STR-ORD-04], [Select shipping method], [Customer],
    [Choose from available delivery methods with calculated rates based on address zone, cart weight, and cart value.],
    [Shipping method and rate applied to the order; shipment total updated.],
    [ORD-FR-04, ORD-FR-12, SHP-FR-02, SHP-FR-06],
    [UC-STR-ORD-05], [Complete checkout], [Customer],
    [After address and shipping method selection, proceed to payment. Select payment method or enter new payment details. Review order summary (items, adjustments, shipping, total). Confirm order. System: validates stock availability, processes payment intent, creates order record, reserves inventory, clears cart.],
    [Order created with unique order number. Inventory reserved for each line item. Payment intent linked to order. Cart cleared.],
    [ORD-FR-04, ORD-FR-05, ORD-FR-08, ORD-FR-11, NFR-01],
  ),
  caption: [Customer use cases — Checkout Flow.],
)

==== Order History
// Diagram placeholder for Order History

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-STR-ORD-06], [View order history], [Customer],
    [List past orders with status, date, and total. Drill into order detail to view line items, payment state, shipment tracking.],
    [Complete order history visible for authenticated customers.],
    [ORD-FR-14],
    [UC-STR-ORD-07], [Cancel order], [Customer],
    [Cancel a pending order before confirmation. System releases reserved inventory and voids the payment intent.],
    [Order cancelled; inventory returned to availability; payment voided.],
    [ORD-FR-07],
  ),
  caption: [Customer use cases — Order History.],
)
