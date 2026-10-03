===== 1. Business Intelligence (Analytics Context)
The *Executive Dashboard* acts as the system's *Composition Layer*, serving as the visual aggregation root for business performance. It is not merely a "view" but a real-time console that composes independent widgets from distinct contexts (Sales, Operations, System Health).

- *Behind the Scenes:* The backend exposes a specialized `GetDashboardMetrics` endpoint that executes parallelized `COUNT/SUM` queries across Read Replicas. This separation ensures that heavy analytical loads do not lock the transactional write database used for order processing.
- *Flow:* Manager logs in $\to$ Dashboard requests Aggregates $\to$ System renders "Revenue (Sales)" and "Pending Shipments (Fulfillment)" side-by-side.

#figure(
  placement: none,
  image("../../../../../images/ui/admin/ui-admin-dashboard.png", width: 100%),
  caption: [Executive Dashboard Composition: Multi-context aggregation of business and system performance metrics.],
)

*Data Aggregation Pipeline:*
- *UI Interaction (Composability):* The Dashboard is a composition of independent widgets. When the page loads, the layout immediately renders *Skeleton Placeholders* for "Revenue", "Orders", and "Inventory". Each widget triggers its own isolated data fetch.
- *Sequence Flow:* @fig:sq-0013-analytics illustrates the backend response. Queries are routed to *Read Replicas* to avoid contending with the Transactional Log (WAL) of the primary node. This pattern ensures that heavy analytical `GROUP BY` operations do not impact the latency of the checkout write-channel.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/admin/sq-0013-analytics.png", width: 80%),
  caption: [Sales Analytics Data Flow: Aggregating order metrics from read replicas for dashboard visualization (UC-0013).],
) <fig:sq-0013-analytics>

Inventory monitoring utilizes real-time stock aggregation logic. The system tracks on-hand, reserved, and shipped quantities across multiple variants, ensuring that the administrative interface provides an accurate reflection of the physical stock status even during high-concurrency reservation events.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/admin/sq-0012-inventory.png", width: 70%),
  caption: [Inventory Monitor: Real-time tracking of stock levels and reservations (UC-0012).],
) <fig:sq-0012>


// TODO: [Implementation] Add Inventory Monitor screenshot.
// #figure(
//   figure-placeholder("UI Screenshot: Inventory Monitor Grid (UC-0012)"),
//   caption: [Inventory Management UI: Real-time grid showing stock levels and reservation status per SKU.],
// )

*Real-Time Inventory Flow:*
- *UI Presentation (Live Grid):* The Inventory Monitor is a specialized Data Grid that uses distinct visual indicators (Green/Yellow/Red) to represent stock health. Crucially, it listens for `InventoryChanged` events. When a packet arrives, the specific SKU row momentarily "flashes" (via CSS class toggle) to alert the operator of activity.
- *Sequence Flow:* This component maintains a persistent WebSocket connection to the `InventoryHub`. As depicted in @fig:sq-0012, whenever a `StockReservedEvent` is processed by the backend, a signal is pushed to connected clients. This "Push-over-Poll" architecture ensures that operators are seeing data that is at most milliseconds old (Real-time).
