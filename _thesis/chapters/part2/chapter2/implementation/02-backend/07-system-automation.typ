===== System Automation (Background Services)
The backend runs hosted services for critical maintenance tasks.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/system/sq-0019-background-jobs.png", width: 60%),
  caption: [Low Stock Indicator Sequence: Background aggregation of inventory levels to trigger operational alerts (UC-0019).],
) <fig:sq-0019>

// TODO: [Implementation] Add Low Stock Alert Widget screenshot.
// #figure(
//   figure-placeholder("Dashboard Widget: Low Stock Alert (UC-0020)"),
//   caption: [Operational Alert UI: Dashboard widget highlighting SKUs below their reorder threshold.],
// )

*Automated Alerting Flow:*
- *UI Notification (Read Model):* The "Low Stock" widget allows administrators to subscribe to specific categories. It does not query the live `Inventory` table (which is hot). Instead, it polls a dedicated `Alerts` table.
- *Sequence Flow:* As shown in @fig:sq-0019, the system uses a *Materialized View* pattern. A background worker periodically aggregates inventory events and updates the `LowStockSnapshot` table. The UI simply reads this pre-computed snapshot, decoupled from the high-throughput inventory write path, ensuring zero impact on checkout performance.
