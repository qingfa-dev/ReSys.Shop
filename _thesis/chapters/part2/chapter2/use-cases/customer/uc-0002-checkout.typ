#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/customer/uc-0002-checkout.png", width: 100%),
  caption: [Use Case Diagram for UC-0002],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0002*], [*Perform Multi-step Checkout*],
    [Actor], [Customer],
    [Description],
    [Finalizes a purchase through a secure process with atomic inventory reservation and payment processing.],

    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Cart contains valid, in-stock items (UC-0005).],
      [-], [User is Authenticated with active session.],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Customer clicks "Proceed to Checkout" from Cart (UC-0005).],
      [2], [System displays Shipping Address selection (UC-0007).],
      [3], [Customer selects/enters Address.],
      [4], [Customer selects Payment Method.],
      [5], [Customer clicks "Place Order".],
      [6], [System starts atomic transaction.],
      [7], [System automatically reserves inventory (UC-0018).],
      [8], [System processes payment via Gateway.],
      [9], [System saves Order and commits transaction.],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Order created with status "Placed" (UC-0006).],
      [-], [Inventory is physically reserved.],
    ),

    [Alternative Sequences],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [A1], [If Out of Stock: Transaction rolls back, user notified.],
      [A2], [If Payment Failed: User prompted to retry.],
      [A3], [If Address Invalid: System blocks progress.],
    ),

    [Related Use Cases], [UC-0005 (Cart), UC-0018 (Stock Reservation)],
  ),
  caption: [UC-0002: Perform Multi-step Checkout],
)

This use case encapsulates the end-to-end purchase flow, transitioning a customer's cart into a confirmed order. It enforces a strict sequence of validation steps, including address selection, payment method configuration, and final review. Crucially, the system employs an atomic transaction across the "Ordering" and "Inventory" domains to ensure stock is reserved exactly when the order is placed, preventing overselling.
