===== Order Management: UC-ADM-ORD, UC-ADM-ORD-ITEMS

Filterable, paginated order grid with order number, customer name (linked), checkout state (colour-coded badge), payment status, shipment status, total, creation date. Filters: status multi-select, date range, payment method, keyword search (see screenshots below). Order detail page shows state transition timeline, line items table with thumbnails and variant details, address blocks, payment section with transaction ID and capture/refund/void buttons, shipment tracking. State transition buttons appear conditionally per checkout state.

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-orders-grid.png", width: 100%),
//   caption: [Order management: data table with Order Number, Customer, Checkout State (coloured badges), Payment Status, Shipment Status, Total, Created. Filter bar with status, date range, search. Summary bar: "All (150) | Pending (12) | Confirmed (45) | Shipped (78) | Cancelled (15)".],
// ) <fig-admin-orders-grid>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-order-detail.png", width: 100%),
//   caption: [Order detail: header with Order #ORD-2025-0042, customer link, status badge (Confirmed). State timeline (Created to Address Set to Delivery Selected to Payment Confirmed). Line items table. Address blocks. Payment section with Stripe transaction ID, state (Succeeded), Capture/Refund buttons. Shipment tracking. Action buttons.],
// ) <fig-admin-order-detail>

===== Payment Management: UC-ADM-PAY, UC-ADM-PAY-METHOD

Payment detail panel shows payment intent ID, gateway provider, transaction ID, amount, currency, state badge with transition timeline, and capture/refund/void action bar (conditionally enabled). Payment log records gateway interactions; webhook event log records Stripe-triggered changes (see screenshots below). Payment method management table lists configured gateways with active toggle and supported currencies.

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-payment-detail.png", width: 100%),
//   caption: [Payment detail: Payment Intent ID, Stripe badge, amount (2,850,000 VND), state timeline (Created to Authorized to Captured). Action bar: "Refund" enabled, "Void" disabled. Payment log table. Webhook event log.],
// ) <fig-admin-payment-detail>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-payment-methods.png", width: 100%),
//   caption: [Payment methods: table (Stripe: active, Cash on Delivery: active, Bank Transfer: active, Bogus Test: inactive). Each row: provider icon, name, active toggle, currencies, Edit/Delete. "Add Payment Method" button.],
// ) <fig-admin-payment-methods>
