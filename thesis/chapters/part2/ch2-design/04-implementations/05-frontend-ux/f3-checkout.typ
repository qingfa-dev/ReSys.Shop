===== Checkout: UC-STR-CHK

Checkout progresses through the five-state pipeline (Address, Delivery, Payment, Confirm, Complete) with a Stepper wizard. Each step renders as a single-page form; each transition is validated server-side.

- *Address:* Saved addresses with picker selection and inline "Add new address" form (screenshot below).
- *Delivery:* Available shipping methods with carrier names, delivery estimates, and calculated rates (screenshot below).
- *Payment:* Payment methods with provider icons, selected method confirmed via payment-intent creation (screenshot below).
- *Confirm:* Read-only order summary with line items, addresses, method, and totals; "Place Order" button finalises (screenshot below).
- *Complete:* Order confirmation with generated order number, summary, and "Continue Shopping" link (screenshot below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-address.png", width: 100%),
  caption: [Address step: Stepper wizard (Address highlighted), saved-address picker with inline "Add New Address" form, "Continue to Delivery" button.],
) <fig-checkout-address>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-delivery.png", width: 100%),
  caption: [Delivery step: Stepper wizard (Address complete, Delivery highlighted), shipping methods with carrier names, delivery dates, and rates. "Continue to Payment" button.],
) <fig-checkout-delivery>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-payment.png", width: 100%),
  caption: [Payment step: Stepper wizard through Delivery, payment methods (Credit Card, Cash on Delivery, Bogus Test Card) with radio buttons. "Continue" button.],
) <fig-checkout-payment>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-confirm.png", width: 100%),
  caption: [Confirm step: Stepper wizard through Payment, read-only summary with line items table, address blocks, delivery/payment methods, totals. "Place Order" button.],
) <fig-checkout-confirm>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-complete.png", width: 100%),
  caption: [Complete step: Stepper wizard all five stages with checkmarks, success confirmation with generated order number, order summary, "Continue Shopping" link.],
) <fig-checkout-complete>
