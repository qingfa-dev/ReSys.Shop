# Promotions Module

Domain-driven design module for promotion, coupon, and discount management. Defines the aggregate roots `Promotion`, `CouponCode`, and `PromotionCategory` along with supporting entities (`PromotionAction`, `PromotionRule`, `OrderPromotion`). Implements 5 promotion action types and 11 promotion rule types ported directly from the Spree Ruby SDK, plus handler services for cart, coupon, free shipping, page-based, and duplication workflows.

Each entity follows a sealed partial class pattern split across per-concern files: Extensions (factory + business methods), Validation (FluentValidation rule builders), Constant (defaults/constraints), Result (success/error definitions), Loggers (structured logging), and Enumerate (enums). Business logic returns `Result&lt;T&gt;` to encode success or failure without exceptions. CAT-10 code annotations (Contract, Invariant, Validate, Enforce, Compute, Guard) provide machine-parseable contracts at all module entry points.

Batch coupon code generation supports up to 10,000 codes per call with optional prefix and dedup. Promotion rules support any/all/none match policies, hierarchical taxon matching, and multi-currency thresholds via the ItemTotal rule. See `README.xml` for the full abstractions catalog with file structure, usage scenarios, and architectural principles.
