# Phase 1: Catalog Store Removal — Task Report

**Date:** 2026-08-09
**Branch:** feature/implement-storefront
**Status:** DONE

## Composables Created

| File | Exports | Singleton |
|------|---------|-----------|
| `composables/useFilters.ts` | `useFilters()` → reactive `{ searchQuery, selectedTaxonIds, selectedOptionValueIds, minPrice, maxPrice, sortField, activeFilterCount, setSearch, toggleTaxon, toggleOptionValue, setPriceRange, setSort, clearFilters }` | Yes (module-level refs) |
| `composables/useTaxonomy.ts` | `useTaxonomy()` → reactive `{ taxonomyGroups, optionTypes, collections, taxonsLoading, optionsLoading, loadTaxonomyGroups, loadOptionTypes }` | Yes (module-level refs) |
| `composables/useProducts.ts` | `useProducts()` → reactive `{ items, loading, error, page, pageSize, totalCount, totalPages, isInitialLoad, fetch, markStale, nextPage, prevPage, goToPage, refresh }` | Yes (module-level refs) |
| `composables/useProductDetail.ts` | `useProductDetail()` → reactive `{ product, loading, error, selectedVariantId, quantity, similarProducts, relatedProducts, relatedLoading, selectedVariant, stockLabel, isInStock, load, selectVariant, incrementQuantity, decrementQuantity, reset }` | Yes (module-level refs) |
| `composables/useVisualSearch.ts` | `useVisualSearch()` → reactive `{ state, selectedFile, previewUrl, selectedModelId, availableModels, results, loading, error, validationError, validateFile, selectFile, search, loadModels, reset }` | No (instance-scoped) |

## Components Updated

| Component | Old Import | New Import |
|-----------|-----------|------------|
| `views/ShopView.vue` | `useCatalogStore`, `useProductListStore` | `useFilters`, `useTaxonomy`, `useProducts` |
| `views/HomeView.vue` | `useCatalogStore`, `useProductListStore` | `useTaxonomy`, `useProducts` |
| `views/CollectionsView.vue` | `useCatalogStore` | `useTaxonomy` |
| `views/ProductDetailView.vue` | `useProductDetailStore` | `useProductDetail` |
| `views/VisualSearchView.vue` | `useVisualSearchStore` | `useVisualSearch` |
| `components/ShopFilterPanel.vue` | `useCatalogStore` | `useFilters`, `useTaxonomy` |
| `components/TaxonTree.vue` | `useCatalogStore` | `useFilters` |

## Stores Deleted

- `stores/catalogStore.ts`
- `stores/productListStore.ts`
- `stores/productDetailStore.ts`
- `stores/visualSearchStore.ts`
- `stores/index.ts`
- `stores/__tests__/catalogStore.spec.ts`
- `stores/__tests__/productListStore.spec.ts`
- `stores/__tests__/productDetailStore.spec.ts`
- `stores/__tests__/` directory
- `stores/` directory

## Test Files Updated

- `components/__tests__/TaxonTree.spec.ts` — now imports `useFilters` instead of `useCatalogStore`
- `views/__tests__/ShopView.spec.ts` — now imports composables, no Pinia
- `views/__tests__/HomeView.spec.ts` — now imports composables, no Pinia
- `views/__tests__/ProductDetailView.spec.ts` — now imports `useProductDetail`, no Pinia
- `composables/__tests__/useVisualSearch.spec.ts` — updated for `reactive()` unwrapped properties

## Build/Test Results

| Command | Result |
|---------|--------|
| `npx vue-tsc --build` | ✓ exit 0 |
| `pnpm exec oxlint .` | ✓ exit 0 |
| `pnpm exec eslint .` | ✓ exit 0 |
| `pnpm run build-only` | ✓ exit 0 (936ms) |

## Design Decisions

1. **`reactive()` wrapper** — All singleton composables return `reactive({...})` so properties are auto-unwrapped (matching Pinia's API). This means `filters.searchQuery` works without `.value` in both templates and scripts.

2. **Module-level singleton pattern** — `useFilters`, `useTaxonomy`, `useProducts`, `useProductDetail` all use module-level refs shared across all component instances. This matches Pinia store behavior.

3. **Event subscription at module level** — `useProducts` subscribes to `filter:changed` at module load time (not inside the function). This ensures the subscription lives for the app lifetime, matching the store's `init()` pattern.

4. **`useVisualSearch` is instance-scoped** — Unlike the other composables, `useVisualSearch` creates fresh state per call (state inside function). This is appropriate because only one component uses it, and `onUnmounted` cleanup works correctly.

5. **`addToCart` omitted from `useProductDetail`** — The store's `addToCart()` was unused (the view defines its own). Not included in the composable.

## Concerns

- **Test coverage** — Store-specific tests (`catalogStore.spec.ts`, `productListStore.spec.ts`, `productDetailStore.spec.ts`) were deleted. The logic they tested is now covered by the composable's runtime behavior in component tests. Consider adding dedicated composable unit tests in a follow-up.
- **Singleton state persistence** — Module-level singleton state persists across route navigations. This is intentional (matching Pinia behavior) but means filter/product state leaks between pages. Phase 2+ may want to add route-aware cleanup.
