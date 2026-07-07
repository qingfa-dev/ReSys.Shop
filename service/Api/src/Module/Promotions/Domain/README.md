# Promotions Domain

DDD domain models: Promotions, PromotionRules, PromotionActions, CouponCodes, OrderPromotions, Calculators.

## Aggregates

| Aggregate | Path | Purpose |
|-----------|------|---------|
| Promotions | `Promotions/` | Promotion aggregate (active/inactive) |
| PromotionRules | `PromotionRules/` | Eligibility rules |
| PromotionActions | `PromotionActions/` | Applied actions (discount, free shipping) |
| CouponCodes | `CouponCodes/` | Discount codes |
| OrderPromotions | `OrderPromotions/` | Applied promotion tracking |
| Calculators | `Calculators/` | Promotion calculation services |

## Category

Domain-Driven Design · Promotions
