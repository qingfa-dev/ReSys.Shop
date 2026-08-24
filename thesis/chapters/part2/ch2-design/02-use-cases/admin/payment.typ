==== Payment Processing

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-admin-payment-processing.png",
    width: 70%
  ),
  caption: [Use case diagram for Payment Processing (UC-ADM-PAY).],
) <fig-uc-adm-pay-d>

==== UC-ADM-PAY: Manage Payments

*Goal:* Capture, refund, void, and review payments. *Trigger:* the administrator opens a payment detail view for an order. *Related requirements:* PAY-FR-03, PAY-FR-05, PAY-FR-07, PAY-FR-09. The flow supports partial or full capture and refund through the gateway using idempotency keys, voiding of authorised-but-uncaptured payments, and filtered payment review; alternatives handle state mismatches and already-captured voids, and exceptions cover gateway rejection or unavailability with safe retry.

==== Payment Method Configuration

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-payment-method-config.png",
    width: 70%
  ),
  caption: [Use case diagram for Payment Method Configuration (UC-ADM-PAY-METHOD).],
) <fig-uc-adm-paym-d>

==== UC-ADM-PAY-METHOD: Manage Payment Methods

*Goal:* Create, update, activate, and deactivate payment methods. *Trigger:* the administrator opens payment method configuration. *Related requirements:* PAY-FR-10. The flow creates, edits, activates, deactivates, or removes methods with name and gateway validation; alternatives warn when a method is in use or has pending payments, and an exception handles concurrent modification.
