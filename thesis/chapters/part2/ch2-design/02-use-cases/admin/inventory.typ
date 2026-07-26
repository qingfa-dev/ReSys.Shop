==== Stock Location Management

// Diagram placeholder: Stock Location use case diagram

*UC-ADM-INV-01 — Manage stock locations.*
*Primary Actor:* Administrator. \
*Main Flow:* Create, update, or remove warehouse locations. Set a default location for new stock intake. \
*Postcondition:* Location configuration updated. Stock items assigned to modified locations retain valid references. \
*Related FR:* INV-FR-01.

==== Stock Item Management

// Diagram placeholder: Stock Item use case diagram

*UC-ADM-INV-02 — Manage stock items.*
*Primary Actor:* Administrator. \
*Main Flow:* Create stock items for product variants at specific locations with initial on-hand quantities. Update, remove, or bulk-adjust quantities. \
*Postcondition:* Stock quantities updated. Changes recorded in the audit log with operator identity and reason. \
*Related FR:* INV-FR-02, INV-FR-06, INV-FR-08.

#v(0.5cm)
*UC-ADM-INV-03 — Restock inventory.*
*Primary Actor:* Administrator. \
*Main Flow:* Increase on-hand quantity for a stock item, recording the restock event. \
*Postcondition:* On-hand quantity incremented. Stock movement audit entry created. \
*Related FR:* INV-FR-02, INV-FR-06, INV-FR-08.

#v(0.5cm)
*UC-ADM-INV-04 — Monitor low stock.*
*Primary Actor:* Administrator. \
*Main Flow:* View a filtered list of stock items where on-hand quantity falls below the configured threshold. \
*Postcondition:* Low-stock items identified for replenishment planning. \
*Related FR:* INV-FR-09.

==== Stock Movement and Transfer

// Diagram placeholder: Stock Movement use case diagram

*UC-ADM-INV-05 — Transfer stock.*
*Primary Actor:* Administrator. \
*Main Flow:* Initiate a stock transfer from one location to another. Record in-transit status, confirm receipt at destination, or cancel pending transfers. \
*Postcondition:* Stock decremented at source, incremented at destination upon receipt. Full audit trail recorded. \
*Related FR:* INV-FR-05, INV-FR-10.

#v(0.5cm)
*UC-ADM-INV-06 — Review stock movements.*
*Primary Actor:* Administrator. \
*Main Flow:* Browse all stock movements with paging. View detail for any movement including source, destination, quantity, and reason. \
*Postcondition:* Complete audit trail visible for compliance and operational review. \
*Related FR:* INV-FR-06, INV-FR-12.
