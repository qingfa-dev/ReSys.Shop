# Store SPA Rebuild — API-First Architecture

**Date:** 2026-08-08
**Status:** Design — pending implementation

---

## 1. Overview

Rebuild the Store SPA from the ground up using an API-first architecture. Focus exclusively on the data layer — types, validations, API services, and Pinia stores — with skeleton PrimeVue pages as placeholders. Zero UI feature implementation.

**Scope:** 8 feature domains, 28 route paths, 15 Pinia stores, 20 service classes (~67 methods), ~55 Zod schemas, 28 skeleton pages.

**Tech stack:** Vue 3.5 + Pinia 4 + Zod 4.4 + Axios 1.18 + PrimeVue 5

---

## 2. Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Type source | TypeScript interfaces, separately defined | Match backend DTOs exactly. Zod schemas validate at runtime (like FluentValidation). |
| File organization | Admin SPA pattern — `types/`, `validations/`, `services/`, `stores/`, `composables/` subfolders per domain | Consistent with Admin SPA. Layer separation makes each file's purpose obvious. |
| Service style | Static classes (`class ProductApi { static getProducts() }`) | Matches Admin SPA. No instantiation overhead. Easy tree-shaking. |
| Store design | Pinia stores as feature plugins — full business logic, coordinates API calls, handles caching/dedup/side-effects | UI imports only stores. No direct API imports from views. |
| Cross-store communication | Event bus (`useStoreEvents`) — typed events, stores subscribe independently | Avoids circular imports. Stores remain independently testable. |
| Skeleton pages | PrimeVue Breadcrumb + Card + Skeleton + Message placeholder, commented sections for future features | Structured enough to wire routes, light enough to avoid premature UI work. |
| Entity splitting | One types file + one validations file + one service file per entity (`product.ts`, `productApi.ts`) | Granular like Admin SPA. Easy to find, easy to test. |

---

## 3. File Structure

