==== Stock Location Management

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-stock-location-management.png",
    width: 60%
  ),
  caption: [Use case diagram for Stock Location Management (UC-ADM-LOC).],
) <fig-uc-adm-loc-d>

==== UC-ADM-LOC: Manage Stock Locations

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-LOC — Manage Stock Locations],
    [*Actor*], [Administrator],
    [*Goal*], [Create, update, and remove stock locations.],
    [*Pre/Post*], [
      Pre: authenticated with location management permissions.
      Post: location configuration updated.
    ],
    [*Scenario*], [
      + Navigates to stock location management.
      + System displays existing stock locations with addresses and active status.
      + Creates new location with name, address, and active status flag.
      + Optionally designates new location as default for stock intake.
      + Optionally edits, deactivates, or removes existing locations.
      + Saves; system validates location name uniqueness, persists, confirms.
    ],
    [*Alternatives*], [
      + A1. Delete location with active stock → system prevents, requires transfer first.
      + A2. Deactivate location → system prevents new intake but allows existing movements.
      + A3. Delete last location → system prevents; at least one must remain.
    ],
    [*Exceptions*], [
      + E1. Concurrent modification → system refreshes, asks to retry.
    ],
    [*Requirements*], [INV-FR-01],
  ),
    kind: table,
  caption: [Manage Stock Locations.],
)

==== Stock Item Management

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-stock-item-management.png",
    height: 45%
  ),
  caption: [Use case diagram for Stock Item Management (UC-ADM-STK).],
) <fig-uc-adm-stk-d>

==== UC-ADM-STK: Manage Stock

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-STK — Manage Stock],
    [*Actor*], [Administrator],
    [*Goal*], [Create, update, restock, transfer, and monitor stock levels.],
    [*Pre/Post*], [
      Pre: authenticated with stock management permissions.
      Post: stock quantities updated; changes logged for audit.
    ],
    [*Scenario*], [
      *Manage Stock Items*
      + Navigates to stock item management.
      + System displays stock items with variant, location, on-hand, and reserved quantities.
      + Creates stock item by selecting variant and location, enters initial on-hand quantity.
      + Alternatively selects existing stock item and updates on-hand quantity.
      + Provides reason for adjustment.
      + Saves; system validates variant-location uniqueness, persists, records audit log, confirms.
      ,
      *Restock Inventory*
      + Locates stock item in management interface.
      + Enters restock quantity, provides reference and notes.
      + Confirms; system increments on-hand quantity, records restock event, confirms updated quantities.
      ,
      *Transfer Stock*
      + Navigates to stock transfer, initiates new transfer.
      + Selects source location, destination, variant, quantity.
      + System validates sufficient stock at source.
      + Submits; system creates transfer record pending, decrements source.
      + Upon arrival, confirms receipt; system increments destination, transitions to completed, confirms.
      ,
      *Review Stock Movements*
      + Navigates to stock movement audit interface.
      + System displays all stock movements in reverse chronological order with pagination.
      + Applies optional filters (date range, variant, location, movement type).
      + Selects movement to view full detail.
      ,
      *Monitor Low Stock*
      + Navigates to low stock monitoring view.
      + System displays stock items below configured threshold with variant, location, on-hand, threshold, days since last restock.
      + Reviews list, identifies items needing replenishment.
      ,
    ],
    [*Alternatives*], [
      + A1. Bulk adjustment via file upload → system processes, validates, reports success/failure counts.
      + A2. Reduce below reserved quantity → system rejects, shows current reserved.
      + A3. Transfer exceeds available stock → system rejects, shows maximum.
      + A4. Cancel pending transfer → system returns deducted quantity to source, logs cancellation.
      + A5. No items below threshold (Low Stock) → system displays message that all stock levels are sufficient.
    ],
    [*Exceptions*], [
      + E1. Variant-location pair already exists → system rejects, suggests editing existing.
      + E2. Concurrent modification → system refreshes, asks to re-enter.
      + E3. Retrieval failure → system displays error, offers retry.
    ],
    [*Requirements*], [INV-FR-02, INV-FR-05, INV-FR-06, INV-FR-08, INV-FR-09, INV-FR-10, INV-FR-12],
  ),
    kind: table,
  caption: [Manage Stock.],
)
