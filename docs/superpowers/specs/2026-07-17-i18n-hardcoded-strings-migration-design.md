# i18n Hardcoded Strings Migration — Design

Date: 2026-07-17
Status: DRAFT

## Problem

The Admin SPA has ~200+ hardcoded English strings across 40+ files:

- **showToast() calls** (72 instances) — summary and detail params as literals
- **Button labels** (38+ instances) — `label="Save Changes"` instead of `:label="t('key')"`
- **Column headers** (55+ instances) — `header="Name"` instead of `:header="t('key')"`
- **Template text** (40+ instances) — `<span>Account Details</span>` instead of `{{ t('key') }}`
- **Dialog/Panel headers** (dozens) — `header="Create Shipment"` instead of `:header="t('key')"`

Some locale files exist (`general.json`, `auth.json`, `catalog.json`, `users.json`, `inventory.json`, `ordering.json`), but many domains have no keys or missing sections.

## Migration Approach

### By-domain, incremental

One domain per commit. Each commit: create/extend locale file → migrate all patterns within that domain.

### Domain order

1. `general.json` — add missing `common.*` action keys
2. catalog — extend existing `catalog.json`
3. users/roles — extend `users.json` + create `roles.json`
4. location — create `location.json`
5. ordering — extend `ordering.json`
6. inventory — extend `inventory.json`
7. profile — create `profile.json`
8. error — create `error.json`
9. auth — extend `auth.json`

### New Locale Files

| File | Root key | Source |
|---|---|---|
| `location.json` | `location.*` | Country/State CRUD views + stores |
| `roles.json` | `roles.*` | Role list, form, permissions views |
| `profile.json` | `profile.*` | Profile view + store, auth Profile view |
| `error.json` | `error.*` | NotFound, ErrorPage, AccessDenied pages |

### Key Naming Convention

```
{domain}.titles.*         — page/section headings
{domain}.messages.*       — toast success/error strings
{domain}.labels.*         — form labels, field names
{domain}.placeholders.*   — input placeholders
{domain}.actions.*        — button labels
{domain}.table.*          — column headers
{domain}.descriptions.*   — descriptive/subtitle text
{domain}.confirm.*        — confirm dialog messages
{domain}.tabs.*           — tab labels
```

## Store i18n Import

Stores (`.ts` files) do NOT have `t` auto-imported. Each store that uses `t()` must import `useI18n` from `vue-i18n`:

```ts
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
```

Views (`.vue` files) have `t` auto-imported in `<script setup>` and templates — no extra import needed.

## Migration Patterns

### showToast

```ts
// Before
showToast('success', 'Created', 'Product created successfully');
// After
showToast('success', t('common.created'), t('catalog.products.messages.create_success'));
```

### Button labels

```vue
<!-- Before -->
<Button label="Cancel" severity="secondary" text @click="router.back()" />
<!-- After -->
<Button :label="t('common.cancel')" severity="secondary" text @click="router.back()" />
```

### Column headers

```vue
<!-- Before -->
<Column field="name" header="Name" sortable>
<!-- After -->
<Column field="name" :header="t('catalog.products.table.name')" sortable>
```

### Template text

```vue
<!-- Before -->
<span>Account Details</span>
<!-- After -->
<span>{{ t('profile.titles.details') }}</span>
```

### Dynamic / conditional toast details

Some toasts interpolate variables or use ternaries inside template literals:

```ts
// Case 1: ternary — branch on truthiness
`Variant ${selectedVariant.value ? 'updated' : 'created'} successfully`
// -> Use two separate locale keys with conditional:
selectedVariant.value ? t('catalog.products.variants.messages.update_success') : t('catalog.products.variants.messages.create_success')

// Case 2: parameterized — interpolate a count
`Created ${successCount} variants.`
// -> Define a key with param: t('catalog.products.variants.wizard.generated', { count: successCount })
// In locale: "generated": "Created {count} variants."
```

### Fallback removal

