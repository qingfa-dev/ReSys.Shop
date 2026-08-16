---
goal: Consolidate all per-feature Shared folders under Features/Admin and Features/Storefront into one Shared folder per module+area with Mappings/Models/Validators grouping and {Entity}.{Kind}.cs / Storefront.{Entity}.{Kind}.cs naming
version: 1.0
date_created: 2026-08-16
last_updated: 2026-08-16
owner: Architecture Team
status: 'Completed'
tags: refactor, architecture, module, convention, namespaces
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Today every feature area under `Features/{Admin|Storefront}/` owns its own `Shared/` folder
(`Features/Admin/OptionTypes/Shared/`, `Features/Admin/OptionTypes/Values/Shared/`,
`Features/Storefront/Products/Shared/`, `Features/Storefront/Orders/Shared/`). These folders each contain `Mappings/`, `Models/`,
`Validators/` (or `Validation`/`Validations`) subfolders and use inconsistent file names
(`OptionType.Model.Request.cs`, `Store.OptionType.Model.cs`, `PaymentMethodStore.Mapping.cs`,
`Inventory.Storefront.Model.cs`). Namespaces drift from folder layout
(e.g. `Module.Identity.Features.Shared.Admin.Permissions.Shared.Models`).

This plan consolidates, per module, all such `Shared` folders under `Admin` into exactly one
`{Module}/Features/Admin/Shared/` and all under `Storefront` into exactly one
`{Module}/Features/Storefront/Shared/`, groups files by kind (`Mappings/`, `Models/`, `Validators/`),
merges files that share the same (Entity, Kind) into a single `{Entity}.{Kind}.cs`, renames
storefront files to `Storefront.{Entity}.{Kind}.cs`, and rewrites every referencing namespace to
`Module.{Module}.Features.{Area}.Shared.{KindDir}`. Modules are migrated one at a time and each phase
is verified by a warnings-as-errors build before the next phase begins.

Scope: 244 source files (176 Admin + 68 Storefront) across 8 modules reduce to 134 target files.
Non-consolidated content (`Services/`, `Clients/`, `Docs/`, root-level `Shared/*.cs` such as
`StoreProductConstant.cs`) stays in place. Module isolation (modules must not reference each other)
is preserved: consolidation is strictly per-module.

## 1. Requirements & Constraints

- **REQ-001**: Consolidate every `Shared` folder nested under `Features/Admin/` into exactly one `{Module}/Features/Admin/Shared/` and every `Shared` folder nested under `Features/Storefront/` into exactly one `{Module}/Features/Storefront/Shared/`, per module. Modules: Billing, Catalog, Customer, Dashboard, Identity, Inventory, Ordering, Shipping. (Location module has no Admin/Storefront Shared folders and is out of scope.)
- **REQ-002**: Within each consolidated `Shared` folder, group files into `Mappings/`, `Models/`, `Validators/`. Normalize legacy kind folders `Validation` and `Validations` to `Validators`.
- **REQ-003**: Name every consolidated file `{Entity}.{Kind}.cs` for Admin and `Storefront.{Entity}.{Kind}.cs` for Storefront, where `{Kind}` is the singular kind (`Model`, `Mapping`, `Validator`). The current sub-suffix (`.Request`, `.Response`, `.Parameters`, `.Domain`, `.Collection`, `.Action`, `.Address`, `.Quantity`, etc.) is dropped from the filename.
- **REQ-004**: Merge all source files that resolve to the same (Entity, Kind) into ONE target file whose content is the union of the sources (see PAT-001).
- **REQ-005**: Set the namespace of every consolidated file to `Module.{Module}.Features.{Area}.Shared.{KindDir}` (`{KindDir}` plural: `Mappings`/`Models`/`Validators`). Rewrite every reference across `service/Api/src` (Module, Api, Migrations) and `service/Api/tests` per the Appendix A map.
- **REQ-006**: Normalize storefront entity names: strip leading `Store.`/`Storefront.` prefixes and trailing `.Store`/`.Storefront` suffixes before applying `Storefront.` prefix (e.g. `Store.OptionType.Model.cs` -> `Storefront.OptionType.Model.cs`, `PaymentMethodStore.Mapping.cs` -> `Storefront.PaymentMethod.Mapping.cs`).
- **REQ-007**: Do NOT move `Services/`, `Clients/`, `Docs/` (including `.gitkeep`) or root-level files directly under a `Shared/` folder (e.g. `StoreProductConstant.cs`). They remain in their current feature location with their current names and namespaces.
- **SEC-001**: No runtime behavior change. Merging is layout-only (folders, filenames, namespaces, usings). No domain logic, validation rule, or mapping expression is altered.
- **SEC-002**: No cross-module references are introduced. Module isolation (AGENTS.md rule 2) holds at every phase.
- **CON-001**: `TreatWarningsAsErrors=true` globally. Every phase must leave the solution build clean (`dotnet build`).
- **CON-002**: `bash scripts/check-feature-conventions.sh` (AC-001/002/003/005) and `bash scripts/check-cross-module-refs.sh` must stay green after every phase.
- **CON-003**: Each module phase is self-contained and independently verifiable; no phase may depend on a later phase.
- **CON-004**: The migration must be performed by a deterministic, idempotent script (TASK-003); hand-editing is limited to the explicitly listed manual adjustments in each phase.
- **GUD-001**: Keep the target folder flat under `Shared/` with only the three kind dirs `Mappings/`, `Models/`, `Validators/`. Do not reintroduce per-feature `Shared` nesting.
- **GUD-002**: Preserve existing `{Entity}.{Kind}.cs` names already conforming to the target convention (e.g. Shipping `Shipment.Model.cs`, `ShippingMethod.Validator.cs`) exactly as-is.
- **PAT-001**: Merged target file layout: deduped+alphabetized `using` directives, one blank line, one file-scoped `namespace <N_new>;`, one blank line, then the concatenated type-declaration bodies of every source sorted by source relative path.
- **PAT-002**: Migration driven by `scripts/consolidate-shared.py` with `--dry-run` (prints planned operations, writes nothing) and `--apply` (executes them).

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Establish a green baseline, codify the target convention, and deliver the deterministic migration script.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Record the baseline: run `dotnet build`, `dotnet test service/Api/tests/Module.UnitTests`, `bash scripts/check-feature-conventions.sh`, `bash scripts/check-cross-module-refs.sh` from the repo root. Save the outputs to `docs/failures/2026-08-16-shared-consolidation-baseline.md`. Any failure must be fixed before proceeding. | | |
| TASK-002 | Add a `Shared` layout convention section to `docs/codebase/CONVENTIONS.md`: one `Shared` folder per module+area at `{Module}/Features/{Admin\|Storefront}/Shared/`; kind dirs only `Mappings/`, `Models/`, `Validators/`; file names `{Entity}.{Kind}.cs` (Admin) and `Storefront.{Entity}.{Kind}.cs` (Storefront); namespace `Module.{Module}.Features.{Area}.Shared.{KindDir}`; `Services/`/`Clients/`/`Docs/` stay with their consuming feature. | | |
| TASK-003 | Write `scripts/consolidate-shared.py` (Python 3.12). CLI: `--module <name> [--module <name>...]`, `--area {admin,storefront}`, `--dry-run`, `--apply`. Embed the 134-row source->target map (Appendix B) and the 110-entry old->new namespace map (Appendix A) as module-level data. Behavior per module+area: (1) for each target, read every listed source; for a source that is 0 bytes, delete it and skip; split each non-empty source into `using` lines, the single file-scoped `namespace X;` line, and body lines; rewrite each `using`/fully-qualified reference through the Appendix A map; compose the target per PAT-001; write the target and delete its sources (never delete when source == target). (2) Rewrite every `.cs` file under `service/Api/src/Module`, `service/Api/src/Api`, `service/Api/src/Migrations`, and `service/Api/tests` by applying the Appendix A map to `using <old>;` and to `<old>.` occurrences, then remove duplicate consecutive `using` lines (CS0105 under warnings-as-errors). (3) Delete now-empty kind directories and orphaned `Docs/.gitkeep` under the old `Shared` folders, but never delete a directory that still contains `Services/`, `Clients/`, or a root-level `.cs`. (4) Fail loudly (exit non-zero) if two sources for one target declare the same type name, or if a source's declared namespace has no entry in the Appendix A map. (5) Re-running with `--apply` after a successful run must produce no diff. | | |
| TASK-004 | Extend `scripts/check-feature-conventions.sh` with AC-006: assert no `Shared/{Mappings,Models,Validators,Validation,Validations}` directory exists deeper than `Features/{Admin\|Storefront}/Shared/` under `service/Api/src/Module`; allow `Services/`, `Clients/`, `Docs/`, and root-level `Shared/*.cs`. Wire AC-006 into the script's existing pass/fail output so a violation fails the script. | | |

