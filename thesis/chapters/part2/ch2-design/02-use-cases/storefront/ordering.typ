==== Cart Management

// Diagram placeholder: Cart Management use case diagram

==== UC-STR-CRT — Manage Cart

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-CRT],
    [*Use Case Name*], [Manage Cart],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Add, update, and remove items in a shopping cart; associate guest cart with account.],
    [*Trigger*], [Customer selects a variant on the product detail page and adds it to the cart.],
    [*Preconditions*], [
      - Product variant exists and is available.
    ],
    [*Postconditions*], [
      - Cart persisted across page navigation. Guest carts survive browser sessions.
    ],
    [*Main Success Scenario*], [
      *Manage Cart Items*
      1. On a product detail page, selects a variant and specifies quantity.
      2. Clicks Add to Cart. System validates the variant and quantity.
      3. System adds the variant to the customer's cart and displays confirmation.
      4. Opens the cart to view all items with quantities, prices, and subtotal.
      5. Updates quantity of an item or removes an item.
      6. System recalculates cart subtotal and updates the display.
      ,
      *Associate Cart with Account*
      1. Browses storefront as guest and adds items to cart.
      2. Logs in or registers for an account.
      3. System detects guest cart exists and retrieves any existing user cart.
      4. System merges guest and user carts: matching variants increase quantity; unique variants are added.
      5. System associates the merged cart with the user account and invalidates guest cart cookie.
    ],
    [*Alternative Flows*], [
      A1. Quantity exceeds stock: system rejects and shows max available.
      A2. Same variant already in cart: system increments existing quantity.
      A3. Guest customer: system assigns session-based cart identifier in signed cookie.
      A4. No existing user cart: system transfers guest cart to user account directly.
      A5. Merge exceeds available stock: system caps at max available and notifies customer.
    ],
    [*Exception Flows*], [
      E1. Variant deactivated or archived: system rejects and suggests refreshing product page.
      E2. Merge fails due to data conflict: system creates user cart with guest items and notifies to review.
    ],
    [*Related Requirements*], [ORD-FR-01, ORD-FR-02, ORD-FR-10],
  ),
  caption: [UC-STR-CRT -- Manage Cart.],
)

==== Checkout Flow

// Diagram placeholder: Checkout Flow use case diagram

==== UC-STR-CHK — Checkout

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-CHK],
    [*Use Case Name*], [Checkout],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [Complete the multi-step checkout: address selection, shipping method, and order confirmation.],
    [*Trigger*], [Customer proceeds from cart to checkout.],
    [*Preconditions*], [
      - Customer is authenticated.
      - Cart is not empty.
      - Stock is available for all items.
    ],
    [*Postconditions*], [
      - Order created with unique number. Inventory reserved. Payment linked. Cart cleared.
    ],
    [*Main Success Scenario*], [
      *Select Shipping Address*
      1. From the cart, clicks Proceed to Checkout.
      2. System transitions checkout to the Address step.
      3. System displays saved addresses with the default pre-selected.
      4. Selects an existing shipping address or enters a new one.
      5. System determines shipping zone and proceeds to the next step.
      ,
      *Select Shipping Method*
      1. System calculates available shipping methods and rates based on address zone, cart weight, and cart value.
      2. System displays list of available methods with rates and estimated delivery times.
      3. Reviews options and selects a preferred shipping method.
      4. System applies the selected method and rate; updates shipment total in order summary.
      5. System presents next checkout step (payment).
      ,
      *Complete Checkout*
      1. System displays order summary with line items, shipping, tax, and total.
      2. Enters payment details or selects a saved payment method.
      3. System creates a payment intent and reserves inventory for each line item.
      4. Reviews final order summary and confirms the purchase.
      5. System captures payment and generates a unique order number.
      6. System transitions order to Confirmed state, clears cart, and displays order confirmation.
      7. System sends order confirmation notification.
    ],
    [*Alternative Flows*], [
      A1. No saved addresses: system presents empty address step with prompt to create new.
      A2. No methods available for zone: system displays message and prompts different address.
      A3. Only one method available: system auto-selects and proceeds.
      A4. Stock depleted during checkout: system notifies, removes affected items, returns to cart.
      A5. Payment failure: system notifies with reason; allows retry; inventory not reserved.
    ],
    [*Exception Flows*], [
      E1. Address validation fails: system highlights missing fields and prevents progression.
      E2. Rate calculation fails: system displays error and suggests contacting support.
      E3. Payment captured but inventory reservation fails: system voids payment and notifies order not completed.
    ],
    [*Related Requirements*], [ORD-FR-04, ORD-FR-05, ORD-FR-08, ORD-FR-11, ORD-FR-12],
  ),
  caption: [UC-STR-CHK -- Checkout.],
)

==== Order History

// Diagram placeholder: Order History use case diagram

==== UC-STR-OHI — Order History

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-OHI],
    [*Use Case Name*], [Order History],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [View past orders and cancel pending orders.],
    [*Trigger*], [Customer navigates to order history in their account.],
    [*Preconditions*], [
      - Customer is authenticated.
    ],
    [*Postconditions*], [
      - Complete order history visible. Cancelled orders release inventory and void payment.
    ],
    [*Main Success Scenario*], [
      *View Order History*
      1. Navigates to order history from account menu.
      2. System displays past orders in reverse chronological order with pagination, showing order number, date, status, and total.
      3. Applies optional date range or status filters.
      4. Selects an order to view full detail: line items, shipping address, method, payment state, shipment state, and status timeline.
      ,
      *Cancel Order*
      1. Opens order detail from order history.
      2. System displays order detail with current status and cancel action if cancellable.
      3. Selects Cancel Order.
      4. System displays confirmation explaining inventory release and payment void.
      5. Confirms. System releases reserved inventory, voids payment, transitions to cancelled, and sends confirmation.
    ],
    [*Alternative Flows*], [
      A1. No orders: system displays message with prompt to browse catalog.
      A2. Order state changed since page loaded: system refreshes and informs cancellation unavailable.
    ],
    [*Exception Flows*], [
      E1. Payment gateway unreachable (Cancel): system cancels order, releases inventory, queues void, notifies customer.
      E2. Retrieval failure (View): system displays error and offers retry.
    ],
    [*Related Requirements*], [ORD-FR-07, ORD-FR-14],
  ),
  caption: [UC-STR-OHI -- Order History.],
)
