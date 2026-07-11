---
goal: Create README.yaml Documentation for All Module and Shared Layer Components
version: 1.0
date_created: 2026-07-11
status: 'Planned'
tags: feature, documentation, yaml-docs, modules, shared, infrastructure
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Create `README.yaml` files for all 9 business modules under `service/Api/src/Module/` and all 6 Shared pillars under `service/Api/src/Shared/`, following the Shared YAML Documentation Standard v3.0 defined in `guide/yaml-docs/SKILL.md`. Each document uses `kind: BuildingBlockModule` with sections dictated by the section inclusion rules. Existing `README.md` / `README.xml` files provide partial source material; `README.yaml` are created alongside them.

## 1. Requirements & Constraints

- **REQ-001**: All 15 README.yaml files MUST pass all 20 HARD RULES from `guide/yaml-docs/SKILL.md`
- **REQ-002**: Document kind MUST be `BuildingBlockModule` with `schema_version: "3.0"`
- **REQ-003**: Root keys (kind, id, name, version, status, schema_version) MUST be present in every file
- **REQ-004**: `meta` MUST be the first named section after root keys
- **REQ-005**: `file_structure` MUST be the last section in every file
- **REQ-006**: All abstraction items MUST have id, name, type, path, description, purpose
- **REQ-007**: All path values MUST be relative from the module/Shared root (start with `./`)
- **REQ-008**: version strings MUST be quoted semver ("1.0.0")
- **REQ-009**: Code blocks MUST use `|` (literal block scalar)
- **REQ-010**: severity MUST be one of: critical | high | medium | low
- **REQ-011**: feature type MUST be one of: core | extension | integration | utility
- **REQ-012**: abstraction type MUST be one of the 10 allowed values
- **REQ-013**: category order values MUST be sequential integers starting at 1
- **REQ-014**: ID prefixes follow tier: A=cat-1, B=cat-2, C=cat-3, D=cat-4; P=principles; F=features; AP=anti_patterns; S=scenarios; TS=testing; M=mechanisms; E=explanations; G=guides
- **CON-001**: Files MUST be placed at the root of each module/Shared directory (e.g., `Module/Catalog/README.yaml`, `Shared/Application/README.yaml`)
- **CON-002**: All domain types and feature structures must be documented from actual code inspection, not guessed
- **CON-003**: Existing `README.md` and `README.xml` files provide source material but must not be deleted
- **GUD-001**: Follow naming conventions from `guide/yaml-docs/referneces/naming-conventions.md`
- **GUD-002**: purpose fields must start with a verb (Enables, Provides, Prevents, Centralizes)
- **GUD-003**: Descriptions in present tense, active voice, 1–2 sentences
- **GUD-004**: The `principles` section is recommended for all `BuildingBlockModule` documents

## 2. Implementation Steps

### Implementation Phase 1: Module Layer — Catalog, Location, Shipping, Webhooks

- GOAL-001: Create README.yaml for Catalog, Location, Shipping (have existing domain README.md source material) and Webhooks (small module, quick win).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `Module/Catalog/README.yaml`: meta (Catalog Module, 19.7K LOC), principles, features (product CRUD, taxonomy, option types, images), abstractions (Domain aggregates: Products, Taxonomies, OptionTypes; Features: Admin/Storefront; Persistence), anti_patterns, usage, file_structure | | |
| TASK-002 | Create `Module/Location/README.yaml`: meta (Location Module, 3K LOC), principles, features (country/state lookup, SEO slugs), abstractions (Domain aggregates: Countries, States; Features: Admin/Store; Persistence), anti_patterns, usage, file_structure | | |
| TASK-003 | Create `Module/Shipping/README.yaml`: meta (Shipping Module, 2.7K LOC), principles, features (shipping methods CRUD, rate calculators), abstractions (Domain aggregates: ShippingMethods, ShippingRates, Calculators; Features: Admin/Storefront; Persistence), anti_patterns, usage, file_structure | | |
| TASK-004 | Create `Module/Webhooks/README.yaml`: meta (Webhooks Module, 643 LOC), principles, features (subscription management, delivery), abstractions (Domain: WebhookSubscription; Features: Admin/Subscriptions; Persistence), anti_patterns, usage, file_structure | | |
| TASK-005 | Validate 4 README.yaml files against HARD RULES and validation checklist | | |

