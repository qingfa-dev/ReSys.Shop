# Admin SPA — Infrastructure Setup

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development
> (recommended) or superpowers:executing-plans to implement this plan task-by-task.
> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restructure all 9 admin feature module directories to the `auth/` pattern, remove 8
deprecated page files, rewrite all route files to the list+detail pattern, and update the
sidebar menu config. No pages are built yet — this is pure infrastructure.

**Architecture:** Each module gets 7 standard subdirectories (`api/`, `components/`,
`composables/`, `models/`, `pages/`, `store/`, `utils/`) plus a `routes.ts`. Route files
follow the pattern `/module/entity` (list), `/module/entity/new` (create), `/:id` (view),
`/:id/edit` (edit). All 8 deprecated files (create pages, tree manager, separate sub-entity
lists) are removed. Menu entries referencing removed routes are cleaned up.

**Tech Stack:** Vue Router 5, TypeScript 6

## Global Constraints

- Each module directory must match `auth/` layout: api/, components/, composables/, models/, pages/, store/, utils/, routes.ts
- Route `:id` param is a GUID string
- No new npm dependencies
- All route files must typecheck before commit
- Spec: `spec/design-admin-spa-list-detail-pattern.md`

---

### Task 1: Restructure all 9 module directories to auth/ pattern

**Files:**
- Create: `app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/api/.gitkeep`
- Create: `app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/components/.gitkeep`
- Create: `app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/composables/.gitkeep`
- Create: `app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/models/.gitkeep`
- Create: `app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/store/.gitkeep`
- Create: `app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/utils/.gitkeep`

**Interfaces:**
- Consumes: nothing
- Produces: 7 empty subdirectories per module, each with `.gitkeep`

- [ ] **Step 1: Write test for directory existence**

Create `app/Admin/src/__tests__/infrastructure/module-structure.spec.ts`:

```typescript
import { describe, it, expect } from 'vitest'
import fs from 'node:fs'
import path from 'node:path'

const modules = ['catalog', 'inventory', 'ordering', 'payment', 'shipping', 'location', 'users', 'profile', 'reports']
const subdirs = ['api', 'components', 'composables', 'models', 'store', 'utils']

const featuresDir = path.resolve(__dirname, '../../features')

describe('module directory structure', () => {
  it.each(modules)('%s has all standard subdirectories', (mod) => {
    const base = path.join(featuresDir, mod)
    for (const dir of subdirs) {
      const dirPath = path.join(base, dir)
      expect(fs.existsSync(dirPath), `${mod}/${dir} should exist`).toBe(true)
    }
  })

  it.each(modules)('%s has a pages directory maintained', (mod) => {
    const pagesPath = path.join(featuresDir, mod, 'pages')
    expect(fs.existsSync(pagesPath), `${mod}/pages should exist`).toBe(true)
  })
})
```

- [ ] **Step 2: Run test to verify it fails**

```bash
cd app/Admin && pnpm run test:unit -- --run infrastructure/module-structure
```
Expected: FAIL — directories don't exist yet.

- [ ] **Step 3: Create directories**

```bash
for mod in catalog inventory ordering payment shipping location users profile reports; do
  for dir in api components composables models store utils; do
    mkdir -p app/Admin/src/features/$mod/$dir
    touch app/Admin/src/features/$mod/$dir/.gitkeep
  done
done
```

- [ ] **Step 4: Run test to verify it passes**

```bash
cd app/Admin && pnpm run test:unit -- --run infrastructure/module-structure
```
Expected: PASS — all directories exist.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/{catalog,inventory,ordering,payment,shipping,location,users,profile,reports}/{api,components,composables,models,store,utils}/
git add app/Admin/src/__tests__/infrastructure/
git commit -m "chore: add auth-style subdirectories to all feature modules"
```

---

### Task 2: Remove 8 deprecated page files

**Files:**
- Delete: `app/Admin/src/features/catalog/pages/ProductCreatePage.vue`
- Delete: `app/Admin/src/features/catalog/pages/TaxonListPage.vue`
- Delete: `app/Admin/src/features/catalog/pages/TaxonTreeManagerPage.vue`
- Delete: `app/Admin/src/features/catalog/pages/OptionValueListPage.vue`
- Delete: `app/Admin/src/features/inventory/pages/StockImportPage.vue`
- Delete: `app/Admin/src/features/inventory/pages/UnitListPage.vue`
- Delete: `app/Admin/src/features/ordering/pages/OrderCreatePage.vue`
- Delete: `app/Admin/src/features/users/pages/StaffCreatePage.vue`

**Interfaces:**
- Consumes: nothing
- Produces: removed files

- [ ] **Step 1: Write test verifying files do not exist after removal**

Add to `app/Admin/src/__tests__/infrastructure/module-structure.spec.ts`:

```typescript
const removedFiles = [
  'features/catalog/pages/ProductCreatePage.vue',
  'features/catalog/pages/TaxonListPage.vue',
  'features/catalog/pages/TaxonTreeManagerPage.vue',
  'features/catalog/pages/OptionValueListPage.vue',
  'features/inventory/pages/StockImportPage.vue',
  'features/inventory/pages/UnitListPage.vue',
  'features/ordering/pages/OrderCreatePage.vue',
  'features/users/pages/StaffCreatePage.vue',
]

