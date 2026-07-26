==== Stock Location Management

// Diagram placeholder: Stock Location use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-INV-01], [Manage stock locations], [Administrator],
    [Create, update, or remove warehouse locations. Set a default location for new stock intake.],
    [Location configuration updated. Stock items assigned to modified locations retain valid references.],
    [INV-FR-01],
  ),
  caption: [Administrator use cases — Stock Location Management.],
)

==== Stock Item Management

// Diagram placeholder: Stock Item use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-INV-02], [Manage stock items], [Administrator],
    [Create stock items for product variants at specific locations with initial on-hand quantities. Update, remove, or bulk-adjust quantities.],
    [Stock quantities updated. Changes recorded in the audit log with operator identity and reason.],
    [INV-FR-02, INV-FR-06, INV-FR-08],
    [UC-ADM-INV-03], [Restock inventory], [Administrator],
    [Increase on-hand quantity for a stock item, recording the restock event.],
    [On-hand quantity incremented. Stock movement audit entry created.],
    [INV-FR-02, INV-FR-06, INV-FR-08],
    [UC-ADM-INV-04], [Monitor low stock], [Administrator],
    [View a filtered list of stock items where on-hand quantity falls below the configured threshold.],
    [Low-stock items identified for replenishment planning.],
    [INV-FR-09],
  ),
  caption: [Administrator use cases — Stock Item Management.],
)

==== Stock Movement and Transfer

// Diagram placeholder: Stock Movement use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-INV-05], [Transfer stock], [Administrator],
    [Initiate a stock transfer from one location to another. Record in-transit status, confirm receipt at destination, or cancel pending transfers.],
    [Stock decremented at source, incremented at destination upon receipt. Full audit trail recorded.],
    [INV-FR-05, INV-FR-10],
    [UC-ADM-INV-06], [Review stock movements], [Administrator],
    [Browse all stock movements with paging. View detail for any movement including source, destination, quantity, and reason.],
    [Complete audit trail visible for compliance and operational review.],
    [INV-FR-06, INV-FR-12],
  ),
  caption: [Administrator use cases — Stock Movement and Transfer.],
)
