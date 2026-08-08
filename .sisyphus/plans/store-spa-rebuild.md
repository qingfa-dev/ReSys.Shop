# Store SPA Rebuild — Full Task Plan

## 0. Execution Strategy

- **Directive**: Per `sisyphus/constitution.md` §1: "Launch planning subagent BEFORE any code. Read skills first."
- **Agent**: Coding Agent (plan-first, implement-all-then-verify)
- **Execution order**: [init] → 8 implementation batches → [verify] (final)

## 1. Shared Foundation Layer (Phase 1)

### T1.1 — API Path Constants
- **What**: Create `app/Store/src/shared/constants/api.ts` with `STOREFRONT`, `STORE`, `CATALOG`, `IDENTITY`, `PROFILES`, `LOCATIONS`, `ORDERS`, `CART`, `PAYMENT`, `SHIPPING`, `AVAILABILITY` constants
- **Depends**: None
- **Blocks**: All subsequent domain features
- **File**: `app/Store/src/shared/constants/api.ts`
- **Acceptance**: Constants resolve correctly, barrel exports from `index.ts`

### T1.2 — Shared Type Definitions
- **What**: Create `app/Store/src/shared/types/result.ts` (Result<T>, PagedResult<T>), `error.ts` (ErrorType, ApiError, StatusCode), barrel `index.ts`
- **Depends**: None
- **Blocks**: All API services and stores
- **Files**: `app/Store/src/shared/types/result.ts`, `error.ts`, `index.ts`
- **Acceptance**: Types compile, `Result<T>` matches backend DTO shape

### T1.3 — Querying Parameter Types
- **What**: Create `app/Store/src/shared/types/querying/` — 13 files: `querying.ts`, `page.ts`, `filter.ts`, `sort.ts`, `search.ts`, `behaviors.ts`, `constants.ts`, `enums.ts`, `error-codes.ts`, `mappers.ts`, `parsers.ts`, `index.ts`
- **Depends**: T1.2
- **Blocks**: All paged API calls
- **Files**: `app/Store/src/shared/types/querying/*.ts`
- **Acceptance**: QueryingParameters, PagedRequestOptions, URLSearchParams conversion all type-check

### T1.4 — Axios HTTP Client
- **What**: Create `app/Store/src/shared/api/` — `axios.ts`, `client.ts`, `errors.ts`, `paged.ts`, `notify.ts`, `interceptors/refresh.ts`, barrel `index.ts`
- **Depends**: T1.1, T1.2, T1.3
- **Blocks**: All API services
- **Files**: `app/Store/src/shared/api/*.ts`, `interceptors/refresh.ts`
- **Acceptance**: get/post/put/del/getPaged all compile, HttpError exported

### T1.5 — Shared Composables + Validations
- **What**: Create `useTheme.ts`, `useStoreEvents.ts`, `useMediaQuery.ts`, `useDebounce.ts`; Create `shared/validations/result.ts`, `error.ts`, barrel `index.ts`
- **Depends**: None (composables), T1.2 (validations)
- **Blocks**: Store event bus, theme, all Zod validation
- **Files**: `app/Store/src/shared/composables/*.ts`, `shared/validations/*.ts`
- **Acceptance**: Event bus emit/on work, Zod schemas parse mock data

### T1.6 — Shared Constants + Barrel
- **What**: Create `storage.ts` (STORAGE_KEYS), finalize `constants/index.ts` barrel
- **Depends**: None
- **Blocks**: Token storage access
- **Files**: `app/Store/src/shared/constants/storage.ts`
- **Acceptance**: STORAGE_KEYS.ACCESS_TOKEN, STORAGE_KEYS.REFRESH_TOKEN resolve

---

## 2. Catalog Domain (Phase 2)

