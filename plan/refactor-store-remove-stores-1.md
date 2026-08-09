---
goal: Remove all Pinia stores (except auth) from the Store SPA and have components call API services directly via composables with local state
version: 1.0
date_created: 2026-08-09
last_updated: 2026-08-09
owner: ng
status: 'Planned'
tags: ['refactor', 'architecture', 'stores', 'api', 'store-spa']
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Store SPA has **13 Pinia stores** (excluding auth) that act as thin wrappers
around API services. Each store duplicates state management that components can
handle locally. This plan removes all 13 stores and replaces them with **composables**
that wrap API calls with local reactive state. Components call composables directly,
eliminating the store indirection layer. The auth store remains as the single source
of truth for authentication state.

## 1. Requirements & Constraints

- **REQ-001**: Remove all 13 non-auth Pinia stores: `catalogStore`, `productListStore`,
  `productDetailStore`, `visualSearchStore`, `cartStore`, `checkoutStore`, `orderStore`,
  `profileStore`, `addressStore`, `wishlistStore`, `locationStore`, `availabilityStore`,
  `shippingStore`.
- **REQ-002**: For each removed store, create a composable in the same feature directory
  (`features/{domain}/composables/`) that provides the same reactive state and methods
  via `ref`/`computed`/functions.
- **REQ-003**: Each composable calls the existing API service directly (e.g., `ProductApi.getProducts()`)
  and manages loading/error state locally. No Pinia dependency.
- **REQ-004**: The auth store (`features/identity/stores/authStore.ts`) is EXCLUDED — it
  remains as-is. Components that need auth state continue to import `useAuthStore`.
- **REQ-005**: The event bus (`shared/composables/useStoreEvents.ts`) remains for
  cross-component communication. Composables subscribe to events (e.g., `auth:login`,
  `auth:logout`) instead of stores.
- **REQ-006**: Shared state between components on the same page (e.g., filter state
  shared between ShopFilterPanel and ShopView) must be lifted to the parent component
  via props/emits or a shared composable instance created in the parent.
- **CON-001**: No changes to API service files (`features/*/services/`) — they are
  already correct.
- **CON-002**: No changes to the shared API client (`shared/api/`) — it is already correct.
- **CON-003**: Warnings-as-errors globally; `vue-tsc`, `oxlint`, `eslint`, `vitest`,
  `build-only` must all pass.
- **CON-004**: The event bus subscriptions must be cleaned up (unsubscribed) when
  composables are torn down (`onUnmounted`).
- **GUD-001**: Composables follow Vue 3 Composition API conventions: return reactive
  refs, not raw values. Use `readonly()` where external mutation should be prevented.
- **GUD-002**: Composable naming: `use{Domain}()` (e.g., `useProducts`, `useCart`,
  `useOrders`). File naming: `use{Domain}.ts`.
- **PAT-001**: Each composable follows this pattern:
  ```ts
  export function useProducts() {
    const items = ref<T[]>([])
    const loading = ref(false)
    const error = ref<string | null>(null)
    
    async function fetch(): Promise<void> { ... }
    
    return { items: readonly(items), loading: readonly(loading), error: readonly(error), fetch }
  }
  ```

## 2. Implementation Steps

### Phase 1 — Catalog Composables (Shop, Home, Collections, Filter)

- GOAL-001: Replace `catalogStore`, `productListStore`, `productDetailStore`,
  `visualSearchStore` with composables. Update all catalog components.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `app/Store/src/features/catalog/composables/useFilters.ts`. Extract filter state from `catalogStore`: `searchQuery`, `selectedTaxonIds`, `selectedOptionValueIds`, `minPrice`, `maxPrice`, `sortField`. Provide `setSearch()`, `toggleTaxon()`, `toggleOptionValue()`, `setPriceRange()`, `setSort()`, `clearFilters()`. This composable is stateful — the parent component (ShopView) creates one instance and passes it down via provide/inject or props. | | |
