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
    [*Goal*], [Create, update, or remove warehouse locations and set a default location for new stock intake.],
    [*Trigger*], [Administrator navigates to the stock location management interface.],
    [*Preconditions*], [
      - Administrator is authenticated with location management permissions.
    ],
    [*Postconditions*], [
      - Location configuration updated.
      - Stock items assigned to modified locations retain valid references.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the stock location management interface.
      2. System -- Displays the list of existing stock locations with their addresses and active status.
      3. Administrator -- Creates a new location with a name, address, and active status flag.
      4. Administrator -- Optionally designates the new location as the default for new stock intake.
      5. Administrator -- Optionally edits, deactivates, or removes existing locations.
      6. Administrator -- Saves the changes.
      7. System -- Validates that the location name is unique.
      8. System -- Persists the location configuration.
      9. System -- Confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Administrator attempts to delete a location with active stock items -- System warns that stock items at this location must be transferred first and prevents deletion.
      A2. Administrator deactivates a location -- System prevents new stock intake at the deactivated location but allows existing stock movements.
      A3. Only one location exists and administrator attempts to delete it -- System prevents deletion and informs the administrator that at least one location must remain.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification detected -- System detects the location was modified by another session, refreshes the data, and asks the administrator to retry.
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
    [*Goal*], [Create stock items for variants at specific locations with initial on-hand quantities; update, remove, or bulk-adjust quantities.],
    [*Trigger*], [Administrator navigates to the stock item management interface.],
    [*Preconditions*], [
      - Administrator is authenticated with stock management permissions.
      - The variant and location exist.
    ],
    [*Postconditions*], [
      - Stock quantities updated.
      - Changes recorded in the audit log with operator identity and reason.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the stock item management interface.
      2. System -- Displays the list of existing stock items with variant, location, on-hand, and reserved quantities.
      3. Administrator -- Creates a new stock item by selecting a variant and a location, then enters initial on-hand quantity.
      4. Administrator -- Alternatively selects an existing stock item and updates its on-hand quantity.
      5. Administrator -- Provides a reason for the quantity adjustment.
      6. Administrator -- Saves the changes.
      7. System -- Validates that the variant-location combination is unique.
      8. System -- Persists the stock item and records the adjustment in the audit log.
      9. System -- Confirms the changes and displays the updated quantities.
    ],
    [*Alternative Flows*], [
      A1. Administrator bulk-adjusts quantities via file upload -- System processes the file, validates each row, and reports success and failure counts.
      A2. Administrator sets on-hand quantity to zero -- System accepts the change but warns that the variant will show as out of stock.
      A3. Administrator attempts to reduce on-hand quantity below reserved quantity -- System rejects and displays the current reserved quantity.
    ],
    [*Exception Flows*], [
      E1. Variant-location pair already has a stock item -- System rejects the creation and suggests editing the existing stock item instead.
      E2. System fails to persist stock changes -- System reports the failure and retains the input data for retry.
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
    [*Goal*], [Increase on-hand quantity for a stock item, recording the restock event.],
    [*Trigger*], [Administrator receives new stock and navigates to the stock item to record the restock.],
    [*Preconditions*], [
      - Administrator is authenticated with stock management permissions.
      - The stock item exists.
    ],
    [*Postconditions*], [
      - On-hand quantity incremented.
      - Stock movement audit entry created with restock details.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Locates the stock item in the management interface.
      2. System -- Displays the current stock item detail with on-hand and reserved quantities.
      3. Administrator -- Enters the restock quantity being added.
      4. Administrator -- Provides a restock reference (e.g. purchase order number) and any notes.
      5. Administrator -- Confirms the restock.
      6. System -- Increments the on-hand quantity by the restock amount.
      7. System -- Creates a stock movement audit entry documenting the restock event with operator identity, timestamp, and reference.
      8. System -- Confirms the restock and displays the updated quantities.
    ],
    [*Alternative Flows*], [
      A1. Administrator bulk-restocks multiple stock items -- System presents a multi-line form or file upload interface; each line is validated independently.
      A2. Administrator enters a negative restock quantity -- System rejects and suggests using the stock adjustment feature instead.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification detected -- System detects the stock item was modified by another session, refreshes the data, and asks the administrator to re-enter the restock quantity.
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
    [*Goal*], [View a filtered list of stock items where on-hand quantity falls below the configured threshold.],
    [*Trigger*], [Administrator navigates to the low stock monitoring view.],
    [*Preconditions*], [
      - Administrator is authenticated with inventory viewing permissions.
    ],
    [*Postconditions*], [
      - Low-stock items identified for replenishment planning.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the low stock monitoring view.
      2. System -- Displays a list of stock items where on-hand quantity is below the configured low-stock threshold.
      3. System -- Shows for each item: variant details, location, on-hand quantity, threshold value, and days since last restock.
      4. Administrator -- Reviews the low-stock list and identifies items requiring replenishment.
      5. Administrator -- Optionally filters by location or product category.
      6. Administrator -- Optionally exports the low-stock list for purchase order creation.
    ],
    [*Alternative Flows*], [
      A1. No items are below the low-stock threshold -- System displays a message indicating that all stock levels are sufficient.
      A2. Administrator adjusts the low-stock threshold for a specific item -- System updates the threshold and re-evaluates the item's low-stock status.
    ],
    [*Exception Flows*], [
      E1. System fails to retrieve stock data -- System displays an error message and offers a retry option.
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
    [*Goal*], [Initiate a stock transfer from one location to another; record in-transit status, confirm receipt, or cancel pending transfers.],
    [*Trigger*], [Administrator navigates to the stock transfer interface.],
    [*Preconditions*], [
      - Administrator is authenticated with transfer permissions.
      - Source and destination locations exist.
      - Sufficient stock at source location.
    ],
    [*Postconditions*], [
      - Stock decremented at source, incremented at destination upon receipt.
      - Full audit trail recorded for each stage of the transfer.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the stock transfer interface and initiates a new transfer.
      2. Administrator -- Selects the source location, destination location, variant, and quantity to transfer.
      3. System -- Validates that the source location has sufficient on-hand stock.
      4. Administrator -- Submits the transfer.
      5. System -- Creates the transfer record with status set to pending.
      6. System -- Decrements on-hand quantity at the source location and records the deduction in the audit log.
      7. Administrator -- When the stock arrives at the destination, confirms receipt.
      8. System -- Increments on-hand quantity at the destination and records the addition in the audit log.
      9. System -- Transitions the transfer to completed status.
      10. System -- Confirms the completed transfer.
    ],
    [*Alternative Flows*], [
      A1. Administrator cancels a pending transfer before shipment -- System returns the decremented quantity to the source location and records the cancellation.
      A2. Partial receipt -- Administrator confirms receipt of less than the full transfer quantity; System records the partial receipt and keeps the transfer open for the remaining quantity.
      A3. Transfer quantity exceeds available stock at source -- System rejects and displays the maximum available quantity.
    ],
    [*Exception Flows*], [
      E1. Source location stock was modified by a concurrent operation -- System detects the stock change, refreshes the available quantity, and asks the administrator to adjust.
      E2. Destination location does not have a stock item for the variant -- System automatically creates the stock item at the destination with zero initial quantity before recording the receipt.
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
    [*Goal*], [Browse all stock movements with paging; view detail for any movement.],
    [*Trigger*], [Administrator navigates to the stock movement audit interface.],
    [*Preconditions*], [
      - Administrator is authenticated with movement viewing permissions.
    ],
    [*Postconditions*], [
      - Complete audit trail visible for compliance and operational review.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the stock movement audit interface.
      2. System -- Displays the list of all stock movements in reverse chronological order with pagination.
      3. System -- Shows each movement with: timestamp, variant, source location, destination location, quantity, reason, and operator identity.
      4. Administrator -- Applies optional filters: date range, variant, location, movement type.
      5. System -- Refreshes the listing with filtered results.
      6. Administrator -- Selects an individual movement to view full detail.
      7. System -- Displays the complete movement record including all metadata fields.
    ],
    [*Alternative Flows*], [
      A1. No movements match the applied filters -- System displays an empty result message with suggestion to broaden the filter criteria.
      A2. Administrator exports the movement audit trail -- System generates a downloadable report with the current filtered results.
    ],
    [*Exception Flows*], [
      E1. System fails to retrieve movement data -- System displays an error message and offers a retry option.
    ],
    [*Related Requirements*], [INV-FR-12],
  ),
  caption: [UC-ADM-STK-05 -- Review Stock Movements.],
)