### Implementation Phase 2: Module Layer — Inventory, Ordering, Payment

- GOAL-002: Create README.yaml for Inventory, Ordering (has module-level README.md), and Payment (has domain/feature README.md source material).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Create `Module/Inventory/README.yaml`: meta (Inventory Module, 7K LOC), principles, features (stock management, reservations, transfers, availability), abstractions (Domain aggregates: Stock, StockLocations, StockReservations, StockTransfers; Services: StockAvailability, StockReservation, CartReservation; Features: Admin/Storefront/Shared; Persistence), anti_patterns, usage, file_structure | | |
| TASK-007 | Create `Module/Ordering/README.yaml`: meta (Ordering Module, 6.4K LOC), principles, features (cart, checkout, orders, line items, adjustments, order events, cart expiry), abstractions (Domain aggregates: Orders, LineItems, Adjustments; Backgrounds: CartExpiryJob; Services: CartExpiryService; Infrastructure: Events; Features: Admin/Storefront/Shared; Persistence), anti_patterns, usage, file_structure | | |
| TASK-008 | Create `Module/Payment/README.yaml`: meta (Payment Module, 4.4K LOC), principles, features (payment intents, methods, refunds, gateway integration, webhooks), abstractions (Domain aggregates: Payments, PaymentMethods, Gateways; Infrastructure: Gateways; Features: Admin/Storefront/Shared; Persistence), anti_patterns, usage, file_structure | | |
| TASK-009 | Validate 3 README.yaml files against HARD RULES and validation checklist | | |

### Implementation Phase 3: Module Layer — Identity, Profile

- GOAL-003: Create README.yaml for Identity (no Domain/, relies on Shared.Security) and Profile (UserProfile with static partial class split).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Create `Module/Identity/README.yaml`: meta (Identity Module, 6.4K LOC), principles, features (user auth, roles, permissions, sessions, admin management), abstractions (Features: Admin permissions/roles/users management, Store/ frontend auth; depends on Shared.Security), anti_patterns, usage, file_structure | | |
| TASK-011 | Create `Module/Profile/README.yaml`: meta (Profile Module, 5.8K LOC), principles, features (addresses, wishlists, notifications, preferences), abstractions (Domain: UserProfile with static partial class split across Constant/Method/Result/Validation/Loggers; Domain aggregates: Addresses, Wishlists, Notifications, Preferences; Features: Shared/Store; Persistence), anti_patterns, usage, file_structure | | |
| TASK-012 | Validate 2 README.yaml files against HARD RULES and validation checklist | | |

### Implementation Phase 4: Shared Layer — Application, Governance

- GOAL-004: Create README.yaml for Application (foundation types, CQRS, Result/Error models, concerns) and Governance (OpenAPI, JSON conventions, FluentValidation).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Create `Shared/Application/README.yaml`: meta, principles, features (Result/Error model, CQRS mediator abstractions, domain entities/aggregates, concerns, endpoint conventions, descriptors), abstractions (Domain/Models: Entity, ValueObject, AggregateRoot; Domain/Concerns: Auditable, SoftDeletable, Publishable, Sluggable, Parameterizable, Versionable, DisplayMoney; Mediators: Commands, Queries, Pipeline Behaviours; Models/Results; Models/Errors; Models/Descriptors; Endpoints; Extensions; Systems), anti_patterns, usage, file_structure | | |
| TASK-014 | Create `Shared/Governance/README.yaml`: meta, principles, features (OpenAPI/Scalar docs, JSON serialization conventions, FluentValidation wiring), abstractions (OpenApi: extension, options, schema naming; Conventions: Base64, Case, Dictionary, Enum converters; Validation: extension), anti_patterns, usage, file_structure | | |
| TASK-015 | Validate 2 README.yaml files against HARD RULES and validation checklist | | |

### Implementation Phase 5: Shared Layer — Observability, Performance