### T2.1 — Catalog Types
- **What**: Create `features/catalog/types/` — `product.ts`, `taxon.ts`, `taxonTree.ts`, `optionType.ts`, `searchByImage.ts`, `catalogQuery.ts`, barrel `index.ts`
- **Depends**: None
- **Blocks**: T2.2, T2.3, T2.4
- **Files**: `app/Store/src/features/catalog/types/*.ts`
- **Acceptance**: StoreProductListItemResponse, StoreProductDetailResponse, TaxonomyGroup, ProductQuery all defined

### T2.2 — Catalog Zod Schemas
- **What**: Create `features/catalog/validations/` — `product.ts`, `taxon.ts`, `optionType.ts`, `searchByImage.ts`, barrel `index.ts`
- **Depends**: T2.1
- **Blocks**: T2.3 (runtime validation)
- **Files**: `app/Store/src/features/catalog/validations/*.ts`
- **Acceptance**: Schemas parse mock catalog JSON, type-infer correctly

### T2.3 — Catalog API Services
- **What**: Create `features/catalog/services/` — `productApi.ts`, `taxonApi.ts`, `optionTypeApi.ts`, `searchByImageApi.ts`, barrel `index.ts`
- **Depends**: T1.4, T2.1, T2.2
- **Blocks**: T2.4, T2.5
- **Files**: `app/Store/src/features/catalog/services/*.ts`
- **Acceptance**: ProductApi.getProducts, getProductBySlug, getSimilar, getRelated compile with Zod parse

### T2.4 — Catalog Pinia Stores
- **What**: Create `features/catalog/stores/` — `catalogStore.ts`, `productListStore.ts`, `productDetailStore.ts`, `visualSearchStore.ts`, barrel `index.ts`
- **Depends**: T2.1, T2.3
- **Blocks**: T2.6, T2.7
- **Files**: `app/Store/src/features/catalog/stores/*.ts`
- **Acceptance**: All stores have loading/error state, actions return Result<T>, _initialized guard

### T2.5 — Catalog Composables
- **What**: Create `features/catalog/composables/` — `useSearch.ts`, `useVisualSearch.ts`, barrel `index.ts`
- **Depends**: T2.4
- **Blocks**: T2.6
- **Files**: `app/Store/src/features/catalog/composables/*.ts`
- **Acceptance**: useSearch wraps catalogStore, useVisualSearch wraps visualSearchStore

### T2.6 — Catalog Utility + Types Utils
- **What**: Create `features/catalog/utils/taxonTree.ts`
- **Depends**: T2.1
- **Blocks**: T2.4 (tree building)
- **File**: `app/Store/src/features/catalog/utils/taxonTree.ts`
- **Acceptance**: buildTaxonTree function compiles

### T2.7 — Catalog Routes + Skeleton Views
- **What**: Create `features/catalog/routes/index.ts`, `views/index.ts`
- **Depends**: T2.4, T2.5
- **Blocks**: Phase 7 router wiring
- **Files**: `app/Store/src/features/catalog/routes/index.ts`, `views/index.ts`
- **Acceptance**: Route definitions exportable, view stubs importable

### T2.8 — Catalog Domain Barrel
- **What**: Create `features/catalog/index.ts` re-exporting types, services, stores
- **Depends**: T2.1–T2.4
- **Blocks**: None (leaf)
- **File**: `app/Store/src/features/catalog/index.ts`
- **Acceptance**: Barrel re-exports all public exports

---

## 3. Identity Domain (Phase 3)

### T3.1 — Identity Types
- **What**: Create `features/identity/types/` — `auth.ts`, `session.ts`, `password.ts`, barrel `index.ts`
- **Depends**: None
- **Blocks**: T3.2, T3.3
- **Files**: `app/Store/src/features/identity/types/*.ts`
- **Acceptance**: LoginRequest, RegisterRequest, TokenPair, SessionUser, AuthUser all defined

