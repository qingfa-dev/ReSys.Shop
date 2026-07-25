---
goal: Implement missing API methods (restore, tree, reposition, activate, discontinue) and Dashboard integration
version: 1.0
date_created: 2026-07-25
status: Planned
tags: feature, api, catalog, admin
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Add 6 missing methods to existing API files (TaxonomyApi.restore, TaxonApi.getTree/restore/reposition, ProductApi.activate/discontinue) and replace static DashboardPage.vue data with live API call. These are surgical additions — no new entity groups, just missing endpoints on already-connected entities.

## 1. Requirements & Constraints

- **REQ-001**: Each new method follows the existing API class conventions (static, async, wraps apiClient)
- **REQ-002**: Response types added to existing `types/{entity}.response.ts` files (not new files)
- **REQ-003**: Taxonomy restore returns `Result<void>` — no response body
- **REQ-004**: Taxon tree returns rich response with nested `TaxonTreeItem[]` (children, isExpanded, isInActivePath)
- **REQ-005**: Taxon restore returns `Result<void>`
- **REQ-006**: Taxon reposition accepts same request body as Taxon update, returns `Result<{ id: string }>`
- **REQ-007**: Product activate/discontinue return `Result<ProductResponse>` (same response as Product get)
- **REQ-008**: Dashboard API returns `Result<CatalogDashboardResponse>` with 7 summary fields
- **CON-001**: Do not modify existing method signatures — only add new methods
- **CON-002**: Zero TypeScript errors
- **PAT-001**: Activate/discontinue are PATCH (not PUT) with no request body

## 2. Implementation Steps

### Phase 1: Missing methods on existing API files

- GOAL-001: Add restore (Taxonomy), getTree/restore/reposition (Taxon), activate/discontinue (Product) to existing api files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Update `api/taxonomy.api.ts` — add `TaxonomyApi.restore(id: string): Promise<Result<void>>` — `PATCH /catalog/taxonomies/${id}/restore` | | |
| TASK-002 | Update `types/taxon.response.ts` — add `TaxonTreeItem`: extends existing TaxonResponse fields + isExpanded: boolean, isInActivePath: boolean, children: TaxonTreeItem[]; add `TaxonTreeResponse`: tree: TaxonTreeItem[], breadcrumbs: TaxonTreeItem[], focusedNode?: TaxonTreeItem, focusedSubtree?: TaxonTreeItem | | |
| TASK-003 | Update `types/taxon.response.ts` — ensure `TaxonResponse` includes all backend fields: parentId, taxonomyId, name, presentation, description, position, slug, depth, lft, rgt, childrenCount, hideFromNav, automatic, rulesMatchPolicy, sortOrder, permalink, prettyName, parentName?, taxonomyName?, taxonRuleCount?, productCount?, metaTitle?, metaDescription?, metaKeywords?, imageUrl?, squareImageUrl?, createdAt, updatedAt | | |
| TASK-004 | Update `api/taxon.api.ts` — add `TaxonApi.getTree(taxonomyId: string, query?): Promise<Result<TaxonTreeResponse>>` — `GET /catalog/taxonomies/${taxonomyId}/taxons/tree` (may accept optional focus taxonId query param) | | |
| TASK-005 | Update `api/taxon.api.ts` — add `TaxonApi.restore(taxonomyId: string, id: string): Promise<Result<void>>` — `PATCH /catalog/taxonomies/${taxonomyId}/taxons/${id}/restore` | | |
| TASK-006 | Update `api/taxon.api.ts` — add `TaxonApi.reposition(taxonomyId: string, id: string, data: TaxonRequest): Promise<Result<{ id: string }>>` — `POST /catalog/taxonomies/${taxonomyId}/taxons/${id}/reposition` | | |
| TASK-007 | Update `api/product.api.ts` — add `ProductApi.activate(id: string): Promise<Result<ProductResponse>>` — `PATCH /catalog/products/${id}/activate` | | |
| TASK-008 | Update `api/product.api.ts` — add `ProductApi.discontinue(id: string): Promise<Result<ProductResponse>>` — `PATCH /catalog/products/${id}/discontinue` | | |
| TASK-009 | Update `composables/useTaxonomy.ts` — add `taxonTreeApi` exposing getTree from TaxonApi | | |
| TASK-010 | Verify: `pnpm build` passes | | |

