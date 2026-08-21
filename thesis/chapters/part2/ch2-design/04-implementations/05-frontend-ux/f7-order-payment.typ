===== Order Management: UC-ADM-ORD, UC-ADM-ORD-ITEMS

Filterable, paginated order grid with order number, customer name (linked), checkout state (colour-coded badge), payment status, shipment status, total, creation date. Filters: status multi-select, date range, payment method, keyword search (see screenshots below). Order detail page shows state transition timeline, line items table with thumbnails and variant details, address blocks, payment section with transaction ID and capture/refund/void buttons, shipment tracking. State transition buttons appear conditionally per checkout state.

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-orders-grid.png", width: 100%),
  caption: [Order management: data table with Order Number, Customer, Checkout State (coloured badges), Payment Status, Shipment Status, Total, Created. Filter bar with status multi-select and checkout-state filters, date range, search.],
) <fig-admin-orders-grid>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-order-detail.png", width: 100%),
  caption: [Order detail: header with Order Number, customer link, status badge. State transition timeline (Created to Address Set to Delivery Selected to Payment Confirmed). Line items table. Address blocks. Payment section with transaction ID, state, Capture/Refund buttons. Shipment tracking. Action buttons.],
) <fig-admin-order-detail>

===== Payment Management: UC-ADM-PAY, UC-ADM-PAY-METHOD

Payment detail panel shows payment intent ID, gateway provider, transaction ID, amount, currency, state badge with transition timeline, and capture/refund/void action bar (conditionally enabled). Payment log records gateway interactions; webhook event log records Stripe-triggered changes (see screenshots below). Payment method management table lists configured gateways with active toggle and supported currencies.

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-payment-detail.png", width: 100%),
  caption: [Payment detail: Payment Intent ID, gateway badge, amount, state timeline. Action bar with Refund/Void. Payment log table. Webhook event log.],
) <fig-admin-payment-detail>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-payment-methods.png", width: 100%),
  caption: [Payment methods: table with gateway name, active toggle, supported currencies, Edit/Delete. "Add Payment Method" button.],
) <fig-admin-payment-methods>

===== Dashboard

The admin dashboard summarises key operational metrics for orders, revenue, inventory, and recent activity, with navigation into each management surface (see screenshot below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-dashboard.png", width: 100%),
  caption: [Admin dashboard: operational overview with order/revenue/inventory summary cards and navigation into each management surface.],
) <fig-admin-dashboard>