### T3.2 — Identity Zod Schemas
- **What**: Create `features/identity/validations/` — `auth.ts`, barrel `index.ts`
- **Depends**: T3.1
- **Blocks**: T3.3
- **Files**: `app/Store/src/features/identity/validations/*.ts`
- **Acceptance**: TokenPairSchema, SessionUserSchema parse mock auth JSON

### T3.3 — Identity API Services
- **What**: Create `features/identity/services/` — `authApi.ts`, `sessionApi.ts`, `emailApi.ts`, `tokenService.ts`, barrel `index.ts`
- **Depends**: T1.4, T3.1, T3.2
- **Blocks**: T3.4
- **Files**: `app/Store/src/features/identity/services/*.ts`
- **Acceptance**: AuthApi.login, register, logout, getSession compile with Zod parse

### T3.4 — Identity Pinia Store
- **What**: Create `features/identity/stores/authStore.ts`, barrel `index.ts`
- **Depends**: T3.1, T3.3
- **Blocks**: T3.5
- **Files**: `app/Store/src/features/identity/stores/*.ts`
- **Acceptance**: useAuthStore has user, isAuthenticated, login, logout, register actions

### T3.5 — Identity Composables
- **What**: Create `features/identity/composables/useAuth.ts`, barrel `index.ts`
- **Depends**: T3.4
- **Blocks**: T3.6
- **File**: `app/Store/src/features/identity/composables/*.ts`
- **Acceptance**: useAuth wraps authStore

### T3.6 — Identity Routes + Views
- **What**: Create `features/identity/routes/index.ts`, `views/index.ts`
- **Depends**: T3.4, T3.5
- **Blocks**: Phase 7
- **Files**: `app/Store/src/features/identity/routes/index.ts`, `views/index.ts`
- **Acceptance**: Login/Register/ForgotPassword routes defined

### T3.7 — Identity Domain Barrel
- **What**: Create `features/identity/index.ts`
- **Depends**: T3.1–T3.4
- **Blocks**: None
- **File**: `app/Store/src/features/identity/index.ts`
- **Acceptance**: Barrel re-exports all

---

## 4. Ordering Domain (Phase 4)

### T4.1 — Ordering Types
- **What**: Create `features/ordering/types/` — `cart.ts`, `checkout.ts`, `order.ts`, barrel `index.ts`
- **Depends**: None
- **Blocks**: T4.2, T4.3
- **Files**: `app/Store/src/features/ordering/types/*.ts`
- **Acceptance**: Cart, CartLineItem, CheckoutState, Order, OrderListItem all defined

### T4.2 — Ordering Zod Schemas
- **What**: Create `features/ordering/validations/` — `cart.ts`, `checkout.ts`, `order.ts`, barrel `index.ts`
- **Depends**: T4.1
- **Blocks**: T4.3
- **Files**: `app/Store/src/features/ordering/validations/*.ts`
- **Acceptance**: CartSchema, CheckoutStateSchema parse mock data

### T4.3 — Ordering API Services
- **What**: Create `features/ordering/services/` — `cartApi.ts`, `checkoutApi.ts`, `orderApi.ts`, barrel `index.ts`
- **Depends**: T1.4, T4.1, T4.2
- **Blocks**: T4.4
- **Files**: `app/Store/src/features/ordering/services/*.ts`
- **Acceptance**: CartApi.getCart, addItem, removeItem; CheckoutApi.createOrder; OrderApi.getOrders compile

### T4.4 — Ordering Pinia Stores
- **What**: Create `features/ordering/stores/` — `cartStore.ts`, `checkoutStore.ts`, `orderStore.ts`, barrel `index.ts`
- **Depends**: T4.1, T4.3
- **Blocks**: T4.5
- **Files**: `app/Store/src/features/ordering/stores/*.ts`
- **Acceptance**: useCartStore has items, addItem, removeItem; useCheckoutStore has processCheckout

### T4.5 — Ordering Composables
- **What**: Create `features/ordering/composables/useQuickAdd.ts`, barrel `index.ts`
- **Depends**: T4.4
- **Blocks**: T4.6
- **File**: `app/Store/src/features/ordering/composables/*.ts`
- **Acceptance**: useQuickAdd wraps cartStore

