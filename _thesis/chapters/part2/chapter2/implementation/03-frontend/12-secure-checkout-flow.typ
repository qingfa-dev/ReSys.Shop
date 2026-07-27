===== 3. Secure Checkout
A distraction-free *Multi-Step Wizard* guides the user through the payment process. By isolating the checkout steps (Shipping, Method, Payment, Review) into distinct views, the interface reduces cognitive load and form fatigue.

// TODO: [Implementation] Add Checkout Stepper screenshot.
// #figure(
//   figure-placeholder("Checkout Stepper UI"),
//   caption: [Multi-step checkout stepper indicating progress and current validation state.],
// )

The checkout interface implements a "Linear Success Path" that strictly enforces the transactional sequence defined in @fig:sq-0002-checkout. To reduce cognitive load, the complex `PlaceOrder` requirement is decomposed into four isolated Validation Gates:

1. *Shipping Gate:* Validates physical address constraints. Upon success, triggers `SetShippingAddressCommand`.
2. *Method Gate:* Dynamic rate calculation based on the validated address. Forces selection of a valid carrier service.
3. *Payment Gate:* Secure handshake with the Payment Gateway. This step isolates the sensitive PCI-DSS handling component.
4. *Review Gate:* The final "Commit" state. The "Place Order" button is only enabled when the backend confirms valid states for all previous steps.

This structural alignment between the UI Stepper and the Backend State Machine ensures that invalid transactions are caught at the boundary, preventing "surprise" failures at the end of the funnel.


The secure checkout flow orchestrates multiple complex operations, including address validation, shipping rate calculation via the Logistics module, and final payment processing. Each step is guarded by a validation layer that ensures all required domain data is present before allowing the transition to the final `PlaceOrder` command.

*Step-by-Step Transaction Sequence (UC-0002):*
1. *Identity Verification:* The system checks the session status. For guest users, it triggers the Cart-Merge logic upon login.
2. *Inventory Pre-Check:* Before the "Payment" step is enabled, the backend performs a soft-check on stock levels to prevent users from attempting to pay for out-of-stock items.
3. *Secure Payment Handshake:* The frontend communicates with the Payment Gateway (Stripe) to create a `PaymentIntent`. The system waits for a successful webhook or client-side confirmation before proceeding.
4. *Final Order Atomic Commit:* Upon payment success, the `PlaceOrder` command is dispatched. This triggers the *Atomic Stock Reservation* logic on the backend, ensuring the physical items are deducted exactly once.

*Transaction Lifecycle:*
- *UI State (Wizard):* The frontend enforces a strict linear progression using *Navigation Guards* (`beforeEnter` hooks). Users cannot manually navigate to "Step 3: Payment" without a valid "Shipping Token" in the Pinia store, effectively preventing out-of-order state corruption.
- *Sequence Flow:* As detailed in @fig:sq-0002-checkout, the backend acts as the authoritative State Machine. Each step (shipping, method, payment) is a discrete transaction that validates domain constraints before allowing the final `PlaceOrder` command.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/customer/sq-0002-checkout.png", width: 75%),
  caption: [Checkout Transaction Sequence: The orchestration of API calls during the multi-step checkout process (UC-0002).],
) <fig:sq-0002-checkout>

/*
  PCI Compliance Strategy (UC-0020)
*/
*Hosted Fields Pattern:*
- *UI Isolation:* The credit card input fields are not standard HTML inputs but are *iframes* served directly from Stripe's CDN. This visual trickery makes the fields look native while ensuring that sensitive PAN data never enters the ReSys DOM or memory space.
- *Sequence Flow:* @fig:sq-0020-payment shows the secure handshake. The browser tokenizes the card directly with the Gateway. The backend receives only a sanitized `pm_token`, drastically reducing the PCI-DSS audit scope from SAQ-D to SAQ-A.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/system/sq-0020-payment-integration.png", width: 80%),
  caption: [Payment Integration: Secure token exchange using Hosted Fields pattern (UC-0020).],
) <fig:sq-0020-payment>
