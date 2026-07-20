---
goal: Adopt Code Commenting Standard v3.0 Across All Handlers, Services, and Stores
version: 1.0
date_created: 2026-07-20
date_completed: 2026-07-20
status: Completed
tags: process, documentation, commenting, adoption, quality
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Apply the structured Code Commenting Standard v3.0 (`guide/code-commenting/`) systematically across every handler, service, endpoint, interface, store, and API client in the ReSys.Shop codebase. The standard defines 10 label categories (CAT-1 to CAT-10), 8 temporal markers, and per-language doc-comment requirements, optimized for both human developers and AI coding agents. Root authority: `guide/code-commenting/CommentingRules.xml`.

## 1. Requirements & Constraints

- **REQ-001**: Every `.Endpoint.cs` file must carry at minimum a `<summary>` doc-comment on the `Endpoint` class and one inline label on the route registration.
- **REQ-002**: Every C# service interface (`I*Service.cs`) must carry XML doc-comments (`<summary>`, `<param>`, `<returns>`) on all method signatures.
- **REQ-003**: Every C# handler feature file must already carry XML doc-comments on `Handle` methods plus CAT-10 `Contract:` annotations — validate existing coverage, add where missing.
- **REQ-004**: Every Pinia store file must carry JSDoc/TSDoc on all public methods and computed properties.
- **REQ-005**: Every TypeScript API client file must carry TSDoc on all exported functions and the class.
- **REQ-006**: All loggers, constants, result, and partial class files must carry XML doc-comments on public surfaces.
- **CON-001**: No existing comments shall be removed or degraded — only added or upgraded to v3.0 format.
- **CON-002**: Inline labels must follow the `// Label: Capitalised imperative sentence.` format (F1-F10).
- **CON-003**: CAT-10 annotations must use `KEY=VALUE` form (F9).
- **CON-004**: `CommentingRules.xml` is the authoritative source; the README is generated from it.
- **PAT-001**: Follow the existing vertical-slice feature file pattern observed in well-commented handlers (e.g., `ListProducts.cs`, `StripeWebhook.cs`).
- **PAT-002**: Use the existing comment style from well-commented areas as the template for uncommented files of the same type.

## 2. Implementation Steps

### Implementation Phase 1 — C# Endpoint Files (Systematic Gap)