### T4.6 — Ordering Routes + Views
- **What**: Create `features/ordering/routes/index.ts`, `views/index.ts`
- **Depends**: T4.4, T4.5
- **Blocks**: Phase 7
- **Files**: `app/Store/src/features/ordering/routes/index.ts`, `views/index.ts`
- **Acceptance**: Cart/Checkout/OrderHistory routes defined

### T4.7 — Ordering Domain Barrel
- **What**: Create `features/ordering/index.ts`
- **Depends**: T4.1–T4.4
- **Blocks**: None
- **File**: `app/Store/src/features/ordering/index.ts`
- **Acceptance**: Barrel re-exports all

---

## 5. Profile Domain (Phase 5)

### T5.1 — Profile Types
- **What**: Create `features/profile/types/` — `profile.ts`, `address.ts`, `wishlist.ts`, `notification.ts`, `preferences.ts`, barrel `index.ts`
- **Depends**: None
- **Blocks**: T5.2, T5.3
- **Files**: `app/Store/src/features/profile/types/*.ts`
- **Acceptance**: ProfileDetail, Address, WishlistListItem, NotificationPreferences all defined

### T5.2 — Profile Zod Schemas
- **What**: Create `features/profile/validations/` — `profile.ts`, `address.ts`, `wishlist.ts`, `notification.ts`, `preferences.ts`, barrel `index.ts`
- **Depends**: T5.1
- **Blocks**: T5.3
- **Files**: `app/Store/src/features/profile/validations/*.ts`
- **Acceptance**: Schemas parse mock profile JSON

### T5.3 — Profile API Services
- **What**: Create `features/profile/services/` — `profileApi.ts`, `addressApi.ts`, `wishlistApi.ts`, `notificationApi.ts`, `accountApi.ts`, barrel `index.ts`
- **Depends**: T1.4, T5.1, T5.2
- **Blocks**: T5.4
- **Files**: `app/Store/src/features/profile/services/*.ts`
- **Acceptance**: ProfileApi.getProfile, updateProfile; AddressApi CRUD compile

### T5.4 — Profile Pinia Stores
- **What**: Create `features/profile/stores/` — `profileStore.ts`, `addressStore.ts`, `wishlistStore.ts`, barrel `index.ts`
- **Depends**: T5.1, T5.3
- **Blocks**: T5.5
- **Files**: `app/Store/src/features/profile/stores/*.ts`
- **Acceptance**: useProfileStore has profile, updateProfile; useAddressStore has addresses CRUD

### T5.5 — Profile Composables
- **What**: Create `features/profile/composables/` — `useAddressForm.ts`, `useWishlistActions.ts`, barrel `index.ts`
- **Depends**: T5.4
- **Blocks**: T5.6
- **Files**: `app/Store/src/features/profile/composables/*.ts`
- **Acceptance**: useAddressForm wraps addressStore, useWishlistActions wraps wishlistStore

### T5.6 — Profile Routes + Views
- **What**: Create `features/profile/routes/index.ts`, `views/index.ts`
- **Depends**: T5.4, T5.5
- **Blocks**: Phase 7
- **Files**: `app/Store/src/features/profile/routes/index.ts`, `views/index.ts`
- **Acceptance**: Account/Addresses/Wishlists/Notifications routes defined

### T5.7 — Profile Domain Barrel
- **What**: Create `features/profile/index.ts`
- **Depends**: T5.1–T5.4
- **Blocks**: None
- **File**: `app/Store/src/features/profile/index.ts`
- **Acceptance**: Barrel re-exports all

---

## 6. Supporting Domains (Phase 6)