### Implementation Phase 2

- GOAL-002: Migrate the Billing module (14 Admin + 7 Storefront source files -> 13 target files).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Run `python3 scripts/consolidate-shared.py --module Billing --area admin --dry-run` and confirm every operation matches the Billing table below (Appendix B, section "Billing"). Then run the same with `--area storefront --dry-run` and confirm. | | |
| TASK-006 | Apply the Billing migration: `python3 scripts/consolidate-shared.py --module Billing --area admin,storefront --apply`. No manual adjustments are expected for Billing. | | |
| TASK-007 | Verify Billing: `dotnet build` (clean), `dotnet test service/Api/tests/Module.UnitTests`, `bash scripts/check-feature-conventions.sh`, `bash scripts/check-cross-module-refs.sh`. | | |

### Implementation Phase 3

- GOAL-003: Migrate the Catalog module (78 Admin + 23 Storefront source files -> 55 target files), resolving the one real type-name collision (`ParametersValidator`).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Rename the colliding validator types so the merged target namespace `Module.Catalog.Features.Admin.Shared.Validators` stays collision-free. In `Catalog/Features/Admin/Taxonomies/Shared/Validators/Taxonomy.Validator.Request.cs` rename `ParametersValidator` (declaration and its two `new ParametersValidator()` uses) to `TaxonomyParametersValidator`. In `Catalog/Features/Admin/Taxonomies/Taxons/Shared/Validators/Taxon.Validator.Request.cs` rename `ParametersValidator` (declaration and its use) to `TaxonParametersValidator`. No other file references either type (verified). | | |
| TASK-009 | Run `python3 scripts/consolidate-shared.py --module Catalog --area admin --dry-run` and `--area storefront --dry-run`; confirm every operation matches the Catalog table below (Appendix B, section "Catalog"). Then `--module Catalog --area admin,storefront --apply`. | | |
| TASK-010 | Post-apply audit: `grep -rn "ParametersValidator\b" service/Api/src/Module/Catalog` must return no matches; `grep -rlE "Features.Admin.(Optiontypes|Taxons|Products.ProductClassifications|Products.Options)\." service/Api/src/Module/Catalog` must return no matches (legacy namespace spellings gone). | | |
| TASK-011 | Verify Catalog: `dotnet build` (clean), `dotnet test service/Api/tests/Module.UnitTests`, `bash scripts/check-feature-conventions.sh`, `bash scripts/check-cross-module-refs.sh`. | | |

### Implementation Phase 4

- GOAL-004: Migrate the Identity module (21 Admin + 12 Storefront source files -> 18 target files), including namespaces that currently live under `Module.Identity.Features.Shared.Admin.*` / `.Storefront.*`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | Run `python3 scripts/consolidate-shared.py --module Identity --area admin --dry-run` and `--area storefront --dry-run`; confirm every operation matches the Identity table below (Appendix B, section "Identity"). Then `--module Identity --area admin,storefront --apply`. | | |
| TASK-013 | Post-apply audit: `grep -rl "Module.Identity.Features.Shared.Admin\|Module.Identity.Features.Shared.Storefront" service/Api/src service/Api/tests` must return no matches. | | |
| TASK-014 | Verify Identity: `dotnet build` (clean), `dotnet test service/Api/tests/Module.UnitTests`, `bash scripts/check-feature-conventions.sh`, `bash scripts/check-cross-module-refs.sh`. | | |

### Implementation Phase 5

- GOAL-005: Migrate the Inventory module (38 Admin + 3 Storefront source files -> 20 target files).

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Run `python3 scripts/consolidate-shared.py --module Inventory --area admin --dry-run` and `--area storefront --dry-run`; confirm every operation matches the Inventory table below (Appendix B, section "Inventory"). Then `--module Inventory --area admin,storefront --apply`. | | |
| TASK-016 | Post-apply audit: `grep -rlE "Features.Admin.(StockItems|StockLocations|StockMovements|StockReservations|StockTransfers|Dashboard)" service/Api/src/Module/Inventory/Features/Admin/Shared` must only match the new `Shared/` namespace; any other hit is a failure. | | |
| TASK-017 | Verify Inventory: `dotnet build` (clean), `dotnet test service/Api/tests/Module.UnitTests`, `bash scripts/check-feature-conventions.sh`, `bash scripts/check-cross-module-refs.sh`. | | |

### Implementation Phase 6

- GOAL-006: Migrate the Ordering (11 Admin + 12 Storefront source files -> 10 target files) and Dashboard (1 Admin source file -> 1 target file) modules.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-018 | Run `python3 scripts/consolidate-shared.py --module Ordering --area admin --dry-run` and `--area storefront --dry-run`; confirm every operation matches the Ordering table below (Appendix B, section "Ordering"). Then `--module Ordering --area admin,storefront --apply`. | | |
| TASK-019 | Run `python3 scripts/consolidate-shared.py --module Dashboard --area admin --dry-run`; confirm the single operation matches the Dashboard table below (Appendix B, section "Dashboard"). Then `--module Dashboard --area admin --apply`. | | |
| TASK-020 | Verify Ordering and Dashboard: `dotnet build` (clean), `dotnet test service/Api/tests/Module.UnitTests`, `bash scripts/check-feature-conventions.sh`, `bash scripts/check-cross-module-refs.sh`. | | |

### Implementation Phase 7

- GOAL-007: Migrate the Shipping (13 Admin + 5 Storefront source files, partially pre-consolidated) and Customer (6 Storefront source files -> 3 target files) modules.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-021 | Run `python3 scripts/consolidate-shared.py --module Shipping --area admin --dry-run` and `--area storefront --dry-run`; confirm every operation matches the Shipping table below (Appendix B, section "Shipping"). The two 0-byte `ShippingMethod.Validator.cs` files must be deleted, not merged. Then `--module Shipping --area admin,storefront --apply`. | | |
| TASK-022 | Run `python3 scripts/consolidate-shared.py --module Customer --area storefront --dry-run`; confirm every operation matches the Customer table below (Appendix B, section "Customer"). Then `--module Customer --area storefront --apply`. | | |
| TASK-023 | Verify Shipping and Customer: `dotnet build` (clean), `dotnet test service/Api/tests/Module.UnitTests`, `bash scripts/check-feature-conventions.sh`, `bash scripts/check-cross-module-refs.sh`. | | |

### Implementation Phase 8

- GOAL-008: Final full-repo verification, drift checks, and documentation updates.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | Full verification: `dotnet build`, `dotnet test` (unit suites; integration suites only if Docker available), `bash scripts/check-feature-conventions.sh` (now including AC-006), `bash scripts/check-cross-module-refs.sh`, `python3 scripts/consolidate-shared.py --apply` (idempotency: must report no diff). | | |
| TASK-025 | Drift audit: `grep -rE "/Shared/(Mappings|Models|Validators|Validation|Validations)/" service/Api/src/Module | grep -vE "Features/(Admin|Storefront)/Shared/"` must return no matches; `grep -r "Module.\w+.Features.Shared.Admin\|Module.\w+.Features.Shared.Storefront" service/Api` must return no matches. Record results in the plan's "Status" section. | | |
| TASK-026 | Documentation: refresh `docs/codebase/STRUCTURE.md` and `docs/codebase/ARCHITECTURE.md` feature-layout examples to the consolidated `Shared` convention; remove the `docs/failures/2026-08-16-shared-consolidation-baseline.md` file after the final green run is recorded. | | |

## 3. Alternatives