```
src/
├── features/
│   ├── catalog/
│   │   ├── index.ts
│   │   ├── types/            # 8 files
│   │   │   ├── index.ts
│   │   │   ├── product.ts
│   │   │   ├── variant.ts
│   │   │   ├── taxon.ts
│   │   │   ├── optionType.ts
│   │   │   ├── searchByImage.ts
│   │   │   ├── catalogQuery.ts
│   │   │   ├── taxonBreadcrumb.ts
│   │   │   └── taxonTree.ts
│   │   ├── validations/      # 5 files
│   │   │   ├── index.ts
│   │   │   ├── product.ts
│   │   │   ├── taxon.ts
│   │   │   ├── optionType.ts
│   │   │   └── searchByImage.ts
│   │   ├── services/         # 4 files
│   │   │   ├── index.ts
│   │   │   ├── productApi.ts
│   │   │   ├── taxonApi.ts
│   │   │   ├── optionTypeApi.ts
│   │   │   └── searchByImageApi.ts
│   │   ├── stores/           # 4 stores
│   │   │   ├── index.ts
│   │   │   ├── catalogStore.ts
│   │   │   ├── productListStore.ts
│   │   │   ├── productDetailStore.ts
│   │   │   └── visualSearchStore.ts
│   │   ├── composables/
│   │   │   ├── index.ts
│   │   │   ├── useSearch.ts
│   │   │   └── useVisualSearch.ts
│   │   ├── views/            # 9 pages
│   │   │   ├── index.ts
│   │   │   ├── HomeView.vue
│   │   │   ├── ShopView.vue
│   │   │   ├── ProductDetailView.vue
│   │   │   ├── CollectionsView.vue
│   │   │   ├── VisualSearchView.vue
│   │   │   ├── NotFoundView.vue
│   │   │   ├── AboutView.vue
│   │   │   ├── TermsView.vue
│   │   │   └── PrivacyView.vue
│   │   └── components/       # Not in scope — deferred
│   │
│   ├── identity/
│   │   ├── index.ts
│   │   ├── types/            # 1 file (auth.ts)
│   │   ├── validations/      # 1 file (auth.ts)
│   │   ├── services/         # 4 files
│   │   │   ├── authApi.ts
│   │   │   ├── emailApi.ts
│   │   │   ├── sessionApi.ts
│   │   │   └── tokenService.ts
│   │   ├── stores/
│   │   │   └── authStore.ts
│   │   └── views/            # 5 pages
│   │       ├── LoginView.vue
│   │       ├── RegisterView.vue
│   │       ├── ForgotPasswordView.vue
│   │       ├── ResetPasswordView.vue
│   │       └── SessionsView.vue
│   │
│   ├── ordering/
│   │   ├── index.ts
│   │   ├── types/            # 3 files (cart.ts, checkout.ts, order.ts)
│   │   ├── validations/      # 3 files
│   │   ├── services/         # 3 files (cartApi.ts, checkoutApi.ts, orderApi.ts)
│   │   ├── stores/           # 3 stores
│   │   │   ├── cartStore.ts
│   │   │   ├── checkoutStore.ts
│   │   │   └── orderStore.ts
│   │   ├── composables/
│   │   │   └── useQuickAdd.ts
│   │   └── views/            # 4 pages
│   │       ├── CartView.vue
│   │       ├── CheckoutView.vue
│   │       ├── OrderListView.vue
│   │       └── OrderDetailView.vue
│   │
│   ├── profile/
│   │   ├── index.ts
│   │   ├── types/            # 5 files (profile.ts, address.ts, wishlist.ts, notification.ts, preferences.ts)
│   │   ├── validations/      # 5 files
│   │   ├── services/         # 5 files
│   │   │   ├── profileApi.ts
│   │   │   ├── addressApi.ts
│   │   │   ├── wishlistApi.ts
│   │   │   ├── notificationApi.ts
│   │   │   └── accountApi.ts
│   │   ├── stores/           # 3 stores
│   │   │   ├── profileStore.ts
│   │   │   ├── addressStore.ts
│   │   │   └── wishlistStore.ts
│   │   └── views/            # 6 pages
│   │       ├── ProfileView.vue
│   │       ├── AddressBookView.vue
│   │       ├── ChangePasswordView.vue
│   │       ├── NotificationPrefsView.vue
│   │       ├── PreferencesView.vue
│   │       └── WishlistsView.vue
│   │
│   ├── inventory/
│   │   ├── index.ts
│   │   ├── types/            # 1 file (availability.ts)
│   │   ├── validations/      # 1 file
│   │   ├── services/         # 2 files (availabilityApi.ts, cartReservationApi.ts)
│   │   └── stores/
│   │       └── availabilityStore.ts
│   │
│   ├── payment/
│   │   ├── index.ts
│   │   ├── types/            # 1 file (payment.ts)
│   │   ├── validations/      # 1 file
│   │   ├── services/         # 1 file (paymentApi.ts)
│   │   └── composables/
│   │       └── useStripe.ts
│   │
│   ├── shipping/
│   │   ├── index.ts
│   │   ├── types/            # 1 file (shipping.ts)
│   │   ├── validations/      # 1 file
│   │   ├── services/         # 1 file (shippingApi.ts)
│   │   └── stores/
│   │       └── shippingStore.ts
│   │
│   └── location/
│       ├── index.ts
│       ├── types/            # 1 file (location.ts)
│       ├── validations/      # 1 file
│       ├── services/         # 2 files (countryApi.ts, stateApi.ts)
│       ├── stores/
│       │   └── locationStore.ts
│       └── composables/
│           └── useLocationCascade.ts
│
├── shared/
│   ├── api/
│   │   ├── client.ts              # Axios instance + HTTP verbs (get/post/put/del/patch)
│   │   ├── errors.ts              # HttpError class
│   │   ├── paged.ts               # getPaged<T> wrapper
│   │   ├── result.ts              # Result<T>, PagedResult<T> types
│   │   └── interceptors/          # auth, camelCase, error, refresh
│   ├── constants/
│   │   └── api.ts                 # API path segments (STOREFRONT, CATALOG, etc.)
│   ├── composables/
│   │   ├── usePagedQuery.ts       # Paged list state factory
│   │   ├── useStoreEvents.ts      # Typed cross-store event bus
│   │   ├── useTheme.ts
│   │   ├── useRecentlyViewed.ts
│   │   ├── usePreferences.ts
│   │   ├── useNotify.ts
│   │   ├── useApiErrorHandler.ts
│   │   └── usePageTitle.ts
│   ├── types/
│   │   ├── result.ts              # Result<T> interface
│   │   ├── error.ts               # ErrorType, ApiError
│   │   └── querying/              # QueryingParameters, FilterModel, SortModel, PageModel
│   └── utils/
│       ├── currency.ts
│       ├── date.ts
│       └── postLoginRedirect.ts
│
└── app/
    └── router/                    # 28 routes, guards, meta (unchanged from current)
```

