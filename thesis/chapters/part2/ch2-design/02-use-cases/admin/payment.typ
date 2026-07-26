==== Payment Processing

// Diagram placeholder: Payment Processing use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-PAY-01], [Capture payment], [Admin], [Capture an authorised payment intent to transfer funds from the customer's account.], [Admin is authenticated with payment capture permissions. Payment intent is authorised.], [Payment captured and funds transferred. Order payment state updated.],
  [UC-ADM-PAY-02], [Refund payment], [Admin], [Issue a refund against a captured payment, not exceeding the captured amount.], [Admin is authenticated with refund permissions. Payment is in captured state.], [Refund processed. Funds returned to the customer. Payment state updated.],
  [UC-ADM-PAY-03], [Void payment], [Admin], [Void an authorised but un-captured payment, releasing the fund hold.], [Admin is authenticated. Payment is authorised but not captured.], [Payment voided and fund hold released. Order payment state updated.],
  [UC-ADM-PAY-04], [View payments], [Admin], [List payments with filtering and view individual payment detail.], [Admin is authenticated with payment viewing permissions.], [Payment records displayed. Gateway state and system state shown together.],
)

==== Payment Method Configuration

// Diagram placeholder: Payment Method Configuration use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-ADM-PAY-05], [Manage payment methods], [Admin], [Create, update, activate, deactivate, or remove payment methods with gateway-specific parameters.], [Admin is authenticated with payment method management permissions.], [Payment method configuration updated. Active methods available for storefront selection.],
)
