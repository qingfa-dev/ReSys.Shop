===== Checkout: UC-STR-CHK

Checkout progresses through the five-state pipeline (Address, Delivery, Payment, Confirm, Complete) with a visual progress indicator. Each step renders as a single-page form; each transition is validated server-side.

- *Address:* Saved addresses with radio-button selection and inline "Add new address" form (screenshot below).
- *Delivery:* Available shipping methods with carrier names, delivery estimates, and calculated rates (screenshot below).
- *Payment:* Payment methods with provider icons, selected method confirmed via `POST /api/storefront/payment/create-intent` (screenshot below).
- *Confirm:* Read-only order summary with line items, addresses, method, and totals; "Place Order" button finalises (screenshot below).
- *Complete:* Order confirmation with generated order number, summary, and "Continue Shopping" link (screenshot below).

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-address.png", width: 100%),
//   caption: [Address step: progress bar (Address highlighted), saved addresses with radio buttons, "Add New Address" collapsible form, "Continue to Delivery" button.],
// ) <fig-checkout-address>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-delivery.png", width: 100%),
//   caption: [Delivery step: progress bar (Address complete, Delivery highlighted), 3 shipping methods (Standard, Express, Next-Day) with carrier names, delivery dates, and rates. "Continue to Payment" button.],
// ) <fig-checkout-delivery>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-payment.png", width: 100%),
//   caption: [Payment step: progress bar through Delivery, payment methods (Stripe Card, Cash on Delivery, Bank Transfer) with provider icons and radio buttons. "Continue to Confirm" button.],
// ) <fig-checkout-payment>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-confirm.png", width: 100%),
//   caption: [Confirm step: progress bar through Payment, read-only summary with line items table, address blocks, delivery/payment methods, totals (Item Total, Shipping, Grand Total). "Place Order" button.],
// ) <fig-checkout-confirm>

// #figure(
//   image("../../../../../figures/chapters/part2/ch2-design/04-implementations/screenshots/storefront-checkout-complete.png", width: 100%),
//   caption: [Complete step: all five stages with checkmarks, success icon, "Order #ORD-2025-0042 Confirmed" heading, order summary, "Continue Shopping" button.],
// ) <fig-checkout-complete>
