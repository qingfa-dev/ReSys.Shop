---
title: Admin SPA — Completion: Design Consistency, Missing Features & Test Coverage
version: 1.0
date_created: 2026-07-25
last_updated: 2026-07-25
owner: Admin SPA team
tags: [design, app, admin-spa, vue, primevue, sakai, testing]
---

# Introduction

The Admin SPA has 10 feature modules (Catalog, Inventory, Ordering, Payment, Shipping,
Location, Users, Profile, Reports, Auth) with 52 pages, 22 Pinia stores, and 22 shared
components. All CRUD operations are wired to backend APIs. However, a design audit
revealed 10 categories of visual and structural inconsistency, 4 critical feature gaps
remain, and 8 modules have zero test coverage.

This specification defines the completion path across three concurrent workstreams:
(1) design consistency fixes via shared layout components and Sakai-pattern enforcement,
(2) implementation of 4 missing features, and (3) 100% store and composable test coverage
for all untested modules plus complex component tests.

## 1. Purpose & Scope

**Purpose:** Define the architecture, component contracts, page patterns, feature
requirements, and test automation strategy to bring the Admin SPA to production-ready
quality with consistent Sakai/PrimeVue design, complete feature set, and comprehensive
test coverage.

**Scope:**
- **Workstream A — Design Consistency:** Create 3 shared layout components (ListLayout, DetailLayout, AppCard), fix FormActions padding, standardize grid system to Tailwind-only, fix 10 inconsistency categories across all 52 pages
- **Workstream B — Missing Features:** Implement variant image upload/management, order fulfillment workflow, advanced search + filter system, notification center
- **Workstream C — Testing:** Write store tests for 17 stores across 7 modules, composable tests where relevant, and complex component tests (forms, managers, workflows); no dashboard live-data backend work

**Out of Scope:**
- Live dashboard data (backend-side) — static chart.js placeholders remain
- Backend API changes or new endpoints
- New npm packages (use existing PrimeVue 5, Tailwind v4, Vitest, Vue Test Utils)
- E2E/Playwright tests
- Auth module (already tested, already following design patterns)

**Assumptions:**
- PrimeVue 5 with Aura preset and Sakai-inspired admin preset (emerald primary) is the design system
- `unplugin-vue-components` resolves PrimeVue components automatically — no manual imports needed
- Tailwind v4 with `tailwindcss-primeui` plugin is available
- Backend APIs exist for all CRUD operations (180+ endpoints verified)
- `src/shared/api/client.ts` Axios infrastructure is reused
- Vitest 4.1 + jsdom + @vue/test-utils 2.4 for testing

## 2. Definitions

- **Sakai** — Admin SPA design system: emerald/teal color scheme, layered surface colors, 8px spacing grid, rounded-border components, Inter + DM Serif Display fonts, dark mode support
- **ListLayout** — Shared wrapper component for list pages: orchestrates PageHeader, TableToolbar, DataTable, and loading/error/empty states
- **DetailLayout** — Shared wrapper component for detail/form pages: PageHeader, AppCard body grid, sticky FormActions footer
- **AppCard** — Single source-of-truth card component replacing `class="card"` (PrimeFlex) and inline card divs
- **Top-level entity** — A domain entity with its own route and full CRUD API endpoints (e.g., Product, Order, StockItem)
- **Sub-entity** — An entity owned by a parent, managed inline on the parent DetailPage (e.g., Variant.Prices, Taxons under Taxonomy)
- **ListTable** — Feature-scoped component rendering DataTable with row actions for a specific entity; consumed by ListLayout
- **ROUTE** — Each module exports a `ROUTE` constant object with all route name strings, replacing bare string route references
- **Workstream** — A parallel development track (A/B/C) that can be executed independently

## 3. Requirements, Constraints & Guidelines

### Design Consistency Requirements

- **REQ-DSN-001**: All 52 pages SHALL use ListLayout or DetailLayout as their root wrapper component
- **REQ-DSN-002**: All card-like visual containers SHALL use AppCard; no inline `rounded-border border p-5` divs
- **REQ-DSN-003**: All grid layouts SHALL use Tailwind CSS (`grid grid-cols-* gap-*`); no PrimeFlex `grid`/`col-*` classes
- **REQ-DSN-004**: All buttons SHALL use PrimeVue `<Button>` component; no raw `<button class="p-button">`
- **REQ-DSN-005**: All user-facing strings SHALL use Vue I18n `t()` function; no hardcoded English text
- **REQ-DSN-006**: All pages SHALL pass a subtitle i18n key to PageHeader; subtitle is mandatory
- **REQ-DSN-007**: All PageHeader instances SHALL include the `icon` prop via `route.meta?.icon`
- **REQ-DSN-008**: All list pages SHALL emit `@page` events from DataTable and react to them in the store; no `@page="() => {}"` no-ops
- **REQ-DSN-009**: All route navigation SHALL use `ROUTE` constants; no bare route name strings like `'profile.addresses'`
- **REQ-DSN-010**: The Dashboard page (reports) SHALL use shared DataTable, not raw HTML `<table>`
- **REQ-DSN-011**: Standard card padding is `p-5` (20px) — AppCard, FormActions negative margin, and DataTable wrapper SHALL all use this value
- **REQ-DSN-012**: All `catch` blocks in stores and components SHALL include `console.error(err)` for debugging

### Shared Component Requirements

- **REQ-CMP-001**: ListLayout SHALL accept `pageSize` (default 20), `first` computed as `page * pageSize`, and emit `@page` events with correct pagination sync
- **REQ-CMP-002**: DetailLayout SHALL accept a `saving` prop to disable FormActions buttons during submission
- **REQ-CMP-003**: AppCard SHALL support `padding` prop variants: `sm` (p-3), `md` (p-5, default), `lg` (p-6)
- **REQ-CMP-004**: FormActions SHALL have negative margin matching AppCard default padding (`-mx-5` not `-mx-6`) for correct sticky footer alignment
- **REQ-CMP-005**: TableToolbar SHALL include a search keydown handler that emits `@search` events (debounced), not just a `v-model` with no event
- **REQ-CMP-006**: DataTable wrapper SHALL use `rounded-border border border-surface-200 bg-white dark:border-surface-700 dark:bg-surface-900 overflow-hidden` for consistent card appearance

### Missing Feature Requirements

- **REQ-FEA-001**: VariantImageManager SHALL support drag-and-drop file upload with preview, checkbox to set primary image, button to delete per image, and drag-reorder via native HTML5 DnD or vuedraggable
- **REQ-FEA-002**: FulfillmentWorkflow SHALL render PrimeVue Steps component with 7 states (Pending, Confirmed, Processing, Picked, Packed, Shipped, Delivered) and 2 escape hatches (Cancel, Return); each step SHALL have an action panel with corresponding API call
- **REQ-FEA-003**: Search bar SHALL debounce input by 300ms and synchronize the store's `query` ref, triggering automatic re-fetch on change
- **REQ-FEA-004**: FilterPanel SHALL render per-entity column filter configuration (text input, select dropdown, date range picker, number range slider) with removable active filter chips below the toolbar
- **REQ-FEA-005**: Notification bell in the topbar SHALL show an unread count badge; clicking opens a popover with the 5 most recent notifications; auto-polls every 30 seconds
- **REQ-FEA-006**: Notification store SHALL expose `unreadCount: Ref<number>`, `items: Ref<Notification[]>`, `markRead(id)`, `markAllRead()`, and `fetch()` actions