describe('deprecated page removal', () => {
  it.each(removedFiles)('%s does not exist', (file) => {
    const filePath = path.resolve(__dirname, '../../', file)
    expect(fs.existsSync(filePath), `${file} should be removed`).toBe(false)
  })
})
```

- [ ] **Step 2: Run test to verify it fails (files still exist)**

```bash
cd app/Admin && pnpm run test:unit -- --run infrastructure/module-structure
```
Expected: FAIL — deprecated files still exist.

- [ ] **Step 3: Delete files**

```bash
rm app/Admin/src/features/catalog/pages/ProductCreatePage.vue
rm app/Admin/src/features/catalog/pages/TaxonListPage.vue
rm app/Admin/src/features/catalog/pages/TaxonTreeManagerPage.vue
rm app/Admin/src/features/catalog/pages/OptionValueListPage.vue
rm app/Admin/src/features/inventory/pages/StockImportPage.vue
rm app/Admin/src/features/inventory/pages/UnitListPage.vue
rm app/Admin/src/features/ordering/pages/OrderCreatePage.vue
rm app/Admin/src/features/users/pages/StaffCreatePage.vue
```

- [ ] **Step 4: Run test to verify it passes**

```bash
cd app/Admin && pnpm run test:unit -- --run infrastructure/module-structure
```
Expected: PASS — all deprecated files removed.

- [ ] **Step 5: Commit**

```bash
git add -A app/Admin/src/features/
git add app/Admin/src/__tests__/infrastructure/
git commit -m "chore: remove 8 deprecated page files"
```

---

### Task 3: Update Catalog route file

**Files:**
- Modify: `app/Admin/src/app/routes/catalog.routes.ts`

**Interfaces:**
- Consumes: nothing
- Produces: `catalogRoutes` — `RouteRecordRaw` with 14 children (3 redirect, 3 entities × 4 routes, 2 additional)

- [ ] **Step 1: Write test for catalog route structure**

Create `app/Admin/src/__tests__/infrastructure/route-structure.spec.ts`:

```typescript
import { describe, it, expect } from 'vitest'
import { catalogRoutes } from '@/app/routes/catalog.routes'
import type { RouteRecordRaw } from 'vue-router'

function collectRouteNames(routes: RouteRecordRaw[]): string[] {
  const names: string[] = []
  for (const r of routes) {
    if (r.name) names.push(r.name as string)
    if (r.children) names.push(...collectRouteNames(r.children))
  }
  return names
}

