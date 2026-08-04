# Shop Frontend — Architecture & Implementation Guide

This document provides a comprehensive reference for the ReSys.Shop Vue 3 frontend architecture, patterns, and implementation examples. The shop frontend follows a **Vertical-Slice Feature Architecture** where code is organized by business capability (features) rather than technical layer.

## Architecture Overview

The shop frontend is organized into organized into the following structure:

```
src/
├── app/                    # Application-level shared code
│   ├── components/         # Layout & shared UI components
│   ├── composables/        # Cross-feature composables
│   ├── layouts/            # Page layout wrappers
│   ├── router/             # Vue Router configuration
│   └── stores/             # Global Pinia stores
│
├── core/                   # Cross-cutting concerns & infrastructure
│   ├── http/               # Axios client & interceptors
│   ├── models/             # Shared domain models & types
│   ├── repositories/       # Abstract repository interfaces
│   ├── services/           # Core services (api, toast)
│   └── utils/              # Shared utility functions
│
└── features/               # Vertical-slice feature modules (10 total)
    ├── catalog/            # Product browsing, search, categories
    ├── identity/           # User authentication & sessions
    ├── inventory/          # Stock & availability management
    ├── locations/          # Address & location management
    ├── ordering/           # Cart & order management
    ├── payment/            # Payment processing
    ├── promotions/         # Discounts & offers
    ├── returns/            # Return requests & RMA
    ├── settings/           # User preferences & account
    └── shipping/           # Shipping methods & tracking
```

### Key Architectural Principles

- **Vertical Slices**: Each feature is self-contained with its own components, services, repositories, stores, and types
- **Composition API**: All Vue components use `<script setup>` (Options API is prohibited)
- **Pinia Setup-Store Pattern**: State management uses setup-store (not options-store)
- **Result<T> Pattern**: All service/repository operations return typed success/error envelopes
- **Dependency Injection**: Services and repositories are injected via composables
- **Lazy Loading**: Feature modules can be lazy-loaded via route definitions

---

# Types & Pinia Stores

---

## TypeScript Types

Types are located in:
- **Core types** (shared across features): `src/core/models/`
- **Feature-specific types**: `src/features/[featureName]/types/`

### Core Domain Models

#### `src/core/models/result.ts` — Result<T> Pattern

All service operations follow the Result<T> pattern for consistent error handling:

```ts
export interface Result<TSuccess, TError = string> {
  isSuccess: boolean
  isFailure: boolean
  value?: TSuccess
  error?: TError
}

export function success<T>(value: T): Result<T> {
  return { isSuccess: true, isFailure: false, value }
}

export function failure<E>(error: E): Result<any, E> {
  return { isSuccess: false, isFailure: true, error }
}
```

#### `src/core/models/paging.model.ts`

```ts
export interface PagedRequest {
  page: number
  pageSize: number
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}
```

### Feature-Specific Types — Catalog Feature

#### `src/features/catalog/types/product.ts`

```ts
export type Currency = 'SGD' | 'USD' | 'GBP' | 'EUR' | 'JPY'

export interface ProductImage {
  id: string
  url: string
  alt: string
  width: number
  height: number
  isVideo?: boolean
  videoUrl?: string
}

export interface ProductVariant {
  id: string
  size: string
  sku: string
  stock: number
  inStock: boolean
  price: number
}

export interface ColorOption {
  id: string
  name: string
  hex: string
  images: ProductImage[]
  variants: ProductVariant[]
  isAvailable: boolean
}

export interface Brand {
  id: string
  slug: string
  name: string
  logo: string
  country: string
  description?: string
}

export interface Category {
  id: string
  slug: string
  name: string
  parentId?: string
  children?: Category[]
}

export type ProductCondition = 'new' | 'pre-owned' | 'vintage'
export type ProductGender = 'women' | 'men' | 'unisex' | 'kids'

export interface Product {
  id: string
  slug: string
  name: string
  description: string
  brand: Brand
  categories: Category[]
  gender: ProductGender
  condition: ProductCondition
  colors: ColorOption[]
  composition: string
  careInstructions: string[]
  price: number
  originalPrice?: number
  currency: Currency
  discount?: number
  isNew: boolean
  isSale: boolean
  isFeatured: boolean
  isExclusive: boolean
  tags: string[]
  rating: number
  reviewCount: number
  images: ProductImage[]
  variants: ProductVariant[]
  relatedProducts?: string[]
  completeLookProducts?: string[]
  sizeGuideId?: string
  createdAt: string
  updatedAt: string
}

export interface ProductListItem extends Pick<Product,
  'id' | 'slug' | 'name' | 'brand' | 'price' | 'originalPrice' |
  'currency' | 'discount' | 'isNew' | 'isSale' | 'rating' | 'reviewCount'
> {
  images: [ProductImage, ProductImage?]
  defaultColor: Pick<ColorOption, 'id' | 'name' | 'hex'>
  availableColors: Array<Pick<ColorOption, 'id' | 'name' | 'hex' | 'isAvailable'>>
  availableSizes: string[]
  isWishlisted?: boolean
}

export interface Review {
  id: string
  productId: string
  userId: string
  userName: string
  rating: number
  title: string
  body: string
  fit: 'runs-small' | 'true-to-size' | 'runs-large'
  verified: boolean
  createdAt: string
  helpfulCount: number
  images?: ProductImage[]
}

export interface SizeGuide {
  id: string
  unit: 'cm' | 'inches'
  rows: { size: string; measurements: Record<string, string> }[]
  columns: string[]
}
```

### Feature-Specific Types — Ordering Feature

#### `src/features/ordering/types/cart.ts`

```ts
import type { Product, ProductVariant, ColorOption, Currency } from '@/features/catalog/types/product'

export interface CartItem {
  id: string
  productId: string
  variantId: string
  colorId: string
  quantity: number
  product: Pick<Product, 'id' | 'slug' | 'name' | 'brand'>
  variant: ProductVariant
  color: Pick<ColorOption, 'id' | 'name' | 'hex'>
  image: string
  price: number
  currency: Currency
  addedAt: string
}

export interface CartSummary {
  subtotal: number
  shipping: number | null
  tax: number
  discount: number
  total: number
  currency: Currency
  itemCount: number
  freeShippingThreshold: number
  amountToFreeShipping: number
}

export interface PromoCode {
  code: string
  type: 'percentage' | 'fixed' | 'free-shipping'
  value: number
  minOrderValue?: number
  isValid: boolean
  message?: string
}
```

#### `src/features/ordering/types/order.ts`

```ts
export interface Address {
  id: string
  label: string
  firstName: string
  lastName: string
  line1: string
  line2?: string
  city: string
  state: string
  postalCode: string
  country: string
  phone: string
  isDefault: boolean
}

export type OrderStatus =
  | 'pending'
  | 'confirmed'
  | 'processing'
  | 'shipped'
  | 'out-for-delivery'
  | 'delivered'
  | 'cancelled'
  | 'return-requested'
  | 'returned'
  | 'refunded'

export interface OrderItem {
  productId: string
  variantId: string
  name: string
  brand: string
  image: string
  size: string
  color: string
  quantity: number
  price: number
  currency: string
}

export interface PaymentInfo {
  method: 'card' | 'paypal' | 'apple-pay' | 'google-pay'
  last4?: string
  brand?: string
  status: 'pending' | 'paid' | 'failed' | 'refunded'
}

export interface OrderFinancials {
  subtotal: number
  shipping: number
  tax: number
  discount: number
  total: number
  currency: string
}

export interface TrackingInfo {
  carrier: string
  trackingNumber: string
  url: string
  events: {
    status: string
    location: string
    timestamp: string
  }[]
}

export interface Order {
  id: string
  number: string
  status: OrderStatus
  items: OrderItem[]
  shippingAddress: Address
  billingAddress: Address
  payment: PaymentInfo
  summary: OrderFinancials
  tracking?: TrackingInfo
  createdAt: string
  estimatedDelivery?: string
}
```

---

## Pinia Stores

Stores are organized into two categories:

- **Global Stores** (`src/app/stores/`): Cross-feature state (UI state, preferences, counter)
- **Feature-Scoped Stores** (`src/features/[name]/store/`): Feature-specific state (product list, cart items, filter state)

### Global Stores — `src/app/stores/`

#### `src/app/stores/ui.ts` — Global UI State

```ts
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useUIStore = defineStore('ui', () => {
  const searchOpen        = ref(false)
  const mobileNavOpen     = ref(false)
  const sizeGuideOpen     = ref(false)
  const cookieBannerShown = ref(false)
  const newsletterShown   = ref(false)
  const recentlyViewed = ref<string[]>([])

  function openSearch()       { searchOpen.value = true }
  function closeSearch()      { searchOpen.value = false }
  function toggleMobileNav()  { mobileNavOpen.value = !mobileNavOpen.value }
  function closeMobileNav()   { mobileNavOpen.value = false }
  function openSizeGuide()    { sizeGuideOpen.value = true }
  function closeSizeGuide()   { sizeGuideOpen.value = false }

  function dismissCookieBanner() {
    cookieBannerShown.value = true
    localStorage.setItem('cookies-accepted', 'true')
  }

  function dismissNewsletter() {
    newsletterShown.value = true
    localStorage.setItem('newsletter-dismissed', 'true')
  }

  function addRecentlyViewed(productId: string) {
    recentlyViewed.value = [
      productId,
      ...recentlyViewed.value.filter(id => id !== productId),
    ].slice(0, 10)
  }

  function hydrateUI() {
    cookieBannerShown.value  = !!localStorage.getItem('cookies-accepted')
    newsletterShown.value    = !!localStorage.getItem('newsletter-dismissed')
    const rv = localStorage.getItem('recently-viewed')
    if (rv) recentlyViewed.value = JSON.parse(rv)
  }

  return {
    searchOpen, mobileNavOpen, sizeGuideOpen,
    cookieBannerShown, newsletterShown, recentlyViewed,
    openSearch, closeSearch, toggleMobileNav, closeMobileNav,
    openSizeGuide, closeSizeGuide,
    dismissCookieBanner, dismissNewsletter, addRecentlyViewed, hydrateUI,
  }
})
```

### Feature-Scoped Stores

#### `src/features/ordering/store/ordering.ts` — Cart & Order Management

This store manages shopping cart and order state within the Ordering feature:

```ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { v4 as uuidv4 } from 'uuid'
import type { CartItem, CartSummary, PromoCode } from '@/features/ordering/types/cart'
import type { Product, ProductVariant, ColorOption } from '@/features/catalog/types/product'
import { useToast } from 'primevue/usetoast'

export const useOrderingStore = defineStore('ordering', () => {
  const items = ref<CartItem[]>([])
  const promoCode = ref<PromoCode | null>(null)
  const isOpen = ref(false)
  const isLoading = ref(false)

  // ─── Getters ────────────────────────────────────────────────────────────────
  const itemCount = computed(() =>
    items.value.reduce((sum, item) => sum + item.quantity, 0)
  )

  const isEmpty = computed(() => items.value.length === 0)

  const summary = computed<CartSummary>(() => {
    const subtotal = items.value.reduce(
      (sum, item) => sum + item.price * item.quantity, 0
    )
    const discount = promoCode.value?.type === 'percentage'
      ? subtotal * (promoCode.value.value / 100)
      : promoCode.value?.type === 'fixed'
        ? promoCode.value.value
        : 0
    const freeShippingThreshold = 500
    const shipping = subtotal >= freeShippingThreshold ? 0 : 15
    const tax = (subtotal - discount) * 0.09
    const total = subtotal - discount + shipping + tax

    return {
      subtotal,
      shipping,
      tax,
      discount,
      total,
      currency: 'SGD',
      itemCount: itemCount.value,
      freeShippingThreshold,
      amountToFreeShipping: Math.max(0, freeShippingThreshold - subtotal),
    }
  })

  // ─── Actions ─────────────────────────────────────────────────────────────────
  function addItem(
    product: Product,
    variant: ProductVariant,
    color: ColorOption,
    quantity = 1
  ) {
    const existing = items.value.find(
      i => i.variantId === variant.id && i.colorId === color.id
    )

    if (existing) {
      existing.quantity = Math.min(existing.quantity + quantity, variant.stock)
    } else {
      items.value.push({
        id: uuidv4(),
        productId: product.id,
        variantId: variant.id,
        colorId: color.id,
        quantity,
        product: { id: product.id, slug: product.slug, name: product.name, brand: product.brand },
        variant,
        color: { id: color.id, name: color.name, hex: color.hex },
        image: color.images[0]?.url ?? product.images[0]?.url,
        price: product.price,
        currency: product.currency,
        addedAt: new Date().toISOString(),
      })
    }
    isOpen.value = true
    persistCart()
  }

  function removeItem(cartItemId: string) {
    items.value = items.value.filter(i => i.id !== cartItemId)
    persistCart()
  }

  function updateQuantity(cartItemId: string, quantity: number) {
    const item = items.value.find(i => i.id === cartItemId)
    if (!item) return
    if (quantity <= 0) { removeItem(cartItemId); return }
    item.quantity = quantity
    persistCart()
  }

  function clearCart() {
    items.value = []
    promoCode.value = null
    persistCart()
  }

  async function applyPromoCode(code: string): Promise<PromoCode> {
    isLoading.value = true
    try {
      await new Promise(r => setTimeout(r, 600))
      const mock: PromoCode = {
        code,
        type: 'percentage',
        value: 10,
        isValid: code.toUpperCase() === 'FASHION10',
        message: code.toUpperCase() === 'FASHION10'
          ? '10% off applied!'
          : 'Invalid promo code',
      }
      if (mock.isValid) promoCode.value = mock
      return mock
    } finally {
      isLoading.value = false
    }
  }

  function removePromoCode() { promoCode.value = null }

  function persistCart() {
    localStorage.setItem('cart', JSON.stringify(items.value))
  }

  function hydrateCart() {
    const saved = localStorage.getItem('cart')
    if (saved) {
      try { items.value = JSON.parse(saved) as CartItem[] }
      catch { items.value = [] }
    }
  }

  return {
    items, promoCode, isOpen, isLoading,
    itemCount, isEmpty, summary,
    addItem, removeItem, updateQuantity,
    clearCart, applyPromoCode, removePromoCode, hydrateCart,
  }
})
```

#### `src/features/catalog/store/catalog.ts` — Product Catalog State

```ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { ProductListItem } from '@/features/catalog/types/product'
import type { PagedResult } from '@/core/models/paging.model'

export const useCatalogStore = defineStore('catalog', () => {
  const products = ref<ProductListItem[]>([])
  const isLoading = ref(false)
  const totalCount = ref(0)
  const currentPage = ref(1)

  const hasMore = computed(() => (currentPage.value * 20) < totalCount.value)

  function setProducts(items: ProductListItem[], total: number) {
    products.value = items
    totalCount.value = total
  }

  function addProducts(items: ProductListItem[]) {
    products.value.push(...items)
  }

  function clear() {
    products.value = []
    totalCount.value = 0
    currentPage.value = 1
  }

  return {
    products, isLoading, totalCount, currentPage, hasMore,
    setProducts, addProducts, clear,
  }
})
```

#### `src/features/catalog/store/wishlist.ts` — Wishlist State

```ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { ProductListItem } from '@/features/catalog/types/product'

export const useWishlistStore = defineStore('wishlist', () => {
  const items = ref<ProductListItem[]>([])

  const count = computed(() => items.value.length)
  const isEmpty = computed(() => items.value.length === 0)
  const productIds = computed(() => new Set(items.value.map(i => i.id)))

  function isWishlisted(productId: string) {
    return productIds.value.has(productId)
  }

  function toggle(product: ProductListItem) {
    if (isWishlisted(product.id)) {
      items.value = items.value.filter(i => i.id !== product.id)
    } else {
      items.value.push(product)
    }
    persist()
  }

  function remove(productId: string) {
    items.value = items.value.filter(i => i.id !== productId)
    persist()
  }

  function persist() {
    localStorage.setItem('wishlist', JSON.stringify(items.value))
  }

  function hydrate() {
    const saved = localStorage.getItem('wishlist')
    if (saved) {
      try { items.value = JSON.parse(saved) }
      catch { items.value = [] }
    }
  }

  return { items, count, isEmpty, isWishlisted, toggle, remove, hydrate }
})
```

#### `src/features/catalog/store/filters.ts` — Filter State

```ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface FilterState {
  categories: string[]
  brands: string[]
  priceRange: [number, number]
  sizes: string[]
  colors: string[]
  sortBy: 'recommended' | 'newest' | 'price-asc' | 'price-desc' | 'rating'
}

export const useFiltersStore = defineStore('filters', () => {
  const filters = ref<FilterState>({
    categories: [],
    brands: [],
    priceRange: [0, 10000],
    sizes: [],
    colors: [],
    sortBy: 'recommended',
  })

  const hasActiveFilters = computed(() =>
    filters.value.categories.length > 0 ||
    filters.value.brands.length > 0 ||
    filters.value.sizes.length > 0 ||
    filters.value.colors.length > 0 ||
    (filters.value.priceRange[0] !== 0 || filters.value.priceRange[1] !== 10000)
  )

  function toggleCategory(categoryId: string) {
    const idx = filters.value.categories.indexOf(categoryId)
    if (idx === -1) {
      filters.value.categories.push(categoryId)
    } else {
      filters.value.categories.splice(idx, 1)
    }
  }

  function setPriceRange(range: [number, number]) {
    filters.value.priceRange = range
  }

  function setSortBy(sort: FilterState['sortBy']) {
    filters.value.sortBy = sort
  }

  function clearAll() {
    filters.value = {
      categories: [],
      brands: [],
      priceRange: [0, 10000],
      sizes: [],
      colors: [],
      sortBy: 'recommended',
    }
  }

  return {
    filters, hasActiveFilters,
    toggleCategory, setPriceRange, setSortBy, clearAll,
  }
})
```
# Router, Services & Composables

---

## Router

### `src/app/router/index.ts`

```ts
import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useUserStore } from '@/app/stores/ui'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('@/app/layouts/DefaultLayout.vue'),
    children: [
      {
        path: '',
        name: 'home',
        component: () => import('@/features/catalog/views/HomeView.vue'),
        meta: { title: 'Maison — Luxury Fashion' },
      },
      {
        path: 'products',
        name: 'catalog',
        component: () => import('@/features/catalog/views/CatalogView.vue'),
        meta: { title: 'Shop — Maison' },
      },
      {
        path: 'product/:slug',
        name: 'product-detail',
        component: () => import('@/features/catalog/views/ProductDetailView.vue'),
      },
      {
        path: 'wishlist',
        name: 'wishlist',
        component: () => import('@/features/catalog/views/WishlistView.vue'),
        meta: { title: 'Wishlist — Maison', requiresAuth: true },
      },
    ],
  },
  {
    path: '/cart',
    component: () => import('@/app/layouts/DefaultLayout.vue'),
    children: [
      {
        path: '',
        name: 'cart',
        component: () => import('@/features/ordering/views/CartView.vue'),
        meta: { title: 'Your Bag — Maison' },
      },
    ],
  },
  {
    path: '/checkout',
    component: () => import('@/app/layouts/DefaultLayout.vue'),
    meta: { requiresAuth: false },
    children: [
      {
        path: 'shipping',
        name: 'checkout-shipping',
        component: () => import('@/features/ordering/views/CheckoutShippingView.vue'),
        meta: { title: 'Shipping — Maison' },
      },
      {
        path: 'payment',
        name: 'checkout-payment',
        component: () => import('@/features/ordering/views/CheckoutPaymentView.vue'),
        meta: { title: 'Payment — Maison' },
      },
    ],
  },
  {
    path: '/auth',
    component: () => import('@/app/layouts/DefaultLayout.vue'),
    children: [
      {
        path: 'login',
        name: 'login',
        component: () => import('@/features/identity/views/LoginView.vue'),
        meta: { title: 'Sign In — Maison', guestOnly: true },
      },
      {
        path: 'register',
        name: 'register',
        component: () => import('@/features/identity/views/RegisterView.vue'),
        meta: { title: 'Create Account — Maison', guestOnly: true },
      },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// ─── Navigation Guards ────────────────────────────────────────────────────────
router.beforeEach((to, _from, next) => {
  if (to.meta.title) {
    document.title = to.meta.title as string
  }

  const userStore = useUserStore()

  if (to.meta.requiresAuth && !userStore.isLoggedIn) {
    return next({ name: 'login', query: { redirect: to.fullPath } })
  }

  if (to.meta.guestOnly && userStore.isLoggedIn) {
    return next({ name: 'home' })
  }

  next()
})

export default router
```

---

## Core Services

### `src/core/services/api.ts` — HTTP Client Setup

```ts
import axios from 'axios'
import type { AxiosInstance, AxiosError } from 'axios'
import { useUserStore } from '@/app/stores/ui'
import router from '@/app/router'

export interface ApiErrorResponse {
  code: string
  message: string
  details?: Record<string, string[]>
}

const api: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api',
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' },
})

// ─── Request Interceptor ──────────────────────────────────────────────────────
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// ─── Response Interceptor ─────────────────────────────────────────────────────
api.interceptors.response.use(
  response => response,
  (error: AxiosError<ApiErrorResponse>) => {
    if (error.response?.status === 401) {
      const userStore = useUserStore()
      userStore.logout?.()
      void router.push({ name: 'login' })
    }
    return Promise.reject(error)
  }
)

export default api
```

---

## Feature Services

### Catalog Feature Services

#### `src/features/catalog/services/product/getProductList.ts`

```ts
import api from '@/core/services/api'
import type { ProductListItem } from '@/features/catalog/types/product'
import type { PagedResult } from '@/core/models/paging.model'

export interface GetProductListRequest {
  page?: number
  pageSize?: number
  categoryId?: string
  brandId?: string
  sortBy?: string
  priceMin?: number
  priceMax?: number
  searchTerm?: string
}

export async function getProductList(
  params: GetProductListRequest
): Promise<PagedResult<ProductListItem>> {
  const { data } = await api.get('/products', { params })
  return data
}
```

#### `src/features/catalog/services/product/getProductDetail.ts`

```ts
import api from '@/core/services/api'
import type { Product } from '@/features/catalog/types/product'

export async function getProductDetail(slug: string): Promise<Product> {
  const { data } = await api.get(`/products/${slug}`)
  return data.data
}
```

### Ordering Feature Services

#### `src/features/ordering/services/cart/cartService.ts`

```ts
import api from '@/core/services/api'
import type { CartItem, CartSummary } from '@/features/ordering/types/cart'

export const cartService = {
  async list(): Promise<CartItem[]> {
    const { data } = await api.get('/cart/items')
    return data.data
  },

  async addItem(payload: {
    productId: string
    variantId: string
    colorId: string
    quantity: number
  }): Promise<CartItem> {
    const { data } = await api.post('/cart/items', payload)
    return data.data
  },

  async updateQuantity(itemId: string, quantity: number): Promise<CartItem> {
    const { data } = await api.patch(`/cart/items/${itemId}`, { quantity })
    return data.data
  },

  async removeItem(itemId: string): Promise<void> {
    await api.delete(`/cart/items/${itemId}`)
  },

  async getSummary(): Promise<CartSummary> {
    const { data } = await api.get('/cart/summary')
    return data.data
  },
}
```

---

## Composables

### App-Level Composables

#### `src/app/composables/index.ts`

```ts
// Barrel export for app-level composables
export { useNavigation } from './useNavigation'
export { useNewsletter } from './useNewsletter'
export { useScroll } from './useScroll'
```

#### `src/app/composables/useScroll.ts`

```ts
import { ref, onMounted, onUnmounted } from 'vue'

export function useScroll() {
  const scrollY = ref(0)

  function onScroll() {
    scrollY.value = window.scrollY
  }

  onMounted(() => {
    window.addEventListener('scroll', onScroll, { passive: true })
  })

  onUnmounted(() => {
    window.removeEventListener('scroll', onScroll)
  })

  return { scrollY }
}
```

