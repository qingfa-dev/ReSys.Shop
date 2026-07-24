== Requirements Traceability Matrix

This matrix establishes *bidirectional traceability* between requirements, design artifacts, implementation, and tests. Each requirement is mapped to:
- The thesis chapter where it is analyzed / designed
- The source file(s) implementing it
- The test file(s) verifying it
- The status (implemented / tested / pending)

*Standard*: IEEE 830-1998 (Software Requirements Specifications) recommends traceability matrices for demonstrating coverage. This matrix fulfills that recommendation.

=== Functional Requirements Traceability

==== Catalog Module

#figure(
  table(
    columns: 7,
    align: (left, left, left, left, left, left, left),
    table.header(
      [*Req ID*], [*Requirement*], [*Design (Chapter)*], [*Implementation*], [*Test Case ID*], [*Test*], [*Status*],
    ),
    [CAT-FR-01], [Create product with fashion metadata], [4 (Domain), 5 (DB), 6 (API), 7 (Seq)], [`CreateProduct.cs`], [TC-CAT-001], [`CreateProduct.Tests.cs`], [✅ Implemented & Tested],
    [CAT-FR-02], [Product variants (SKU, price, dims)], [4 (Domain), 5 (DB)], [`Variant.cs`], [TC-CAT-001], [`CreateProduct.Tests.cs` (indirect)], [✅ Implemented & Tested],
    [CAT-FR-03], [Taxonomy (hierarchical categories)], [4 (Domain), 5 (DB)], [`Taxonomy.cs`, `Taxon.cs`], [TC-CAT-002], [`ApiTests/Catalog/Admin/taxonomies.http`], [✅ Implemented],
    [CAT-FR-04], [Option types and values], [4 (Domain)], [`OptionType.cs`, `OptionValue.cs`], [TC-CAT-003], [`ApiTests/Catalog/Admin/option-types.http`], [✅ Implemented],
    [CAT-FR-05], [Variant image upload (Local/S3)], [3 (Arch), 6 (API), 8 (Security)], [`UploadVariantImage.cs`], [TC-CAT-004], [`ApiTests/Catalog/Admin/variant-images.http`], [✅ Implemented],
    [CAT-FR-06], [Pluggable ML sidecar generates embeddings for 4 models (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic)], [3 (Arch), 5 (DB), 7 (ML)], [`ImageEmbedding.Inference.cs`, `Vector.Configuration.cs`, `embedding_service.py`, `base.py`], [TC-CAT-005], [`test_embedding.py`, `test_model_registry.py`], [✅ Implemented],
    [CAT-FR-07], [Search by image (CBIR)], [3 (Arch), 5 (DB), 7 (Seq)], [`SearchByImage handler`], [TC-CAT-006], [`ApiTests/Catalog/Storefront/search-by-image.http`], [✅ Implemented],
    [CAT-FR-08], [Product status lifecycle (Draft→Active→Archived)], [4 (Domain), 4.4 (State)], [`Product.cs:20`, `ProductStatus` enum], [—], [`[TODO] Unit test for status transition`], [✅ Implemented, ⚠️ Test pending],
    [CAT-FR-09], [Slug uniqueness], [4 (Domain), 5 (DB)], [`CreateProduct.cs:41-43` (EF query + UK)], [TC-CAT-001], [`CreateProduct.Tests.cs`], [✅ Implemented & Tested],
    [CAT-FR-10], [Embedding model configurable per deployment via `EMBEDDING_MODEL` env var], [3 (Arch), 7 (ML)], [`settings.py:11`, `embedding_service.py:42`], [TC-CAT-007], [`test_model_registry.py`], [✅ Implemented],
  ),
  caption: [Catalog module requirements traceability],
)

==== Identity Module

