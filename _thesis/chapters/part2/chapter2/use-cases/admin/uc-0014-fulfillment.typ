#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/admin/uc-0014-fulfillment.png", width: 100%),
  caption: [Use Case Diagram for UC-0014],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0014*], [*Order Fulfillment*],
    [Actor], [Administrator],
    [Description], [Process orders, select shipment origin, and create shipments, updating the stock ledger.],
    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Order is in "placed" state (UC-0002).],
      [-], [Inventory is reserved (UC-0018).],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Admin views Pending Orders.],
      [2], [Selects Order and clicks "Ship".],
      [3], [System uses Logic to assign Warehouse.],
      [4], [System creates Shipment Record.],
      [5], [System records Stock Out Movement (Reserved -> Shipped).],
      [6], [System updates Order Status to "Shipped".],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Shipment created.],
      [-], [Stock physically deducted.],
    ),

    [Related Use Cases], [UC-0006 (Track Order), UC-0019 (Fulfillment Logic)],
  ),
  caption: [UC-0014: Order Fulfillment],
)

Order Fulfillment covers the operational workflow of picking, packing, and shipping customer orders. This process transforms a "Reserved" inventory item into a "Shipped" item, decrementing the physical stock ledger. The system automates the selection of the optimal fulfillment center (if multiple exist) and updates the order status to keep the customer informed.
