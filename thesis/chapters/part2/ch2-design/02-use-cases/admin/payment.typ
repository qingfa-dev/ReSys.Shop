==== Payment Processing

// Diagram placeholder: Payment Processing use case diagram

*UC-ADM-PAY-01 — Capture payment.*
*Primary Actor:* Administrator. \
*Main Flow:* Capture an authorised payment intent to transfer funds from the customer's account. \
*Postcondition:* Payment captured and funds transferred. Order payment state updated. \
*Related FR:* PAY-FR-03.

#v(0.5cm)
*UC-ADM-PAY-02 — Refund payment.*
*Primary Actor:* Administrator. \
*Main Flow:* Issue a refund against a captured payment. The refund amount must not exceed the captured amount. \
*Postcondition:* Refund processed. Funds returned to the customer. Payment state updated. \
*Related FR:* PAY-FR-03, PAY-FR-05.

#v(0.5cm)
*UC-ADM-PAY-03 — Void payment.*
*Primary Actor:* Administrator. \
*Main Flow:* Void an authorised but un-captured payment, releasing the fund hold without charging the customer. \
*Postcondition:* Payment voided and fund hold released. Order payment state updated. \
*Related FR:* PAY-FR-09.

#v(0.5cm)
*UC-ADM-PAY-04 — View payments.*
*Primary Actor:* Administrator. \
*Main Flow:* List payments with filtering and view individual payment detail. \
*Postcondition:* Payment records displayed. Gateway state and system state shown together. \
*Related FR:* PAY-FR-07.

==== Payment Method Configuration

// Diagram placeholder: Payment Method Configuration use case diagram

*UC-ADM-PAY-05 — Manage payment methods.*
*Primary Actor:* Administrator. \
*Main Flow:* Create, update, activate, deactivate, or remove payment methods. Configure gateway-specific parameters per method. \
*Postcondition:* Payment method configuration updated. Active methods available for storefront selection. \
*Related FR:* PAY-FR-10.