### Constraints

- **CON-001**: No new npm packages — reuse existing `primevue`, `@primevue/themes`, `vee-validate`, `@vee-validate/zod`, `chart.js`, `vue-i18n`, `vitest`, `@vue/test-utils`
- **CON-002**: No direct module-to-module component imports — all cross-module communication via shared components or Pinia stores only
- **CON-003**: All Pinia stores SHALL expose state as `readonly()` refs
- **CON-004**: All destructive actions (delete, cancel order, void payment) SHALL use `useConfirm` dialog before proceeding
- **CON-005**: All mutation outcomes (create, update, delete, status change) SHALL show toast notifications
- **CON-006**: No backend API changes — work within existing endpoint contracts
- **CON-007**: `vue-tsc --noEmit` SHALL pass with zero errors after all changes

### Guidelines

- **GUD-001**: Prefer Tailwind utility classes over inline styles; avoid `style=""` attributes
- **GUD-002**: Use `gap-4` (16px) for form field grids, `gap-6` (24px) for section spacing, `gap-2` (8px) for inline action buttons
- **GUD-003**: Form field width defaults: text inputs `col-span-full sm:col-span-6`, textareas `col-span-full`, selects `col-span-full sm:col-span-6`
- **GUD-004**: ListTable components SHALL NOT handle their own loading/error/empty states — delegate to ListLayout
- **GUD-005**: DetailPage mode detection: `!id → create`, `route.name endsWith('.edit') → edit`, otherwise `view`

### Patterns

- **PAT-001**: Store methods return `Result<T>` and callers check `result.isSuccess` before mutating state
- **PAT-002**: Store `getMany` always resets `items` on error: `items.value = []; totalRecords.value = 0`
- **PAT-003**: API service files export static async functions, not classes
- **PAT-004**: Form components accept loading state via `props` and emit `saved`/`cancelled` events, not router.push directly
- **PAT-005**: `onMounted` in list pages calls store `fetchMany({ page: 1, pageSize: 20 })` with default params
- **PAT-006**: ROUTE constants use nested objects: `ROUTE.ENTITY.ACTION` (e.g., `ROUTE.COUNTRIES.LIST`)

## 4. Design Consistency: Shared Layout Components

### 4.1 ListLayout

```
┌─────────────────────────────────────────────────────────────┐
│ <ListLayout>                                                 │
│                                                               │
│ #header slot                                                 │
│   <PageHeader title="..." subtitle="..." :icon="..." />     │
│                                                               │
│ #toolbar slot                                                │
│   <TableToolbar @search="..." @filter="..." />               │
│                                                               │
│ #default slot (content)                                       │
│   <LoadingSkeleton v-if="loading && !items.length" />         │
│   <ErrorState v-else-if="error" @retry="retry" />            │
│   <EmptyState v-else-if="!items.length" />                   │
│   <DataTable v-else :rows="items" :loading="loading"         │
│     :total-records="totalRecords" :page-size="pageSize"      │
│     :first="page * pageSize" @page="onPage">                │
│     <Column ... />                                           │
│     <Column #rowActions> <ActionMenu ... /> </Column>        │
│   </DataTable>                                                │
│                                                               │
│   <BulkActionBar v-if="selected.length" />                    │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

**Props:**
| Prop | Type | Default | Description |
|------|------|---------|-------------|
| None — purely structural wrapper | — | — | All state managed by parent page |

**Slots:**
| Slot | Purpose |
|------|---------|
| `header` | PageHeader component |
| `toolbar` | TableToolbar component |
| `default` | DataTable + state guards + ActionMenu |
| `bulk-actions` | BulkActionBar (optional) |

**Responsibilities:**
- Provides consistent `px-6 py-4` outer spacing
- Does NOT own loading/error/empty state — consumes it from parent page or store
- Does NOT own pagination — receives `@page` events and forwards them

### 4.2 DetailLayout

```
┌─────────────────────────────────────────────────────────────┐
│ <DetailLayout :saving="saving">                               │
│                                                               │
│ #header slot                                                 │
│   <PageHeader :title="..." subtitle="..." :icon="..." />     │
│                                                               │
│ #actions slot                                                │
│   <Button v-if="canEdit" label="Edit" @click="edit" />       │
│   <Button label="Approve" @click="approve" /> (lifecycle)    │
│                                                               │
│ #default slot (body)                                          │
│   <AppCard>                                                   │
│     <LoadingSkeleton v-if="loading" />                        │
│     <ErrorState v-else-if="error" @retry="load" />           │
│     <div v-else class="grid grid-cols-12 gap-4">             │
│       <FormField ... class="col-span-full sm:col-span-6" />  │
│       ...                                                     │
│     </div>                                                    │
│   </AppCard>                                                  │
│                                                               │
│ #sub-entities slot (optional)                                 │
│   <AppCard v-for="sub in subEntities">                       │
│     <Fieldset :legend="...">                                  │
│       ... sub-entity DataTable ...                            │
│     </Fieldset>                                               │
│   </AppCard>                                                  │
│                                                               │
│ #footer slot                                                 │
│   <FormActions :saving="saving" @save="save"                  │
│     @cancel="cancel" />                                       │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

**Props:**
| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `saving` | `boolean` | `false` | Disables save/cancel buttons during submission |

**Slots:**
| Slot | Purpose |
|------|---------|
| `header` | PageHeader (back-button, title, subtitle, icon) |
| `actions` | Header action buttons (Edit, lifecycle actions) |
| `default` | AppCard with form grid + state guards |
| `sub-entities` | Additional AppCard sections for sub-entities |
| `footer` | FormActions with save/cancel |

**Responsibilities:**
- Does NOT own form state or validation logic
- Does NOT own loading/error state — consumes from parent
- Provides consistent outer spacing and sticky footer
- Manages only the `saving` prop passthrough

### 4.3 AppCard

**Props:**
| Prop | Type | Default | Description |
|------|------|---------|-------------|
| `padding` | `'sm' \| 'md' \| 'lg'` | `'md'` | Internal padding: sm=p-3, md=p-5, lg=p-6 |
| `as` | `string` | `'div'` | Element type (div, section, article) |

**Template structure:**
```html
<component :is="as"
  class="rounded-border border border-surface-200 bg-white
         dark:border-surface-700 dark:bg-surface-900"
  :class="{
    'p-3': padding === 'sm',
    'p-5': padding === 'md',
    'p-6': padding === 'lg',
  }">
  <slot />
</component>
```

### 4.4 Components to Fix

**FormActions.vue:**
- Change `-mx-6` to `-mx-5` to match AppCard default padding
- Change parent assumption from `px-6` to `px-5`

**PageHeader.vue:**
- Add mandatory `subtitle` prop (string, i18n key)
- Add validation warning if icon not provided (console.warn in dev mode)