#figure(
  table(
    columns: 7,
    align: (left, left, left, left, left, left, left),
    table.header(
      [*Req ID*], [*Requirement*], [*Design (Chapter)*], [*Implementation*], [*Test Case ID*], [*Test*], [*Status*],
    ),
    [ID-FR-01], [Register with email/password], [4 (Domain), 6 (API), 8 (Security)], [`Register.cs`], [TC-ID-001], [`ApiTests/Identity/Store/auth-register.http`], [✅ Implemented],
    [ID-FR-02], [Login (password + Google OAuth)], [3 (Arch), 6 (API), 8 (Security)], [`PasswordLogin.cs`, `ExternalLogin.Extensions.cs`], [TC-ID-002], [`ApiTests/Identity/Store/auth-login.http`], [✅ Implemented],
    [ID-FR-03], [JWT with refresh rotation + reuse detection], [8 (Security)], [`Tokens.Extensions.cs`, `RefreshTokenService.cs`], [TC-ID-003], [`ApiFactory.cs` (integration)], [✅ Implemented & Tested],
    [ID-FR-04], [Guest sessions via cookie], [8 (Security)], [`GuestSession middleware`, `AssociateCartWithUser.cs`], [TC-ID-004], [`ApiTests/Ordering/Cart.http`], [✅ Implemented],
    [ID-FR-05], [Role + permission authorization], [6 (API), 8 (Security)], [`PermissionContext.cs`, `HasPermission.Attribute`], [TC-ID-005], [`ApiTests/Identity/Admin/permissions.http`], [✅ Implemented],
    [ID-FR-06], [Password reset via email], [6 (API), 8 (Security)], [`PasswordReset.cs`], [TC-ID-006], [`ApiTests/Identity/Store/passwords.http`], [✅ Implemented],
    [ID-FR-07], [Admin user/role/permission management], [4 (Domain), 6 (API)], [`Users/`, `Roles/`, `Permissions/` features], [TC-ID-007], [`ApiTests/Identity/Admin/*.http`], [✅ Implemented],
  ),
  caption: [Identity module requirements traceability],
)

==== Inventory Module

#figure(
  table(
    columns: 7,
    align: (left, left, left, left, left, left, left),
    table.header(
      [*Req ID*], [*Requirement*], [*Design (Chapter)*], [*Implementation*], [*Test Case ID*], [*Test*], [*Status*],
    ),
    [INV-FR-01], [Stock locations], [4 (Domain), 5 (DB)], [`StockLocation.cs`], [TC-INV-001], [`[TODO]`], [✅ Implemented],
    [INV-FR-02], [Stock items (quantity per variant per location)], [4 (Domain), 5 (DB)], [`StockItem.cs`], [TC-INV-002], [`[TODO]`], [✅ Implemented],
    [INV-FR-03], [Stock reservations], [4 (Domain)], [`StockReservation.cs`], [TC-INV-003], [`[TODO]`], [✅ Implemented],
    [INV-FR-04], [Stock transfers], [4 (Domain)], [`StockTransfer.cs`], [TC-INV-004], [`[TODO]`], [✅ Implemented],
    [INV-FR-05], [Stock movements (adjustments)], [4 (Domain)], [`StockMovement.cs`], [TC-INV-005], [`[TODO]`], [✅ Implemented],
  ),
  caption: [Inventory module requirements traceability],
)

==== Ordering Module

#figure(
  table(
    columns: 7,
    align: (left, left, left, left, left, left, left),
    table.header(
      [*Req ID*], [*Requirement*], [*Design (Chapter)*], [*Implementation*], [*Test Case ID*], [*Test*], [*Status*],
    ),
    [ORD-FR-01], [Add items to cart], [4 (Domain), 6 (API)], [`AddCartItem.cs`], [TC-ORD-001], [`ApiTests/Ordering/Cart.http`], [✅ Implemented],
    [ORD-FR-02], [Cart auto-expiry (7 days)], [3 (Arch), 4 (Domain)], [`CartExpiryJob.cs`, `appsettings.json:181-183`], [—], [`[TODO]`], [✅ Implemented, ⚠️ Test pending],
    [ORD-FR-03], [Checkout state machine], [4.4 (State), 7 (Seq)], [`Order.cs:20`, `CheckoutState` enum], [TC-ORD-002], [`CreateOrderFromCart.Tests.cs`], [✅ Implemented & Tested],
    [ORD-FR-04], [Order total calculation], [4 (Domain)], [`Order.cs:22-25`, invariant comment], [TC-ORD-002], [`CreateOrderFromCart.Tests.cs`], [✅ Implemented & Tested],
    [ORD-FR-05], [Payment + shipment state tracking], [4 (Domain)], [`Order.cs:28-29`], [—], [`[TODO]`], [✅ Implemented],
    [ORD-FR-06], [Cancel order], [4 (Domain), 6 (API)], [`CancelOrder.cs`, `CancelOrderAdmin.cs`], [TC-ORD-003], [`CancelOrder.Tests.cs`], [✅ Implemented & Tested],
    [ORD-FR-07], [Associate guest cart with user], [6 (API)], [`AssociateCartWithUser.cs`], [TC-ORD-004], [`ApiTests/Ordering/Cart.http`], [✅ Implemented],
    [ORD-FR-08], [Order number generation (transaction + RepeatableRead)], [5 (DB), 7 (Seq)], [`CreateOrderFromCart.cs` (commit `887a77c7`)], [TC-ORD-002], [`CreateOrderFromCart.Tests.cs`], [✅ Implemented & Tested],
  ),
  caption: [Ordering module requirements traceability],
)

