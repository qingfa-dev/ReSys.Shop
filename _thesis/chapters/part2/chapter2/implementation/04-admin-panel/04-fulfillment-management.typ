===== 2. Fulfillment Management (Ordering Context)
The *Order Processing Interface* provides the primary mechanism for *Aggregate Lifecycle Management*. It guides the operator through the valid state transitions defined by the Domain Model, effectively functioning as a "Single Pane of Glass" for the shipping lifecycle.

- *The Interface:* The layout is divided into three functional zones to reduce cognitive switching: *Customer Info* (Identity), *Line Items* (Catalog), and *Fulfillment Controls* (Ordering).
- *The Flow:*
  1. *Validation:* Operator clicks "Ship". UI validates tracking number format.
  2. *Command:* Frontend sends `ShipOrderCommand` payload.
  3. *Consistency:* Backend checks `Inventory.StockLevel`. If valid, commits Transaction and emits `OrderShippedEvent`.
  4. *Feedback:* UI updates status to "Shipped" via Optimistic Concurrency.

// TODO: [Implementation] Add Order Detail View screenshot.
// #figure(
//   figure-placeholder("Order Processing Interface"),
//   caption: [Order Detail View showing the lifecycle controls for the Order Aggregate.],
// )

The Order Detail View functions as the command center for the *Order Aggregate*, utilizing a "Three-Zone Layout" that directly mirrors the backend domain boundaries. This organization enables the operational workflow depicted in @fig:sq-0014-fulfillment:

- *Zone 1: Identity Context (Left):* Displays immutable Customer snapshot data.
- *Zone 2: Catalog Context (Center):* Visualizes the Line Items, linking directly to Product definitions.
- *Zone 3: Ordering Context (Right):* The *Command Control Panel*.
  - *Pattern:* UI buttons map 1:1 to CQRS Commands (e.g., `ShipOrder`, `CancelOrder`).
  - *Safety:* Controls are defensively disabled based on the Aggregate's state machine. For example, once the `OrderShippedEvent` is emitted (Step 3 in @fig:sq-0014-fulfillment), the "Ship" action is permanently locked, enforcing domain invariants at the glass level.


The logistics flow manages the transition from order confirmation to physical delivery. It involves updating the order aggregate state, selecting appropriate fulfillment providers, and generating tracking information, all while maintaining strict consistency with the inventory levels.

*Detailed Fulfillment Workflow (UC-0013):*
1. *Order Selection:* The operator identifies orders in the `Placed` or `Paid` state from the centralized management grid.
2. *Resource Allocation:* When the order is moved to `Processing`, the system confirms the shipment carrier and generates a unique `TrackingNumber`.
3. *Physical-to-Digital Sync:* The `ShipOrder` command is executed. This updates the internal state of the Order Aggregate to `Shipped` and triggers an asynchronous notification to the customer.
4. *Final Stock Reconciliation:* The reserved stock is officially marked as "Deducted" from the physical inventory records, closing the transaction loop.

*Shipment Command Chain:*
- *UI Trigger (Optimistic Guard):* The "Ship Order" button is the primary operator action. When clicked, the button serves an `isProcessing` state (Spinner) and prevents broken clicks.
- *Sequence Flow:* @fig:sq-0014-fulfillment maps this action to the `ShipOrderCommand`. The handler performs a rigid check against the `Inventory` service to ensure stock is still reserved before committing the status change to `Shipped`. If the reservation has expired, the command is rejected, and the UI displays a "Stock Expired" error, forcing a page refresh.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/admin/sq-0014-fulfillment.png", width: 60%),
  caption: [Shipment Processing: The workflow for assigning carriers and generating tracking numbers (UC-0014).],
) <fig:sq-0014-fulfillment>
