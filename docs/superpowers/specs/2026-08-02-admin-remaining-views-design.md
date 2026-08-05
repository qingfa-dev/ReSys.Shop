# Admin Remaining Views — Design Spec

## Objective

Replace the 27 placeholder views across 7 modules (Dashboard, Identity, Inventory, Ordering, Payment, Profile, Shipping) with real CRUD UIs. All routes, menu items, and the complete data layer (types, validations, API services, Pinia stores, composables, unit tests) already exist per the completed `feature-admin-data-layer-1` plan. This spec covers only the Vue template/layout layer — wiring existing stores and services into functional views following the proven Catalog/Location pattern.

Additionally: remove 3 unused feature directories (`reports/`, `users/`, `error/pages/`). Leave the auth guard disabled.

## Approach

Module-by-module, building all views for one module before moving to the next. Each module's list + detail views share stores and types, so building them together ensures immediate integration validation. Order: Dashboard → Identity → Inventory → Ordering → Payment → Profile → Shipping.

## Established Patterns (from Catalog/Location)

All views follow these conventions already proven by the working Catalog and Location modules:

**List view pattern:**
- Full-height flex layout (`flex flex-col h-full`)
- `Toolbar` header with search `InputText`, optional filter dropdowns, and a "Create New" `Button`
- `DataTable` bound to `usePagedQuery` composable (pagination, sorting, filtering from store)
- Export button using `useDataTableExport`
- Row actions: edit (navigates to `/:id` route), delete (`ConfirmDialog`), toggle for boolean columns
- Skeleton loading state

**Detail view pattern:**
- `Toolbar` header with "Back to list" link
- `Tabs` component for multi-section layouts (single-form views skip tabs)
- `<Form>` from `@primevue/forms` with Zod resolver matching the existing validation schemas
- Form fields use `InputText`, `InputNumber`, `Textarea`, `Select`, `ToggleSwitch`, `DatePicker` from PrimeVue
- Footer `Toolbar` with Save + Cancel buttons
- Navigation: `vue-router` `useRouter()` for redirects after create/update/delete

**State management:**
- List data via `usePagedQuery` composable → store's `fetchActive({...query})` method
- Detail data via store's `fetchById(id)` + `active` getter
- Mutations via `create(request)`, `update(id, request)`, `remove(id)` on API service

---

## Module Designs

### 1. Dashboard

**DashboardPage.vue** — replaces the placeholder card.

Four stat cards in a responsive grid (`grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4`):

| Card | Metric | Icon | Link |
|------|--------|------|------|
| Total Products | productCount from store | `pi-box` | → `/catalog/products` |
| Orders Today | ordersToday from store | `pi-shopping-cart` | → `/ordering/orders` |
| Registered Users | registeredUsers from store | `pi-users` | → `/identity/users` |
| Low Stock Items | lowStockCount from store | `pi-exclamation-triangle` | → `/inventory/stock-items` |

Each card uses `Card` with a colored top border, the icon in the header, the metric value in large text, and a subtle "View all →" link. Cards pull data from `dashboardStore.fetchDashboard()` on mount.

### 2. Identity

#### UsersList.vue
Standard list view. Columns: Email, Name, Phone, Email Confirmed (badge), Lockout End, Actions (Edit, Delete). Filter: `IsActive` toggle, search across email/name/phone. Create button → `/identity/users/new`. Delete confirmation dialog.

#### UserDetail.vue
Two tabs:

**Tab 1 — Profile:** Form fields: Email (`InputText`, disabled on edit), User Name, First Name, Last Name, Phone Number, Email Confirmed (`ToggleSwitch`), Is Active (`ToggleSwitch`), Lockout End (`DatePicker`, nullable). Form resolves with `userFormSchema` Zod schema from validations.

**Tab 2 — Roles:** Table of all available roles with checkboxes. Loads `roleStore.fetchAll()` and `userStore.fetchUserRoles(userId)` on mount. Toggling a checkbox calls `userStore.assignRole`/`unassignRole`.

Create mode: `/identity/users/new` — same form, no roles tab (roles assigned after creation). Edit mode: `/identity/users/:id` — both tabs available.

#### RolesList.vue
Standard list view. Columns: Name, Actions (Edit, Delete). Search by name. Create button.

#### RoleDetail.vue
Two tabs:

**Tab 1 — Profile:** Name (`InputText`), Description (`Textarea`).

**Tab 2 — Permissions:** Table of all system permissions grouped by category, each with a `ToggleSwitch`. Loads `permissionStore.fetchAll()` and `roleStore.fetchRolePermissions(roleId)` on mount.

#### PermissionsList.vue
Read-only list view. Columns: Name, Category, Description. Search bar. No create/edit/delete actions.

### 3. Inventory

#### StockItemsList.vue
Standard list view. Columns: Variant Name, SKU, Reorder Point, Reorder Quantity, Low Stock (badge), Actions (Edit, Delete). Filter by reorder point threshold. Create button.

#### StockItemDetail.vue
Two tabs:

**Tab 1 — Main Form:** Product Variant selector (dropdown from `variantStore.fetchAll()`), SKU, Reorder Point (`InputNumber`), Reorder Quantity (`InputNumber`).

**Tab 2 — Stock Levels:** Table showing `Location Name`, `Quantity On Hand`, `Reserved Quantity` for each location this item is stocked at. Loaded from `stockItemStore.fetchStockLevels(id)`. Read-only informational view.

#### StockLocationsList.vue
Standard list view. Columns: Name, Type, Is Active (badge), Address summary, Actions (Edit, Delete). Create button.

