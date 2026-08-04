#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/customer/uc-0006-track-order.png", width: 100%),
  caption: [Use Case Diagram for UC-0006],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0006*], [*Track Order Status*],
    [Actor], [Customer],
    [Description], [View history and specific status of past orders.],
    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [User is Authenticated.],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Customer accesses "Order History".],
      [2], [System retrieves orders for User ID.],
      [3], [Customer selects Order to view.],
      [4], [System displays status and items.],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [History displayed.],
    ),

    [Related Use Cases], [UC-0002 (Checkout)],
  ),
  caption: [UC-0006: Track Order Status],
)

Post-purchase transparency is provided through the Order Tracking feature. Authenticated customers can access a historical view of their transactions, drilling down into specific orders to view current status (e.g., Pending, Shipped, Delivered) and line item details. This read-only view aggregates data from the Order Processing service to keep the user informed.