```ts
// Before
showToast('success', t('common.success') || 'Success', t('catalog.products.messages.delete_success') || 'Product removed.');
// After
showToast('success', t('common.success'), t('catalog.products.messages.delete_success'));
```

## Key Inventory

### `location.json`

```json
{
  "titles": {
    "countries": "Countries",
    "states": "States",
    "create_country": "New Country",
    "create_state": "New State",
    "edit_country": "Edit Country",
    "edit_state": "Edit State"
  },
  "labels": {
    "name": "Name",
    "abbreviation": "Abbreviation",
    "calling_code": "Calling Code",
    "country": "Country",
    "active": "Active"
  },
  "actions": {
    "new_country": "New Country",
    "new_state": "New State",
    "cancel": "Cancel",
    "delete": "Delete"
  },
  "messages": {
    "create_success": "Country created successfully",
    "update_success": "Country updated successfully",
    "delete_success": "Country removed successfully",
    "state_create_success": "State created successfully",
    "state_update_success": "State updated successfully",
    "state_delete_success": "State removed successfully",
    "load_error": "Failed to load countries",
    "states_load_error": "Failed to load states"
  }
}
```

### `roles.json`

```json
{
  "titles": {
    "list": "Roles",
    "create": "Create Role",
    "edit": "Edit Role",
    "permissions": "Permissions"
  },
  "labels": {
    "name": "Role Name",
    "description": "Description",
    "priority": "Priority",
    "users": "Users",
    "type": "Type"
  },
  "actions": {
    "create": "Create Role",
    "save": "Save Changes",
    "cancel": "Cancel",
    "delete": "Delete",
    "save_permissions": "Save Changes"
  },
  "messages": {
    "create_success": "Role created successfully",
    "update_success": "Role updated successfully",
    "delete_success": "Role deleted successfully",
    "permissions_updated": "Permissions updated successfully",
    "load_error": "Failed to load role details"
  },
  "table": {
    "name": "Role Name",
    "priority": "Priority",
    "users": "Users",
    "type": "Type",
    "actions": "Actions"
  },
  "picklist": {
    "available": "Available",
    "assigned": "Assigned"
  }
}
```

### `profile.json`

```json
{
  "titles": {
    "details": "Account Details",
    "password": "Change Password",
    "notifications": "Notifications"
  },
  "labels": {
    "full_name": "Full Name",
    "email": "Email Address",
    "email_short": "Email",
    "username": "Username",
    "phone": "Phone",
    "joined": "Joined On",
    "current_password": "Current Password",
    "new_password": "New Password",
    "confirm_password": "Confirm New Password"
  },
  "actions": {
    "save_profile": "Save Profile",
    "update_password": "Update Password"
  },
  "messages": {
    "update_success": "Profile updated successfully",
    "password_updated": "Password updated successfully",
    "password_mismatch": "New passwords do not match",
    "load_error": "Failed to load user profile",
    "update_error": "Failed to update password"
  },
  "notifications": {
    "title": "Notifications",
    "email": "Email Notifications",
    "email_desc": "Receive email updates about your account",
    "order_updates": "Order Updates",
    "order_updates_desc": "Get notified about order status changes",
    "marketing": "Marketing",
    "marketing_desc": "Receive promotional offers and news",
    "security": "Security Alerts",
    "security_desc": "Important security notifications about your account"
  }
}
```

### `error.json`

```json
{
  "not_found": {
    "title": "Page Not Found",
    "message": "The page you're looking for doesn't exist or has been moved.",
    "action": "Go to Dashboard"
  },
  "server_error": {
    "title": "Something Went Wrong",
    "message": "An unexpected error occurred. Please try again.",
    "action_retry": "Try Again",
    "action_home": "Go to Dashboard"
  },
  "access_denied": {
    "title": "Access Denied",
    "message": "You don't have permission to access this page.",
    "action": "Back to Dashboard"
  }
}
```

### Extensions to existing files

#### `general.json` — add under `common`:
```
created, updated, deleted, saved, removed, cancel
```