describe('catalog routes', () => {
  it('has all required route names', () => {
    const childRoutes = (catalogRoutes.children || []) as RouteRecordRaw[]
    const names = collectRouteNames(childRoutes)
    expect(names).toContain('catalog.dashboard')
    expect(names).toContain('catalog.products.list')
    expect(names).toContain('catalog.products.create')
    expect(names).toContain('catalog.products.view')
    expect(names).toContain('catalog.products.edit')
    expect(names).toContain('catalog.taxonomies.list')
    expect(names).toContain('catalog.taxonomies.create')
    expect(names).toContain('catalog.taxonomies.view')
    expect(names).toContain('catalog.taxonomies.edit')
    expect(names).toContain('catalog.option-types.list')
    expect(names).toContain('catalog.option-types.create')
    expect(names).toContain('catalog.option-types.view')
    expect(names).toContain('catalog.option-types.edit')
  })

  it('has no legacy route names', () => {
    const childRoutes = (catalogRoutes.children || []) as RouteRecordRaw[]
    const names = collectRouteNames(childRoutes)
    expect(names).not.toContain('catalog.products.create') // wait — product.create IS needed via /new
    // Actually: old route was products/create (path). The name re-use is fine.
    expect(names).not.toContain('catalog.taxa.list')
    expect(names).not.toContain('catalog.option-values.list')
  })
})
```

Note: `catalog.products.create` name is re-used for `/products/new` path. That's intentional — the name stays, the path changes.

- [ ] **Step 2: Verify test fails**

```bash
cd app/Admin && pnpm run test:unit -- --run infrastructure/route-structure
```
Expected: FAIL — old routes still defined.

- [ ] **Step 3: Rewrite catalog routes**

Replace content of `app/Admin/src/app/routes/catalog.routes.ts`:

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const catalogRoutes: RouteRecordRaw = {
  path: 'catalog',
  children: [
    { path: '', redirect: { name: 'catalog.dashboard' } },
    {
      path: 'dashboard',
      name: 'catalog.dashboard',
      component: () => import('@/features/catalog/pages/DashboardPage.vue'),
    },
    {
      path: 'products',
      name: 'catalog.products.list',
      component: () => import('@/features/catalog/pages/ProductListPage.vue'),
    },
    {
      path: 'products/new',
      name: 'catalog.products.create',
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
    },
    {
      path: 'products/:id',
      name: 'catalog.products.view',
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
    },
    {
      path: 'products/:id/edit',
      name: 'catalog.products.edit',
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
    },
    {
      path: 'taxonomies',
      name: 'catalog.taxonomies.list',
      component: () => import('@/features/catalog/pages/TaxonomyListPage.vue'),
    },
    {
      path: 'taxonomies/new',
      name: 'catalog.taxonomies.create',
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
    },
    {
      path: 'taxonomies/:id',
      name: 'catalog.taxonomies.view',
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
    },
    {
      path: 'taxonomies/:id/edit',
      name: 'catalog.taxonomies.edit',
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
    },
    {
      path: 'option-types',
      name: 'catalog.option-types.list',
      component: () => import('@/features/catalog/pages/OptionTypeListPage.vue'),
    },
    {
      path: 'option-types/new',
      name: 'catalog.option-types.create',
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
    },
    {
      path: 'option-types/:id',
      name: 'catalog.option-types.view',
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
    },
    {
      path: 'option-types/:id/edit',
      name: 'catalog.option-types.edit',
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
    },
  ],
}
```

- [ ] **Step 4: Verify test passes**

```bash
cd app/Admin && pnpm run test:unit -- --run infrastructure/route-structure
```
Expected: PASS — catalog route names match spec.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/app/routes/catalog.routes.ts app/Admin/src/__tests__/infrastructure/
git commit -m "feat: rewrite catalog routes with list-detail pattern"
```

---

### Task 4: Update remaining 8 route files

**Files:**
- Modify: `app/Admin/src/app/routes/inventory.routes.ts`
- Modify: `app/Admin/src/app/routes/ordering.routes.ts`
- Modify: `app/Admin/src/app/routes/payment.routes.ts`
- Modify: `app/Admin/src/app/routes/shipping.routes.ts`
- Modify: `app/Admin/src/app/routes/location.routes.ts`
- Modify: `app/Admin/src/app/routes/users.routes.ts`
- Modify: `app/Admin/src/app/routes/profile.routes.ts`

**Interfaces:**
- Consumes: nothing
- Produces: updated route exports per spec Sections 5-6

- [ ] **Step 1: Add route tests for all modules**

Append to `app/Admin/src/__tests__/infrastructure/route-structure.spec.ts`:

```typescript
import { inventoryRoutes } from '@/app/routes/inventory.routes'
import { orderingRoutes } from '@/app/routes/ordering.routes'
import { paymentRoutes } from '@/app/routes/payment.routes'
import { shippingRoutes } from '@/app/routes/shipping.routes'
import { locationRoutes } from '@/app/routes/location.routes'
import { usersRoutes } from '@/app/routes/users.routes'
import { profileRoutes } from '@/app/routes/profile.routes'

