===== Order Management: UC-ADM-ORD, UC-ADM-ORD-ITEMS

Filterable, paginated order grid with order number, customer name (linked), checkout state (colour-coded badge), payment status, shipment status, total, creation date. Filters: status multi-select, date range, payment method, keyword search (see screenshots below). Order detail page shows state transition timeline, line items table with thumbnails and variant details, address blocks, payment section with transaction ID and capture/refund/void buttons, shipment tracking. State transition buttons appear conditionally per checkout state.

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-orders-grid.png", width: 100%),
  caption: [Order management: table with checkout/payment/shipment statuses and filters.],
) <fig-admin-orders-grid>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-order-detail.png", width: 100%),
  caption: [Order detail: status header, state timeline, line items, payment actions.],
) <fig-admin-order-detail>

===== Payment Management: UC-ADM-PAY, UC-ADM-PAY-METHOD

Payment detail panel shows payment intent ID, gateway provider, transaction ID, amount, currency, state badge with transition timeline, and capture/refund/void action bar (conditionally enabled). Payment log records gateway interactions; webhook event log records Stripe-triggered changes (see screenshots below). Payment method management table lists configured gateways with active toggle and supported currencies.

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-payment-detail.png", width: 100%),
  caption: [Payment detail: Payment Intent ID, gateway, timeline, refund/void actions.],
) <fig-admin-payment-detail>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-payment-methods.png", width: 100%),
  caption: [Payment methods: gateways with active toggles and supported currencies.],
) <fig-admin-payment-methods>

===== Dashboard

The admin dashboard summarises key operational metrics for orders, revenue, inventory, and recent activity, with navigation into each management surface (see screenshot below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/admin-dashboard.png", width: 100%),
  caption: [Admin dashboard: order/revenue/inventory summary cards.],
) <fig-admin-dashboard>