| TASK-002 | Create `app/Store/src/features/catalog/composables/useTaxonomy.ts`. Extract taxonomy loading from `catalogStore`: `taxonomyGroups`, `optionTypes`, `taxonLoading`, `optionLoading`. Provide `loadTaxonomyGroups()`, `loadOptionTypes()`. Calls `TaxonApi.getTaxonomies()`, `TaxonApi.getTaxons()`, `OptionTypeApi.getOptionTypes()`. Subscribe to `auth:init-done` event to reload if needed. | | |
| TASK-003 | Create `app/Store/src/features/catalog/composables/useProducts.ts`. Extract product list state from `productListStore`: `items`, `loading`, `error`, `page`, `pageSize`, `totalCount`, `isInitialLoad`. Provide `fetch(filters)`, `markStale()`, `goToPage()`, `refresh()`. Takes filters as a reactive parameter (from `useFilters`). Calls `ProductApi.getProducts()`. Subscribe to `filter:changed` event. | | |
| TASK-004 | Create `app/Store/src/features/catalog/composables/useProductDetail.ts`. Extract from `productDetailStore`: `product`, `loading`, `error`, `selectedVariantId`, `quantity`, `similarProducts`, `relatedProducts`. Provide `load(slug)`, `selectVariant()`, `incrementQuantity()`, `decrementQuantity()`, `reset()`. Calls `ProductApi.getProductBySlug()`, `ProductApi.getSimilar()`, `ProductApi.getRelated()`. | | |
| TASK-005 | Create `app/Store/src/features/catalog/composables/useVisualSearch.ts`. Extract from `visualSearchStore`: `selectedFile`, `previewUrl`, `results`, `loading`, `error`, `availableModels`. Provide `validateFile()`, `selectFile()`, `search()`, `loadModels()`, `reset()`. Calls `SearchByImageApi.searchByImage()`, `SearchByImageApi.getVisualSearchModels()`. | | |
| TASK-006 | Update `app/Store/src/features/catalog/views/ShopView.vue`: replace `useCatalogStore` + `useProductListStore` with `useFilters()` + `useTaxonomy()` + `useProducts(filters)`. Create the filter instance in ShopView and pass it down to ShopFilterPanel via provide/inject or props. | | |
| TASK-007 | Update `app/Store/src/features/catalog/views/HomeView.vue`: replace `useCatalogStore` + `useProductListStore` with `useTaxonomy()` + `useProducts()`. | | |
| TASK-008 | Update `app/Store/src/features/catalog/views/CollectionsView.vue`: replace `useCatalogStore` with `useTaxonomy()`. | | |
| TASK-009 | Update `app/Store/src/features/catalog/views/ProductDetailView.vue`: replace `useProductDetailStore` + `useAvailabilityStore` with `useProductDetail()` + `useAvailability()`. | | |
| TASK-010 | Update `app/Store/src/features/catalog/views/VisualSearchView.vue`: replace `useVisualSearchStore` with `useVisualSearch()`. | | |
| TASK-011 | Update `app/Store/src/features/catalog/components/ShopFilterPanel.vue`: replace `useCatalogStore` with inject or props for filter state + taxonomy data. | | |
| TASK-012 | Update `app/Store/src/features/catalog/components/TaxonTree.vue`: replace `useCatalogStore` with inject or props for selected taxon IDs + toggle function. | | |
| TASK-013 | Delete `app/Store/src/features/catalog/stores/catalogStore.ts`, `productListStore.ts`, `productDetailStore.ts`, `visualSearchStore.ts`. Delete `app/Store/src/features/catalog/stores/` directory if empty. | | |

### Phase 2 — Ordering Composables (Cart, Checkout, Orders)

- GOAL-002: Replace `cartStore`, `checkoutStore`, `orderStore` with composables.
  Update all ordering components.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Create `app/Store/src/features/ordering/composables/useCart.ts`. Extract from `cartStore`: `id`, `items`, `loading`, `error`, `itemCount` (computed). Provide `fetchCart()`, `addItem()`, `updateQuantity()`, `removeItem()`, `clearCart()`, `reset()`. Subscribe to `auth:login` and `auth:logout` events. Emit `cart:updated` on changes. Calls `CartApi.*`. | | |
