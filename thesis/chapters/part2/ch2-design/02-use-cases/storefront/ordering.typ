==== Cart Management

// Diagram placeholder: Cart Management use case diagram

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
    [UC-STR-ORD-01], [Manage cart], [Customer],
    [Add product variants with desired quantity to the cart. Update quantities or remove items. View cart summary with item totals.],
    [Cart persisted across page navigation. Guest carts survive browser sessions.],
    [ORD-FR-01, ORD-FR-10],
    [UC-STR-ORD-02], [Associate cart with account], [Customer],
    [Upon login or registration, the existing guest cart is promoted to the authenticated user context. Contents are merged without data loss.],
    [Cart associated with user account and available across devices.],
    [ORD-FR-02],
  ),
  caption: [Customer use cases — Cart Management.],
)

==== Checkout Flow

// Diagram placeholder: Checkout Flow use case diagram

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
    [UC-STR-ORD-03], [Select shipping address], [Customer],
    [Select a saved address or enter a new shipping address for the order.],
    [Shipping address set on the order. Shipping zone determined for rate calculation.],
    [ORD-FR-04, PRF-FR-01],
    [UC-STR-ORD-04], [Select shipping method], [Customer],
    [Choose from available delivery methods with calculated rates based on address zone, cart weight, and cart value.],
    [Shipping method and rate applied to the order. Shipment total updated.],
    [ORD-FR-04, ORD-FR-12, SHP-FR-02, SHP-FR-06],
    [UC-STR-ORD-05], [Complete checkout], [Customer],
    [After address and shipping selection, proceed to payment. Enter payment details. Review order summary and confirm.],
    [Order created with unique order number. Inventory reserved for each line item. Payment linked to order. Cart cleared.],
    [ORD-FR-04, ORD-FR-05, ORD-FR-08, ORD-FR-11],
  ),
  caption: [Customer use cases — Checkout Flow.],
)

==== Order History

// Diagram placeholder: Order History use case diagram

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
    [UC-STR-ORD-06], [View order history], [Customer],
    [List past orders with status, date, and total. View individual order detail.],
    [Complete order history visible for authenticated customers.],
    [ORD-FR-14],
    [UC-STR-ORD-07], [Cancel order], [Customer],
    [Cancel a pending order before confirmation. Inventory is released.],
    [Order cancelled. Inventory returned to availability. Payment voided.],
    [ORD-FR-07],
  ),
  caption: [Customer use cases — Order History.],
)