### Feature Composables — Catalog Feature

#### `src/features/catalog/composables/useCatalog.ts`

Main composable for product listing and filtering:

```ts
import { ref, computed } from 'vue'
import { getProductList } from '@/features/catalog/services/product/getProductList'
import { useCatalogStore } from '@/features/catalog/store/catalog'
import { useFiltersStore } from '@/features/catalog/store/filters'
import type { ProductListItem } from '@/features/catalog/types/product'

export function useCatalog() {
  const catalogStore = useCatalogStore()
  const filtersStore = useFiltersStore()

  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchProducts() {
    isLoading.value = true
    error.value = null

    try {
      const result = await getProductList({
        page: catalogStore.currentPage,
        categoryId: filtersStore.filters.categories[0],
        brandId: filtersStore.filters.brands[0],
        sortBy: filtersStore.filters.sortBy,
        priceMin: filtersStore.filters.priceRange[0],
        priceMax: filtersStore.filters.priceRange[1],
      })

      catalogStore.setProducts(result.items, result.totalCount)
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load products'
    } finally {
      isLoading.value = false
    }
  }

  function loadMore() {
    catalogStore.currentPage++
    fetchProducts()
  }

  return {
    products: computed(() => catalogStore.products),
    isLoading,
    error,
    hasMore: computed(() => catalogStore.hasMore),
    fetchProducts,
    loadMore,
  }
}
```

#### `src/features/catalog/composables/useWishlist.ts`

```ts
import { computed } from 'vue'
import { useWishlistStore } from '@/features/catalog/store/wishlist'
import { useToast } from 'primevue/usetoast'
import type { ProductListItem } from '@/features/catalog/types/product'

export function useWishlist() {
  const wishlistStore = useWishlistStore()
  const toast = useToast()

  function toggleWishlist(product: ProductListItem) {
    wishlistStore.toggle(product)
    const action = wishlistStore.isWishlisted(product.id) ? 'Added to' : 'Removed from'
    toast.add({
      severity: 'success',
      summary: `${action} Wishlist`,
      detail: product.name,
      life: 2000,
    })
  }

  return {
    items: computed(() => wishlistStore.items),
    count: computed(() => wishlistStore.count),
    isEmpty: computed(() => wishlistStore.isEmpty),
    isWishlisted: wishlistStore.isWishlisted,
    toggleWishlist,
  }
}
```

### Feature Composables — Ordering Feature

#### `src/features/ordering/composables/useCart.ts`

```ts
import { computed } from 'vue'
import { useOrderingStore } from '@/features/ordering/store/ordering'
import { useToast } from 'primevue/usetoast'
import type { Product, ProductVariant, ColorOption } from '@/features/catalog/types/product'

export function useCart() {
  const orderingStore = useOrderingStore()
  const toast = useToast()

  function addToCart(
    product: Product,
    variant: ProductVariant,
    color: ColorOption,
    qty = 1
  ) {
    if (variant.stock < qty) {
      toast.add({
        severity: 'warn',
        summary: 'Low Stock',
        detail: `Only ${variant.stock} left in stock`,
        life: 3000,
      })
      return
    }

    orderingStore.addItem(product, variant, color, qty)
    toast.add({
      severity: 'success',
      summary: 'Added to Bag',
      detail: `${product.name} — Size ${variant.size}`,
      life: 3000,
    })
  }

  return {
    items: computed(() => orderingStore.items),
    itemCount: computed(() => orderingStore.itemCount),
    isEmpty: computed(() => orderingStore.isEmpty),
    summary: computed(() => orderingStore.summary),
    isOpen: computed(() => orderingStore.isOpen),
    addToCart,
    removeItem: orderingStore.removeItem,
    updateQuantity: orderingStore.updateQuantity,
  }
}
```

### Shared Composables

#### `src/app/composables/useBreakpoint.ts`

```ts
import { ref, onMounted, onUnmounted } from 'vue'

const BREAKPOINTS = { sm: 640, md: 768, lg: 1024, xl: 1280, '2xl': 1536 }

export function useBreakpoint() {
  const width = ref(window.innerWidth)

  const isMobile = () => width.value < BREAKPOINTS.md
  const isTablet = () => width.value >= BREAKPOINTS.md && width.value < BREAKPOINTS.lg
  const isDesktop = () => width.value >= BREAKPOINTS.lg
  const isMd = () => width.value >= BREAKPOINTS.md
  const isLg = () => width.value >= BREAKPOINTS.lg

  function onResize() {
    width.value = window.innerWidth
  }

  onMounted(() => window.addEventListener('resize', onResize, { passive: true }))
  onUnmounted(() => window.removeEventListener('resize', onResize))

  return { width, isMobile, isTablet, isDesktop, isMd, isLg }
}
```

#### `src/app/composables/useCurrency.ts`

```ts
import { computed } from 'vue'

export function useCurrency() {
  const currency = computed(() => 'SGD')

  function format(amount: number): string {
    return new Intl.NumberFormat('en-SG', {
      style: 'currency',
      currency: currency.value,
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(amount)
  }

  function formatCompact(amount: number): string {
    if (amount >= 1000) return `${(amount / 1000).toFixed(1)}k`
    return format(amount)
  }

  return { currency, format, formatCompact }
}
```
# Layout Components

---

## `src/layouts/DefaultLayout.vue`

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppFooter from '@/components/layout/AppFooter.vue'
import CartDrawer from '@/components/feature/cart/CartDrawer.vue'
import SearchOverlay from '@/components/feature/search/SearchOverlay.vue'
import NewsletterPopup from '@/components/ui/NewsletterPopup.vue'
import CookieBanner from '@/components/ui/CookieBanner.vue'
import Toast from 'primevue/toast'
import { useCartStore } from '@/stores/cart'
import { useWishlistStore } from '@/stores/wishlist'
import { useUIStore } from '@/stores/ui'

const cartStore     = useCartStore()
const wishlistStore = useWishlistStore()
const uiStore       = useUIStore()

onMounted(() => {
  cartStore.hydrateCart()
  wishlistStore.hydrate()
  uiStore.hydrateUI()
})
</script>

<template>
  <div class="default-layout">
    <AppHeader />

    <main class="default-layout__main" role="main">
      <RouterView v-slot="{ Component, route }">
        <Transition name="page" mode="out-in">
          <component :is="Component" :key="route.path" />
        </Transition>
      </RouterView>
    </main>

    <AppFooter />

    <!-- Global overlays -->
    <CartDrawer />
    <SearchOverlay />
    <NewsletterPopup v-if="!uiStore.newsletterShown" />
    <CookieBanner v-if="!uiStore.cookieBannerShown" />
    <Toast position="bottom-right" />
  </div>
</template>

<style lang="scss">
.default-layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;

  &__main {
    flex: 1;
  }
}

// ─── Page Transitions ─────────────────────────────────────────────────────────
.page-enter-active,
.page-leave-active {
  transition: opacity $duration-base $ease-default,
              transform $duration-base $ease-default;
}

.page-enter-from { opacity: 0; transform: translateY(8px); }
.page-leave-to   { opacity: 0; transform: translateY(-8px); }
</style>
```

---

## `src/components/layout/AppHeader.vue`

```vue
<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { useCartStore } from '@/stores/cart'
import { useWishlistStore } from '@/stores/wishlist'
import { useUserStore } from '@/stores/user'
import { useUIStore } from '@/stores/ui'
import MegaMenu from '@/components/layout/MegaMenu.vue'
import AccountMenu from '@/components/feature/account/AccountMenu.vue'

const cartStore     = useCartStore()
const wishlistStore = useWishlistStore()
const userStore     = useUserStore()
const uiStore       = useUIStore()
const route         = useRoute()

const scrollY         = ref(0)
const activeNavItem   = ref<string | null>(null)
const hoverTimeout    = ref<ReturnType<typeof setTimeout>>()

const isScrolled    = computed(() => scrollY.value > 60)
const isTransparent = computed(() =>
  route.name === 'home' && !isScrolled.value && !activeNavItem.value
)

const navItems = [
  { id: 'women',     label: 'Women',     href: '/women' },
  { id: 'men',       label: 'Men',       href: '/men' },
  { id: 'kids',      label: 'Kids',      href: '/kids' },
  { id: 'designers', label: 'Designers', href: '/designers' },
  { id: 'new-in',    label: 'New In',    href: '/new-in', highlight: true },
  { id: 'sale',      label: 'Sale',      href: '/sale',   highlight: true, accent: 'sale' },
]

function onNavEnter(id: string) {
  clearTimeout(hoverTimeout.value)
  activeNavItem.value = id
}
function onNavLeave() {
  hoverTimeout.value = setTimeout(() => {
    activeNavItem.value = null
  }, 150)
}

function onScroll() { scrollY.value = window.scrollY }
onMounted(() => window.addEventListener('scroll', onScroll, { passive: true }))
onUnmounted(() => window.removeEventListener('scroll', onScroll))
</script>

<template>
  <header
    class="app-header"
    :class="{
      'app-header--scrolled': isScrolled,
      'app-header--transparent': isTransparent,
      'app-header--menu-open': activeNavItem,
    }"
  >
    <!-- Promo bar -->
    <div class="app-header__promo-bar">
      <p>Complimentary shipping on orders over SGD 500 · <a href="/new-in">Explore New In →</a></p>
    </div>

    <div class="app-header__inner">
      <!-- Logo -->
      <RouterLink to="/" class="app-header__logo" aria-label="Maison — Home">
        <span class="app-header__logo-text">MAISON</span>
      </RouterLink>

      <!-- Primary Navigation -->
      <nav class="app-header__nav" aria-label="Primary navigation">
        <ul class="app-header__nav-list" role="list">
          <li
            v-for="item in navItems"
            :key="item.id"
            class="app-header__nav-item"
            @mouseenter="onNavEnter(item.id)"
            @mouseleave="onNavLeave"
          >
            <RouterLink
              :to="item.href"
              class="app-header__nav-link"
              :class="{
                'app-header__nav-link--highlight': item.highlight,
                [`app-header__nav-link--${item.accent}`]: item.accent,
                'app-header__nav-link--active': activeNavItem === item.id,
              }"
            >
              {{ item.label }}
            </RouterLink>

            <Transition name="mega-menu">
              <MegaMenu
                v-if="activeNavItem === item.id && !['new-in','sale'].includes(item.id)"
                :category="item.id"
                @mouseenter="onNavEnter(item.id)"
                @mouseleave="onNavLeave"
              />
            </Transition>
          </li>
        </ul>
      </nav>

      <!-- Header Actions -->
      <div class="app-header__actions">
        <!-- Search -->
        <button
          class="app-header__icon-btn"
          aria-label="Search"
          @click="uiStore.openSearch()"
        >
          <i class="pi pi-search" />
        </button>

        <!-- Account -->
        <AccountMenu v-if="userStore.isLoggedIn" />
        <RouterLink v-else to="/auth/login" class="app-header__icon-btn" aria-label="Sign in">
          <i class="pi pi-user" />
        </RouterLink>

        <!-- Wishlist -->
        <RouterLink to="/wishlist" class="app-header__icon-btn app-header__icon-btn--badge" aria-label="Wishlist">
          <i class="pi pi-heart" />
          <span v-if="wishlistStore.count > 0" class="app-header__badge">
            {{ wishlistStore.count }}
          </span>
        </RouterLink>

        <!-- Cart -->
        <button
          class="app-header__icon-btn app-header__icon-btn--badge"
          aria-label="Shopping bag"
          @click="cartStore.isOpen = true"
        >
          <i class="pi pi-shopping-bag" />
          <span v-if="cartStore.itemCount > 0" class="app-header__badge">
            {{ cartStore.itemCount }}
          </span>
        </button>

        <!-- Mobile hamburger -->
        <button
          class="app-header__hamburger"
          aria-label="Open menu"
          @click="uiStore.toggleMobileNav()"
        >
          <span /><span /><span />
        </button>
      </div>
    </div>

    <!-- Mega menu backdrop -->
    <Transition name="backdrop">
      <div v-if="activeNavItem" class="app-header__backdrop" @mouseenter="onNavLeave" />
    </Transition>
  </header>
</template>

<style lang="scss" scoped>
.app-header {
  position: sticky;
  top: 0;
  z-index: 100;
  background: var(--color-bg-canvas);
  border-bottom: 1px solid var(--color-border);
  @include transition(background border-color box-shadow, $duration-slow);

  &--scrolled {
    box-shadow: $shadow-sm;
  }

  &--transparent {
    background: transparent;
    border-color: transparent;

    .app-header__nav-link,
    .app-header__icon-btn,
    .app-header__logo-text {
      color: white;
    }
  }

  &__promo-bar {
    background: var(--color-invert-bg);
    color: var(--color-invert-text);
    text-align: center;
    padding: $space-2 $space-4;
    font-size: $text-xs;
    letter-spacing: $tracking-wide;

    a {
      color: var(--color-invert-text);
      text-decoration: underline;
      text-underline-offset: 2px;
    }
  }

  &__inner {
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: 64px;
    padding: 0 $space-6;
    gap: $space-8;

    @include respond-to('xl') {
      padding: 0 $space-12;
    }
  }

  &__logo-text {
    font-family: $font-display;
    font-size: $text-xl;
    font-weight: $weight-light;
    letter-spacing: $tracking-widest;
    color: var(--color-text-primary);
  }

  &__nav {
    display: none;

    @include respond-to('lg') {
      display: block;
    }
  }

  &__nav-list {
    display: flex;
    align-items: center;
    gap: $space-8;
    list-style: none;
    margin: 0;
    padding: 0;
  }

  &__nav-link {
    @include label-caps;
    color: var(--color-text-primary);
    text-decoration: none;
    position: relative;
    padding: $space-2 0;
    @include transition(color);

    &::after {
      content: '';
      position: absolute;
      bottom: 0;
      left: 0;
      width: 0;
      height: 1px;
      background: currentColor;
      @include transition(width);
    }

    &:hover::after,
    &--active::after { width: 100%; }

    &--sale { color: $color-error; }
  }

  &__actions {
    display: flex;
    align-items: center;
    gap: $space-3;
  }

  &__icon-btn {
    position: relative;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 40px;
    color: var(--color-text-primary);
    background: transparent;
    border: none;
    cursor: pointer;
    @include transition(color);

    .pi { font-size: 18px; }

    &:hover { color: var(--color-accent); }
  }

  &__badge {
    position: absolute;
    top: 4px;
    right: 4px;
    min-width: 16px;
    height: 16px;
    padding: 0 4px;
    background: var(--color-accent);
    color: white;
    border-radius: 999px;
    font-size: 10px;
    font-weight: $weight-semibold;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  &__hamburger {
    display: flex;
    flex-direction: column;
    gap: 4px;
    padding: $space-2;
    background: none;
    border: none;
    cursor: pointer;

    @include respond-to('lg') { display: none; }

    span {
      display: block;
      width: 22px;
      height: 1.5px;
      background: var(--color-text-primary);
      @include transition(transform opacity);
    }
  }

  &__backdrop {
    position: fixed;
    inset: 0;
    top: 100%;
    background: rgb(0 0 0 / 0.3);
    backdrop-filter: blur(2px);
    z-index: -1;
  }
}

// Mega menu transition
.mega-menu-enter-active,
.mega-menu-leave-active {
  transition: opacity $duration-fast $ease-default,
              transform $duration-fast $ease-default;
}
.mega-menu-enter-from,
.mega-menu-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

.backdrop-enter-active,
.backdrop-leave-active { transition: opacity $duration-base $ease-default; }
.backdrop-enter-from,
.backdrop-leave-to { opacity: 0; }
</style>
```

---

## `src/components/layout/MegaMenu.vue`

```vue
<script setup lang="ts">
defineProps<{ category: string }>()

const menus: Record<string, {
  columns: { title: string; links: { label: string; href: string }[] }[]
  featured: { image: string; title: string; href: string; tag: string }[]
}> = {
  women: {
    columns: [
      {
        title: 'Clothing',
        links: [
          { label: 'Dresses',     href: '/women/dresses' },
          { label: 'Tops',        href: '/women/tops' },
          { label: 'Trousers',    href: '/women/trousers' },
          { label: 'Jackets & Coats', href: '/women/jackets' },
          { label: 'Knitwear',    href: '/women/knitwear' },
          { label: 'Jumpsuits',   href: '/women/jumpsuits' },
          { label: 'Skirts',      href: '/women/skirts' },
          { label: 'Swimwear',    href: '/women/swimwear' },
        ],
      },
      {
        title: 'Bags',
        links: [
          { label: 'Tote Bags',    href: '/women/totes' },
          { label: 'Shoulder Bags', href: '/women/shoulder-bags' },
          { label: 'Clutches',     href: '/women/clutches' },
          { label: 'Crossbody',    href: '/women/crossbody' },
          { label: 'Backpacks',    href: '/women/backpacks' },
          { label: 'Mini Bags',    href: '/women/mini-bags' },
        ],
      },
      {
        title: 'Shoes',
        links: [
          { label: 'Heels',    href: '/women/heels' },
          { label: 'Flats',    href: '/women/flats' },
          { label: 'Boots',    href: '/women/boots' },
          { label: 'Sneakers', href: '/women/sneakers' },
          { label: 'Sandals',  href: '/women/sandals' },
          { label: 'Loafers',  href: '/women/loafers' },
        ],
      },
      {
        title: 'Accessories',
        links: [
          { label: 'Jewellery',  href: '/women/jewellery' },
          { label: 'Sunglasses', href: '/women/sunglasses' },
          { label: 'Scarves',    href: '/women/scarves' },
          { label: 'Belts',      href: '/women/belts' },
          { label: 'Hats',       href: '/women/hats' },
          { label: 'Watches',    href: '/women/watches' },
        ],
      },
    ],
    featured: [
      { image: 'https://picsum.photos/seed/fw1/400/500', title: 'New Season', tag: 'Just Arrived', href: '/women/new-in' },
      { image: 'https://picsum.photos/seed/fw2/400/500', title: 'Resort Edit', tag: 'Curated', href: '/women/resort' },
    ],
  },
  men: {
    columns: [
      {
        title: 'Clothing',
        links: [
          { label: 'Shirts',     href: '/men/shirts' },
          { label: 'T-Shirts',   href: '/men/t-shirts' },
          { label: 'Trousers',   href: '/men/trousers' },
          { label: 'Jackets',    href: '/men/jackets' },
          { label: 'Knitwear',   href: '/men/knitwear' },
          { label: 'Suits',      href: '/men/suits' },
          { label: 'Shorts',     href: '/men/shorts' },
        ],
      },
      {
        title: 'Bags',
        links: [
          { label: 'Briefcases',  href: '/men/briefcases' },
          { label: 'Backpacks',   href: '/men/backpacks' },
          { label: 'Tote Bags',   href: '/men/totes' },
          { label: 'Messenger',   href: '/men/messenger' },
        ],
      },
      {
        title: 'Shoes',
        links: [
          { label: 'Oxford',    href: '/men/oxford' },
          { label: 'Sneakers',  href: '/men/sneakers' },
          { label: 'Boots',     href: '/men/boots' },
          { label: 'Loafers',   href: '/men/loafers' },
          { label: 'Sandals',   href: '/men/sandals' },
        ],
      },
      {
        title: 'Accessories',
        links: [
          { label: 'Watches',    href: '/men/watches' },
          { label: 'Sunglasses', href: '/men/sunglasses' },
          { label: 'Ties',       href: '/men/ties' },
          { label: 'Belts',      href: '/men/belts' },
          { label: 'Wallets',    href: '/men/wallets' },
        ],
      },
    ],
    featured: [
      { image: 'https://picsum.photos/seed/mw1/400/500', title: 'Tailoring Edit', tag: 'New In', href: '/men/new-in' },
      { image: 'https://picsum.photos/seed/mw2/400/500', title: 'Weekend Wear', tag: 'Curated', href: '/men/casual' },
    ],
  },
}
</script>

<template>
  <div class="mega-menu" role="dialog" :aria-label="`${category} navigation`">
    <div class="mega-menu__inner">
      <!-- Link columns -->
      <div class="mega-menu__columns">
        <div
          v-for="col in menus[category]?.columns"
          :key="col.title"
          class="mega-menu__column"
        >
          <h3 class="mega-menu__col-title">{{ col.title }}</h3>
          <ul class="mega-menu__col-list" role="list">
            <li v-for="link in col.links" :key="link.label">
              <RouterLink :to="link.href" class="mega-menu__link">
                {{ link.label }}
              </RouterLink>
            </li>
          </ul>
        </div>
      </div>

      <!-- Featured editorial images -->
      <div class="mega-menu__featured">
        <RouterLink
          v-for="item in menus[category]?.featured"
          :key="item.title"
          :to="item.href"
          class="mega-menu__featured-item"
        >
          <div class="mega-menu__featured-image">
            <img :src="item.image" :alt="item.title" loading="lazy" />
            <span class="mega-menu__featured-tag">{{ item.tag }}</span>
          </div>
          <p class="mega-menu__featured-title">{{ item.title }}</p>
        </RouterLink>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.mega-menu {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: var(--color-bg-canvas);
  border-top: 1px solid var(--color-border);
  border-bottom: 1px solid var(--color-border);
  box-shadow: $shadow-lg;
  z-index: 99;

  &__inner {
    display: grid;
    grid-template-columns: 1fr 340px;
    gap: $space-12;
    max-width: 1400px;
    margin: 0 auto;
    padding: $space-10 $space-12;
  }

  &__columns {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: $space-8;
  }

  &__col-title {
    @include label-caps;
    color: var(--color-text-primary);
    margin-bottom: $space-4;
    padding-bottom: $space-3;
    border-bottom: 1px solid var(--color-border);
  }

  &__col-list { list-style: none; margin: 0; padding: 0; }

  &__link {
    display: block;
    font-size: $text-sm;
    color: var(--color-text-secondary);
    text-decoration: none;
    padding: $space-1 0;
    @include transition(color);

    &:hover { color: var(--color-text-primary); }
  }

  &__featured {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: $space-4;
  }

  &__featured-item {
    text-decoration: none;
  }

  &__featured-image {
    position: relative;
    overflow: hidden;
    margin-bottom: $space-3;

    img {
      width: 100%;
      aspect-ratio: 4/5;
      object-fit: cover;
      @include transition(transform, $duration-slow);
    }

    &:hover img { transform: scale(1.04); }
  }

  &__featured-tag {
    position: absolute;
    top: $space-3;
    left: $space-3;
    @include label-caps;
    background: var(--color-bg-canvas);
    color: var(--color-text-primary);
    padding: $space-1 $space-3;
  }

  &__featured-title {
    font-family: $font-display;
    font-size: $text-md;
    font-weight: $weight-light;
    color: var(--color-text-primary);
  }
}
</style>
```

---

## `src/components/feature/cart/CartDrawer.vue`

```vue
<script setup lang="ts">
import Drawer from 'primevue/drawer'
import { useCartStore } from '@/stores/cart'
import { useCurrency } from '@/composables/useCurrency'
import CartItem from './CartItem.vue'
import CartSummaryPanel from './CartSummaryPanel.vue'
import PromoCodeInput from './PromoCodeInput.vue'
import FreeShippingBar from './FreeShippingBar.vue'

const cartStore = useCartStore()
const { format }  = useCurrency()
</script>

<template>
  <Drawer
    v-model:visible="cartStore.isOpen"
    position="right"
    class="cart-drawer"
    :pt="{ root: { class: 'cart-drawer__root' }, mask: { class: 'cart-drawer__mask' } }"
  >
    <template #header>
      <div class="cart-drawer__header">
        <h2 class="cart-drawer__title">Your Bag</h2>
        <span class="cart-drawer__count">
          {{ cartStore.itemCount }} {{ cartStore.itemCount === 1 ? 'item' : 'items' }}
        </span>
      </div>
    </template>

    <!-- Empty State -->
    <div v-if="cartStore.isEmpty" class="cart-drawer__empty">
      <div class="cart-drawer__empty-icon">
        <i class="pi pi-shopping-bag" />
      </div>
      <p class="cart-drawer__empty-title">Your bag is empty</p>
      <p class="cart-drawer__empty-sub">Discover our latest arrivals</p>
      <RouterLink
        to="/new-in"
        class="cart-drawer__empty-cta"
        @click="cartStore.isOpen = false"
      >
        Shop New In
      </RouterLink>
    </div>

    <!-- Cart Items -->
    <template v-else>
      <FreeShippingBar :summary="cartStore.summary" />

      <div class="cart-drawer__items">
        <CartItem
          v-for="item in cartStore.items"
          :key="item.id"
          :item="item"
          @remove="cartStore.removeItem(item.id)"
          @update-quantity="(qty) => cartStore.updateQuantity(item.id, qty)"
        />
      </div>

      <PromoCodeInput />

      <CartSummaryPanel :summary="cartStore.summary" />

      <div class="cart-drawer__actions">
        <RouterLink
          to="/checkout"
          class="cart-drawer__checkout-btn"
          @click="cartStore.isOpen = false"
        >
          Proceed to Checkout · {{ format(cartStore.summary.total) }}
        </RouterLink>
        <button
          class="cart-drawer__continue-btn"
          @click="cartStore.isOpen = false"
        >
          Continue Shopping
        </button>
      </div>
    </template>
  </Drawer>
</template>

<style lang="scss">
.cart-drawer {
  &__root {
    width: 100% !important;
    max-width: 480px !important;
    border-radius: 0 !important;
  }

  &__header {
    display: flex;
    align-items: baseline;
    gap: $space-3;
  }

  &__title {
    font-family: $font-display;
    font-size: $text-xl;
    font-weight: $weight-light;
    color: var(--color-text-primary);
  }

  &__count {
    @include label-caps;
    color: var(--color-text-muted);
  }

  &__empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: $space-16 $space-6;
    text-align: center;
    gap: $space-4;
  }

  &__empty-icon .pi {
    font-size: 48px;
    color: var(--color-border-strong);
  }

  &__empty-title {
    font-family: $font-display;
    font-size: $text-lg;
    font-weight: $weight-light;
    color: var(--color-text-primary);
  }

  &__empty-sub {
    font-size: $text-sm;
    color: var(--color-text-muted);
  }

  &__empty-cta {
    @include label-caps;
    display: inline-block;
    padding: $space-3 $space-8;
    background: var(--color-text-primary);
    color: var(--color-bg-canvas);
    text-decoration: none;
    @include transition(background);

    &:hover { background: var(--color-accent); }
  }

  &__items {
    display: flex;
    flex-direction: column;
    gap: $space-6;
    padding: $space-6 0;
    border-bottom: 1px solid var(--color-border);
  }

  &__actions {
    display: flex;
    flex-direction: column;
    gap: $space-3;
    padding: $space-6 0;
  }

  &__checkout-btn {
    @include label-caps;
    display: block;
    text-align: center;
    padding: $space-4 $space-6;
    background: var(--color-text-primary);
    color: var(--color-bg-canvas);
    text-decoration: none;
    @include transition(background);

    &:hover { background: var(--color-accent); }
  }

  &__continue-btn {
    @include label-caps;
    display: block;
    text-align: center;
    padding: $space-3;
    background: none;
    color: var(--color-text-muted);
    border: none;
    cursor: pointer;
    text-decoration: underline;
    text-underline-offset: 3px;
  }
}
</style>
```

---

## `src/components/feature/cart/CartItem.vue`

```vue
<script setup lang="ts">
import type { CartItem } from '@/types/cart'
import { useCurrency } from '@/composables/useCurrency'