- GOAL-001: Add v3.0-standard comments to all ~250 `.Endpoint.cs` files across Catalog, Identity, Ordering, Payment, Inventory, Location, and Shipping modules (both Storefront and Admin).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Catalog Storefront endpoint files: add `<summary>` doc-comment on Endpoint class + inline `// Map:` label on the route registration (12 files: ListProducts, GetProductDetail, GetSimilarProducts, GetRelatedProducts, GetAvailability, SearchByImage, GetAllTaxons, GetProducts, GetTree, GetImage, GetAllOptionTypes) | | |
| TASK-002 | Catalog Admin endpoint files: add `<summary>` + inline `// Map:` label (9 files: GetCatalogDashboard, CreateTaxonomy, UpdateTaxonomy, DeleteTaxonomy, RestoreTaxonomy, GetTaxonomiesPaged, GetTaxonomyById, UpdateTaxon, RepositionTaxon) | | |
| TASK-003 | Identity Storefront endpoint files: add `<summary>` + inline label by action type (`// Validate:` for auth, `// Send:` for email) (13 files: PasswordLogin, ExternalAuthenticate, ExternalProviders, EmailRegister, RefreshSession, GetSession, Logout, ChangePassword, RequestPasswordReset, ResetPassword, ConfirmEmail, ChangeEmail, ResendEmailVerification) | | |
| TASK-004 | Identity Admin endpoint files: add `<summary>` + inline label (7 files: CreateUser, UpdateUser, DeleteUser, ToggleUserStatus, GetUsersPagedOrAll, GetUserById, SyncUserRoles) | | |
| TASK-005 | Ordering Storefront endpoint files: add `<summary>` + inline label (18 files: CreateCart, GetCart, AddToCart, RemoveCartItem, UpdateCartItemQuantity, EmptyCart, DeleteCart, AssociateCartWithUser, CreateOrderFromCart, SelectShippingRate, UpdateCheckout, ValidateCheckout, GetCustomerOrder, ListCustomerOrders, CancelOrder) | | |
| TASK-006 | Ordering Admin endpoint files: add `<summary>` + inline label (7 files: GetOrderingDashboard, CreateOrder, ApproveOrder, CompleteOrder, CancelOrderAdmin, ResumeOrder, UpdateOrderLineItem) | | |
| TASK-007 | Payment Storefront endpoint files: add `<summary>` + inline label (5 files: CreatePaymentIntent, ConfirmPayment, CreateSetupIntent, ListPaymentMethods, StripeWebhook) | | |
| TASK-008 | Payment Admin endpoint files: add `<summary>` + inline label (12 files: CapturePayment, VoidPayment, RefundPayment, GetPagedPayments, GetPaymentById, CreatePaymentMethod, UpdatePaymentMethod, DeletePaymentMethod, ActivatePaymentMethod, DeactivatePaymentMethod, GetPagedPaymentMethods, GetPaymentMethodById) | | |
| TASK-009 | Inventory endpoint files (Storefront + Admin): add `<summary>` + inline label (14 files: GetStockAvailability, ReserveCartStock, GetCartReservations, GetInventoryDashboard, GetAllStockItems, GetStockItemById, UpdateStockItem, GetStockSummary, CreateStockLocation, UpdateStockLocation, GetPagedStockLocations, GetStockLocationById, SetDefaultStockLocation, CreateStockTransfer, TransferStockTransfer, ReceiveStockTransfer, CancelStockTransfer, GetStockTransferPagedOrAll, GetStockTransferById) | | |
| TASK-010 | Location endpoint files (Storefront + Admin): add `<summary>` + inline label (24 files: GetStorefrontCountryPagedOrAll, GetStorefrontCountryById, GetStorefrontCountryByIso, GetStorefrontStatePagedOrAll, GetStorefrontStateById, GetStorefrontStateByIso, CreateCountry, UpdateCountry, DeleteCountry, GetCountryPagedOrAll, GetCountryById, GetCountryByIso, CreateState, UpdateState, DeleteState, GetStatePagedOrAll, GetStateById, GetStateByIso) | | |
| TASK-011 | Shipping endpoint files: add `<summary>` + inline label (1 file: GetShippingMethods) | | |
| TASK-012 | Profile endpoint files: if any exist, same treatment | | |

### Implementation Phase 2 — C# Service Interfaces

- GOAL-002: Add XML doc-comments to all ~40 service interfaces across Shared and Module layers.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Shared Security service interfaces: add `<summary>` `<param>` `<returns>` on all method signatures (5 files: IPermissionService, IAccessTokenService, IRefreshTokenService, ITokenBlacklistService, IExternalProviderService) | | |
| TASK-014 | Shared Performance/Caching service interfaces: add XML doc-comments (1 file: ICachingService) | | |
| TASK-015 | Shared Operational service interfaces: add XML doc-comments (4 files: INotificationService, IStorageService, IEncryptionService, IDatabaseInitializerService) | | |
| TASK-016 | Inventory service interfaces: add XML doc-comments (5 files: IStockReservationService, IStockAvailabilityService, IStockRestockService, IStockSummaryService, ICartReservationService) | | |
| TASK-017 | Payment service interfaces: add XML doc-comments + CAT-10 `Boundary:` annotations at layer boundaries (5 files: IPaymentProcessingService, IGatewayRegistry, IPaymentGatewayActionProvider, IWebhookHandler, IStripeWebhookService) | | |
| TASK-018 | Catalog service interfaces: add XML doc-comments (3 files: ITaxonHierarchyService.*, IAutoClassificationService, ITaxonRuleEvaluator) | | |

