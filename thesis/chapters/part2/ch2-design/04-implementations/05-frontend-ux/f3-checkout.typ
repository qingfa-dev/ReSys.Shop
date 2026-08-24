===== Checkout: UC-STR-CHK

Checkout progresses through the five-state pipeline (Address, Delivery, Payment, Confirm, Complete) with a Stepper wizard. Each step renders as a single-page form; each transition is validated server-side.

- *Address:* Saved addresses with picker selection and inline "Add new address" form (screenshot below).
- *Delivery:* Available shipping methods with carrier names, delivery estimates, and calculated rates (screenshot below).
- *Payment:* Payment methods with provider icons, selected method confirmed via payment-intent creation (screenshot below).
- *Confirm:* Read-only order summary with line items, addresses, method, and totals; "Place Order" button finalises (screenshot below).
- *Complete:* Order confirmation with generated order number, summary, and "Continue Shopping" link (screenshot below).

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-address.png", width: 100%),
  caption: [Address step: stepper wizard, saved-address picker with inline add form.],
) <fig-checkout-address>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-delivery.png", width: 100%),
  caption: [Delivery step: shipping methods with carriers, delivery dates, and rates.],
) <fig-checkout-delivery>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-payment.png", width: 100%),
  caption: [Payment step: stepper wizard with Credit Card and Cash on Delivery options.],
) <fig-checkout-payment>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-confirm.png", width: 100%),
  caption: [Confirm step: read-only summary with totals and Place Order action.],
) <fig-checkout-confirm>

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-complete.png", width: 100%),
  caption: [Complete step: confirmation with generated order number and summary.],
) <fig-checkout-complete>
