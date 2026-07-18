# Admin SPA Standardization — Design Spec

**Date**: 2026-07-19
**Status**: Approved
**Scope**: Full Admin SPA refactor — layout fixes, menu system, shared component library, 8 module standardizations, form migration, dashboard unification, polish

---

## Section 1: Architecture & Layout Foundation

### 1.1 Layout Preferences Persistence (`Phase 0`)

`useLayout()` composable in `app/layout/composables/layout.composable.ts` currently resets on every reload. Add localStorage read on init / write on change for: `darkTheme`, `primary`, `surface`, `menuMode`, `preset`. Same pattern as Sakai Vue's `layout.js`.

- Read from `localStorage` in `layoutConfig` initialization (with defaults as fallback)
- `watch` on `layoutConfig` properties, write to `localStorage` on change
- Use `document.startViewTransition` API for dark mode toggle (already implemented)

### 1.2 Remove Duplicate FloatingConfigurator (`Phase 0`)

The bottom-right `FloatingConfigurator` button doubles the topbar's dark-mode toggle and config gear icon. Remove `FloatingConfigurator` import and instance from `Main.Layout.vue`. Keep the `Configurator.Layout.vue` panel — it's triggered only from the topbar gear icon.

### 1.3 Wire Topbar Action Buttons (`Phase 0`)

File: `app/layout/Topbar.Layout.vue:48-59`

- Profile button → dropdown menu: My Profile (route), Change Password (route), Logout (auth store action)
- Messages and Calendar buttons → remove (no backend yet). If kept, mark with `v-tooltip` no-op

### 1.4 Add Sidebar User Footer (`Phase 0`)

Add a user info section at the bottom of the sidebar in `Sidebar.Layout.vue`:
- Avatar (user initials or icon fallback)
- User display name + email
- Logout button
- Follows Sakai Vue's sidebar footer pattern

Data source: `useAuthStore()` — read current user display name, email.

### 1.5 Fix Roles/Permissions Routes (`Phase 0`)

Files: `app/router/index.ts:21-22`, `features/users/roles.routes.ts`, `features/users/permissions.routes.ts`

- `rolesRoutes` and `permissionsRoutes` are currently root-level routes with paths `/roles` and `/permissions` — they do NOT inherit `AppLayout`, so navigating to them renders without sidebar/topbar
- Move them as children of `usersRoutes` (paths `users/roles`, `users/permissions`) or as direct children of the `/` AppLayout route
- Rename route names to dot-notation: `users.roles.list`, `users.roles.create`, `users.roles.edit`, `users.roles.permissions`, `users.permissions.list`

### 1.6 Normalize All Route Names to Dot Notation (`Phase 1`)

Convert inconsistent kebab-case route names to dot-notation matching backend module boundaries:

| Before | After |
|---|---|
| `admin-users` | `users.staff.list` |
| `admin-user-create` | `users.staff.create` |
| `admin-user-detail` | `users.staff.detail` |
| `admin-user-edit` | `users.staff.edit` |
| `customer-users` | `users.customers.list` |
| `customer-detail` | `users.customers.detail` |
| `roles-list` | `users.roles.list` |
| `role-create` | `users.roles.create` |
| `role-edit` | `users.roles.edit` |
| `role-permissions` | `users.roles.permissions` |
| `permissions-list` | `users.permissions.list` |

Update all references in menu config, router links, and programmatic navigation calls.

---

## Section 2: Menu System

### 2.1 Design Principle

The sidebar menu reflects the **domain entity hierarchy** — one menu level per entity group. Sub-features of a specific entity (e.g., Product → Variants, Images) are navigated via in-page TabView, keeping the sidebar clean. Each menu item maps to a navigable route.

### 2.2 Menu Config File (`Phase 1`)

Extract `model` ref from `Menu.Layout.vue` → `app/config/admin-menu.config.ts` as a plain export. This separates data from presentation and enables:
- Permission filtering at render time
- Testing the menu config independently
- Future dynamic menu loading from API without component changes

### 2.3 Final Menu Structure