| TASK-015 | Create `app/Store/src/features/ordering/composables/useCheckout.ts`. Extract from `checkoutStore`: `currentStep`, `shipAddressId`, `shippingMethodId`, `paymentMethodId`, `orderId`, `loading`, `error`. Provide `init()`, `saveAddress()`, `selectShippingRate()`, `createPaymentIntent()`, `placeOrder()`, `reset()`. Emit `checkout:placed` on success. Takes cart as parameter (to access cart.id). Calls `CheckoutApi.*`. | | |
| TASK-016 | Create `app/Store/src/features/ordering/composables/useOrders.ts`. Extract from `orderStore`: `items`, `loading`, `error`, `page`, `totalCount`, `currentOrder`, `detailLoading`. Provide `fetchOrders()`, `fetchOrder()`, `cancelOrder()`, `goToPage()`. Subscribe to `checkout:placed` event. Calls `OrderApi.*`. | | |
| TASK-017 | Update `app/Store/src/features/ordering/views/CartView.vue`: replace `useCartStore` with `useCart()`. | | |
| TASK-018 | Update `app/Store/src/features/ordering/views/CheckoutView.vue`: replace `useCheckoutStore` + `useCartStore` + `useAddressStore` + `useShippingStore` + `useLocationStore` with `useCheckout(cart)` + `useAddresses()` + `useShipping()` + `useLocation()`. | | |
| TASK-019 | Update `app/Store/src/features/ordering/views/OrderListView.vue`: replace `useOrderStore` with `useOrders()`. | | |
| TASK-020 | Update `app/Store/src/features/ordering/views/OrderDetailView.vue`: replace `useOrderStore` + `useAddressStore` with `useOrders()` + `useAddresses()`. | | |
| TASK-021 | Update `app/Store/src/features/ordering/components/CartDrawer.vue`: replace `useCartStore` with `useCart()`. | | |
| TASK-022 | Update `app/Store/src/app/components/layout/AppHeader.vue`: replace `useCartStore` with `useCart()` (for badge count). | | |
| TASK-023 | Update `app/Store/src/features/ordering/composables/useQuickAdd.ts`: replace `useCartStore` with `useCart()`. | | |
| TASK-024 | Delete `app/Store/src/features/ordering/stores/cartStore.ts`, `checkoutStore.ts`, `orderStore.ts`. Delete directory if empty. | | |

### Phase 3 — Profile Composables (Profile, Addresses, Wishlists)

- GOAL-003: Replace `profileStore`, `addressStore`, `wishlistStore` with composables.
  Update all profile components.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | Create `app/Store/src/features/profile/composables/useProfile.ts`. Extract from `profileStore`: `profile`, `loading`, `saving`, `error`. Provide `fetchProfile()`, `updateProfile()`, `deleteProfile()`, `reset()`. Calls `ProfileApi.*`, `AccountApi.*`. | | |
| TASK-026 | Create `app/Store/src/features/profile/composables/useAddresses.ts`. Extract from `addressStore`: `addresses`, `loading`, `saving`, `error`. Provide `fetchAddresses()`, `createAddress()`, `updateAddress()`, `deleteAddress()`. Calls `AddressApi.*`. | | |
| TASK-027 | Create `app/Store/src/features/profile/composables/useWishlists.ts`. Extract from `wishlistStore`: `lists`, `loading`, `saving`, `details`, `wishlistedVariantIds`. Provide `fetchWishlists()`, `createWishlist()`, `addItem()`, `removeItem()`, `reset()`. Subscribe to `auth:init-done` event. Calls `WishlistApi.*`. | | |
| TASK-028 | Update `app/Store/src/features/profile/views/ProfileView.vue`: replace `useProfileStore` with `useProfile()`. | | |
| TASK-029 | Update `app/Store/src/features/profile/views/AddressBookView.vue`: replace `useAddressStore` with `useAddresses()`. | | |
| TASK-030 | Update `app/Store/src/features/profile/views/WishlistsView.vue`: replace `useWishlistStore` with `useWishlists()`. | | |
| TASK-031 | Update `app/Store/src/app/layouts/AccountLayout.vue`: replace `useOrderStore` with `useOrders()` (for active order count badge). | | |
| TASK-032 | Update `app/Store/src/features/catalog/components/ProductCard.vue`: replace `useWishlistStore` with `useWishlists()`. | | |
| TASK-033 | Delete `app/Store/src/features/profile/stores/profileStore.ts`, `addressStore.ts`, `wishlistStore.ts`. Delete directory if empty. | | |

### Phase 4 — Location, Availability, Shipping Composables

- GOAL-004: Replace `locationStore`, `availabilityStore`, `shippingStore` with composables.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-034 | Create `app/Store/src/features/location/composables/useLocation.ts`. Extract from `locationStore`: `countries`, `states`, `selectedCountryId`, `loading`. Provide `loadAll()`, `selectCountry()`. Calls `getCountries()`, `getStates()`. | | |
| TASK-035 | Create `app/Store/src/features/inventory/composables/useAvailability.ts`. Extract from `availabilityStore`: `cache` (Map), `loading`. Provide `check(variantId)`, `checkBatch(variantIds)`. Calls `checkAvailability()`. | | |
| TASK-036 | Create `app/Store/src/features/shipping/composables/useShipping.ts`. Extract from `shippingStore`: `methods`, `rates`, `selectedMethodId`, `loading`, `error`. Provide `fetchMethods()`, `fetchRates()`, `selectMethod()`. Calls `getShippingMethods()`, `getShippingRates()`. | | |
| TASK-037 | Update `app/Store/src/features/catalog/views/ProductDetailView.vue`: replace `useAvailabilityStore` with `useAvailability()`. | | |
| TASK-038 | Update `app/Store/src/features/ordering/views/CheckoutView.vue`: replace `useShippingStore` + `useLocationStore` with `useShipping()` + `useLocation()`. | | |
| TASK-039 | Delete `app/Store/src/features/location/stores/locationStore.ts`, `app/Store/src/features/inventory/stores/availabilityStore.ts`, `app/Store/src/features/shipping/stores/shippingStore.ts`. Delete directories if empty. | | |