const props = defineProps<{ item: CartItem }>()
const emit = defineEmits<{
  remove: []
  updateQuantity: [qty: number]
}>()

const { format } = useCurrency()
</script>

<template>
  <article class="cart-item">
    <RouterLink :to="`/product/${item.product.slug}`" class="cart-item__image-link">
      <img :src="item.image" :alt="item.product.name" class="cart-item__image" />
    </RouterLink>

    <div class="cart-item__info">
      <div class="cart-item__header">
        <div>
          <p class="cart-item__brand">{{ item.product.brand.name }}</p>
          <p class="cart-item__name">{{ item.product.name }}</p>
        </div>
        <button class="cart-item__remove" aria-label="Remove item" @click="emit('remove')">
          <i class="pi pi-times" />
        </button>
      </div>

      <div class="cart-item__meta">
        <span class="cart-item__attr">Size: {{ item.variant.size }}</span>
        <span class="cart-item__attr">
          <span
            class="cart-item__color-dot"
            :style="{ background: item.color.hex }"
          />
          {{ item.color.name }}
        </span>
      </div>

      <div class="cart-item__footer">
        <div class="cart-item__qty">
          <button
            :disabled="item.quantity <= 1"
            @click="emit('updateQuantity', item.quantity - 1)"
          >−</button>
          <span>{{ item.quantity }}</span>
          <button
            :disabled="item.quantity >= item.variant.stock"
            @click="emit('updateQuantity', item.quantity + 1)"
          >+</button>
        </div>
        <p class="cart-item__price">{{ format(item.price * item.quantity) }}</p>
      </div>
    </div>
  </article>
</template>

<style lang="scss" scoped>
.cart-item {
  display: grid;
  grid-template-columns: 100px 1fr;
  gap: $space-4;

  &__image {
    width: 100%;
    aspect-ratio: 3/4;
    object-fit: cover;
    background: var(--color-bg-elevated);
  }

  &__header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: $space-2;
    margin-bottom: $space-2;
  }

  &__brand {
    @include label-caps;
    font-size: 10px;
    color: var(--color-text-primary);
  }

  &__name {
    font-size: $text-sm;
    color: var(--color-text-secondary);
    @include truncate(2);
  }

  &__remove {
    background: none;
    border: none;
    cursor: pointer;
    color: var(--color-text-muted);
    padding: 4px;
    flex-shrink: 0;
    @include transition(color);

    &:hover { color: $color-error; }
    .pi { font-size: 12px; }
  }

  &__meta {
    display: flex;
    flex-wrap: wrap;
    gap: $space-3;
    margin-bottom: $space-3;
  }

  &__attr {
    font-size: $text-xs;
    color: var(--color-text-muted);
    display: flex;
    align-items: center;
    gap: $space-1;
  }

  &__color-dot {
    width: 12px;
    height: 12px;
    border-radius: 50%;
    border: 1px solid var(--color-border);
    display: inline-block;
  }

  &__footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-top: auto;
  }

  &__qty {
    display: flex;
    align-items: center;
    gap: $space-3;
    border: 1px solid var(--color-border);
    padding: $space-1 $space-2;

    button {
      background: none;
      border: none;
      cursor: pointer;
      font-size: $text-md;
      line-height: 1;
      color: var(--color-text-primary);
      padding: 0 4px;

      &:disabled {
        color: var(--color-border-strong);
        cursor: not-allowed;
      }
    }

    span {
      font-size: $text-sm;
      min-width: 20px;
      text-align: center;
    }
  }

  &__price {
    font-size: $text-sm;
    font-weight: $weight-medium;
    color: var(--color-text-primary);
  }
}
</style>
```

---

## `src/components/feature/search/SearchOverlay.vue`

```vue
<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useUIStore } from '@/stores/ui'
import { useScrollLock } from '@/composables/useScrollLock'
import { productsService } from '@/services/products'
import type { ProductListItem } from '@/types/product'

const uiStore    = useUIStore()
const router     = useRouter()
const { lock, unlock } = useScrollLock()

const query       = ref('')
const results     = ref<ProductListItem[]>([])
const isSearching = ref(false)
const searchInput = ref<HTMLInputElement>()

const trendingSearches = ['Loewe', 'Bottega Veneta', 'Maxi Dress', 'Loafers', 'Linen Shirt', 'Canvas Tote']

watch(() => uiStore.searchOpen, async (open) => {
  if (open) {
    lock()
    await new Promise(r => setTimeout(r, 50))
    searchInput.value?.focus()
  } else {
    unlock()
    query.value  = ''
    results.value = []
  }
})

let debounceTimer: ReturnType<typeof setTimeout>
watch(query, async (val) => {
  clearTimeout(debounceTimer)
  if (val.trim().length < 2) { results.value = []; return }
  isSearching.value = true
  debounceTimer = setTimeout(async () => {
    try {
      const res = await productsService.search(val.trim(), { perPage: 6 })
      results.value = res.data
    } finally {
      isSearching.value = false
    }
  }, 300)
})

function submitSearch() {
  if (!query.value.trim()) return
  void router.push({ name: 'search', query: { q: query.value.trim() } })
  uiStore.closeSearch()
}

function selectTrending(term: string) {
  query.value = term
  void submitSearch()
}
</script>

<template>
  <Teleport to="body">
    <Transition name="search-overlay">
      <div
        v-if="uiStore.searchOpen"
        class="search-overlay"
        role="dialog"
        aria-label="Search"
        aria-modal="true"
      >
        <div class="search-overlay__inner">
          <!-- Search Input -->
          <div class="search-overlay__input-wrap">
            <i class="pi pi-search search-overlay__input-icon" />
            <input
              ref="searchInput"
              v-model="query"
              type="search"
              placeholder="Search designers, styles, categories..."
              class="search-overlay__input"
              @keydown.enter="submitSearch"
              @keydown.esc="uiStore.closeSearch()"
            />
            <button class="search-overlay__close" @click="uiStore.closeSearch()">
              <i class="pi pi-times" />
            </button>
          </div>

          <!-- Trending (empty state) -->
          <div v-if="!query && !results.length" class="search-overlay__trending">
            <p class="search-overlay__section-title">Trending Searches</p>
            <div class="search-overlay__trending-tags">
              <button
                v-for="term in trendingSearches"
                :key="term"
                class="search-overlay__tag"
                @click="selectTrending(term)"
              >
                {{ term }}
              </button>
            </div>
          </div>

          <!-- Loading -->
          <div v-else-if="isSearching" class="search-overlay__loading">
            <i class="pi pi-spin pi-spinner" />
          </div>

          <!-- Results -->
          <div v-else-if="results.length" class="search-overlay__results">
            <p class="search-overlay__section-title">
              Products for "{{ query }}"
            </p>
            <div class="search-overlay__result-grid">
              <RouterLink
                v-for="product in results"
                :key="product.id"
                :to="`/product/${product.slug}`"
                class="search-overlay__result-item"
                @click="uiStore.closeSearch()"
              >
                <img
                  :src="product.images[0]?.url"
                  :alt="product.name"
                  class="search-overlay__result-image"
                />
                <div class="search-overlay__result-info">
                  <p class="search-overlay__result-brand">{{ product.brand.name }}</p>
                  <p class="search-overlay__result-name">{{ product.name }}</p>
                  <p class="search-overlay__result-price">
                    SGD {{ product.price.toLocaleString() }}
                  </p>
                </div>
              </RouterLink>
            </div>
            <button class="search-overlay__view-all" @click="submitSearch">
              View all results for "{{ query }}" →
            </button>
          </div>

          <!-- No results -->
          <div v-else-if="query.length >= 2" class="search-overlay__no-results">
            <p>No results for "{{ query }}"</p>
            <p class="search-overlay__no-results-sub">Try a different search term or browse our categories</p>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style lang="scss" scoped>
.search-overlay {
  position: fixed;
  inset: 0;
  background: var(--color-bg-canvas);
  z-index: 200;
  overflow-y: auto;

  &__inner {
    max-width: 900px;
    margin: 0 auto;
    padding: $space-8 $space-6;

    @include respond-to('lg') {
      padding: $space-12 $space-8;
    }
  }

  &__input-wrap {
    display: flex;
    align-items: center;
    gap: $space-4;
    border-bottom: 2px solid var(--color-text-primary);
    padding-bottom: $space-4;
    margin-bottom: $space-10;
  }

  &__input-icon {
    font-size: 20px;
    color: var(--color-text-muted);
    flex-shrink: 0;
  }

  &__input {
    flex: 1;
    font-family: $font-display;
    font-size: $text-2xl;
    font-weight: $weight-light;
    color: var(--color-text-primary);
    background: none;
    border: none;
    outline: none;

    &::placeholder { color: var(--color-border-strong); }
  }

  &__close {
    background: none;
    border: none;
    cursor: pointer;
    color: var(--color-text-muted);
    padding: $space-2;
    @include transition(color);

    &:hover { color: var(--color-text-primary); }
    .pi { font-size: 20px; }
  }

  &__section-title {
    @include label-caps;
    color: var(--color-text-muted);
    margin-bottom: $space-4;
  }

  &__trending-tags {
    display: flex;
    flex-wrap: wrap;
    gap: $space-2;
  }

  &__tag {
    padding: $space-2 $space-4;
    border: 1px solid var(--color-border);
    background: none;
    font-size: $text-sm;
    color: var(--color-text-primary);
    cursor: pointer;
    @include transition(background color border-color);

    &:hover {
      background: var(--color-text-primary);
      color: var(--color-bg-canvas);
      border-color: var(--color-text-primary);
    }
  }

  &__loading {
    display: flex;
    justify-content: center;
    padding: $space-12;
    color: var(--color-text-muted);
    .pi { font-size: 28px; }
  }

  &__result-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: $space-4;
    margin-bottom: $space-6;

    @include respond-to('md') {
      grid-template-columns: repeat(3, 1fr);
    }
  }

  &__result-item {
    display: flex;
    gap: $space-3;
    text-decoration: none;
    @include transition(opacity);

    &:hover { opacity: 0.7; }
  }

  &__result-image {
    width: 60px;
    height: 80px;
    object-fit: cover;
    flex-shrink: 0;
    background: var(--color-bg-elevated);
  }

  &__result-brand {
    @include label-caps;
    font-size: 10px;
    color: var(--color-text-primary);
    margin-bottom: 2px;
  }

  &__result-name {
    font-size: $text-xs;
    color: var(--color-text-secondary);
    @include truncate(2);
    margin-bottom: $space-1;
  }

  &__result-price {
    font-size: $text-xs;
    font-weight: $weight-medium;
    color: var(--color-text-primary);
  }

  &__view-all {
    @include label-caps;
    background: none;
    border: none;
    cursor: pointer;
    color: var(--color-accent);
    text-decoration: underline;
    text-underline-offset: 3px;
    padding: 0;
  }

  &__no-results {
    text-align: center;
    padding: $space-12;
    font-family: $font-display;
    font-size: $text-lg;
    color: var(--color-text-secondary);
  }

  &__no-results-sub {
    font-size: $text-sm;
    color: var(--color-text-muted);
    margin-top: $space-2;
  }
}

.search-overlay-enter-active,
.search-overlay-leave-active {
  transition: opacity $duration-base $ease-default;
}
.search-overlay-enter-from,
.search-overlay-leave-to { opacity: 0; }
</style>
```

---

## `src/components/layout/AppFooter.vue`

```vue
<script setup lang="ts">
const year = new Date().getFullYear()

const footerLinks = {
  Shop: [
    { label: 'Women',     href: '/women' },
    { label: 'Men',       href: '/men' },
    { label: 'Kids',      href: '/kids' },
    { label: 'Designers', href: '/designers' },
    { label: 'New In',    href: '/new-in' },
    { label: 'Sale',      href: '/sale' },
  ],
  Help: [
    { label: 'FAQ',             href: '/help/faq' },
    { label: 'Shipping & Returns', href: '/help/shipping' },
    { label: 'Size Guide',      href: '/help/size-guide' },
    { label: 'Order Tracking',  href: '/help/tracking' },
    { label: 'Contact Us',      href: '/help/contact' },
  ],
  Company: [
    { label: 'About Maison',    href: '/about' },
    { label: 'Sustainability',  href: '/sustainability' },
    { label: 'Press',           href: '/press' },
    { label: 'Careers',         href: '/careers' },
    { label: 'Affiliates',      href: '/affiliates' },
  ],
}
</script>

<template>
  <footer class="app-footer">
    <div class="app-footer__inner">
      <!-- Brand + newsletter -->
      <div class="app-footer__brand-col">
        <span class="app-footer__logo">MAISON</span>
        <p class="app-footer__tagline">
          The world's most exceptional fashion, curated for the discerning eye.
        </p>
        <form class="app-footer__newsletter" @submit.prevent>
          <input
            type="email"
            placeholder="Your email address"
            class="app-footer__newsletter-input"
          />
          <button type="submit" class="app-footer__newsletter-btn">Join</button>
        </form>
        <p class="app-footer__newsletter-note">
          Subscribe for early access to new arrivals & private sales.
        </p>
      </div>

      <!-- Link columns -->
      <div
        v-for="(links, title) in footerLinks"
        :key="title"
        class="app-footer__col"
      >
        <h3 class="app-footer__col-title">{{ title }}</h3>
        <ul class="app-footer__col-list" role="list">
          <li v-for="link in links" :key="link.label">
            <RouterLink :to="link.href" class="app-footer__link">
              {{ link.label }}
            </RouterLink>
          </li>
        </ul>
      </div>
    </div>

    <div class="app-footer__bottom">
      <p class="app-footer__copy">
        © {{ year }} Maison Pte. Ltd. All rights reserved.
      </p>
      <div class="app-footer__legal">
        <RouterLink to="/privacy">Privacy Policy</RouterLink>
        <RouterLink to="/terms">Terms of Service</RouterLink>
        <RouterLink to="/cookies">Cookie Policy</RouterLink>
      </div>
      <div class="app-footer__payment-icons">
        <span class="app-footer__payment-icon">VISA</span>
        <span class="app-footer__payment-icon">MC</span>
        <span class="app-footer__payment-icon">AMEX</span>
        <span class="app-footer__payment-icon">PayPal</span>
        <span class="app-footer__payment-icon">Apple Pay</span>
      </div>
    </div>
  </footer>
</template>

<style lang="scss" scoped>
.app-footer {
  background: var(--color-invert-bg);
  color: var(--color-invert-text);
  margin-top: $space-20;

  &__inner {
    display: grid;
    grid-template-columns: 1fr;
    gap: $space-10;
    max-width: 1400px;
    margin: 0 auto;
    padding: $space-16 $space-6;

    @include respond-to('md') { grid-template-columns: repeat(2, 1fr); }
    @include respond-to('lg') { grid-template-columns: 2fr repeat(3, 1fr); gap: $space-12; }
    @include respond-to('xl') { padding: $space-20 $space-12; }
  }

  &__logo {
    font-family: $font-display;
    font-size: $text-2xl;
    font-weight: $weight-light;
    letter-spacing: $tracking-widest;
    display: block;
    margin-bottom: $space-4;
  }

  &__tagline {
    font-size: $text-sm;
    color: rgb(255 255 255 / 0.6);
    line-height: $leading-relaxed;
    max-width: 280px;
    margin-bottom: $space-6;
  }

  &__newsletter {
    display: flex;
    border-bottom: 1px solid rgb(255 255 255 / 0.3);
    margin-bottom: $space-3;

    &-input {
      flex: 1;
      background: none;
      border: none;
      outline: none;
      color: white;
      font-size: $text-sm;
      padding: $space-2 0;

      &::placeholder { color: rgb(255 255 255 / 0.4); }
    }

    &-btn {
      @include label-caps;
      background: none;
      border: none;
      color: white;
      cursor: pointer;
      padding: $space-2 0 $space-2 $space-4;
      @include transition(color);

      &:hover { color: var(--color-accent); }
    }
  }

  &__newsletter-note {
    font-size: $text-xs;
    color: rgb(255 255 255 / 0.4);
  }

  &__col-title {
    @include label-caps;
    color: rgb(255 255 255 / 0.5);
    margin-bottom: $space-5;
  }

  &__col-list { list-style: none; margin: 0; padding: 0; }

  &__link {
    display: block;
    font-size: $text-sm;
    color: rgb(255 255 255 / 0.7);
    text-decoration: none;
    padding: $space-1 0;
    @include transition(color);

    &:hover { color: white; }
  }

  &__bottom {
    border-top: 1px solid rgb(255 255 255 / 0.1);
    max-width: 1400px;
    margin: 0 auto;
    padding: $space-6;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: $space-4;
    justify-content: space-between;

    @include respond-to('xl') { padding: $space-6 $space-12; }
  }

  &__copy {
    font-size: $text-xs;
    color: rgb(255 255 255 / 0.4);
  }

  &__legal {
    display: flex;
    gap: $space-4;

    a {
      font-size: $text-xs;
      color: rgb(255 255 255 / 0.4);
      text-decoration: none;
      @include transition(color);
      &:hover { color: white; }
    }
  }

  &__payment-icons {
    display: flex;
    gap: $space-2;
    flex-wrap: wrap;
  }

  &__payment-icon {
    @include label-caps;
    font-size: 9px;
    padding: 3px 8px;
    border: 1px solid rgb(255 255 255 / 0.2);
    color: rgb(255 255 255 / 0.5);
    border-radius: 2px;
  }
}
</style>
```
# All Pages & Views

---

## Homepage `src/pages/index.vue`

```vue
<script setup lang="ts">
import { useQuery } from '@tanstack/vue-query'
import { productsService } from '@/services/products'
import HeroBanner from '@/components/feature/homepage/HeroBanner.vue'
import FeaturedCategories from '@/components/feature/homepage/FeaturedCategories.vue'
import ProductCarousel from '@/components/ui/ProductCarousel.vue'
import EditorialGrid from '@/components/feature/homepage/EditorialGrid.vue'
import BrandStrip from '@/components/feature/homepage/BrandStrip.vue'
import LookbookTeaser from '@/components/feature/homepage/LookbookTeaser.vue'
import NewsletterBanner from '@/components/ui/NewsletterBanner.vue'

const { data: newIn }   = useQuery({ queryKey: ['new-in'],  queryFn: () => productsService.getNewIn(12) })
const { data: onSale }  = useQuery({ queryKey: ['on-sale'], queryFn: () => productsService.getSale(12) })
</script>

<template>
  <div class="home-page">
    <HeroBanner />
    <FeaturedCategories />

    <ProductCarousel
      title="New In"
      subtitle="The latest arrivals, curated daily"
      :products="newIn ?? []"
      cta-text="View All New In"
      cta-href="/new-in"
    />

    <EditorialGrid />
    <BrandStrip />

    <ProductCarousel
      title="On Sale"
      subtitle="Exceptional pieces at exceptional prices"
      :products="onSale ?? []"
      cta-text="Shop the Sale"
      cta-href="/sale"
      accent="sale"
    />

    <LookbookTeaser />
    <NewsletterBanner />
  </div>
</template>
```

---

## `src/components/feature/homepage/HeroBanner.vue`

```vue
<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

const slides = [
  {
    id: 1,
    image: 'https://picsum.photos/seed/hero1/1800/900',
    eyebrow: 'New Season',
    title: 'Dressed\nfor Everything',
    subtitle: 'Spring / Summer 2025',
    cta: { label: 'Explore Women', href: '/women' },
    ctaSecondary: { label: 'Explore Men', href: '/men' },
    align: 'left',
  },
  {
    id: 2,
    image: 'https://picsum.photos/seed/hero2/1800/900',
    eyebrow: 'Exclusive Edit',
    title: 'The Art\nof Luxury',
    subtitle: 'Curated Designer Pieces',
    cta: { label: 'Shop Designers', href: '/designers' },
    align: 'center',
  },
  {
    id: 3,
    image: 'https://picsum.photos/seed/hero3/1800/900',
    eyebrow: 'Sale',
    title: 'Up to\n60% Off',
    subtitle: 'On Selected Designer Pieces',
    cta: { label: 'Shop Sale', href: '/sale' },
    align: 'right',
    accentColor: true,
  },
]

