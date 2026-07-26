==== Payment Processing

// Diagram placeholder: Payment Processing use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-PAY-01], [Capture payment], [Administrator],
    [Capture an authorised payment intent to transfer funds from the customer's account.],
    [Payment captured and funds transferred. Order payment state updated.],
    [PAY-FR-03],
    [UC-ADM-PAY-02], [Refund payment], [Administrator],
    [Issue a refund against a captured payment. The refund amount must not exceed the captured amount.],
    [Refund processed. Funds returned to the customer. Payment state updated.],
    [PAY-FR-03, PAY-FR-05],
    [UC-ADM-PAY-03], [Void payment], [Administrator],
    [Void an authorised but un-captured payment, releasing the fund hold without charging the customer.],
    [Payment voided and fund hold released. Order payment state updated.],
    [PAY-FR-09],
    [UC-ADM-PAY-04], [View payments], [Administrator],
    [List payments with filtering and view individual payment detail.],
    [Payment records displayed. Gateway state and system state shown together.],
    [PAY-FR-07],
  ),
  caption: [Administrator use cases — Payment Processing.],
)

==== Payment Method Configuration

// Diagram placeholder: Payment Method Configuration use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-ADM-PAY-05], [Manage payment methods], [Administrator],
    [Create, update, activate, deactivate, or remove payment methods. Configure gateway-specific parameters per method.],
    [Payment method configuration updated. Active methods available for storefront selection.],
    [PAY-FR-10],
  ),
  caption: [Administrator use cases — Payment Method Configuration.],
)
