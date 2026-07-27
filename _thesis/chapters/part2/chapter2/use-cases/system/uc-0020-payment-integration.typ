#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/system/uc-0020-payment-integration.png", width: 100%),
  caption: [Use Case Diagram for UC-0020],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0020*], [*Payment Integration*],
    [Actor], [System],
    [Description], [Securely process payments via Stripe Gateway.],
    [Trigger], [Checkout (UC-0002).],
    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [System creates Payment Intent.],
      [2], [Customer confirms payment on Client.],
      [3], [System verifies Webhook/Confirmation.],
      [4], [System captures funds.],
    ),

    [Related Use Cases], [UC-0002 (Checkout)],
  ),
  caption: [UC-0020: Payment Integration],
)

Payment Integration securely handles the interaction with external payment gateways (e.g., Stripe). It manages the "Payment Intent" lifecycle, from initial authorization during checkout (UC-0002) to the final capture of funds upon successful order validation. This isolation ensures that sensitive financial data is handled compliantly and that payment status updates are accurately reflected in the order history.