#### `users.json` — add sections:
- `users.security.*` — security panel headings (status_title, actions_title, lockout_end, failed_attempts, email_verified, phone_verified)
- `users.titles.customers` = "Customer Management"
- `users.labels.*` — remaining field labels
- `users.messages.status_updated`, `users.messages.customer_detail_error`
- `users.table.orders`, `users.table.total_spent`

#### `ordering.json` — add sections:
- `ordering.titles.*` — panel headers (audit_log, customer_communication, logistics, financials)
- `ordering.messages.*` — all toast messages (order_created, item_added, addresses_updated, state_advanced, order_canceled, shipment_created, refund_processed, etc.)
- `ordering.labels.currency`
- `ordering.table.*` — column header keys
- `ordering.panels.*` — panel header keys
- `ordering.tabs.*` — tab labels

#### `inventory.json` — add sections:
- `inventory.actions.*` — cancel, add_to_transfer, save, etc.
- `inventory.messages.*` — add missing toasts (item_added, transfer_shipped, stock_received, location_updated)
- `inventory.table.*` — missing column headers
- `inventory.titles.*` — panel section headings
- `inventory.labels.*` — missing labels (identification)

#### `catalog.json` — add keys:
- `catalog.products.images.messages.*` — upload, delete, update, failed
- `catalog.products.variants.messages.*` — saved, deleted, save_error, delete_error
- `catalog.products.variants.wizard.generated` — "Created {count} variants" (with param)
- `catalog.products.messages.classifications_saved`
- `catalog.products.tabs.options`, `catalog.products.tabs.specifications`
- `catalog.products.labels.searchable`
- `catalog.option_types.messages.delete_error`
- `catalog.property_types.messages.delete_error`

#### `auth.json` — add keys:
- `auth.labels.account_details`
- `auth.labels.current_password`, `new_password`, `confirm_password`
- `auth.actions.update_password`
- `auth.messages.password_mismatch`

## Files to Migrate (by domain)

### general (1 file)
- `shared/locales/messages/en/general.json` — add keys

### catalog (11 files)
- `shared/locales/messages/en/catalog.json` — extend
- `features/catalog/products/stores/product.store.ts`
- `features/catalog/products/views/ProductList.View.vue`
- `features/catalog/products/views/ProductForm.View.vue`
- `features/catalog/products/components/ProductClassificationManager.Component.vue`
- `features/catalog/products/components/ProductImageManager.Component.vue`
- `features/catalog/products/components/ProductVariantManager.Component.vue`
- `features/catalog/products/components/ProductPropertyManager.Component.vue`
- `features/catalog/products/components/ProductOptionTypeManager.Component.vue`
- `features/catalog/products/components/dialogs/VariantGenerationDialog.Component.vue`
- `features/catalog/products/components/images/ProductImageList.Component.vue`
- `features/catalog/option-types/views/OptionTypeList.View.vue`
- `features/catalog/option-types/views/OptionTypeForm.View.vue`
- `features/catalog/option-types/views/OptionTypeManager.View.vue`
- `features/catalog/option-types/option-values/views/OptionValueList.View.vue`
- `features/catalog/property-types/views/PropertyTypeList.View.vue`
- `features/catalog/property-types/views/PropertyTypeForm.View.vue`
- `features/catalog/taxonomies/stores/taxonomy.store.ts`
- `features/catalog/taxonomies/views/TaxonomyList.View.vue`
- `features/catalog/taxonomies/views/TaxonomyForm.View.vue`
- `features/catalog/taxonomies/views/TaxonomyManager.View.vue`
- `features/catalog/taxonomies/taxa/stores/taxon.store.ts`
- `features/catalog/taxonomies/taxa/views/TaxonList.View.vue`
- `features/catalog/taxonomies/taxa/views/TaxonForm.View.vue`
- `features/catalog/taxonomies/taxa/views/TaxonTreeManager.View.vue`
- `features/catalog/taxonomies/taxa/components/TaxonRulesManager.Component.vue`
- `features/catalog/taxonomies/taxa/components/TaxonProductsPreview.Component.vue`
- `features/catalog/dashboard/views/CatalogDashboard.View.vue`

