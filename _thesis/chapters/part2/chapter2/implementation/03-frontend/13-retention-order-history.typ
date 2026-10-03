===== 4. Retention (Order History)
Authenticated users access a self-service *Account Dashboard*. This area provides a tabular view of order history and a detailed receipt view for tracking shipments, empowering users to self-resolve common support queries.

// TODO: [Implementation] Add Order History Dashboard screenshot.
// #figure(
//   figure-placeholder("Order History Dashboard UI"),
//   caption: [User Account Dashboard showing Order History and detailed receipt view.],
// )

*Dashboard Interaction Flow:*
- *UI Action (Timeline):* When a user views an order, the frontend parses the raw event stream (e.g., `OrderPlaced`, `PaymentSucceeded`, `Shipped`) to render a linear progress bar. This client-side projection allows for rich visual storytelling without storing redundant "Status String" fields in the database.
- *Sequence Flow:* As shown in @fig:sq-0006, the `GetOrderTimeline` query hits a specialized Read Model. This ensures that even if the core Order Aggregate is locked by a fulfillment process, the customer can still view their history (High Availability).

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/customer/sq-0006-track-order.png", width: 60%),
  caption: [Order Tracking Sequence: Retrieving and displaying the status timeline for a specific order (UC-0006).],
) <fig:sq-0006>

/*
  Address Management (UC-0007)
*/
*Profile & Shipping Logic:*
- *UI Pattern (CRUD):* The Address Book uses a "Card Grid" layout. The "Set Default" toggle is a high-frequency action that immediately updates the local session state to reflect the new preferred shipping destination.
- *Sequence Flow:* @fig:sq-0007 details the persistence. Setting a default address triggers a `UpdateCustomerProfile` command. Crucially, this update proactively invalidates any cached shipping rates in the active Cart context to ensure next-step accuracy.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/customer/sq-0007-address-book.png", width: 60%),
  caption: [Address Book Sequence: Managing user delivery locations for streamlined checkout (UC-0007).],
) <fig:sq-0007>