const current = ref(0)
let timer: ReturnType<typeof setInterval>

function next() { current.value = (current.value + 1) % slides.length }
function prev() { current.value = (current.value - 1 + slides.length) % slides.length }
function goTo(i: number) { current.value = i }

onMounted(() => { timer = setInterval(next, 6000) })
onUnmounted(() => clearInterval(timer))
</script>

<template>
  <section class="hero-banner" aria-label="Featured campaigns">
    <div
      v-for="(slide, i) in slides"
      :key="slide.id"
      class="hero-banner__slide"
      :class="{ 'hero-banner__slide--active': i === current }"
    >
      <img :src="slide.image" :alt="slide.title" class="hero-banner__image" />
      <div class="hero-banner__overlay" />

      <div
        class="hero-banner__content"
        :class="`hero-banner__content--${slide.align}`"
      >
        <p class="hero-banner__eyebrow">{{ slide.eyebrow }}</p>
        <h1
          class="hero-banner__title"
          :class="{ 'hero-banner__title--accent': slide.accentColor }"
        >
          {{ slide.title }}
        </h1>
        <p class="hero-banner__subtitle">{{ slide.subtitle }}</p>
        <div class="hero-banner__ctas">
          <RouterLink :to="slide.cta.href" class="hero-banner__cta hero-banner__cta--primary">
            {{ slide.cta.label }}
          </RouterLink>
          <RouterLink
            v-if="slide.ctaSecondary"
            :to="slide.ctaSecondary.href"
            class="hero-banner__cta hero-banner__cta--ghost"
          >
            {{ slide.ctaSecondary.label }}
          </RouterLink>
        </div>
      </div>
    </div>

    <!-- Controls -->
    <button class="hero-banner__arrow hero-banner__arrow--prev" @click="prev">
      <i class="pi pi-chevron-left" />
    </button>
    <button class="hero-banner__arrow hero-banner__arrow--next" @click="next">
      <i class="pi pi-chevron-right" />
    </button>

    <!-- Dots -->
    <div class="hero-banner__dots">
      <button
        v-for="(_, i) in slides"
        :key="i"
        class="hero-banner__dot"
        :class="{ 'hero-banner__dot--active': i === current }"
        :aria-label="`Go to slide ${i + 1}`"
        @click="goTo(i)"
      />
    </div>

    <!-- Progress bar -->
    <div class="hero-banner__progress">
      <div
        class="hero-banner__progress-bar"
        :key="current"
      />
    </div>
  </section>
</template>

<style lang="scss" scoped>
.hero-banner {
  position: relative;
  height: 70vh;
  min-height: 500px;
  max-height: 900px;
  overflow: hidden;

  &__slide {
    position: absolute;
    inset: 0;
    opacity: 0;
    @include transition(opacity, $duration-slower);

    &--active { opacity: 1; z-index: 1; }
  }

  &__image {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  &__overlay {
    position: absolute;
    inset: 0;
    background: linear-gradient(
      to right,
      rgb(0 0 0 / 0.5) 0%,
      rgb(0 0 0 / 0.1) 60%,
      transparent 100%
    );
  }

  &__content {
    position: absolute;
    bottom: 20%;
    left: $space-12;
    max-width: 560px;
    z-index: 2;
    color: white;

    &--center {
      left: 50%;
      transform: translateX(-50%);
      text-align: center;
    }

    &--right {
      left: auto;
      right: $space-12;
    }
  }

  &__eyebrow {
    @include label-caps;
    color: rgb(255 255 255 / 0.8);
    margin-bottom: $space-3;
  }

  &__title {
    font-family: $font-display;
    font-size: clamp($text-3xl, 5vw, $text-4xl);
    font-weight: $weight-light;
    line-height: $leading-tight;
    letter-spacing: $tracking-tight;
    white-space: pre-line;
    margin-bottom: $space-4;

    &--accent { color: #ffd700; }
  }

  &__subtitle {
    font-size: $text-md;
    color: rgb(255 255 255 / 0.8);
    margin-bottom: $space-8;
  }

  &__ctas {
    display: flex;
    gap: $space-4;
    flex-wrap: wrap;
  }

  &__cta {
    @include label-caps;
    text-decoration: none;
    padding: $space-4 $space-8;

    &--primary {
      background: white;
      color: #0d0c0a;
      @include transition(background color);
      &:hover { background: var(--color-accent); color: white; }
    }

    &--ghost {
      border: 1px solid rgb(255 255 255 / 0.7);
      color: white;
      @include transition(background);
      &:hover { background: rgb(255 255 255 / 0.1); }
    }
  }

  &__arrow {
    position: absolute;
    top: 50%;
    transform: translateY(-50%);
    z-index: 3;
    background: rgb(255 255 255 / 0.15);
    border: 1px solid rgb(255 255 255 / 0.3);
    color: white;
    width: 48px;
    height: 48px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    @include transition(background);
    backdrop-filter: blur(4px);

    &:hover { background: rgb(255 255 255 / 0.3); }
    &--prev { left: $space-4; }
    &--next { right: $space-4; }
  }

  &__dots {
    position: absolute;
    bottom: $space-8;
    left: 50%;
    transform: translateX(-50%);
    display: flex;
    gap: $space-2;
    z-index: 3;
  }

  &__dot {
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: rgb(255 255 255 / 0.5);
    border: none;
    cursor: pointer;
    @include transition(all);

    &--active {
      background: white;
      width: 24px;
      border-radius: 3px;
    }
  }

  &__progress {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    height: 2px;
    background: rgb(255 255 255 / 0.2);
    z-index: 3;
  }

  &__progress-bar {
    height: 100%;
    background: white;
    animation: progress 6s linear forwards;
  }
}

@keyframes progress {
  from { width: 0; }
  to   { width: 100%; }
}
</style>
```

---

## Product Listing Page `src/pages/[gender]/[category].vue`

```vue
<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useFiltersStore } from '@/stores/filters'
import { useProductList } from '@/composables/useProducts'
import FilterPanel from '@/components/ui/FilterPanel/FilterPanel.vue'
import ProductGrid from '@/components/ui/ProductGrid.vue'
import ActiveFiltersBar from '@/components/ui/ActiveFiltersBar.vue'
import SortDropdown from '@/components/ui/SortDropdown.vue'
import Drawer from 'primevue/drawer'
import Breadcrumb from 'primevue/breadcrumb'

const route       = useRoute()
const filtersStore = useFiltersStore()

const showMobileFilter = ref(false)
const viewMode = ref<'grid' | 'list'>('grid')
const page = ref(1)

const params = computed(() => ({
  categorySlug: route.params.category as string,
  gender:       route.params.gender as string,
  sortBy:       filtersStore.state.sortBy,
  page:         page.value,
  minPrice:     filtersStore.state.activePriceRange[0],
  maxPrice:     filtersStore.state.activePriceRange[1],
  sizes:        filtersStore.state.sizes.filter(s => s.active).map(s => s.value),
  colors:       filtersStore.state.colors.filter(c => c.active).map(c => c.id),
}))

const { products, meta, isLoading } = useProductList(params.value)

const categoryTitle = computed(() =>
  (route.params.category as string).replace(/-/g, ' ').replace(/\b\w/g, c => c.toUpperCase())
)

const breadcrumbItems = computed(() => [
  { label: 'Home', route: '/' },
  { label: (route.params.gender as string).replace(/\b\w/g, c => c.toUpperCase()), route: `/${route.params.gender}` },
  { label: categoryTitle.value },
])

watch(() => filtersStore.state.sortBy, () => { page.value = 1 })
</script>

<template>
  <div class="plp">
    <!-- Page Header -->
    <div class="plp__header">
      <Breadcrumb :model="breadcrumbItems" class="plp__breadcrumb" />
      <h1 class="plp__title">{{ categoryTitle }}</h1>
      <p v-if="meta" class="plp__count">{{ meta.total }} items</p>
    </div>

    <div class="plp__body">
      <!-- Desktop Sidebar Filter -->
      <aside class="plp__sidebar">
        <FilterPanel
          :filters="filtersStore.state"
          :loading="isLoading"
          @update="(f) => filtersStore.setAvailableFilters(f)"
          @clear="filtersStore.clearAll()"
        />
      </aside>

      <!-- Main Content -->
      <div class="plp__main">
        <!-- Toolbar -->
        <div class="plp__toolbar">
          <div class="plp__toolbar-left">
            <!-- Mobile Filter Toggle -->
            <button
              class="plp__filter-btn"
              @click="showMobileFilter = true"
            >
              <i class="pi pi-sliders-h" />
              Filter
              <span v-if="filtersStore.activeCount > 0" class="plp__filter-count">
                {{ filtersStore.activeCount }}
              </span>
            </button>

            <!-- Active filters chips -->
            <ActiveFiltersBar
              :filters="filtersStore.activeFilters"
              @remove="filtersStore.removeFilter"
              @clear="filtersStore.clearAll()"
            />
          </div>

          <div class="plp__toolbar-right">
            <!-- View Mode Toggle -->
            <div class="plp__view-toggle">
              <button
                :class="{ active: viewMode === 'grid' }"
                @click="viewMode = 'grid'"
                aria-label="Grid view"
              >
                <i class="pi pi-th-large" />
              </button>
              <button
                :class="{ active: viewMode === 'list' }"
                @click="viewMode = 'list'"
                aria-label="List view"
              >
                <i class="pi pi-list" />
              </button>
            </div>

            <SortDropdown
              :value="filtersStore.state.sortBy"
              @change="filtersStore.setSortBy"
            />
          </div>
        </div>

        <!-- Product Grid -->
        <ProductGrid
          :products="products"
          :loading="isLoading"
          :view-mode="viewMode"
        />

        <!-- Pagination -->
        <div v-if="meta && meta.totalPages > 1" class="plp__pagination">
          <button
            v-if="meta.hasPrevPage"
            class="plp__page-btn"
            @click="page--"
          >← Previous</button>
          <span class="plp__page-info">
            Page {{ meta.page }} of {{ meta.totalPages }}
          </span>
          <button
            v-if="meta.hasNextPage"
            class="plp__page-btn"
            @click="page++"
          >Next →</button>
        </div>
      </div>
    </div>

    <!-- Mobile Filter Drawer -->
    <Drawer v-model:visible="showMobileFilter" position="left" class="plp__mobile-filter">
      <template #header>
        <div class="plp__drawer-header">
          <span>Filter</span>
          <button @click="filtersStore.clearAll()">Clear All</button>
        </div>
      </template>
      <FilterPanel
        :filters="filtersStore.state"
        :loading="isLoading"
        @update="(f) => filtersStore.setAvailableFilters(f)"
        @clear="filtersStore.clearAll()"
      />
      <template #footer>
        <button
          class="plp__apply-btn"
          @click="showMobileFilter = false"
        >
          Apply Filters ({{ meta?.total ?? 0 }} items)
        </button>
      </template>
    </Drawer>
  </div>
</template>

<style lang="scss" scoped>
.plp {
  max-width: 1600px;
  margin: 0 auto;
  padding: $space-6;

  @include respond-to('xl') { padding: $space-8 $space-12; }

  &__header {
    margin-bottom: $space-8;
  }

  &__breadcrumb {
    margin-bottom: $space-4;
  }

  &__title {
    font-family: $font-display;
    font-size: $text-3xl;
    font-weight: $weight-light;
    letter-spacing: $tracking-tight;
    color: var(--color-text-primary);
    margin-bottom: $space-2;
  }

  &__count {
    @include label-caps;
    color: var(--color-text-muted);
  }

  &__body {
    display: grid;
    grid-template-columns: 1fr;
    gap: $space-8;

    @include respond-to('lg') {
      grid-template-columns: 260px 1fr;
    }
  }

  &__sidebar {
    display: none;
    @include respond-to('lg') { display: block; }
  }

  &__toolbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: $space-4;
    margin-bottom: $space-6;
    padding-bottom: $space-4;
    border-bottom: 1px solid var(--color-border);
  }

  &__toolbar-left,
  &__toolbar-right {
    display: flex;
    align-items: center;
    gap: $space-4;
  }

  &__filter-btn {
    @include label-caps;
    display: flex;
    align-items: center;
    gap: $space-2;
    padding: $space-2 $space-4;
    border: 1px solid var(--color-border);
    background: none;
    cursor: pointer;
    color: var(--color-text-primary);
    @include transition(border-color);

    @include respond-to('lg') { display: none; }

    &:hover { border-color: var(--color-text-primary); }
  }

  &__filter-count {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 18px;
    height: 18px;
    border-radius: 50%;
    background: var(--color-text-primary);
    color: var(--color-bg-canvas);
    font-size: 10px;
  }

  &__view-toggle {
    display: flex;
    border: 1px solid var(--color-border);

    button {
      width: 36px;
      height: 36px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: none;
      border: none;
      cursor: pointer;
      color: var(--color-text-muted);
      @include transition(background color);

      &.active,
      &:hover {
        background: var(--color-text-primary);
        color: var(--color-bg-canvas);
      }
    }
  }

  &__pagination {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: $space-6;
    margin-top: $space-12;
    padding-top: $space-8;
    border-top: 1px solid var(--color-border);
  }

  &__page-btn {
    @include label-caps;
    background: none;
    border: 1px solid var(--color-border);
    padding: $space-2 $space-6;
    cursor: pointer;
    color: var(--color-text-primary);
    @include transition(background color);

    &:hover {
      background: var(--color-text-primary);
      color: var(--color-bg-canvas);
    }
  }

  &__page-info {
    font-size: $text-sm;
    color: var(--color-text-muted);
  }

  &__apply-btn {
    @include label-caps;
    width: 100%;
    padding: $space-4;
    background: var(--color-text-primary);
    color: var(--color-bg-canvas);
    border: none;
    cursor: pointer;
  }
}
</style>
```

---

## Product Detail Page `src/pages/product/[slug].vue`

```vue
<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useProduct, useRelatedProducts } from '@/composables/useProducts'
import { useCart } from '@/composables/useCart'
import { useWishlist } from '@/composables/useWishlist'
import { useUIStore } from '@/stores/ui'
import ProductGallery from '@/components/ui/ProductGallery/ProductGallery.vue'
import SizeSelector from '@/components/ui/SizeSelector.vue'
import ColorSwatchSelector from '@/components/ui/ColorSwatchSelector.vue'
import ProductCarousel from '@/components/ui/ProductCarousel.vue'
import ReviewsSection from '@/components/feature/reviews/ReviewsSection.vue'
import SizeGuideModal from '@/components/ui/SizeGuideModal.vue'
import StickyAddToCart from '@/components/ui/StickyAddToCart.vue'
import AccordionPanel from 'primevue/accordionpanel'
import AccordionHeader from 'primevue/accordionheader'
import AccordionContent from 'primevue/accordioncontent'
import Accordion from 'primevue/accordion'
import Breadcrumb from 'primevue/breadcrumb'

const route   = useRoute()
const uiStore = useUIStore()
const { addToCart } = useCart()
const { isWishlisted, toggleWishlist } = useWishlist()

const slug = computed(() => route.params.slug as string)

const { product, isLoading } = useProduct(slug.value)
const { related }            = useRelatedProducts(computed(() => product.value?.id ?? '').value)

const selectedColorId = ref<string>('')
const selectedSize    = ref<string>('')
const quantity        = ref(1)
const addingToCart    = ref(false)

const selectedColor = computed(() =>
  product.value?.colors.find(c => c.id === selectedColorId.value)
    ?? product.value?.colors[0]
)

const selectedVariant = computed(() =>
  selectedColor.value?.variants.find(v => v.size === selectedSize.value)
)

const isInStock        = computed(() => (selectedVariant.value?.stock ?? 0) > 0)
const isLowStock       = computed(() => {
  const stock = selectedVariant.value?.stock ?? 0
  return stock > 0 && stock <= 3
})

async function handleAddToCart() {
  if (!product.value || !selectedVariant.value || !selectedColor.value) return
  addingToCart.value = true
  try {
    addToCart(product.value, selectedVariant.value, selectedColor.value, quantity.value)
  } finally {
    addingToCart.value = false
  }
}

onMounted(() => {
  if (product.value) {
    selectedColorId.value = product.value.colors[0]?.id ?? ''
    uiStore.addRecentlyViewed(product.value.id)
  }
})

watch(product, (p) => {
  if (p && !selectedColorId.value) {
    selectedColorId.value = p.colors[0]?.id ?? ''
  }
})
</script>

<template>
  <div v-if="!isLoading && product" class="pdp">
    <!-- Breadcrumb -->
    <div class="pdp__breadcrumb-wrap">
      <Breadcrumb
        :model="[
          { label: 'Home', route: '/' },
          { label: product.gender.charAt(0).toUpperCase() + product.gender.slice(1), route: `/${product.gender}` },
          { label: product.categories[0]?.name, route: `/${product.gender}/${product.categories[0]?.slug}` },
          { label: product.brand.name },
        ]"
      />
    </div>

    <div class="pdp__body">
      <!-- Gallery -->
      <div class="pdp__gallery-col">
        <ProductGallery
          :images="selectedColor?.images ?? product.images"
          :product-name="product.name"
        />
      </div>

      <!-- Product Info -->
      <div class="pdp__info-col">
        <!-- Brand & Name -->
        <div class="pdp__identity">
          <RouterLink :to="`/designers/${product.brand.slug}`" class="pdp__brand">
            {{ product.brand.name }}
          </RouterLink>
          <h1 class="pdp__name">{{ product.name }}</h1>

          <!-- Badges -->
          <div class="pdp__badges">
            <span v-if="product.isNew" class="pdp__badge pdp__badge--new">New In</span>
            <span v-if="product.isExclusive" class="pdp__badge pdp__badge--exclusive">
              Exclusive
            </span>
          </div>
        </div>

        <!-- Price -->
        <div class="pdp__price-block">
          <span
            class="pdp__price"
            :class="{ 'pdp__price--sale': product.isSale }"
          >
            {{ product.currency }} {{ product.price.toLocaleString() }}
          </span>
          <span v-if="product.originalPrice" class="pdp__price-original">
            {{ product.currency }} {{ product.originalPrice.toLocaleString() }}
          </span>
          <span v-if="product.discount" class="pdp__price-discount">
            -{{ product.discount }}%
          </span>
        </div>

        <!-- Colour Selector -->
        <div class="pdp__section">
          <div class="pdp__section-label">
            Colour: <strong>{{ selectedColor?.name }}</strong>
          </div>
          <ColorSwatchSelector
            :colors="product.colors"
            :selected-id="selectedColorId"
            @select="(id) => { selectedColorId = id; selectedSize = '' }"
          />
        </div>

        <!-- Size Selector -->
        <div class="pdp__section">
          <div class="pdp__section-label">
            <span>Size</span>
            <button class="pdp__size-guide-link" @click="uiStore.openSizeGuide()">
              Size Guide
            </button>
          </div>
          <SizeSelector
            :variants="selectedColor?.variants ?? product.variants"
            :selected-size="selectedSize"
            @select="(size) => selectedSize = size"
          />
        </div>

        <!-- Stock Warning -->
        <p v-if="isLowStock" class="pdp__low-stock">
          <i class="pi pi-exclamation-circle" />
          Only {{ selectedVariant?.stock }} left in stock
        </p>

        <!-- Quantity + Add to Cart -->
        <div class="pdp__add-section">
          <div class="pdp__quantity">
            <button :disabled="quantity <= 1" @click="quantity--">−</button>
            <span>{{ quantity }}</span>
            <button
              :disabled="quantity >= (selectedVariant?.stock ?? 1)"
              @click="quantity++"
            >+</button>
          </div>

          <button
            class="pdp__add-btn"
            :disabled="!selectedSize || !isInStock || addingToCart"
            @click="handleAddToCart"
          >
            <span v-if="!selectedSize">Select a Size</span>
            <span v-else-if="!isInStock">Out of Stock</span>
            <span v-else-if="addingToCart">
              <i class="pi pi-spin pi-spinner" /> Adding...
            </span>
            <span v-else>Add to Bag</span>
          </button>

          <!-- Wishlist -->
          <button
            class="pdp__wishlist-btn"
            :class="{ 'pdp__wishlist-btn--active': isWishlisted(product.id) }"
            :aria-label="isWishlisted(product.id) ? 'Remove from wishlist' : 'Add to wishlist'"
            @click="toggleWishlist(product as any)"
          >
            <i :class="isWishlisted(product.id) ? 'pi pi-heart-fill' : 'pi pi-heart'" />
          </button>
        </div>

        <!-- Trust Badges -->
        <div class="pdp__trust">
          <div class="pdp__trust-item">
            <i class="pi pi-truck" />
            <span>Free shipping over SGD 500</span>
          </div>
          <div class="pdp__trust-item">
            <i class="pi pi-replay" />
            <span>Free 30-day returns</span>
          </div>
          <div class="pdp__trust-item">
            <i class="pi pi-shield" />
            <span>Authenticity guaranteed</span>
          </div>
        </div>

        <!-- Product Details Accordion -->
        <Accordion>
          <AccordionPanel value="description">
            <AccordionHeader>Description</AccordionHeader>
            <AccordionContent>
              <p class="pdp__accordion-text">{{ product.description }}</p>
            </AccordionContent>
          </AccordionPanel>

          <AccordionPanel value="composition">
            <AccordionHeader>Composition & Care</AccordionHeader>
            <AccordionContent>
              <p class="pdp__accordion-text pdp__composition">{{ product.composition }}</p>
              <ul class="pdp__care-list">
                <li v-for="instruction in product.careInstructions" :key="instruction">
                  {{ instruction }}
                </li>
              </ul>
            </AccordionContent>
          </AccordionPanel>

          <AccordionPanel value="delivery">
            <AccordionHeader>Delivery & Returns</AccordionHeader>
            <AccordionContent>
              <div class="pdp__accordion-text">
                <p><strong>Standard Delivery</strong>: 3-5 business days (Free over SGD 500)</p>
                <p><strong>Express Delivery</strong>: 1-2 business days (SGD 25)</p>
                <p><strong>Returns</strong>: Free returns within 30 days of delivery</p>
              </div>
            </AccordionContent>
          </AccordionPanel>
        </Accordion>

        <!-- Brand Info -->
        <RouterLink :to="`/designers/${product.brand.slug}`" class="pdp__brand-card">
          <span class="pdp__brand-card-label">About the Designer</span>
          <span class="pdp__brand-card-name">{{ product.brand.name }}</span>
          <i class="pi pi-arrow-right" />
        </RouterLink>
      </div>
    </div>

    <!-- Reviews -->
    <ReviewsSection :product-id="product.id" :rating="product.rating" :count="product.reviewCount" />

    <!-- Related Products -->
    <ProductCarousel
      title="Complete the Look"
      :products="related"
    />

    <!-- Sticky Add to Cart (mobile) -->
    <StickyAddToCart
      :product="product"
      :selected-size="selectedSize"
      :is-in-stock="isInStock"
      :adding-to-cart="addingToCart"
      @add="handleAddToCart"
    />

    <!-- Size Guide Modal -->
    <SizeGuideModal
      v-if="product.sizeGuideId"
      :size-guide-id="product.sizeGuideId"
    />
  </div>

  <!-- Skeleton Loading -->
  <div v-else-if="isLoading" class="pdp pdp--loading">
    <div class="pdp__body">
      <Skeleton height="600px" />
      <div class="pdp__info-col">
        <Skeleton width="120px" height="14px" class="mb-3" />
        <Skeleton width="80%" height="32px" class="mb-6" />
        <Skeleton width="100px" height="28px" class="mb-8" />
        <Skeleton height="120px" class="mb-6" />
        <Skeleton height="52px" />
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.pdp {
  max-width: 1400px;
  margin: 0 auto;
  padding: $space-4;

  @include respond-to('xl') { padding: $space-8 $space-12; }

  &__breadcrumb-wrap {
    margin-bottom: $space-6;
  }

  &__body {
    display: grid;
    grid-template-columns: 1fr;
    gap: $space-8;

    @include respond-to('lg') {
      grid-template-columns: 1fr 480px;
      gap: $space-12;
    }

    @include respond-to('xl') {
      grid-template-columns: 1fr 560px;
    }
  }

  &__identity {
    margin-bottom: $space-5;
  }

  &__brand {
    @include label-caps;
    display: block;
    color: var(--color-text-primary);
    text-decoration: none;
    margin-bottom: $space-2;
    @include transition(color);

    &:hover { color: var(--color-accent); }
  }

  &__name {
    font-family: $font-display;
    font-size: $text-2xl;
    font-weight: $weight-light;
    letter-spacing: $tracking-tight;
    line-height: $leading-snug;
    color: var(--color-text-primary);
    margin-bottom: $space-3;
  }

  &__badges {
    display: flex;
    gap: $space-2;
  }

  &__badge {
    @include label-caps;
    font-size: 10px;
    padding: 3px 10px;

    &--new       { background: var(--color-text-primary); color: var(--color-bg-canvas); }
    &--exclusive { border: 1px solid var(--color-accent); color: var(--color-accent); }
  }

  &__price-block {
    display: flex;
    align-items: baseline;
    gap: $space-3;
    margin-bottom: $space-6;
    padding-bottom: $space-6;
    border-bottom: 1px solid var(--color-border);
  }

  &__price {
    font-size: $text-xl;
    font-weight: $weight-medium;
    color: var(--color-text-primary);

    &--sale { color: $color-error; }
  }

  &__price-original {
    font-size: $text-md;
    color: var(--color-text-muted);
    text-decoration: line-through;
  }

  &__price-discount {
    @include label-caps;
    color: $color-error;
    font-size: $text-xs;
  }

  &__section {
    margin-bottom: $space-6;
  }

  &__section-label {
    @include label-caps;
    display: flex;
    align-items: center;
    justify-content: space-between;
    color: var(--color-text-muted);
    margin-bottom: $space-3;

    strong {
      color: var(--color-text-primary);
      font-weight: $weight-medium;
    }
  }

  &__size-guide-link {
    background: none;
    border: none;
    cursor: pointer;
    font-size: $text-xs;
    color: var(--color-text-muted);
    text-decoration: underline;
    text-underline-offset: 2px;
    @include transition(color);

    &:hover { color: var(--color-text-primary); }
  }

  &__low-stock {
    display: flex;
    align-items: center;
    gap: $space-2;
    font-size: $text-xs;
    color: $color-error;
    margin-top: -$space-4;
    margin-bottom: $space-4;
  }

  &__add-section {
    display: grid;
    grid-template-columns: auto 1fr auto;
    gap: $space-3;
    margin-bottom: $space-6;
    align-items: center;
  }

  &__quantity {
    display: flex;
    align-items: center;
    border: 1px solid var(--color-border);
    height: 52px;

    button {
      width: 36px;
      height: 100%;
      background: none;
      border: none;
      cursor: pointer;
      font-size: $text-md;
      color: var(--color-text-primary);

      &:disabled { color: var(--color-border-strong); cursor: not-allowed; }
    }

    span {
      min-width: 28px;
      text-align: center;
      font-size: $text-sm;
    }
  }

  &__add-btn {
    @include label-caps;
    height: 52px;
    background: var(--color-text-primary);
    color: var(--color-bg-canvas);
    border: none;
    cursor: pointer;
    @include transition(background);

    &:hover:not(:disabled) { background: var(--color-accent); }

    &:disabled {
      background: var(--color-border-strong);
      cursor: not-allowed;
    }
  }

  &__wishlist-btn {
    width: 52px;
    height: 52px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid var(--color-border);
    background: none;
    cursor: pointer;
    color: var(--color-text-muted);
    @include transition(all);

    .pi { font-size: 18px; }

    &:hover {
      border-color: $color-error;
      color: $color-error;
    }

    &--active {
      border-color: $color-error;
      color: $color-error;
    }
  }

  &__trust {
    display: flex;
    flex-direction: column;
    gap: $space-3;
    padding: $space-5;
    background: var(--color-bg-elevated);
    margin-bottom: $space-6;
  }

  &__trust-item {
    display: flex;
    align-items: center;
    gap: $space-3;
    font-size: $text-xs;
    color: var(--color-text-secondary);

    .pi { color: var(--color-accent); }
  }

  &__accordion-text {
    font-size: $text-sm;
    color: var(--color-text-secondary);
    line-height: $leading-relaxed;
  }

  &__care-list {
    margin-top: $space-3;
    padding-left: $space-4;
    font-size: $text-sm;
    color: var(--color-text-secondary);
    line-height: $leading-relaxed;
  }

  &__brand-card {
    display: flex;
    align-items: center;
    gap: $space-3;
    padding: $space-4;
    border: 1px solid var(--color-border);
    text-decoration: none;
    margin-top: $space-6;
    @include transition(border-color);

    &:hover { border-color: var(--color-text-primary); }

    &-label {
      @include label-caps;
      font-size: 10px;
      color: var(--color-text-muted);
      display: block;
    }

    &-name {
      font-family: $font-display;
      font-size: $text-md;
      color: var(--color-text-primary);
      flex: 1;
    }

    .pi { color: var(--color-text-muted); }
  }
}
</style>
```

---

## Cart Page `src/pages/checkout/cart.vue`

```vue
<script setup lang="ts">
import { useCartStore } from '@/stores/cart'
import { useCurrency } from '@/composables/useCurrency'
import CartItem from '@/components/feature/cart/CartItem.vue'
import PromoCodeInput from '@/components/feature/cart/PromoCodeInput.vue'
import FreeShippingBar from '@/components/feature/cart/FreeShippingBar.vue'