---

## 4. Store Design — Full Detail

### Design Principles

1. Every action returns `Result<T>` — UI never interprets raw API errors
2. Every async action has `loading: boolean` + `error: string | null`
3. In-flight request deduplication — concurrent calls reuse existing promise
4. Optimistic updates where safe (cart quantity), fetch-after-mutate otherwise (checkout)
5. Side-effect cleanup on store `$dispose`
6. Cross-store coordination via typed event bus (never direct imports)
7. `init()` guarded — each store inits once, idempotent

### Store Inventory (15 stores)

#### `useAuthStore` (identity)

```
State:
  user: AuthUser | null
  status: 'idle' | 'loading' | 'authenticated' | 'error'
  error: string | null
  pendingRedirect: string | null
  _initialized: boolean

Getters:
  isAuthenticated: boolean
  userId: string | null
  hasPermission(p: string): boolean

Actions:
  init()                    → Validates token, fetches session, emits auth:login or stays idle
  login(credential, pwd)    → POST login/password, stores tokens, session, cart merge
  loginWithGoogle()         → Fetches provider list, redirects
  register(req)             → POST register, auto-login if tokens returned
  logout(revokeAll?)        → POST logout, clear tokens, reset cartStore, emit auth:logout
  changePassword(old, new)  → POST passwords/change
  forgotPassword(email)     → POST passwords/forgot
  resetPassword(token, pwd) → POST passwords/reset
  changeEmail(email)        → POST emails/change
  confirmEmail(token)       → POST emails/confirm
  resendVerification()      → POST emails/resend

Side-effects: tokenService (localStorage), cartStore.associateGuestCart() on login
Events: auth:login, auth:logout, auth:init-done
```

#### `useCartStore` (ordering)

```
State:
  id: string | null
  items: CartLineItem[]
  reservations: CartReservationStatus[]
  loading: boolean
  error: string | null
  lastFetchedAt: number
  cartToken: string
  pendingAdds: Map<string, Promise<Result<CartResponse>>>
  _initialized: boolean

Getters:
  itemCount: number
  subtotal: number
  isEmpty: boolean
  lineItemFor(variantId: string): CartLineItem | undefined

Actions:
  initCartToken()           → Generates UUID if not in localStorage
  fetchCart()               → GET /cart + GET /cart/reserve (parallel), deduplicated, 30s throttle
  addItem(variantId, qty)   → POST /cart/items + POST /cart/reserve, optimistic, dedup
  updateQuantity(id, qty)   → PUT /cart/items/{id}, optimistic, rollback on failure
  removeItem(lineItemId)    → DELETE /cart/items/{id}, releases reservation, optimistic
  clearCart()               → POST /cart/empty, releases all reservations
  associateGuestCart()      → POST /cart/associate, called on auth:login
  refreshIfStale()          → Fetch if lastFetchedAt > 30s ago
  reset()                   → Clear all state

Events: cart:updated
Cross-store: listens to auth:login (associateGuestCart), auth:logout (reset)
```

