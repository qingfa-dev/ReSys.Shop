==== Order Lifecycle

// Diagram placeholder: Order Lifecycle use case diagram

*UC-ADM-ORD-01 — View orders.*
*Primary Actor:* Administrator. \
*Main Flow:* List orders with filtering by status, date range, and customer. View individual order detail. \
*Postcondition:* Order data displayed with full transactional context. \
*Related FR:* ORD-FR-05, ORD-FR-06, ORD-FR-13.

#v(0.5cm)
*UC-ADM-ORD-02 — Update order.*
*Primary Actor:* Administrator. \
*Main Flow:* Modify order attributes: adjust line items, update shipping and billing addresses, change delivery method. \
*Postcondition:* Order updated and totals recalculated to reflect the changes. \
*Related FR:* ORD-FR-05, ORD-FR-13.

#v(0.5cm)
*UC-ADM-ORD-03 — Approve order.*
*Primary Actor:* Administrator. \
*Main Flow:* Approve a pending order for fulfilment after verifying payment status and inventory availability. \
*Postcondition:* Order approved and moved to fulfilment queue. \
*Related FR:* ORD-FR-04, ORD-FR-13.

#v(0.5cm)
*UC-ADM-ORD-04 — Complete order.*
*Primary Actor:* Administrator. \
*Main Flow:* Mark an order as fulfilled after shipment confirmation. \
*Postcondition:* Order completed and locked against further modification. Inventory on-hand quantities decremented. \
*Related FR:* ORD-FR-09, ORD-FR-13.

#v(0.5cm)
*UC-ADM-ORD-05 — Cancel order.*
*Primary Actor:* Administrator. \
*Main Flow:* Cancel an order at any pre-confirmation stage, providing a reason. Release reserved inventory. \
*Postcondition:* Order cancelled. Inventory returned to availability. Payment voided. \
*Related FR:* ORD-FR-07.

#v(0.5cm)
*UC-ADM-ORD-06 — Resume order.*
*Primary Actor:* Administrator. \
*Main Flow:* Resume a previously paused or stalled order, returning it to the active workflow. \
*Postcondition:* Order returned to processing state. \
*Related FR:* ORD-FR-13.