### Phase 5 — Cleanup & Verification

- GOAL-005: Remove all store registration, verify no remaining store imports, run full gate battery.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-040 | Run `rg -n 'useCatalogStore\|useProductListStore\|useProductDetailStore\|useVisualSearchStore\|useCartStore\|useCheckoutStore\|useOrderStore\|useProfileStore\|useAddressStore\|useWishlistStore\|useLocationStore\|useAvailabilityStore\|useShippingStore' app/Store/src/ --glob '*.vue' --glob '*.ts'` → zero results (excluding auth). | | |
| TASK-041 | Run `rg -n 'from.*stores/' app/Store/src/ --glob '*.vue' --glob '*.ts'` → only `authStore` imports remain. | | |
| TASK-042 | Run full gate battery from `app/Store/`: `npx vue-tsc --build` (0), `pnpm exec oxlint .` (0), `pnpm exec eslint .` (0), `npx vitest run --test-timeout=60000` (all green), `pnpm run build-only` (0). | | |
| TASK-043 | Verify no remaining `stores/` directories (except `features/identity/stores/`): `find app/Store/src/features -type d -name stores` → only identity. | | |

## 3. Alternatives

- **ALT-001**: Keep stores but thin them out to just re-export composables. Rejected:
  this adds indirection without value. The composables replace the stores entirely.
- **ALT-002**: Use `reactive()` objects instead of individual `ref()`s in composables.
  Rejected: individual refs are more idiomatic Vue 3 and allow partial destructuring
  without losing reactivity.
- **ALT-003**: Keep the event bus but have composables emit events directly. Rejected:
  composables should not know about the event bus — the calling component handles
  event emission. Exception: auth event subscriptions (login/logout) are inherent
  to the composable's lifecycle.
- **ALT-004**: Use Vue's `provide/inject` for shared filter state instead of props.
  Possible fallback if prop drilling becomes excessive in Phase 1.

## 4. Dependencies

- **DEP-001**: All API service files (`features/*/services/`) remain unchanged.
- **DEP-002**: The shared API client (`shared/api/`) remains unchanged.
- **DEP-003**: The event bus (`shared/composables/useStoreEvents.ts`) remains unchanged.
- **DEP-004**: The auth store remains unchanged.
- **DEP-005**: Pinia remains installed (used by auth store) but unused stores are deleted.

## 5. Files

