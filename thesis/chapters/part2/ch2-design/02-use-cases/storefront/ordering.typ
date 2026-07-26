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
    [*Goal*], [Add product variants with desired quantity to the cart; update quantities or remove items; view cart summary with item totals.],
    [*Trigger*], [Customer selects a variant on the product detail page and adds it to the cart.],
    [*Preconditions*], [
      - The product variant exists and is available.
    ],
    [*Postconditions*], [
      - Cart persisted across page navigation.
      - Guest carts survive browser sessions.
    ],
    [*Main Success Scenario*], [
      1. Customer -- On a product detail page, selects a variant (e.g. Size M, Colour Red) and specifies quantity.
      2. Customer -- Clicks Add to Cart.
      3. System -- Validates that the variant exists and the requested quantity is within available limits.
      4. System -- Adds the variant and quantity to the customer's cart.
      5. System -- Displays a confirmation that the item was added, with the option to view the cart.
      6. Customer -- Opens the cart to view all items.
      7. System -- Displays the cart summary: list of items with quantities, individual prices, and subtotal.
      8. Customer -- Updates the quantity of an item or removes an item.
      9. System -- Recalculates the cart subtotal and updates the display.
    ],
    [*Alternative Flows*], [
      A1. Requested quantity exceeds available stock -- System rejects the addition and displays the maximum available quantity.
      A2. Same variant already exists in the cart -- System increments the existing quantity rather than creating a duplicate line item.
      A3. Customer removes the last item from the cart -- System shows an empty cart message with a prompt to browse products.
      A4. Customer is a guest -- System assigns a session-based cart identifier stored in a signed cookie; the cart persists across browser sessions.
    ],
    [*Exception Flows*], [
      E1. Variant was deactivated or archived since the page was loaded -- System rejects the addition and suggests the customer refresh the product detail page.
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
    [*Goal*], [Upon login or registration, promote the existing guest cart to the authenticated user context, merging contents without data loss.],
    [*Trigger*], [Customer logs in or registers while having an active guest cart.],
    [*Preconditions*], [
      - A guest cart exists with items.
      - Customer has a valid account (or creates one during the flow).
    ],
    [*Postconditions*], [
      - Cart associated with the user account.
      - Cart available across devices and sessions.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Browses the storefront as a guest and adds items to the cart.
      2. Customer -- Logs in or registers for an account during the session.
      3. System -- Detects that a guest cart exists for the current session.
      4. System -- Retrieves any existing cart associated with the authenticated user account.
      5. System -- Merges the guest cart items with the user's existing cart: matching variants increase quantity; unique variants are added.
      6. System -- Associates the merged cart with the user account.
      7. System -- Invalidates the guest cart cookie.
    ],
    [*Alternative Flows*], [
      A1. User has no existing cart -- System simply transfers the guest cart to the user account.
      A2. Merge results in a quantity exceeding available stock for a variant -- System caps the quantity at the maximum available and notifies the customer.
      A3. Guest cart is empty -- System simply associates the session with the user account without any cart transfer.
    ],
    [*Exception Flows*], [
      E1. Cart merge fails due to a data conflict -- System creates the user cart with the guest items and notifies the customer to review the cart for correctness.
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
    [*Trigger*], [Customer proceeds from the cart to the checkout workflow.],
    [*Preconditions*], [
      - Customer is authenticated.
      - Cart contains items.
    ],
    [*Postconditions*], [
      - Shipping address set on the order.
      - Shipping zone determined for rate calculation.
    ],
    [*Main Success Scenario*], [
      1. Customer -- From the cart, clicks Proceed to Checkout.
      2. System -- Transitions the checkout to the Address step.
      3. System -- Displays the customer's saved addresses with the default pre-selected.
      4. Customer -- Selects an existing shipping address.
      5. System -- Determines the shipping zone based on the selected address country and state.
      6. System -- Proceeds to the next checkout step (shipping method selection).
    ],
    [*Alternative Flows*], [
      A1. Customer has no saved addresses -- System presents an empty address step with a prompt to create a new address.
      A2. Customer enters a new address -- System validates the address fields (name, street, city, country, state, postal code), saves it to the customer's address book, and sets it as the shipping address for this order.
      A3. Customer goes back to the cart -- System returns to the cart review step and retains the checkout progress.
    ],
    [*Exception Flows*], [
      E1. Address validation fails due to incomplete required fields -- System highlights the missing fields and prevents progression to the next step.
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
    [*Goal*], [Choose from available delivery methods with calculated rates based on address zone, cart weight, and cart value.],
    [*Trigger*], [Customer completes the shipping address step and advances to the shipping method step.],
    [*Preconditions*], [
      - Shipping address is set.
      - Cart contains items.
    ],
    [*Postconditions*], [
      - Shipping method and rate applied to the order.
      - Shipment total updated.
    ],
    [*Main Success Scenario*], [
      1. System -- Calculates available shipping methods and rates based on the shipping address zone, cart weight, and cart value.
      2. System -- Displays the list of available shipping methods with calculated rates and estimated delivery times.
      3. Customer -- Reviews the options and selects a preferred shipping method.
      4. System -- Applies the selected method and rate to the order.
      5. System -- Updates the shipment total in the order summary.
      6. System -- Presents the next checkout step (payment).
    ],
    [*Alternative Flows*], [
      A1. No shipping methods are available for the delivery zone -- System displays a message indicating delivery is not available for the selected address and prompts the customer to choose a different address.
      A2. Only one shipping method is available -- System auto-selects the single available method and proceeds to the next step.
      A3. Customer goes back to change the shipping address -- System returns to the address step; rates will be recalculated based on the new address.
    ],
    [*Exception Flows*], [
      E1. Rate calculation fails due to missing rate configuration -- System displays an error message and suggests the customer contact support to complete the order.
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
    [*Goal*], [Finalise an order by confirming payment and completing the purchase workflow.],
    [*Trigger*], [Customer proceeds to the payment step after address and shipping method selection.],
    [*Preconditions*], [
      - Customer is authenticated.
      - Cart is not empty.
      - Stock is available for all cart items.
      - Shipping address is set.
      - Shipping method is selected.
    ],
    [*Postconditions*], [
      - Order created with a unique order number.
      - Inventory reserved for each line item.
      - Payment linked to the order.
      - Cart cleared.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Initiates the payment step from the checkout flow.
      2. System -- Displays the order summary with line items, shipping cost, tax, and total amount.
      3. Customer -- Enters payment details or selects a saved payment method.
      4. System -- Creates a payment intent with the payment gateway and validates the payment details.
      5. System -- Reserves inventory for each line item in the cart.
      6. Customer -- Reviews the final order summary and confirms the purchase.
      7. System -- Captures the payment and generates a unique order number.
      8. System -- Transitions the order to Confirmed state.
      9. System -- Clears the cart and displays the order confirmation with order number.
      10. System -- Sends an order confirmation notification to the customer.
    ],
    [*Alternative Flows*], [
      A1. Stock depleted during checkout -- System notifies the customer that specific items are no longer available; removes the affected items and returns to the cart review step.
      A2. Payment failure -- System notifies the customer of the payment failure with the reason; allows retry with the same or a different payment method; inventory is not reserved.
      A3. Concurrent checkout conflict -- System detects that another session has modified the cart or inventory; informs the customer and refreshes the cart state before allowing a retry.
      A4. Customer navigates back during payment -- System retains the checkout state and allows the customer to resume from the payment step.
    ],
    [*Exception Flows*], [
      E1. Order number generation fails under concurrent load -- System retries within the transaction boundary and succeeds; no duplicate order numbers are issued.
      E2. Payment is captured but inventory reservation fails -- System voids the payment and notifies the customer that the order could not be completed.
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
    [*Trigger*], [Customer navigates to the order history section of their account.],
    [*Preconditions*], [
      - Customer is authenticated.
    ],
    [*Postconditions*], [
      - Complete order history visible for the authenticated customer.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Navigates to the order history page from their account menu.
      2. System -- Displays the list of past orders in reverse chronological order with pagination, showing order number, date, status, and total.
      3. Customer -- Selects an individual order to view detail.
      4. System -- Displays the full order detail: line items with prices, shipping address, shipping method, payment state, shipment state, and status timeline.
    ],
    [*Alternative Flows*], [
      A1. Customer has no orders -- System displays a message that no orders have been placed yet with a prompt to browse the catalog.
      A2. Customer applies date range or status filters -- System refreshes the listing with filtered results.
    ],
    [*Exception Flows*], [
      E1. System fails to retrieve order data -- System displays an error message and offers a retry option.
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
    [*Goal*], [Cancel a pending order before confirmation, releasing reserved inventory and voiding the payment.],
    [*Trigger*], [Customer selects the cancel action on a pending order from the order history.],
    [*Preconditions*], [
      - Customer is authenticated.
      - The order is in a cancellable state (not confirmed, completed, or already cancelled).
    ],
    [*Postconditions*], [
      - Order cancelled.
      - Reserved inventory returned to availability.
      - Payment voided.
    ],
    [*Main Success Scenario*], [
      1. Customer -- Opens the order detail from the order history.
      2. System -- Displays the order detail with current status and the cancel action if the order is cancellable.
      3. Customer -- Selects Cancel Order.
      4. System -- Displays a confirmation prompt explaining that inventory will be released and the payment will be voided.
      5. Customer -- Confirms the cancellation.
      6. System -- Releases all reserved inventory back to available stock.
      7. System -- Voids the payment associated with the order.
      8. System -- Transitions the order to cancelled state.
      9. System -- Displays the updated order status and sends a cancellation confirmation notification.
    ],
    [*Alternative Flows*], [
      A1. Order state changed since the page was loaded -- System refreshes the order and informs the customer that cancellation is no longer available.
      A2. Customer cancels the confirmation prompt -- System returns to the order detail view without changes.
    ],
    [*Exception Flows*], [
      E1. Payment gateway is unreachable for the void operation -- System cancels the order and releases inventory; the void is queued for retry and the customer is notified that the payment void will be processed shortly.
    ],
    [*Related Requirements*], [ORD-FR-07],
  ),
  caption: [UC-STR-OHI-02 -- Cancel Order.],
)