const cart    = useCartStore()
const { format } = useCurrency()
</script>

<template>
  <div class="cart-page">
    <div class="cart-page__inner">
      <h1 class="cart-page__title">Shopping Bag</h1>

      <div v-if="cart.isEmpty" class="cart-page__empty">
        <i class="pi pi-shopping-bag" />
        <p>Your bag is empty</p>
        <RouterLink to="/women" class="cart-page__shop-btn">Start Shopping</RouterLink>
      </div>

      <div v-else class="cart-page__body">
        <!-- Items -->
        <div class="cart-page__items-col">
          <FreeShippingBar :summary="cart.summary" />

          <div class="cart-page__items">
            <CartItem
              v-for="item in cart.items"
              :key="item.id"
              :item="item"
              @remove="cart.removeItem(item.id)"
              @update-quantity="(qty) => cart.updateQuantity(item.id, qty)"
            />
          </div>

          <PromoCodeInput />
        </div>

        <!-- Summary -->
        <aside class="cart-page__summary-col">
          <div class="cart-page__summary-card">
            <h2 class="cart-page__summary-title">Order Summary</h2>

            <div class="cart-page__summary-row">
              <span>Subtotal ({{ cart.itemCount }} items)</span>
              <span>{{ format(cart.summary.subtotal) }}</span>
            </div>

            <div v-if="cart.summary.discount > 0" class="cart-page__summary-row cart-page__summary-row--discount">
              <span>Discount</span>
              <span>-{{ format(cart.summary.discount) }}</span>
            </div>

            <div class="cart-page__summary-row">
              <span>Shipping</span>
              <span>{{ cart.summary.shipping === 0 ? 'FREE' : format(cart.summary.shipping ?? 0) }}</span>
            </div>

            <div class="cart-page__summary-row">
              <span>GST (9%)</span>
              <span>{{ format(cart.summary.tax) }}</span>
            </div>

            <div class="cart-page__summary-total">
              <span>Total</span>
              <span>{{ format(cart.summary.total) }}</span>
            </div>

            <RouterLink to="/checkout/shipping" class="cart-page__checkout-btn">
              Proceed to Checkout
            </RouterLink>

            <div class="cart-page__payment-icons">
              <span>We accept:</span>
              <span>VISA · MC · AMEX · PayPal · Apple Pay</span>
            </div>
          </div>
        </aside>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.cart-page {
  padding: $space-8 $space-6;

  @include respond-to('xl') { padding: $space-12; }

  &__inner {
    max-width: 1200px;
    margin: 0 auto;
  }

  &__title {
    font-family: $font-display;
    font-size: $text-3xl;
    font-weight: $weight-light;
    margin-bottom: $space-8;
    color: var(--color-text-primary);
  }

  &__empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: $space-4;
    padding: $space-20;
    text-align: center;

    .pi { font-size: 60px; color: var(--color-border-strong); }
    p { font-family: $font-display; font-size: $text-xl; color: var(--color-text-muted); }
  }

  &__shop-btn {
    @include label-caps;
    padding: $space-3 $space-10;
    background: var(--color-text-primary);
    color: var(--color-bg-canvas);
    text-decoration: none;
  }

  &__body {
    display: grid;
    gap: $space-8;

    @include respond-to('lg') {
      grid-template-columns: 1fr 360px;
      align-items: start;
    }
  }

  &__items {
    display: flex;
    flex-direction: column;
    gap: $space-6;
    padding: $space-6 0;
  }

  &__summary-card {
    padding: $space-6;
    border: 1px solid var(--color-border);
    position: sticky;
    top: 80px;
  }

  &__summary-title {
    font-family: $font-display;
    font-size: $text-lg;
    font-weight: $weight-light;
    margin-bottom: $space-6;
    padding-bottom: $space-4;
    border-bottom: 1px solid var(--color-border);
  }

  &__summary-row {
    display: flex;
    justify-content: space-between;
    font-size: $text-sm;
    color: var(--color-text-secondary);
    margin-bottom: $space-3;

    &--discount { color: $color-success; }
  }

  &__summary-total {
    display: flex;
    justify-content: space-between;
    font-size: $text-md;
    font-weight: $weight-semibold;
    color: var(--color-text-primary);
    padding-top: $space-4;
    margin-top: $space-4;
    border-top: 1px solid var(--color-border);
    margin-bottom: $space-6;
  }

  &__checkout-btn {
    @include label-caps;
    display: block;
    text-align: center;
    padding: $space-4;
    background: var(--color-text-primary);
    color: var(--color-bg-canvas);
    text-decoration: none;
    margin-bottom: $space-4;
    @include transition(background);

    &:hover { background: var(--color-accent); }
  }

  &__payment-icons {
    text-align: center;
    font-size: $text-xs;
    color: var(--color-text-muted);
    display: flex;
    flex-direction: column;
    gap: $space-1;
  }
}
</style>
```

---

## Checkout Shipping `src/pages/checkout/shipping.vue`

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'

const router    = useRouter()
const userStore = useUserStore()

const form = ref({
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  line1: '',
  line2: '',
  city: '',
  state: '',
  postalCode: '',
  country: 'SG',
})

const shippingMethod = ref<'standard' | 'express'>('standard')

const countries = [
  { label: 'Singapore', value: 'SG' },
  { label: 'Australia', value: 'AU' },
  { label: 'United Kingdom', value: 'GB' },
  { label: 'United States', value: 'US' },
]

const shippingOptions = [
  { id: 'standard', label: 'Standard Delivery', time: '3–5 business days', price: 'Free over SGD 500' },
  { id: 'express', label: 'Express Delivery',  time: '1–2 business days', price: 'SGD 25.00' },
]

function submit() {
  void router.push('/checkout/payment')
}
</script>

<template>
  <div class="checkout-shipping">
    <h2 class="checkout-shipping__title">Shipping Details</h2>

    <form class="checkout-shipping__form" @submit.prevent="submit">
      <div class="checkout-shipping__row checkout-shipping__row--2col">
        <div class="checkout-shipping__field">
          <label>First Name *</label>
          <InputText v-model="form.firstName" required />
        </div>
        <div class="checkout-shipping__field">
          <label>Last Name *</label>
          <InputText v-model="form.lastName" required />
        </div>
      </div>

      <div class="checkout-shipping__field">
        <label>Email Address *</label>
        <InputText v-model="form.email" type="email" required />
      </div>

      <div class="checkout-shipping__field">
        <label>Phone Number *</label>
        <InputText v-model="form.phone" type="tel" required />
      </div>

      <div class="checkout-shipping__field">
        <label>Address Line 1 *</label>
        <InputText v-model="form.line1" placeholder="Street, apartment, unit" required />
      </div>

      <div class="checkout-shipping__field">
        <label>Address Line 2</label>
        <InputText v-model="form.line2" placeholder="Floor, building name (optional)" />
      </div>

      <div class="checkout-shipping__row checkout-shipping__row--3col">
        <div class="checkout-shipping__field">
          <label>City *</label>
          <InputText v-model="form.city" required />
        </div>
        <div class="checkout-shipping__field">
          <label>State / Region</label>
          <InputText v-model="form.state" />
        </div>
        <div class="checkout-shipping__field">
          <label>Postal Code *</label>
          <InputText v-model="form.postalCode" required />
        </div>
      </div>

      <div class="checkout-shipping__field">
        <label>Country *</label>
        <Select v-model="form.country" :options="countries" option-label="label" option-value="value" />
      </div>

      <!-- Shipping Method -->
      <div class="checkout-shipping__methods">
        <h3 class="checkout-shipping__methods-title">Shipping Method</h3>
        <div
          v-for="option in shippingOptions"
          :key="option.id"
          class="checkout-shipping__method"
          :class="{ 'checkout-shipping__method--selected': shippingMethod === option.id }"
          @click="shippingMethod = option.id as 'standard' | 'express'"
        >
          <div class="checkout-shipping__method-radio">
            <span v-if="shippingMethod === option.id" class="checkout-shipping__method-dot" />
          </div>
          <div class="checkout-shipping__method-info">
            <span class="checkout-shipping__method-label">{{ option.label }}</span>
            <span class="checkout-shipping__method-time">{{ option.time }}</span>
          </div>
          <span class="checkout-shipping__method-price">{{ option.price }}</span>
        </div>
      </div>

      <button type="submit" class="checkout-shipping__submit">
        Continue to Payment →
      </button>
    </form>
  </div>
</template>

<style lang="scss" scoped>
.checkout-shipping {
  &__title {
    font-family: $font-display;
    font-size: $text-xl;
    font-weight: $weight-light;
    margin-bottom: $space-8;
  }

  &__form {
    display: flex;
    flex-direction: column;
    gap: $space-4;
  }

  &__row {
    display: grid;
    gap: $space-4;

    &--2col { @include respond-to('md') { grid-template-columns: 1fr 1fr; } }
    &--3col { @include respond-to('md') { grid-template-columns: 2fr 1fr 1fr; } }
  }

  &__field {
    display: flex;
    flex-direction: column;
    gap: $space-2;

    label {
      @include label-caps;
      font-size: 11px;
      color: var(--color-text-secondary);
    }
  }

  &__methods {
    border: 1px solid var(--color-border);
    padding: $space-5;
    margin-top: $space-4;
  }

  &__methods-title {
    @include label-caps;
    margin-bottom: $space-4;
  }

  &__method {
    display: flex;
    align-items: center;
    gap: $space-4;
    padding: $space-4;
    border: 1px solid var(--color-border);
    margin-bottom: $space-3;
    cursor: pointer;
    @include transition(border-color);

    &:last-child { margin-bottom: 0; }
    &--selected { border-color: var(--color-text-primary); }
  }

  &__method-radio {
    width: 18px;
    height: 18px;
    border-radius: 50%;
    border: 1px solid var(--color-border-strong);
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
  }

  &__method-dot {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    background: var(--color-text-primary);
  }

  &__method-info {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  &__method-label {
    font-size: $text-sm;
    font-weight: $weight-medium;
    color: var(--color-text-primary);
  }

  &__method-time {
    font-size: $text-xs;
    color: var(--color-text-muted);
  }

  &__method-price {
    font-size: $text-sm;
    font-weight: $weight-medium;
    color: var(--color-text-primary);
  }

  &__submit {
    @include label-caps;
    width: 100%;
    padding: $space-4;
    background: var(--color-text-primary);
    color: var(--color-bg-canvas);
    border: none;
    cursor: pointer;
    margin-top: $space-4;
    @include transition(background);

    &:hover { background: var(--color-accent); }
  }
}
</style>
```

---

## Account Dashboard `src/pages/account/index.vue`

```vue
<script setup lang="ts">
import { useUserStore } from '@/stores/user'
import { useWishlistStore } from '@/stores/wishlist'

const user      = useUserStore()
const wishlist  = useWishlistStore()
</script>

<template>
  <div class="account-home">
    <div class="account-home__welcome">
      <h1 class="account-home__greeting">
        Welcome back,<br>
        <span>{{ user.user?.firstName }}</span>
      </h1>
      <p class="account-home__loyalty" v-if="user.user?.loyaltyPoints">
        You have <strong>{{ user.user.loyaltyPoints.toLocaleString() }}</strong> loyalty points
      </p>
    </div>

    <div class="account-home__grid">
      <!-- Recent Orders -->
      <RouterLink to="/account/orders" class="account-home__card">
        <i class="pi pi-box account-home__card-icon" />
        <h3 class="account-home__card-title">My Orders</h3>
        <p class="account-home__card-sub">Track and manage your orders</p>
        <span class="account-home__card-arrow">→</span>
      </RouterLink>

      <!-- Wishlist -->
      <RouterLink to="/wishlist" class="account-home__card">
        <i class="pi pi-heart account-home__card-icon" />
        <h3 class="account-home__card-title">Wishlist</h3>
        <p class="account-home__card-sub">
          {{ wishlist.count }} saved {{ wishlist.count === 1 ? 'item' : 'items' }}
        </p>
        <span class="account-home__card-arrow">→</span>
      </RouterLink>

      <!-- Addresses -->
      <RouterLink to="/account/addresses" class="account-home__card">
        <i class="pi pi-map-marker account-home__card-icon" />
        <h3 class="account-home__card-title">Addresses</h3>
        <p class="account-home__card-sub">Manage your delivery addresses</p>
        <span class="account-home__card-arrow">→</span>
      </RouterLink>

      <!-- Returns -->
      <RouterLink to="/account/returns" class="account-home__card">
        <i class="pi pi-replay account-home__card-icon" />
        <h3 class="account-home__card-title">Returns</h3>
        <p class="account-home__card-sub">Start a return or exchange</p>
        <span class="account-home__card-arrow">→</span>
      </RouterLink>

      <!-- Profile -->
      <RouterLink to="/account/profile" class="account-home__card">
        <i class="pi pi-user-edit account-home__card-icon" />
        <h3 class="account-home__card-title">Profile</h3>
        <p class="account-home__card-sub">Update your personal details</p>
        <span class="account-home__card-arrow">→</span>
      </RouterLink>

      <!-- Payment Methods -->
      <RouterLink to="/account/payment-methods" class="account-home__card">
        <i class="pi pi-credit-card account-home__card-icon" />
        <h3 class="account-home__card-title">Payment Methods</h3>
        <p class="account-home__card-sub">Manage saved cards</p>
        <span class="account-home__card-arrow">→</span>
      </RouterLink>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.account-home {
  &__welcome {
    margin-bottom: $space-10;
    padding-bottom: $space-8;
    border-bottom: 1px solid var(--color-border);
  }

  &__greeting {
    font-family: $font-display;
    font-size: $text-3xl;
    font-weight: $weight-light;
    color: var(--color-text-primary);
    line-height: $leading-snug;
    margin-bottom: $space-3;

    span { color: var(--color-accent); }
  }

  &__loyalty {
    font-size: $text-sm;
    color: var(--color-text-muted);
  }

  &__grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
    gap: $space-4;
  }

  &__card {
    display: flex;
    flex-direction: column;
    padding: $space-6;
    border: 1px solid var(--color-border);
    text-decoration: none;
    position: relative;
    @include transition(border-color box-shadow);

    &:hover {
      border-color: var(--color-text-primary);
      box-shadow: $shadow-sm;
    }
  }

  &__card-icon {
    font-size: 28px;
    color: var(--color-accent);
    margin-bottom: $space-4;
  }

  &__card-title {
    font-size: $text-md;
    font-weight: $weight-medium;
    color: var(--color-text-primary);
    margin-bottom: $space-2;
  }

  &__card-sub {
    font-size: $text-sm;
    color: var(--color-text-muted);
    flex: 1;
    line-height: $leading-relaxed;
  }

  &__card-arrow {
    position: absolute;
    bottom: $space-5;
    right: $space-5;
    color: var(--color-text-muted);
    font-size: $text-sm;
    @include transition(transform);
  }

  &__card:hover &__card-arrow {
    transform: translateX(4px);
    color: var(--color-text-primary);
  }
}
</style>
```

---

## Auth Login Page `src/pages/auth/login.vue`

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useUserStore } from '@/stores/user'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'

const router     = useRouter()
const route      = useRoute()
const userStore  = useUserStore()

const form   = ref({ email: '', password: '' })
const error  = ref('')

async function submit() {
  error.value = ''
  try {
    await userStore.login(form.value.email, form.value.password)
    const redirect = (route.query.redirect as string) ?? '/account'
    void router.push(redirect)
  } catch (e: any) {
    error.value = 'Invalid email or password. Please try again.'
  }
}
</script>

<template>
  <div class="auth-login">
    <h1 class="auth-login__title">Sign In</h1>
    <p class="auth-login__sub">Welcome back to Maison</p>

    <form class="auth-login__form" @submit.prevent="submit">
      <div v-if="error" class="auth-login__error">
        <i class="pi pi-exclamation-triangle" />
        {{ error }}
      </div>

      <div class="auth-login__field">
        <label for="email">Email Address</label>
        <InputText id="email" v-model="form.email" type="email" required autocomplete="email" />
      </div>

      <div class="auth-login__field">
        <div class="auth-login__field-header">
          <label for="password">Password</label>
          <RouterLink to="/auth/forgot-password" class="auth-login__forgot">
            Forgot password?
          </RouterLink>
        </div>
        <Password
          id="password"
          v-model="form.password"
          :feedback="false"
          toggleMask
          required
          autocomplete="current-password"
        />
      </div>

      <button
        type="submit"
        class="auth-login__submit"
        :disabled="userStore.loading"
      >
        <i v-if="userStore.loading" class="pi pi-spin pi-spinner" />
        Sign In
      </button>
    </form>

    <p class="auth-login__register">
      New to Maison?
      <RouterLink to="/auth/register">Create an account</RouterLink>
    </p>

    <div class="auth-login__divider"><span>or continue with</span></div>

    <div class="auth-login__social">
      <button class="auth-login__social-btn">
        <img src="/icons/google.svg" alt="" width="18" height="18" />
        Google
      </button>
      <button class="auth-login__social-btn">
        <img src="/icons/apple.svg" alt="" width="18" height="18" />
        Apple
      </button>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.auth-login {
  max-width: 400px;
  margin: 0 auto;

  &__title {
    font-family: $font-display;
    font-size: $text-3xl;
    font-weight: $weight-light;
    text-align: center;
    margin-bottom: $space-2;
  }

  &__sub {
    text-align: center;
    color: var(--color-text-muted);
    font-size: $text-sm;
    margin-bottom: $space-8;
  }

  &__error {
    display: flex;
    align-items: center;
    gap: $space-2;
    padding: $space-3 $space-4;
    background: rgb(192 57 43 / 0.1);
    color: $color-error;
    font-size: $text-sm;
    margin-bottom: $space-4;
    border: 1px solid rgb(192 57 43 / 0.3);
  }

  &__form {
    display: flex;
    flex-direction: column;
    gap: $space-5;
  }

  &__field {
    display: flex;
    flex-direction: column;
    gap: $space-2;

    label {
      @include label-caps;
      font-size: 11px;
      color: var(--color-text-secondary);
    }
  }

  &__field-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  &__forgot {
    font-size: $text-xs;
    color: var(--color-text-muted);
    text-decoration: underline;
    text-underline-offset: 2px;
    @include transition(color);

    &:hover { color: var(--color-text-primary); }
  }

  &__submit {
    @include label-caps;
    width: 100%;
    padding: $space-4;
    background: var(--color-text-primary);
    color: var(--color-bg-canvas);
    border: none;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: $space-2;
    @include transition(background);

    &:hover:not(:disabled) { background: var(--color-accent); }
    &:disabled { opacity: 0.6; cursor: not-allowed; }
  }

  &__register {
    text-align: center;
    font-size: $text-sm;
    color: var(--color-text-muted);
    margin-top: $space-6;

    a {
      color: var(--color-text-primary);
      font-weight: $weight-medium;
      text-decoration: underline;
      text-underline-offset: 2px;
    }
  }

  &__divider {
    position: relative;
    text-align: center;
    margin: $space-6 0;

    &::before {
      content: '';
      position: absolute;
      top: 50%;
      left: 0;
      right: 0;
      height: 1px;
      background: var(--color-border);
    }

    span {
      position: relative;
      background: var(--color-bg-canvas);
      padding: 0 $space-4;
      font-size: $text-xs;
      color: var(--color-text-muted);
    }
  }

  &__social {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: $space-3;
  }

  &__social-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: $space-2;
    padding: $space-3;
    border: 1px solid var(--color-border);
    background: none;
    cursor: pointer;
    font-size: $text-sm;
    color: var(--color-text-primary);
    @include transition(border-color);

    &:hover { border-color: var(--color-text-primary); }
  }
}
</style>
```

---

## Wishlist Page `src/pages/wishlist.vue`

```vue
<script setup lang="ts">
import { useWishlist } from '@/composables/useWishlist'
import ProductCard from '@/components/ui/ProductCard/ProductCard.vue'