### Implementation Phase 3 — C# Domain Entity Invariants & Boundaries

- GOAL-003: Add `Invariant:` and `Boundary:` CAT-10 annotations to domain entity and aggregate root classes.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | Catalog domain aggregate roots: add `// Invariant:` declarations for key invariants (Product, Variant, Taxonomy, Taxon entities) | | |
| TASK-020 | Ordering domain aggregate roots: add `// Invariant:` (Cart, Order entities) | | |
| TASK-021 | Identity domain entities: add `// Invariant:` (User, Role entities) | | |
| TASK-022 | Payment domain entities: add `// Invariant:` (Payment, PaymentMethod entities) + `// Boundary: Domain → Infrastructure` on repository interfaces | | |
| TASK-023 | Inventory domain entities: add `// Invariant:` (StockItem, StockLocation, StockTransfer entities) | | |
| TASK-024 | Shipping, Location, Profile domain entities: add `// Invariant:` on aggregate roots | | |

### Implementation Phase 4 — C# Handler Enhancement (Gap Fill)

- GOAL-004: Audit all existing handlers — add missing CAT-10 `Contract:`, CAT-1 `Validate:`, and inline labels to handlers currently marked "Partial" or "Minimal".

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | Catalog handlers marked Partial: add missing inline labels (RepositionTaxonUseCase, GetRelatedProducts) | | |
| TASK-026 | Identity handlers marked Partial: add missing labels (Logout, RequestPasswordReset, ConfirmEmail, ChangeEmail, CreateUser, UpdateUser, DeleteUser, SyncUserRoles, GetUsersPagedOrAll) | | |
| TASK-027 | Ordering handlers marked Partial: add missing labels (GetCart, RemoveCartItem, UpdateCartItemQuantity, EmptyCart, DeleteCart, SelectShippingRate, GetCustomerOrder, ListCustomerOrders, CreateOrder) | | |
| TASK-028 | Payment handlers marked Partial: add missing labels (CreatePaymentMethod, UpdatePaymentMethod, VoidOrderPayments) | | |
| TASK-029 | Inventory handlers marked Partial: add missing labels (CheckStockAvailability, GetCartReservations, GetAllStockItems, GetStockItemById, CreateStockLocation, UpdateStockLocation, GetPagedStockLocations, GetStockSummary, TransferStockTransfer, ReceiveStockTransfer, CancelStockTransfer, GetStockTransferPagedOrAll) | | |
| TASK-030 | Location handlers marked Partial: add missing labels (all Create/Update country and state handlers) | | |
| TASK-031 | Shipping handlers marked Partial: add missing labels (GetShippingMethods) | | |

### Implementation Phase 5 — TypeScript Storefront Stores & API Client

- GOAL-005: Add TSDoc comments to all Pinia stores and the API client in `app/Store/src/`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-032 | `app/Store/src/api.ts`: add TSDoc on `ApiError` class, `request()` function, and each method in the `api` object (`get`, `post`, `put`, `delete`). Add `// Contract: pre=..., post=..., throws=ApiError` on `request()`. | | |
| TASK-033 | `app/Store/src/stores/cart.ts`: add TSDoc on `CartItem` interface, `useCartStore`, and every exported function (`addItem`, `removeItem`, `updateQuantity`, `clearCart`, `toggleCart`). Add `// Contract:` on `addItem` and `updateQuantity`. | | |
| TASK-034 | `app/Store/src/stores/catalog.ts`: add TSDoc on the store, all state refs, computed properties, and actions. Add `// Cache:` TTL note on product data fetching. | | |

### Implementation Phase 6 — C# Loggers, Constants, Result Partial Files

