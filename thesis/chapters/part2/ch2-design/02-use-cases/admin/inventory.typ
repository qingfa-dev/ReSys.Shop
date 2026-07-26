==== Stock Location Management

// Diagram placeholder: Stock Location use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-LOC-01], [Manage stock locations], [Admin], [Create, update, or remove warehouse locations and set a default location for new stock intake.], [Admin is authenticated with location management permissions.], [Location configuration updated. Stock items assigned to modified locations retain valid references.],
)

==== Stock Item Management

// Diagram placeholder: Stock Item use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-STK-01], [Manage stock items], [Admin], [Create stock items for variants at specific locations with initial on-hand quantities; update, remove, or bulk-adjust quantities.], [Admin is authenticated with stock management permissions. The variant and location exist.], [Stock quantities updated. Changes recorded in the audit log with operator identity and reason.],
  [UC-ADM-STK-02], [Restock inventory], [Admin], [Increase on-hand quantity for a stock item, recording the restock event.], [Admin is authenticated. The stock item exists.], [On-hand quantity incremented. Stock movement audit entry created.],
  [UC-ADM-STK-03], [Monitor low stock], [Admin], [View a filtered list of stock items where on-hand quantity falls below the configured threshold.], [Admin is authenticated with inventory viewing permissions.], [Low-stock items identified for replenishment planning.],
)

==== Stock Movement and Transfer

// Diagram placeholder: Stock Movement use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-STK-04], [Transfer stock], [Admin], [Initiate a stock transfer from one location to another; record in-transit status, confirm receipt, or cancel pending transfers.], [Admin is authenticated with transfer permissions. Source and destination locations exist. Sufficient stock at source.], [Stock decremented at source, incremented at destination upon receipt. Full audit trail recorded.],
  [UC-ADM-STK-05], [Review stock movements], [Admin], [Browse all stock movements with paging; view detail for any movement.], [Admin is authenticated with movement viewing permissions.], [Complete audit trail visible for compliance and operational review.],
)
