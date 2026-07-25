# Admin App Completion — Design Spec

**Date:** 2026-07-25
**Branch:** `feature/implement-admin-panel`
**Status:** Approved

## Scope

1. **Polish existing CRUD pages** to consistent Sakai/PrimeVue design standards (audit + fix hybrid approach)
2. **Write tests** — 100% store + composable coverage for 8 untested modules; complex component tests
3. **Implement 4 missing features:** variant image upload, order fulfillment workflow, advanced search/filters, notification system
4. **Out of scope:** Live dashboard data (static charts only)

---

## Architecture

### 3 new shared layout components

| Component | Purpose | Replaces |
|---|---|---|
| `AppCard` | Unified card with consistent padding, border-radius, bg-color, dark-mode support | `class="card"` (PrimeFlex), inline `rounded-border border p-5` divs |
| `ListLayout` | Standard list page: PageHeader, toolbar slot, DataTable with built-in loading/error/empty state orchestration, pagination sync | Ad-hoc list page structures |
| `DetailLayout` | Standard detail/form page: PageHeader, card body with grid-based form slots, sticky FormActions footer | Inconsistent detail page structures |

### Components to fix

- `FormActions.vue` — fix `-mx-6` negative margin to match card padding (use Tailwind `p-5` standard)
- `PageHeader.vue` — add mandatory `subtitle` prop (i18n key), enforce icon presence
- `TableToolbar.vue` — add debounced search input, filter button, active filter chips

### Components to delete

- `DetailDrawer.vue`, `FilterPanel.vue` — unused, remove. FilterPanel will be reimplemented as part of the search/filter feature.

### Design rules

- **Grid system:** Tailwind CSS only (`grid grid-cols-* gap-*`). No PrimeFlex grid.
- **Buttons:** Always `<Button>` component. No raw `<button class="p-button">`.
- **Cards:** Always `<AppCard>`. No inline card divs.
- **i18n:** All user-facing strings via `t()`. No hardcoded English.
- **Standard card padding:** `p-5` (20px) — uniform across AppCard, DataTable wrapper, FormActions.

---

## Page Structure Per Module

### ListLayout slots
```
<header>   — PageHeader (title, subtitle, icon, actions slot for Create button)
<toolbar>  — TableToolbar (search, filters, bulk actions, column toggle)
<content>  — DataTable (with v-if state guards for loading/error/empty), ActionMenu per row
<empty>    — EmptyState
<error>    — ErrorState
```

### DetailLayout slots
```
<header>   — PageHeader (back-button, title, subtitle, icon, actions slot for lifecycle buttons)
<body>     — AppCard grid of FormField components
<footer>   — FormActions (save/cancel, sticky)
```

### Per-module page inventory

| Module | List Pages | Detail Pages | Dashboard | Components |
|---|---|---|---|---|
| Catalog | ProductList, VariantList, OptionTypeList, TaxonomyList | ProductDetail, VariantDetail, OptionTypeDetail, TaxonomyDetail | DashboardPage | ProductForm, VariantForm, OptionTypeForm, TaxonomyForm, TaxonForm, OptionValueForm, ProductClassificationManager, VariantImageManager (NEW) |
| Inventory | StockList, LocationList, TransferList, ReservationList, MovementList | LocationDetail, StockItemDetail, TransferDetail | DashboardPage | StockLocationForm, StockItemForm, TransferForm, StockReservationListTable, StockMovementListTable |
| Ordering | OrderList, FulfillmentQueue | OrderDetail | DashboardPage | OrderForm, FulfillmentWorkflow (NEW) |
| Users | StaffList, RoleList, PermissionList, CustomerList | StaffDetail, RoleDetail, PermissionDetail, CustomerDetail | — | UserForm, RoleForm, RolePermissionManager, UserRoleManager |
| Payment | PaymentList, PaymentMethodList | PaymentDetail, PaymentMethodDetail | — | PaymentMethodForm |
| Shipping | ShippingMethodList, ShippingRateList | ShippingMethodDetail, ShippingRateDetail | — | ShippingMethodForm, ShippingRateForm |
| Location | CountryList, StateList | CountryDetail, StateDetail | — | CountryForm, StateForm |
| Profile | AddressList | ProfilePage, AddressDetail | — | AddressForm |
| Reports | — | — | DashboardPage | — |
| Auth | Login, Register, ForgotPassword, ResetPassword, ChangePassword | — | — | PasswordStrength |

