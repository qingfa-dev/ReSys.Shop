#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/admin/uc-0012-inventory.png", width: 100%),
  caption: [Use Case Diagram for UC-0012],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0012*], [*Inventory Management*],
    [Actor], [Administrator],
    [Description], [View real-time physical stock levels across locations and receive visual low-stock indications.],
    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Admin has Inventory permissions.],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Admin navigates to Inventory Dashboard.],
      [2], [System queries Inventory Aggregates (OnHand, Reserved).],
      [3], [System generates Inventory Summary for alerts (UC-0020).],
      [4], [Displays Stock Table with "Available" counts.],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Stock levels displayed accurately.],
    ),

    [Related Use Cases], [UC-0020 (Low Stock Alert)],
  ),
  caption: [UC-0012: Inventory Management],
)

This use case provides a real-time window into the physical stock levels of the warehouse. The Inventory Management interface aggregates data from various stock movements (inbound shipments, sales, returns) to present an accurate count of "On Hand" versus "Available" quantities. It serves as the source of truth for the system's "Available to Promise" (ATP) logic.