```
Home
├── Dashboard              → { name: 'reports.dashboard' }
├── My Profile             → { name: 'profile' }

Catalog                                               (permission: Catalog)
├── Dashboard              → { name: 'catalog.dashboard' }
├── Products                                         (permission: Catalog.Products)
│   ├── All Products       → { name: 'catalog.products.list' }
│   └── Add Product        → { name: 'catalog.products.create' }
├── Categories                                       (permission: Catalog.Taxonomies)
│   ├── All Categories     → { name: 'catalog.taxa.list' }
│   └── Manager            → { name: 'catalog.taxonomies.list' }
├── Option Types                                     (permission: Catalog.OptionTypes)
│   ├── All Types          → { name: 'catalog.option-types.list' }
│   └── Values             → { name: 'catalog.option-values.list' }

Inventory                                             (permission: Inventory)
├── Dashboard              → { name: 'inventory.dashboard' }
├── Stock Items            → { name: 'inventory.stocks.list' }
├── Import                 → { name: 'inventory.stocks.import' }
├── Locations              → { name: 'inventory.locations.list' }
├── Stock Units            → { name: 'inventory.units.list' }
├── Movements              → { name: 'inventory.movements.list' }
├── Transfers              → { name: 'inventory.transfers.list' }

Orders                                                (permission: Ordering)
├── Dashboard              → { name: 'ordering.dashboard' }
├── All Orders                                       (permission: Ordering.Orders)
│   ├── List               → { name: 'ordering.orders.list' }
│   └── Create Order       → { name: 'ordering.orders.create' }
├── Fulfillment            → { name: 'ordering.fulfillment.queue' }

Payments                                              (permission: Payment)
├── All Payments           → { name: 'payment.payments.list' }
├── Payment Methods        → { name: 'payment.methods.list' }

Shipping                                              (permission: Shipping)
├── Methods                → { name: 'shipping.methods.list' }
├── Rates                  → { name: 'shipping.rates.list' }

Locations                                             (permission: Location)
├── Countries              → { name: 'location.countries.list' }
├── States                 → { name: 'location.states.list' }

Users                                                 (permission: Identity.Users)
├── Staff                                            (permission: Identity.Users.Staff)
│   ├── All Staff          → { name: 'users.staff.list' }
│   └── Invite Staff       → { name: 'users.staff.create' }
├── Customers              → { name: 'users.customers.list' }
├── Addresses              → { name: 'addresses' }

Access Control                                        (permission: Identity.Roles + Identity.Permissions)
├── Roles                  → { name: 'users.roles.list' }
├── Permissions            → { name: 'users.permissions.list' }
```

### 2.4 MenuItem Type

```typescript
interface MenuItem {
  label: string
  icon?: string
  to?: RouteLocationRaw
  items?: MenuItem[]
  permission?: string           // e.g. 'Catalog.Products.View'
  separator?: boolean
  badge?: string | number
  class?: string
  disabled?: boolean
}
```

### 2.5 Permission Filtering (`Phase 1`)

The menu config declares all items. At render time in `Menu.Layout.vue`, items and empty parent groups are filtered against `authStore.permissions`:
- Items with `permission` not in user's permissions → hidden
- Groups where all children are hidden → hidden
- Group headers (root `label`) always show if they have at least one visible child

---

## Section 3: Shared Component Library (`Phase 2`)

Eight opinionated wrapper components. Live in `shared/components/`. Each wraps a PrimeVue component with project-specific defaults.

### 3.1 `DataTableShell.Component.vue` — Standardized List Table

Wraps PrimeVue `DataTable` with all project defaults:

```
Props:
  columns            ColumnDef[]        { field, header, sortable?, filter?, body?, class? }
  value              any[]              items from store
  loading            boolean
  totalRecords       number
  rows               number             10
  lazy               boolean            true
  dataKey            string             'id'
  sortField?         string
  sortOrder?         number             1 | -1
  filters?           DataTableFilterMeta
  emptyIcon          string             'pi-inbox'
  emptyTitle         string             i18n key for empty state
  emptyDescription?  string
  searchPlaceholder  string             i18n key
  showCreateButton   boolean            true
  createRoute?       RouteLocationRaw
  createLabel?       string
  showExport         boolean            false

Events:
  @page(page: PageEvent)
  @sort(sort: SortEvent)
  @filter()

Slots:
  #toolbar-actions    extra buttons in toolbar
  #row-actions="{ data }"    per-row action buttons
  #header              override entire header template
  #empty               custom empty state
  #column-{field}      per-column body override
```

Standardized behavior:
- Lazy loading with server-side pagination
- Global search via `IconField` + `InputText` in header
- Clear-filters button in header
- Skeleton rows (matching column count) during loading
- Empty state with icon + title + optional CTA
- Frozen right "Actions" column for row buttons
- `RowsPerPageDropdown` in paginator
- Optional CSV export button

### 3.2 `FormField.Component.vue` — Standardized Form Input

```
Props:
  label        string
  name         string              for `for` attr and error binding
  error?       string              from vee-validate errors object
  required?    boolean             show asterisk
  hint?        string              help text below input

Slots:
  #default     the input component
```