- **ALT-001**: One global `Shared` folder shared by all modules. Rejected: violates the non-negotiable module-isolation rule (AGENTS.md rule 2), which forbids any module referencing another module's types.
- **ALT-002**: Keep the status quo (per-feature `Shared` folders). Rejected: 244 files across 8 modules with inconsistent naming, drifting namespaces (`Module.Identity.Features.Shared.Admin.*`, `Module.Catalog.Features.Admin.Optiontypes.*`), and no single convention to enforce.
- **ALT-003**: Consolidate only within each feature area (e.g. `Admin/OptionTypes/*/Shared` -> `Admin/OptionTypes/Shared`). Rejected: leaves dozens of `Shared` folders, does not establish the uniform `{Entity}.{Kind}.cs` naming, and does not fix namespace drift.
- **ALT-004**: Keep `.Request`/`.Response`/`.Parameters` file splits and only rename `Store.` -> `Storefront.`. Rejected: does not match the requested `{Entity}.{Kind}.cs` target (`OptionType.Model.cs`), so the plan merges per (Entity, Kind).
- **ALT-005**: Rename only, without namespace rewrite (keep `using Module.X.Features.<FeaturePath>.Shared.*`). Rejected: leaves the moved files under namespaces that contradict their folders; the codebase convention derives namespaces from folders.
- **ALT-006**: Do the migration by hand across all modules. Rejected: 479+ referencing files and 526 `using` occurrences make a script the only deterministic, verifiable option.

## 4. Dependencies

