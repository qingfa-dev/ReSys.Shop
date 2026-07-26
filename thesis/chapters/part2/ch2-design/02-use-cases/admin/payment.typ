==== Payment Processing
// Diagram placeholder for Payment Processing

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-ADM-PAY-01], [Capture payment], [Admin],
    [Capture an authorised payment intent, transferring funds from the customer's account. Idempotency key prevents duplicate capture.],
    [Payment captured; funds transferred. Order payment state updated to captured.],
    [PAY-FR-03],
    [UC-ADM-PAY-02], [Refund payment], [Admin],
    [Issue a refund against a captured payment. Validate that refund amount does not exceed captured amount.],
    [Refund processed; payment state updated to refunded (partial or full). Funds returned to customer.],
    [PAY-FR-03, PAY-FR-05],
    [UC-ADM-PAY-03], [Void payment], [Admin],
    [Void an authorised but un-captured payment, releasing the fund hold without charging the customer.],
    [Payment voided; fund hold released. Order payment state updated.],
    [PAY-FR-09],
    [UC-ADM-PAY-04], [View payments], [Admin],
    [List payments with paging and filtering by status, date, gateway, and order reference. View individual payment detail.],
    [Payment records displayed with gateway state and system state shown side by side.],
    [PAY-FR-07],
  ),
  caption: [Administrator use cases — Payment Processing.],
)

==== Payment Method Configuration
// Diagram placeholder for Payment Method Configuration

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-ADM-PAY-05], [Manage payment methods], [Admin],
    [Create, update, activate, deactivate, or delete payment methods. Configure gateway-specific parameters per method.],
    [Payment method configuration updated; active methods available for storefront selection.],
    [PAY-FR-10],
  ),
  caption: [Administrator use cases — Payment Method Configuration.],
)
