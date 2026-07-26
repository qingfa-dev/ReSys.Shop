==== Stock Location Management

// Diagram placeholder: Stock Location use case diagram

==== UC-ADM-LOC-01 — Manage Stock Locations

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-LOC-01],
    [*Use Case Name*], [Manage Stock Locations],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, or remove warehouse locations and set a default.],
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
      6. Saves the changes.
      7. System validates location name uniqueness.
      8. System persists the location configuration.
      9. System confirms the changes.
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
  caption: [UC-ADM-LOC-01 -- Manage Stock Locations.],
)

==== Stock Item Management

// Diagram placeholder: Stock Item use case diagram

==== UC-ADM-STK-01 — Manage Stock Items

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-STK-01],
    [*Use Case Name*], [Manage Stock Items],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create or adjust stock items for variants at specific locations.],
    [*Trigger*], [Administrator navigates to stock item management.],
    [*Preconditions*], [
      - Authenticated with stock management permissions.
      - Variant and location exist.
    ],
    [*Postconditions*], [
      - Stock quantities updated. Changes logged.
    ],
    [*Main Success Scenario*], [
      1. Navigates to stock item management.
      2. System displays stock items with variant, location, on-hand, and reserved quantities.
      3. Creates a stock item by selecting variant and location, enters initial on-hand quantity.
      4. Alternatively selects existing stock item and updates on-hand quantity.
      5. Provides a reason for the adjustment.
      6. Saves the changes.
      7. System validates variant-location combination is unique.
      8. System persists the stock item and records the adjustment in audit log.
      9. System confirms and displays updated quantities.
    ],
    [*Alternative Flows*], [
      A1. Bulk adjustment via file upload: system processes, validates, reports success/failure counts.
      A2. Set on-hand to zero: system warns variant shows as out of stock.
      A3. Reduce below reserved quantity: system rejects and shows current reserved.
    ],
    [*Exception Flows*], [
      E1. Variant-location pair already exists: system rejects and suggests editing existing.
      E2. Persistence failure: system reports and retains input for retry.
    ],
    [*Related Requirements*], [INV-FR-02, INV-FR-08],
  ),
  caption: [UC-ADM-STK-01 -- Manage Stock Items.],
)

==== UC-ADM-STK-02 — Restock Inventory

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-STK-02],
    [*Use Case Name*], [Restock Inventory],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Increase on-hand quantity for a stock item and record the restock.],
    [*Trigger*], [Administrator receives new stock and navigates to record the restock.],
    [*Preconditions*], [
      - Authenticated with stock management permissions.
      - Stock item exists.
    ],
    [*Postconditions*], [
      - On-hand quantity incremented. Restock event logged.
    ],
    [*Main Success Scenario*], [
      1. Locates the stock item in the management interface.
      2. System displays current stock detail with on-hand and reserved quantities.
      3. Enters the restock quantity.
      4. Provides a restock reference (e.g. purchase order number) and notes.
      5. Confirms the restock.
      6. System increments on-hand quantity by the restock amount.
      7. System records the restock event with operator, timestamp, and reference.
      8. System confirms restock and displays updated quantities.
    ],
    [*Alternative Flows*], [
      A1. Bulk restock: system presents multi-line or file upload interface; each line validated independently.
      A2. Negative restock quantity: system rejects and suggests stock adjustment instead.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system refreshes and asks to re-enter quantity.
    ],
    [*Related Requirements*], [INV-FR-06],
  ),
  caption: [UC-ADM-STK-02 -- Restock Inventory.],
)