- GOAL-006: Add XML doc-comments and inline labels to all `.Loggers.cs`, `.Constants.cs`, `.Result.cs` partial class files across Shared and Module layers.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-035 | Shared partial files: add XML doc-comments (TokenBlacklist.Service.Loggers, Permission.Service.Loggers, Permission.Service.Constant, Notification.Service.Loggers, Notification.Service.Mappings, Storage.Service.Loggers, Caching.Service.Loggers, DatabaseInitializerHostedService.Loggers, RefreshToken.Service.Loggers, PaymentProcessingService.Loggers) | | |
| TASK-036 | Module partial files: add XML doc-comments + inline labels (CartExpiryService.Loggers, ProcessStripeWebhookEventJob.Loggers, GatewayConstants, GatewayRegistry.Result, PaymentGatewayResponse, StripeGateway.Result, StripeOptions, StripeSettingValidation, BogusGateway, Gateway, GatewayOptions) | | |

### Implementation Phase 7 — TypeScript Admin SPA

- GOAL-007: Add TSDoc comments to `app/Admin/src/` files where they contain logic.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-037 | `app/Admin/src/main.ts`: add module-level comment explaining app entry point | | |
| TASK-038 | `app/Admin/src/stores/counter.ts`: add TSDoc on store, state, and increment action | | |

### Implementation Phase 8 — Verification & Quality Gate

- GOAL-008: Verify all changes comply with `CommentingRules.xml` and that `dotnet build` succeeds.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-039 | Run `dotnet build` — fix any warnings-as-errors introduced by comment changes | | |
| TASK-040 | Run `cd app/Store && pnpm run lint` — fix any lint issues | | |
| TASK-041 | Spot-check 20 random files across all phases against `CommentingRules.xml` decision tree and anti-pattern checklist (AP-1 to AP-8) | | |
| TASK-042 | Verify no stale comments or dead code were introduced (AP-5, AP-6 check) | | |
| TASK-043 | Run `dotnet test service/Api/tests/Module.UnitTests` and `dotnet test service/Api/tests/Shared.UnitTests` to verify no regressions | | |

## 3. Alternatives

- **ALT-001 — Big-bang rewrite of all comments**: Rejected. The codebase already follows a consistent style in well-commented areas. A full rewrite would risk AP-5 (stale comments) and AP-6 (dead code). Incremental targeted addition is safer.
- **ALT-002 — Automated tooling (docfx, typedoc)**: Rejected for inline labels. The v3.0 CAT-10 annotations (`Contract:`, `Invariant:`, `Boundary:`) require human judgment — automated tools cannot infer design-by-contract pre/post conditions. Docfx/Typedoc can generate docs from existing XML/TSDoc after this plan, but cannot originate the content.
- **ALT-003 — Skip TypeScript entirely**: Rejected. The stores and API client are the primary frontend-backend boundary and need `Boundary:` and `Contract:` annotations for AI agent editing.
- **ALT-004 — Comment only new code, skip existing gaps**: Rejected. The AGENTS.md (line 80-83) explicitly notes that AI agents read existing comments as operational context. Leaving gaps degrades agent performance per the ETH Zurich AGENTbench study (2026).

## 4. Dependencies

