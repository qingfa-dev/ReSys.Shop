# Payment Domain

DDD domain models: Payments, PaymentMethods, Gateways, CaptureEvents, RefundReasons.

## Aggregates

| Aggregate | Path | Purpose |
|-----------|------|---------|
| Payments | `Payments/` | Payment transactions |
| PaymentMethods | `PaymentMethods/` | Stored payment methods |
| Gateways | `Gateways/` | Payment gateway definitions |
| CaptureEvents | `PaymentCaptureEvents/` | Payment capture audit |
| RefundReasons | `RefundReasons/` | Refund reason codes |

## Category

Domain-Driven Design · Payments