==== UC-ADM-STK-03 — Monitor Low Stock

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-STK-03],
    [*Use Case Name*], [Monitor Low Stock],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [View stock items where on-hand quantity falls below the configured threshold.],
    [*Trigger*], [Administrator navigates to the low stock monitoring view.],
    [*Preconditions*], [
      - Authenticated with inventory viewing permissions.
    ],
    [*Postconditions*], [
      - Low-stock items identified for replenishment.
    ],
    [*Main Success Scenario*], [
      1. Navigates to low stock monitoring view.
      2. System displays stock items below the configured threshold.
      3. System shows for each: variant, location, on-hand quantity, threshold, days since last restock.
      4. Reviews low-stock list and identifies items needing replenishment.
      5. Optionally filters by location or product category.
      6. Optionally exports list for purchase order creation.
    ],
    [*Alternative Flows*], [
      A1. No items below threshold: system displays message that all stock levels are sufficient.
      A2. Adjusts threshold for an item: system updates and re-evaluates status.
    ],
    [*Exception Flows*], [
      E1. Retrieval failure: system displays error and offers retry.
    ],
    [*Related Requirements*], [INV-FR-09],
  ),
  caption: [UC-ADM-STK-03 -- Monitor Low Stock.],
)

==== Stock Movement and Transfer

// Diagram placeholder: Stock Movement use case diagram

==== UC-ADM-STK-04 — Transfer Stock

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-STK-04],
    [*Use Case Name*], [Transfer Stock],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Initiate a stock transfer between locations; confirm receipt or cancel pending transfers.],
    [*Trigger*], [Administrator navigates to the stock transfer interface.],
    [*Preconditions*], [
      - Authenticated with transfer permissions.
      - Source and destination locations exist.
      - Sufficient stock at source.
    ],
    [*Postconditions*], [
      - Stock decremented at source, incremented at destination upon receipt. Audit trail recorded.
    ],
    [*Main Success Scenario*], [
      1. Navigates to stock transfer and initiates a new transfer.
      2. Selects source location, destination, variant, and quantity.
      3. System validates sufficient stock at source.
      4. Submits the transfer.
      5. System creates transfer record with pending status.
      6. System decrements on-hand quantity at source and logs the deduction.
      7. Upon arrival at destination, confirms receipt.
      8. System increments on-hand quantity at destination and logs the addition.
      9. System transitions transfer to completed.
      10. System confirms the completed transfer.
    ],
    [*Alternative Flows*], [
      A1. Cancel pending transfer: system returns deducted quantity to source and logs cancellation.
      A2. Partial receipt: system records partial receipt and keeps transfer open for remainder.
      A3. Transfer exceeds available stock: system rejects and shows maximum.
    ],
    [*Exception Flows*], [
      E1. Source stock modified concurrently: system refreshes and asks to adjust.
      E2. Destination has no stock item for variant: system auto-creates with zero initial quantity before receipt.
    ],
    [*Related Requirements*], [INV-FR-05, INV-FR-10],
  ),
  caption: [UC-ADM-STK-04 -- Transfer Stock.],
)

==== UC-ADM-STK-05 — Review Stock Movements

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-STK-05],
    [*Use Case Name*], [Review Stock Movements],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Browse all stock movements with paging; view movement detail.],
    [*Trigger*], [Administrator navigates to the stock movement audit interface.],
    [*Preconditions*], [
      - Authenticated with movement viewing permissions.
    ],
    [*Postconditions*], [
      - Complete audit trail visible for review.
    ],
    [*Main Success Scenario*], [
      1. Navigates to stock movement audit interface.
      2. System displays all stock movements in reverse chronological order with pagination.
      3. System shows each movement: timestamp, variant, source, destination, quantity, reason, operator.
      4. Applies optional filters: date range, variant, location, movement type.
      5. System refreshes listing with filtered results.
      6. Selects a movement to view full detail.
      7. System displays the complete movement record.
    ],
    [*Alternative Flows*], [
      A1. No movements match: system displays empty message with suggestion to broaden filters.
      A2. Exports audit trail: system generates downloadable report of filtered results.
    ],
    [*Exception Flows*], [
      E1. Retrieval failure: system displays error and offers retry.
    ],
    [*Related Requirements*], [INV-FR-12],
  ),
  caption: [UC-ADM-STK-05 -- Review Stock Movements.],
)
