==== Payment Processing

// Diagram placeholder: Payment Processing use case diagram
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-admin-payment-processing.png",
    width: 100%
  ),
  caption: [Use case diagram for Payment Processing (UC-ADM-PAY).],
) <fig-uc-adm-pay-d>

==== UC-ADM-PAY — Manage Payments

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-PAY],
    [*Use Case Name*], [Manage Payments],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [Capture, refund, void, and review payments.],
    [*Trigger*], [Administrator navigates to payment management.],
    [*Preconditions*], [
      - Authenticated with payment management permissions.
    ],
    [*Postconditions*], [
      - Payment state updated. Gateway operations recorded.
    ],
    [*Main Success Scenario*], [
      *Capture Payment*
      1. Opens payment detail view for an order.
      2. System displays payment state, authorised amount, and capture eligibility.
      3. Optionally adjusts capture amount (partial) not exceeding authorised amount.
      4. Confirms. System sends capture request to gateway with idempotency key, updates state to captured, and confirms.
      ,
      *Refund Payment*
      1. Opens payment detail view for a captured payment.
      2. Enters refund amount (full or partial) and reason.
      3. Confirms. System validates amount, sends refund to gateway, updates state, and confirms.
      ,
      *Void Payment*
      1. Opens payment detail view for an authorised but un-captured payment.
      2. Selects void action and confirms.
      3. System sends void request to gateway, updates state to voided, and confirms.
      ,
      *View Payments*
      1. Navigates to payment management.
      2. System displays payment list sorted by most recent.
      3. Applies optional filters: state, date range, gateway, order number.
      4. Selects a payment to view full detail: amount, currency, gateway, state timeline, order reference, capture and refund history.
    ],
    [*Alternative Flows*], [
      A1. Partial capture/refund: amount less than authorised/captured; remainder available.
      A2. Already captured (Void): system prevents and suggests refund instead.
      A3. State mismatch (View): system highlights discrepancy and suggests syncing.
    ],
    [*Exception Flows*], [
      E1. Gateway rejects: system reports rejection reason.
      E2. Gateway unreachable: system reports failure; idempotency key ensures safe retry.
    ],
    [*Related Requirements*], [PAY-FR-03, PAY-FR-05, PAY-FR-07, PAY-FR-09],
  ),
    kind: table,
  caption: [Manage Payments.],
)

==== Payment Method Configuration

// Diagram placeholder: Payment Method Configuration use case diagram
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-payment-method-config.png",
    width: 100%
  ),
  caption: [Use case diagram for Payment Method Configuration (UC-ADM-PAY-METHOD).],
) <fig-uc-adm-paym-d>

==== UC-ADM-PAY-METHOD — Manage Payment Methods

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-PAY-METHOD],
    [*Use Case Name*], [Manage Payment Methods],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Create, update, activate, and deactivate payment methods.],
    [*Trigger*], [Administrator navigates to payment method configuration.],
    [*Preconditions*], [
      - Authenticated with payment method management permissions.
    ],
    [*Postconditions*], [
      - Payment methods updated and available for storefront selection.
    ],
    [*Main Success Scenario*], [
      1. Navigates to payment method configuration.
      2. System displays configured payment methods with activation status.
      3. Creates a new method with name, description, gateway identifier, and parameters.
      4. Optionally edits, activates, deactivates, or removes methods.
      5. Saves. System validates name uniqueness and gateway support. Persists and confirms.
    ],
    [*Alternative Flows*], [
      A1. Deactivate method in use: system warns new orders cannot use it; existing unaffected.
      A2. Remove method: system verifies no pending payments reference it.
      A3. Invalid gateway parameters: system rejects and highlights invalid ones.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification: system refreshes and asks to retry.
    ],
    [*Related Requirements*], [PAY-FR-10],
  ),
    kind: table,
  caption: [Manage Payment Methods.],
)
