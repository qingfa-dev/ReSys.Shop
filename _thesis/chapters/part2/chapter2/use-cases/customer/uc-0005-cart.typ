#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/customer/uc-0005-cart.png", width: 100%),
  caption: [Use Case Diagram for UC-0005],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0005*], [*Manage Shopping Cart*],
    [Actor], [Customer],
    [Description], [Add, update, or remove items in the shopping cart.],
    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Product variant is selected.],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Customer clicks "Add to Cart".],
      [2], [System updates Cart in Database/Session.],
      [3], [System recalculates totals.],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Cart updated.],
    ),

    [Related Use Cases], [UC-0002 (Checkout)],
  ),
  caption: [UC-0005: Manage Shopping Cart],
)

This use case governs the temporary storage of products selected for purchase. It acts as a staging area where users can adjust quantities, remove items, or review their potential purchase. The cart state is maintained across sessions (for authenticated users), persisting data to ensuring a continuous shopping experience even if the user navigates away or switches devices.
