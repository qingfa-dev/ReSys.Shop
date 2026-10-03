#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/system/uc-0017-reservations.png", width: 100%),
  caption: [Use Case Diagram for UC-0017],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0017*], [*Automatic Stock Reservation*],
    [Actor], [System (Transaction)],
    [Description], [Prevent overselling by atomically reserving inventory during the checkout process.],
    [Trigger], [Checkout Confirmation (UC-0002).],
    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Transaction begins.],
      [2], [Inventory Service checks availability (OnHand - Reserved) (UC-0012).],
      [3], [If sufficient: Increments `Reserved` count.],
      [4], [Updates Stock Record.],
      [5], [Commit Transaction.],
    ),

    [Alternative Sequences],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [A1], [If insufficient stock: Throw Denial Exception (Rollback Transaction).],
    ),

    [Related Use Cases], [UC-0002 (Checkout)],
  ),
  caption: [UC-0017: Automatic Stock Reservation],
)

Automatic Stock Reservation is a critical transactional mechanism that prevents overselling in a high-concurrency e-commerce environment. When a customer places an order (UC-0002), this process atomically locks the required quantity of inventory. If the stock is insufficient, the entire transaction is rolled back, ensuring data consistency between the Order and Inventory domains.