**TableToolbar.vue:**
- Add `searchValue` v-model with 300ms debounce
- Add `@search` emit on debounced change
- Add filter button slot
- Add active filter chips row

**DataTable wrapper:**
- Replace current bare `rounded-border border ... overflow-hidden` classes
- Add `bg-white dark:bg-surface-900` for consistent card background

**BulkActionBar.vue:**
- Move from `position: fixed` to flexible positioning within ListLayout
- Ensure it has `mb-4` spacing when visible

### 4.5 Components to Delete

- `DetailDrawer.vue` — unused; remove file and barrel export entry
- `FilterPanel.vue` — placeholder stub; remove and reimplement as part of Workstream B

### 4.6 Grid System Unification

**Rule:** Tailwind CSS grid only (`grid grid-cols-N gap-N`). No PrimeFlex `grid` class, no `col-N`, `col-offset-N`, `field`, `formgrid`.

**Migration map:**
| PrimeFlex class | Tailwind replacement |
|---|---|
| `<div class="grid">` | `<div class="grid grid-cols-12 gap-4">` |
| `<div class="col-6">` | `<div class="col-span-full sm:col-span-6">` |
| `<div class="col-4">` | `<div class="col-span-full sm:col-span-4">` |
| `<div class="col-3">` | `<div class="col-span-full sm:col-span-3">` |
| `<div class="col-12">` | `<div class="col-span-full">` |
| `class="card"` | `<AppCard>` |

**Files to migrate from PrimeFlex grid:**
- `catalog/components/ProductForm.vue` — uses `class="grid"` + `col-6`, `col-12`
- `location/components/CountryForm.vue` — uses `class="grid"` + `col-3`, `col-4`, `col-6`
- `profile/components/AddressForm.vue` — uses `class="formgrid grid"` + `col-6`
- All other form components — audit and migrate

### 4.7 i18n Coverage Gaps

**Pages with hardcoded English (must fix):**
| Page | Hardcoded Strings | Action |
|------|-------------------|--------|
| `StockListPage` | "Stock Items" title | Add `inventory.stocks.title` key |
| `StaffListPage` | "Staff" title, "Manage staff accounts" subtitle | Add `users.staff.title`, `users.staff.subtitle` keys |
| `DashboardPage` (reports) | "Analytics Dashboard" title, subtitle | Add `reports.dashboard.title`, `reports.dashboard.subtitle` keys |
| `CountryForm` | "Create Country", "Edit:", "Country Details", "Save Country" | Add `location.countries.*` keys |

**All empty states (must fix):**
Every list component's `<EmptyState title="..." description="..." />` SHALL use i18n keys.

**i18n key namespace convention:**
```
module.entity.label
```
Examples: `catalog.products.title`, `inventory.stocks.subtitle`, `users.roles.empty.title`

## 5. Design Consistency: Per-Module Page Inventory

### 5.1 Catalog (9 pages, 8 components)

| Page | Layout | Components | State |
|------|--------|------------|-------|
| DashboardPage | ListLayout (no table) | StatCard grid | Page-level state guards |
| ProductListPage | ListLayout | ProductListTable | productStore |
| ProductDetailPage | DetailLayout | ProductForm, VariantListTable, VariantImageManager, PriceListTable, ProductClassificationManager | productStore + variantStore |
| VariantListPage | ListLayout | VariantListTable | variantStore |
| VariantDetailPage | DetailLayout | VariantForm, VariantImageManager, OptionValueManager, PriceManager | variantStore |
| OptionTypeListPage | ListLayout | OptionTypeListTable | optionTypeStore |
| OptionTypeDetailPage | DetailLayout | OptionTypeForm, OptionValueManager | optionTypeStore |
| TaxonomyListPage | ListLayout | TaxonomyListTable | taxonomyStore |
| TaxonomyDetailPage | DetailLayout | TaxonomyForm, TaxonManager (MPTT depth-indented DataTable) | taxonomyStore |

### 5.2 Inventory (9 pages, 8 components)

| Page | Layout | Components | State |
|------|--------|------------|-------|
| DashboardPage | ListLayout (no table) | StatCard grid | Page-level state guards |
| StockListPage | ListLayout | StockItemListTable | stockItemStore |
| StockItemDetailPage | DetailLayout | StockItemForm | stockItemStore |
| LocationListPage | ListLayout | StockLocationListTable | stockLocationStore |
| LocationDetailPage | DetailLayout | StockLocationForm | stockLocationStore |
| MovementListPage | ListLayout | StockMovementListTable (read-only) | stockMovementStore |
| TransferListPage | ListLayout | StockTransferListTable | stockTransferStore |
| TransferDetailPage | DetailLayout | StockTransferForm | stockTransferStore |
| StockReservationListPage | ListLayout | StockReservationListTable (cancel action) | stockReservationStore |

### 5.3 Ordering (4 pages, 3 components)

| Page | Layout | Components | State |
|------|--------|------------|-------|
| DashboardPage | ListLayout (no table) | StatCard grid | Page-level state guards |
| OrderListPage | ListLayout | OrderListTable | orderStore |
| OrderDetailPage | DetailLayout | OrderForm, FulfillmentWorkflow (NEW), OrderLineItemsTable | orderStore |
| FulfillmentQueuePage | ListLayout | OrderListTable (filtered: processing orders), BulkActionBar | orderStore |

### 5.4 Users (8 pages, 8 components)

| Page | Layout | Components | State |
|------|--------|------------|-------|
| StaffListPage | ListLayout | UserListTable (filter: staff role) | userStore |
| StaffDetailPage | DetailLayout | UserForm, UserRoleManager, UserPermissionManager | userStore + roleStore |
| CustomerListPage | ListLayout | UserListTable (filter: customer role) | userStore |
| CustomerDetailPage | DetailLayout | UserForm (read-only fields), AddressList | userStore |
| RoleListPage | ListLayout | RoleListTable | roleStore |
| RoleDetailPage | DetailLayout | RoleForm, RolePermissionManager | roleStore |
| PermissionListPage | ListLayout | PermissionListTable (read-only) | permissionStore |
| PermissionDetailPage | DetailLayout | PermissionForm | permissionStore |

### 5.5 Payment (4 pages, 3 components)

| Page | Layout | Components | State |
|------|--------|------------|-------|
| PaymentListPage | ListLayout | PaymentListTable | paymentStore |
| PaymentDetailPage | DetailLayout | PaymentForm (read-only fields), lifecycle buttons (capture/void/refund) | paymentStore |
| PaymentMethodListPage | ListLayout | PaymentMethodListTable | paymentMethodStore |
| PaymentMethodDetailPage | DetailLayout | PaymentMethodForm | paymentMethodStore |

### 5.6 Shipping (4 pages, 4 components)

| Page | Layout | Components | State |
|------|--------|------------|-------|
| ShippingMethodListPage | ListLayout | ShippingMethodListTable | shippingMethodStore |
| ShippingMethodDetailPage | DetailLayout | ShippingMethodForm | shippingMethodStore |
| ShippingRateListPage | ListLayout | ShippingRateListTable | shippingRateStore |
| ShippingRateDetailPage | DetailLayout | ShippingRateForm | shippingRateStore |

### 5.7 Location (4 pages, 3 components)

