=== Use Case 2: Multi-Step Checkout

#figure(
  table(
    columns: (1fr, 3fr),
    stroke: 0.5pt,
    align: (left + horizon, left),

    [*Actor*], [Customer (Guest or Authenticated)],
    [*Precondition*], [
      The customer's cart contains at least one valid, in-stock item. The customer has a shipping address on file or is prepared to enter one.
    ],
    [*Main Flow*], [
      1. Customer clicks "Proceed to Checkout" from the cart page. \
      2. System presents the checkout interface, showing the current cart contents with item totals. \
      3. Customer selects or enters a shipping address. \
      4. Customer selects a delivery method from available shipping options. \
      5. Customer selects a payment method and provides payment details. \
      6. Customer reviews the order summary (items, shipping cost, tax, total) and clicks "Place Order". \
      7. System begins an atomic transaction: creates the order record, reserves inventory quantities for each line item, processes the payment through the configured gateway, and clears the cart. \
      8. System displays the order confirmation page with the order number and summary.
    ],
    [*Postcondition*], [
      An order record is created with status "Placed". Inventory quantities for each ordered variant are reserved. A payment intent is linked to the order. The customer's cart is emptied. A confirmation is displayed with the order reference number.
    ],
  ),
  caption: [UC-2: Multi-Step Checkout, the primary e-commerce transaction use case.],
) <tbl-uc-checkout>
