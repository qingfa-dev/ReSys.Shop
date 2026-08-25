===== Order History: UC-STR-OHI

Past orders display in vertically stacked cards with order number, date, colour-coded status badge, item count, and total. Expanding a card reveals line items with thumbnails, quantities, and prices. Active orders show a Cancel button when cancellable. A detail page displays the full state transition timeline (see screenshots below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-order-history.png", width: 100%),
  caption: [Order history: stacked order cards with status badges, expandable line items.],
) <fig-order-history>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-order-detail.png", width: 100%),
  caption: [Order detail: status header, state timeline, line items, addresses, totals.],
) <fig-order-detail>

===== Authentication: UC-STR-AUT, UC-STR-SES

Two authentication pathways: email/password login and guest sessions. Registration includes email, password with strength indicator, and full name. Password reset uses a two-step email-token flow. The session page lists active sessions with device, IP, and last-activity; "Logout All Devices" terminates all sessions (see screenshots below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-login.png", width: 100%),
  caption: [Login page: email/password card with register link.],
) <fig-login>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-register.png", width: 100%),
  caption: [Registration page: account form with password strength indicator.],
) <fig-register>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-sessions.png", width: 100%),
  caption: [Session management: active sessions with device, IP, last activity; logout-all.],
) <fig-sessions>

===== Payment Processing: UC-STR-PAY

During checkout, the payment step presents methods from the payment-methods API. Selecting a method triggers payment intent creation. For Stripe, the Stripe Elements embedded UI collects card details within an iframe, isolating sensitive data from the storefront's JavaScript context.