| Page | Layout | Components | State |
|------|--------|------------|-------|
| CountryListPage | ListLayout | CountryListTable | countryStore |
| CountryDetailPage | DetailLayout | CountryForm | countryStore |
| StateListPage | ListLayout | StateListTable | stateStore |
| StateDetailPage | DetailLayout | StateForm | stateStore |

### 5.8 Profile (3 pages, 1 component)

| Page | Layout | Components | State |
|------|--------|------------|-------|
| ProfilePage | DetailLayout | ProfileForm, password change section | profileStore |
| AddressListPage | ListLayout | AddressListTable | addressStore |
| AddressDetailPage | DetailLayout | AddressForm | addressStore |

### 5.9 Reports (1 page)

| Page | Layout | Components | State |
|------|--------|------------|-------|
| DashboardPage | ListLayout (no table) | StatCard grid, chart.js Chart widgets, shared DataTable for monthly performance | Page-level state guards |

### 5.10 Auth (5 pages — no changes)

Auth module already uses its own `AuthLayout` and follows Sakai design patterns. All 5 pages are already tested.

## 6. Missing Feature: Variant Image Upload & Management

### 6.1 Architecture

```
VariantDetailPage
 └── #sub-entities slot
      └── AppCard
           └── VariantImageManager
                ├── FileDropZone (drag-and-drop upload area)
                ├── ImageGrid (thumbnail grid with drag-reorder)
                ├── ImageCard (per-image: preview, primary star, delete X)
                └── UploadProgress (per-file progress indicator)
```

### 6.2 Data Flow

```
User drops files on FileDropZone
  → FileDropZone emits @files-selected
  → VariantImageManager: for each File, POST /catalog/variants/{variantId}/images
  → On success: append to local images array, show toast
  → On error: show toast with error message

User clicks delete on ImageCard
  → useConfirm dialog
  → DELETE /catalog/variants/{variantId}/images/{imageId}
  → On success: remove from local array, show toast

User drag-reorders images
  → PUT /catalog/variants/{variantId}/images/reorder with index map
  → On success: update local array order

User clicks star to set primary
  → (if API exists) PUT /catalog/variants/{variantId}/images/{imageId}/primary
  → On success: update local array, mark card as primary
```

### 6.3 Component Contracts

**VariantImageManager props:**
| Prop | Type | Description |
|------|------|-------------|
| `variantId` | `string` | ID of the variant to manage images for |
| `images` | `VariantImageResponse[]` | Current images array (may be empty on create) |

**VariantImageManager emits:**
| Event | Payload | Description |
|-------|---------|-------------|
| `update:images` | `VariantImageResponse[]` | Emitted when images array changes |