==== Payment Module

#figure(
  table(
    columns: 7,
    align: (left, left, left, left, left, left, left),
    table.header(
      [*Req ID*], [*Requirement*], [*Design (Chapter)*], [*Implementation*], [*Test Case ID*], [*Test*], [*Status*],
    ),
    [PAY-FR-01], [Create payment intent], [4 (Domain), 6 (API), 7 (Seq)], [`CreatePaymentIntent.cs`], [TC-PAY-001], [`CreatePaymentIntentTests.cs`], [✅ Implemented & Tested],
    [PAY-FR-02], [Confirm payment], [4 (Domain), 6 (API)], [`ConfirmPayment.cs`], [TC-PAY-002], [`ConfirmPaymentTests.cs`], [✅ Implemented & Tested],
    [PAY-FR-03], [Capture / void / refund (Admin)], [4 (Domain), 6 (API)], [`CapturePayment.cs`, `VoidPayment.cs`, `RefundPayment.cs`], [TC-PAY-003], [`CapturePaymentTests.cs`, `VoidPaymentTests.cs`, `RefundPaymentTests.cs`], [✅ Implemented & Tested],
    [PAY-FR-04], [Stripe webhook with signature validation], [3 (Arch), 8 (Security), 7 (Seq)], [`StripeWebhook.cs:32-36`], [TC-PAY-004], [`StripeWebhookTests.cs`], [✅ Implemented & Tested],
    [PAY-FR-05], [Bogus gateway for dev/test], [3 (Arch)], [`BogusGateway.cs`], [TC-PAY-005], [`BogusGatewayTests.cs`], [✅ Implemented & Tested],
  ),
  caption: [Payment module requirements traceability],
)

==== Shipping Module

#figure(
  table(
    columns: 7,
    align: (left, left, left, left, left, left, left),
    table.header(
      [*Req ID*], [*Requirement*], [*Design (Chapter)*], [*Implementation*], [*Test Case ID*], [*Test*], [*Status*],
    ),
    [SHIP-FR-01], [Shipping methods], [4 (Domain), 5 (DB)], [`ShippingMethod.cs`], [TC-SHIP-001], [`ApiTests/Shipping/Methods.http`], [✅ Implemented],
    [SHIP-FR-02], [Shipping rate calculation], [4 (Domain)], [`ShippingRate.cs`, `ShippingRateCalculator.cs`], [TC-SHIP-002], [`ApiTests/Shipping/Calculate.http`], [✅ Implemented],
  ),
  caption: [Shipping module requirements traceability],
)

==== Profile Module

#figure(
  table(
    columns: 7,
    align: (left, left, left, left, left, left, left),
    table.header(
      [*Req ID*], [*Requirement*], [*Design (Chapter)*], [*Implementation*], [*Test Case ID*], [*Test*], [*Status*],
    ),
    [PROF-FR-01], [Profile, addresses, wishlists], [4 (Domain), 5 (DB)], [`UserProfile.cs`, `Address.cs`, `Wishlist.cs`], [TC-PROF-001], [`ApiTests/Profile/Store/*.http`], [✅ Implemented],
    [PROF-FR-02], [Notification preferences], [4 (Domain)], [`NotificationPreferences.cs`], [TC-PROF-002], [`ApiTests/Profile/Store/notifications.http`], [✅ Implemented],
  ),
  caption: [Profile module requirements traceability],
)

==== Location Module

#figure(
  table(
    columns: 7,
    align: (left, left, left, left, left, left, left),
    table.header(
      [*Req ID*], [*Requirement*], [*Design (Chapter)*], [*Implementation*], [*Test Case ID*], [*Test*], [*Status*],
    ),
    [LOC-FR-01], [Countries and states (ISO codes)], [4 (Domain), 5 (DB)], [`Country.cs`, `State.cs`], [TC-LOC-001], [`ApiTests/Location/Store/*.http`], [✅ Implemented],
  ),
  caption: [Location module requirements traceability],
)