#### `useCheckoutStore` (ordering)

```
State:
  currentStep: 1|2|3|4|5
  shipAddressId, billAddressId: string|null
  shippingMethodId, paymentMethodId: string|null
  paymentIntentId, paymentClientSecret: string|null
  orderId: string|null
  email: string
  loading, error: boolean|null
  stepValidation: Record<number, boolean>

Getters:
  steps: {label, number, complete, current}[]
  canProceed: boolean
  isComplete: boolean

Actions:
  init()                    → Guards auth + non-empty cart
  saveAddress(id, email)    → PUT /cart, validates with Zod
  selectShippingRate(id)    → POST /cart/shipping-rate
  createPaymentIntent(id)   → POST /payment/create-intent
  validateCheckout()        → POST /cart/validate
  placeOrder()              → POST /cart/checkout, emits checkout:placed
  confirmPayment(id)        → POST /payment/confirm/{id}
  goToStep(step)            → Validates previous steps before advancing
  reset()

Events: checkout:placed
Cross-store: reads cartStore.id, addressStore, shippingStore
```

#### `useOrderStore` (ordering)

```
State:
  items: OrderListItem[]
  loading, detailLoading, cancelLoading: boolean
  error: string|null
  page, pageSize, totalCount: number
  statusFilter: OrderStatus|'All'
  currentOrder: OrderDetail|null
  timeline: OrderTimelineEntry[]
  isInitialLoad, retryCount: number

Actions:
  fetchOrders()             → GET /orders (paged, filtered, sorted), dedup
  setStatusFilter(status)   → Resets page to 1
  fetchOrder(id)            → GET /orders/{id} + GET /orders/{id}/tracking
  cancelOrder(id)           → PUT /orders/{id}/cancel, optimistic
  setPage(), setSort(), nextPage(), prevPage(), refreshOrderList(), retry()
  resetDetail()

Events: listens to checkout:placed (refreshOrderList)
```

#### `useCatalogStore` (catalog)

```
State:
  searchQuery: string
  selectedTaxonIds: string[]
  selectedOptionValueIds: string[]
  minPrice, maxPrice: number|null
  sortField: string
  taxonomyGroups: TaxonomyGroup[]
  optionTypes: FilterableOptionType[]
  taxonsLoading, optionsLoading: boolean

Getters:
  activeFilterCount: number
  activeFilterSummary: {label, onRemove}[]
  queryingParams: QueryingParameters

Actions:
  setSearch(query)          → Debounced 300ms
  toggleTaxon(id), toggleOptionValue(id)
  setPriceRange(min, max)
  clearFilters()            → Resets all, emits filter:changed
  loadTaxonomyGroups()      → Cached, recursive tree build
  loadOptionTypes()         → Filterable only, cached
  applyUrlParams(params)    → Parses ?search=, ?taxonId=, etc.
  toUrlParams()             → Serializes current filters to URL string

Events: filter:changed
```

#### `useProductListStore` (catalog)

```
State:
  items: StoreProductListItemResponse[]
  loading, error: boolean|null
  page, pageSize, totalCount: number
  isInitialLoad, retryCount: number

Actions:
  fetch()                   → GET /products with filters from catalogStore, dedup
  nextPage(), prevPage(), goToPage(), refresh(), retry()
  init()                    → Watches catalogStore filters, debounced auto-fetch

Events: listens to filter:changed (marks stale, debounced fetch)
```

#### `useProductDetailStore` (catalog)

```
State:
  product: StoreProductDetailResponse|null
  loading, relatedLoading, error: boolean|null
  selectedVariantId: string|null
  quantity: number
  availability: AvailabilityEntry[]
  similarProducts, relatedProducts: StoreProductListItemResponse[]

Getters:
  selectedVariant: StoreVariantResponse|null
  stockLabel: string|null
  stockSeverity: severity
  sizeOptions: string[]
  breadcrumbs: {label, to?}[]
  isInStock: boolean

Actions:
  load(slug)                → GET /products/{slug}, records to recentlyViewed, background loads similar+related
  loadSimilar(id, topK?)
  loadRelated(id, params?)
  selectVariant(id)         → Triggers availabilityStore.check()
  addToCart()               → Delegates to cartStore
  quickAdd(variantId)       → Delegates to cartStore
  increment/decrementQuantity()
  reset()

Cross-store: availabilityStore.check(), useRecentlyViewed.add()
```

