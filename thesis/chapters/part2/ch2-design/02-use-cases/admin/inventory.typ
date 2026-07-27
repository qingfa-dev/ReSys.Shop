==== Stock Location Management

// Diagram placeholder: Stock Location use case diagram

==== UC-ADM-LOC — Manage Stock Locations

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-LOC],
    [*Use Case Name*], [Manage Stock Locations],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, and remove stock locations.],
    [*Trigger*], [Administrator navigates to stock location management.],
    [*Preconditions*], [
      - Authenticated with location management permissions.
    ],
    [*Postconditions*], [
      - Location configuration updated.
    ],
    [*Main Success Scenario*], [
      1. Navigates to stock location management.
      2. System displays existing stock locations with addresses and active status.
      3. Creates a new location with name, address, and active status flag.
      4. Optionally designates the new location as default for stock intake.
      5. Optionally edits, deactivates, or removes existing locations.
      6. Saves. System validates location name uniqueness. Persists and confirms.
    ],
    [*Alternative Flows*], [
      A1. Delete location with active stock: system prevents and requires transfer first.
      A2. Deactivate location: system prevents new intake but allows existing movements.
      A3. Delete last location: system prevents; at least one must remain.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system refreshes and asks to retry.
    ],
    [*Related Requirements*], [INV-FR-01],
  ),
    kind: table,
  caption: [Manage Stock Locations.],
)

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-stock-location-management.png",
    width: 100%
  ),
  caption: [Use case diagram for Stock Location Management (UC-ADM-LOC).],
) <fig-uc-adm-loc-d>

==== Stock Item Management

// Diagram placeholder: Stock Item use case diagram

==== UC-ADM-STK — Manage Stock

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-STK],
    [*Use Case Name*], [Manage Stock],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, restock, transfer, and monitor stock levels.],
    [*Trigger*], [Administrator navigates to stock item management.],
    [*Preconditions*], [
      - Authenticated with stock management permissions.
    ],
    [*Postconditions*], [
      - Stock quantities updated. Changes logged for audit.
    ],
    [*Main Success Scenario*], [
      *Manage Stock Items*
      1. Navigates to stock item management.
      2. System displays stock items with variant, location, on-hand, and reserved quantities.
      3. Creates a stock item by selecting variant and location, enters initial on-hand quantity.
      4. Alternatively selects existing stock item and updates on-hand quantity.
      5. Provides a reason for the adjustment.
      6. Saves. System validates variant-location uniqueness, persists, and records audit log. Confirms.
      ,
      *Restock Inventory*
      1. Locates a stock item in the management interface.
      2. Enters the restock quantity and provides a reference and notes.
      3. Confirms. System increments on-hand quantity and records the restock event. Confirms updated quantities.
      ,
      *Transfer Stock*
      1. Navigates to stock transfer and initiates a new transfer.
      2. Selects source location, destination, variant, and quantity.
      3. System validates sufficient stock at source.
      4. Submits. System creates transfer record pending, decrements source.
      5. Upon arrival, confirms receipt. System increments destination and transitions to completed. Confirms.
      ,
      *Review Stock Movements*
      1. Navigates to stock movement audit interface.
      2. System displays all stock movements in reverse chronological order with pagination.
      3. Applies optional filters: date range, variant, location, movement type.
      4. Selects a movement to view full detail.
      ,
      *Monitor Low Stock*
      1. Navigates to low stock monitoring view.
      2. System displays stock items below configured threshold with variant, location, on-hand, threshold, and days since last restock.
      3. Reviews list and identifies items needing replenishment.
    ],
    [*Alternative Flows*], [
      A1. Bulk adjustment via file upload: system processes, validates, reports success/failure counts.
      A2. Reduce below reserved quantity: system rejects and shows current reserved.
      A3. Transfer exceeds available stock: system rejects and shows maximum.
      A4. Cancel pending transfer: system returns deducted quantity to source and logs cancellation.
      A5. No items below threshold (Low Stock): system displays message that all stock levels are sufficient.
    ],
    [*Exception Flows*], [
      E1. Variant-location pair already exists: system rejects and suggests editing existing.
      E2. Concurrent modification: system refreshes and asks to re-enter.
      E3. Retrieval failure: system displays error and offers retry.
    ],
    [*Related Requirements*], [INV-FR-02, INV-FR-05, INV-FR-06, INV-FR-08, INV-FR-09, INV-FR-10, INV-FR-12],
  ),
    kind: table,
  caption: [Manage Stock.],
)

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-stock-item-management.png",
    width: 100%
  ),
  caption: [Use case diagram for Stock Item Management (UC-ADM-STK).],
) <fig-uc-adm-stk-d>
