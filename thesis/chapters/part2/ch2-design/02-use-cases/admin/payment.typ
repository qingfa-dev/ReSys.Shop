==== Payment Processing

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-admin-payment-processing.png",
    width: 70%
  ),
  caption: [Use case diagram for Payment Processing (UC-ADM-PAY).],
) <fig-uc-adm-pay-d>

==== UC-ADM-PAY: Manage Payments

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-PAY — Manage Payments],
    [*Actor*], [Administrator],
    [*Support*], [Payment Gateway],
    [*Goal*], [Capture, refund, void, and review payments.],
    [*Pre/Post*], [
      Pre: authenticated with payment management permissions.
      Post: payment state updated; gateway operations recorded.
    ],
    [*Scenario*], [
      *Capture Payment*
      + Opens payment detail view for order.
      + System displays payment state, authorised amount, capture eligibility.
      + Optionally adjusts capture amount (partial) not exceeding authorised amount.
      + Confirms; system sends capture request to gateway with idempotency key, updates state to captured, confirms.
      ,
      *Refund Payment*
      + Opens payment detail view for captured payment.
      + Enters refund amount (full or partial) and reason.
      + Confirms; system validates amount, sends refund to gateway, updates state, confirms.
      ,
      *Void Payment*
      + Opens payment detail view for authorised but un-captured payment.
      + Selects void action, confirms.
      + System sends void request to gateway, updates state to voided, confirms.
      ,
      *View Payments*
      + Navigates to payment management.
      + System displays payment list sorted by most recent.
      + Applies optional filters (state, date range, gateway, order number).
      + Selects payment to view full detail: amount, currency, gateway, state timeline, order reference, capture and refund history.
      ,
    ],
    [*Alternatives*], [
      + A1. Partial capture/refund → amount less than authorised/captured; remainder available.
      + A2. Already captured (Void) → system prevents, suggests refund instead.
      + A3. State mismatch (View) → system highlights discrepancy, suggests syncing.
    ],
    [*Exceptions*], [
      + E1. Gateway rejects → system reports rejection reason.
      + E2. Gateway unreachable → system reports failure; idempotency key ensures safe retry.
    ],
    [*Requirements*], [PAY-FR-03, PAY-FR-05, PAY-FR-07, PAY-FR-09],
  ),
    kind: table,
  caption: [Manage Payments.],
)

==== Payment Method Configuration

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-payment-method-config.png",
    width: 70%
  ),
  caption: [Use case diagram for Payment Method Configuration (UC-ADM-PAY-METHOD).],
) <fig-uc-adm-paym-d>

==== UC-ADM-PAY-METHOD: Manage Payment Methods

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-ADM-PAY-METHOD — Manage Payment Methods],
    [*Actor*], [Administrator],
    [*Goal*], [Create, update, activate, and deactivate payment methods.],
    [*Pre/Post*], [
      Pre: authenticated with payment method management permissions.
      Post: payment methods updated and available for storefront selection.
    ],
    [*Scenario*], [
      + Navigates to payment method configuration.
      + System displays configured payment methods with activation status.
      + Creates new method with name, description, gateway identifier, and parameters.
      + Optionally edits, activates, deactivates, or removes methods.
      + Saves; system validates name uniqueness and gateway support, persists, confirms.
    ],
    [*Alternatives*], [
      + A1. Deactivate method in use → system warns new orders cannot use it; existing unaffected.
      + A2. Remove method → system verifies no pending payments reference it.
      + A3. Invalid gateway parameters → system rejects, highlights invalid ones.
    ],
    [*Exceptions*], [
      + E1. Concurrent modification → system refreshes, asks to retry.
    ],
    [*Requirements*], [PAY-FR-10],
  ),
    kind: table,
  caption: [Manage Payment Methods.],
)
