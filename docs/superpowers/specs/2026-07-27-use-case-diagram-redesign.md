# Design: Correct Use Case Diagrams with Standard UML Format

**Date:** 2026-07-27
**Status:** Approved
**Scope:** Redesign all use case diagrams in thesis `figures/chapters/part2/ch2-design/02-use-cases/diagrams/`

## Problem

The current use case diagram (1 file, `P2S2.2.2_use-case-overview.puml`) uses
`skinparam` styling with oversized fonts (32-40pt), flat package-per-module
grouping, and actor-to-package associations. It does not follow standard UML use
case conventions: no topic-level grouping, no scenario-level sub-use-case
decomposition, no `<<include>>`/`<<extend>>` relationships.

## Design

### Two-Level Diagram Hierarchy

**Level 1 — Overview** (`P2S2.2.2_use-case-overview.puml`):
- 3 top-level rectangles: Administration, Storefront, System Services
- Each rectangle contains *topic rectangles* (one per level-4 `.typ` heading)
- Each topic rectangle contains its UC ellipse(s) labeled with UC-ID
- Actor → area associations (not actor → individual topic — keeps connectors manageable: Admin→Administration, Customer→Storefront, System→System Services)

**Level 2 — Per-Topic Detail** (25 `.puml` files, one per topic):
- Rectangle: topic name (from `.typ` heading)
- Main UC ellipse(s) with UC-ID
- Scenario-level sub-use-cases decomposed from the main success scenario
- `<<include>>` relationships from main UC to sub-use-cases
- Supporting actors (e.g., ML Service, Payment Gateway) connected to relevant
  sub-use-cases
- Title: `title <UC-ID>: <Use Case Name>`

### Style System

Shared `<style>` block in `_shared/styles.iuml`, adapted from old `_thesis`
monochrome palette:

- Font: Arial, 14pt base, 16pt for actors/use-cases, 12pt for arrows
- Palette: `#FFFFFF` background, `#000000` borders/text, 1.5pt stroke
- `shadowing false`, `left to right direction` per diagram
- All 25 per-topic files include `!include _shared/styles.iuml`

### Use Case Decomposition

Sub-use-cases derived from each UC's main success scenario at scenario level
(matching old `_thesis` depth). Examples:

| UC-ID | Main Success Scenarios | Sub-Use Cases |
|-------|----------------------|---------------|
| UC-ADM-IMG | Upload Images, Regenerate Embeddings | Upload Images, Regenerate Embeddings |
| UC-ADM-ORD | View Orders, Update Order, Cancel Order | View Orders, Update Order, Cancel Order |
| UC-STR-CHK | Address Selection, Shipping Selection, Payment, Confirmation | Select Address, Choose Shipping, Process Payment, Confirm Order |

### Topic → UC Mapping (25 topics, 26 UCs)

| # | Topic Heading | UC(s) | Actor(s) |
|---|--------------|-------|----------|
| 1 | Product Management | UC-ADM-PROD | Administrator |
| 2 | Variant and Pricing | UC-ADM-VAR | Administrator |
| 3 | Image and Embedding Management | UC-ADM-IMG | Administrator, ML Service |
| 4 | Taxonomy and Classification | UC-ADM-TAX | Administrator |
| 5 | Option Type Configuration | UC-ADM-OPT | Administrator |
| 6 | Order Lifecycle | UC-ADM-ORD, UC-ADM-ORD-ITEMS | Administrator, Payment Gateway |
| 7 | Payment Processing (Admin) | UC-ADM-PAY | Administrator, Payment Gateway |
| 8 | Payment Method Configuration | UC-ADM-PAY-METHOD | Administrator |
| 9 | Stock Item Management | UC-ADM-STK | Administrator |
| 10 | Stock Location Management | UC-ADM-LOC | Administrator |
| 11 | User Management | UC-ADM-USR | Administrator |
| 12 | Role and Permission Governance | UC-ADM-ROL | Administrator |
| 13 | Shipping Method Configuration | UC-ADM-SHP | Administrator |
| 14 | Reference Data Management | UC-ADM-REF | Administrator |
| 15 | Catalog Browsing | UC-STR-BRW | Customer |
| 16 | Search | UC-STR-SRC | Customer, ML Service |
| 17 | Cart Management | UC-STR-CRT | Customer |
| 18 | Checkout Flow | UC-STR-CHK | Customer, Payment Gateway |
| 19 | Order History | UC-STR-OHI | Customer |
| 20 | Payment Processing (Storefront) | UC-STR-PAY | Customer, Payment Gateway |
| 21 | Authentication | UC-STR-AUT | Customer |
| 22 | Session Management | UC-STR-SES | Customer |
| 23 | Profile and Preferences | UC-STR-PRF | Customer |
| 24 | Embedding Operations | UC-SYS-EMB | System, ML Service |
| 25 | Background Maintenance | UC-SYS-MNT | System |