### T6.1 — Inventory Domain
- **What**: Create `features/inventory/` — types (`availability.ts`), validations, services (`availabilityApi.ts`, `cartReservationApi.ts`), store (`availabilityStore.ts`), barrel `index.ts`
- **Depends**: T1.4
- **Blocks**: T6.2–T6.4
- **Files**: `app/Store/src/features/inventory/types/availability.ts`, `validations/availability.ts`, `services/availabilityApi.ts`, `services/cartReservationApi.ts`, `stores/availabilityStore.ts`, `index.ts`
- **Acceptance**: AvailabilityEntry, CartReservation types; availabilityApi.check() with Zod; availabilityStore with cache + TTL

### T6.2 — Payment Domain
- **What**: Create `features/payment/` — types (`payment.ts`), validations, services (`paymentApi.ts`), composables (`useStripe.ts`), barrel `index.ts`
- **Depends**: T1.4
- **Blocks**: T6.3–T6.4
- **Files**: `app/Store/src/features/payment/types/payment.ts`, `validations/payment.ts`, `services/paymentApi.ts`, `composables/useStripe.ts`, `index.ts`
- **Acceptance**: PaymentMethod, PaymentIntent types; useStripe init/mount/unmount; Stripe.js singleton

### T6.3 — Shipping Domain
- **What**: Create `features/shipping/` — types (`shipping.ts`), validations, services (`shippingApi.ts`), store (`shippingStore.ts`), barrel `index.ts`
- **Depends**: T1.4
- **Blocks**: T6.4
- **Files**: `app/Store/src/features/shipping/types/shipping.ts`, `validations/shipping.ts`, `services/shippingApi.ts`, `stores/shippingStore.ts`, `index.ts`
- **Acceptance**: ShippingMethod, ShippingRate types; shippingStore with methods/rates/selectedMethodId

### T6.4 — Location Domain
- **What**: Create `features/location/` — types (`location.ts`), validations, services (`countryApi.ts`, `stateApi.ts`), store (`locationStore.ts`), composables (`useLocationCascade.ts`), barrel `index.ts`
- **Depends**: T1.4
- **Blocks**: None
- **Files**: `app/Store/src/features/location/types/location.ts`, `validations/location.ts`, `services/countryApi.ts`, `services/stateApi.ts`, `stores/locationStore.ts`, `composables/useLocationCascade.ts`, `index.ts`
- **Acceptance**: Country, State types; locationStore with countries/states/filteredStates; useLocationCascade for address forms

### T6.5 — Supporting Domain Barrels
- **What**: Verify all 4 domain `index.ts` barrels re-export correctly
- **Depends**: T6.1–T6.4
- **Blocks**: Phase 7
- **Acceptance**: `import { ... } from '@/features/inventory'` resolves

---

## 7. Layout + Router Wiring (Phase 7)

### T7.1 — App.vue
- **What**: Modify `app/Store/src/App.vue` — add Toast, ScrollTop, SearchOverlay, global Cmd+K handler
- **Depends**: T1.5, T2.5
- **Blocks**: T7.2–T7.6
- **File**: `app/Store/src/App.vue`
- **Acceptance**: App.vue imports useTheme, useSearch, renders Toast + router-view

### T7.2 — DefaultLayout.vue
- **What**: Modify `app/Store/src/layouts/DefaultLayout.vue` — AppHeader + main + AppFooter
- **Depends**: T7.1, T7.5, T7.6
- **Blocks**: T7.3
- **File**: `app/Store/src/layouts/DefaultLayout.vue`
- **Acceptance**: Layout renders header/content/footer structure

### T7.3 — AuthLayout.vue
- **What**: Modify `app/Store/src/layouts/AuthLayout.vue` — centered card with logo + router-view
- **Depends**: T7.1
- **Blocks**: T7.4
- **File**: `app/Store/src/layouts/AuthLayout.vue`
- **Acceptance**: Auth pages render in centered card layout