Renders:
- `<label>` with `text-xs uppercase tracking-wider font-bold text-surface-500 ml-1` + optional red asterisk
- `<slot />` for the actual form control
- `<small class="p-error">` for validation error
- `<small class="text-surface-400">` for hint text
- Outer wrapper: `flex flex-col gap-2`

### 3.3 `DetailField.Component.vue` — Read-Only Field Display

```
Props:
  label        string
  value?       string | number
  emptyText    string              default '—'
```

Renders:
- `text-xs text-surface-400 uppercase font-bold mb-1` label
- `text-lg font-medium text-surface-900 dark:text-surface-0` value
- Falls back to muted dash when value is null/empty/undefined

### 3.4 `StatusBadge.Component.vue` — Standardized Status Tag

```
Props:
  status       string
  statusMap    Record<string, { label: string; severity: string }>
  size?        'small' | 'normal'  default 'normal'
```

Each feature module defines its own `statusMap` (e.g., `OrderStatusMap`, `PaymentStatusMap`). The component handles Tag rendering with correct severity color.

### 3.5 `EmptyState.Component.vue` — Standardized Empty State

```
Props:
  icon          string              default 'pi-inbox'
  title         string
  description?  string
  actionLabel?  string
  actionRoute?  RouteLocationRaw
```

Renders centered icon (large, low opacity) + title + description + optional action button.

### 3.6 `ConfirmButton.Component.vue` — Delete/Archive with Dialog

```
Props:
  icon          string              default 'pi-trash'
  severity      string              default 'danger'
  rounded?      boolean             default true
  text?         boolean             default true
  header        string              confirm dialog header (i18n)
  message       string              confirm dialog body (i18n)
  acceptLabel?  string
  rejectLabel?  string
  loading?      boolean

Events:
  @confirm      emits when user accepts
```

Wraps: icon button → PrimeVue ConfirmDialog → on accept, emits `@confirm`. Caller handles the async action and toast.

### 3.7 `StatCard.Component.vue` — Dashboard Metric Card

```
Props:
  title          string
  value          string | number
  icon           string              PrimeIcons class
  iconBg         string              tailwind bg class (e.g. 'bg-blue-100 dark:bg-blue-900/20')
  trendLabel?    string              e.g. 'vs last month'
  trendValue?    number              percentage or absolute delta
  trendPositive? boolean
  skeleton?      boolean             show loading skeleton
```

Renders card with: colored circle icon + metric value + optional trend arrow + label.

### 3.8 `TabbedDetail.Component.vue` — Tab Container

```
Props:
  tabs           TabDef[]            { label, icon?, value: number|string, panel: Component }
  activeTab      number | string     v-model
  scrollable?    boolean             default true
  class?         string
```

Wraps `<Tabs>` + `<TabList>` + `<TabPanels>` with consistent styling. Each tab renders its `panel` component lazily.

---

## Section 4: Page Type Templates

Every module page is classified as one of 4 types and follows a fixed structure.

### 4.1 List Page Template

```html
<PageShell max-width="7xl">
  <PageHeader :title="t('...')" :description="t('...')">
    <template #badge>
      <Badge :value="totalRecords" severity="info" />
    </template>
    <template #actions>
      <Button icon="pi pi-refresh" severity="secondary" outlined @click="refresh" :loading="loading" />
      <Button :label="t('...create')" icon="pi pi-plus" @click="router.push(createRoute)" />
    </template>
  </PageHeader>

  <DataTableShell
    :columns="columns"
    :value="items"
    :loading="loading"
    :totalRecords="totalRecords"
    :sortField="sortField"
    :sortOrder="sortOrder"
    @page="onPage"
    @sort="onSort"
    @filter="onFilter"
    @refresh="refresh"
  >
    <template #row-actions="{ data }">
      <Button icon="pi pi-pencil" severity="secondary" text rounded @click="editItem(data)" />
      <ConfirmButton header="Delete" :message="deleteMessage(data)" @confirm="deleteItem(data)" />
    </template>
  </DataTableShell>
</PageShell>
```

### 4.2 Form Page (Create/Edit) Template

```html
<PageShell max-width="7xl">
  <PageHeader :back="true" :title="isEdit ? 'Edit X' : 'New X'" :description="t('...')">
    <template #actions>
      <Button label="Cancel" severity="secondary" outlined @click="router.back()" />
      <Button label="Save" icon="pi pi-check" :loading="submitting" @click="submitForm" />
    </template>
  </PageHeader>

  <Card class="border-none shadow-sm rounded-3xl">
    <TabbedDetail v-model="activeTab" :tabs="tabs" />
  </Card>
</PageShell>
```