## Files

### New (26 files)

```
figures/chapters/part2/ch2-design/02-use-cases/diagrams/
├── _shared/
│   └── styles.iuml                              ← shared <style> definitions
├── P2S2.2.2_use-case-overview.puml               ← REPLACE existing
├── P2S2.2.2_usecase-product-management.puml
├── P2S2.2.2_usecase-variant-pricing.puml
├── P2S2.2.2_usecase-image-embedding.puml
├── P2S2.2.2_usecase-taxonomy-classification.puml
├── P2S2.2.2_usecase-option-type-config.puml
├── P2S2.2.2_usecase-order-lifecycle.puml
├── P2S2.2.2_usecase-admin-payment-processing.puml
├── P2S2.2.2_usecase-payment-method-config.puml
├── P2S2.2.2_usecase-stock-item-management.puml
├── P2S2.2.2_usecase-stock-location-management.puml
├── P2S2.2.2_usecase-user-management.puml
├── P2S2.2.2_usecase-role-permission.puml
├── P2S2.2.2_usecase-shipping-method.puml
├── P2S2.2.2_usecase-reference-data.puml
├── P2S2.2.2_usecase-catalog-browsing.puml
├── P2S2.2.2_usecase-search.puml
├── P2S2.2.2_usecase-cart-management.puml
├── P2S2.2.2_usecase-checkout-flow.puml
├── P2S2.2.2_usecase-order-history.puml
├── P2S2.2.2_usecase-stf-payment-processing.puml
├── P2S2.2.2_usecase-authentication.puml
├── P2S2.2.2_usecase-session-management.puml
├── P2S2.2.2_usecase-profile-preferences.puml
├── P2S2.2.2_usecase-embedding-operations.puml
└── P2S2.2.2_usecase-background-maintenance.puml
```

### Modified (25 `.typ` files)

Each topic's `.typ` file gets an `#figure(image(...))` reference placed after
the UC specification table, before the next topic heading. Example:

```typst
#figure(
  image(
    "../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-image-embedding.png",
    width: 100%
  ),
  caption: [Use case diagram for Image and Embedding Management (UC-ADM-IMG).],
) <fig-uc-adm-img>
```

### Existing file to replace

`P2S2.2.2_use-case-overview.puml` — overwritten with new `<style>`-based version.

### Not modified

- Makefile (generic `.puml` pattern rule handles new files)
- `P2S2.2.2_use-case-overview.png` reference in `03-use-case-overview.typ` (image path unchanged)

## Implementation Plan

### Phase 1: Shared style

1. Create `_shared/styles.iuml` with `<style>` block (Arial, monochrome, 14pt)
2. Commit

### Phase 2: Overview diagram

3. Write new `P2S2.2.2_use-case-overview.puml` — 3 areas, 25 topic rectangles,
   actor→area associations
4. Build + verify PNG renders, dimensions readable, commit

### Phase 3: Per-topic diagrams (25 files)

5. Write all 25 per-topic `.puml` files. Each:
   - `!include _shared/styles.iuml`
   - `title <UC-ID>: <Use Case Name>`
   - Rectangle with topic name, UC ellipse(s), sub-use cases, `<<include>>` links
   - Actor connections
6. `make plantuml` — render all 25
7. Verify all 25 PNGs, commit

### Phase 4: Update `.typ` references

8. Insert `#figure(image(...))` references in 25 `.typ` files (one per topic)
9. Update 2 existing `.typ` files that already reference the overview (path unchanged — only the image content changes)

### Phase 5: Verify

10. `typst compile main.typ` — zero errors
11. Final commit

## Verification

```bash
make plantuml              # 26 PUML → 26 PNG (1 overview + 25 per-topic)
typst compile main.typ     # zero errors
```

## What Does NOT Change

- `.typ` use case specification tables (content only — not structure)
- Naming convention (`P2S2.2.2_*`)
- Makefile
- Diagram location (`02-use-cases/diagrams/`)
- Existing `P2S2.2.2_functional-decomposition.*` and `P2S2.2.2_cbir-search-sequence.*` (unchanged)