- GOAL-005: Create README.yaml for Observability (OpenTelemetry, correlation, health checks, logging) and Performance (HybridCache + Redis).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Create `Shared/Observability/README.yaml`: meta, principles, features (correlation IDs, health checks, structured logging, OpenTelemetry), abstractions (Correlation: middleware, context, extension; HealthChecks: extension; Logging: extension; ObservabilitySetting: POCO, validator, result), anti_patterns, usage, file_structure | | |
| TASK-017 | Create `Shared/Performance/README.yaml`: meta, principles, features (HybridCache, Redis distributed cache, in-memory cache, cache wrappers), abstractions (Caching: extension, Options (CachingSetting, Distributed, Hybrid, InMemory), Wrappers (service, entry options, converter)), anti_patterns, usage, file_structure | | |
| TASK-018 | Validate 2 README.yaml files against HARD RULES and validation checklist | | |

### Implementation Phase 6: Shared Layer — Operational, Security

- GOAL-006: Create README.yaml for Operational (largest pillar — 7 sub-pillars: persistence, storage, notifications, backgrounds, HTTP, webhooks, specifications) and Security (full auth stack).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | Create `Shared/Operational/README.yaml`: meta, principles, features (EF Core persistence, file storage, notifications, background jobs, HTTP resilience, webhooks, query specifications), abstractions organized by sub-pillar categories: Persistence (AppDbContext, interceptors, seeders, configurations, initializers, specifications framework), Storages (providers, security, services, processing), Notifications (channels, providers, services, templates, hubs, store), Backgrounds (Hangfire extension, options), Http (resilience, correlation propagation, options), Webhooks (domain, services, persistence, backgrounds), anti_patterns, usage, testing for specifications, file_structure | | |
| TASK-020 | Create `Shared/Security/README.yaml`: meta, principles, features (JWT auth, ASP.NET Identity, permission-based authorization, rate limiting, CORS, anti-forgery, security headers, external login, guest sessions), abstractions organized by sub-pillar categories: Authentication (tokens, external providers, guest sessions, contexts), Authorization (permissions, policies, requirements, features, registry), Identity (Domain: users, roles, permissions, tokens; Seeders; Options), AntiForgery (endpoints, options), Cors (extension, options), Headers (middleware, options), RateLimiting (extension, options), anti_patterns, usage, file_structure | | |
| TASK-021 | Validate 2 README.yaml files against HARD RULES and validation checklist | | |

### Implementation Phase 7: Final Validation

- GOAL-007: Run cross-cutting validation across all 15 files.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Run `python -c "import yaml; yaml.safe_load(open(f))"` on all 15 README.yaml files to verify YAML well-formedness | | |
| TASK-023 | Verify all file_structure trees match actual directory layouts | | |
| TASK-024 | Verify all abstraction paths reference real files on disk | | |
| TASK-025 | Verify no duplicate IDs within any document | | |
| TASK-026 | Run cross-document check: every `related` cross-reference ID exists in the target document | | |
| TASK-027 | Run `dotnet build service/Api/src/Api/Api.csproj` to confirm no build warnings (YAML files are embedded resources or content — confirm no MSBuild warnings) | | |

## 3. Alternatives

- **ALT-001**: Use `DomainModule` kind for `Module/*/Domain/` subdirectories — rejected because user requested one doc per module at module root, not per subdirectory; a single `BuildingBlockModule` doc covers the entire module including Domain, Features, and Persistence.
- **ALT-002**: Use `ServiceModule` kind for each business module — rejected because all 9 modules live within a single monolith (Api project), not as independent microservices. `BuildingBlockModule` is the correct kind for monolithic modules.
- **ALT-003**: Write separate README.yaml for each Shared sub-pillar (e.g., one for Persistence, one for Storages, one for Notifications) — rejected because the user explicitly said "for each pillar for each the Shared", meaning one document per top-level pillar directory. The Operational pillar's 7 sub-pillars are documented as separate `abstractions` categories within one file.
- **ALT-004**: Generate docs purely from code analysis without inspecting existing README.md files — rejected because several modules already have domain-level README.md and README.xml files that capture important architectural context not visible in code alone.

## 4. Dependencies

