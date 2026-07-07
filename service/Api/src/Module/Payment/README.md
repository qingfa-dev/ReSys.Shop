# Payment Module

Domain entities and services for payment processing in the ReSys eCommerce platform. Ported from Spree::Payment, Spree::PaymentMethod, Spree::Gateway, Spree::StoreCredit, Spree::CreditCard, Spree::Refund, Spree::RefundReason, and all supporting models.

Provides payment lifecycle management (authorize, capture, void, credit), gateway abstraction via `IPaymentGatewayActionProvider`, store credit issuance and redemption, tokenized credit card storage, and refund processing with reason tracking. Includes a test-only `BogusGateway` with known test card numbers for development.

Each entity follows a sealed partial class pattern split across per-concern files: Extensions (factory + business methods), Validation (FluentValidation rule builders), Constant (defaults/constraints), Result (success/error definitions), Loggers (structured logging), and Enumerate (enums). Business logic returns `Result&lt;T&gt;` to encode success or failure without exceptions. CAT-10 code annotations (Contract, Invariant, Validate, Enforce, Compute, Guard) provide machine-parseable contracts at all module entry points.

Create entities via static factory extension methods, mutate via fluent extension methods, and validate using the `Apply*Rules` FluentValidation builders. See `README.xml` for the full abstractions catalog with file structure, usage scenarios, and architectural principles.