const { items, isEmpty, remove } = useWishlist()
</script>

<template>
  <div class="wishlist-page">
    <div class="wishlist-page__header">
      <h1 class="wishlist-page__title">My Wishlist</h1>
      <span class="wishlist-page__count">{{ items.length }} items</span>
    </div>

    <div v-if="isEmpty" class="wishlist-page__empty">
      <i class="pi pi-heart" />
      <p>Your wishlist is empty</p>
      <RouterLink to="/women">Discover Pieces You'll Love</RouterLink>
    </div>

    <div v-else class="wishlist-page__grid">
      <ProductCard
        v-for="product in items"
        :key="product.id"
        :product="product as any"
        @quick-view="() => {}"
      />
    </div>
  </div>
</template>

<style lang="scss" scoped>
.wishlist-page {
  max-width: 1400px;
  margin: 0 auto;
  padding: $space-8 $space-6;

  @include respond-to('xl') { padding: $space-10 $space-12; }

  &__header {
    display: flex;
    align-items: baseline;
    gap: $space-4;
    margin-bottom: $space-8;
  }

  &__title {
    font-family: $font-display;
    font-size: $text-3xl;
    font-weight: $weight-light;
    color: var(--color-text-primary);
  }

  &__count {
    @include label-caps;
    color: var(--color-text-muted);
  }

  &__empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: $space-4;
    padding: $space-20;
    text-align: center;

    .pi { font-size: 60px; color: var(--color-border-strong); }
    p { font-family: $font-display; font-size: $text-xl; color: var(--color-text-muted); }
    a {
      @include label-caps;
      padding: $space-3 $space-8;
      background: var(--color-text-primary);
      color: var(--color-bg-canvas);
      text-decoration: none;
    }
  }

  &__grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: $space-6;

    @include respond-to('md') { grid-template-columns: repeat(3, 1fr); }
    @include respond-to('lg') { grid-template-columns: repeat(4, 1fr); }
  }
}
</style>
```

---

## Order Confirmation `src/pages/checkout/confirmation.vue`

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ordersService } from '@/services/orders'
import { useCartStore } from '@/stores/cart'
import type { Order } from '@/types/user'

const route   = useRoute()
const router  = useRouter()
const cart    = useCartStore()

const order   = ref<Order | null>(null)
const loading = ref(true)

onMounted(async () => {
  try {
    order.value = await ordersService.get(route.params.orderId as string)
    cart.clearCart()
  } catch {
    void router.push('/')
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div v-if="!loading && order" class="confirmation">
    <div class="confirmation__icon">
      <i class="pi pi-check-circle" />
    </div>

    <h1 class="confirmation__title">Order Confirmed</h1>
    <p class="confirmation__order-num">Order {{ order.number }}</p>
    <p class="confirmation__message">
      Thank you for your order. We'll send a confirmation email to
      <strong>{{ order.shippingAddress.firstName }}</strong> shortly.
    </p>

    <div class="confirmation__summary">
      <h2>Order Summary</h2>
      <div v-for="item in order.items" :key="item.productId" class="confirmation__item">
        <img :src="item.image" :alt="item.name" class="confirmation__item-image" />
        <div class="confirmation__item-info">
          <p class="confirmation__item-brand">{{ item.brand }}</p>
          <p class="confirmation__item-name">{{ item.name }}</p>
          <p class="confirmation__item-meta">Size: {{ item.size }} · {{ item.color }}</p>
        </div>
        <p class="confirmation__item-price">
          {{ item.currency }} {{ (item.price * item.quantity).toLocaleString() }}
        </p>
      </div>

      <div class="confirmation__totals">
        <div class="confirmation__total-row">
          <span>Subtotal</span>
          <span>{{ order.summary.currency }} {{ order.summary.subtotal.toLocaleString() }}</span>
        </div>
        <div class="confirmation__total-row">
          <span>Shipping</span>
          <span>{{ order.summary.shipping === 0 ? 'FREE' : `${order.summary.currency} ${order.summary.shipping}` }}</span>
        </div>
        <div class="confirmation__total-row confirmation__total-row--grand">
          <span>Total Paid</span>
          <span>{{ order.summary.currency }} {{ order.summary.total.toLocaleString() }}</span>
        </div>
      </div>
    </div>

    <div class="confirmation__actions">
      <RouterLink to="/account/orders" class="confirmation__btn confirmation__btn--primary">
        Track My Order
      </RouterLink>
      <RouterLink to="/women" class="confirmation__btn confirmation__btn--ghost">
        Continue Shopping
      </RouterLink>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.confirmation {
  max-width: 680px;
  margin: $space-12 auto;
  padding: 0 $space-6;
  text-align: center;

  &__icon .pi {
    font-size: 64px;
    color: $color-success;
    display: block;
    margin-bottom: $space-6;
  }

  &__title {
    font-family: $font-display;
    font-size: $text-3xl;
    font-weight: $weight-light;
    color: var(--color-text-primary);
    margin-bottom: $space-2;
  }

  &__order-num {
    @include label-caps;
    color: var(--color-text-muted);
    margin-bottom: $space-4;
  }

  &__message {
    font-size: $text-sm;
    color: var(--color-text-secondary);
    margin-bottom: $space-10;
    line-height: $leading-relaxed;
  }

  &__summary {
    text-align: left;
    border: 1px solid var(--color-border);
    padding: $space-6;
    margin-bottom: $space-8;

    h2 {
      font-family: $font-display;
      font-size: $text-lg;
      font-weight: $weight-light;
      margin-bottom: $space-6;
    }
  }

  &__item {
    display: grid;
    grid-template-columns: 80px 1fr auto;
    gap: $space-4;
    margin-bottom: $space-5;
    padding-bottom: $space-5;
    border-bottom: 1px solid var(--color-border);

    &:last-of-type { border-bottom: none; }
  }

  &__item-image {
    width: 80px;
    height: 100px;
    object-fit: cover;
  }

  &__item-brand {
    @include label-caps;
    font-size: 10px;
    margin-bottom: 2px;
  }

  &__item-name {
    font-size: $text-sm;
    color: var(--color-text-primary);
    margin-bottom: $space-1;
  }

  &__item-meta {
    font-size: $text-xs;
    color: var(--color-text-muted);
  }

  &__item-price {
    font-size: $text-sm;
    font-weight: $weight-medium;
    color: var(--color-text-primary);
    white-space: nowrap;
  }

  &__totals {
    border-top: 1px solid var(--color-border);
    padding-top: $space-5;
  }

  &__total-row {
    display: flex;
    justify-content: space-between;
    font-size: $text-sm;
    color: var(--color-text-secondary);
    margin-bottom: $space-2;

    &--grand {
      font-size: $text-md;
      font-weight: $weight-semibold;
      color: var(--color-text-primary);
      margin-top: $space-3;
      padding-top: $space-3;
      border-top: 1px solid var(--color-border);
    }
  }

  &__actions {
    display: flex;
    gap: $space-4;
    justify-content: center;
    flex-wrap: wrap;
  }

  &__btn {
    @include label-caps;
    padding: $space-3 $space-8;
    text-decoration: none;
    @include transition(background color);

    &--primary {
      background: var(--color-text-primary);
      color: var(--color-bg-canvas);
      &:hover { background: var(--color-accent); }
    }

    &--ghost {
      border: 1px solid var(--color-border);
      color: var(--color-text-primary);
      &:hover { border-color: var(--color-text-primary); }
    }
  }
}
</style>
```
# UI Components — Continued

---

## `src/layouts/AccountLayout.vue` (completed)

```vue
      &--active {
        color: var(--color-text-primary);
        border-left-color: var(--color-accent);
        background: var(--color-bg-elevated);
        font-weight: $weight-medium;
      }
    }

  &__signout {
    display: flex;
    align-items: center;
    gap: $space-3;
    width: 100%;
    padding: $space-3 $space-4;
    margin-top: $space-4;
    background: none;
    border: 1px solid var(--color-border);
    font-size: $text-sm;
    color: var(--color-text-muted);
    cursor: pointer;
    @include transition(all);

    .pi { font-size: 15px; }

    &:hover {
      border-color: $color-error;
      color: $color-error;
    }
  }
}
</style>
```

---

## `src/layouts/CheckoutLayout.vue`

```vue
<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useCartStore } from '@/stores/cart'

const route = useRoute()
const cart  = useCartStore()

const steps = [
  { name: 'cart',              label: 'Bag',      route: '/checkout' },
  { name: 'checkout-shipping', label: 'Shipping', route: '/checkout/shipping' },
  { name: 'checkout-payment',  label: 'Payment',  route: '/checkout/payment' },
  { name: 'checkout-review',   label: 'Review',   route: '/checkout/review' },
]

const currentStepIndex = computed(() =>
  steps.findIndex(s => s.name === route.name)
)
</script>

<template>
  <div class="checkout-layout">
    <!-- Minimal Header -->
    <header class="checkout-layout__header">
      <RouterLink to="/" class="checkout-layout__logo">MAISON</RouterLink>
      <div class="checkout-layout__secure">
        <i class="pi pi-lock" />
        <span>Secure Checkout</span>
      </div>
    </header>

    <!-- Progress Stepper -->
    <div class="checkout-layout__stepper">
      <div
        v-for="(step, i) in steps"
        :key="step.name"
        class="checkout-layout__step"
        :class="{
          'checkout-layout__step--complete': i < currentStepIndex,
          'checkout-layout__step--active':   i === currentStepIndex,
        }"
      >
        <div class="checkout-layout__step-dot">
          <i v-if="i < currentStepIndex" class="pi pi-check" />
          <span v-else>{{ i + 1 }}</span>
        </div>
        <span class="checkout-layout__step-label">{{ step.label }}</span>
        <div v-if="i < steps.length - 1" class="checkout-layout__step-line" />
      </div>
    </div>

    <!-- Main Content + Order Summary -->
    <div class="checkout-layout__body">
      <main class="checkout-layout__main">
        <RouterView />
      </main>

      <!-- Order Summary Sidebar (not shown on confirmation) -->
      <aside v-if="route.name !== 'order-confirmation'" class="checkout-layout__summary">
        <h3 class="checkout-layout__summary-title">Order Summary</h3>
        <div class="checkout-layout__summary-items">
          <div
            v-for="item in cart.items"
            :key="item.id"
            class="checkout-layout__summary-item"
          >
            <div class="checkout-layout__summary-item-img-wrap">
              <img :src="item.image" :alt="item.product.name" />
              <span class="checkout-layout__summary-qty">{{ item.quantity }}</span>
            </div>
            <div class="checkout-layout__summary-item-info">
              <p class="checkout-layout__summary-item-brand">{{ item.product.brand.name }}</p>
              <p class="checkout-layout__summary-item-name">{{ item.product.name }}</p>
              <p class="checkout-layout__summary-item-meta">
                {{ item.variant.size }} · {{ item.color.name }}
              </p>
            </div>
            <span class="checkout-layout__summary-item-price">
              SGD {{ (item.price * item.quantity).toLocaleString() }}
            </span>
          </div>
        </div>

        <div class="checkout-layout__summary-totals">
          <div class="checkout-layout__totals-row">
            <span>Subtotal</span>
            <span>SGD {{ cart.summary.subtotal.toLocaleString() }}</span>
          </div>
          <div class="checkout-layout__totals-row">
            <span>Shipping</span>
            <span>{{ cart.summary.shipping === 0 ? 'FREE' : `SGD ${cart.summary.shipping}` }}</span>
          </div>
          <div v-if="cart.summary.discount > 0" class="checkout-layout__totals-row checkout-layout__totals-row--discount">
            <span>Discount</span>
            <span>−SGD {{ cart.summary.discount.toLocaleString() }}</span>
          </div>
          <div class="checkout-layout__totals-row">
            <span>GST (9%)</span>
            <span>SGD {{ cart.summary.tax.toFixed(2) }}</span>
          </div>
          <div class="checkout-layout__totals-grand">
            <span>Total</span>
            <span>SGD {{ cart.summary.total.toLocaleString() }}</span>
          </div>
        </div>
      </aside>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.checkout-layout {
  min-height: 100vh;
  background: var(--color-bg-surface);

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: $space-5 $space-8;
    background: var(--color-bg-canvas);
    border-bottom: 1px solid var(--color-border);
  }

  &__logo {
    font-family: $font-display;
    font-size: $text-xl;
    font-weight: $weight-light;
    letter-spacing: $tracking-widest;
    color: var(--color-text-primary);
    text-decoration: none;
  }

  &__secure {
    display: flex;
    align-items: center;
    gap: $space-2;
    font-size: $text-xs;
    color: var(--color-text-muted);

    .pi { color: $color-success; }
  }

  &__stepper {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: $space-6 $space-8;
    background: var(--color-bg-canvas);
    border-bottom: 1px solid var(--color-border);
    gap: 0;
  }

  &__step {
    display: flex;
    align-items: center;
    gap: $space-2;
    opacity: 0.4;
    @include transition(opacity);

    &--active,
    &--complete { opacity: 1; }
  }

  &__step-dot {
    width: 28px;
    height: 28px;
    border-radius: 50%;
    border: 1px solid var(--color-border-strong);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: $text-xs;
    font-weight: $weight-medium;
    color: var(--color-text-muted);
    flex-shrink: 0;

    .checkout-layout__step--active & {
      background: var(--color-text-primary);
      border-color: var(--color-text-primary);
      color: var(--color-bg-canvas);
    }

    .checkout-layout__step--complete & {
      background: $color-success;
      border-color: $color-success;
      color: white;
      .pi { font-size: 12px; }
    }
  }

  &__step-label {
    @include label-caps;
    font-size: 11px;
    color: var(--color-text-muted);

    .checkout-layout__step--active & { color: var(--color-text-primary); }
  }

  &__step-line {
    width: 60px;
    height: 1px;
    background: var(--color-border);
    margin: 0 $space-4;

    @include respond-to('md') { width: 80px; }
  }

  &__body {
    display: grid;
    grid-template-columns: 1fr;
    gap: 0;
    max-width: 1100px;
    margin: 0 auto;
    padding: $space-8 $space-6;
    align-items: start;

    @include respond-to('lg') {
      grid-template-columns: 1fr 380px;
      gap: $space-8;
    }
  }

  &__main {
    background: var(--color-bg-canvas);
    padding: $space-8;
    border: 1px solid var(--color-border);
  }

  &__summary {
    background: var(--color-bg-canvas);
    padding: $space-6;
    border: 1px solid var(--color-border);
    position: sticky;
    top: 20px;
  }

  &__summary-title {
    @include label-caps;
    margin-bottom: $space-5;
    padding-bottom: $space-4;
    border-bottom: 1px solid var(--color-border);
    color: var(--color-text-primary);
  }

  &__summary-items {
    display: flex;
    flex-direction: column;
    gap: $space-4;
    margin-bottom: $space-5;
    padding-bottom: $space-5;
    border-bottom: 1px solid var(--color-border);
  }

  &__summary-item {
    display: grid;
    grid-template-columns: 56px 1fr auto;
    gap: $space-3;
    align-items: start;
  }

  &__summary-item-img-wrap {
    position: relative;

    img {
      width: 56px;
      height: 72px;
      object-fit: cover;
      background: var(--color-bg-elevated);
    }
  }

  &__summary-qty {
    position: absolute;
    top: -8px;
    right: -8px;
    width: 18px;
    height: 18px;
    background: var(--color-text-primary);
    color: var(--color-bg-canvas);
    border-radius: 50%;
    font-size: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: $weight-semibold;
  }

  &__summary-item-brand {
    @include label-caps;
    font-size: 9px;
    color: var(--color-text-muted);
    margin-bottom: 2px;
  }

  &__summary-item-name {
    font-size: $text-xs;
    color: var(--color-text-primary);
    @include truncate(2);
    margin-bottom: 2px;
  }

  &__summary-item-meta {
    font-size: 10px;
    color: var(--color-text-muted);
  }

  &__summary-item-price {
    font-size: $text-xs;
    font-weight: $weight-medium;
    color: var(--color-text-primary);
    white-space: nowrap;
  }

  &__summary-totals {
    display: flex;
    flex-direction: column;
    gap: $space-2;
  }

  &__totals-row {
    display: flex;
    justify-content: space-between;
    font-size: $text-xs;
    color: var(--color-text-secondary);

    &--discount { color: $color-success; }
  }

  &__totals-grand {
    display: flex;
    justify-content: space-between;
    font-size: $text-sm;
    font-weight: $weight-semibold;
    color: var(--color-text-primary);
    padding-top: $space-3;
    margin-top: $space-2;
    border-top: 1px solid var(--color-border);
  }
}
</style>
```

---

## `src/layouts/AuthLayout.vue`

```vue
<template>
  <div class="auth-layout">
    <!-- Decorative Left Panel -->
    <div class="auth-layout__panel" aria-hidden="true">
      <img
        src="https://picsum.photos/seed/auth/800/1200"
        alt=""
        class="auth-layout__panel-image"
      />
      <div class="auth-layout__panel-overlay">
        <span class="auth-layout__panel-logo">MAISON</span>
        <blockquote class="auth-layout__panel-quote">
          "Fashion is the armor to survive the reality of everyday life."
          <cite>— Bill Cunningham</cite>
        </blockquote>
      </div>
    </div>

    <!-- Form Panel -->
    <div class="auth-layout__form-panel">
      <RouterLink to="/" class="auth-layout__back">
        <i class="pi pi-arrow-left" /> Back to store
      </RouterLink>
      <div class="auth-layout__form-inner">
        <RouterView />
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.auth-layout {
  display: grid;
  grid-template-columns: 1fr;
  min-height: 100vh;

  @include respond-to('lg') {
    grid-template-columns: 1fr 1fr;
  }

  &__panel {
    display: none;
    position: relative;
    overflow: hidden;

    @include respond-to('lg') { display: block; }
  }

  &__panel-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  &__panel-overlay {
    position: absolute;
    inset: 0;
    background: linear-gradient(
      to top,
      rgb(13 12 10 / 0.8) 0%,
      transparent 60%
    );
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    padding: $space-10;
  }

  &__panel-logo {
    font-family: $font-display;
    font-size: $text-2xl;
    font-weight: $weight-light;
    letter-spacing: $tracking-widest;
    color: white;
  }

  &__panel-quote {
    font-family: $font-display;
    font-size: $text-lg;
    font-weight: $weight-light;
    font-style: italic;
    color: rgb(255 255 255 / 0.9);
    line-height: $leading-relaxed;
    border: none;
    margin: 0;
    padding: 0;

    cite {
      display: block;
      font-style: normal;
      font-size: $text-sm;
      color: rgb(255 255 255 / 0.6);
      margin-top: $space-3;
      font-family: $font-body;
      letter-spacing: $tracking-wide;
    }
  }

  &__form-panel {
    display: flex;
    flex-direction: column;
    padding: $space-8;
    overflow-y: auto;
  }

  &__back {
    display: inline-flex;
    align-items: center;
    gap: $space-2;
    font-size: $text-xs;
    color: var(--color-text-muted);
    text-decoration: none;
    margin-bottom: $space-10;
    @include transition(color);

    .pi { font-size: 11px; }
    &:hover { color: var(--color-text-primary); }
  }

  &__form-inner {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}
</style>
```

---

## `src/components/feature/cart/PromoCodeInput.vue`

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useCartStore } from '@/stores/cart'
import InputText from 'primevue/inputtext'

const cart    = useCartStore()
const code    = ref('')
const message = ref('')
const valid   = ref<boolean | null>(null)

async function apply() {
  if (!code.value.trim()) return
  const result = await cart.applyPromoCode(code.value.trim())
  valid.value   = result.isValid
  message.value = result.message ?? ''
  if (result.isValid) code.value = ''
}

function remove() {
  cart.removePromoCode()
  code.value    = ''
  message.value = ''
  valid.value   = null
}
</script>

<template>
  <div class="promo-code">
    <!-- Applied state -->
    <div v-if="cart.promoCode?.isValid" class="promo-code__applied">
      <div class="promo-code__applied-info">
        <i class="pi pi-tag" />
        <span class="promo-code__applied-code">{{ cart.promoCode.code }}</span>
        <span class="promo-code__applied-desc">
          {{ cart.promoCode.type === 'percentage'
            ? `${cart.promoCode.value}% off`
            : `SGD ${cart.promoCode.value} off` }}
        </span>
      </div>
      <button class="promo-code__remove" @click="remove">
        <i class="pi pi-times" />
      </button>
    </div>

    <!-- Input state -->
    <div v-else class="promo-code__input-wrap">
      <InputText
        v-model="code"
        placeholder="Promo code"
        class="promo-code__input"
        @keydown.enter="apply"
      />
      <button
        class="promo-code__apply-btn"
        :disabled="!code.trim() || cart.isLoading"
        @click="apply"
      >
        <i v-if="cart.isLoading" class="pi pi-spin pi-spinner" />
        <span v-else>Apply</span>
      </button>
    </div>

    <!-- Feedback message -->
    <p
      v-if="message"
      class="promo-code__message"
      :class="valid ? 'promo-code__message--success' : 'promo-code__message--error'"
    >
      <i :class="valid ? 'pi pi-check-circle' : 'pi pi-exclamation-circle'" />
      {{ message }}
    </p>
  </div>
</template>