#### StockLocationDetail.vue
Two tabs:

**Tab 1 — Main Form:** Name, Location Type (`Select`: Warehouse/Store/Returns), Is Active (`ToggleSwitch`), Address fields (Street, City, State, Country, Postal Code).

**Tab 2 — Stock Items at Location:** Table showing `Variant Name`, `SKU`, `Quantity On Hand` for each item at this location. Read-only.

#### StockReservationsList.vue
Read-only list view (no detail route). Columns: Variant, Order #, Quantity, State (badge: Reserved/Confirmed/Released), Expires At, Created At. Filter by `State`. No create/edit/delete.

#### StockTransfersList.vue
Standard list view. Columns: ID, Source Location, Destination Location, Status (badge), Created At, Actions (Edit, Delete). Create button.

#### StockTransferDetail.vue
Two tabs:

**Tab 1 — Header:** Source Location (`Select`), Destination Location (`Select`), Status (`Select`: Draft/In Transit/Completed/Cancelled), Requested/Shipped/Received dates.

**Tab 2 — Line Items:** Editable table of items being transferred. Each row: Stock Item selector, Quantity. Add/remove rows. Total quantity summary.

#### StockMovementsList.vue
Read-only list view (no detail route). Columns: Stock Item, Originator Type, Quantity Change, Reason, Created At. Search bar. No CRUD actions.

### 4. Ordering

#### OrdersList.vue
Standard list view. Columns: Order #, Customer, Status (badge), Total, Created At, Actions (View). Filter by `Status` dropdown. No create or delete — orders are created by customers. Edit navigates to detail view.

#### OrderDetail.vue
Three tabs:

**Tab 1 — Overview:** Order Number (read-only), Customer name (read-only), Status (`Select` with transitions: Pending → Confirmed → Processing → Shipped → Delivered, or Cancelled), Subtotal, Tax, Shipping Cost, Total (all read-only), Created/Modified dates. Status transition dropdown with confirmation dialog.

**Tab 2 — Items:** Read-only table of order line items. Columns: Product Name, Variant, SKU, Unit Price, Quantity, Line Total.

**Tab 3 — Payments:** Table of payments associated with this order. Columns: Payment ID, Method, Amount, Status, Created At. Read-only.

### 5. Payment

#### PaymentsList.vue
Read-only list view (no detail route). Columns: Payment #, Order #, Method, Amount, Status (badge), Created At. Filter by `Status`. No CRUD actions.

#### PaymentMethodsList.vue
Standard list view. Columns: Display Name, Provider, Active (badge), Actions (Edit, Delete). Create button.

#### PaymentMethodDetail.vue
Single form tab. Fields: Name, Display Name, Provider Type (`Select`: CreditCard/PayPal/BankTransfer/Cash/Other), Is Active (`ToggleSwitch`), Configuration (optional `Textarea` for JSON config). No additional tabs.

### 6. Profile

#### ProfilesList.vue
Standard list view. Columns: Name, Phone, Email, Created At, Actions (View). Search by name/email/phone. No create — profiles are created via registration. Edit navigates to detail view.

#### ProfileDetail.vue
Two tabs:

**Tab 1 — Profile:** First Name, Last Name, Phone Number, Date of Birth (`DatePicker`), Gender (`Select`). Read-only user ID display.

**Tab 2 — Addresses:** Table of addresses belonging to this profile. Columns: Type, Street, City, State, Country, Postal Code, Default (badge), Actions (Edit, Delete). "Add Address" button opens inline form or navigates to address detail. Delete with confirmation.

#### AddressesList.vue
Standard list view. Columns: Profile Name, Type, Street, City, Country, Default (badge), Actions (Edit, Delete). Create button (requires selecting a profile).

#### AddressDetail.vue
Single form tab. Fields: Profile selector (dropdown, disabled on edit), Address Type (`Select`: Shipping/Billing/Both), Street, City, State, Country, Postal Code, Is Default (`ToggleSwitch`). No additional tabs.

### 7. Shipping

#### ShippingMethodsList.vue
Standard list view. Columns: Name, Carrier, Active (badge), Actions (Edit, Delete). Create button.

#### ShippingMethodDetail.vue
Single form tab. Fields: Name, Description (`Textarea`), Carrier (`Select`: FedEx/UPS/USPS/DHL/Custom), Is Active (`ToggleSwitch`), Configuration (optional `Textarea`). No additional tabs.

#### ShippingRatesList.vue
Standard list view. Columns: Name, Shipping Method, Price, Min/Max Weight, Condition, Actions (Edit, Delete). Create button.

#### ShippingRateDetail.vue
Single form tab. Fields: Shipping Method (`Select` from methods list), Name, Price (`InputNumber`), Min Weight (`InputNumber`, nullable), Max Weight (`InputNumber`, nullable), Condition Type (`Select`: Weight/Price/Flat). No additional tabs.

---

## Cleanup Tasks

Remove the following unused directories and their contents:
- `app/Admin/src/features/reports/` (empty barrel, no routes)
- `app/Admin/src/features/users/` (empty, duplicates identity/users)
- `app/Admin/src/features/error/pages/` (empty, 404 handled by ErrorPageShell in app/)

---

## Non-Goals

- Auth guard remains disabled (explicitly deferred)
- No chart libraries or complex visualization (dashboard is stat cards only)
- No backend changes — all APIs already exist
- No changes to existing Catalog, Location, or Auth views
- No refactoring of existing stores/services/types