**FileDropZone:**
- Accepts image/* MIME types
- Highlights border on drag-over
- Shows file count and size limits
- Emits `@files-selected` with `File[]`

**ImageGrid:**
- Renders images in a CSS grid (4 columns, gap-4)
- Supports HTML5 drag-and-drop reorder via `draggable` events
- Each ImageCard is `draggable="true"` with dragstart/dragover/dragend/drop handlers

**ImageCard:**
- 16:9 aspect ratio thumbnail via `object-cover`
- Star icon overlay (filled gold if primary, outline gray if not)
- Delete X button (top-right corner, translucent background)
- Upload progress bar overlay (during upload)

### 6.4 Edge Cases

- Uploading while images array is empty: show empty state "No images. Drag and drop to upload."
- Uploading duplicate filenames: API handles this; show error toast
- Maximum 10 images per variant: disable drop zone when count >= 10
- Network failure during upload: show error toast, keep files in queue for retry
- Component mounted on new variant (no variantId yet): disable drop zone until variant is saved

### 6.5 API Endpoints Used

| Method | Path | Purpose |
|--------|------|---------|
| `POST` | `/catalog/variants/{variantId}/images` | Upload image (multipart/form-data) |
| `DELETE` | `/catalog/variants/{variantId}/images/{imageId}` | Delete image |
| `PUT` | `/catalog/variants/{variantId}/images/reorder` | Reorder images (array of {imageId, position}) |
| `PUT` | `/catalog/variants/{variantId}/images/{imageId}/primary` | Set primary image (if available) |

## 7. Missing Feature: Order Fulfillment Workflow

### 7.1 Architecture

```
OrderDetailPage
 └── #sub-entities slot
      └── AppCard
           └── FulfillmentWorkflow
                ├── StepsBar (PrimeVue Steps: Pending → ... → Delivered)
                ├── CurrentStepPanel (action buttons for current status)
                └── Timeline (collapsed history of status changes, expandable)
```

### 7.2 State Machine

```
State: Pending
│ action: ConfirmOrder  → POST /ordering/orders/{id}/approve
▼
State: Confirmed
│ action: ProcessOrder  → (set processing status)
▼
State: Processing
│ action: MarkPicked    → (set picked status)
▼
State: Picked
│ action: MarkPacked    → (set packed status)
▼
State: Packed
│ action: ShipOrder     → (set shipped status, optionally add tracking)
▼
State: Shipped
│ action: MarkDelivered → (set delivered status)
▼
State: Delivered [terminal]

Any non-terminal state:
│ action: CancelOrder   → POST /ordering/orders/{id}/cancel
▼
State: Cancelled [terminal]

State: Shipped or Delivered:
│ action: ReturnOrder   → (initiate return)
▼
State: Returned [terminal]
```

### 7.3 Component Contracts

**FulfillmentWorkflow props:**
| Prop | Type | Description |
|------|------|-------------|
| `orderId` | `string` | ID of the order |
| `status` | `OrderStatus` | Current order status enum |
| `statusHistory` | `StatusHistoryItem[]` | Array of past status changes |

**FulfillmentWorkflow emits:**
| Event | Payload | Description |
|-------|---------|-------------|
| `status-changed` | `{ newStatus: OrderStatus }` | After successful status transition |

**StepsBar:**
- Renders PrimeVue `<Steps>` with `activeStep` bound to current status index
- Completed steps show checkmark icon, green color
- Current step shows spinner if transition in progress
- Future steps show grayed-out, muted color
- Error states (Cancelled, Returned) show red/warning at the step where transition stopped

**CurrentStepPanel:**
- Shows contextual action button based on current status
- Button label: "Confirm Order", "Start Processing", "Mark as Picked", "Mark as Packed", "Ship Order", "Mark as Delivered"
- Show escape-hatch buttons: "Cancel Order" (red, severe) for any non-terminal; "Process Return" for Shipped/Delivered
- Disabled state during API call in progress
- Confirm dialog before destructive actions (cancel, return)

### 7.4 Fulfillment Queue Page

The existing `FulfillmentQueuePage` SHALL be enhanced with a `BulkActionBar`:

```
┌──────────────────────────────────────────────────────────────┐
│ [Batch Pick]  [Batch Pack]  [Batch Ship]   3 orders selected  │
└──────────────────────────────────────────────────────────────┘
```

- Row checkboxes on DataTable
- BulkActionBar appears when 1+ rows selected
- Batch actions: Pick (mark all selected as Picked), Pack, Ship
- Each bulk action confirms via `useConfirm` then calls API per order sequentially
- Progress toast: "Processing 3 of 5 orders..." with cancel option

### 7.5 Edge Cases

- Transition from shipped back to processing: not allowed — API rejects, show error toast
- Concurrent transitions on same order: handle API conflict (409) gracefully, show status refresh
- Bulk action partial failure: 3 of 5 succeeded, show summary toast with failures
- Order already fulfilled (Delivered): all action buttons hidden, show "Delivered on {date}"

## 8. Missing Feature: Advanced Search & Filters

### 8.1 Architecture

```
ListLayout (toolbar slot)
 └── TableToolbar
      ├── SearchInput (debounced, 300ms, v-model:query → store.query)
      ├── FilterButton (toggles FilterPanel visibility)
      ├── ColumnToggleButton (shows/hides DataTable columns)
      ├── FilterChips (removable chips for active filters)
      │    └── Chip "Status: Active ×" (click × removes filter)
      └── FilterPanel (slide-down or popover panel with filter forms)
           ├── FilterField (per-column: text input, select, date range, number range)
           ├── ApplyFilters / ClearAll buttons
           └── SavedFilterPreset dropdown (future enhancement)
```

### 8.2 Data Flow

```
User types in SearchInput
  → debounce 300ms
  → emit @search with { query: "foo" }
  → ListPage updates store.query = "foo"
  → ListPage calls store.fetchMany({ query: "foo", filters: currentFilters, page: 1 })

User opens FilterPanel, selects "Status = Active", clicks Apply
  → emit @filter with { filters: [{ field: "status", op: "eq", value: "Active" }] }
  → ListPage updates store.filters = [...]
  → TableToolbar renders FilterChips for active filters
  → ListPage calls store.fetchMany({ query, filters: newFilters, page: 1 })

User clicks × on a FilterChip
  → remove that filter from store.filters
  → re-fetch with updated filters

User clicks ClearAll in FilterPanel
  → store.filters = []
  → re-fetch
  → FilterChips disappear
```

### 8.3 Component Contracts

**TableToolbar enhancements:**
| Prop | Type | Description |
|------|------|-------------|
| `query` | `string` | Current search query (v-model) |
| `filters` | `FilterConfig[]` | Active filter configurations |
| `searchPlaceholder` | `string` | Placeholder text for search input |
| `filterDefinitions` | `ColumnFilterDef[]` | Available filter fields (column name, type, options) |

| Emit | Payload | Description |
|------|---------|-------------|
| `update:query` | `string` | Debounced (at 300ms, not immediate) |
| `update:filters` | `FilterConfig[]` | When filters applied or removed |
| `search` | `{ query: string }` | Deprecated in favor of `update:query` |
| `column-toggle` | `string[]` | Array of visible column keys |

**FilterPanel (rebuilt, replacing deleted stub):**
| Prop | Type | Description |
|------|------|-------------|
| `definitions` | `ColumnFilterDef[]` | Per-column filter field definitions |
| `modelValue` | `boolean` | Panel visibility (v-model) |
| `activeFilters` | `FilterConfig[]` | Current filter values |

| Emit | Payload | Description |
|------|---------|-------------|
| `apply` | `FilterConfig[]` | User clicked Apply |
| `clear` | — | User clicked Clear All |

**ColumnFilterDef type:**
```ts
interface ColumnFilterDef {
  field: string           // e.g., "status", "price", "createdAt"
  label: string           // Display label
  type: 'text' | 'select' | 'date-range' | 'number-range' | 'boolean'
  options?: { label: string; value: string }[]  // For 'select' type
}
```

**FilterConfig type:**
```ts
interface FilterConfig {
  field: string
  operator: 'eq' | 'neq' | 'gte' | 'lte' | 'contains' | 'between'
  value: string | number | [number, number] | [string, string]
}
```

### 8.4 Store Integration

Each store that supports filters SHALL add:
```ts
const query = ref('')
const filters = ref<FilterConfig[]>([])

function fetchMany(params?: { page?: number; pageSize?: number }) {
  return getMany({
    page: page.value,
    pageSize: pageSize.value,
    query: query.value || undefined,
    filters: filters.value.length > 0 ? filters.value : undefined,
  })
}
```

### 8.5 Edge Cases

- Search query is empty string: API receives no query param (returns all records)
- Filter with zero results: DataTable shows EmptyState, filter chips remain visible
- Rapid typing in search: 300ms debounce, previous in-flight request is aborted (Axios cancel token or ignore response)
- Filter with date range "from" > "to": validate client-side, show inline error on FilterPanel
- Navigation away and back: store.query and store.filters persist in Pinia; page re-fetches on mount

## 9. Missing Feature: Notification System

### 9.1 Architecture

```
App.vue (existing)
 └── Topbar (existing layout/_topbar.scss)
      └── NotificationBell (NEW)
           ├── Bell icon (PrimeIcons pi-bell)
           ├── Badge (unread count, red circle, hidden if 0)
           └── Popover (PrimeVue Popover, opens on click)
                └── NotificationList
                     ├── NotificationItem × 5
                     │    ├── Dot indicator (● unread, ○ read)
                     │    ├── Title text (truncated 1 line)
                     │    ├── Relative time ("2m ago")
                     │    └── Click → mark as read + navigate
                     ├── Divider
                     └── Footer
                          ├── "Mark all read" button
                          └── "See all notifications" link
```

### 9.2 Data Flow

```
App.vue onMounted
  → notificationStore.startPolling(30000) // 30s interval

NotificationBell click
  → notificationStore.fetch()
  → popover opens with latest 5 items

NotificationItem click
  → notificationStore.markRead(id)
  → if notification.linkRoute: router.push(linkRoute)
  → close popover

"Mark all read" click
  → notificationStore.markAllRead()

Polling timer fires
  → notificationStore.fetch()
  → unreadCount updates
  → badge re-renders
```

### 9.3 Component Contracts

**NotificationBell:**
- No props (consumes notificationStore directly)
- Renders bell icon + badge
- Opens Popover on click

**NotificationList:**
| Prop | Type | Description |
|------|------|-------------|
| `items` | `Notification[]` | Array of notifications (max 5 displayed) |
| `loading` | `boolean` | Loading state |

| Emit | Payload | Description |
|------|---------|-------------|
| `mark-read` | `string` | Notification ID to mark as read |
| `mark-all-read` | — | Mark all as read |
| `see-all` | — | Navigate to full notification page |

**NotificationItem:**
| Prop | Type | Description |
|------|------|-------------|
| `notification` | `Notification` | Single notification object |

### 9.4 Store Contract

**useNotificationStore:**
```ts
interface Notification {
  id: string
  type: 'order_status' | 'payment_status' | 'stock_alert' | 'system'
  title: string
  message: string
  linkRoute?: { name: string; params?: Record<string, string> }
  isRead: boolean
  createdAt: string  // ISO 8601
}

// State
readonly unreadCount: Ref<number>      // Badge count
readonly items: Ref<Notification[]>    // All loaded notifications
readonly recentItems: ComputedRef<Notification[]>  // Last 5

// Actions
fetch(): Promise<void>                 // GET /notifications?limit=50
markRead(id: string): Promise<void>    // PUT /notifications/{id}/read
markAllRead(): Promise<void>           // PUT /notifications/read-all
startPolling(intervalMs: number): void // Set interval
stopPolling(): void                    // Clear interval
```

### 9.5 Fallback Mode

If a Notification API does not exist on the backend, the notification store SHALL operate in
client-only mode:

- `fetch()`: no-op (empty array)
- `markRead()`, `markAllRead()`: no-op
- `startPolling()`: no-op
- UI renders normally but shows "No notifications" empty state
- This enables a future backend integration without UI code changes

### 9.6 Edge Cases

- 0 unread notifications: hide badge, bell icon in muted color
- 99+ unread: show "99+" in badge
- Notification navigation targets deleted entity (e.g., deleted order): catch route error, show toast "This item no longer exists"
- Polling fails (network error): silently retry next interval, don't show error to user
- User on a different tab: polling continues; when they click bell, latest items are shown

## 10. Testing Strategy

### 10.1 Store Tests (17 stores, all new)

**Pattern (per store):**
```ts
// features/<module>/store/__tests__/<entity>.store.spec.ts
import { setActivePinia, createPinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useXxxStore } from '../xxx.store'
import { XxxApi } from '../../api'
import type { XxxResponse } from '../../types'

vi.mock('../../api')

describe('useXxxStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('fetchMany', () => {
    it('sets items and totalRecords on success', async () => { ... })
    it('sets loading=true during fetch', async () => { ... })
    it('resets items to [] on error', async () => { ... })
    it('sets error message on failure', async () => { ... })
    it('passes page and pageSize params', async () => { ... })
  })

  describe('getById', () => {
    it('sets currentItem on success', async () => { ... })
    it('sets error on not-found', async () => { ... })
    it('sets loading during fetch', async () => { ... })
  })

  describe('create', () => {
    it('calls API create with form data', async () => { ... })
    it('returns success result', async () => { ... })
    it('handles validation error', async () => { ... })
  })

  describe('update', () => {
    it('calls API update with id and form data', async () => { ... })
    it('handles not-found error', async () => { ... })
  })

  describe('delete', () => {
    it('calls API delete with id', async () => { ... })
    it('removes item from local array on success', async () => { ... })
  })

  // Module-specific actions
  describe('activate', () => { ... })     // payment-method, shipping-method
  describe('deactivate', () => { ... })   // payment-method, shipping-method
  describe('assignRole', () => { ... })   // user
  describe('revokeRole', () => { ... })   // user
  describe('cancel', () => { ... })       // order, stock-reservation, stock-transfer
  describe('complete', () => { ... })     // order
})
```

**Stores to test:**
| Module | Stores | Estimated test count |
|--------|--------|---------------------|
| Inventory | stock-item, stock-location, stock-movement, stock-reservation, stock-transfer | ~40 tests |
| Location | country, state | ~20 tests |
| Ordering | order | ~15 tests |
| Payment | payment, payment-method | ~20 tests |
| Profile | profile, address | ~20 tests |
| Reports | (no store) | 0 |
| Shipping | shipping-method, shipping-rate | ~20 tests |
| Users | user, role, permission | ~25 tests |
| **Total** | **17 stores** | **~160 tests** |

### 10.2 API Tests

**Pattern (per API module):**
```ts
// features/<module>/api/__tests__/<entity>.api.spec.ts
import { apiClient } from '@/shared/api/client'
import * as api from '../<entity>.api'
import { describe, expect, it, vi } from 'vitest'

vi.mock('@/shared/api/client', () => ({
  apiClient: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}))

describe('XxxApi', () => {
  describe('getMany', () => {
    it('calls GET with correct URL', async () => { ... })
    it('serializes query params correctly', async () => { ... })
    it('returns paged result on success', async () => { ... })
  })
  describe('getById', () => {
    it('calls GET with correct URL including id', async () => { ... })
  })
  describe('create', () => {
    it('calls POST with correct URL and body', async () => { ... })
  })
  describe('update', () => {
    it('calls PUT with correct URL and body', async () => { ... })
  })
  describe('delete', () => {
    it('calls DELETE with correct URL', async () => { ... })
  })
})
```

### 10.3 Component Tests (complex components only)

**Tested:**
| Component | Key behaviors to test |
|-----------|----------------------|
| TransferForm | field validation, submit, loading state, error display |
| StockItemForm | quantity validation, location select, submit |
| CountryForm | code validation (ISO format), submit |
| StateForm | country association, submit |
| OrderForm | status badges, lifecycle action button state, read-only vs edit mode |
| FulfillmentWorkflow | step highlighting, current action button, confirm dialog, error state |
| PaymentMethodForm | activate/deactivate toggle, submit |
| AddressForm | country-state cascading, submit |
| ShippingMethodForm | activate/deactivate, submit |
| ShippingRateForm | currency input, weight range, submit |
| UserForm | role assignment, permission assignment, status toggle, submit |
| RoleForm | permission selection, submit |
| RolePermissionManager | assign/revoke flow, search, submit |
| VariantImageManager | file select, upload, delete confirm, reorder, primary toggle |

**Not tested (simple display-only):**
StockItemListTable, StockLocationListTable, StockMovementListTable, StockReservationListTable,
StockTransferListTable, PaymentListTable, PaymentMethodListTable, CountryListTable,
StateListTable, ShippingMethodListTable, ShippingRateListTable, OrderListTable,
UserListTable, RoleListTable, PermissionListTable, all DashboardPages.

### 10.4 Test Infrastructure

**Test setup file (`app/Admin/src/test-setup.ts`):**
```ts
import { config } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { createI18n } from 'vue-i18n'

// Mock PrimeVue toast service globally
config.global.mocks = {
  $toast: { add: vi.fn() },
  $confirm: { require: vi.fn() },
  $t: (key: string) => key, // i18n passthrough for tests
}

// Suppress Vue warnings about missing PrimeVue config in tests
config.global.stubs = {
  Toast: true,
  ConfirmDialog: true,
  Button: { template: '<button><slot /></button>' },
  DataTable: { template: '<div><slot /></div>', props: ['rows', 'loading'] },
}
```

**Vitest config (in `vite.config.ts` or `vitest.config.ts`):**
```ts
test: {
  environment: 'jsdom',
  globals: true,
  setupFiles: ['./src/test-setup.ts'],
}
```

**Running tests:**
```bash
pnpm run test:unit         # All unit tests
pnpm run test:unit -- Xxx  # Filter by name
```

### 10.5 Coverage Targets

| Category | Target | Measurement |
|----------|--------|-------------|
| Store line coverage | 100% | All store methods have success + error paths tested |
| API line coverage | 100% | All API service methods tested |
| Component behavior coverage | 85% | All form/manager/workflow components have core behavior tested |
| Overall line coverage | No numeric target | Measurable improvement over current ~35% baseline |

## 11. Acceptance Criteria

### Design Consistency

- **AC-DSN-001**: All 52 pages use ListLayout or DetailLayout as root wrapper
- **AC-DSN-002**: No files contain `class="card"` (PrimeFlex) or inline `rounded-border border p-5` divs
- **AC-DSN-003**: No files contain PrimeFlex grid classes (`class="grid"` without `grid-cols-*`, `col-N`)
- **AC-DSN-004**: No files contain raw `<button class="p-button">` — all use `<Button>` component
- **AC-DSN-005**: No hardcoded English strings in any `.vue` file's template section; all strings are `t('...')` or `{{ t('...') }}`
- **AC-DSN-006**: All PageHeader instances include `subtitle` prop with a valid i18n key
- **AC-DSN-007**: The reports DashboardPage uses shared DataTable, not raw HTML `<table>`
- **AC-DSN-008**: FormActions uses `-mx-5` (not `-mx-6`) negative margin
- **AC-DSN-009**: DetailDrawer.vue and FilterPanel.vue (old) are deleted from the codebase
- **AC-DSN-010**: `vue-tsc --noEmit` passes with zero errors

### Missing Features

- **AC-FEA-001**: A user can drag-and-drop image files onto a variant detail page, see upload progress, reorder thumbnails via drag, set a primary image, and delete images with confirmation
- **AC-FEA-002**: On the order detail page, the fulfillment Steps component shows the current order status with correct highlighting; action buttons are contextual to the current status; transitions call the correct API endpoint
- **AC-FEA-003**: Typing in the TableToolbar search input triggers a 300ms-debounced API re-fetch; search results update without full page reload
- **AC-FEA-004**: The FilterPanel allows selecting column filters (text, select, date range, number range); applying filters shows removable chips below the toolbar; clearing filters restores the unfiltered list
- **AC-FEA-005**: The topbar shows a bell icon with unread count badge; clicking opens a popover with recent notifications; "Mark all read" clears the badge
- **AC-FEA-006**: Notification polling runs every 30 seconds while the user is on any admin page; stops on navigation away

### Testing

- **AC-TST-001**: All 17 stores across 7 modules have passing test files with coverage of `fetchMany`, `getById`, `create`, `update`, `delete`, and module-specific actions
- **AC-TST-002**: All Store tests verify both success paths (API returns Result.isSuccess=true) and error paths (API returns Result.isSuccess=false)
- **AC-TST-003**: All complex component tests (forms, managers, workflows) verify: loading state, error display, successful submit flow
- **AC-TST-004**: All API test files verify correct HTTP method, URL path, and query parameter serialization for each endpoint
- **AC-TST-005**: Running `pnpm run test:unit` from `app/Admin/` results in zero failing tests and zero unhandled errors

## 12. Rationale & Context

### Why ListLayout and DetailLayout instead of a composable?

A generic `useListPage()` composable would need escape hatches for every special case:
- Catalog ProductListPage with variant count badges
- Ordering OrderListPage with status tag colors
- Inventory TransferListPage with source/destination location columns

A slot-based layout component gives each page full control over its unique content
while enforcing consistent chrome (header, toolbar, spacing, state handling). The
alternative — a `useListPage` composable returning reactive state — would require
every page to re-implement the wrapper `div` structure, which is the inconsistency
we're trying to fix.

### Why delete DetailDrawer and FilterPanel?

`DetailDrawer.vue` and `FilterPanel.vue` are empty stubs that were created optimistically
but never implemented. They pollute the component registry and confuse `unplugin-vue-components`.
Deleting them reduces cognitive load. FilterPanel is reimplemented from scratch as part of
Workstream B with a proper spec.

### Why Tailwind-only grid?

The codebase currently mixes Tailwind grid utilities (`grid grid-cols-2`) with PrimeFlex
grid classes (`class="grid"`, `col-6`). PrimeFlex's `grid` class uses a 12-column system
that conflicts with Tailwind's approach. Having two grid systems:
1. Makes spacing impossible to standardize
2. Forces developers to know both systems
3. Causes visual drift when one system's gap values differ from the other

Tailwind is chosen because it's the project's utility CSS framework (Tailwind v4 + tailwindcss-primeui),
and PrimeVue 5 is designed to be used with Tailwind via `tailwindcss-primeui`.

### Why test only complex components?

Simple list table components have zero unique logic — they pass props to a DataTable
with hardcoded columns. Testing them would mean testing PrimeVue's DataTable (which
is already tested by PrimeTek). Complex components (forms with validation, managers
with assign/revoke flows, workflows with state machines) have unique behavior that
warrants test coverage.

### Why client-only notification fallback?

There's no verified `/notifications` API endpoint. Building the full UI and store
with a client-only fallback means:
1. The UI is ready when a backend integration is available
2. No dead code if notifications never get a backend
3. The `startPolling()`/`fetch()` pattern is testable even in fallback mode

## 13. Examples & Edge Cases

### Example: ListLayout usage (ProductListPage)

```vue
<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('catalog.products.title')"
        :subtitle="t('catalog.products.subtitle')"
        :icon="route.meta?.icon as string"
      />
    </template>

    <template #toolbar>
      <TableToolbar
        v-model:query="store.query"
        :filters="store.filters"
        :search-placeholder="t('catalog.products.searchPlaceholder')"
        :filter-definitions="productFilterDefs"
        @create="router.push({ name: ROUTE.PRODUCTS.CREATE })"
        @update:filters="onFiltersChanged"
      />
    </template>

    <template #default>
      <LoadingSkeleton v-if="store.loading && !store.items.length" :rows="10" :columns="6" />
      <ErrorState v-else-if="store.error" :description="store.error" @retry="store.fetchMany()" />
      <EmptyState
        v-else-if="!store.items.length"
        :title="t('catalog.products.empty.title')"
        :description="t('catalog.products.empty.description')"
      />
      <DataTable
        v-else
        :rows="[...store.items]"
        :loading="store.loading"
        :total-records="store.totalRecords"
        :page-size="store.pageSize"
        :first="store.page * store.pageSize"
        @page="onPage"
      >
        <Column field="name" header="Name" sortable />
        <Column field="sku" header="SKU" sortable />
        <Column field="status" header="Status">
          <template #body="{ data }">
            <StatusTag :status="data.status" />
          </template>
        </Column>
        <Column :header="t('common.actions')">
          <template #rowActions="{ data }">
            <ActionMenu
              @view="router.push({ name: ROUTE.PRODUCTS.VIEW, params: { id: data.id } })"
              @edit="router.push({ name: ROUTE.PRODUCTS.EDIT, params: { id: data.id } })"
              @delete="onDelete(data.id)"
            />
          </template>
        </Column>
      </DataTable>
    </template>
  </ListLayout>
</template>
```

### Example: DetailLayout usage (CountryDetailPage)

```vue
<template>
  <DetailLayout :saving="saving">
    <template #header>
      <PageHeader
        :title="pageTitle"
        :subtitle="t('location.countries.subtitle')"
        :icon="route.meta?.icon as string"
      >
        <template #back>
          <Button
            :label="t('common.back')"
            icon="pi pi-arrow-left"
            text
            @click="router.push({ name: ROUTE.COUNTRIES.LIST })"
          />
        </template>
      </PageHeader>
    </template>

    <template #actions v-if="mode === 'view'">
      <Button
        :label="t('common.edit')"
        icon="pi pi-pencil"
        @click="router.push({ name: ROUTE.COUNTRIES.EDIT, params: { id } })"
      />
    </template>

    <template #default>
      <LoadingSkeleton v-if="loading" :rows="6" :columns="2" />
      <ErrorState v-else-if="error" :description="error" @retry="loadCountry" />
      <div v-else class="grid grid-cols-12 gap-4">
        <FormField :label="t('location.countries.fields.name')" class="col-span-full sm:col-span-6">
          <InputText v-model="form.name" :disabled="mode === 'view'" />
        </FormField>
        <FormField :label="t('location.countries.fields.isoCode')" class="col-span-full sm:col-span-3" required>
          <InputText v-model="form.isoCode" :disabled="mode === 'view'" maxlength="2" />
        </FormField>
        <FormField :label="t('location.countries.fields.currency')" class="col-span-full sm:col-span-3">
          <InputText v-model="form.currency" :disabled="mode === 'view'" maxlength="3" />
        </FormField>
      </div>
    </template>

    <template #footer v-if="mode !== 'view'">
      <FormActions :saving="saving" @save="onSubmit" @cancel="onCancel" />
    </template>
  </DetailLayout>
</template>
```

### Edge Case: Empty state after search filter

When a user searches for "xyz" and no results match:
- DataTable is hidden (v-else condition triggers EmptyState)
- The search input still shows "xyz"
- The EmptyState title shows: "No products match your search"
- The EmptyState description shows: "Try adjusting your search terms or filters"
- If filters are also active, the description includes: "...or clear your active filters"
- A "Clear filters" button appears alongside the EmptyState content

### Edge Case: Concurrent save prevention

When the user clicks "Save" rapidly:
1. First click: `saving.value = true`, disables button
2. API call in progress
3. Second click: button is disabled, `v-bind:disabled="saving"` prevents double-submit
4. API returns: `saving.value = false`, button re-enables
5. If API returns error: toast shown, button re-enabled for retry

### Edge Case: Race condition on pagination

When the user clicks "Next page" page=2, then immediately "Next page" page=3:
1. Request for page=2 fires
2. Request for page=3 fires
3. Page=3 response arrives first — items updated to page 3 data
4. Page=2 response arrives — items overwritten with page 2 data
5. User sees page 2 data but paginator shows page=3

**Fix:** Store SHALL ignore responses for stale pages:
```ts
const currentPage = ref(0)
async function fetchMany(params: { page: number }) {
  const requestedPage = params.page
  currentPage.value = requestedPage
  const result = await api.getMany(params)
  if (currentPage.value !== requestedPage) return // Stale response
  // ... update items
}
```

## 14. Validation Criteria

- **VAL-001**: `bash scripts/check-feature-conventions.sh` passes (no missing feature files)
- **VAL-002**: `bash scripts/check-cross-module-refs.sh` passes (no cross-module imports)
- **VAL-003**: `npx vue-tsc --noEmit` in `app/Admin/` exits with code 0
- **VAL-004**: `npx eslint --max-warnings=0` in `app/Admin/` exits with code 0
- **VAL-005**: `pnpm run test:unit` in `app/Admin/` exits with code 0 (zero failing tests)
- **VAL-006**: `pnpm run test:unit -- --coverage` shows measurable improvement in line coverage
- **VAL-007**: All 10 inconsistency categories from the design audit (July 2026) are resolved
- **VAL-008**: All 4 missing feature acceptance criteria (AC-FEA-001 through AC-FEA-006) pass manual verification

## 15. Dependencies & External Integrations

### Platform Runtime
- **PLT-001**: Vue 3.5+ with TypeScript 6.0+ — SFC composition API
- **PLT-002**: Vite 8.0+ — dev server and build toolchain
- **PLT-003**: Node.js 22+ — runtime for dev server, tests, and build

### Component Libraries
- **LIB-001**: PrimeVue 5.0+ — UI component library (Button, DataTable, Steps, Popover, FileUpload, etc.)
- **LIB-002**: @primevue/themes 4.5+ with Aura preset — theming engine
- **LIB-003**: Tailwind CSS 4.3+ with tailwindcss-primeui plugin — utility CSS

### State & Routing
- **LIB-004**: Pinia 3.0+ — state management
- **LIB-005**: Vue Router 5.1+ — client-side routing

### Forms & Validation
- **LIB-006**: vee-validate 4.15+ with @vee-validate/zod — form state management
- **LIB-007**: Zod 3.25+ — schema validation

### Charts (Reports module)
- **LIB-008**: chart.js 4.5+ — chart rendering

### Internationalization
- **LIB-009**: vue-i18n 11.4+ — translation framework

### HTTP Client
- **LIB-010**: Axios 1.18+ — HTTP requests (via shared `apiClient`)

### Testing
- **LIB-011**: Vitest 4.1+ — test runner
- **LIB-012**: @vue/test-utils 2.4+ — Vue component testing
- **LIB-013**: @pinia/testing — Pinia store testing utilities
- **LIB-014**: jsdom 29.1+ — test DOM environment

### Backend Services
- **SVC-001**: Catalog API (`/api/catalog/*`) — 62 endpoints (products, variants, images, prices, option types, taxonomies, taxons)
- **SVC-002**: Inventory API (`/api/inventory/*`) — 28 endpoints (stock items, locations, movements, transfers, reservations)
- **SVC-003**: Ordering API (`/api/ordering/*`) — 19 endpoints (orders, fulfillment, line items)
- **SVC-004**: Payment API (`/api/payment/*`) — 12 endpoints (payments, capture, void, refund, payment methods)
- **SVC-005**: Shipping API (`/api/shipping/*`) — 12 endpoints (shipping methods, rates)
- **SVC-006**: Location API (`/api/locations/*`) — 12 endpoints (countries, states)
- **SVC-007**: Identity API (`/api/identity/*`) — 24 endpoints (users, roles, permissions, user-role, role-permission)
- **SVC-008**: Profile API (`/api/profiles/*`) — 10 endpoints (profile, addresses)
- **SVC-009**: Notification API (`/api/notifications/*`) — 0 endpoints currently (future; use client-only fallback)

### Development Tools
- **TOOL-001**: vue-tsc 3.3+ — TypeScript type checking for .vue files
- **TOOL-002**: eslint 10.5+ — JavaScript/TypeScript linting
- **TOOL-003**: oxlint 1.69+ — fast Rust-based linter
- **TOOL-004**: unplugin-vue-components 32.1+ — auto-import PrimeVue components
- **TOOL-005**: unplugin-auto-import 21.0+ — auto-import Vue APIs

## 16. Related Specifications

- `spec/design-admin-spa-list-detail-pattern.md` — List + Detail page pattern (already implemented)
- `spec/design-admin-spa-auth-module.md` — Auth feature module (already implemented)
- `docs/superpowers/specs/2026-07-25-admin-app-completion-design.md` — Initial brainstorming design doc
- `docs/codebase/ARCHITECTURE.md` — Overall system architecture
- `docs/codebase/CONVENTIONS.md` — Coding conventions
- `docs/codebase/TESTING.md` — Testing strategy overview
- `.harness/domains.yml` — Domain boundaries and quality scores
- `.harness/principles.yml` — Golden principles with enforcement
- `.harness/enforcement.yml` — Naming, file limits, import rules
