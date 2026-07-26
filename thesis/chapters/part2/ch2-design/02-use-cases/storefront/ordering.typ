==== Cart Management

// Diagram placeholder: Cart Management use case diagram

==== UC-STR-CRT-01 — Manage Cart

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-CRT-01],
    [*Use Case Name*], [Manage Cart],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Add product variants to the cart; update quantities or remove items; view cart summary.],
    [*Trigger*], [Customer selects a variant on the product detail page and adds it to the cart.],
    [*Preconditions*], [
      - Product variant exists and is available.
    ],
    [*Postconditions*], [
      - Cart persisted across page navigation. Guest carts survive browser sessions.
    ],
    [*Main Success Scenario*], [
      1. On a product detail page, selects a variant (e.g. Size M, Colour Red) and specifies quantity.
      2. Clicks Add to Cart.
      3. System validates the variant exists and quantity is within available limits.
      4. System adds the variant and quantity to the customer's cart.
      5. System displays confirmation with option to view cart.
      6. Opens the cart to view all items.
      7. System displays cart summary: items with quantities, prices, and subtotal.
      8. Updates quantity of an item or removes an item.
      9. System recalculates cart subtotal and updates the display.
    ],
    [*Alternative Flows*], [
      A1. Quantity exceeds stock: system rejects and shows max available.
      A2. Same variant already in cart: system increments existing quantity.
      A3. Last item removed: system shows empty cart message with browse prompt.
      A4. Guest customer: system assigns session-based cart identifier in signed cookie; cart persists.
    ],
    [*Exception Flows*], [
      E1. Variant deactivated or archived: system rejects and suggests refreshing product page.
    ],
    [*Related Requirements*], [ORD-FR-01, ORD-FR-10],
  ),
  caption: [UC-STR-CRT-01 -- Manage Cart.],
)

==== UC-STR-CRT-02 — Associate Cart with Account

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-CRT-02],
    [*Use Case Name*], [Associate Cart with Account],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Upon login or registration, promote the guest cart to the authenticated user context, merging contents.],
    [*Trigger*], [Customer logs in or registers while having an active guest cart.],
    [*Preconditions*], [
      - Guest cart exists with items.
      - Customer has a valid account (or creates one during the flow).
    ],
    [*Postconditions*], [
      - Cart associated with user account and available across sessions.
    ],
    [*Main Success Scenario*], [
      1. Browses storefront as guest and adds items to cart.
      2. Logs in or registers for an account.
      3. System detects guest cart exists for the current session.
      4. System retrieves any existing cart associated with the user account.
      5. System merges guest and user carts: matching variants increase quantity; unique variants are added.
      6. System associates the merged cart with the user account.
      7. System invalidates the guest cart cookie.
    ],
    [*Alternative Flows*], [
      A1. No existing user cart: system transfers guest cart to user account.
      A2. Merge exceeds available stock: system caps at max available and notifies customer.
      A3. Guest cart is empty: system associates session with account without transfer.
    ],
    [*Exception Flows*], [
      E1. Merge fails due to data conflict: system creates user cart with guest items and notifies to review.
    ],
    [*Related Requirements*], [ORD-FR-02],
  ),
  caption: [UC-STR-CRT-02 -- Associate Cart with Account.],
)

==== Checkout Flow

// Diagram placeholder: Checkout Flow use case diagram

==== UC-STR-CHK-01 — Select Shipping Address

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-CHK-01],
    [*Use Case Name*], [Select Shipping Address],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Select a saved address or enter a new shipping address for the order.],
    [*Trigger*], [Customer proceeds from cart to checkout.],
    [*Preconditions*], [
      - Customer is authenticated.
      - Cart contains items.
    ],
    [*Postconditions*], [
      - Shipping address set on order. Shipping zone determined for rate calculation.
    ],
    [*Main Success Scenario*], [
      1. From the cart, clicks Proceed to Checkout.
      2. System transitions checkout to the Address step.
      3. System displays saved addresses with the default pre-selected.
      4. Selects an existing shipping address.
      5. System determines shipping zone based on address country and state.
      6. System proceeds to the next checkout step (shipping method selection).
    ],
    [*Alternative Flows*], [
      A1. No saved addresses: system presents empty address step with prompt to create new.
      A2. Enters new address: system validates and saves to address book; sets as shipping address.
      A3. Goes back to cart: system returns to cart review step, retaining checkout progress.
    ],
    [*Exception Flows*], [
      E1. Address validation fails: system highlights missing fields and prevents progression.
    ],
    [*Related Requirements*], [ORD-FR-04],
  ),
  caption: [UC-STR-CHK-01 -- Select Shipping Address.],
)