### 4.3 Detail Page Template

```html
<PageShell :card="false" gap max-width="7xl">
  <PageHeader :back="true" :title="entityLabel">
    <template #badge>
      <StatusBadge :status="entity.status" :statusMap="statusMap" />
    </template>
    <template #actions>
      <!-- action buttons -->
    </template>
  </PageHeader>

  <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
    <div class="lg:col-span-2 flex flex-col gap-6">
      <!-- primary content cards -->
    </div>
    <div class="flex flex-col gap-6">
      <!-- sidebar panels: customer info, timeline, metadata -->
    </div>
  </div>
</PageShell>
```

### 4.4 Dashboard Page Template

```html
<PageShell max-width="7xl">
  <PageHeader :title="t('...')" :description="t('...')">
    <template #actions>
      <!-- date range picker, export -->
    </template>
  </PageHeader>

  <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6 mb-8">
    <StatCard v-for="stat in stats" :key="stat.title" v-bind="stat" :skeleton="loading" />
  </div>

  <div class="grid grid-cols-1 xl:grid-cols-2 gap-8">
    <Card><!-- chart --></Card>
    <Card><!-- recent items DataTable --></Card>
  </div>
</PageShell>
```

---

## Section 5: Module-by-Module Standardization

### 5.1 Round 1: Broken → Functional

#### Payment Module (`features/payment/`)

**PaymentList.View.vue** → List template:
- Add PageShell + PageHeader with title, total badge, create button
- Replace raw DataTable with DataTableShell
- Add search, pagination, sort, empty state
- Row actions: view detail, capture, void

**PaymentDetail.View.vue** → Detail template:
- Replace raw `<h2>`/`<p>` with PageShell(card=false) + PageHeader(back)
- StatusBadge for payment status
- Action buttons: Capture, Void, Refund (conditional on status)
- Two-column layout: left = transactions, right = customer + method summary
- Add full i18n

#### Shipping Module (`features/shipping/`)

**ShippingMethodList.View.vue** → List template:
- Add PageShell + PageHeader with create button
- Replace bare DataTable with DataTableShell
- Add search, pagination, empty state
- Row actions: edit, delete
- Inline isActive toggle

**ShippingRateList.View.vue** → List template:
- Same treatment as methods

### 5.2 Round 2: Inconsistent → Standard

| File | Issue | Fix |
|---|---|---|
| `catalog/taxonomies/views/TaxonomyForm.View.vue` | Custom header, not PageHeader | Replace with PageHeader |
| `ordering/fulfillment/views/FulfillmentQueue.View.vue` | No paginator, filters, batch actions | Wrap in DataTableShell |
| `location/countries/views/CountryForm.View.vue` | Manual ref-based, no zod | Migrate to vee-validate + zod + FormField |
| `location/states/views/StateForm.View.vue` | Manual ref-based, no zod | Migrate to vee-validate + zod + FormField |
| `catalog/taxonomies/taxa/views/TaxonForm.View.vue` | Is a component, not routed view | Make proper routed create/edit with PageShell |
| `inventories/` | Dashboard exists but not in menu | Add `inventory.dashboard` to menu config |

### 5.3 Round 3: Swap to Shared Components

| File | Action |
|---|---|
| `catalog/products/views/ProductList.View.vue` | Replace hand-rolled DataTable with DataTableShell. Replace `label`+`InputText` pairs with FormField |
| `catalog/products/views/ProductForm.View.vue` | Swap to FormField wrappers. Add loading skeleton. Already gold standard. |
| `ordering/orders/views/OrderList.View.vue` | Swap to DataTableShell |
| `ordering/orders/views/OrderDetail.View.vue` | Swap to DetailField + StatusBadge. Add TabView (Overview / Items / Shipments / Timeline) |
| `users/views/StaffForm.View.vue` | Migrate from manual refs to vee-validate + zod + FormField |
| `users/roles/views/RoleList.View.vue` | Ensure store usage (bypasses in some paths) |

### 5.4 Round 4: Missing Coverage

| Feature | Action |
|---|---|
| `inventories/stock-movements/` | Build StockMovementList.View + StockMovementDetail.View. API/service/store are already present. Add routes. |
| `users/views/CustomerDetail.View.vue` | Currently a placeholder (`// Placeholder` comment). Build full Detail page with TabView (Profile, Orders, Addresses) |
| `inventories/` | Add missing route: `inventory.stocks.import` review to ensure backend readiness |
| Aggregated Dashboard | `/reports/dashboard` — fix broken report APIs or migrate to domain dashboard data |

---