<style lang="scss" scoped>
.promo-code {
  padding: $space-5 0;
  border-bottom: 1px solid var(--color-border);

  &__applied {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: $space-3 $space-4;
    background: rgb(39 174 96 / 0.08);
    border: 1px solid rgb(39 174 96 / 0.3);
  }

  &__applied-info {
    display: flex;
    align-items: center;
    gap: $space-2;
    font-size: $text-sm;

    .pi { color: $color-success; }
  }

  &__applied-code {
    font-weight: $weight-semibold;
    color: var(--color-text-primary);
    letter-spacing: $tracking-wide;
  }

  &__applied-desc {
    color: $color-success;
    font-size: $text-xs;
  }

  &__remove {
    background: none;
    border: none;
    cursor: pointer;
    color: var(--color-text-muted);
    padding: $space-1;
    @include transition(color);

    .pi { font-size: 12px; }
    &:hover { color: $color-error; }
  }

  &__input-wrap {
    display: flex;
    gap: $space-2;
    border: 1px solid var(--color-border);
  }

  &__input {
    flex: 1;
    border: none !important;
    border-radius: 0 !important;
    background: transparent;
    font-size: $text-sm;
    letter-spacing: $tracking-wide;

    &:focus { box-shadow: none !important; }
  }

  &__apply-btn {
    @include label-caps;
    font-size: 11px;
    padding: 0 $space-4;
    background: none;
    border: none;
    border-left: 1px solid var(--color-border);
    cursor: pointer;
    color: var(--color-text-primary);
    @include transition(background color);
    min-width: 60px;

    &:hover:not(:disabled) {
      background: var(--color-text-primary);
      color: var(--color-bg-canvas);
    }

    &:disabled { color: var(--color-border-strong); cursor: not-allowed; }
  }

  &__message {
    display: flex;
    align-items: center;
    gap: $space-2;
    font-size: $text-xs;
    margin-top: $space-2;

    &--success { color: $color-success; }
    &--error   { color: $color-error; }
  }
}
</style>
```

---

## `src/components/feature/cart/FreeShippingBar.vue`

```vue
<script setup lang="ts">
import { computed } from 'vue'
import type { CartSummary } from '@/types/cart'

const props = defineProps<{ summary: CartSummary }>()

const progress = computed(() =>
  Math.min(100, (props.summary.subtotal / props.summary.freeShippingThreshold) * 100)
)

const isQualified = computed(() => props.summary.shipping === 0)
</script>

<template>
  <div class="free-shipping-bar">
    <p class="free-shipping-bar__message">
      <template v-if="isQualified">
        <i class="pi pi-check-circle" />
        You qualify for <strong>free shipping</strong>!
      </template>
      <template v-else>
        Add <strong>SGD {{ summary.amountToFreeShipping.toFixed(0) }}</strong> more for free shipping
      </template>
    </p>
    <div class="free-shipping-bar__track">
      <div
        class="free-shipping-bar__fill"
        :style="{ width: `${progress}%` }"
        :class="{ 'free-shipping-bar__fill--complete': isQualified }"
      />
    </div>
  </div>
</template>

<style lang="scss" scoped>
.free-shipping-bar {
  padding: $space-4;
  background: var(--color-bg-elevated);
  margin-bottom: $space-4;

  &__message {
    font-size: $text-xs;
    color: var(--color-text-secondary);
    margin-bottom: $space-2;
    display: flex;
    align-items: center;
    gap: $space-2;

    strong { color: var(--color-text-primary); }

    .pi { color: $color-success; }
  }

  &__track {
    height: 3px;
    background: var(--color-border);
  }

  &__fill {
    height: 100%;
    background: var(--color-text-primary);
    @include transition(width, $duration-slow);

    &--complete { background: $color-success; }
  }
}
</style>
```

---

## `src/components/feature/cart/CartSummaryPanel.vue`

```vue
<script setup lang="ts">
import type { CartSummary } from '@/types/cart'

defineProps<{ summary: CartSummary }>()
</script>

<template>
  <div class="cart-summary-panel">
    <div class="cart-summary-panel__row">
      <span>Subtotal</span>
      <span>SGD {{ summary.subtotal.toLocaleString() }}</span>
    </div>
    <div v-if="summary.discount > 0" class="cart-summary-panel__row cart-summary-panel__row--discount">
      <span>Discount</span>
      <span>−SGD {{ summary.discount.toLocaleString() }}</span>
    </div>
    <div class="cart-summary-panel__row">
      <span>Shipping</span>
      <span>{{ summary.shipping === 0 ? 'FREE' : `SGD ${summary.shipping}` }}</span>
    </div>
    <div class="cart-summary-panel__row">
      <span>GST (9%)</span>
      <span>SGD {{ summary.tax.toFixed(2) }}</span>
    </div>
    <div class="cart-summary-panel__total">
      <span>Estimated Total</span>
      <span>SGD {{ summary.total.toLocaleString() }}</span>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.cart-summary-panel {
  padding: $space-5 0;
  border-top: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  gap: $space-3;

  &__row {
    display: flex;
    justify-content: space-between;
    font-size: $text-sm;
    color: var(--color-text-secondary);

    &--discount { color: $color-success; }
  }

  &__total {
    display: flex;
    justify-content: space-between;
    font-size: $text-md;
    font-weight: $weight-semibold;
    color: var(--color-text-primary);
    padding-top: $space-3;
    border-top: 1px solid var(--color-border);
  }
}
</style>
```

---

## `src/components/ui/SortDropdown.vue`

```vue
<script setup lang="ts">
import Select from 'primevue/select'
import type { SortOption } from '@/types/filters'

defineProps<{ value: SortOption }>()
const emit = defineEmits<{ change: [v: SortOption] }>()

const options: { label: string; value: SortOption }[] = [
  { label: 'Recommended',    value: 'recommended' },
  { label: 'New In',         value: 'newest' },
  { label: 'Price: Low–High', value: 'price-asc' },
  { label: 'Price: High–Low', value: 'price-desc' },
  { label: 'Most Reviewed',  value: 'most-reviewed' },
  { label: 'Top Rated',      value: 'top-rated' },
  { label: 'Sale',           value: 'sale' },
]
</script>

<template>
  <Select
    :model-value="value"
    :options="options"
    option-label="label"
    option-value="value"
    placeholder="Sort by"
    class="sort-dropdown"
    @change="(e) => emit('change', e.value)"
  />
</template>

<style lang="scss">
.sort-dropdown {
  border-radius: 0 !important;
  border-color: var(--color-border) !important;
  font-size: $text-sm !important;

  .p-select-label {
    font-family: $font-body !important;
    font-size: $text-sm !important;
    padding: $space-2 $space-3 !important;
  }
}
</style>
```

---

## `src/components/feature/product/ProductQuickView.vue`

```vue
<script setup lang="ts">
import { ref, computed } from 'vue'
import Dialog from 'primevue/dialog'
import type { ProductListItem } from '@/types/product'
import { useProduct } from '@/composables/useProducts'
import { useCart } from '@/composables/useCart'
import SizeSelector from '@/components/ui/SizeSelector.vue'
import Skeleton from 'primevue/skeleton'

const props = defineProps<{ product: ProductListItem }>()
const emit  = defineEmits<{ close: [] }>()

const { product: fullProduct, isLoading } = useProduct(props.product.slug)
const { addToCart } = useCart()

const selectedSize = ref('')

const variants = computed(() => fullProduct.value?.variants ?? [])

function handleAdd() {
  if (!fullProduct.value || !selectedSize.value) return
  const variant = variants.value.find(v => v.size === selectedSize.value)
  const color   = fullProduct.value.colors[0]
  if (variant && color) addToCart(fullProduct.value, variant, color)
}
</script>

<template>
  <Dialog
    :visible="true"
    modal
    :style="{ width: '90vw', maxWidth: '820px', padding: '0' }"
    :pt="{ content: { style: 'padding: 0' } }"
    @update:visible="(v) => !v && emit('close')"
  >
    <div class="quick-view">
      <!-- Image -->
      <div class="quick-view__image-col">
        <img
          :src="product.images[0]?.url"
          :alt="product.name"
          class="quick-view__image"
        />
      </div>

      <!-- Info -->
      <div class="quick-view__info-col">
        <template v-if="isLoading">
          <Skeleton height="16px" width="80px" class="mb-3" />
          <Skeleton height="28px" width="90%" class="mb-4" />
          <Skeleton height="22px" width="60px" class="mb-8" />
          <Skeleton height="80px" class="mb-6" />
          <Skeleton height="52px" />
        </template>

        <template v-else-if="fullProduct">
          <RouterLink
            :to="`/designers/${fullProduct.brand.slug}`"
            class="quick-view__brand"
          >
            {{ fullProduct.brand.name }}
          </RouterLink>
          <h2 class="quick-view__name">{{ fullProduct.name }}</h2>
          <p class="quick-view__price">
            SGD {{ fullProduct.price.toLocaleString() }}
            <span v-if="fullProduct.originalPrice" class="quick-view__price-original">
              SGD {{ fullProduct.originalPrice.toLocaleString() }}
            </span>
          </p>

          <div class="quick-view__section">
            <p class="quick-view__label">Select Size</p>
            <SizeSelector
              :variants="variants"
              :selected-size="selectedSize"
              @select="(s) => selectedSize = s"
            />
          </div>

          <button
            class="quick-view__add-btn"
            :disabled="!selectedSize"
            @click="handleAdd"
          >
            {{ selectedSize ? 'Add to Bag' : 'Select a Size' }}
          </button>

          <RouterLink
            :to="`/product/${fullProduct.slug}`"
            class="quick-view__full-link"
            @click="emit('close')"
          >
            View Full Details →
          </RouterLink>
        </template>
      </div>
    </div>
  </Dialog>
</template>

<style lang="scss" scoped>
.quick-view {
  display: grid;
  grid-template-columns: 1fr;

  @include respond-to('md') { grid-template-columns: 1fr 1fr; }

  &__image-col {
    background: var(--color-bg-elevated);
  }

  &__image {
    width: 100%;
    aspect-ratio: 3/4;
    object-fit: cover;
  }

  &__info-col {
    padding: $space-8;
    display: flex;
    flex-direction: column;
    gap: $space-4;
  }

  &__brand {
    @include label-caps;
    font-size: 11px;
    color: var(--color-text-primary);
    text-decoration: none;
    @include transition(color);
    &:hover { color: var(--color-accent); }
  }

  &__name {
    font-family: $font-display;
    font-size: $text-xl;
    font-weight: $weight-light;
    color: var(--color-text-primary);
    line-height: $leading-snug;
  }

  &__price {
    font-size: $text-lg;
    font-weight: $weight-medium;
    color: var(--color-text-primary);
    display: flex;
    align-items: baseline;
    gap: $space-3;
  }

  &__price-original {
    font-size: $text-sm;
    font-weight: $weight-regular;
    color: var(--color-text-muted);
    text-decoration: line-through;
  }

  &__section { margin-top: $space-2; }

  &__label {
    @include label-caps;
    font-size: 11px;
    color: var(--color-text-muted);
    margin-bottom: $space-3;
  }

  &__add-btn {
    @include label-caps;
    width: 100%;
    padding: $space-4;
    background: var(--color-text-primary);
    color: var(--color-bg-canvas);
    border: none;
    cursor: pointer;
    @include transition(background);

    &:hover:not(:disabled) { background: var(--color-accent); }
    &:disabled { background: var(--color-border-strong); cursor: not-allowed; }
  }

  &__full-link {
    @include label-caps;
    font-size: 11px;
    color: var(--color-text-muted);
    text-decoration: underline;
    text-underline-offset: 3px;
    text-align: center;
    @include transition(color);

    &:hover { color: var(--color-text-primary); }
  }
}
</style>
```

---

## `src/components/ui/NewsletterPopup.vue`

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useUIStore } from '@/stores/ui'
import InputText from 'primevue/inputtext'
import Dialog from 'primevue/dialog'

const uiStore = useUIStore()
const visible  = ref(false)
const email    = ref('')
const submitted = ref(false)

onMounted(() => {
  // Show after 8 seconds on first visit
  setTimeout(() => {
    if (!uiStore.newsletterShown) visible.value = true
  }, 8000)
})

function dismiss() {
  visible.value = false
  uiStore.dismissNewsletter()
}

function submit() {
  if (!email.value) return
  submitted.value = true
  setTimeout(dismiss, 2000)
}
</script>

<template>
  <Dialog
    v-model:visible="visible"
    modal
    :closable="true"
    :style="{ width: '90vw', maxWidth: '700px', padding: '0' }"
    :pt="{ content: { style: 'padding: 0' } }"
    @hide="dismiss"
  >
    <div class="newsletter-popup">
      <div class="newsletter-popup__image" aria-hidden="true">
        <img src="https://picsum.photos/seed/news1/400/600" alt="" />
        <div class="newsletter-popup__image-overlay">
          <span class="newsletter-popup__logo">MAISON</span>
        </div>
      </div>

      <div class="newsletter-popup__content">
        <template v-if="!submitted">
          <p class="newsletter-popup__eyebrow">Exclusive Access</p>
          <h2 class="newsletter-popup__title">
            Discover Fashion Before Anyone Else
          </h2>
          <p class="newsletter-popup__body">
            Join our community for early access to new arrivals, private sales,
            and curated edits — delivered directly to you.
          </p>

          <form class="newsletter-popup__form" @submit.prevent="submit">
            <InputText
              v-model="email"
              type="email"
              placeholder="Your email address"
              required
              class="newsletter-popup__input"
            />
            <button type="submit" class="newsletter-popup__submit">
              Join Now
            </button>
          </form>

          <button class="newsletter-popup__skip" @click="dismiss">
            No thanks, I'll pay full price
          </button>
        </template>

        <template v-else>
          <div class="newsletter-popup__success">
            <i class="pi pi-check-circle newsletter-popup__success-icon" />
            <h3>Welcome to Maison</h3>
            <p>You're on the list. Expect something wonderful in your inbox soon.</p>
          </div>
        </template>
      </div>
    </div>
  </Dialog>
</template>

<style lang="scss" scoped>
.newsletter-popup {
  display: grid;
  grid-template-columns: 1fr;

  @include respond-to('md') { grid-template-columns: 1fr 1fr; }

  &__image {
    display: none;
    position: relative;
    overflow: hidden;

    @include respond-to('md') { display: block; }

    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
  }

  &__image-overlay {
    position: absolute;
    inset: 0;
    background: rgb(0 0 0 / 0.3);
    display: flex;
    align-items: flex-end;
    padding: $space-8;
  }

  &__logo {
    font-family: $font-display;
    font-size: $text-2xl;
    font-weight: $weight-light;
    letter-spacing: $tracking-widest;
    color: white;
  }

  &__content {
    padding: $space-10 $space-8;
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: $space-4;
  }

  &__eyebrow {
    @include label-caps;
    color: var(--color-accent);
    font-size: 11px;
  }

  &__title {
    font-family: $font-display;
    font-size: $text-2xl;
    font-weight: $weight-light;
    line-height: $leading-tight;
    color: var(--color-text-primary);
  }

  &__body {
    font-size: $text-sm;
    color: var(--color-text-muted);
    line-height: $leading-relaxed;
  }

  &__form {
    display: flex;
    flex-direction: column;
    gap: $space-3;
    margin-top: $space-2;
  }

  &__input {
    width: 100% !important;
    border-radius: 0 !important;
    font-size: $text-sm !important;
  }

  &__submit {
    @include label-caps;
    width: 100%;
    padding: $space-4;
    background: var(--color-text-primary);
    color: var(--color-bg-canvas);
    border: none;
    cursor: pointer;
    @include transition(background);

    &:hover { background: var(--color-accent); }
  }

  &__skip {
    background: none;
    border: none;
    cursor: pointer;
    font-size: $text-xs;
    color: var(--color-text-muted);
    text-decoration: underline;
    text-underline-offset: 2px;
    text-align: center;
    padding: $space-2 0;
    @include transition(color);

    &:hover { color: var(--color-text-primary); }
  }

  &__success {
    text-align: center;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: $space-4;
    padding: $space-8 0;

    h3 {
      font-family: $font-display;
      font-size: $text-xl;
      font-weight: $weight-light;
    }

    p {
      font-size: $text-sm;
      color: var(--color-text-muted);
      line-height: $leading-relaxed;
    }
  }

  &__success-icon {
    font-size: 52px;
    color: $color-success;
  }
}
</style>
```

---

## `src/components/feature/homepage/FeaturedCategories.vue`

```vue
<script setup lang="ts">
const categories = [
  { label: 'Dresses',   image: 'https://picsum.photos/seed/cat1/400/500', href: '/women/dresses' },
  { label: 'Bags',      image: 'https://picsum.photos/seed/cat2/400/500', href: '/women/bags' },
  { label: 'Shoes',     image: 'https://picsum.photos/seed/cat3/400/500', href: '/women/shoes' },
  { label: 'Jewellery', image: 'https://picsum.photos/seed/cat4/400/500', href: '/women/jewellery' },
  { label: 'Knitwear',  image: 'https://picsum.photos/seed/cat5/400/500', href: '/women/knitwear' },
  { label: 'Coats',     image: 'https://picsum.photos/seed/cat6/400/500', href: '/women/coats' },
]
</script>

<template>
  <section class="featured-categories">
    <div class="featured-categories__header">
      <h2 class="featured-categories__title">Shop by Category</h2>
    </div>
    <div class="featured-categories__grid">
      <RouterLink
        v-for="cat in categories"
        :key="cat.label"
        :to="cat.href"
        class="featured-categories__item"
      >
        <div class="featured-categories__image-wrap">
          <img :src="cat.image" :alt="cat.label" loading="lazy" />
        </div>
        <span class="featured-categories__label">{{ cat.label }}</span>
      </RouterLink>
    </div>
  </section>
</template>

<style lang="scss" scoped>
.featured-categories {
  padding: $space-12 $space-6;
  max-width: 1400px;
  margin: 0 auto;

  @include respond-to('xl') { padding: $space-16 $space-12; }

  &__header {
    margin-bottom: $space-8;
  }

  &__title {
    font-family: $font-display;
    font-size: $text-2xl;
    font-weight: $weight-light;
    color: var(--color-text-primary);
  }

  &__grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: $space-4;

    @include respond-to('lg') {
      grid-template-columns: repeat(6, 1fr);
    }
  }

  &__item {
    text-decoration: none;
    display: block;
  }

  &__image-wrap {
    overflow: hidden;
    margin-bottom: $space-3;
    background: var(--color-bg-elevated);

    img {
      width: 100%;
      aspect-ratio: 4/5;
      object-fit: cover;
      @include transition(transform, $duration-slow);
    }
  }

  &__item:hover &__image-wrap img {
    transform: scale(1.04);
  }

  &__label {
    @include label-caps;
    font-size: 11px;
    color: var(--color-text-primary);
    display: block;
    text-align: center;
  }
}
</style>
```

---

## `src/components/feature/homepage/EditorialGrid.vue`

```vue
<script setup lang="ts">
const editorials = [
  {
    id: 1,
    tag: 'The Edit',
    title: 'Resort Season Essentials',
    image: 'https://picsum.photos/seed/ed1/800/1000',
    href: '/lookbook',
    size: 'large',
  },
  {
    id: 2,
    tag: 'Style Guide',
    title: 'How to Wear Linen This Season',
    image: 'https://picsum.photos/seed/ed2/600/700',
    href: '/women/linen',
    size: 'small',
  },
  {
    id: 3,
    tag: 'New Designers',
    title: 'Rising Stars to Watch',
    image: 'https://picsum.photos/seed/ed3/600/700',
    href: '/designers',
    size: 'small',
  },
]
</script>

<template>
  <section class="editorial-grid">
    <div class="editorial-grid__inner">
      <div class="editorial-grid__eyebrow">
        <span class="editorial-grid__tag">Editorial</span>
        <h2 class="editorial-grid__title">From the Edit</h2>
      </div>

      <div class="editorial-grid__layout">
        <RouterLink
          v-for="item in editorials"
          :key="item.id"
          :to="item.href"
          class="editorial-grid__card"
          :class="`editorial-grid__card--${item.size}`"
        >
          <div class="editorial-grid__card-image">
            <img :src="item.image" :alt="item.title" loading="lazy" />
          </div>
          <div class="editorial-grid__card-content">
            <span class="editorial-grid__card-tag">{{ item.tag }}</span>
            <h3 class="editorial-grid__card-title">{{ item.title }}</h3>
            <span class="editorial-grid__card-cta">Read →</span>
          </div>
        </RouterLink>
      </div>
    </div>
  </section>
</template>

<style lang="scss" scoped>
.editorial-grid {
  background: var(--color-bg-surface);
  padding: $space-16 0;

  &__inner {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 $space-6;

    @include respond-to('xl') { padding: 0 $space-12; }
  }

  &__eyebrow {
    margin-bottom: $space-8;
  }

  &__tag {
    @include label-caps;
    color: var(--color-accent);
    display: block;
    margin-bottom: $space-2;
  }

  &__title {
    font-family: $font-display;
    font-size: $text-2xl;
    font-weight: $weight-light;
    color: var(--color-text-primary);
  }

  &__layout {
    display: grid;
    grid-template-columns: 1fr;
    gap: $space-4;

    @include respond-to('md') {
      grid-template-columns: 1fr 1fr;
      grid-template-rows: auto auto;
    }

    @include respond-to('lg') {
      grid-template-columns: 1.4fr 1fr;
    }
  }

  &__card {
    display: block;
    text-decoration: none;
    position: relative;
    overflow: hidden;

    &--large {
      @include respond-to('md') {
        grid-row: 1 / 3;
      }
    }
  }

  &__card-image {
    overflow: hidden;
    background: var(--color-bg-elevated);

    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      @include transition(transform, $duration-slow);

      .editorial-grid__card--large & {
        aspect-ratio: 4/5;
        @include respond-to('md') { aspect-ratio: unset; height: 100%; }
      }

      .editorial-grid__card--small & {
        aspect-ratio: 3/2;
      }
    }
  }

  &__card:hover &__card-image img {
    transform: scale(1.04);
  }

  &__card-content {
    padding: $space-4 0 0;
  }

  &__card-tag {
    @include label-caps;
    font-size: 10px;
    color: var(--color-text-muted);
    display: block;
    margin-bottom: $space-2;
  }

  &__card-title {
    font-family: $font-display;
    font-size: $text-lg;
    font-weight: $weight-light;
    color: var(--color-text-primary);
    margin-bottom: $space-3;
    line-height: $leading-snug;

    .editorial-grid__card--large & {
      font-size: $text-xl;
    }
  }

  &__card-cta {
    font-size: $text-xs;
    color: var(--color-accent);
    @include transition(letter-spacing);
  }

  &__card:hover &__card-cta {
    letter-spacing: 0.08em;
  }
}
</style>
```

---

## `src/components/feature/homepage/BrandStrip.vue`

```vue
<script setup lang="ts">
const brands = [
  { name: 'Loewe',           slug: 'loewe' },
  { name: 'Bottega Veneta',  slug: 'bottega-veneta' },
  { name: 'Jacquemus',       slug: 'jacquemus' },
  { name: 'The Row',         slug: 'the-row' },
  { name: 'Toteme',          slug: 'toteme' },
  { name: 'Loro Piana',      slug: 'loro-piana' },
  { name: 'Brunello Cucinelli', slug: 'brunello-cucinelli' },
  { name: 'Lemaire',         slug: 'lemaire' },
]
</script>

<template>
  <section class="brand-strip">
    <div class="brand-strip__inner">
      <p class="brand-strip__label">Featured Designers</p>
      <div class="brand-strip__track">
        <RouterLink
          v-for="brand in brands"
          :key="brand.slug"
          :to="`/designers/${brand.slug}`"
          class="brand-strip__brand"
        >
          {{ brand.name }}
        </RouterLink>
      </div>
    </div>
  </section>
</template>

<style lang="scss" scoped>
.brand-strip {
  border-top: 1px solid var(--color-border);
  border-bottom: 1px solid var(--color-border);
  padding: $space-5 0;
  overflow: hidden;

  &__inner {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 $space-6;
    display: flex;
    align-items: center;
    gap: $space-8;

    @include respond-to('xl') { padding: 0 $space-12; }
  }

  &__label {
    @include label-caps;
    font-size: 10px;
    color: var(--color-text-muted);
    white-space: nowrap;
    flex-shrink: 0;
  }

  &__track {
    display: flex;
    gap: $space-8;
    overflow-x: auto;
    scrollbar-width: none;
    &::-webkit-scrollbar { display: none; }
  }

  &__brand {
    @include label-caps;
    font-size: $text-xs;
    color: var(--color-text-muted);
    text-decoration: none;
    white-space: nowrap;
    @include transition(color);

    &:hover { color: var(--color-text-primary); }
  }
}
</style>
```

