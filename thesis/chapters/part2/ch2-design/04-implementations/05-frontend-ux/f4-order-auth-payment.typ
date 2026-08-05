===== Order History: UC-STR-OHI

Past orders display in vertically stacked cards with order number, date, colour-coded status badge, item count, and total. Expanding a card reveals line items with thumbnails, quantities, and prices. Active orders show a Cancel button when cancellable. A detail page displays the full state transition timeline (see screenshots below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-order-history.png", width: 100%),
//   caption: [Order history: 3 stacked order cards showing order number, date, status badge (Shipped in green), item count, and total. Top card expanded with line items including thumbnails and prices.],
// ) <fig-order-history>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-order-detail.png", width: 100%),
//   caption: [Order detail: order number and status at top, state transition timeline with timestamps (Address to Delivery to Payment to Confirm to Complete), line items table, address blocks, payment method, totals table.],
// ) <fig-order-detail>

===== Authentication: UC-STR-AUT, UC-STR-SES

Three authentication pathways: email/password login, Google OAuth 2.0, and guest sessions. Registration includes email, password with strength indicator, and full name. Password reset uses a two-step email-token flow. The session page lists active sessions with device, IP, and last-activity; "Logout All Devices" terminates all sessions (see screenshots below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-login.png", width: 100%),
//   caption: [Login page: centered card with email input, password with show/hide toggle, Remember me checkbox, Sign In button, Forgot password link, divider, and Sign in with Google button. Registration link at bottom.],
// ) <fig-login>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-register.png", width: 100%),
//   caption: [Registration page: centered card with full name, email, password (with strength bar), confirm password fields, Create Account button. Login link at bottom.],
// ) <fig-register>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-sessions.png", width: 100%),
//   caption: [Session management: list of active sessions with device type icon, browser, IP address, location, last activity timestamp. "Logout All Devices" button at top.],
// ) <fig-sessions>

===== Payment Processing: UC-STR-PAY

During checkout, the payment step presents methods from `GET /api/storefront/payment/methods`. Selecting a method triggers payment intent creation. For Stripe, the Stripe Elements embedded UI collects card details within an iframe, isolating sensitive data from the storefront's JavaScript context (see screenshots below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-payment-methods.png", width: 100%),
//   caption: [Payment method selection: 3 methods (Stripe with card icon, Cash on Delivery, Bank Transfer) with radio buttons and descriptions. Selected method highlighted. Summary sidebar showing order total.],
// ) <fig-payment-methods>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-payment-stripe.png", width: 100%),
//   caption: [Stripe payment form: embedded Stripe Elements card-number, expiry, and CVC fields within storefront layout. Order summary in sidebar. "Pay 2,850,000 VND" button.],
// ) <fig-payment-stripe>
