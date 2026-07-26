==== Cart Management

// Diagram placeholder: Cart Management use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-STR-CRT-01], [Manage cart], [Customer], [Add product variants with desired quantity to the cart; update quantities or remove items; view cart summary with item totals.], [The product variant exists and is available.], [Cart persisted across page navigation. Guest carts survive browser sessions.],
  [UC-STR-CRT-02], [Associate cart with account], [Customer], [Upon login or registration, promote the existing guest cart to the authenticated user context, merging contents without data loss.], [A guest cart exists. Customer has a valid account.], [Cart associated with user account and available across devices.],
)

==== Checkout Flow

// Diagram placeholder: Checkout Flow use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-STR-CHK-01], [Select shipping address], [Customer], [Select a saved address or enter a new shipping address for the order.], [Customer is authenticated. Cart contains items.], [Shipping address set on the order. Shipping zone determined for rate calculation.],
  [UC-STR-CHK-02], [Select shipping method], [Customer], [Choose from available delivery methods with calculated rates based on address zone, cart weight, and cart value.], [Shipping address is set. Cart contains items.], [Shipping method and rate applied to the order. Shipment total updated.],
  [UC-STR-CHK-03], [Complete checkout], [Customer], [Proceed to payment, enter payment details, review order summary, and confirm.], [Cart is not empty. Stock is available. Shipping address is set. Shipping method is selected.], [Order created with unique order number. Inventory reserved for each line item. Payment linked to order. Cart cleared.],
)

=== UC-STR-CHK-03 — Complete Checkout

#table(
  columns: (auto, 1fr),
  stroke: 0.5pt,
  [*Field*], [*Description*],
  [Use Case ID], [UC-STR-CHK-03],
  [Use Case Name], [Complete Checkout],
  [Primary Actor], [Customer],
  [Goal], [Finalise an order by confirming payment and completing the purchase workflow.],
  [Trigger], [The customer proceeds to the payment step after address and shipping method selection.],
  [Preconditions], [
    - The customer is authenticated.
    - The cart is not empty.
    - Stock is available for all cart items.
    - A shipping address is set.
    - A shipping method is selected.
  ],
  [Postconditions], [
    - Order created with a unique order number.
    - Inventory reserved for each line item.
    - Payment linked to the order.
    - Cart cleared.
  ],
  [Related FR], [ORD-FR-04, ORD-FR-05, ORD-FR-08, ORD-FR-11],
)

*Main Success Scenario*

#table(
  columns: (auto, 1fr, 2fr),
  stroke: 0.5pt,
  [*Step*], [*Actor*], [*System Response*],
  [1], [Customer], [Initiates the payment step from the checkout flow.],
  [2], [System], [Displays the order summary with line items, shipping cost, and total amount.],
  [3], [Customer], [Enters payment details or selects a saved payment method.],
  [4], [System], [Creates a payment intent and validates the payment details.],
  [5], [System], [Reserves inventory for each line item in the cart.],
  [6], [Customer], [Reviews the final order summary and confirms the purchase.],
  [7], [System], [Captures the payment and transitions the order to Confirmed state.],
  [8], [System], [Clears the cart and displays the order confirmation with order number.],
  [9], [System], [Sends an order confirmation notification to the customer.],
)

*Alternative and Exception Flows*

#table(
  columns: (auto, 1fr, 2fr),
  stroke: 0.5pt,
  [*ID*], [*Condition*], [*System Response*],
  [A1], [Stock depleted during checkout], [Notifies the customer that specific items are no longer available. Removes the affected items from the cart and returns to the cart review step.],
  [A2], [Payment failure], [Notifies the customer of the payment failure with the reason. Allows retry with a different payment method. Order is not created and inventory is not reserved.],
  [A3], [Concurrent checkout conflict], [Detects that another session has modified the cart or inventory. Informs the customer and refreshes the cart state before allowing a retry.],
)

==== Order History

// Diagram placeholder: Order History use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-STR-OHI-01], [View order history], [Customer], [List past orders with status, date, and total; view individual order detail.], [Customer is authenticated.], [Complete order history visible for authenticated customers.],
  [UC-STR-OHI-02], [Cancel order], [Customer], [Cancel a pending order before confirmation, releasing inventory.], [Customer is authenticated. The order is in a cancellable state.], [Order cancelled. Inventory returned to availability. Payment voided.],
)
