==== Cart Management

// Diagram placeholder: Cart Management use case diagram

*UC-STR-ORD-01 — Manage cart.*
*Primary Actor:* Customer. \
*Main Flow:* Add product variants with desired quantity to the cart. Update quantities or remove items. View cart summary with item totals. \
*Postcondition:* Cart persisted across page navigation. Guest carts survive browser sessions. \
*Related FR:* ORD-FR-01, ORD-FR-10.

#v(0.5cm)
*UC-STR-ORD-02 — Associate cart with account.*
*Primary Actor:* Customer. \
*Main Flow:* Upon login or registration, the existing guest cart is promoted to the authenticated user context. Contents are merged without data loss. \
*Postcondition:* Cart associated with user account and available across devices. \
*Related FR:* ORD-FR-02.

==== Checkout Flow

// Diagram placeholder: Checkout Flow use case diagram

*UC-STR-ORD-03 — Select shipping address.*
*Primary Actor:* Customer. \
*Main Flow:* Select a saved address or enter a new shipping address for the order. \
*Postcondition:* Shipping address set on the order. Shipping zone determined for rate calculation. \
*Related FR:* ORD-FR-04, PRF-FR-01.

#v(0.5cm)
*UC-STR-ORD-04 — Select shipping method.*
*Primary Actor:* Customer. \
*Main Flow:* Choose from available delivery methods with calculated rates based on address zone, cart weight, and cart value. \
*Postcondition:* Shipping method and rate applied to the order. Shipment total updated. \
*Related FR:* ORD-FR-04, ORD-FR-12, SHP-FR-02, SHP-FR-06.

#v(0.5cm)
*UC-STR-ORD-05 — Complete checkout.*
*Primary Actor:* Customer. \
*Main Flow:* After address and shipping selection, proceed to payment. Enter payment details. Review order summary and confirm. \
*Postcondition:* Order created with unique order number. Inventory reserved for each line item. Payment linked to order. Cart cleared. \
*Related FR:* ORD-FR-04, ORD-FR-05, ORD-FR-08, ORD-FR-11.

==== Order History

// Diagram placeholder: Order History use case diagram

*UC-STR-ORD-06 — View order history.*
*Primary Actor:* Customer. \
*Main Flow:* List past orders with status, date, and total. View individual order detail. \
*Postcondition:* Complete order history visible for authenticated customers. \
*Related FR:* ORD-FR-14.

#v(0.5cm)
*UC-STR-ORD-07 — Cancel order.*
*Primary Actor:* Customer. \
*Main Flow:* Cancel a pending order before confirmation. Inventory is released. \
*Postcondition:* Order cancelled. Inventory returned to availability. Payment voided. \
*Related FR:* ORD-FR-07.