==== UC-STR-CHK-02 — Select Shipping Method

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-CHK-02],
    [*Use Case Name*], [Select Shipping Method],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [Choose from available delivery methods with calculated rates based on zone, cart weight, and value.],
    [*Trigger*], [Customer completes shipping address step and advances to shipping method.],
    [*Preconditions*], [
      - Shipping address is set.
      - Cart contains items.
    ],
    [*Postconditions*], [
      - Shipping method and rate applied to order. Shipment total updated.
    ],
    [*Main Success Scenario*], [
      1. System calculates available shipping methods and rates based on address zone, cart weight, and cart value.
      2. System displays list of available methods with rates and estimated delivery times.
      3. Reviews options and selects a preferred shipping method.
      4. System applies the selected method and rate to the order.
      5. System updates shipment total in the order summary.
      6. System presents next checkout step (payment).
    ],
    [*Alternative Flows*], [
      A1. No methods available for zone: system displays message and prompts different address.
      A2. Only one method available: system auto-selects and proceeds.
      A3. Goes back to change address: system returns to address step; rates recalculated for new address.
    ],
    [*Exception Flows*], [
      E1. Rate calculation fails: system displays error and suggests contacting support.
    ],
    [*Related Requirements*], [ORD-FR-04, ORD-FR-12],
  ),
  caption: [UC-STR-CHK-02 -- Select Shipping Method.],
)

==== UC-STR-CHK-03 — Complete Checkout

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-CHK-03],
    [*Use Case Name*], [Complete Checkout],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [Finalise an order by confirming payment and completing the purchase.],
    [*Trigger*], [Customer proceeds to payment step after address and shipping method selection.],
    [*Preconditions*], [
      - Customer is authenticated.
      - Cart is not empty.
      - Stock is available for all items.
      - Shipping address is set.
      - Shipping method is selected.
    ],
    [*Postconditions*], [
      - Order created with unique number. Inventory reserved. Payment linked. Cart cleared.
    ],
    [*Main Success Scenario*], [
      1. Initiates the payment step from the checkout flow.
      2. System displays order summary with line items, shipping, tax, and total.
      3. Enters payment details or selects a saved payment method.
      4. System creates a payment intent with the payment gateway.
      5. System reserves inventory for each line item.
      6. Reviews final order summary and confirms the purchase.
      7. System captures payment and generates a unique order number.
      8. System transitions order to Confirmed state.
      9. System clears cart and displays order confirmation with order number.
      10. System sends order confirmation notification.
    ],
    [*Alternative Flows*], [
      A1. Stock depleted during checkout: system notifies, removes affected items, returns to cart.
      A2. Payment failure: system notifies with reason; allows retry; inventory not reserved.
      A3. Concurrent checkout conflict: system informs and refreshes cart before allowing retry.
      A4. Navigates back during payment: system retains checkout state for resumption.
    ],
    [*Exception Flows*], [
      E1. Order number generation fails under concurrent load: system retries within transaction; no duplicates issued.
      E2. Payment captured but inventory reservation fails: system voids payment and notifies order not completed.
    ],
    [*Related Requirements*], [ORD-FR-04, ORD-FR-05, ORD-FR-08, ORD-FR-11],
  ),
  caption: [UC-STR-CHK-03 -- Complete Checkout.],
)

==== Order History

// Diagram placeholder: Order History use case diagram

==== UC-STR-OHI-01 — View Order History

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-OHI-01],
    [*Use Case Name*], [View Order History],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [None],
    [*Goal*], [List past orders with status, date, and total; view individual order detail.],
    [*Trigger*], [Customer navigates to order history in their account.],
    [*Preconditions*], [
      - Customer is authenticated.
    ],
    [*Postconditions*], [
      - Complete order history visible.
    ],
    [*Main Success Scenario*], [
      1. Navigates to order history from account menu.
      2. System displays past orders in reverse chronological order with pagination, showing order number, date, status, and total.
      3. Selects an order to view detail.
      4. System displays full order detail: line items, shipping address, method, payment state, shipment state, and status timeline.
    ],
    [*Alternative Flows*], [
      A1. No orders: system displays message with prompt to browse catalog.
      A2. Applies date range or status filters: system refreshes listing with filtered results.
    ],
    [*Exception Flows*], [
      E1. Retrieval failure: system displays error and offers retry.
    ],
    [*Related Requirements*], [ORD-FR-14],
  ),
  caption: [UC-STR-OHI-01 -- View Order History.],
)

==== UC-STR-OHI-02 — Cancel Order

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-STR-OHI-02],
    [*Use Case Name*], [Cancel Order],
    [*Primary Actor*], [Customer],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [Cancel a pending order before confirmation, releasing reserved inventory and voiding payment.],
    [*Trigger*], [Customer selects cancel on a pending order from order history.],
    [*Preconditions*], [
      - Customer is authenticated.
      - Order is cancellable (not confirmed, completed, or already cancelled).
    ],
    [*Postconditions*], [
      - Order cancelled. Inventory released. Payment voided.
    ],
    [*Main Success Scenario*], [
      1. Opens order detail from order history.
      2. System displays order detail with current status and cancel action if cancellable.
      3. Selects Cancel Order.
      4. System displays confirmation explaining inventory release and payment void.
      5. Confirms the cancellation.
      6. System releases all reserved inventory.
      7. System voids the payment associated with the order.
      8. System transitions order to cancelled state.
      9. System displays updated status and sends cancellation confirmation.
    ],
    [*Alternative Flows*], [
      A1. Order state changed since page loaded: system refreshes and informs cancellation unavailable.
      A2. Cancels confirmation prompt: system returns to order detail without changes.
    ],
    [*Exception Flows*], [
      E1. Payment gateway unreachable: system cancels order, releases inventory, queues void, notifies customer.
    ],
    [*Related Requirements*], [ORD-FR-07],
  ),
  caption: [UC-STR-OHI-02 -- Cancel Order.],
)