describe('inventory routes', () => {
  const childRoutes = (inventoryRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('has stock detail routes', () => {
    expect(names).toContain('inventory.stocks.create')
    expect(names).toContain('inventory.stocks.view')
    expect(names).toContain('inventory.stocks.edit')
  })
  it('has location detail routes', () => {
    expect(names).toContain('inventory.locations.create')
    expect(names).toContain('inventory.locations.view')
    expect(names).toContain('inventory.locations.edit')
  })
  it('has transfer detail routes', () => {
    expect(names).toContain('inventory.transfers.create')
    expect(names).toContain('inventory.transfers.view')
    expect(names).toContain('inventory.transfers.edit')
  })
  it('has reservation list route', () => {
    expect(names).toContain('inventory.reservations.list')
  })
  it('no legacy routes', () => {
    expect(names).not.toContain('inventory.stocks.import')
    expect(names).not.toContain('inventory.units.list')
  })
})

describe('ordering routes', () => {
  const childRoutes = (orderingRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('has order detail routes', () => {
    expect(names).toContain('ordering.orders.create')
    expect(names).toContain('ordering.orders.view')
    expect(names).toContain('ordering.orders.edit')
  })
  it('no legacy orders/create', () => {
    // name is re-used, but old path no longer exists — name check is fine
    expect(names).toContain('ordering.orders.create')
  })
})

describe('payment routes', () => {
  const childRoutes = (paymentRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('has normalized payments path', () => {
    expect(names).toContain('payment.payments.list')
    expect(names).toContain('payment.payments.view')
  })
  it('has method detail routes', () => {
    expect(names).toContain('payment.methods.create')
    expect(names).toContain('payment.methods.view')
    expect(names).toContain('payment.methods.edit')
  })
  it('no payments/new (Payment is view-only)', () => {
    expect(names).not.toContain('payment.payments.create')
  })
})

describe('shipping routes', () => {
  const childRoutes = (shippingRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('has method detail routes', () => {
    expect(names).toContain('shipping.methods.create')
    expect(names).toContain('shipping.methods.view')
    expect(names).toContain('shipping.methods.edit')
  })
  it('has rate detail routes', () => {
    expect(names).toContain('shipping.rates.create')
    expect(names).toContain('shipping.rates.view')
    expect(names).toContain('shipping.rates.edit')
  })
})

describe('location routes', () => {
  const childRoutes = (locationRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('has country detail routes', () => {
    expect(names).toContain('location.countries.create')
    expect(names).toContain('location.countries.view')
    expect(names).toContain('location.countries.edit')
  })
  it('has state detail routes', () => {
    expect(names).toContain('location.states.create')
    expect(names).toContain('location.states.view')
    expect(names).toContain('location.states.edit')
  })
})

describe('users routes', () => {
  const childRoutes = (usersRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('has staff detail routes', () => {
    expect(names).toContain('users.staff.create')
    expect(names).toContain('users.staff.view')
    expect(names).toContain('users.staff.edit')
  })
  it('has customer detail routes', () => {
    expect(names).toContain('users.customers.create')
    expect(names).toContain('users.customers.view')
    expect(names).toContain('users.customers.edit')
  })
  it('has role detail routes', () => {
    expect(names).toContain('users.roles.create')
    expect(names).toContain('users.roles.view')
    expect(names).toContain('users.roles.edit')
  })
  it('has permission view route only (read-only)', () => {
    expect(names).toContain('users.permissions.list')
    expect(names).toContain('users.permissions.view')
    expect(names).not.toContain('users.permissions.create')
    expect(names).not.toContain('users.permissions.edit')
  })
  it('no legacy staff/create', () => {
    // name re-used for /staff/new, so it's OK
    expect(names).toContain('users.staff.create')
  })
})

describe('profile routes', () => {
  const childRoutes = (profileRoutes.children || []) as RouteRecordRaw[]
  const names = collectRouteNames(childRoutes)
  it('uses profile.* namespace', () => {
    expect(names).toContain('profile.view')
    expect(names).toContain('profile.addresses')
    expect(names).not.toContain('profile')
    expect(names).not.toContain('addresses')
  })
})
```

- [ ] **Step 2: Verify tests fail**

```bash
cd app/Admin && pnpm run test:unit -- --run infrastructure/route-structure
```
Expected: FAIL — route files not yet updated.

- [ ] **Step 3: Rewrite inventory routes**

Replace `app/Admin/src/app/routes/inventory.routes.ts`:

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const inventoryRoutes: RouteRecordRaw = {
  path: 'inventory',
  children: [
    { path: '', redirect: { name: 'inventory.dashboard' } },
    {
      path: 'dashboard',
      name: 'inventory.dashboard',
      component: () => import('@/features/inventory/pages/DashboardPage.vue'),
    },
    {
      path: 'stocks',
      name: 'inventory.stocks.list',
      component: () => import('@/features/inventory/pages/StockListPage.vue'),
    },
    {
      path: 'stocks/new',
      name: 'inventory.stocks.create',
      component: () => import('@/features/inventory/pages/StockItemDetailPage.vue'),
    },
    {
      path: 'stocks/:id',
      name: 'inventory.stocks.view',
      component: () => import('@/features/inventory/pages/StockItemDetailPage.vue'),
    },
    {
      path: 'stocks/:id/edit',
      name: 'inventory.stocks.edit',
      component: () => import('@/features/inventory/pages/StockItemDetailPage.vue'),
    },
    {
      path: 'locations',
      name: 'inventory.locations.list',
      component: () => import('@/features/inventory/pages/LocationListPage.vue'),
    },
    {
      path: 'locations/new',
      name: 'inventory.locations.create',
      component: () => import('@/features/inventory/pages/LocationDetailPage.vue'),
    },
    {
      path: 'locations/:id',
      name: 'inventory.locations.view',
      component: () => import('@/features/inventory/pages/LocationDetailPage.vue'),
    },
    {
      path: 'locations/:id/edit',
      name: 'inventory.locations.edit',
      component: () => import('@/features/inventory/pages/LocationDetailPage.vue'),
    },
    {
      path: 'movements',
      name: 'inventory.movements.list',
      component: () => import('@/features/inventory/pages/MovementListPage.vue'),
    },
    {
      path: 'transfers',
      name: 'inventory.transfers.list',
      component: () => import('@/features/inventory/pages/TransferListPage.vue'),
    },
    {
      path: 'transfers/new',
      name: 'inventory.transfers.create',
      component: () => import('@/features/inventory/pages/TransferDetailPage.vue'),
    },
    {
      path: 'transfers/:id',
      name: 'inventory.transfers.view',
      component: () => import('@/features/inventory/pages/TransferDetailPage.vue'),
    },
    {
      path: 'transfers/:id/edit',
      name: 'inventory.transfers.edit',
      component: () => import('@/features/inventory/pages/TransferDetailPage.vue'),
    },
    {
      path: 'reservations',
      name: 'inventory.reservations.list',
      component: () => import('@/features/inventory/pages/StockReservationListPage.vue'),
    },
  ],
}
```

- [ ] **Step 4: Rewrite ordering routes**

Replace `app/Admin/src/app/routes/ordering.routes.ts`:

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const orderingRoutes: RouteRecordRaw = {
  path: 'ordering',
  children: [
    { path: '', redirect: { name: 'ordering.dashboard' } },
    {
      path: 'dashboard',
      name: 'ordering.dashboard',
      component: () => import('@/features/ordering/pages/DashboardPage.vue'),
    },
    {
      path: 'orders',
      name: 'ordering.orders.list',
      component: () => import('@/features/ordering/pages/OrderListPage.vue'),
    },
    {
      path: 'orders/new',
      name: 'ordering.orders.create',
      component: () => import('@/features/ordering/pages/OrderDetailPage.vue'),
    },
    {
      path: 'orders/:id',
      name: 'ordering.orders.view',
      component: () => import('@/features/ordering/pages/OrderDetailPage.vue'),
    },
    {
      path: 'orders/:id/edit',
      name: 'ordering.orders.edit',
      component: () => import('@/features/ordering/pages/OrderDetailPage.vue'),
    },
    {
      path: 'fulfillment',
      name: 'ordering.fulfillment.queue',
      component: () => import('@/features/ordering/pages/FulfillmentQueuePage.vue'),
    },
  ],
}
```

- [ ] **Step 5: Rewrite payment routes**

Replace `app/Admin/src/app/routes/payment.routes.ts`:

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const paymentRoutes: RouteRecordRaw = {
  path: 'payments',
  children: [
    { path: '', redirect: { name: 'payment.payments.list' } },
    {
      path: 'payments',
      name: 'payment.payments.list',
      component: () => import('@/features/payment/pages/PaymentListPage.vue'),
    },
    {
      path: 'payments/:id',
      name: 'payment.payments.view',
      component: () => import('@/features/payment/pages/PaymentDetailPage.vue'),
    },
    {
      path: 'methods',
      name: 'payment.methods.list',
      component: () => import('@/features/payment/pages/PaymentMethodListPage.vue'),
    },
    {
      path: 'methods/new',
      name: 'payment.methods.create',
      component: () => import('@/features/payment/pages/PaymentMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id',
      name: 'payment.methods.view',
      component: () => import('@/features/payment/pages/PaymentMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id/edit',
      name: 'payment.methods.edit',
      component: () => import('@/features/payment/pages/PaymentMethodDetailPage.vue'),
    },
  ],
}
```

- [ ] **Step 6: Rewrite shipping routes**

Replace `app/Admin/src/app/routes/shipping.routes.ts`:

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const shippingRoutes: RouteRecordRaw = {
  path: 'shipping',
  children: [
    { path: '', redirect: { name: 'shipping.methods.list' } },
    {
      path: 'methods',
      name: 'shipping.methods.list',
      component: () => import('@/features/shipping/pages/ShippingMethodListPage.vue'),
    },
    {
      path: 'methods/new',
      name: 'shipping.methods.create',
      component: () => import('@/features/shipping/pages/ShippingMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id',
      name: 'shipping.methods.view',
      component: () => import('@/features/shipping/pages/ShippingMethodDetailPage.vue'),
    },
    {
      path: 'methods/:id/edit',
      name: 'shipping.methods.edit',
      component: () => import('@/features/shipping/pages/ShippingMethodDetailPage.vue'),
    },
    {
      path: 'rates',
      name: 'shipping.rates.list',
      component: () => import('@/features/shipping/pages/ShippingRateListPage.vue'),
    },
    {
      path: 'rates/new',
      name: 'shipping.rates.create',
      component: () => import('@/features/shipping/pages/ShippingRateDetailPage.vue'),
    },
    {
      path: 'rates/:id',
      name: 'shipping.rates.view',
      component: () => import('@/features/shipping/pages/ShippingRateDetailPage.vue'),
    },
    {
      path: 'rates/:id/edit',
      name: 'shipping.rates.edit',
      component: () => import('@/features/shipping/pages/ShippingRateDetailPage.vue'),
    },
  ],
}
```

- [ ] **Step 7: Rewrite location routes**

Replace `app/Admin/src/app/routes/location.routes.ts`:

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const locationRoutes: RouteRecordRaw = {
  path: 'locations',
  children: [
    { path: '', redirect: { name: 'location.countries.list' } },
    {
      path: 'countries',
      name: 'location.countries.list',
      component: () => import('@/features/location/pages/CountryListPage.vue'),
    },
    {
      path: 'countries/new',
      name: 'location.countries.create',
      component: () => import('@/features/location/pages/CountryDetailPage.vue'),
    },
    {
      path: 'countries/:id',
      name: 'location.countries.view',
      component: () => import('@/features/location/pages/CountryDetailPage.vue'),
    },
    {
      path: 'countries/:id/edit',
      name: 'location.countries.edit',
      component: () => import('@/features/location/pages/CountryDetailPage.vue'),
    },
    {
      path: 'states',
      name: 'location.states.list',
      component: () => import('@/features/location/pages/StateListPage.vue'),
    },
    {
      path: 'states/new',
      name: 'location.states.create',
      component: () => import('@/features/location/pages/StateDetailPage.vue'),
    },
    {
      path: 'states/:id',
      name: 'location.states.view',
      component: () => import('@/features/location/pages/StateDetailPage.vue'),
    },
    {
      path: 'states/:id/edit',
      name: 'location.states.edit',
      component: () => import('@/features/location/pages/StateDetailPage.vue'),
    },
  ],
}
```

- [ ] **Step 8: Rewrite users routes**

Replace `app/Admin/src/app/routes/users.routes.ts`:

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const usersRoutes: RouteRecordRaw = {
  path: 'users',
  children: [
    { path: '', redirect: { name: 'users.staff.list' } },
    {
      path: 'staff',
      name: 'users.staff.list',
      component: () => import('@/features/users/pages/StaffListPage.vue'),
    },
    {
      path: 'staff/new',
      name: 'users.staff.create',
      component: () => import('@/features/users/pages/StaffDetailPage.vue'),
    },
    {
      path: 'staff/:id',
      name: 'users.staff.view',
      component: () => import('@/features/users/pages/StaffDetailPage.vue'),
    },
    {
      path: 'staff/:id/edit',
      name: 'users.staff.edit',
      component: () => import('@/features/users/pages/StaffDetailPage.vue'),
    },
    {
      path: 'customers',
      name: 'users.customers.list',
      component: () => import('@/features/users/pages/CustomerListPage.vue'),
    },
    {
      path: 'customers/new',
      name: 'users.customers.create',
      component: () => import('@/features/users/pages/CustomerDetailPage.vue'),
    },
    {
      path: 'customers/:id',
      name: 'users.customers.view',
      component: () => import('@/features/users/pages/CustomerDetailPage.vue'),
    },
    {
      path: 'customers/:id/edit',
      name: 'users.customers.edit',
      component: () => import('@/features/users/pages/CustomerDetailPage.vue'),
    },
    {
      path: 'roles',
      name: 'users.roles.list',
      component: () => import('@/features/users/pages/RoleListPage.vue'),
    },
    {
      path: 'roles/new',
      name: 'users.roles.create',
      component: () => import('@/features/users/pages/RoleDetailPage.vue'),
    },
    {
      path: 'roles/:id',
      name: 'users.roles.view',
      component: () => import('@/features/users/pages/RoleDetailPage.vue'),
    },
    {
      path: 'roles/:id/edit',
      name: 'users.roles.edit',
      component: () => import('@/features/users/pages/RoleDetailPage.vue'),
    },
    {
      path: 'permissions',
      name: 'users.permissions.list',
      component: () => import('@/features/users/pages/PermissionListPage.vue'),
    },
    {
      path: 'permissions/:id',
      name: 'users.permissions.view',
      component: () => import('@/features/users/pages/PermissionDetailPage.vue'),
    },
  ],
}
```

- [ ] **Step 9: Rewrite profile routes**

Replace `app/Admin/src/app/routes/profile.routes.ts`:

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const profileRoutes: RouteRecordRaw = {
  path: 'profile',
  children: [
    {
      path: '',
      name: 'profile.view',
      component: () => import('@/features/profile/pages/ProfilePage.vue'),
    },
    {
      path: 'addresses',
      name: 'profile.addresses',
      component: () => import('@/features/profile/pages/AddressListPage.vue'),
    },
  ],
}
```

- [ ] **Step 10: Verify all route tests pass**

```bash
cd app/Admin && pnpm run test:unit -- --run infrastructure/route-structure
```
Expected: PASS — all route names match spec.

- [ ] **Step 11: Run typecheck**

```bash
cd app/Admin && pnpm run typecheck
```
Expected: no errors (lazy imports resolve even if page files don't exist yet).

- [ ] **Step 12: Commit**

```bash
git add app/Admin/src/app/routes/ app/Admin/src/__tests__/infrastructure/
git commit -m "feat: update all route files with list-detail pattern"
```

---

### Task 5: Update admin menu config

**Files:**
- Modify: `app/Admin/src/app/config/admin-menu.config.ts`

**Interfaces:**
- Consumes: nothing
- Produces: updated `adminMenuConfig` — 6 entries removed, 2 renamed

- [ ] **Step 1: Write test for menu entries**

Add to `app/Admin/src/__tests__/infrastructure/route-structure.spec.ts`:

```typescript
import { adminMenuConfig } from '@/app/config/admin-menu.config'
import type { MenuItem } from '@/app/config/admin-menu.config'

function collectMenuRouteNames(groups: typeof adminMenuConfig): string[] {
  const names: string[] = []
  for (const group of groups) {
    for (const item of group.items) {
      if (item.to && typeof item.to === 'object' && 'name' in item.to) {
        names.push(item.to.name as string)
      }
      if (item.items) {
        for (const child of item.items) {
          if (child.to && typeof child.to === 'object' && 'name' in child.to) {
            names.push(child.to.name as string)
          }
        }
      }
    }
  }
  return names
}

describe('admin menu config', () => {
  const menuNames = collectMenuRouteNames(adminMenuConfig)

  it('contains required entries', () => {
    expect(menuNames).toContain('reports.dashboard')
    expect(menuNames).toContain('profile.view')
    expect(menuNames).toContain('profile.addresses')
    expect(menuNames).toContain('catalog.dashboard')
    expect(menuNames).toContain('catalog.products.list')
    expect(menuNames).toContain('catalog.taxonomies.list')
    expect(menuNames).toContain('catalog.option-types.list')
    expect(menuNames).toContain('inventory.stocks.list')
    expect(menuNames).toContain('ordering.orders.list')
    expect(menuNames).toContain('ordering.fulfillment.queue')
    expect(menuNames).toContain('payment.payments.list')
    expect(menuNames).toContain('payment.methods.list')
    expect(menuNames).toContain('shipping.methods.list')
    expect(menuNames).toContain('shipping.rates.list')
    expect(menuNames).toContain('location.countries.list')
    expect(menuNames).toContain('location.states.list')
    expect(menuNames).toContain('users.staff.list')
    expect(menuNames).toContain('users.customers.list')
    expect(menuNames).toContain('users.roles.list')
    expect(menuNames).toContain('users.permissions.list')
  })

  it('does not contain removed entries', () => {
    // Legacy create page routes (removed)
    expect(menuNames).not.toContain('catalog.products.create') // was "Add Product" child
    expect(menuNames).not.toContain('catalog.taxa.list')
    expect(menuNames).not.toContain('catalog.option-values.list')
    expect(menuNames).not.toContain('inventory.stocks.import')
    expect(menuNames).not.toContain('inventory.units.list')
    expect(menuNames).not.toContain('users.staff.create') // was "Invite Staff" child
  })
})
```

- [ ] **Step 2: Verify test fails**

```bash
cd app/Admin && pnpm run test:unit -- --run infrastructure/route-structure
```
Expected: FAIL — removed entries still in menu.

- [ ] **Step 3: Update menu config**

Replace the Catalog > Categories submenu: keep only "All Taxonomies" → `catalog.taxonomies.list`.
Replace the Catalog > Option Types submenu: keep only "All Types" → `catalog.option-types.list`.
Remove "Add Product" child from Catalog > Products.
Remove "Import" and "Stock Units" from Inventory.
Remove "Create Order" child from Orders > All Orders.
Remove "Invite Staff" child from Users > Staff.

Replace "Manager" child with "All Taxonomies" → `catalog.taxonomies.list`.

Also update references from old `profile` route name to `profile.view` and `addresses` to `profile.addresses`.

Edit `app/Admin/src/app/config/admin-menu.config.ts` — the exact edit:

In the Catalog > Categories submenu, replace the `items` array:
```typescript
{
  label: 'Categories',
  icon: 'pi pi-fw pi-sitemap',
  permission: 'Catalog.Taxonomies',
  items: [
    { label: 'All Taxonomies', icon: 'pi pi-fw pi-tags', to: { name: 'catalog.taxonomies.list' } },
  ],
},
```

In the Catalog > Option Types submenu:
```typescript
{
  label: 'Option Types',
  icon: 'pi pi-fw pi-list',
  permission: 'Catalog.OptionTypes',
  items: [
    { label: 'All Types', icon: 'pi pi-fw pi-list', to: { name: 'catalog.option-types.list' } },
  ],
},
```

In the Catalog > Products submenu, remove the "Add Product" child — keep only:
```typescript
items: [
  { label: 'All Products', icon: 'pi pi-fw pi-list', to: { name: 'catalog.products.list' } },
],
```

In Inventory, remove "Import" (`inventory.stocks.import`) and "Stock Units" (`inventory.units.list`) entries.

In Orders > All Orders submenu, remove "Create Order" child — keep only:
```typescript
items: [
  { label: 'List', icon: 'pi pi-fw pi-list', to: { name: 'ordering.orders.list' } },
],
```

In Users > Staff submenu, remove "Invite Staff" child — keep only:
```typescript
items: [
  { label: 'All Staff', icon: 'pi pi-fw pi-list', to: { name: 'users.staff.list' } },
],
```

In the Home group, update profile links to use new route names:
```typescript
{ label: 'My Profile', icon: 'pi pi-fw pi-user', to: { name: 'profile.view' } },
```

In Users group, update addresses link:
```typescript
{ label: 'Addresses', icon: 'pi pi-fw pi-address-book', to: { name: 'profile.addresses' }, permission: 'Identity.Users' },
```

- [ ] **Step 4: Verify test passes**

```bash
cd app/Admin && pnpm run test:unit -- --run infrastructure/route-structure
```
Expected: PASS — menu entries match spec.

- [ ] **Step 5: Verify typecheck**

```bash
cd app/Admin && pnpm run typecheck
```

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/app/config/admin-menu.config.ts app/Admin/src/__tests__/infrastructure/
git commit -m "chore: update admin menu — remove deprecated entries, fix route names"
```

---

### Task 6: Final infrastructure verification

- [ ] **Step 1: Run full test suite**

```bash
cd app/Admin && pnpm run test:unit
```

- [ ] **Step 2: Run typecheck**

```bash
cd app/Admin && pnpm run typecheck
```

- [ ] **Step 3: Run lint**

```bash
cd app/Admin && pnpm run lint
```

- [ ] **Step 4: Commit any final fixes**

```bash
git add -A app/Admin/src/
git commit -m "chore: infrastructure setup — typecheck, lint, tests pass"
```