=== Non-Functional Requirements Traceability

#figure(
  table(
    columns: 7,
    align: (left, left, left, left, left, left, left),
    table.header(
      [*Req ID*], [*Requirement*], [*Design (Chapter)*], [*Implementation*], [*Test Case ID*], [*Test / Verification*], [*Status*],
    ),
    [NFR-01], [Module isolation (zero cross-references)], [3 (Arch), 4.1a (BC Map)], [`Directory.Build.targets:42-53` (intent), convention compliance], [TC-NFR-001], [Manual audit of `using` directives; `ValidateVerticalSliceIsolation` target], [✅ Compliant by convention, ⚠️ Target disabled],
    [NFR-02], [Explicit error handling (`Result<T>`)], [3 (Arch), 7 (Class)], [`Result.cs`, `Error.cs`, all handlers], [TC-NFR-002], [`grep` audit: zero `throw` in domain/handlers], [✅ Fully adopted],
    [NFR-03], [Warnings as errors], [3 (Arch)], [`Directory.Build.props:17`], [TC-NFR-003], [`dotnet build` passes], [✅ Enforced],
    [NFR-04], [Testability (no Docker for unit tests)], [10 (Testing)], [`Module.UnitTests` (InMemory EF), `Shared.UnitTests`], [TC-NFR-004], [`dotnet test service/Api/tests/Module.UnitTests`], [✅ Verified],
    [NFR-05], [Observability (OTel traces/metrics/logs)], [3 (Arch), 9 (Deploy)], [`Extensions.cs:58-103` (ServiceDefaults)], [TC-NFR-005], [Health checks pass at `/health`, `/alive`], [✅ Implemented],
    [NFR-06], [Rate limiting], [6 (API), 8 (Security)], [`RateLimit.Extensions.cs`, `appsettings.json:79-86`], [TC-NFR-006], [`ApiTests/` + integration tests for 429 responses], [✅ Implemented],
    [NFR-07], [Security headers], [8 (Security)], [`SecurityHeadersMiddleware.cs`], [TC-NFR-007], [Integration test: assert headers in response], [✅ Implemented, ⚠️ Test pending],
    [NFR-08], [File upload security], [8 (Security)], [`Storage.Extensions.cs:35-74`, `appsettings.json:129-155`], [TC-NFR-008], [Unit tests for `StorageSecurityEnforcer`], [✅ Implemented],
    [NFR-09], [Multi-tier caching], [3 (Arch), 5 (DB)], [`HybridCache`, `appsettings.json:104-122`], [TC-NFR-009], [`ApiFactory.cs` disables cache in tests; manual perf test], [✅ Implemented],
    [NFR-10], [Background job reliability], [3 (Arch), 9 (Deploy)], [`Hangfire`, `Background.Extension.cs:54-80`], [TC-NFR-010], [Integration test: enqueue + verify job execution], [✅ Implemented],
  ),
  caption: [Non-functional requirements traceability],
)

=== Research / Evaluation Requirements (Thesis Contribution)

These requirements are unique to the *dual-contribution* nature of the thesis (software architecture + ML model comparison). They trace the evaluation methodology in Chapter 11 to the implementation and planned experiments.

#figure(
  table(
    columns: 7,
    align: (left, left, left, left, left, left, left),
    table.header(
      [*Req ID*], [*Requirement*], [*Design (Chapter)*], [*Implementation*], [*Test Case ID*], [*Test*], [*Status*],
    ),
    [RES-FR-01], [Sidecar supports runtime model swap without code changes], [3 (Arch), 7 (ML)], [`embedding_service.py` registry + `BaseEmbeddingModel`], [TC-RES-001], [Unit test: swap `EMBEDDING_MODEL` env → different model class loaded], [✅ Implemented],
    [RES-FR-02], [Ground-truth dataset protocol: 100 images, 10 categories, human-labeled similarity], [11 (Eval)], [`11-evaluation.md:§11.5.2`], [TC-RES-002], [Manual dataset curation + inter-annotator agreement (κ≥0.75)], [⏳ Planned],
    [RES-FR-03], [Retrieval metrics computed per model: `Precision@K`, `Recall@K`, mAP], [11 (Eval)], [`11-evaluation.md:§11.5.3` + Python benchmark script], [TC-RES-003], [Automated benchmark: foreach model → compute metrics], [⏳ Planned],
    [RES-FR-04], [Operational metrics collected: embedding time, query latency, storage, RAM], [11 (Eval)], [`encode_image()` telemetry (`elapsed_ms`), `psutil` memory probe], [TC-RES-004], [Benchmark script records `time.perf_counter()` + `psutil`], [✅ Implemented (telemetry)],
    [RES-FR-05], [Statistical analysis: paired t-tests, Cohen's d, bootstrap 95% CI], [11 (Eval)], [`11-evaluation.md:§11.5.7` + Python `scipy.stats` script], [TC-RES-005], [Verify significance of Fashion-CLIP vs. each competitor], [⏳ Planned],
    [RES-NFR-01], [Reproducibility: pinned package versions, fixed random seed, documented hardware], [11 (Eval)], [`pyproject.toml` exact versions, benchmark protocol in thesis], [TC-RES-006], [Re-run benchmark on identical hardware → identical results], [✅ Implemented (version pins)],
  ),
  caption: [Research/evaluation requirements traceability],
)