### users/roles (3 files)
- `shared/locales/messages/en/users.json` — extend
- `features/users/views/StaffDetail.View.vue`
- `features/users/views/StaffForm.View.vue`
- `features/users/views/CustomerDetail.View.vue`
- `features/users/views/CustomerList.View.vue`
- `features/users/stores/user.store.ts`
- `features/users/components/UserSecurityManager.Component.vue`
- `features/users/components/UserRoleManager.Component.vue`
- `features/users/components/UserPermissionManager.Component.vue`
- `features/users/roles/views/RoleList.View.vue`
- `features/users/roles/views/RoleForm.View.vue`
- `features/users/roles/views/RolePermissionsManager.View.vue`
- `features/users/permissions/views/PermissionList.View.vue`
- plus `roles.json` (new)

### location (4 files)
- `location.json` (new)
- `features/location/views/CountryList.View.vue`
- `features/location/views/CountryForm.View.vue`
- `features/location/views/StateList.View.vue`
- `features/location/views/StateForm.View.vue`
- `features/location/stores/country.store.ts`
- `features/location/stores/state.store.ts`

### ordering (8 files)
- `shared/locales/messages/en/ordering.json` — extend
- `features/ordering/stores/order.store.ts`
- `features/ordering/views/OrderList.View.vue`
- `features/ordering/views/OrderDetail.View.vue`
- `features/ordering/views/OrderForm.View.vue`
- `features/ordering/components/ShipmentDialog.Component.vue`
- `features/ordering/components/RefundDialog.Component.vue`
- `features/ordering/components/ItemDialog.Component.vue`
- `features/ordering/components/AddressDialog.Component.vue`
- `features/ordering/fulfillment/stores/fulfillment.store.ts`
- `features/ordering/fulfillment/views/FulfillmentQueue.View.vue`
- `features/ordering/dashboard/views/OrderingDashboard.View.vue`

### inventory (6 files)
- `shared/locales/messages/en/inventory.json` — extend
- `features/inventories/views/StockLocationForm.View.vue`
- `features/inventories/views/StockLocationList.View.vue`
- `features/inventories/views/StockLocationManager.View.vue`
- `features/inventories/views/StockTransferForm.View.vue`
- `features/inventories/views/StockTransferDetail.View.vue`
- `features/inventories/components/StockAdjustmentDialog.Component.vue`
- `features/inventories/dashboard/views/InventoryDashboard.View.vue`

### profile (2 files)
- `profile.json` (new)
- `features/profile/views/Profile.View.vue`
- `features/profile/stores/profile.store.ts`

### auth (2 files)
- `shared/locales/messages/en/auth.json` — extend
- `features/auth/views/Profile.View.vue`

### error (2 files)
- `error.json` (new)
- `features/error/pages/NotFound.View.vue`
- `features/error/pages/ErrorPage.View.vue`
- `features/error/pages/AccessDenied.View.vue`

### dashboard (1 file)
- `features/dashboard/ui/DashboardPage.View.vue`
- `features/reports/views/Dashboard.View.vue`

## i18n.ts Registration

For each new locale file, add import + spread in `i18n.ts`:

```ts
import locationEn from "@/shared/locales/messages/en/location.json";
import rolesEn from "@/shared/locales/messages/en/roles.json";
import profileEn from "@/shared/locales/messages/en/profile.json";
import errorEn from "@/shared/locales/messages/en/error.json";

// In messages.en:
...locationEn,
...rolesEn,
...profileEn,
...errorEn,
```

## Verification

- `cd app/Admin && pnpm run lint` — no warnings on migrated files
- `cd app/Admin && pnpm run test:unit` — all tests pass
- `dotnet build` at repo root — no C# breakage (Vue-only change, but ensures no side effects)
- Manual smoke test: load the app, visit each migrated page, confirm no `$t()` resolution errors in console