#### `useVisualSearchStore` (catalog)

```
State:
  state: 'empty'|'upload'|'loading'|'results'
  selectedFile: File|null
  previewUrl: string|null
  selectedModelId: string|null
  availableModels: VisualSearchModel[]
  results: SearchByImageResponse[]
  loading, error: boolean|null
  validationError: {message}|null

Actions:
  validateFile(file): boolean
  selectFile(file)          → Validates, creates preview, state→upload
  search(topK?, model?)     → POST multipart /images/search, state→results
  loadModels()              → GET /visual-search/models
  reset()                   → Revokes object URL, state→empty
```

#### `useProfileStore` (profile)

```
State:
  profile: ProfileDetail|null
  loading, saving, error: boolean|null
  _initialized: boolean

Actions:
  init()                    → Fetch if authenticated
  fetchProfile()            → GET /profiles
  updateProfile(req)        → PUT /profiles, optimistic, rollback on failure
  deleteProfile()           → DELETE /profiles, emits auth:logout
  reset()

Events: profile:deleted
```

#### `useAddressStore` (profile)

```
State:
  addresses: Address[]
  loading, saving, error: boolean|null
  defaultAddressId: string|null

Getters:
  defaultAddress: Address|undefined
  shippingAddresses: Address[]

Actions:
  fetchAddresses()
  createAddress(req)        → POST /profiles/addresses
  updateAddress(id, req)    → PUT /profiles/addresses/{id}
  deleteAddress(id)         → DELETE /profiles/addresses/{id}
  setDefault(id)
```

#### `useWishlistStore` (profile)

```
State:
  lists: WishlistListItem[]
  loading, saving, error: boolean|null
  details: Record<string, WishlistDetail>
  detailLoadingIds: Set<string>
  wishlistedVariantIds: Set<string>

Getters:
  isWishlisted(variantId): boolean

Actions:
  fetchWishlists()
  fetchWishlist(id)         → Cached in details[]
  createWishlist(req)       → Appends to lists
  updateWishlist(id, req)
  deleteWishlist(id)
  addItem(listId, req)      → Updates wishlistedVariantIds
  removeItem(listId, itemId)→ Rebuilds wishlistedVariantIds
  fetchWishlistedIds()      → Builds Set<variantId>, called once on auth init
  toggleWishlist(variantId) → Auto-create default list if none exists
```

#### `useAvailabilityStore` (inventory)

```
State:
  cache: Record<string, {entry: AvailabilityEntry, fetchedAt: number}>
  loading: boolean
  pendingIds: Set<string>

Actions:
  check(variantId)          → GET /availability/{variantId}, 60s cache, dedup
  checkBatch(variantIds)    → Parallel, max 10 concurrent
  invalidate(variantId)     → Called by cartStore after reservation changes
  resetCache()
```

#### `useShippingStore` (shipping)

```
State:
  methods: ShippingMethod[]
  rates: ShippingRate[]
  selectedMethodId: string|null
  loading, error: boolean|null

Actions:
  fetchMethods()            → GET /shipping/methods, session-cached
  fetchRates(orderId)       → GET /shipping/rates
  calculateRate(methodId, orderId) → POST /shipping/calculate
  selectMethod(id)
```

#### `useLocationStore` (location)

```
State:
  countries: Country[]
  states: State[]
  filteredStates: State[]
  selectedCountryId, selectedStateId: string|null
  loading: boolean
  _initialized: boolean

Getters:
  statesForCountry(id): State[]
  statesRequireSelection: boolean

Actions:
  loadAll()                 → Parallel GET /countries + GET /states, one-time cache
  selectCountry(id)         → Resets state, filters states
```