- **DEP-001**: `guide/yaml-docs/SKILL.md` — master specification with 20 HARD RULES
- **DEP-002**: `guide/yaml-docs/referneces/element-reference.md` — authoritative key reference
- **DEP-003**: `guide/yaml-docs/referneces/naming-conventions.md` — ID prefixes, enum values, text conventions
- **DEP-004**: `guide/yaml-docs/referneces/validation-checklist.md` — pre-output self-check
- **DEP-005**: `guide/yaml-docs/templates/module.yaml` — template for BuildingBlockModule kind
- **DEP-006**: `.harness/domains.yml` — domain boundary definitions and descriptions
- **DEP-007**: `.harness/principles.yml` — golden principles for cross-referencing in principles sections
- **DEP-008**: `docs/codebase/ARCHITECTURE.md` — architecture overview for context
- **DEP-009**: Existing `README.md` files in module Domain/ directories (Catalog, Location, Ordering, Payment, Shipping)

## 5. Files

- **FILE-001**: `service/Api/src/Module/Catalog/README.yaml` — Catalog module documentation (new)
- **FILE-002**: `service/Api/src/Module/Identity/README.yaml` — Identity module documentation (new)
- **FILE-003**: `service/Api/src/Module/Inventory/README.yaml` — Inventory module documentation (new)
- **FILE-004**: `service/Api/src/Module/Location/README.yaml` — Location module documentation (new)
- **FILE-005**: `service/Api/src/Module/Ordering/README.yaml` — Ordering module documentation (new)
- **FILE-006**: `service/Api/src/Module/Payment/README.yaml` — Payment module documentation (new)
- **FILE-007**: `service/Api/src/Module/Profile/README.yaml` — Profile module documentation (new)
- **FILE-008**: `service/Api/src/Module/Shipping/README.yaml` — Shipping module documentation (new)
- **FILE-009**: `service/Api/src/Module/Webhooks/README.yaml` — Webhooks module documentation (new)
- **FILE-010**: `service/Api/src/Shared/Application/README.yaml` — Application pillar documentation (new)
- **FILE-011**: `service/Api/src/Shared/Governance/README.yaml` — Governance pillar documentation (new)
- **FILE-012**: `service/Api/src/Shared/Observability/README.yaml` — Observability pillar documentation (new)
- **FILE-013**: `service/Api/src/Shared/Operational/README.yaml` — Operational pillar documentation (new)
- **FILE-014**: `service/Api/src/Shared/Performance/README.yaml` — Performance pillar documentation (new)
- **FILE-015**: `service/Api/src/Shared/Security/README.yaml` — Security pillar documentation (new)

## 6. Testing

- **TEST-001**: Run validation checklist on each file — all 20 HARD RULES must pass
- **TEST-002**: YAML well-formedness — `python -c "import yaml; yaml.safe_load(open('FILE'))"` on all 15 files
- **TEST-003**: Build verification — `dotnet build service/Api/src/Api/Api.csproj` with no warnings
- **TEST-004**: Path audit — verify every `path` value in `abstractions` resolves to an existing file
- **TEST-005**: ID uniqueness — scan each document for duplicate IDs
- **TEST-006**: Section ordering — verify `meta` is first named section and `file_structure` is last in every file

## 7. Risks & Assumptions

- **RISK-001**: The Operational pillar is large (~30K LOC across 7 sub-pillars) — the single README.yaml may become very large. Mitigation: organize abstractions by sub-pillar categories, keep descriptions concise.
- **RISK-002**: Some modules (Identity) lack Domain/ and Persistence/ folders — the document structure must accurately reflect their atypical architecture.
- **RISK-003**: Existing README.md files may contain outdated information — verify against actual code before referencing.
- **ASSUMPTION-001**: `PyYAML` >= 6.x is available for validation (`pip install pyyaml` or `uv pip install pyyaml`).
- **ASSUMPTION-002**: All module `*.Extension.cs` files contain the complete DI registration surface — used to determine `features` and `dependencies`.
- **ASSUMPTION-003**: The `file_structure` ASCII trees do not need to include test projects or build artifacts (`bin/`, `obj/`).

## 8. Related Specifications / Further Reading

- `guide/yaml-docs/SKILL.md` — Shared YAML Documentation Standard v3.0
- `guide/yaml-docs/referneces/element-reference.md` — Element key specification
- `guide/yaml-docs/referneces/naming-conventions.md` — Naming conventions
- `guide/yaml-docs/referneces/validation-checklist.md` — Validation checklist
- `guide/yaml-docs/templates/module.yaml` — BuildingBlockModule template
- `.harness/domains.yml` — Domain boundaries and layer maps
- `docs/codebase/ARCHITECTURE.md` — Architecture overview
