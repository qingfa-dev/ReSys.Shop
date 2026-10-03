=== Domain-Driven Design (DDD)

An Aggregate is a cluster of associated objects that we treat as a unit for the purpose of data changes.

#rotate(-90deg, reflow: true)[
  #figure(
    image(
      "../../../../images/diagrams/02-system-architecture/sys-04-domain-model.png",
      width: 100%,
      height: 90%,
      fit: "contain",
    ),
    caption: [Domain Model: Vertical Slice Architecture showing Aggregate Roots and Bounded Contexts.],
  )
]

#figure(
  table(
    columns: (auto, 4fr, 1fr),
    stroke: 0.5pt,
    align: (left, left, left),
    [*Aggregate Root*], [*Child Entities*], [*Key Responsibility*],

    [*Product*],
    [`Variant`, `ProductImage`, `ProductProperty`, `Classification`],
    [Catalog management with multi-variant support, image library, dynamic properties, and taxonomy classification.],

    [*Order*],
    [`LineItem`, `Payment`, `Shipment`, `InventoryUnit`, `OrderHistory`],
    [Order lifecycle from cart to fulfillment with payment processing, shipping tracking, and complete audit trail.],

    [*StockItem*],
    [`StockMovement`, `InventoryUnit` (shared)],
    [Physical and logical inventory with immutable ledger, reservation tracking, and backorder support.],

    [*User*],
    [`UserAddress`, `UserRole`, `RefreshToken`, `CustomerProfile`, `StaffProfile`],
    [Identity and access management with role-based permissions, session management, and extensible profiles.],

    [*StockLocation*],
    [`StockItem` (owns)],
    [Warehouse and retail location management for multi-site inventory distribution.],

    [*Role*], [`UserRole` (join)], [Permission grouping and role-based access control (RBAC).],
  ),
  caption: [Domain Aggregates and Bounded Contexts],
)