- **FILE-001**: `app/Store/src/features/catalog/composables/useFilters.ts` — new
- **FILE-002**: `app/Store/src/features/catalog/composables/useTaxonomy.ts` — new
- **FILE-003**: `app/Store/src/features/catalog/composables/useProducts.ts` — new
- **FILE-004**: `app/Store/src/features/catalog/composables/useProductDetail.ts` — new
- **FILE-005**: `app/Store/src/features/catalog/composables/useVisualSearch.ts` — new
- **FILE-006**: `app/Store/src/features/catalog/views/ShopView.vue` — update
- **FILE-007**: `app/Store/src/features/catalog/views/HomeView.vue` — update
- **FILE-008**: `app/Store/src/features/catalog/views/CollectionsView.vue` — update
- **FILE-009**: `app/Store/src/features/catalog/views/ProductDetailView.vue` — update
- **FILE-010**: `app/Store/src/features/catalog/views/VisualSearchView.vue` — update
- **FILE-011**: `app/Store/src/features/catalog/components/ShopFilterPanel.vue` — update
- **FILE-012**: `app/Store/src/features/catalog/components/TaxonTree.vue` — update
- **FILE-013**: `app/Store/src/features/catalog/stores/` — delete (4 files)
- **FILE-014**: `app/Store/src/features/ordering/composables/useCart.ts` — new
- **FILE-015**: `app/Store/src/features/ordering/composables/useCheckout.ts` — new
- **FILE-016**: `app/Store/src/features/ordering/composables/useOrders.ts` — new
- **FILE-017**: `app/Store/src/features/ordering/views/CartView.vue` — update
- **FILE-018**: `app/Store/src/features/ordering/views/CheckoutView.vue` — update
- **FILE-019**: `app/Store/src/features/ordering/views/OrderListView.vue` — update
- **FILE-020**: `app/Store/src/features/ordering/views/OrderDetailView.vue` — update
- **FILE-021**: `app/Store/src/features/ordering/components/CartDrawer.vue` — update
- **FILE-022**: `app/Store/src/app/components/layout/AppHeader.vue` — update
- **FILE-023**: `app/Store/src/features/ordering/composables/useQuickAdd.ts` — update
- **FILE-024**: `app/Store/src/features/ordering/stores/` — delete (3 files)
- **FILE-025**: `app/Store/src/features/profile/composables/useProfile.ts` — new
- **FILE-026**: `app/Store/src/features/profile/composables/useAddresses.ts` — new
- **FILE-027**: `app/Store/src/features/profile/composables/useWishlists.ts` — new
- **FILE-028**: `app/Store/src/features/profile/views/ProfileView.vue` — update
- **FILE-029**: `app/Store/src/features/profile/views/AddressBookView.vue` — update
- **FILE-030**: `app/Store/src/features/profile/views/WishlistsView.vue` — update
- **FILE-031**: `app/Store/src/app/layouts/AccountLayout.vue` — update
- **FILE-032**: `app/Store/src/features/catalog/components/ProductCard.vue` — update
- **FILE-033**: `app/Store/src/features/profile/stores/` — delete (3 files)
- **FILE-034**: `app/Store/src/features/location/composables/useLocation.ts` — new
- **FILE-035**: `app/Store/src/features/inventory/composables/useAvailability.ts` — new
- **FILE-036**: `app/Store/src/features/shipping/composables/useShipping.ts` — new
- **FILE-037**: `app/Store/src/features/location/stores/` — delete (1 file)
- **FILE-038**: `app/Store/src/features/inventory/stores/` — delete (1 file)
- **FILE-039**: `app/Store/src/features/shipping/stores/` — delete (1 file)

## 6. Testing

- **TEST-001**: `rg -n 'useCatalogStore|useProductListStore|useProductDetailStore|useVisualSearchStore|useCartStore|useCheckoutStore|useOrderStore|useProfileStore|useAddressStore|useWishlistStore|useLocationStore|useAvailabilityStore|useShippingStore' app/Store/src/` → zero results.
- **TEST-002**: `rg -n 'from.*stores/' app/Store/src/ --glob '*.ts' --glob '*.vue'` → only authStore imports.
- **TEST-003**: `find app/Store/src/features -type d -name stores` → only `features/identity/stores/`.
- **TEST-004**: `npx vue-tsc --build` → exit 0.
- **TEST-005**: `pnpm exec oxlint . && pnpm exec eslint .` → 0 warnings/errors.
- **TEST-006**: `npx vitest run --test-timeout=60000` → all tests pass.
- **TEST-007**: `pnpm run build-only` → exit 0.

## 7. Risks & Assumptions

- **RISK-001**: Shared state between ShopFilterPanel and ShopView requires careful
  lifting. If provide/inject is used, the filter composable instance must be created
  in ShopView and provided to ShopFilterPanel. Mitigation: use props/emits as the
  default pattern; fall back to provide/inject if prop drilling exceeds 2 levels.
- **RISK-002**: Event bus subscriptions in composables must be cleaned up in
  `onUnmounted` to avoid memory leaks. Mitigation: each composable that subscribes
  to events must also unsubscribe in its teardown.
- **RISK-003**: The cart composable is used in multiple components (AppHeader, CartDrawer,
  CartView, CheckoutView, useQuickAdd). Each creates its own instance — they must share
  the same cart state. Mitigation: use a module-level singleton pattern (create the
  refs outside the function, return them from the composable).
- **RISK-004**: The checkout composable depends on cart state (cart.id). The calling
  component must pass the cart composable instance to checkout. Mitigation: checkout
  takes `cart` as a parameter.
- **ASSUMPTION-001**: All API service files are already correct and need no changes.
- **ASSUMPTION-002**: The event bus pattern (`emit`/`on`/`off`) is sufficient for
  cross-component communication without stores.
- **ASSUMPTION-003**: Pinia remains installed for the auth store — we do not remove
  the Pinia dependency.

## 8. Related Specifications / Further Reading

- `app/Store/src/shared/api/client.ts` — the shared HTTP client all API services use.
- `app/Store/src/shared/composables/useStoreEvents.ts` — the event bus.
- `app/Store/src/features/identity/stores/authStore.ts` — the auth store (EXCLUDED).
- `plan/refactor-store-color-tokens-1.md` — the preceding color/token refactor.
