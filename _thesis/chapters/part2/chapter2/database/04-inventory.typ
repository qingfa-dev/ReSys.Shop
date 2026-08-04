=== Inventory Context

The *Inventory Context* manages the physical availability of goods across the supply chain. Unlike simple "counter" systems, this implementation uses an *Event Sourcing-lite* architecture to provide ERP-grade auditability and multi-warehouse support.

==== Warehousing
The system supports multiple physical locations (Warehouses, Stores, Dropshippers). Each location operates independently, allowing for complex fulfillment strategies (e.g., "Ship from nearest store").

#figure(
  placement: none,
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [Name], [VARCHAR(100)], [Public name (e.g. 'New York Fulfillment Center').],
    [3], [Code], [VARCHAR(20)], [Internal logistics code (e.g. 'NYC-01'). Unique and utilized on shipping labels.],
    [4], [IsActive], [BOOL], [If false, no new orders are routed here.],
    [5], [IsDefault], [BOOL], [If true, this location acts as the fallback for resolving stock queries.],
    [6], [Presentation], [VARCHAR(100)], [Customer-facing localized name.],
    [7], [Type], [INT], [0=Warehouse, 1=RetailStore, 2=Transit. Determines fulfillment capabilities.],
    [8], [AddressId], [UUID], [Link to the physical address for shipping calculations.],
    [9], [IsDeleted], [BOOL], [Soft delete flag to preserve historical data.],
  ),
  caption: [StockLocations table],
)

==== Inventory State (Read Model)
The `StockItems` table represents the *current* state of inventory. It is a cached projection designed for high-speed reads during the checkout process (e.g., checking "Is X in stock?" takes milliseconds).

- *Optimistic Concurrency:* Because inventory is a high-contention resource (thousands of users buying the same item on Black Friday), this table relies heavily on the `RowVersion` (xmin) column. If two processes try to decrement stock simultaneously, the database ensures serialized access, preventing "overselling".

#figure(
  placement: none,
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [VariantId], [UUID], [The product variant being stored.],
    [3], [StockLocationId], [UUID], [The warehouse where it is stored.],
    [4], [Sku], [VARCHAR(100)], [Denormalized SKU for quicker lookup during scans.],
    [5], [QuantityOnHand], [INT], [Physical units currently on the shelf involved in availability calculations.],
    [6], [QuantityReserved], [INT], [Units "soft-locked" by active carts but not yet shipped.],
    [7], [Backorderable], [BOOL], [If true, allows sales even when `QuantityOnHand` is zero.],
    [8], [BackorderLimit], [INT], [Safety floor for negative inventory (e.g., -100).],
    [9], [RowVersion], [BYTEA], [PostgreSQL System Column (xmin). Used to detect concurrent modifications.],
    [10], [IsDeleted], [BOOL], [Soft delete flag.],
  ),
  caption: [StockItems table],
)

==== Inventory Ledger (Write Model / Source of Truth)
The `StockMovements` table is an *Immutable Ledger*. It is the single source of truth for *why* inventory counts changed.

- *Traceability:* You never simply "set stock to 5". You must insert a movement record: "Received 5 units from Supplier PO#123".
- *Reversibility:* Mistakes (e.g., accidental adjustment) are corrected by inserting an *inverse* movement, preserving the history of the error.
- *Valuation:* It tracks `UnitCost` at the time of movement, enabling precise "Cost of Goods Sold" (COGS) reporting (FIFO/LIFO capability).

#figure(
  placement: none,
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [StockItemId], [UUID], [Link to the aggregate stock record.],
    [3], [Type], [INT], [Discriminator: Adjustment, Sale, Receipt, Return, StockCount.],
    [4], [Quantity], [INT], [The delta value (positive for in-flow, negative for out-flow).],
    [5], [BalanceBefore], [INT], [Audit Snapshot: Count before this tx.],
    [6], [BalanceAfter], [INT], [Audit Snapshot: Count after this tx. Ensures mathematical integrity.],
    [7], [UnitCost], [DECIMAL(18,2)], [Financial value of this specific batch of items.],
    [8], [Reason], [VARCHAR(255)], [User-provided explanation (e.g. 'Found during stock take').],
    [9], [Reference], [VARCHAR(100)], [External document ID (Order Number, RMA Number, Purchase Order).],
    [10], [CreatedAt], [TIMESTAMP], [The exact moment the stock moved. Immutable.],
  ),
  caption: [StockMovements table],
)

