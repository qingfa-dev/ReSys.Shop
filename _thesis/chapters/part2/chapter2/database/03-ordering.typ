=== Order Context

The *Order Context* is the transactional heart of the system. It orchestrates the complex lifecycle of converting a customer's shopping cart into a legally binding sales contract and ensuring its fulfillment.

==== Order Aggregate
The `Order` entity functions as a strict *Finite State Machine (FSM)*. It progresses through defined stages (Cart $\to$ Address $\to$ Payment $\to$ Complete) to ensure data integrity.

- *Financial Precision:* Unlike the Catalog (which acts as a "Menu"), the Order (which acts as the "Receipt") stores all monetary values in `BIGINT` cents. This avoids accumulating floating-point rounding errors during tax calculation and ensures exact reconciliation with payment gateways (Stripe/PayPal), which also process in minor units.
- *Snapshot Pattern:* Changes to catalog prices *must not* affect past orders. Therefore, `LineItems` do not merely link to `Products`; they copy (snapshot) the price, name, and SKU at the moment of purchase.

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [Number], [VARCHAR(50)], [Human-readable reference (e.g., 'ORD-2023-1001').],
    [3], [State], [INT], [Enum representing the FSM state: 0=Cart, 1=Address, 2=Payment, 3=Complete, 4=Canceled.],
    [4], [ItemTotalCents], [BIGINT], [Subtotal of all merchandise.],
    [5], [ShipmentTotalCents], [BIGINT], [Subtotal of all shipping/handling costs.],
    [6], [TotalCents], [BIGINT], [The final amount to be charged to the customer. Aggregated from items + shipping.],
    [7], [UserId], [UUID], [Link to registered user. Nullable to support 'Guest Checkout'.],
    [8], [SessionId], [VARCHAR(100)], [Cookie-based ID to track anonymous carts before login.],
    [9], [CompletedAt], [TIMESTAMP], [The legal timestamp of the contract finalization.],
    [10], [CanceledAt], [TIMESTAMP], [Timestamp if the order was aborted.],
    [11], [ShipAddressId], [UUID], [Snapshot of where the goods should go.],
    [12], [BillAddressId], [UUID], [Snapshot of the billing verification address.],
    [13], [Currency], [VARCHAR(3)], [ISO 4217 code (e.g. 'USD').],
  ),
  caption: [Orders table],
)

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [OrderId], [UUID], [Foreign Key to Parent Order.],
    [3], [VariantId], [UUID], [Link to the source product (for restocking).],
    [4], [Quantity], [INT], [Number of units purchased.],
    [5], [PriceCents], [BIGINT], [Fixed price per unit at checkout time. IMMUTABLE.],
    [6], [SnapshotName], [VARCHAR(255)], [Fixed product name at checkout time. IMMUTABLE.],
  ),
  caption: [LineItems table],
)

==== Fulfillment (Shipments & Granular Inventory)
The system decouples "Selling" (Order) from "Shipping" (Shipment). This supports split shipments (e.g., items coming from different warehouses).

- *Granular Tracking:* A key innovation is the `InventoryUnit` table. If a user buys 3 iPhones, the generic `LineItem` says "Qty: 3". However, the system generates 3 distinct `InventoryUnit` records. This allows tracking the specific Serial Number of *each* phone sent, enabling precise warranty support and returns management.

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [OrderId], [UUID], [Parent Order.],
    [3], [StockLocationId], [UUID], [The Warehouse fulfilling this package.],
    [4], [TrackingNumber], [VARCHAR(100)], [Carrier tracking code (e.g. UPS/FedEx).],
    [5], [State], [INT], [Logistics State: Pending $\to$ Picked $\to$ Packed $\to$ Shipped.],
    [6], [CostCents], [BIGINT], [The actual cost incurred to ship this package.],
    [7], [PickedAt], [TIMESTAMP], [Audit: When items left the shelf.],
    [8], [PackedAt], [TIMESTAMP], [Audit: When items were boxed.],
    [9], [ShippedAt], [TIMESTAMP], [Audit: When carrier accepted the package.],
  ),
  caption: [Shipments table],
)

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [OrderId], [UUID], [The Order requiring this item.],
    [3], [VariantId], [UUID], [The SKU being tracked.],
    [4], [ShipmentId], [UUID], [The specific box this unit was packed into.],
    [5], [State], [INT], [Unit Status: Pending, OnHand (Reserved/Allocated), Shipped, Returned.],
    [6], [SerialNumber], [VARCHAR(50)], [Unique hardware ID scanned during packing.],
  ),
  caption: [InventoryUnits table (Granular Tracking)],
)

==== Financial Reconciliation (Payments)
Payments are tracked separately to allow for complex scenarios like "Authorization & Capture" (reserving funds before shipping) or split payments (Gift Card + Credit Card).

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [OrderId], [UUID], [Parent Order.],
    [3], [AmountCents], [BIGINT], [The portion of the total covered by this transaction.],
    [4], [State], [INT], [Payment State: Pending $\to$ Authorized $\to$ Captured.],
    [5], [Provider], [VARCHAR(50)], [Payment Processor (e.g. 'Stripe').],
    [6], [TransactionId], [VARCHAR(100)], [Gateway's internal reference ID.],
    [7], [GatewayErrorCode], [VARCHAR(50)], [Raw error code for debugging failed transactions.],
    [8], [AuthorizedAt], [TIMESTAMP], [Timestamp of funds reservation.],
    [9], [CapturedAt], [TIMESTAMP], [Timestamp of funds transfer.],
  ),
  caption: [Payments table],
)

==== Audit Trail (History)
To comply with Enterprise requirements, every significant action is logged. This provides a "Flight Recorder" for the order.

#figure(
  table(
    columns: (auto, auto, auto, 1fr),
    stroke: 0.5pt,
    [*No*], [*Field*], [*Type*], [*Description*],
    [1], [Id], [UUID], [Primary Key],
    [2], [OrderId], [UUID], [Parent Order.],
    [3], [Description], [TEXT], [Human-readable log (e.g. 'Order placed by User').],
    [4], [FromState], [VARCHAR(50)], [State transition start.],
    [5], [ToState], [VARCHAR(50)], [State transition end.],
    [6], [TriggeredBy], [VARCHAR(100)], [The user or system component responsible.],
    [7], [Context], [JSONB], [Structured metadata (e.g. Webhook payload) for debugging.],
  ),
  caption: [OrderHistory table],
)