- **DEP-001**: `guide/code-commenting/CommentingRules.xml` v3.0 — authoritative source for all label formats, rules, and anti-patterns.
- **DEP-002**: Existing well-commented files serve as templates (e.g., `ListProducts.cs`, `StripeWebhook.cs`, `SearchByImage.cs` for C#; `inference_engine.py` for Python).
- **DEP-003**: `dotnet build` must pass after each phase — the entire project uses `TreatWarningsAsErrors=true` (per AGENTS.md).
- **DEP-004**: `app/Store` pnpm tooling for TypeScript lint checks.
- **DEP-005**: No external dependencies — all commenting is additive and language-native.

## 5. Files

- **FILE-001** to **FILE-250**: All `.Endpoint.cs` files under `service/Api/src/Module/{Catalog,Identity,Ordering,Payment,Inventory,Location,Shipping,Profile}/Features/{Admin,Storefront}/**/Endpoint.cs`
- **FILE-251** to **FILE-290**: All `I*Service.cs` interface files under `service/Api/src/Module/*/Services/Abstractions/` and `service/Api/src/Shared/*/Services/`
- **FILE-291** to **FILE-310**: Domain aggregate root entities under `service/Api/src/Module/*/Domain/`
- **FILE-311** to **FILE-330**: Handler files currently marked Partial/Minimal coverage
- **FILE-331** to **FILE-333**: `app/Store/src/api.ts`, `app/Store/src/stores/cart.ts`, `app/Store/src/stores/catalog.ts`
- **FILE-334** to **FILE-345**: `.Loggers.cs`, `.Constants.cs`, `.Result.cs` partial files

## 6. Testing

- **TEST-001**: `dotnet build` — must pass without warnings (warnings-as-errors)
- **TEST-002**: `cd app/Store && pnpm run lint` — must pass
- **TEST-003**: `dotnet test service/Api/tests/Module.UnitTests` — all existing tests green
- **TEST-004**: `dotnet test service/Api/tests/Shared.UnitTests` — all existing tests green
- **TEST-005**: Grep-based audit: `rg '// (Validate:|Check:|Guard:|Verify:|Assert:|Create:|Assign:|Update:|Add:|Remove:|Clone:|Merge:|Initialize:|Reset:|Compute:|Transform:|Parse:|Format:|Filter:|Generate:|Normalize:|Aggregate:|Sort:|Explain:|Enforce:|Raise:|Trigger:|Notify:|Handle:|Subscribe:|Policy:|Await:|Retry:|Skip:|Fallback:|Batch:|Throttle:|Defer:|Continue:|Break:|Circuit:|Acquire:|Release:|Lock:|Cache:|Purge:|Pool:|Dispose:|Catch:|Recover:|Compensate:|Degrade:|Escalate:|Suppress:|Call:|Send:|Receive:|Publish:|Map:|Serialize:|Deserialize:|Webhook:|Log:|Trace:|Monitor:|Audit:|Profile:|Debug:|Contract:|Invariant:|Assume:|AgentHint:|AgentSkip:|Boundary:|Context:)'` — ensure every module has at least 3 inline labels per feature file
- **TEST-006**: Anti-pattern spot-check: 20 random files reviewed against AP-1 to AP-8 from `CommentingRules.xml`

## 7. Risks & Assumptions

- **RISK-001 — Comment inconsistency**: Adding comments manually by different agents may introduce style drift (AP-4). Mitigation: Use the strict label format from `CommentingRules.xml` (F1-F10). The existing well-commented files serve as the canonical template.
- **RISK-002 — Stale comments during refactoring** (AP-5): This plan is purely additive. No existing functionality is modified, so the risk of stale comments is limited. Future refactoring should update comments using the same standard.
- **RISK-003 — Build failures from comment changes**: XML doc-comments and inline comments are not executable code. No build failures are expected. However, if any warnings-as-errors rules trigger on malformed XML doc tags, they will be caught in Phase 8.
- **ASSUMPTION-001**: The current well-commented files represent the intended v3.0 standard compliance target for all files. New comments should match their density and format.
- **ASSUMPTION-002**: All ~447 source files listed in the scan are valid targets for comment addition. Placeholder/scaffold files (e.g., `siglip.py`, `app/Admin/src/App.vue`) receive only the minimum viable annotation.
- **ASSUMPTION-003**: No files outside the scanned set (ApiTests, benchmarks, infra) need commenting — they are test/fixture/infrastructure code.

## 8. Related Specifications / Further Reading

- `guide/code-commenting/CommentingRules.xml` — authoritative source (XML, machine-readable)
- `guide/code-commenting/README.md` — human-readable overview
- `guide/code-commenting/SKILL.md` — agent workflow for applying comments
- `guide/code-commenting/references/label-quick-reference.md` — full label table
- `guide/code-commenting/references/anti-patterns.md` — anti-pattern checklist
- `AGENTS.md` — repository agent guide (non-negotiable rules, tech stack, verification commands)