---

## `src/components/feature/account/AccountMenu.vue`

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useUserStore } from '@/stores/user'
import Popover from 'primevue/popover'

const user   = useUserStore()
const popover = ref()

function toggle(e: Event) {
  popover.value.toggle(e)
}
</script>

<template>
  <div class="account-menu">
    <button class="account-menu__trigger" aria-label="Account menu" @click="toggle">
      <div class="account-menu__avatar">
        {{ user.user?.firstName?.charAt(0) }}
      </div>
    </button>

    <Popover ref="popover" class="account-menu__popover">
      <div class="account-menu__dropdown">
        <div class="account-menu__user-info">
          <p class="account-menu__user-name">{{ user.fullName }}</p>
          <p class="account-menu__user-email">{{ user.user?.email }}</p>
        </div>

        <nav class="account-menu__nav">
          <RouterLink to="/account"         class="account-menu__link">My Account</RouterLink>
          <RouterLink to="/account/orders"  class="account-menu__link">Orders</RouterLink>
          <RouterLink to="/wishlist"         class="account-menu__link">Wishlist</RouterLink>
          <RouterLink to="/account/returns" class="account-menu__link">Returns</RouterLink>
        </nav>

        <button class="account-menu__signout" @click="user.logout()">
          <i class="pi pi-sign-out" /> Sign Out
        </button>
      </div>
    </Popover>
  </div>
</template>

<style lang="scss" scoped>
.account-menu {
  &__trigger {
    background: none;
    border: none;
    cursor: pointer;
    padding: 0;
  }

  &__avatar {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    background: var(--color-accent);
    color: white;
    font-size: $text-xs;
    font-weight: $weight-semibold;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  &__dropdown {
    width: 220px;
    padding: $space-2 0;
  }

  &__user-info {
    padding: $space-4;
    border-bottom: 1px solid var(--color-border);
    margin-bottom: $space-2;
  }

  &__user-name {
    font-size: $text-sm;
    font-weight: $weight-medium;
    color: var(--color-text-primary);
    margin-bottom: 2px;
  }

  &__user-email {
    font-size: $text-xs;
    color: var(--color-text-muted);
    @include truncate(1);
  }

  &__nav {
    display: flex;
    flex-direction: column;
    padding: 0 $space-2;
  }

  &__link {
    display: block;
    padding: $space-2 $space-3;
    font-size: $text-sm;
    color: var(--color-text-secondary);
    text-decoration: none;
    @include transition(background color);
    border-radius: 2px;

    &:hover {
      background: var(--color-bg-elevated);
      color: var(--color-text-primary);
    }
  }

  &__signout {
    display: flex;
    align-items: center;
    gap: $space-2;
    width: calc(100% - #{$space-4});
    margin: $space-2 $space-2 $space-1;
    padding: $space-2 $space-3;
    background: none;
    border: none;
    cursor: pointer;
    font-size: $text-sm;
    color: $color-error;
    @include transition(background);
    border-radius: 2px;

    &:hover { background: rgb(192 57 43 / 0.06); }
    .pi { font-size: 14px; }
  }
}
</style>
```

---

## `src/styles/main.scss`

```scss
// ─── Base ─────────────────────────────────────────────────────────────────────
@use 'base/theme';
@use 'base/reset';
@use 'base/typography';
@use 'base/fonts';

// ─── Components (global only) ─────────────────────────────────────────────────
@use 'components/primevue-overrides';

// ─── Utilities ────────────────────────────────────────────────────────────────
@use 'utilities/aspect-ratios';
```

## `src/styles/base/_reset.scss`

```scss
*,
*::before,
*::after {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

html {
  scroll-behavior: smooth;
  text-size-adjust: 100%;
  -webkit-text-size-adjust: 100%;
}

body {
  font-family: $font-body;
  font-size: $text-base;
  line-height: $leading-normal;
  color: var(--color-text-primary);
  background: var(--color-bg-canvas);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

img,
video {
  max-width: 100%;
  height: auto;
  display: block;
}

button {
  font-family: inherit;
}

a {
  color: inherit;
}

ul,
ol {
  list-style: none;
}

input,
textarea,
select {
  font-family: inherit;
  font-size: inherit;
}
```

## `src/styles/components/_primevue-overrides.scss`

```scss
// ─── Global PrimeVue overrides to match brand ─────────────────────────────────

// Accordion
.p-accordionheader {
  font-family: $font-body !important;
  @include label-caps;
  font-size: 11px !important;
  padding: $space-4 0 !important;
  background: none !important;
  border: none !important;
  border-bottom: 1px solid var(--color-border) !important;
  border-radius: 0 !important;
  color: var(--color-text-primary) !important;
  letter-spacing: $tracking-widest !important;

  &:hover { background: none !important; }
}

.p-accordionpanel {
  border: none !important;
}

.p-accordioncontent-content {
  padding: $space-5 0 !important;
  border: none !important;
}

// Slider
.p-slider {
  background: var(--color-border) !important;
  border-radius: 0 !important;
  height: 3px !important;

  .p-slider-range {
    background: var(--color-text-primary) !important;
    border-radius: 0 !important;
  }

  .p-slider-handle {
    width: 14px !important;
    height: 14px !important;
    border: 2px solid var(--color-text-primary) !important;
    background: var(--color-bg-canvas) !important;
    border-radius: 50% !important;
    margin-top: -5px !important;

    &:focus { box-shadow: 0 0 0 3px var(--color-accent-subtle) !important; }
  }
}

// Drawer
.p-drawer {
  border-radius: 0 !important;

  .p-drawer-header {
    padding: $space-5 $space-6 !important;
    border-bottom: 1px solid var(--color-border) !important;
  }

  .p-drawer-content {
    padding: $space-5 $space-6 !important;
  }
}

// Dialog
.p-dialog {
  border-radius: 0 !important;
  box-shadow: $shadow-xl !important;

  .p-dialog-header {
    padding: $space-5 $space-6 !important;
    border-bottom: 1px solid var(--color-border) !important;
    font-family: $font-display !important;
    font-size: $text-lg !important;
    font-weight: $weight-light !important;
  }
}

// Breadcrumb
.p-breadcrumb {
  background: none !important;
  border: none !important;
  padding: 0 !important;

  .p-breadcrumb-list {
    display: flex;
    align-items: center;
    gap: $space-2;
    flex-wrap: wrap;
  }

  .p-menuitem-link {
    font-size: $text-xs;
    color: var(--color-text-muted) !important;
    text-decoration: none;
    @include label-caps;
    font-size: 10px !important;

    &:hover { color: var(--color-text-primary) !important; }
  }

  .p-menuitem-separator {
    color: var(--color-border-strong) !important;
  }
}

// Rating
.p-rating {
  .p-rating-icon {
    color: var(--color-border-strong) !important;
    font-size: 14px !important;

    &.p-rating-icon-active { color: #f59e0b !important; }
  }
}

// Toast
.p-toast {
  .p-toast-message {
    border-radius: 0 !important;
    border-left: 3px solid;

    &.p-toast-message-success { border-color: $color-success !important; }
    &.p-toast-message-error   { border-color: $color-error !important; }
    &.p-toast-message-warn    { border-color: $color-warning !important; }
    &.p-toast-message-info    { border-color: $color-info !important; }
  }
}

// InputText
.p-inputtext {
  border-radius: 0 !important;
  border-color: var(--color-border) !important;
  font-size: $text-sm !important;
  padding: $space-3 $space-4 !important;

  &:focus {
    border-color: var(--color-text-primary) !important;
    box-shadow: none !important;
  }
}

// Select
.p-select {
  border-radius: 0 !important;
  border-color: var(--color-border) !important;

  &:not(.p-disabled):hover { border-color: var(--color-text-primary) !important; }
  &:not(.p-disabled).p-focus { box-shadow: none !important; border-color: var(--color-text-primary) !important; }
}

// DataTable
.p-datatable {
  .p-datatable-thead > tr > th {
    @include label-caps;
    font-size: 11px !important;
    background: var(--color-bg-elevated) !important;
    border-color: var(--color-border) !important;
    color: var(--color-text-primary) !important;
  }

  .p-datatable-tbody > tr > td {
    font-size: $text-sm !important;
    border-color: var(--color-border) !important;
    color: var(--color-text-secondary) !important;
  }

  .p-datatable-tbody > tr:hover > td {
    background: var(--color-bg-elevated) !important;
  }
}
```

---

# Feature Module Architecture

---

## Vertical-Slice Pattern Overview

The ReSys.Shop shop frontend uses **Vertical-Slice Architecture** where each feature module is self-contained and owns all layers needed for independent functionality. Features are organized by business capability rather than technical layer.

### Core Principles

1. **Independence**: Each feature can be developed, tested, and deployed independently
2. **Encapsulation**: Feature state, services, and components are colocated and scoped
3. **Scalability**: New features added without modifying existing feature code
4. **Lazy Loading**: Route-based code splitting enables per-feature loading
5. **Testing**: Services and stores easier to unit test in isolation
6. **Organization**: Developers work within one feature directory

### The 10 Feature Modules

```
src/features/
├── catalog/           # Product browsing, search, filtering, wishlist
├── identity/          # User authentication, profiles, accounts
├── inventory/         # Stock levels, availability tracking
├── locations/         # Shipping addresses, locations, store finder
├── ordering/          # Cart management, order processing
├── payment/           # Payment methods, checkout
├── promotions/        # Discount codes, promotional offers
├── returns/           # Return management, RMA workflow
├── settings/          # User preferences, notifications
└── shipping/          # Shipping methods, tracking, logistics
```

---

## Generic Feature Structure

Every feature follows this standardized structure:

```
src/features/[feature-name]/
├── components/              # Feature-specific Vue components
│   ├── ProductCard.vue
│   ├── ProductFilter.vue
│   └── ProductDetail.vue
├── composables/             # Feature composables (hooks)
│   ├── useCatalog.ts        # Main feature hook
│   ├── useWishlist.ts
│   └── useFilters.ts
├── services/                # Business logic, API calls
│   ├── product/
│   │   ├── getProductList.ts
│   │   ├── getProductDetail.ts
│   │   └── searchProducts.ts
│   └── category/
│       └── getCategoryList.ts
├── repositories/            # Data access abstractions
│   ├── productRepository.ts
│   └── categoryRepository.ts
├── store/                   # Pinia state stores
│   ├── catalog.ts           # Main feature store
│   ├── filters.ts           # Secondary store (filters state)
│   └── wishlist.ts          # Secondary store (wishlist state)
├── types/                   # TypeScript interfaces & types
│   ├── product.ts
│   ├── category.ts
│   └── filter.ts
└── views/                   # Route-level pages
    ├── ProductListView.vue
    ├── ProductDetailView.vue
    └── WishlistView.vue
```

---

## Example 1: Catalog Feature

### Purpose
Manage product browsing, searching, filtering, and wishlist functionality.

### Key Files

#### **Types** (`src/features/catalog/types/product.ts`)

```ts
export interface Product {
  id: string
  name: string
  brand: string
  price: number
  originalPrice?: number
  images: { url: string; alt: string }[]
  variants: ProductVariant[]
  rating: number
  description: string
  inStock: boolean
}

export interface ProductVariant {
  id: string
  color: string
  size: string
  stock: number
  code: string
}

export interface ProductFilter {
  categories: string[]
  brands: string[]
  priceRange: [number, number]
  sizes: string[]
  colors: string[]
  onSale: boolean
  inStock: boolean
}
```

#### **Store** (`src/features/catalog/store/catalog.ts`)

```ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { getProductList } from '../services/product/getProductList'

export const useCatalogStore = defineStore('catalog', () => {
  const products = ref<Product[]>([])
  const selectedProduct = ref<Product | null>(null)
  const isLoading = ref(false)

  const productCount = computed(() => products.value.length)
  const hasProducts = computed(() => productCount.value > 0)

  async function fetchProductList(categoryId: string) {
    isLoading.value = true
    try {
      const result = await getProductList({ categoryId })
      if (result.isSuccess) {
        products.value = result.data.items
      }
    } finally {
      isLoading.value = false
    }
  }

  function selectProduct(product: Product) {
    selectedProduct.value = product
  }

  return { products, selectedProduct, isLoading, productCount, hasProducts, fetchProductList, selectProduct }
})
```

#### **Service** (`src/features/catalog/services/product/getProductList.ts`)

```ts
import { apiClient } from '@/core/services/api'
import type { Result } from '@/core/models/result'

interface GetProductListRequest {
  categoryId: string
  page?: number
  limit?: number
  filters?: ProductFilter
}

interface GetProductListResponse {
  items: Product[]
  total: number
  page: number
  limit: number
}

export async function getProductList(req: GetProductListRequest): Promise<Result<GetProductListResponse>> {
  try {
    const { data } = await apiClient.get('/products', { params: req })
    return { isSuccess: true, data }
  } catch (error) {
    return { isSuccess: false, errors: [{ code: 'FETCH_FAILED', message: String(error) }] }
  }
}
```

#### **Composable** (`src/features/catalog/composables/useCatalog.ts`)

```ts
import { ref, computed } from 'vue'
import { useCatalogStore } from '../store/catalog'
import { useFiltersStore } from '../store/filters'
import { getProductList } from '../services/product/getProductList'

export function useCatalog() {
  const catalogStore = useCatalogStore()
  const filtersStore = useFiltersStore()

  const isLoading = computed(() => catalogStore.isLoading)
  const products = computed(() => catalogStore.products)

  async function loadProducts(categoryId: string) {
    const filters = filtersStore.activeFilters
    await catalogStore.fetchProductList(categoryId)
  }

  function selectProduct(product: Product) {
    catalogStore.selectProduct(product)
  }

  return { isLoading, products, loadProducts, selectProduct }
}
```

---

## Example 2: Ordering Feature

### Purpose
Manage shopping cart, checkout flow, and order processing.

### Key Files

#### **Types** (`src/features/ordering/types/order.ts`)

```ts
export interface CartItem {
  id: string
  productId: string
  product: Product
  variant: ProductVariant
  color: ColorOption
  quantity: number
  price: number
}

export interface CartSummary {
  itemCount: number
  subtotal: number
  tax: number
  shipping: number
  discount: number
  total: number
}

export interface Order {
  id: string
  items: CartItem[]
  summary: CartSummary
  status: 'pending' | 'confirmed' | 'shipped' | 'delivered'
  shippingAddress: Address
  paymentMethod: PaymentMethod
  createdAt: Date
}

export interface Address {
  id: string
  name: string
  street: string
  city: string
  state: string
  postalCode: string
  country: string
  isDefault: boolean
}
```

#### **Store** (`src/features/ordering/store/ordering.ts`)

```ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { cartService } from '../services/cart/cartService'

export const useOrderingStore = defineStore('ordering', () => {
  const items = ref<CartItem[]>([])
  const isOpen = ref(false)

  const itemCount = computed(() => items.value.reduce((sum, item) => sum + item.quantity, 0))
  const isEmpty = computed(() => items.value.length === 0)
  const subtotal = computed(() => items.value.reduce((sum, item) => sum + item.price * item.quantity, 0))
  const summary = computed<CartSummary>(() => ({
    itemCount: itemCount.value,
    subtotal: subtotal.value,
    tax: subtotal.value * 0.07,
    shipping: subtotal.value > 500 ? 0 : 15,
    discount: 0,
    total: subtotal.value + (subtotal.value * 0.07) + (subtotal.value > 500 ? 0 : 15),
  }))

  async function addItem(product: Product, variant: ProductVariant, color: ColorOption, qty = 1) {
    const result = await cartService.addToCart(product, variant, color, qty)
    if (result.isSuccess) {
      items.value.push(result.data)
    }
    return result
  }

  function removeItem(itemId: string) {
    items.value = items.value.filter(item => item.id !== itemId)
  }

  function updateQuantity(itemId: string, qty: number) {
    const item = items.value.find(item => item.id === itemId)
    if (item) item.quantity = qty
  }

  return { items, isOpen, itemCount, isEmpty, summary, addItem, removeItem, updateQuantity }
})
```

#### **Service** (`src/features/ordering/services/cart/cartService.ts`)

```ts
import { apiClient } from '@/core/services/api'
import type { Result } from '@/core/models/result'

export const cartService = {
  async addToCart(product: Product, variant: ProductVariant, color: ColorOption, qty: number): Promise<Result<CartItem>> {
    try {
      const { data } = await apiClient.post('/cart/items', {
        productId: product.id,
        variantId: variant.id,
        color: color.name,
        quantity: qty,
      })
      return { isSuccess: true, data }
    } catch (error) {
      return { isSuccess: false, errors: [{ code: 'ADD_FAILED', message: String(error) }] }
    }
  },

  async removeFromCart(itemId: string): Promise<Result<void>> {
    try {
      await apiClient.delete(`/cart/items/${itemId}`)
      return { isSuccess: true, data: undefined }
    } catch (error) {
      return { isSuccess: false, errors: [{ code: 'REMOVE_FAILED', message: String(error) }] }
    }
  },
}
```

#### **Composable** (`src/features/ordering/composables/useCart.ts`)

```ts
import { computed } from 'vue'
import { useOrderingStore } from '../store/ordering'
import { useToast } from 'primevue/usetoast'

export function useCart() {
  const orderingStore = useOrderingStore()
  const toast = useToast()

  async function addToCart(product: Product, variant: ProductVariant, color: ColorOption, qty = 1) {
    if (variant.stock < qty) {
      toast.add({ severity: 'warn', summary: 'Low Stock', detail: 'Not enough stock', life: 3000 })
      return
    }

    const result = await orderingStore.addItem(product, variant, color, qty)
    if (result.isSuccess) {
      toast.add({ severity: 'success', summary: 'Added to Bag', life: 3000 })
    } else {
      toast.add({ severity: 'error', summary: 'Failed to Add', detail: result.errors?.[0].message, life: 3000 })
    }
  }

  return {
    items: computed(() => orderingStore.items),
    cartSummary: computed(() => orderingStore.summary),
    isEmpty: computed(() => orderingStore.isEmpty),
    isOpen: computed(() => orderingStore.isOpen),
    addToCart,
  }
}
```

---

# Data Flow & Architectural Patterns

---

## Result<T> Pattern

All service layer operations return a `Result<T>` envelope to standardize success/failure handling. This avoids throwing business exceptions across layer boundaries.

### Result Type Definition

```ts
// src/core/models/result.ts
export interface ResultError {
  code: string
  message: string
  details?: Record<string, unknown>
}

export interface Result<T> {
  isSuccess: boolean
  data?: T
  errors?: ResultError[]
}
```

### Pattern Usage in Services

```ts
// ✅ GOOD: Service returns Result<T>
export async function getProductList(): Promise<Result<Product[]>> {
  try {
    const { data } = await apiClient.get('/products')
    return { isSuccess: true, data }
  } catch (error) {
    return {
      isSuccess: false,
      errors: [{ code: 'FETCH_FAILED', message: String(error) }],
    }
  }
}

// ✅ Service caller checks Result
export function useCatalog() {
  async function loadProducts() {
    const result = await getProductList()
    if (result.isSuccess) {
      store.products = result.data
    } else {
      toast.add({ severity: 'error', summary: result.errors?.[0].message })
    }
  }
}
```

---

## Data Flow: Views → Stores → Services → API

```
Vue Component (View)
    ↓ imports composable
Composable Hook (useCart, useCatalog)
    ↓ accesses store
Pinia Store (setup-store)
    ↓ calls service
Service Layer (async functions)
    ↓ executes API call
API Client (Axios + interceptors)
    ↓ HTTP request
Backend API
```

### Example: Adding Product to Cart

```ts
// 1. Component calls composable hook
<script setup>
const { addToCart } = useCart()
// ↓
// 2. Composable wraps store action + adds toast
export function useCart() {
  const orderingStore = useOrderingStore()
  const toast = useToast()

  async function addToCart(product, variant, color, qty) {
    const result = await orderingStore.addItem(product, variant, color, qty)
    // ↓
    // 3. Store calls service
    if (result.isSuccess) {
      toast.add({ ... })
    }
  }
}

export const useOrderingStore = defineStore('ordering', () => {
  async function addItem(product, variant, color, qty) {
    const result = await cartService.addToCart(...)
    // ↓
    // 4. Service makes API call
    if (result.isSuccess) {
      items.value.push(result.data)
    }
    return result
  }
})

export const cartService = {
  async addToCart(product, variant, color, qty) {
    const { data } = await apiClient.post('/cart/items', {...})
    // ↓
    // 5. API Client sends HTTP request
    return { isSuccess: true, data }
  }
}
```

---

## Composition API Conventions

**MANDATORY**: All Vue components use `<script setup lang="ts">` with Composition API. Options API is **prohibited**.

### ✅ Correct Pattern

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useCatalog } from '@/features/catalog/composables/useCatalog'

const { products, loadProducts } = useCatalog()
const isLoading = ref(false)

const productCount = computed(() => products.value.length)

onMounted(() => {
  loadProducts()
})
</script>

<template>
  <div class="product-list">
    <div v-if="isLoading">Loading...</div>
    <div v-else class="product-list__items">
      <div v-for="product in products" :key="product.id">
        {{ product.name }}
      </div>
    </div>
  </div>
</template>
```

### ❌ Incorrect Pattern (DO NOT USE)

```vue
<!-- ❌ Options API NOT allowed -->
<script>
export default {
  data() { return { products: [] } },
  methods: { loadProducts() { ... } },
}
</script>
```

---

## Pinia Setup-Store Pattern

Pinia stores use the composable/setup pattern (functions returning state/actions), not the options-store pattern.

### Store Structure

```ts
export const useOrderingStore = defineStore('ordering', () => {
  // State (refs)
  const items = ref<CartItem[]>([])
  
  // Computed state
  const itemCount = computed(() => items.value.length)
  
  // Actions (functions)
  async function addItem(product: Product) {
    items.value.push(...)
  }
  
  // Return public API
  return { items, itemCount, addItem }
})
```

### Store Usage in Components

```ts
const orderingStore = useOrderingStore()

// Access state
orderingStore.items        // CartItem[]
orderingStore.itemCount    // number

// Call actions
await orderingStore.addItem(product)
```

---

## Repository & Service Layer Pattern

Services typically compose repositories for data access, keeping API logic separate from business logic.

### Repository (Data Access)

```ts
// src/features/catalog/repositories/productRepository.ts
export const productRepository = {
  async list(filter: ProductFilter): Promise<Product[]> {
    const { data } = await apiClient.get('/products', { params: filter })
    return data.items
  },

  async detail(productId: string): Promise<Product> {
    const { data } = await apiClient.get(`/products/${productId}`)
    return data
  },
}
```

### Service (Business Logic)

```ts
// src/features/catalog/services/product/getProductList.ts
import { productRepository } from '../../repositories/productRepository'

export async function getProductList(filter: ProductFilter): Promise<Result<Product[]>> {
  try {
    const products = await productRepository.list(filter)
    return { isSuccess: true, data: products }
  } catch (error) {
    return { isSuccess: false, errors: [{ code: 'FETCH_FAILED', message: String(error) }] }
  }
}
```

---

**End of Reference Guide** — This reference documents the current Vertical-Slice Feature Architecture, Result<T> pattern, and recommended dev patterns. Refer to feature folders for implementation details.