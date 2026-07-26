==== Payment Processing

// Diagram placeholder: Payment Processing use case diagram

==== UC-ADM-PAY-01 — Capture Payment

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-PAY-01],
    [*Use Case Name*], [Capture Payment],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [Capture an authorised payment to transfer funds.],
    [*Trigger*], [Administrator selects the capture action on an authorised payment.],
    [*Preconditions*], [
      - Authenticated with payment capture permissions.
      - Payment authorised.
    ],
    [*Postconditions*], [
      - Payment captured. Order payment state updated.
    ],
    [*Main Success Scenario*], [
      1. Opens payment detail view for an order.
      2. System displays payment state, authorised amount, and capture eligibility.
      3. Optionally adjusts capture amount (partial) not exceeding authorised amount.
      4. Confirms the capture.
      5. System sends capture request to gateway with idempotency key.
      6. System receives gateway confirmation.
      7. System updates payment state to captured.
      8. System updates order payment state.
      9. System confirms successful capture.
    ],
    [*Alternative Flows*], [
      A1. Partial capture: amount less than authorised; remainder capturable later.
      A2. Exceeds authorised: system rejects and shows maximum.
    ],
    [*Exception Flows*], [
      E1. Gateway rejects: system reports rejection reason.
      E2. Gateway unreachable: system reports failure; idempotency key ensures safe retry.
      E3. Already captured concurrently: system detects duplicate and reports existing result.
    ],
    [*Related Requirements*], [PAY-FR-03],
  ),
  caption: [UC-ADM-PAY-01 -- Capture Payment.],
)

==== UC-ADM-PAY-02 — Refund Payment

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-PAY-02],
    [*Use Case Name*], [Refund Payment],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [Issue a refund against a captured payment.],
    [*Trigger*], [Administrator selects the refund action on a captured payment.],
    [*Preconditions*], [
      - Authenticated with refund permissions.
      - Payment is captured.
    ],
    [*Postconditions*], [
      - Refund processed. Payment state updated.
    ],
    [*Main Success Scenario*], [
      1. Opens payment detail view for a captured payment.
      2. System displays captured amount and refund eligibility.
      3. Enters refund amount (full or partial) not exceeding captured amount.
      4. Provides a refund reason.
      5. Confirms the refund.
      6. System validates amount does not exceed captured amount.
      7. System sends refund request to gateway with idempotency key.
      8. System receives gateway confirmation.
      9. System updates payment state to refunded or partially refunded.
      10. System confirms refund and displays transaction details.
    ],
    [*Alternative Flows*], [
      A1. Partial refund: remaining amount refundable later.
      A2. Multiple partial refunds: system tracks cumulative amount and prevents exceeding.
      A3. Exceeds captured: system rejects and shows maximum.
    ],
    [*Exception Flows*], [
      E1. Gateway rejects: system reports rejection reason.
      E2. Gateway unreachable: system reports failure; idempotency key ensures safe retry.
    ],
    [*Related Requirements*], [PAY-FR-03, PAY-FR-05],
  ),
  caption: [UC-ADM-PAY-02 -- Refund Payment.],
)

==== UC-ADM-PAY-03 — Void Payment

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-PAY-03],
    [*Use Case Name*], [Void Payment],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [Void an authorised but un-captured payment, releasing the fund hold.],
    [*Trigger*], [Administrator selects the void action on an authorised payment.],
    [*Preconditions*], [
      - Authenticated with payment management permissions.
      - Payment authorised but not captured.
    ],
    [*Postconditions*], [
      - Payment voided. Fund hold released.
    ],
    [*Main Success Scenario*], [
      1. Opens payment detail view for an authorised payment.
      2. System displays authorised amount and void eligibility.
      3. Selects the void action.
      4. System displays confirmation prompt.
      5. Confirms the void.
      6. System sends void request to gateway with idempotency key.
      7. System receives gateway confirmation.
      8. System updates payment state to voided.
      9. System confirms successful void.
    ],
    [*Alternative Flows*], [
      A1. Already captured: system prevents and suggests refund (UC-ADM-PAY-02).
      A2. State doesn't support void: system displays current state and explains.
    ],
    [*Exception Flows*], [
      E1. Gateway rejects: system reports rejection reason.
      E2. Gateway unreachable: system reports failure and suggests retry.
    ],
    [*Related Requirements*], [PAY-FR-09],
  ),
  caption: [UC-ADM-PAY-03 -- Void Payment.],
)

==== UC-ADM-PAY-04 — View Payments

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-PAY-04],
    [*Use Case Name*], [View Payments],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [List payments with filters; view payment detail including gateway and system state.],
    [*Trigger*], [Administrator navigates to payment management.],
    [*Preconditions*], [
      - Authenticated with payment viewing permissions.
    ],
    [*Postconditions*], [
      - Payment records displayed with gateway and system state.
    ],
    [*Main Success Scenario*], [
      1. Navigates to payment management.
      2. System displays payment list sorted by most recent.
      3. Applies optional filters: state, date range, gateway, order number.
      4. System refreshes listing with pagination.
      5. Selects a payment to view detail.
      6. System displays full payment detail: amount, currency, gateway, state timeline, order reference, capture and refund history.
    ],
    [*Alternative Flows*], [
      A1. No payments match: system displays empty message with suggestion to broaden filters.
      A2. State mismatch: system highlights discrepancy and suggests syncing.
    ],
    [*Exception Flows*], [
      E1. Retrieval failure: system displays error and offers retry.
    ],
    [*Related Requirements*], [PAY-FR-07],
  ),
  caption: [UC-ADM-PAY-04 -- View Payments.],
)

==== Payment Method Configuration

// Diagram placeholder: Payment Method Configuration use case diagram

==== UC-ADM-PAY-05 — Manage Payment Methods

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-ADM-PAY-05],
    [*Use Case Name*], [Manage Payment Methods],
    [*Primary Actor*], [Administrator],
    [*Supporting Actors*], [None],
    [*Goal*], [Manage payment methods with gateway-specific parameters.],
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
      5. Saves the changes.
      6. System validates name uniqueness and gateway support.
      7. System persists the configuration.
      8. System confirms the changes.
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
  caption: [UC-ADM-PAY-05 -- Manage Payment Methods.],
)