---

## Four Missing Features

### 1. Variant Image Upload + Management

**New component:** `VariantImageManager.vue` (catalog/components)
**Added to:** VariantDetailPage

- Drag-and-drop upload zone (PrimeVue FileUpload or custom drop zone)
- Image thumbnail grid with drag-reorder
- Set primary image (star badge), delete per image
- Backend APIs: `POST /catalog/variants/{id}/images`, `DELETE .../images/{imageId}`, `PUT .../images/reorder`

### 2. Order Fulfillment Workflow

**New component:** `FulfillmentWorkflow.vue` (ordering/components)
**Added to:** OrderDetailPage

- PrimeVue Steps component: Pending → Confirmed → Processing → Picked → Packed → Shipped → Delivered
- Escape hatches: Cancel, Return
- Per-step action panel with status transitions
- FulfillmentQueuePage: add bulk action toolbar (Batch Pick, Batch Pack)
- Backend APIs: `POST /ordering/orders/{id}/approve|complete|cancel|resume`

### 3. Advanced Search + Filters

**Enhance:** `TableToolbar.vue`, all list pages, all list stores

- Debounced search input wired to store `query` ref
- FilterPanel per-entity column filter config (text, select, date range, number range)
- Active filter chips below toolbar
- Each list store adds `query`, `filters` refs synced with API call params

### 4. Notification System

**New:** Topbar bell icon + notification center dropdown

- `useNotification` composable: `fetch()`, `markRead(id)`, `markAllRead()`, auto-poll 30s
- Notification store: `unreadCount`, `items`
- Types: order status, payment status, stock alerts, system messages
- Client-side only (no backend dependency initially)

---

## Testing Strategy

### Coverage targets

| Module | Store tests | Component tests |
|---|---|---|
| Inventory | 5 stores | TransferForm, StockItemForm |
| Location | 2 stores | CountryForm, StateForm |
| Ordering | 1 store | OrderForm, FulfillmentWorkflow |
| Payment | 2 stores | PaymentMethodForm |
| Profile | 2 stores | AddressForm |
| Reports | 0 stores | DashboardPage (widgets) |
| Shipping | 2 stores | ShippingMethodForm, ShippingRateForm |
| Users | 3 stores | UserForm, RoleForm, RolePermissionManager |

### Store test pattern

Each store method tested for: success path, loading state, error handling, pagination (for getMany), toast notification on mutations.

### API test pattern

Each API module tested for: correct HTTP method, correct URL path, query parameter serialization, request body mapping, result shape, error response handling.

### Component test pattern

Only components with behavior: form validation, error display, loading state, assign/revoke flows, step transitions, drag-reorder, search/filter.

### Excluded

Simple list tables, display-only pages, static dashboards.

### Existing coverage

- Shared layer: 28 spec files (complete)
- Auth: 5 spec files (complete)
- Catalog: 15 spec files (most coverage, gaps in TaxonForm/OptionValueForm/VariantListPage)
- 7 remaining modules: 0 spec files (all new)

---

## Inconsistency Fixes (from audit)

| # | Issue | Fix |
|---|---|---|
| 1 | Three card patterns | AppCard for all cards |
| 2 | Two grid systems | Tailwind only, remove PrimeFlex grid |
| 3 | i18n gaps (3 pages + empty states) | Add i18n keys, use t() everywhere |
| 4 | Header subtitle inconsistency | Mandatory subtitle prop on PageHeader |
| 5 | FormActions -mx-6 vs p-4 card | Standardize to p-5, fix FormActions |
| 6 | Dashboard raw table | Replace with shared DataTable |
| 7 | Raw button classes | Replace with Button component |
| 8 | Spacing mb-5/4/6 divergence | Standardize: header→content = mb-6, toolbar→table = mb-4 |
| 9 | State handling location split | ListLayout/DetailLayout handles all states |
| 10 | Dashboard missing header icon | Add `:icon="route.meta?.icon"` |