- **DEP-001**: Python 3.12 interpreter (already required by the repo's `service/Embedding` and `benchmarks`).
- **DEP-002**: `dotnet` SDK build/test toolchain with warnings-as-errors enabled.
- **DEP-003**: POSIX shell for the convention scripts (`scripts/check-feature-conventions.sh`, `scripts/check-cross-module-refs.sh`).
- **DEP-004**: The Appendix A old->new namespace map (110 entries) and Appendix B source->target file map (134 rows). The script must be generated to exactly match both; the phase `--dry-run` diffs verify the match.
- **DEP-005**: A green baseline (TASK-001) before Phase 2 begins; each phase depends only on the previous phase's green verification.
- **DEP-006**: Git must track the renames so the large diff stays reviewable; no `git stash`/`restore`/`reset`/`revert` operations are used at any point (AGENTS.md rule 6).

## 5. Files

- **FILE-001**: `plan/refactor-shared-consolidation-1.md` — this plan.
- **FILE-002**: `scripts/consolidate-shared.py` — new deterministic migration script (Phase 1).
- **FILE-003**: `scripts/check-feature-conventions.sh` — extended with AC-006 (Phase 1).
- **FILE-004**: `docs/codebase/CONVENTIONS.md` — new Shared-layout convention section (Phase 1).
- **FILE-005**: `docs/codebase/STRUCTURE.md` — feature-layout examples updated (Phase 8).
- **FILE-006**: `docs/codebase/ARCHITECTURE.md` — feature-layout examples updated (Phase 8).
- **FILE-007**: `docs/failures/2026-08-16-shared-consolidation-baseline.md` — temporary baseline record (created Phase 1, removed Phase 8).
- **FILE-008**: `{Module}/Features/Admin/Shared/` — created for Billing, Catalog, Dashboard, Identity, Inventory, Ordering (Shipping already has one); contains only `Mappings/`, `Models/`, `Validators/`.
- **FILE-009**: `{Module}/Features/Storefront/Shared/` — created for Billing, Catalog, Customer, Ordering (Identity, Inventory, Shipping already have one); contains only `Mappings/`, `Models/`, `Validators/`.
- **FILE-010**: Every legacy `{Module}/Features/{Area}/{FeaturePath}/Shared/{Mappings,Models,Validators,Validation,Validations}` directory — removed after its files are consolidated (approximately 166 Admin + 62 Storefront files across the 8 modules).

## 6. Testing

- **TEST-001**: `dotnet build` succeeds with zero warnings after every phase (warnings-as-errors gate).
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` passes after every phase.
- **TEST-003**: `bash scripts/check-feature-conventions.sh` and `bash scripts/check-cross-module-refs.sh` pass after every phase.
- **TEST-004**: AC-006 (new) passes only when no `Shared/{Mappings,Models,Validators,Validation,Validations}` directory exists below `Features/{Admin|Storefront}/Shared/`.
- **TEST-005**: `scripts/consolidate-shared.py --apply` run twice in a row reports a zero diff (idempotency) and exits 0.
- **TEST-006**: Script completeness self-check: the script fails if a source's declared namespace is absent from the Appendix A map or if two sources for one target declare the same type name (this is what forces the TASK-008 rename before Catalog migration).
- **TEST-007**: Full `dotnet test` (including `Api.Tests` scenario tests, which reference the Shared namespaces; integration suites require Docker) passes at Phase 8.

## 7. Risks & Assumptions

- **RISK-001**: After namespace rewrite, a consumer file may end up with two identical `using <new>;` lines from different old namespaces; with `TreatWarningsAsErrors=true` the CS0105 duplicate-using warning fails the build. Mitigation: the script dedupes `using` lines (TASK-003 step 2); TASK-010/013/016/025 audit for leftovers.
- **RISK-002**: Two distinct types with the same name end up in the same target namespace. Only known instance: Catalog `ParametersValidator` (Taxonomy vs Taxon), resolved by TASK-008 before migration. The script's collision check (TEST-006) guards the rest.
- **RISK-003**: Merging `partial` classes (e.g. `OptionTypeMapping`, `StockLocationValidator`, `TaxonRuleValidations`) must keep the `partial` modifier; a merge that drops it fails to compile, caught by `dotnet build`.
- **RISK-004**: 0-byte placeholder `.cs` files (two `ShippingMethod.Validator.cs` files in Shipping) have no content to merge; they are deleted (TASK-021), never converted to empty targets.
- **RISK-005**: Declared namespaces do not always match folder paths (`Module.Identity.Features.Shared.Admin.Permissions.Shared.Models`, `Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Models`). Mitigation: the script derives old namespaces from the declared namespace in each file, never from the folder path, and fails on any namespace missing from Appendix A.
- **RISK-006**: Large diff and review noise across 8 modules. Mitigation: one module per phase, per-module `--dry-run` review, and one commit per phase (no commits unless requested by the human).
- **RISK-007**: `RecentProductData` and `EmbeddingOrchestrator` appear twice in the codebase. These are in different modules (`Catalog` vs `Dashboard`) or are partial classes of one type, so no collision occurs under per-module consolidation.
- **ASSUMPTION-001**: No behavioral change is required; merging alters only layout, filenames, namespaces, and usings. If a merge changes semantics, the phase stops and the plan is reassessed.
- **ASSUMPTION-002**: Every consolidated file uses a file-scoped namespace and contains only balanced `#region`/`#endregion` preprocessor lines (verified across all 244 source files); concatenating type bodies is therefore safe.
- **ASSUMPTION-003**: No code outside `service/Api` references the `Module.*` namespaces (verified: 0 references in `app/`, `service/Embedding`, `benchmarks`); only `service/Api/src/Module`, `service/Api/src/Api`, `service/Api/src/Migrations`, and `service/Api/tests` need rewriting.
- **ASSUMPTION-004**: `dotnet build` is the ground-truth correctness validator; a green build for a phase is the completion criterion for that phase's migration.

## 8. Related Specifications / Further Reading

- [docs/codebase/ARCHITECTURE.md](../docs/codebase/ARCHITECTURE.md)
- [docs/codebase/CONVENTIONS.md](../docs/codebase/CONVENTIONS.md)
- [docs/codebase/STRUCTURE.md](../docs/codebase/STRUCTURE.md)
- [AGENTS.md](../AGENTS.md)

## Appendix A: Old -> New Namespace Map

All rows are the exact (old, new) pairs the migration script embeds. A row where old == new means the namespace already conforms; the script leaves such namespaces untouched. Sources with no declared namespace (the two 0-byte Shipping files) are excluded.

| Old namespace | New namespace |
|---|---|
| `Module.Billing.Features.Admin.PaymentMethods.Shared.Mappings` | `Module.Billing.Features.Admin.Shared.Mappings` |
| `Module.Billing.Features.Admin.PaymentMethods.Shared.Models` | `Module.Billing.Features.Admin.Shared.Models` |
| `Module.Billing.Features.Admin.PaymentMethods.Shared.Validators` | `Module.Billing.Features.Admin.Shared.Validators` |
| `Module.Billing.Features.Admin.Payments.Shared.Mappings` | `Module.Billing.Features.Admin.Shared.Mappings` |
| `Module.Billing.Features.Admin.Payments.Shared.Models` | `Module.Billing.Features.Admin.Shared.Models` |
| `Module.Billing.Features.Admin.Payments.Shared.Validators` | `Module.Billing.Features.Admin.Shared.Validators` |
| `Module.Billing.Features.Storefront.Payment.Shared.Mappings` | `Module.Billing.Features.Storefront.Shared.Mappings` |
| `Module.Billing.Features.Storefront.Payment.Shared.Models` | `Module.Billing.Features.Storefront.Shared.Models` |
| `Module.Billing.Features.Storefront.Payment.Shared.Validators` | `Module.Billing.Features.Storefront.Shared.Validators` |
| `Module.Billing.Features.Storefront.PaymentMethods.Shared.Mappings` | `Module.Billing.Features.Storefront.Shared.Mappings` |
| `Module.Billing.Features.Storefront.PaymentMethods.Shared.Models` | `Module.Billing.Features.Storefront.Shared.Models` |
| `Module.Billing.Features.Storefront.PaymentMethods.Shared.Validators` | `Module.Billing.Features.Storefront.Shared.Validators` |
| `Module.Catalog.Features.Admin.Dashboard.Get.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.OptionTypes.Shared.Mappings` | `Module.Catalog.Features.Admin.Shared.Mappings` |
| `Module.Catalog.Features.Admin.OptionTypes.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.OptionTypes.Shared.Validators` | `Module.Catalog.Features.Admin.Shared.Validators` |
| `Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Mappings` | `Module.Catalog.Features.Admin.Shared.Mappings` |
| `Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.Optiontypes.Values.Shared.Validators` | `Module.Catalog.Features.Admin.Shared.Validators` |
| `Module.Catalog.Features.Admin.Products.Classifications.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.Products.Options.Shared.Mappings` | `Module.Catalog.Features.Admin.Shared.Mappings` |
| `Module.Catalog.Features.Admin.Products.Options.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.Products.Options.Shared.Validations` | `Module.Catalog.Features.Admin.Shared.Validators` |
| `Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Mappings` | `Module.Catalog.Features.Admin.Shared.Mappings` |
| `Module.Catalog.Features.Admin.Products.ProductClassifications.Shared.Validations` | `Module.Catalog.Features.Admin.Shared.Validators` |
| `Module.Catalog.Features.Admin.Products.Shared.Mappings` | `Module.Catalog.Features.Admin.Shared.Mappings` |
| `Module.Catalog.Features.Admin.Products.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.Products.Shared.Validation` | `Module.Catalog.Features.Admin.Shared.Validators` |
| `Module.Catalog.Features.Admin.Taxonomies.Shared.Mappings` | `Module.Catalog.Features.Admin.Shared.Mappings` |
| `Module.Catalog.Features.Admin.Taxonomies.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.Taxonomies.Shared.Validators` | `Module.Catalog.Features.Admin.Shared.Validators` |
| `Module.Catalog.Features.Admin.Taxons.Rules.Shared.Mappings` | `Module.Catalog.Features.Admin.Shared.Mappings` |
| `Module.Catalog.Features.Admin.Taxons.Rules.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.Taxons.Rules.Shared.Validations` | `Module.Catalog.Features.Admin.Shared.Validators` |
| `Module.Catalog.Features.Admin.Taxons.Shared.Mappings` | `Module.Catalog.Features.Admin.Shared.Mappings` |
| `Module.Catalog.Features.Admin.Taxons.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.Taxons.Shared.Validators` | `Module.Catalog.Features.Admin.Shared.Validators` |
| `Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.Variants.Images.Shared.Mappings` | `Module.Catalog.Features.Admin.Shared.Mappings` |
| `Module.Catalog.Features.Admin.Variants.Images.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.Variants.Images.Shared.Validators` | `Module.Catalog.Features.Admin.Shared.Validators` |
| `Module.Catalog.Features.Admin.Variants.Prices.Shared.Mappings` | `Module.Catalog.Features.Admin.Shared.Mappings` |
| `Module.Catalog.Features.Admin.Variants.Prices.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.Variants.Shared.Mappings` | `Module.Catalog.Features.Admin.Shared.Mappings` |
| `Module.Catalog.Features.Admin.Variants.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Admin.Variants.Shared.Validators` | `Module.Catalog.Features.Admin.Shared.Validators` |
| `Module.Catalog.Features.Admin.Variants.Values.Shared.Models` | `Module.Catalog.Features.Admin.Shared.Models` |
| `Module.Catalog.Features.Storefront.Classifications.Shared.Mappings` | `Module.Catalog.Features.Storefront.Shared.Mappings` |
| `Module.Catalog.Features.Storefront.Classifications.Shared.Models` | `Module.Catalog.Features.Storefront.Shared.Models` |
| `Module.Catalog.Features.Storefront.OptionTypes.Shared.Mappings` | `Module.Catalog.Features.Storefront.Shared.Mappings` |
| `Module.Catalog.Features.Storefront.OptionTypes.Shared.Models` | `Module.Catalog.Features.Storefront.Shared.Models` |
| `Module.Catalog.Features.Storefront.Products.Images.Inferences.Shared.Mappings` | `Module.Catalog.Features.Storefront.Shared.Mappings` |
| `Module.Catalog.Features.Storefront.Products.Images.Inferences.Shared.Models` | `Module.Catalog.Features.Storefront.Shared.Models` |
| `Module.Catalog.Features.Storefront.Products.Images.Search.Shared.Models` | `Module.Catalog.Features.Storefront.Shared.Models` |
| `Module.Catalog.Features.Storefront.Products.Shared.Mappings` | `Module.Catalog.Features.Storefront.Shared.Mappings` |
| `Module.Catalog.Features.Storefront.Products.Shared.Models` | `Module.Catalog.Features.Storefront.Shared.Models` |
| `Module.Customer.Features.Storefront.Wishlists.Shared.Mappings` | `Module.Customer.Features.Storefront.Shared.Mappings` |
| `Module.Customer.Features.Storefront.Wishlists.Shared.Models` | `Module.Customer.Features.Storefront.Shared.Models` |
| `Module.Dashboard.Features.Admin.Get.Shared.Models` | `Module.Dashboard.Features.Admin.Shared.Models` |
| `Module.Identity.Features.Shared.Admin.Permissions.Shared.Mappings` | `Module.Identity.Features.Admin.Shared.Mappings` |
| `Module.Identity.Features.Shared.Admin.Permissions.Shared.Models` | `Module.Identity.Features.Admin.Shared.Models` |
| `Module.Identity.Features.Shared.Admin.Roles.Shared.Mappings` | `Module.Identity.Features.Admin.Shared.Mappings` |
| `Module.Identity.Features.Shared.Admin.Roles.Shared.Models` | `Module.Identity.Features.Admin.Shared.Models` |
| `Module.Identity.Features.Shared.Admin.Roles.Shared.Validators` | `Module.Identity.Features.Admin.Shared.Validators` |
| `Module.Identity.Features.Shared.Admin.Users.Roles.Shared.Models` | `Module.Identity.Features.Admin.Shared.Models` |
| `Module.Identity.Features.Shared.Admin.Users.Shared.Mappings` | `Module.Identity.Features.Admin.Shared.Mappings` |
| `Module.Identity.Features.Shared.Admin.Users.Shared.Models` | `Module.Identity.Features.Admin.Shared.Models` |
| `Module.Identity.Features.Shared.Admin.Users.Shared.Validators` | `Module.Identity.Features.Admin.Shared.Validators` |
| `Module.Identity.Features.Shared.Storefront.Auth.Login.External.Shared.Models` | `Module.Identity.Features.Storefront.Shared.Models` |
| `Module.Identity.Features.Shared.Storefront.Auth.Shared.Models` | `Module.Identity.Features.Storefront.Shared.Models` |
| `Module.Identity.Features.Shared.Storefront.Emails.Shared.Models` | `Module.Identity.Features.Storefront.Shared.Models` |
| `Module.Identity.Features.Shared.Storefront.Passwords.Shared.Models` | `Module.Identity.Features.Storefront.Shared.Models` |
| `Module.Identity.Features.Shared.Storefront.Shared.Mappings` | `Module.Identity.Features.Storefront.Shared.Mappings` |
| `Module.Identity.Features.Shared.Storefront.Shared.Models` | `Module.Identity.Features.Storefront.Shared.Models` |
| `Module.Inventory.Features.Admin.Dashboard.Shared.Models` | `Module.Inventory.Features.Admin.Shared.Models` |
| `Module.Inventory.Features.Admin.StockItems.Shared.Mappings` | `Module.Inventory.Features.Admin.Shared.Mappings` |
| `Module.Inventory.Features.Admin.StockItems.Shared.Models` | `Module.Inventory.Features.Admin.Shared.Models` |
| `Module.Inventory.Features.Admin.StockItems.Shared.Validators` | `Module.Inventory.Features.Admin.Shared.Validators` |
| `Module.Inventory.Features.Admin.StockLocations.Shared.Mappings` | `Module.Inventory.Features.Admin.Shared.Mappings` |
| `Module.Inventory.Features.Admin.StockLocations.Shared.Models` | `Module.Inventory.Features.Admin.Shared.Models` |
| `Module.Inventory.Features.Admin.StockLocations.Shared.Validators` | `Module.Inventory.Features.Admin.Shared.Validators` |
| `Module.Inventory.Features.Admin.StockMovements.Shared.Mappings` | `Module.Inventory.Features.Admin.Shared.Mappings` |
| `Module.Inventory.Features.Admin.StockMovements.Shared.Models` | `Module.Inventory.Features.Admin.Shared.Models` |
| `Module.Inventory.Features.Admin.StockMovements.Shared.Validators` | `Module.Inventory.Features.Admin.Shared.Validators` |
| `Module.Inventory.Features.Admin.StockReservations.Shared.Mappings` | `Module.Inventory.Features.Admin.Shared.Mappings` |
| `Module.Inventory.Features.Admin.StockReservations.Shared.Models` | `Module.Inventory.Features.Admin.Shared.Models` |
| `Module.Inventory.Features.Admin.StockReservations.Shared.Validators` | `Module.Inventory.Features.Admin.Shared.Validators` |
| `Module.Inventory.Features.Admin.StockTransfers.Shared.Mappings` | `Module.Inventory.Features.Admin.Shared.Mappings` |
| `Module.Inventory.Features.Admin.StockTransfers.Shared.Models` | `Module.Inventory.Features.Admin.Shared.Models` |
| `Module.Inventory.Features.Admin.StockTransfers.Shared.Validators` | `Module.Inventory.Features.Admin.Shared.Validators` |
| `Module.Inventory.Features.Storefront.Shared.Models` | `Module.Inventory.Features.Storefront.Shared.Models` |
| `Module.Inventory.Features.Storefront.Shared.Validators` | `Module.Inventory.Features.Storefront.Shared.Validators` |
| `Module.Inventory.Features.Storefront.StockReservations.Shared.Models` | `Module.Inventory.Features.Storefront.Shared.Models` |
| `Module.Ordering.Features.Admin.Dashboard.Get.Shared.Models` | `Module.Ordering.Features.Admin.Shared.Models` |
| `Module.Ordering.Features.Admin.Orders.Shared.Mappings` | `Module.Ordering.Features.Admin.Shared.Mappings` |
| `Module.Ordering.Features.Admin.Orders.Shared.Models` | `Module.Ordering.Features.Admin.Shared.Models` |
| `Module.Ordering.Features.Admin.Orders.Shared.Validators` | `Module.Ordering.Features.Admin.Shared.Validators` |
| `Module.Ordering.Features.Storefront.Cart.Shared.Mappings` | `Module.Ordering.Features.Storefront.Shared.Mappings` |
| `Module.Ordering.Features.Storefront.Cart.Shared.Models` | `Module.Ordering.Features.Storefront.Shared.Models` |
| `Module.Ordering.Features.Storefront.Cart.Shared.Validators` | `Module.Ordering.Features.Storefront.Shared.Validators` |
| `Module.Ordering.Features.Storefront.Orders.GetTracking.Shared.Models` | `Module.Ordering.Features.Storefront.Shared.Models` |
| `Module.Ordering.Features.Storefront.Orders.Shared.Mappings` | `Module.Ordering.Features.Storefront.Shared.Mappings` |
| `Module.Ordering.Features.Storefront.Orders.Shared.Models` | `Module.Ordering.Features.Storefront.Shared.Models` |
| `Module.Shipping.Features.Admin.Shared.Mappings` | `Module.Shipping.Features.Admin.Shared.Mappings` |
| `Module.Shipping.Features.Admin.Shared.Models` | `Module.Shipping.Features.Admin.Shared.Models` |
| `Module.Shipping.Features.Admin.Shared.Validators` | `Module.Shipping.Features.Admin.Shared.Validators` |
| `Module.Shipping.Features.Admin.Shipments.Shared.Mappings` | `Module.Shipping.Features.Admin.Shared.Mappings` |
| `Module.Shipping.Features.Admin.ShippingMethods.Shared.Models` | `Module.Shipping.Features.Admin.Shared.Models` |
| `Module.Shipping.Features.Storefront.Shared.Mappings` | `Module.Shipping.Features.Storefront.Shared.Mappings` |
| `Module.Shipping.Features.Storefront.Shared.Models` | `Module.Shipping.Features.Storefront.Shared.Models` |

## Appendix B: Per-Module File Migration Map

All paths are relative to `{Module}/Features/{Admin|Storefront}/`. The `Target` column is the file
written under `{Module}/Features/{Admin|Storefront}/Shared/`; `MERGE` means all listed sources are
merged into one target, `move` means a direct rename+relocation. Sources are listed in deterministic
(sort) order. A source whose path equals its target (e.g. Shipping rows already at `Shared/`)
contributes its existing content to the merge and is not deleted.

### Billing Admin

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Mappings/Payment.Mapping.cs` | MERGE | `Payments/Shared/Mappings/Payment.Mapping.Domain.cs; Payments/Shared/Mappings/Payment.Mapping.Model.cs` |
| `Shared/Mappings/PaymentMethod.Mapping.cs` | MERGE | `PaymentMethods/Shared/Mappings/PaymentMethod.Mapping.Domain.cs; PaymentMethods/Shared/Mappings/PaymentMethod.Mapping.Model.cs` |
| `Shared/Models/Payment.Model.cs` | MERGE | `Payments/Shared/Models/Payment.Model.Parameters.cs; Payments/Shared/Models/Payment.Model.Request.cs; Payments/Shared/Models/Payment.Model.Response.cs` |
| `Shared/Models/PaymentMethod.Model.cs` | MERGE | `PaymentMethods/Shared/Models/PaymentMethod.Model.Parameters.cs; PaymentMethods/Shared/Models/PaymentMethod.Model.Request.cs; PaymentMethods/Shared/Models/PaymentMethod.Model.Response.cs` |
| `Shared/Models/PaymentMethodUpdate.Model.cs` | MERGE | `PaymentMethods/Shared/Models/PaymentMethodUpdateParameters.cs; PaymentMethods/Shared/Models/PaymentMethodUpdateRequest.cs` |
| `Shared/Validators/Payment.Validator.cs` | move | `Payments/Shared/Validators/Payment.Validator.cs` |
| `Shared/Validators/PaymentMethod.Validator.cs` | move | `PaymentMethods/Shared/Validators/PaymentMethod.Validator.cs` |

### Billing Storefront

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Mappings/Storefront.Payment.Mapping.cs` | move | `Payment/Shared/Mappings/PaymentStore.Mapping.cs` |
| `Shared/Mappings/Storefront.PaymentMethod.Mapping.cs` | move | `PaymentMethods/Shared/Mappings/PaymentMethodStore.Mapping.cs` |
| `Shared/Models/Storefront.Payment.Model.cs` | MERGE | `Payment/Shared/Models/PaymentStore.Model.Request.cs; Payment/Shared/Models/PaymentStore.Model.Response.cs` |
| `Shared/Models/Storefront.PaymentMethod.Model.cs` | move | `PaymentMethods/Shared/Models/PaymentMethodStore.Model.Response.cs` |
| `Shared/Validators/Storefront.Payment.Validator.cs` | move | `Payment/Shared/Validators/PaymentStore.Validator.cs` |
| `Shared/Validators/Storefront.PaymentMethod.Validator.cs` | move | `PaymentMethods/Shared/Validators/PaymentMethodStore.Validator.cs` |

### Catalog Admin

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Mappings/OptionType.Mapping.cs` | MERGE | `OptionTypes/Shared/Mappings/OptionType.Mapping.Domain.cs; OptionTypes/Shared/Mappings/OptionType.Mapping.Model.cs` |
| `Shared/Mappings/OptionValue.Mapping.cs` | MERGE | `OptionTypes/Values/Shared/Mappings/OptionValue.Mapping.Domain.cs; OptionTypes/Values/Shared/Mappings/OptionValue.Mapping.Model.cs` |
| `Shared/Mappings/Price.Mapping.cs` | move | `Variants/Prices/Shared/Mappings/Price.Mapping.cs` |
| `Shared/Mappings/Product.Mapping.cs` | MERGE | `Products/Shared/Mappings/Product.Mapping.Domain.cs; Products/Shared/Mappings/Product.Mapping.Model.cs` |
| `Shared/Mappings/ProductClassification.Mapping.cs` | MERGE | `Products/Classifications/Shared/Mappings/ProductClassification.Mapping.Domain.cs; Products/Classifications/Shared/Mappings/ProductClassification.Mapping.Model.cs` |
| `Shared/Mappings/ProductOptionType.Mapping.cs` | MERGE | `Products/Options/Shared/Mappings/ProductOptionType.Mapping.Domain.cs; Products/Options/Shared/Mappings/ProductOptionType.Mapping.Model.cs` |
| `Shared/Mappings/Taxon.Mapping.cs` | MERGE | `Taxonomies/Taxons/Shared/Mappings/Taxon.Mapping.Domain.cs; Taxonomies/Taxons/Shared/Mappings/Taxon.Mapping.Model.cs` |
| `Shared/Mappings/TaxonRule.Mapping.cs` | MERGE | `Taxonomies/Taxons/Rules/Shared/Mappings/TaxonRule.Mapping.Domain.cs; Taxonomies/Taxons/Rules/Shared/Mappings/TaxonRule.Mapping.Model.cs` |
| `Shared/Mappings/Taxonomy.Mapping.cs` | MERGE | `Taxonomies/Shared/Mappings/Taxonomy.Mapping.Domain.cs; Taxonomies/Shared/Mappings/Taxonomy.Mapping.Model.cs` |
| `Shared/Mappings/Variant.Mapping.cs` | MERGE | `Variants/Shared/Mappings/Variant.Mapping.Domain.cs; Variants/Shared/Mappings/Variant.Mapping.Model.cs` |
| `Shared/Mappings/VariantImage.Mapping.cs` | move | `Variants/Images/Shared/Mappings/VariantImage.Mapping.Model.cs` |
| `Shared/Models/CatalogDashboard.Model.cs` | move | `Dashboard/Get/Shared/Models/CatalogDashboard.Model.Parameters.cs` |
| `Shared/Models/ImageEmbedding.Model.cs` | MERGE | `Variants/Images/Embeddings/Shared/Models/ImageEmbedding.Model.Parameters.cs; Variants/Images/Embeddings/Shared/Models/ImageEmbedding.Model.Request.cs; Variants/Images/Embeddings/Shared/Models/ImageEmbedding.Model.Response.cs` |
| `Shared/Models/OptionType.Model.cs` | MERGE | `OptionTypes/Shared/Models/OptionType.Model.Parameters.cs; OptionTypes/Shared/Models/OptionType.Model.Request.cs; OptionTypes/Shared/Models/OptionType.Model.Response.cs` |
| `Shared/Models/OptionValue.Model.cs` | MERGE | `OptionTypes/Values/Shared/Models/OptionValue.Model.Parameters.cs; OptionTypes/Values/Shared/Models/OptionValue.Model.Request.cs; OptionTypes/Values/Shared/Models/OptionValue.Model.Response.cs` |
| `Shared/Models/Price.Model.cs` | MERGE | `Variants/Prices/Shared/Models/Price.Model.Parameters.cs; Variants/Prices/Shared/Models/Price.Model.Request.cs; Variants/Prices/Shared/Models/Price.Model.Response.cs` |
| `Shared/Models/Product.Model.cs` | MERGE | `Products/Shared/Models/Product.Model.Parameters.cs; Products/Shared/Models/Product.Model.Request.cs; Products/Shared/Models/Product.Model.Response.cs` |
| `Shared/Models/ProductClassification.Model.cs` | move | `Products/Classifications/Shared/Models/ProductClassification.Model.cs` |
| `Shared/Models/ProductOptionType.Model.cs` | move | `Products/Options/Shared/Models/ProductOptionType.Model.cs` |
| `Shared/Models/Taxon.Model.cs` | MERGE | `Taxonomies/Taxons/Shared/Models/Taxon.Model.Parameters.cs; Taxonomies/Taxons/Shared/Models/Taxon.Model.Request.cs; Taxonomies/Taxons/Shared/Models/Taxon.Model.Response.cs` |
| `Shared/Models/TaxonRule.Model.cs` | MERGE | `Taxonomies/Taxons/Rules/Shared/Models/TaxonRule.Model.Action.cs; Taxonomies/Taxons/Rules/Shared/Models/TaxonRule.Model.Collection.cs; Taxonomies/Taxons/Rules/Shared/Models/TaxonRule.Model.Parameters.cs; Taxonomies/Taxons/Rules/Shared/Models/TaxonRule.Model.Request.cs; Taxonomies/Taxons/Rules/Shared/Models/TaxonRule.Model.Response.cs` |
| `Shared/Models/Taxonomy.Model.cs` | MERGE | `Taxonomies/Shared/Models/Taxonomy.Model.Parameters.cs; Taxonomies/Shared/Models/Taxonomy.Model.Request.cs; Taxonomies/Shared/Models/Taxonomy.Model.Response.cs` |
| `Shared/Models/Variant.Model.cs` | MERGE | `Variants/Shared/Models/Variant.Model.Parameters.cs; Variants/Shared/Models/Variant.Model.Request.cs; Variants/Shared/Models/Variant.Model.Response.cs` |
| `Shared/Models/VariantImage.Model.cs` | MERGE | `Variants/Images/Shared/Models/VariantImage.Model.Parameters.cs; Variants/Images/Shared/Models/VariantImage.Model.Request.cs; Variants/Images/Shared/Models/VariantImage.Model.Response.cs` |
| `Shared/Models/VariantOptionValue.Model.cs` | move | `Variants/Values/Shared/Models/VariantOptionValue.Model.Parameters.cs` |
| `Shared/Models/VariantPrice.Model.cs` | MERGE | `Variants/Prices/Shared/Models/VariantPrice.Model.Action.cs; Variants/Prices/Shared/Models/VariantPrice.Model.Collection.cs` |
| `Shared/Validators/OptionType.Validator.cs` | move | `OptionTypes/Shared/Validators/OptionType.Validator.cs` |
| `Shared/Validators/OptionValue.Validator.cs` | move | `OptionTypes/Values/Shared/Validators/OptionValue.Validator.cs` |
| `Shared/Validators/Product.Validator.cs` | move | `Products/Shared/Validation/Product.Validator.cs` |
| `Shared/Validators/ProductClassification.Validator.cs` | move | `Products/Classifications/Shared/Validations/ProductClassification.Validator.cs` |
| `Shared/Validators/ProductOptionType.Validator.cs` | move | `Products/Options/Shared/Validations/ProductOptionType.Validator.cs` |
| `Shared/Validators/Taxon.Validator.cs` | move | `Taxonomies/Taxons/Shared/Validators/Taxon.Validator.Request.cs` |
| `Shared/Validators/TaxonRule.Validator.cs` | move | `Taxonomies/Taxons/Rules/Shared/Validations/TaxonRuleValidationExtension.cs` |
| `Shared/Validators/Taxonomy.Validator.cs` | move | `Taxonomies/Shared/Validators/Taxonomy.Validator.Request.cs` |
| `Shared/Validators/Variant.Validator.cs` | move | `Variants/Shared/Validators/Variant.Validator.cs` |
| `Shared/Validators/VariantImage.Validator.cs` | move | `Variants/Images/Shared/Validators/VariantImage.Validator.cs` |

### Catalog Storefront

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Mappings/Storefront.OptionType.Mapping.cs` | move | `OptionTypes/Shared/Mappings/Store.OptionType.Mapping.cs` |
| `Shared/Mappings/Storefront.OptionValue.Mapping.cs` | move | `OptionTypes/Shared/Mappings/Store.OptionValue.Mapping.cs` |
| `Shared/Mappings/Storefront.Product.Mapping.cs` | move | `Products/Shared/Mappings/Store.Product.Mapping.cs` |
| `Shared/Mappings/Storefront.Taxon.Mapping.cs` | move | `Taxonomies/Shared/Mappings/Store.Taxon.Mapping.cs` |
| `Shared/Mappings/Storefront.Taxonomy.Mapping.cs` | move | `Taxonomies/Shared/Mappings/Store.Taxonomy.Mapping.cs` |
| `Shared/Mappings/Storefront.Variant.Mapping.cs` | move | `Products/Shared/Mappings/Store.Variant.Mapping.cs` |
| `Shared/Mappings/Storefront.VariantImage.Mapping.cs` | move | `Products/Shared/Mappings/Store.VariantImage.Mapping.cs` |
| `Shared/Mappings/Storefront.VariantPrice.Mapping.cs` | move | `Products/Shared/Mappings/Store.VariantPrice.Mapping.cs` |
| `Shared/Mappings/Storefront.VisualSearchModel.Mapping.cs` | move | `Products/Images/Inferences/Shared/Mappings/VisualSearchModel.Mapping.cs` |
| `Shared/Models/Storefront.ImageSearch.Model.cs` | move | `Products/Images/Search/Shared/Models/ImageSearch.Model.Parameters.cs` |
| `Shared/Models/Storefront.OptionType.Model.cs` | move | `OptionTypes/Shared/Models/Store.OptionType.Model.cs` |
| `Shared/Models/Storefront.OptionValue.Model.cs` | move | `OptionTypes/Shared/Models/Store.OptionValue.Model.cs` |
| `Shared/Models/Storefront.Product.Model.cs` | move | `Products/Shared/Models/Store.Product.Model.cs` |
| `Shared/Models/Storefront.Taxon.Model.cs` | move | `Taxonomies/Shared/Models/Store.Taxon.Model.cs` |
| `Shared/Models/Storefront.Taxonomy.Model.cs` | move | `Taxonomies/Shared/Models/Store.Taxonomy.Model.cs` |
| `Shared/Models/Storefront.Variant.Model.cs` | move | `Products/Shared/Models/Store.Variant.Model.cs` |
| `Shared/Models/Storefront.VariantImage.Model.cs` | move | `Products/Shared/Models/Store.VariantImage.Model.cs` |
| `Shared/Models/Storefront.VariantPrice.Model.cs` | move | `Products/Shared/Models/Store.VariantPrice.Model.cs` |
| `Shared/Models/Storefront.VisualSearchModel.Model.cs` | move | `Products/Images/Inferences/Shared/Models/VisualSearchModel.Response.cs` |

### Customer Storefront

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Mappings/Storefront.Wishlist.Mapping.cs` | MERGE | `Wishlists/Shared/Mappings/Wishlist.Mapping.Domain.cs; Wishlists/Shared/Mappings/Wishlist.Mapping.Model.cs` |
| `Shared/Models/Storefront.WishedItem.Model.cs` | move | `Wishlists/Shared/Models/WishedItem.Model.Response.cs` |
| `Shared/Models/Storefront.Wishlist.Model.cs` | MERGE | `Wishlists/Shared/Models/Wishlist.Model.Parameters.cs; Wishlists/Shared/Models/Wishlist.Model.Request.cs; Wishlists/Shared/Models/Wishlist.Model.Response.cs` |

### Dashboard Admin

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Models/Dashboard.Model.cs` | move | `Get/Shared/Models/Dashboard.Model.Parameters.cs` |

### Identity Admin

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Mappings/Permission.Mapping.cs` | move | `Permissions/Shared/Mappings/Permission.Mapping.Response.cs` |
| `Shared/Mappings/PermissionComposite.Mapping.cs` | move | `Permissions/Shared/Mappings/PermissionComposite.Mapping.cs` |
| `Shared/Mappings/Role.Mapping.cs` | MERGE | `Roles/Shared/Mappings/Role.Mapping.Domain.cs; Roles/Shared/Mappings/Role.Mapping.Model.cs` |
| `Shared/Mappings/User.Mapping.cs` | MERGE | `Users/Shared/Mappings/User.Mapping.Domain.cs; Users/Shared/Mappings/User.Mapping.Model.cs` |
| `Shared/Models/Permission.Model.cs` | MERGE | `Permissions/Shared/Models/Permission.Model.Category.cs; Permissions/Shared/Models/Permission.Model.Group.cs; Permissions/Shared/Models/Permission.Model.Resouce.cs; Permissions/Shared/Models/Permission.Model.Response.cs` |
| `Shared/Models/PermissionCollection.Model.cs` | move | `Permissions/Shared/Models/PermissionCollection.Model.Parameters.cs` |
| `Shared/Models/Role.Model.cs` | MERGE | `Roles/Shared/Models/Role.Model.Parameters.cs; Roles/Shared/Models/Role.Model.Request.cs; Roles/Shared/Models/Role.Model.Response.cs` |
| `Shared/Models/User.Model.cs` | MERGE | `Users/Shared/Models/User.Model.Parameters.cs; Users/Shared/Models/User.Model.Request.cs; Users/Shared/Models/User.Model.Response.cs` |
| `Shared/Models/UserRoles.Model.cs` | move | `Users/Roles/Shared/Models/UserRoles.Model.Parameters.cs` |
| `Shared/Validators/Role.Validator.cs` | move | `Roles/Shared/Validators/Role.Validator.cs` |
| `Shared/Validators/User.Validator.cs` | MERGE | `Users/Shared/Validators/User.Validator.RoleName.cs; Users/Shared/Validators/User.Validator.cs` |

### Identity Storefront

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Mappings/Storefront.AuthToken.Mapping.cs` | move | `Shared/Mappings/AuthToken.Mapping.cs` |
| `Shared/Mappings/Storefront.Session.Mapping.cs` | move | `Shared/Mappings/Session.Mapping.cs` |
| `Shared/Models/Storefront.Auth.Model.cs` | MERGE | `Shared/Models/Auth.Request.Model.cs; Shared/Models/Auth.Response.Model.cs` |
| `Shared/Models/Storefront.Email.Model.cs` | MERGE | `Emails/Shared/Models/Email.Model.Parameters.cs; Emails/Shared/Models/Email.Model.Request.cs; Emails/Shared/Models/Email.Model.Response.cs` |
| `Shared/Models/Storefront.External.Model.cs` | MERGE | `Auth/Login/External/Shared/Models/External.Model.Request.cs; Auth/Login/External/Shared/Models/External.Model.Response.cs` |
| `Shared/Models/Storefront.Password.Model.cs` | MERGE | `Passwords/Shared/Models/Password.Model.Parameters.cs; Passwords/Shared/Models/Password.Model.Request.cs` |
| `Shared/Models/Storefront.Register.Model.cs` | move | `Auth/Shared/Models/Register.Model.Request.cs` |

### Inventory Admin

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Mappings/ImportStockItems.Mapping.cs` | move | `StockItems/Shared/Mappings/ImportStockItems.Mapping.cs` |
| `Shared/Mappings/StockItem.Mapping.cs` | MERGE | `StockItems/Shared/Mappings/StockItem.Mapping.Domain.cs; StockItems/Shared/Mappings/StockItem.Mapping.Model.cs` |
| `Shared/Mappings/StockLocation.Mapping.cs` | MERGE | `StockLocations/Shared/Mappings/StockLocation.Mapping.Domain.cs; StockLocations/Shared/Mappings/StockLocation.Mapping.Model.cs` |
| `Shared/Mappings/StockMovement.Mapping.cs` | MERGE | `StockMovements/Shared/Mappings/StockMovement.Mapping.Domain.cs; StockMovements/Shared/Mappings/StockMovement.Mapping.Model.cs` |
| `Shared/Mappings/StockReservation.Mapping.cs` | MERGE | `StockReservations/Shared/Mappings/StockReservation.Mapping.Domain.cs; StockReservations/Shared/Mappings/StockReservation.Mapping.Model.cs` |
| `Shared/Mappings/StockTransfer.Mapping.cs` | MERGE | `StockTransfers/Shared/Mappings/StockTransfer.Mapping.Domain.cs; StockTransfers/Shared/Mappings/StockTransfer.Mapping.Model.cs` |
| `Shared/Models/InventoryDashboard.Model.cs` | move | `Dashboard/Shared/Models/InventoryDashboard.Model.Parameters.cs` |
| `Shared/Models/StockItem.Model.cs` | MERGE | `StockItems/Shared/Models/StockItem.Model.Parameters.cs; StockItems/Shared/Models/StockItem.Model.Request.cs; StockItems/Shared/Models/StockItem.Model.Response.cs` |
| `Shared/Models/StockLocation.Model.cs` | MERGE | `StockLocations/Shared/Models/StockLocation.Model.Parameters.cs; StockLocations/Shared/Models/StockLocation.Model.Request.cs; StockLocations/Shared/Models/StockLocation.Model.Response.cs` |
| `Shared/Models/StockMovement.Model.cs` | MERGE | `StockMovements/Shared/Models/StockMovement.Model.Parameters.cs; StockMovements/Shared/Models/StockMovement.Model.Response.cs` |
| `Shared/Models/StockReservation.Model.cs` | MERGE | `StockReservations/Shared/Models/StockReservation.Model.Parameters.cs; StockReservations/Shared/Models/StockReservation.Model.Request.cs; StockReservations/Shared/Models/StockReservation.Model.Response.cs` |
| `Shared/Models/StockTransfer.Model.cs` | MERGE | `StockTransfers/Shared/Models/StockTransfer.Model.Parameters.cs; StockTransfers/Shared/Models/StockTransfer.Model.ReceiveRequest.cs; StockTransfers/Shared/Models/StockTransfer.Model.Request.cs; StockTransfers/Shared/Models/StockTransfer.Model.Response.cs` |
| `Shared/Validators/StockItem.Validator.cs` | move | `StockItems/Shared/Validators/StockItem.Validator.cs` |
| `Shared/Validators/StockLocation.Validator.cs` | MERGE | `StockLocations/Shared/Validators/StockLocation.Validator.Address.cs; StockLocations/Shared/Validators/StockLocation.Validator.City.cs; StockLocations/Shared/Validators/StockLocation.Validator.Code.cs; StockLocations/Shared/Validators/StockLocation.Validator.Name.cs; StockLocations/Shared/Validators/StockLocation.Validator.Parameters.cs; StockLocations/Shared/Validators/StockLocation.Validator.Phone.cs; StockLocations/Shared/Validators/StockLocation.Validator.PostalCode.cs` |
| `Shared/Validators/StockMovement.Validator.cs` | move | `StockMovements/Shared/Validators/StockMovement.Validator.cs` |
| `Shared/Validators/StockReservation.Validator.cs` | move | `StockReservations/Shared/Validators/StockReservation.Validator.cs` |
| `Shared/Validators/StockTransfer.Validator.cs` | move | `StockTransfers/Shared/Validators/StockTransfer.Validator.cs` |

### Inventory Storefront

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Models/Storefront.Inventory.Model.cs` | move | `Shared/Models/Inventory.Storefront.Model.cs` |
| `Shared/Models/Storefront.StockReservationReserve.Model.cs` | move | `StockReservations/Shared/Models/StockReservationReserve.Model.Parameters.cs` |
| `Shared/Validators/Storefront.Inventory.Validator.cs` | move | `Shared/Validators/Inventory.Storefront.Validator.cs` |

### Ordering Admin

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Mappings/Order.Mapping.cs` | MERGE | `Orders/Shared/Mappings/Order.Mapping.Domain.cs; Orders/Shared/Mappings/Order.Mapping.Model.cs` |
| `Shared/Models/Order.Model.cs` | MERGE | `Orders/Shared/Models/Order.Model.Action.cs; Orders/Shared/Models/Order.Model.AddressAction.cs; Orders/Shared/Models/Order.Model.Parameters.cs; Orders/Shared/Models/Order.Model.QuantityAction.cs; Orders/Shared/Models/Order.Model.Request.cs; Orders/Shared/Models/Order.Model.Response.cs; Orders/Shared/Models/Order.Model.ShippingMethodAction.cs` |
| `Shared/Models/OrderingDashboard.Model.cs` | move | `Dashboard/Get/Shared/Models/OrderingDashboard.Model.Parameters.cs` |
| `Shared/Validators/Order.Validator.cs` | move | `Orders/Shared/Validators/Order.Validator.cs` |

### Ordering Storefront

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Mappings/Storefront.Cart.Mapping.cs` | MERGE | `Cart/Shared/Mappings/Cart.Mapping.Domain.cs; Cart/Shared/Mappings/Cart.Mapping.Model.cs` |
| `Shared/Mappings/Storefront.Order.Mapping.cs` | move | `Orders/Shared/Mappings/OrderStore.Mapping.cs` |
| `Shared/Models/Storefront.Cart.Model.cs` | MERGE | `Cart/Shared/Models/Cart.Model.Parameters.cs; Cart/Shared/Models/Cart.Model.Request.cs; Cart/Shared/Models/Cart.Model.Response.Base.cs; Cart/Shared/Models/Cart.Model.Response.cs` |
| `Shared/Models/Storefront.Order.Model.cs` | move | `Orders/Shared/Models/Order.Model.Response.cs` |
| `Shared/Models/Storefront.OrderTracking.Model.cs` | move | `Orders/GetTracking/Shared/Models/OrderTracking.Model.Parameters.cs` |
| `Shared/Validators/Storefront.Cart.Validator.cs` | move | `Cart/Shared/Validators/Cart.Validator.cs` |

### Shipping Admin

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Mappings/Shipment.Mapping.cs` | move | `Shared/Mappings/Shipment.Mapping.cs` |
| `Shared/Mappings/ShippingMethod.Mapping.cs` | move | `Shared/Mappings/ShippingMethod.Mapping.cs` |
| `Shared/Mappings/ShippingRate.Mapping.cs` | move | `Shared/Mappings/ShippingRate.Mapping.cs` |
| `Shared/Models/Shipment.Model.cs` | move | `Shared/Models/Shipment.Model.cs` |
| `Shared/Models/ShippingMethod.Model.cs` | MERGE | `Shared/Models/ShippingMethod.Model.cs; ShippingMethods/Shared/Models/ShippingMethod.Model.Parameters.cs; ShippingMethods/Shared/Models/ShippingMethod.Model.Request.cs; ShippingMethods/Shared/Models/ShippingMethod.Model.Response.cs` |
| `Shared/Models/ShippingRate.Model.cs` | move | `Shared/Models/ShippingRate.Model.cs` |
| `Shared/Validators/Shipment.Validator.cs` | move | `Shared/Validators/Shipment.Validator.cs` |
| `Shared/Validators/ShippingMethod.Validator.cs` | MERGE | `Shared/Validators/ShippingMethod.Validator.cs; ShippingMethods/Shared/Validators/ShippingMethod.Validator.cs` |
| `Shared/Validators/ShippingRate.Validator.cs` | move | `Shared/Validators/ShippingRate.Validator.cs` |

### Shipping Storefront

| Target | Op | Sources |
|------|-------------|-----------|
| `Shared/Mappings/Storefront.CalculateShipping.Mapping.cs` | move | `Shared/Mappings/CalculateShipping.Mapping.cs` |
| `Shared/Mappings/Storefront.ShippingRate.Mapping.cs` | move | `Shared/Mappings/ShippingRate.Mapping.cs` |
| `Shared/Models/Storefront.ShippingCalculation.Model.cs` | move | `Shared/Models/Storefront.ShippingCalculation.Model.cs` |
| `Shared/Models/Storefront.ShippingMethod.Model.cs` | move | `Shared/Models/Storefront.ShippingMethod.Model.cs` |
| `Shared/Models/Storefront.ShippingRate.Model.cs` | move | `Shared/Models/Storefront.ShippingRate.Model.cs` |

## Status

- 2026-08-16: Plan executed to completion. Status `Completed`.
  - TASK-001 baseline GREEN: build 0 warnings/0 errors; Module.UnitTests 2713 passed / 0 failed;
    both convention scripts exit 0. (Baseline was repaired first — 4 Shipping errors + subsequent
    namespace drift + 2 cross-module Shipment navigations were in the human's in-flight work;
    `ShipmentState.Partial` restored and `RecordOrderShipmentState` handler reordered to make the
    tree compile.)
  - Migrations applied (all 8 modules consolidated per Appendix B targets): Billing 13, Catalog 55,
    Customer 3, Dashboard 1, Identity 18, Inventory 20, Ordering 10, Shipping 15 (pre-consolidated
    in-flight, verified conformant). Namespace rewrites: Billing 38, Catalog 242, Identity 78,
    Inventory 69, Ordering 71, Dashboard 2, Customer 16.
  - Final verification (TASK-024) green: build 0/0; Module.UnitTests 2713/0/1 pre-existing skip;
    Shared.UnitTests 2444/0; AC-001..006 all PASS; 7-module idempotency all "No changes.".
  - Drift audit (TASK-025) clean: 0 per-feature Shared kind dirs; 0 legacy Shared-kind namespaces.
  - Final whole-branch review: MERGE-READY; all deferred/parked findings non-blocking.
  - Follow-ups (not part of this plan): update `EXPECTED_BASELINE` 35->32 in
    `scripts/check-cross-module-refs.sh`; bump SSH.NET/Testcontainers to clear Api.Tests NU1903;
    separate plan for 148 pre-existing Identity feature-action namespace drifts.
