==== Cart Management
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-cart-management.png",
    width: 70%
  ),
  caption: [Use case diagram for Cart Management (UC-STR-CRT).],
) <fig-uc-str-crt-d>

==== UC-STR-CRT: Manage Cart

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-STR-CRT — Manage Cart],
    [*Actor*], [Customer],
    [*Goal*], [Add, update, and remove items in a shopping cart; associate guest cart with account.],
    [*Pre/Post*], [
      Pre: product variant exists and is available.
      Post: cart persisted across page navigation; guest carts survive browser sessions.
    ],
    [*Scenario*], [
      *Manage Cart Items*
      + On product detail page, selects variant and specifies quantity.
      + Clicks Add to Cart; system validates variant and quantity.
      + System adds variant to customer's cart, displays confirmation.
      + Opens cart to view all items with quantities, prices, subtotal.
      + Updates quantity of item or removes item.
      + System recalculates cart subtotal, updates display.
      ,
      *Associate Cart with Account*
      + Browses storefront as guest, adds items to cart.
      + Logs in or registers for account.
      + System detects guest cart exists, retrieves any existing user cart.
      + System merges guest and user carts: matching variants increase quantity, unique variants are added.
      + System associates merged cart with user account, invalidates guest cart cookie.
      ,
    ],
    [*Alternatives*], [
      + A1. Quantity exceeds stock → system rejects, shows max available.
      + A2. Same variant already in cart → system increments existing quantity.
      + A3. Guest customer → system assigns session-based cart identifier in signed cookie.
      + A4. No existing user cart → system transfers guest cart to user account directly.
      + A5. Merge exceeds available stock → system caps at max available, notifies customer.
    ],
    [*Exceptions*], [
      + E1. Variant deactivated or archived → system rejects, suggests refreshing product page.
      + E2. Merge fails due to data conflict → system creates user cart with guest items, notifies to review.
    ],
    [*Requirements*], [ORD-FR-01, ORD-FR-02, ORD-FR-10],
  ),
    kind: table,
  caption: [Manage Cart.],
)

==== Checkout Flow
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-checkout-flow.png",
    width: 100%
  ),
  caption: [Use case diagram for Checkout Flow (UC-STR-CHK).],
) <fig-uc-str-chk-d>

==== UC-STR-CHK: Checkout

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-STR-CHK — Checkout],
    [*Actor*], [Customer],
    [*Support*], [Payment Gateway],
    [*Goal*], [Complete the multi-step checkout: address selection, shipping method, and order confirmation.],
    [*Pre/Post*], [
      Pre: customer is authenticated; cart is not empty; stock is available for all items.
      Post: order created with unique number; inventory reserved; payment linked; cart cleared.
    ],
    [*Scenario*], [
      *Select Shipping Address*
      + From cart, clicks Proceed to Checkout.
      + System transitions checkout to Address step.
      + System displays saved addresses with default pre-selected.
      + Selects existing shipping address or enters new one.
      + System determines shipping zone, proceeds to next step.
      ,
      *Select Shipping Method*
      + System calculates available shipping methods and rates based on address zone, cart weight, cart value.
      + System displays list of available methods with rates and estimated delivery times.
      + Reviews options, selects preferred shipping method.
      + System applies selected method and rate, updates shipment total in order summary.
      + System presents next checkout step (payment).
      ,
      *Complete Checkout*
      + System displays order summary with line items, shipping, tax, total.
      + Enters payment details or selects saved payment method.
      + System creates payment intent, reserves inventory for each line item.
      + Reviews final order summary, confirms purchase.
      + System captures payment, generates unique order number.
      + System transitions order to Confirmed state, clears cart, displays order confirmation.
      + System sends order confirmation notification.
      ,
    ],
    [*Alternatives*], [
      + A1. No saved addresses → system presents empty address step with prompt to create new.
      + A2. No methods available for zone → system displays message, prompts different address.
      + A3. Only one method available → system auto-selects, proceeds.
      + A4. Stock depleted during checkout → system notifies, removes affected items, returns to cart.
      + A5. Payment failure → system notifies with reason, allows retry; inventory not reserved.
    ],
    [*Exceptions*], [
      + E1. Address validation fails → system highlights missing fields, prevents progression.
      + E2. Rate calculation fails → system displays error, suggests contacting support.
      + E3. Payment captured but inventory reservation fails → system voids payment, notifies order not completed.
    ],
    [*Requirements*], [ORD-FR-04, ORD-FR-05, ORD-FR-08, ORD-FR-11, ORD-FR-12],
  ),
    kind: table,
  caption: [Checkout.],
)

==== Order History
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-order-history.png",
    width: 100%
  ),
  caption: [Use case diagram for Order History (UC-STR-OHI).],
) <fig-uc-str-ohi-d>

==== UC-STR-OHI: Order History

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-STR-OHI — Order History],
    [*Actor*], [Customer],
    [*Support*], [Payment Gateway],
    [*Goal*], [View past orders and cancel pending orders.],
    [*Pre/Post*], [
      Pre: customer is authenticated.
      Post: complete order history visible; cancelled orders release inventory and void payment.
    ],
    [*Scenario*], [
      *View Order History*
      + Navigates to order history from account menu.
      + System displays past orders in reverse chronological order with pagination, showing order number, date, status, and total.
      + Applies optional date range or status filters.
      + Selects an order to view full detail: line items, shipping address, method, payment state, shipment state, and status timeline.
      ,
      *Cancel Order*
      + Opens order detail from order history.
      + System displays order detail with current status and cancel action if cancellable.
      + Selects Cancel Order.
      + System displays confirmation explaining inventory release and payment void.
      + Confirms; system releases reserved inventory, voids payment, transitions to cancelled, and sends confirmation.
      ,
    ],
    [*Alternatives*], [
      + A1. No orders → system displays message with prompt to browse catalog.
      + A2. Order state changed since page loaded → system refreshes and informs cancellation unavailable.
    ],
    [*Exceptions*], [
      + E1. Payment gateway unreachable (Cancel) → system cancels order, releases inventory, queues void, notifies customer.
      + E2. Retrieval failure (View) → system displays error and offers retry.
    ],
    [*Requirements*], [ORD-FR-07, ORD-FR-14],
  ),
    kind: table,
  caption: [Order History.],
)