### Cross-Store Event Map

| Emitter | Event | Subscriber | Action |
|---------|-------|-----------|--------|
| authStore | `auth:login` | cartStore | associateGuestCart |
| authStore | `auth:login` | profileStore | fetchProfile |
| authStore | `auth:logout` | cartStore | reset |
| authStore | `auth:logout` | profileStore | reset |
| authStore | `auth:login` | wishlistStore | fetchWishlistedIds |
| authStore | `auth:init-done` | wishlistStore | fetchWishlistedIds |
| catalogStore | `filter:changed` | productListStore | markStale + debounced fetch |
| checkoutStore | `checkout:placed` | orderStore | refreshOrderList |
| checkoutStore | `checkout:placed` | cartStore | fetchCart |
| cartStore | `cart:updated` | checkoutStore | validateStep |
| profileStore | `profile:deleted` | authStore | logout |

---

## 5. API Service Layer

### Pattern

Every service file exports a **static class** with:
- `private static readonly BASE` — endpoint path
- Static async methods returning `Result<T>` / `PagedResult<T>`
- `Schema.parse()` called on all response data before returning
- Import path constants from `@/shared/constants/api`

### All Service Classes (20 classes, ~67 methods)

**catalog — 4 classes**
| Class | Methods | Endpoints |
|-------|---------|-----------|
| `ProductApi` | `getProducts(q)`, `getProductBySlug(slug)`, `getSimilar(id, topK?)`, `getRelated(id, q)` | products, products/{slug}, similar, related |
| `TaxonApi` | `getTaxonomies(q)`, `getTaxons(q)` | taxonomies, taxons |
| `OptionTypeApi` | `getOptionTypes(q)`, `getOptionValues(q)` | option-types, option-values |
| `SearchByImageApi` | `getVisualSearchModels()`, `searchByImage(file, topK?, model?)` | visual-search/models, images/search |