## Section 6: Form System Standardization

### 6.1 Mandated Stack

Every form MUST use: `vee-validate` v4 + `zod` (via `@vee-validate/zod`) + `FormField` wrapper component.

### 6.2 Schema Location Convention

```
features/{module}/{entity}/schemas/
├── create-{entity}.schema.ts
└── update-{entity}.schema.ts
```

Each schema is a function accepting the i18n `t` function:

```typescript
export function createProductSchema(t: (key: string) => string) {
  return z.object({
    name: z.string().min(1, t('validation.required')),
    slug: z.string().min(1, t('validation.required')),
    // ...
  })
}
```

### 6.3 Form Migration Targets

Forms currently using manual `ref` + `v-model` to migrate to vee-validate + zod:

1. `StaffForm.View.vue` — `features/users/views/`
2. `RoleForm.View.vue` — `features/users/roles/views/`
3. `CountryForm.View.vue` — `features/location/countries/views/`
4. `StateForm.View.vue` — `features/location/states/views/`
5. `TaxonomyForm.View.vue` — `features/catalog/taxonomies/views/`
6. `OptionTypeForm.View.vue` — `features/catalog/option-types/views/`
7. `ShippingMethodForm.View.vue` — `features/shipping/shipping-methods/views/`
8. `ShippingRateForm.View.vue` — `features/shipping/shipping-rates/views/`
9. `PaymentMethodForm.View.vue` — `features/payment/payment-methods/views/`

---

## Section 7: Dashboard Standardization

### 7.1 Shared StatCard Usage

All 4 dashboards (Catalog, Inventory, Orders, Home/Reports) use the same `StatCard` component.

### 7.2 Per-Dashboard Minimums

| Dashboard | Stat Cards | Chart | Table |
|---|---|---|---|
| **Catalog** | Total products, active, out-of-stock, categories | Products over time (line) | Recent products |
| **Inventory** | Total stock, low-stock alerts, locations, movements | Stock by location (bar) | Low-stock items |
| **Orders** | Total orders, revenue, pending, fulfillment rate | Revenue trend (line) + status breakdown (doughnut) | Recent orders |
| **Home (Reports)** | Aggregate stats from all above | Revenue trend (line) | Recent orders + activity feed |

---

## Section 8: Polish Phase (`Phase 7`)

### 8.1 Animated Gradient Topbar Border

3px bottom border on `.layout-topbar`, CSS-only gradient animation cycling through brand spectrum (emerald → teal → cyan). Respects `prefers-reduced-motion`. Single signature visual element.

### 8.2 Route Transition Animation

`<Transition name="layout-main">` wrapper around `<router-view>` in `Main.Layout.vue`. Fade + slight slide.

### 8.3 Skeleton Loading Audit

Every list page → `DataTableShell` shows skeleton rows during loading.
Every detail page → show `<ProgressSpinner>` or `<Skeleton>` shapes during fetch.
Every dashboard → `StatCard` shows skeleton when `loading=true`.

### 8.4 Empty State Audit

Every list page → `DataTableShell` uses `EmptyState` with action CTA ("Create your first X").

### 8.5 Breadcrumb Completeness

Add `meta.breadcrumb` to every route. Use i18n keys (e.g., `navigation.catalog`). The `Breadcrumb.Component.vue` already auto-generates from route `meta`.

### 8.6 Column Visibility Toggle

Add `<MultiSelect>` column chooser to `DataTableShell` toolbar for tables with 6+ columns. Saves user's column visibility to localStorage.

### 8.7 CSV Export

Button in `DataTableShell` toolbar exports the current filtered/sorted dataset to CSV using a utility function.

---

## Section 9: Execution Order

```
Phase 0: Layout Foundation         1 day
Phase 1: Menu + Route Normalization  1 day
Phase 2: Shared Component Library   2 days
Phase 3: Round 1 — Broken→Functional   2 days
Phase 4: Round 2 — Inconsistent→Standard  2 days
Phase 5: Round 3 — Swap to shared components  2 days
Phase 6: Round 4 — Missing coverage   2 days
Phase 7: Polish              1 day
                               ─────────
Total:                        13 days
```

Each phase is independently testable. Build verifies after every phase. No phase depends on a later phase.

---

## Section 10: Non-Goals (Out of Scope)

- Backend API changes (stubs return 501 — mark as known issues, do not implement backend)
- New business features (no new module domains)
- Mobile bottom tab bar (Sakai Vue uses hamburger menu for mobile; keep that pattern)
- Role-based menu from API (use static permission map for now; API-driven menu is future work)
- Visual companion / mockup generation (this is a structural spec)