### Phase 2: Catalog Dashboard API integration

- GOAL-002: Replace static hardcoded data on DashboardPage.vue with live API call

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Create `types/dashboard.response.ts` — `CatalogDashboardResponse`: totalProducts: number, activeProducts: number, draftProducts: number, totalVariants: number, totalTaxonomies: number, totalTaxons: number, recentProducts: RecentProductData[]; `RecentProductData`: id: string, name: string, slug: string, createdAtUtc: string | | |
| TASK-012 | Create `api/dashboard.api.ts` — `CatalogDashboardApi` with: `get(): Promise<Result<CatalogDashboardResponse>>` — `GET /catalog/dashboard` | | |
| TASK-013 | Update `pages/DashboardPage.vue` — replace hardcoded static data with reactive state loaded via `CatalogDashboardApi.get()` on mount; use LoadingSkeleton while loading; show ErrorState on failure | | |
| TASK-014 | Update `api/index.ts` — add export for `CatalogDashboardApi` | | |
| TASK-015 | Update `types/index.ts` — add export for dashboard response types | | |
| TASK-016 | Verify: `pnpm build` passes | | |

## 3. Alternatives

- **ALT-001**: Create dedicated Dashboard store — rejected: too simple for a store (single GET, no query/pagination)

## 4. Dependencies

- **DEP-001**: Existing `taxonomy.api.ts`, `taxon.api.ts`, `product.api.ts` — modification targets
- **DEP-002**: Existing `TaxonResponse` type — needs extension with tree-related fields
- **DEP-003**: Existing `DashboardPage.vue` — static data replacement target

## 5. Files

- **FILE-001**: `api/taxonomy.api.ts` (updated with restore)
- **FILE-002**: `api/taxon.api.ts` (updated with getTree, restore, reposition)
- **FILE-003**: `api/product.api.ts` (updated with activate, discontinue)
- **FILE-004**: `types/taxon.response.ts` (updated with TaxonTreeItem, TaxonTreeResponse, extended fields)
- **FILE-005**: `composables/useTaxonomy.ts` (updated)
- **FILE-006**: `types/dashboard.response.ts` (new)
- **FILE-007**: `api/dashboard.api.ts` (new)
- **FILE-008**: `pages/DashboardPage.vue` (updated)
- **FILE-009**: `api/index.ts`, `types/index.ts` (updated barrels)

## 6. Testing

- **TEST-001**: `api/__tests__/taxonomies.spec.ts` — add test for restore
- **TEST-002**: `api/__tests__/taxons.spec.ts` — add tests for getTree, restore, reposition
- **TEST-003**: `api/__tests__/products.spec.ts` — add tests for activate, discontinue
- **TEST-004**: `api/__tests__/dashboard.spec.ts` — new test for DashboardApi.get

## 7. Risks & Assumptions

- **RISK-001**: Activate/Discontinue may change product status indirectly — ensure UI refreshes product data after calling these
- **RISK-002**: DashboardPage.vue current implementation uses hardcoded data — verify no other components depend on the static data shape before replacing
- **ASSUMPTION-001**: All new endpoints are already deployed and functional in the backend

## 8. Related Specifications / Further Reading

Backend restore endpoints: `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Restore/`, `.../Taxons/Restore/`
Backend tree: `.../Taxons/Get/Tree/`
Backend reposition: `.../Taxons/Reposition/`
Backend activate/discontinue: `.../Products/Activate/`, `.../Products/Discontinue/`
Backend dashboard: `.../Admin/Dashboard/Get/`