### T7.4 — AccountLayout.vue
- **What**: Modify `app/Store/src/layouts/AccountLayout.vue` — sidebar nav (8 items) + content
- **Depends**: T7.1
- **Blocks**: None
- **File**: `app/Store/src/layouts/AccountLayout.vue`
- **Acceptance**: Sidebar with Orders/Addresses/Profile/Sessions/Wishlists/Notifications/ChangePassword/Preferences links

### T7.5 — AppHeader.vue
- **What**: Modify `app/Store/src/app/components/AppHeader.vue` — nav links, search button, cart badge, auth buttons, mobile menu toggle
- **Depends**: T3.4, T4.4, T2.5
- **Blocks**: T7.2
- **File**: `app/Store/src/app/components/AppHeader.vue`
- **Acceptance**: Header shows Shop/Collections/VisualSearch nav, cart count badge, sign-in/account buttons

### T7.6 — MobileNav + AppFooter + ThemeToggle
- **What**: Create `app/Store/src/app/components/MobileNav.vue`, `AppFooter.vue`, `ThemeToggle.vue`
- **Depends**: T7.5
- **Blocks**: T7.2
- **Files**: `app/Store/src/app/components/MobileNav.vue`, `AppFooter.vue`, `ThemeToggle.vue`
- **Acceptance**: MobileNav overlay, AppFooter 4-column grid, ThemeToggle dark/light toggle

---

## 8. Final Verification (Phase 8)

### T8.1 — TypeScript Check
- **What**: Run `cd app/Store && npx tsc --noEmit` — zero errors
- **Depends**: All Phase 1–7 tasks
- **Blocks**: T8.2
- **Acceptance**: Exit code 0, no type errors

### T8.2 — Vite Build
- **What**: Run `cd app/Store && npx vite build` — no missing modules, no unresolved imports
- **Depends**: T8.1
- **Blocks**: T8.3
- **Acceptance**: Build succeeds, output in `dist/`

### T8.3 — Run Test Suites
- **What**: Run `cd app/Store && npx vitest run` — all 257 existing tests pass
- **Depends**: T8.2
- **Blocks**: None
- **Acceptance**: All tests green

---

## Dependency Graph

```
T1.1 ──┐
T1.2 ──┤
T1.3 ──┼── T1.4 ──┬── T2.3 ── T2.4 ── T2.5 ── T2.7
T1.5 ──┤          │                          │
T1.6 ──┘          ├── T3.3 ── T3.4 ── T3.5 ── T3.6
                   │                          │
T2.1 ── T2.2 ─────┤                          ├── T7.1 ── T7.2
T2.6 ─────────────┤                          │   T7.3
                   ├── T4.3 ── T4.4 ── T4.5 ── T4.6    T7.4
                   │                          │          T7.5 ── T7.6
T3.1 ── T3.2 ─────┤                          │
T4.1 ── T4.2 ─────┤                          │
T5.1 ── T5.2 ─────┼── T5.3 ── T5.4 ── T5.5 ── T5.6
                   │
T6.1 ─────────────┤
T6.2 ─────────────┤
T6.3 ─────────────┤
T6.4 ─────────────┘

T7.1 ── T8.1 ── T8.2 ── T8.3 (FINAL)
```

## Parallel Execution Windows

**Window 1** (after T1.1–T1.6): T2.1, T2.2, T2.6, T3.1, T3.2, T4.1, T4.2, T5.1, T5.2, T6.1, T6.2, T6.3, T6.4 (all in parallel)
**Window 2** (after types): T2.3, T3.3, T4.3, T5.3 (all API services in parallel)
**Window 3** (after services): T2.4, T2.5, T3.4, T3.5, T4.4, T4.5, T5.4, T5.5 (all stores + composables in parallel)
**Window 4** (after stores): T2.7, T3.6, T4.6, T5.6, T6.5 (all views + routes + barrels in parallel)
**Window 5** (after views): T7.1–T7.6 (layout wiring, sequential dependencies)
**Window 6** (after layouts): T8.1–T8.3 (final verification, sequential)