=== Coverage Summary

#figure(
  table(
    columns: 7,
    align: (left, center, center, center, center, center, center),
    table.header(
      [*Module*], [*FRs*], [*NFRs*], [*Unit Tests*], [*Integration Tests*], [*HTTP Tests*], [*Coverage*],
    ),
    [Catalog], [9], [—], [✅], [⚠️ Partial], [✅ 10], [~70%],
    [Identity], [7], [NFR-05/06/07], [✅], [✅], [✅ 11], [~75%],
    [Inventory], [5], [—], [⚠️ Minimal], [⚠️ None], [⚠️ None], [~20%],
    [Ordering], [8], [—], [✅], [✅], [✅ 2], [~80%],
    [Payment], [5], [—], [✅], [✅], [✅ 2], [~85%],
    [Shipping], [2], [—], [⚠️ Minimal], [⚠️ None], [✅ 2], [~30%],
    [Profile], [2], [—], [⚠️ Minimal], [⚠️ None], [✅ 5], [~25%],
    [Location], [1], [—], [✅], [✅], [✅ 4], [~60%],
    [*Cross-cutting*], [—], [10], [✅ (Shared)], [✅ (Host/AntiForgery)], [✅ 8], [~70%],
    [*Research / ML Evaluation*], [6 (RES-FR)], [1 (RES-NFR)], [✅ (model registry)], [⏳ (benchmark script)], [—], [N/A],
  ),
  caption: [Test coverage summary by module],
)

*Legend*: ✅ = comprehensive / well-tested | ⚠️ = partial / gaps exist | ⏳ = planned for final submission | ~% = estimated coverage (opt-in via `/p:CollectCoverage=true`)

=== Gaps and Action Items

#figure(
  table(
    columns: 4,
    align: (left, center, left, left),
    table.header(
      [*Gap*], [*Priority*], [*Action*], [*Owner*],
    ),
    [Inventory module has minimal unit tests], [High], [Add `StockLocation`, `StockItem`, `StockReservation` unit tests], [[TODO]],
    [Shipping module has minimal unit tests], [Medium], [Add `ShippingMethod`, `ShippingRate` unit tests], [[TODO]],
    [Profile module has minimal unit tests], [Medium], [Add `UserProfile`, `Address`, `Wishlist` unit tests], [[TODO]],
    [`CAT-FR-08` status transition tests], [Low], [Add test for `ProductStatus` state machine], [[TODO]],
    [`ORD-FR-02` cart expiry job test], [Low], [Add integration test for `CartExpiryJob` Hangfire execution], [[TODO]],
    [NFR-07 security headers integration test], [Low], [Assert `SecurityHeadersMiddleware` output in `Api.Tests`], [[TODO]],
    [`ValidateVerticalSliceIsolation` enabled], [High], [Enable target and fix any offenders], [[TODO]],
  ),
  caption: [Traceability gaps and action items],
)

=== Evidence

- `service/Api/tests/Module.UnitTests/` — unit test directory structure mirroring source
- `service/Api/tests/Api.Tests/` — integration test scenarios
- `ApiTests/` — 49 manual HTTP test files
- `Directory.Packages.props:102-112` — test dependency versions
- `Directory.Build.props:95-98` — coverage configuration
- `CONCERNS.md` — known gaps and risks
- All feature handler files referenced in the Implementation column above