**identity — 3 classes**
| Class | Methods | Endpoints |
|-------|---------|-----------|
| `AuthApi` | `login(req)`, `register(req)`, `logout(req?)`, `getSession()`, `getLoginProviders()`, `forgotPassword(e)`, `resetPassword(t,p)`, `changePassword(o,n)` | auth/login/password, register, logout, sessions, login/providers, passwords/* |
| `EmailApi` | `changeEmail(e)`, `confirmEmail(t)`, `resendVerification()` | emails/change, confirm, resend |
| `SessionApi` | `getSessions()`, `revokeCurrentDevice()`, `revokeAll()` | sessions, logout |

**ordering — 3 classes**
| Class | Methods | Endpoints |
|-------|---------|-----------|
| `CartApi` | `getCart()`, `addItem(req)`, `updateItem(id, req)`, `removeItem(id)`, `emptyCart()`, `associateCart(id)` | cart, items, items/{id}, empty, associate |
| `CheckoutApi` | `updateCheckout(req)`, `selectShippingRate(req)`, `validateCheckout()`, `createPaymentIntent(req)`, `placeOrder(req)` | cart (PUT), shipping-rate, validate, payment/create-intent, checkout |
| `OrderApi` | `getOrders(q)`, `getOrder(id)`, `getOrderTracking(id)`, `cancelOrder(id)` | orders, orders/{id}, tracking, cancel |

**profile — 4 classes**
| Class | Methods | Endpoints |
|-------|---------|-----------|
| `ProfileApi` | `getProfile()`, `updateProfile(req)`, `deleteProfile()` | profiles |
| `AddressApi` | `getAddresses()`, `createAddress(req)`, `updateAddress(id, req)`, `deleteAddress(id)`, `getDefaultAddress()` | profiles/addresses |
| `WishlistApi` | `getWishlists()`, `getWishlist(id)`, `createWishlist(req)`, `updateWishlist(id, req)`, `deleteWishlist(id)`, `addWishlistItem(id, req)`, `removeWishlistItem(listId, itemId)` | profiles/wishlists |
| `NotificationApi` | `getNotificationPreferences()`, `updateNotificationPreferences(req)` | profiles/notification-preferences |

**inventory — 2 classes**
| Class | Methods | Endpoints |
|-------|---------|-----------|
| `AvailabilityApi` | `checkAvailability(variantId)` | availability/{variantId} |
| `ReservationApi` | `reserveStock(req, token)`, `releaseReservation(id)`, `getCartReservations(token, q?)` | cart/reserve |

**payment — 1 class**
| Class | Methods | Endpoints |
|-------|---------|-----------|
| `PaymentApi` | `getPaymentMethods(q?)`, `confirmPayment(id)`, `createSetupIntent(req)` | payment/methods, confirm/{id}, setup-intent |

**shipping — 1 class**
| Class | Methods | Endpoints |
|-------|---------|-----------|
| `ShippingApi` | `getShippingMethods(q?)`, `getShippingRates(q?)` | shipping/methods, rates |

**location — 2 classes**
| Class | Methods | Endpoints |
|-------|---------|-----------|
| `CountryApi` | `getCountries()` | locations/countries |
| `StateApi` | `getStates()` | locations/states |

### API Constants (`shared/constants/api.ts`)

```typescript
export const STOREFRONT = 'api/storefront'
export const STORE = 'api/store'
export const CATALOG = `${STOREFRONT}`
export const IDENTITY = `${STORE}/identity`
export const PROFILES = `${STORE}/profiles`
export const LOCATIONS = `${STORE}/locations`
export const ORDERS = `${STOREFRONT}/orders`
export const CART = `${STOREFRONT}/cart`
export const PAYMENT = `${STOREFRONT}/payment`
export const SHIPPING = `${STOREFRONT}/shipping`
export const AVAILABILITY = `${STOREFRONT}/availability`
```

---

## 6. Validation Layer

Each `validations/{entity}.ts` exports:
1. Named field schemas (reusable in forms via vee-validate)
2. Compound entity schemas (for API response validation)
3. `z.infer` derived form types

**Total: ~55 Zod schemas across 8 domains + shared**

### Catalog (5 entity schemas)
- `product.ts` — ProductListItemSchema, ProductDetailSchema, ProductSearchFormSchema
- `taxon.ts` — TaxonListItemSchema, TaxonTreeSchema, TaxonomyGroupSchema
- `optionType.ts` — OptionTypeSchema, OptionValueSchema
- `searchByImage.ts` — SearchByImageResponseSchema, VisualSearchModelSchema, ImageSearchFormSchema

### Identity (1 entity schema)
- `auth.ts` — LoginRequestSchema, RegisterRequestSchema, TokenPairSchema, SessionUserSchema, SessionInfoSchema, ForgotPasswordSchema, ResetPasswordSchema, ChangePasswordSchema, EmailSchema

### Ordering (3 entity schemas)
- `cart.ts` — CartLineItemSchema, CartResponseSchema, AddCartItemSchema, UpdateCartItemSchema
- `checkout.ts` — UpdateCheckoutSchema, SelectShippingRateSchema, CreatePaymentIntentSchema, PlaceOrderSchema
- `order.ts` — OrderListItemSchema, OrderDetailSchema, OrderTrackingSchema

### Profile (5 entity schemas)
- `profile.ts` — ProfileDetailSchema, UpdateProfileSchema
- `address.ts` — AddressSchema, AddressInputSchema
- `wishlist.ts` — WishlistListItemSchema, WishlistDetailSchema, CreateWishlistSchema, UpdateWishlistSchema, AddWishlistItemSchema
- `notification.ts` — NotificationPreferencesSchema
- `account.ts` — DeleteAccountSchema (confirm)

### Shared (4 entity schemas)
- `result.ts` — ResultSchema, PagedResultSchema
- `error.ts` — ErrorSchema, HttpErrorSchema
- `querying.ts` — QueryingParametersSchema, PageSchema, FilterSchema, SortSchema

---

## 7. Skeleton Page Format

Every view follows this template with PrimeVue components:

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'

usePageTitle('Page Name')
</script>

<template>
  <!-- Section: Page Name -->
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Page Name' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Page Name</h1>

    <Card>
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2rem" />
          <Skeleton width="75%" height="1rem" />
          <Skeleton width="50%" height="1rem" />
        </div>
      </template>
    </Card>

    <Message severity="info" class="mt-4">
      Feature content will be implemented here.
    </Message>
  </div>
</template>
```

### Page Inventory (28 views)

| Domain | Views | Type |
|--------|-------|------|
| catalog | Home, Shop, ProductDetail, Collections, VisualSearch, NotFound, About, Terms, Privacy | 5 skeletons + 4 static pages |
| identity | Login, Register, ForgotPassword, ResetPassword, Sessions | 5 skeletons |
| ordering | Cart, Checkout, OrderList, OrderDetail | 4 skeletons |
| profile | Profile, AddressBook, ChangePassword, NotificationPrefs, Preferences, Wishlists | 6 skeletons |

---

## 8. Router

28 routes, 3 layouts. Unchanged from current setup. Only change: views call store init in `onMounted` instead of scattered service calls.

| Route | Layout | Store Init |
|-------|--------|-----------|
| `/` | Default | catalogStore (static content) |
| `/shop` | Default | catalogStore.applyUrlParams() + productListStore.init() |
| `/products/:slug` | Default | productDetailStore.load(slug) |
| `/recommendations` | Default | visualSearchStore.loadModels() |
| `/cart` | Default | cartStore.fetchCart() |
| `/checkout` | Default | checkoutStore.init() (guards) |
| `/login` | Auth | - |
| `/account/*` | Account | Profile stores init on demand |

---

## 9. Shared Infrastructure

### New files
- `shared/constants/api.ts` — API path constants (no magic strings)
- `shared/composables/useStoreEvents.ts` — Typed event bus with `emit()` / `on()` / `off()`
- `shared/composables/usePageTitle.ts` — `document.title` from route meta

### Existing (kept)
- `shared/api/` — Axios client, interceptors, paged wrapper, error handler
- `shared/composables/usePagedQuery.ts`, `useTheme.ts`, `useNotify.ts`, `useApiErrorHandler.ts`
- `shared/utils/currency.ts`, `date.ts`, `postLoginRedirect.ts`
- `app/router/` — routes, guards, meta (unchanged)

---

## 10. Quality Gates

| # | Rule | Verification |
|---|------|-------------|
| 1 | Every API service method calls `Schema.parse()` before returning | Grep: `get<unknown>` without `.parse` |
| 2 | Every store action returns `Result<T>` | TypeScript compile check |
| 3 | No `.vue` imports from `services/` directly | ESLint rule: no-restricted-imports |
| 4 | Every Zod schema has corresponding types file | Directory listing check |
| 5 | All stores have `loading` + `error` for every async action | Code review |
| 6 | Cross-store communication uses `useStoreEvents` | Grep: direct store imports in store files |
| 7 | All stores have `_initialized` guard on `init()` | Code review |
| 8 | No hardcoded API paths in service files | Grep: `'api/'` string literal outside constants file |

---

## 11. Implementation Phases

**Phase 1 — Shared foundation** (no feature deps)
1. `shared/constants/api.ts`
2. `shared/composables/useStoreEvents.ts`
3. `shared/composables/usePageTitle.ts`
4. `shared/validations/` (result.ts, error.ts, querying.ts)

**Phase 2 — Domain layers** (all 8 domains in parallel)
For each domain: types → validations → services → stores → composables

**Phase 3 — Skeleton pages + route wiring**
28 views with PrimeVue Breadcrumb + Card + Skeleton + Message, store init in `onMounted`

**Phase 4 — Verification**
Build, lint, typecheck, test run for each phase
