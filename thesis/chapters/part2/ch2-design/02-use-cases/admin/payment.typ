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
    [*Goal*], [Capture an authorised payment intent to transfer funds from the customer's account.],
    [*Trigger*], [Administrator selects the capture action on an authorised payment.],
    [*Preconditions*], [
      - Administrator is authenticated with payment capture permissions.
      - Payment intent is authorised.
    ],
    [*Postconditions*], [
      - Payment captured and funds transferred.
      - Order payment state updated.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Opens the payment detail view for an order.
      2. System -- Displays the current payment state, authorised amount, and capture eligibility.
      3. Administrator -- Optionally adjusts the capture amount (partial capture) not exceeding the authorised amount.
      4. Administrator -- Confirms the capture action.
      5. System -- Sends the capture request to the payment gateway with an idempotency key.
      6. System -- Receives confirmation from the gateway.
      7. System -- Updates the payment state to captured.
      8. System -- Updates the order payment state accordingly.
      9. System -- Confirms successful capture.
    ],
    [*Alternative Flows*], [
      A1. Partial capture -- Administrator specifies a capture amount less than the authorised amount; the remaining authorised amount can be captured later.
      A2. Capture amount exceeds authorised amount -- System rejects and displays the maximum capturable amount.
    ],
    [*Exception Flows*], [
      E1. Payment gateway rejects the capture -- System reports the rejection reason from the gateway and suggests the administrator investigate.
      E2. Payment gateway is unreachable -- System reports the failure and suggests retrying; the idempotency key ensures safe retry.
      E3. Payment was already captured by a concurrent operation -- System detects the duplicate and reports the existing capture result.
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
    [*Goal*], [Issue a refund against a captured payment, not exceeding the captured amount.],
    [*Trigger*], [Administrator selects the refund action on a captured payment.],
    [*Preconditions*], [
      - Administrator is authenticated with refund permissions.
      - Payment is in captured state.
    ],
    [*Postconditions*], [
      - Refund processed.
      - Funds returned to the customer.
      - Payment state updated.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Opens the payment detail view for a captured payment.
      2. System -- Displays the captured amount and refund eligibility.
      3. Administrator -- Enters the refund amount (full or partial) not exceeding the captured amount.
      4. Administrator -- Provides a refund reason.
      5. Administrator -- Confirms the refund.
      6. System -- Validates that the refund amount does not exceed the captured amount.
      7. System -- Sends the refund request to the payment gateway with an idempotency key.
      8. System -- Receives confirmation from the gateway.
      9. System -- Updates the payment state to refunded (or partially refunded).
      10. System -- Confirms the refund and displays the refund transaction details.
    ],
    [*Alternative Flows*], [
      A1. Partial refund -- Administrator issues a refund for less than the full captured amount; the remaining amount can be refunded later.
      A2. Multiple partial refunds -- System tracks cumulative refunded amount and prevents the total refunds from exceeding the captured amount.
      A3. Refund amount exceeds captured amount -- System rejects and displays the maximum refundable amount.
    ],
    [*Exception Flows*], [
      E1. Payment gateway rejects the refund -- System reports the rejection reason from the gateway and suggests the administrator investigate.
      E2. Payment gateway is unreachable -- System reports the failure and suggests retrying; the idempotency key ensures safe retry.
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
    [*Goal*], [Void an authorised but un-captured payment, releasing the fund hold on the customer's account.],
    [*Trigger*], [Administrator selects the void action on an authorised payment.],
    [*Preconditions*], [
      - Administrator is authenticated with payment management permissions.
      - Payment is authorised but not captured.
    ],
    [*Postconditions*], [
      - Payment voided and fund hold released.
      - Order payment state updated.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Opens the payment detail view for an authorised payment.
      2. System -- Displays the authorised amount and void eligibility.
      3. Administrator -- Selects the void action.
      4. System -- Displays a confirmation prompt explaining that the fund hold will be released.
      5. Administrator -- Confirms the void.
      6. System -- Sends the void request to the payment gateway with an idempotency key.
      7. System -- Receives confirmation from the gateway.
      8. System -- Updates the payment state to voided.
      9. System -- Confirms successful void.
    ],
    [*Alternative Flows*], [
      A1. Payment was already captured -- System prevents void and suggests a refund instead (see UC-ADM-PAY-02).
      A2. Payment is in a state that does not support void -- System displays the current payment state and explains why void is not available.
    ],
    [*Exception Flows*], [
      E1. Payment gateway rejects the void -- System reports the rejection reason from the gateway.
      E2. Payment gateway is unreachable -- System reports the failure and suggests retrying.
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
    [*Goal*], [List payments with filtering and view individual payment detail including gateway state and system state.],
    [*Trigger*], [Administrator navigates to the payment management interface.],
    [*Preconditions*], [
      - Administrator is authenticated with payment viewing permissions.
    ],
    [*Postconditions*], [
      - Payment records displayed.
      - Gateway state and system state shown together for each payment.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the payment management interface.
      2. System -- Displays the payment list with default sorting (most recent first) showing payment ID, order reference, amount, state, and gateway.
      3. Administrator -- Applies optional filters: payment state, date range, gateway, order number.
      4. System -- Refreshes the listing with filtered results and pagination controls.
      5. Administrator -- Selects an individual payment to view detail.
      6. System -- Displays the full payment detail: amount, currency, gateway, state timeline, associated order reference, capture and refund history.
    ],
    [*Alternative Flows*], [
      A1. No payments match the applied filters -- System displays an empty result message with suggestion to broaden the filter criteria.
      A2. Administrator views a payment with a state mismatch between system and gateway -- System highlights the discrepancy and suggests synchronising with the gateway.
    ],
    [*Exception Flows*], [
      E1. System fails to retrieve payment data -- System displays an error message and offers a retry option.
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
    [*Goal*], [Create, update, activate, deactivate, or remove payment methods with gateway-specific parameters.],
    [*Trigger*], [Administrator navigates to the payment method configuration interface.],
    [*Preconditions*], [
      - Administrator is authenticated with payment method management permissions.
    ],
    [*Postconditions*], [
      - Payment method configuration updated.
      - Active methods available for storefront selection.
    ],
    [*Main Success Scenario*], [
      1. Administrator -- Navigates to the payment method configuration interface.
      2. System -- Displays the list of configured payment methods with their activation status.
      3. Administrator -- Creates a new payment method with a name, description, gateway identifier, and gateway-specific parameters.
      4. Administrator -- Optionally edits, activates, deactivates, or removes existing payment methods.
      5. Administrator -- Saves the changes.
      6. System -- Validates that the method name is unique and the gateway identifier is supported.
      7. System -- Persists the payment method configuration.
      8. System -- Confirms the changes.
    ],
    [*Alternative Flows*], [
      A1. Administrator deactivates a payment method currently in use by active orders -- System warns that new orders cannot use this method but existing orders remain unaffected.
      A2. Administrator removes a payment method -- System verifies no pending payments reference the method and asks for confirmation.
      A3. Gateway parameters are invalid for the selected gateway -- System rejects and highlights the invalid parameters.
    ],
    [*Exception Flows*], [
      E1. Concurrent modification detected -- System detects the payment method was modified by another session, refreshes the data, and asks the administrator to retry.
    ],
    [*Related Requirements*], [PAY-FR-10],
  ),
  caption: [UC-ADM-PAY-05 -- Manage Payment Methods.],
)
