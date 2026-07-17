# i18n Hardcoded Strings Migration — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace all ~200+ hardcoded English strings across 40+ files with `t('locale.key')` calls, creating new locale files where missing.

**Architecture:** By-domain incremental migration. Each domain gets its own commit: create/extend the locale JSON → register in i18n.ts → migrate all `.vue` and `.ts` files in that domain. Use `useI18n` in Pinia stores (`.ts`) since `t` is only auto-imported in Vue SFCs. Three migration patterns: `showToast` calls, template text/labels, and component props (`label`, `header`).

**Tech Stack:** vue-i18n (composable API, non-legacy), Pinia stores, PrimeVue components

## Global Constraints

- Vue-i18n: use composition API (`useI18n()`) — not `this.$t`
- `t` is auto-imported in `.vue` files only. In `.ts` files (stores), import `useI18n` from `vue-i18n`.
- Remove all `|| 'Fallback'` dead code — fallback locale is `en`, keys always resolve.
- Register new locale files in `app/Admin/src/app/plugins/i18n.ts`
- Locale files live in `app/Admin/src/shared/locales/messages/en/`
- `pnpm run lint` must pass after each task
- `pnpm run test:unit` must pass after each task

---

### Task 0: general.json — missing common action keys

**Files:**
- Modify: `shared/locales/messages/en/general.json`
- Test: `pnpm run lint`

- [ ] **Step 1: Add common action keys to general.json**

Insert these under the `common` section alphabetically:

```json
    "cancel": "Cancel",
    "created": "Created",
    "deleted": "Deleted",
    "removed": "Removed",
    "saved": "Saved",
    "updated": "Updated"
```

`cancel` already exists — skip it. Add the others in alphabetical order within the `common` object.

- [ ] **Step 2: Lint check**

Run: `cd app/Admin && pnpm run lint`
Expected: No errors.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/locales/messages/en/general.json
git commit -m "feat(i18n): add missing common action keys to general.json"
```

---

### Task 1: Catalog — extend locale + migrate all catalog files

**Design reference:** See design doc `catalog.json` extension section for all keys to add.

**Files:**
- Modify: `shared/locales/messages/en/catalog.json`
- Modify: `features/catalog/products/stores/product.store.ts`
- Modify: `features/catalog/products/views/ProductList.View.vue`
- Modify: `features/catalog/products/views/ProductForm.View.vue`
- Modify: `features/catalog/products/components/ProductClassificationManager.Component.vue`
- Modify: `features/catalog/products/components/ProductImageManager.Component.vue`
- Modify: `features/catalog/products/components/ProductVariantManager.Component.vue`
- Modify: `features/catalog/products/components/ProductPropertyManager.Component.vue`
- Modify: `features/catalog/products/components/ProductOptionTypeManager.Component.vue`
- Modify: `features/catalog/products/components/dialogs/VariantGenerationDialog.Component.vue`
- Modify: `features/catalog/products/components/images/ProductImageList.Component.vue`
- Modify: `features/catalog/option-types/views/OptionTypeList.View.vue`
- Modify: `features/catalog/option-types/views/OptionTypeForm.View.vue`
- Modify: `features/catalog/option-types/views/OptionTypeManager.View.vue`
- Modify: `features/catalog/option-types/option-values/views/OptionValueList.View.vue`
- Modify: `features/catalog/property-types/views/PropertyTypeList.View.vue`
- Modify: `features/catalog/property-types/views/PropertyTypeForm.View.vue`
- Modify: `features/catalog/taxonomies/stores/taxonomy.store.ts`
- Modify: `features/catalog/taxonomies/views/TaxonomyList.View.vue`
- Modify: `features/catalog/taxonomies/views/TaxonomyForm.View.vue`
- Modify: `features/catalog/taxonomies/views/TaxonomyManager.View.vue`
- Modify: `features/catalog/taxonomies/taxa/stores/taxon.store.ts`
- Modify: `features/catalog/taxonomies/taxa/views/TaxonList.View.vue`
- Modify: `features/catalog/taxonomies/taxa/views/TaxonForm.View.vue`
- Modify: `features/catalog/taxonomies/taxa/views/TaxonTreeManager.View.vue`
- Modify: `features/catalog/taxonomies/taxa/components/TaxonRulesManager.Component.vue`
- Modify: `features/catalog/taxonomies/taxa/components/TaxonProductsPreview.Component.vue`
- Modify: `features/catalog/dashboard/views/CatalogDashboard.View.vue`

- [ ] **Step 1: Extend catalog.json**

Add these sections to `catalog.json`:

Under `catalog.products.images`, add after the existing `roles` block:

```json
    "messages": {
      "upload_failed": "Upload failed",
      "delete_success": "Image removed",
      "update_success": "Image details updated",
      "update_failed": "Failed to update image"
    }
```

Under `catalog.products.variants`, add after the existing `form` block:

```json
    "messages": {
      "create_success": "Variant created successfully",
      "update_success": "Variant updated successfully",
      "delete_success": "Variant removed",
      "save_failed": "Failed to save variant",
      "delete_failed": "Failed to delete variant",
      "generation_failed": "Failed to generate some variants."
    }
```

Inside `catalog.products.variants.wizard` add:

```json
    "generated": "Created {count} variants."
```

Under `catalog.products.messages` add after `loading`:

```json
    "classifications_saved": "Classifications saved"
```

Under `catalog.products.labels` add:

```json
    "searchable": "Searchable"
```

Under `catalog.products.tabs` add:

```json
    "options": "Options",
    "specifications": "Specifications"
```

Under `catalog.option_types.messages` add after `loading`:

```json
    "delete_error": "Failed to delete option type"
```

Under `catalog.property_types` (new section) add:

```json
  "property_types": {
    "messages": {
      "delete_error": "Failed to delete property type"
    }
  }
```

- [ ] **Step 2: Migrate product.store.ts**

Add `useI18n` import at top:
```ts
import { useI18n } from 'vue-i18n';
```

Add after `const { showToast } = useToast();`:
```ts
const { t } = useI18n();
```

Replace `showToast` calls:

```ts
// L57
showToast('success', t('common.created'), t('catalog.products.messages.create_success'));
// L71
showToast('success', t('common.updated'), t('catalog.products.messages.update_success'));
// L85
showToast('success', t('common.deleted'), t('catalog.products.messages.delete_success'));
// L115
showToast('success', t('common.updated'), t('catalog.products.messages.classifications_saved'));
```

- [ ] **Step 3: Migrate product .vue files**

For each `.vue` file in the catalog domain:

**Pattern A — showToast in `<script>`:** Replace hardcoded strings with `t()` calls. The `t` function is auto-imported in `.vue` files, no extra import needed.

**Pattern B — template text:** Replace `<span>Text</span>` with `<span>{{ t('catalog.products.tabs.options') }}</span>` and so on. `:label` and `:header` bindings for component props. `v-tooltip` for PrimeVue tooltips if any.

**Pattern C — fallback removal:** `t('key') || 'Fallback'` → `t('key')`

Files and their specific changes:

**`ProductForm.View.vue`:**
- L165: `<span>Options</span>` → `<span>{{ t('catalog.products.tabs.options') }}</span>`
- L183: `<span>Specifications</span>` → `<span>{{ t('catalog.products.tabs.specifications') }}</span>`
- L250: `<span>Searchable</span>` → `<span>{{ t('catalog.products.labels.searchable') }}</span>`

**`ProductImageManager.Component.vue`:**
- L44: `showToast('error', 'Error', 'Upload failed')` → `showToast('error', t('common.error'), t('catalog.products.images.messages.upload_failed'))`
- L51: `showToast('success', 'Deleted', 'Image removed')` → `showToast('success', t('common.deleted'), t('catalog.products.images.messages.delete_success'))`
- L63: `showToast('success', 'Updated', 'Image details updated')` → `showToast('success', t('common.updated'), t('catalog.products.images.messages.update_success'))`
- L69: `showToast('error', 'Error', 'Failed to update image')` → `showToast('error', t('common.error'), t('catalog.products.images.messages.update_failed'))`

**`ProductVariantManager.Component.vue`:**
- L72: `showToast('success', 'Success', \`Variant ${selectedVariant.value ? 'updated' : 'created'} successfully\`)` → `showToast('success', t('common.success'), selectedVariant.value ? t('catalog.products.variants.messages.update_success') : t('catalog.products.variants.messages.create_success'))`
- L77: `showToast('error', 'Error', 'Failed to save variant')` → `showToast('error', t('common.error'), t('catalog.products.variants.messages.save_failed'))`
- L91: `showToast('success', 'Deleted', 'Variant removed')` → `showToast('success', t('common.deleted'), t('catalog.products.variants.messages.delete_success'))`
- L95: `showToast('error', 'Error', 'Failed to delete variant')` → `showToast('error', t('common.error'), t('catalog.products.variants.messages.delete_failed'))`

**`ProductClassificationManager.Component.vue`:**
- L75: `showToast('success', 'Updated', 'Classifications synchronized')` → `showToast('success', t('common.updated'), t('catalog.products.messages.classifications_saved'))`
- L88: `showToast('success', 'Updated', 'Main category updated')` → `showToast('success', t('common.updated'), 'Main category updated')` (no locale key — add to catalog.json or leave as-is if low traffic)
- L101: `<h3>Categorization</h3>` → `<h3>{{ t('catalog.products.titles.classifications') }}</h3>` (key exists)

**`ProductPropertyManager.Component.vue`:**
- L64: `showToast('success', 'Added', 'Property assigned to product')` → `showToast('success', t('common.saved'), 'Property assigned to product')` (needs locale key — add `catalog.products.messages.property_assigned`)
- L85: `showToast('success', 'Removed', 'Property removed')` → `showToast('success', t('common.removed'), 'Property removed')` (needs locale key — add `catalog.products.messages.property_removed`)
- L119: `<label>Value</label>` → `<label>{{ t('catalog.products.labels.value') }}</label>` (needs key — add `"value": "Value"` under `catalog.products.labels`)

**`ProductOptionTypeManager.Component.vue`:**
- L47: `showToast('success', 'Updated', 'Product option types updated successfully')` → `showToast('success', t('common.updated'), t('catalog.products.option_types.messages.update_success'))` (needs key — add `"update_success": "Product option types updated successfully"` under new `catalog.products.option_types.messages`)

**`VariantGenerationDialog.Component.vue`:**
- L143: `showToast('success', 'Generation Complete', \`Created ${successCount} variants.\`)` → `showToast('success', t('common.success'), t('catalog.products.variants.wizard.generated', { count: successCount }))`
- L149: `showToast('error', 'Error', 'Failed to generate some variants.')` → `showToast('error', t('common.error'), t('catalog.products.variants.messages.generation_failed'))`

**`ProductImageList.Component.vue`:**
- L100: `header="Preview"` → `:header="t('catalog.products.table.preview')"` (key exists)
- L119: `header="Actions"` → `:header="t('catalog.products.table.actions')"` (key exists — but check if `catalog.products.table.actions` exists; yes from `catalog.json`)

**`ProductList.View.vue`:**
- L124: `showToast('success', t('common.success') || 'Deleted', t('catalog.products.messages.delete_success') || 'Product removed.')` → `showToast('success', t('common.success'), t('catalog.products.messages.delete_success'))`
- L257: `header="Variants"` → `:header="t('catalog.products.table.variants')"` (need key — add `"variants": "Variants"` under `catalog.products.table`)

- [ ] **Step 4: Migrate option-types files**

**`OptionTypeList.View.vue`:**
- L100: `showToast('success', t('common.success') || 'Success', t('catalog.option_types.messages.delete_success') || 'Deleted successfully')` → `showToast('success', t('common.success'), t('catalog.option_types.messages.delete_success'))`
- L102: `showToast('error', t('common.error') || 'Error', 'Failed to delete option type')` → `showToast('error', t('common.error'), t('catalog.option_types.messages.delete_error'))` (will be added in Step 1)

**`OptionTypeForm.View.vue`:**
- L102-108: showToast with fallbacks — remove `|| 'Success'` fallback, keep `t()` calls
- L119-123: same pattern
- L219: `<Tab :value="2">Metadata</Tab>` → `<Tab :value="2">{{ t('catalog.option_types.tabs.metadata') }}</Tab>` (need key — add `"tabs": { "metadata": "Metadata" }` to option_types)

**`OptionTypeManager.View.vue`:**
- L47: `showToast('success', 'Deleted', t('catalog.option_types.messages.delete_success') || 'Option type removed')` → `showToast('success', t('common.deleted'), t('catalog.option_types.messages.delete_success'))`

**`OptionValueList.View.vue`:**
- L96: remove fallback `|| 'Success'`
- L184: remove fallback `|| 'Option value deleted'`
- L220: `label="Add Value"` → `:label="t('catalog.option_values.actions.add_value')"` (need key — design has `catalog.option_values.actions.delete` and `cancel` but not `add_value`; add `"add_value": "Add Value"` to `catalog.option_values.actions`)
- L349: `label="Cancel"` → `:label="t('common.cancel')"` 
- L350: `label="Save"` → `:label="t('common.save')"`

- [ ] **Step 5: Migrate property-types files**

**`PropertyTypeList.View.vue`:**
- L102: `showToast('error', t('common.error') || 'Error', 'Failed to delete property type')` → `showToast('error', t('common.error'), t('catalog.property_types.messages.delete_error'))`
- L176: `label="Clear"` → `:label="t('catalog.property_types.table.clear_filter')"` (key exists at `catalog.property_types.table` — need to check existing locale)

**`PropertyTypeForm.View.vue`:**
- L140: `<Tab value="1">Metadata</Tab>` → `<Tab value="1">{{ t('catalog.property_types.tabs.metadata') }}</Tab>` (need key — add `"tabs": { "metadata": "Metadata" }` to property_types)

- [ ] **Step 6: Migrate taxonomy files**

**`taxonomy.store.ts`** (store — add `useI18n` import):
```ts
import { useI18n } from 'vue-i18n';
// Add after showToast:
const { t } = useI18n();
// L72: → showToast('success', t('common.created'), t('catalog.taxonomies.messages.create_success'))
// L89: → showToast('success', t('common.updated'), t('catalog.taxonomies.messages.update_success'))
// L106: → showToast('success', t('common.deleted'), t('catalog.taxonomies.messages.delete_success'))
// L123: → showToast('success', 'Rebuilt', t('catalog.taxonomies.messages.rebuilt_success')) (need key — add `"rebuilt_success": "Taxonomy tree successfully rebuilt"`)
```

**`taxon.store.ts`** (store — add `useI18n`):
```ts
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
```
Then replace all hardcoded showToast calls in this file (read the file first, as the full content wasn't captured in the review).

**`TaxonomyForm.View.vue`:**
- L62-66: remove `|| 'Success'` fallback from existing `t()` calls

**`TaxonomyList.View.vue`:**
- L108: remove `|| 'Deleted'` and `|| 'Taxonomy removed.'` fallbacks

**`TaxonomyManager.View.vue`:**
- L49: `showToast('success', 'Deleted', t('catalog.taxonomies.messages.delete_success') || 'Taxonomy removed')` → `showToast('success', t('common.deleted'), t('catalog.taxonomies.messages.delete_success'))`

**`TaxonList.View.vue`:**
- L91: `showToast('success', 'Deleted', t('catalog.taxa.messages.delete_success') || 'Category deleted')` → `showToast('success', t('common.deleted'), t('catalog.taxa.messages.delete_success'))`
- L169: `label="Clear"` → `:label="t('catalog.taxa.table.clear_filter')"` (or common key)

**`TaxonTreeManager.View.vue`:**
- L69: `showToast('success', 'Deleted', t('catalog.taxa.messages.delete_success') || 'Category removed')` → `showToast('success', t('common.deleted'), t('catalog.taxa.messages.delete_success'))`

**`TaxonForm.View.vue`:**
- L162-168: remove `|| 'Success'` fallback

**`TaxonRulesManager.Component.vue`:**
- L97-103: remove `|| 'Success'` fallback
- L115-118: remove `|| 'Rule removed'` fallback
- L127-131: remove `|| 'Task started'` fallback
- L283: `label="Cancel"` → `:label="t('common.cancel')"`
- L289: `label="Save Rule"` → `:label="t('catalog.taxa.actions.add_rule')"` (key exists)

**`TaxonProductsPreview.Component.vue`:**
- L80: `header="Preview"` → `:header="t('catalog.products.table.preview')"`
- L89: `header="Product Name"` → `:header="t('catalog.products.table.name')"`
- L97: `header="Price"` → `:header="t('catalog.products.table.price')"`
- L103: `header="Status"` → `:header="t('catalog.products.table.status')"`

**`CatalogDashboard.View.vue`:**
- L83: `label="View All"` → `:label="t('catalog.actions.view_all')"` (need key)
- L115: `label="Create New Product"` → `:label="t('catalog.products.actions.new')"` (key exists)
- L116: `label="Add Taxonomy"` → `:label="t('catalog.taxonomies.actions.create')"` (key exists)
- L117: `label="Manage Option Types"` → `:label="t('catalog.option_types.actions.manage')"` (need key)
- L87: `header="Product Name"` → `:header="t('catalog.products.table.name')"`
- L92: `header="Added On"` → need key — add to `catalog.products.table` as `"added_on": "Added On"`

- [ ] **Step 7: Lint + test**

```bash
cd app/Admin && pnpm run lint
pnpm run test:unit
```

- [ ] **Step 8: Commit**

```bash
git add app/Admin/src/shared/locales/messages/en/catalog.json app/Admin/src/features/catalog/
git commit -m "feat(i18n): migrate catalog domain to use locale keys"
```

---

### Task 2: Users & Roles — extend users.json + create roles.json

**Files:**
- Create: `shared/locales/messages/en/roles.json`
- Modify: `shared/locales/messages/en/users.json`
- Modify: `app/plugins/i18n.ts`
- Modify: `features/users/views/StaffDetail.View.vue`
- Modify: `features/users/views/StaffForm.View.vue`
- Modify: `features/users/views/CustomerDetail.View.vue`
- Modify: `features/users/views/CustomerList.View.vue`
- Modify: `features/users/stores/user.store.ts`
- Modify: `features/users/components/UserSecurityManager.Component.vue`
- Modify: `features/users/components/UserRoleManager.Component.vue`
- Modify: `features/users/components/UserPermissionManager.Component.vue`
- Modify: `features/users/roles/views/RoleList.View.vue`
- Modify: `features/users/roles/views/RoleForm.View.vue`
- Modify: `features/users/roles/views/RolePermissionsManager.View.vue`
- Modify: `features/users/permissions/views/PermissionList.View.vue`

- [ ] **Step 1: Create roles.json**

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

- [ ] **Step 2: Extend users.json**

Add under `users.titles:`
```json
    "customers": "Customer Management",
    "customer_detail": "Customer Detail"
```

Add under `users.labels:`
```json
    "phone": "Phone",
    "joined": "Joined"
```

Add under `users.messages:`
```json
    "status_updated": "User is now {status}",
    "customer_detail_error": "Failed to load customer details",
    "customer_load_error": "Failed to load customers"
```

Add under `users.security:` (replace existing if partial):
```json
    "status_title": "Account Security Status",
    "actions_title": "Administrative Actions",
    "lockout_end": "Lockout Ends",
    "failed_attempts": "Failed Attempts",
    "email_verified": "Email Verified",
    "phone_verified": "Phone Verified"
```

Add under `users.table:`
```json
    "orders": "Orders",
    "total_spent": "Total Spent"
```

Add under `users.actions:`
```json
    "save_roles": "Save Changes",
    "save_permissions": "Save Changes"
```

- [ ] **Step 3: Register roles.json in i18n.ts**

Add import at top:
```ts
import rolesEn from "@/shared/locales/messages/en/roles.json";
```

Add inside `messages.en`:
```ts
      roles: rolesEn,
```

- [ ] **Step 4: Migrate user.store.ts**

Add `useI18n` import:
```ts
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
```

Replace:
```ts
// L64: showToast('success', 'Created', 'Staff account created') → showToast('success', t('common.created'), t('users.messages.create_success'))
// L78: showToast('success', 'Deleted', 'Staff account removed') → showToast('success', t('common.deleted'), t('users.messages.delete_success'))
```

- [ ] **Step 5: Migrate view files**

**`StaffDetail.View.vue`:**
- L39: `showToast('error', 'Error', 'Failed to load user details')` → `showToast('error', t('common.error'), t('users.messages.load_error'))` (key `load_error` exists at `users.messages.loading` — check the actual key name, may need `users.messages.load_error` which doesn't exist yet; add it)
- L64: `` showToast('success', 'Status Updated', `User is now ${newStatus ? 'active' : 'inactive'}`) `` → `showToast('success', t('common.saved'), t('users.messages.status_updated', { status: newStatus ? 'active' : 'inactive' }))`
- L130: `<label>Username</label>` → `<label>{{ t('users.labels.username') }}</label>` (key exists)

**`StaffForm.View.vue`:**
- L62: `showToast('error', 'Error', 'Failed to load user details')` → `showToast('error', t('common.error'), t('users.messages.load_error'))`
- L79: `showToast('success', 'Success', 'Staff member updated successfully')` → `showToast('success', t('common.success'), t('users.messages.update_success'))`
- L93: `showToast('success', 'Success', 'Staff member invited successfully')` → `showToast('success', t('common.success'), t('users.messages.create_success'))`
- L173: `label="Cancel"` → `:label="t('common.cancel')"`

**`CustomerDetail.View.vue`:**
- L39: `showToast('error', 'Error', 'Failed to load customer details')` → `showToast('error', t('common.error'), t('users.messages.customer_detail_error'))`
- L60: `` showToast('success', 'Status Updated', `Customer is now ${newStatus ? 'active' : 'inactive'}`) `` → `showToast('success', t('common.saved'), t('users.messages.status_updated', { status: newStatus ? 'active' : 'inactive' }))`
- L100: `<span>Commerce</span>` — no locale key needed? Check what this is — likely a tab or badge label. Add to ticket if needed.

**`CustomerList.View.vue`:**
- L129: `header="Orders"` → `:header="t('users.table.orders')"`
- L135: `header="Total Spent"` → `:header="t('users.table.total_spent')"`

**`UserSecurityManager.Component.vue`:**
- L50: `showToast('success', 'Success', (t('users.messages.unlock_success') as string) || 'Account unlocked')` → `showToast('success', t('common.success'), t('users.messages.unlock_success') as string)` — keep `as string` cast
- L63: same pattern for verify_success

**`UserRoleManager.Component.vue`:**
- L47: `showToast('success', 'Success', 'User roles updated')` → `showToast('success', t('common.success'), t('users.messages.roles_updated'))` (need key — add `"roles_updated": "User roles updated"` to `users.messages`)
- L63: `label="Save Changes"` → `:label="t('users.actions.save_roles')"`

**`UserPermissionManager.Component.vue`:**
- L58: `showToast('success', 'Success', 'Direct permissions updated')` → `showToast('success', t('common.success'), t('users.messages.permissions_updated'))` (need key — add)
- L73: `label="Save Changes"` → `:label="t('users.actions.save_permissions')"`
- L82: `#sourceheader> Available </template>` → use `t('roles.picklist.available')`
- L83: `#targetheader> Assigned </template>` → use `t('roles.picklist.assigned')`

- [ ] **Step 6: Migrate roles files**

**`RoleList.View.vue`:**
- L55: `showToast('success', 'Deleted', 'Role deleted successfully')` → `showToast('success', t('common.deleted'), t('roles.messages.delete_success'))`
- L71: `<span>Roles</span>` → `<span>{{ t('roles.titles.list') }}</span>`
- L76: `label="Create Role"` → `:label="t('roles.actions.create')"`
- L94: `header="Role Name"` → `:header="t('roles.table.name')"`
- L103: `header="Priority"` → `:header="t('roles.table.priority')"`
- L109: `header="Users"` → `:header="t('roles.table.users')"`
- L118: `header="Type"` → `:header="t('roles.table.type')"`
- L126: `header="Actions"` → `:header="t('roles.table.actions')"`

**`RoleForm.View.vue`:**
- L49: `showToast('error', 'Error', 'Failed to load role details')` → `showToast('error', t('common.error'), t('roles.messages.load_error'))`
- L65: `showToast('success', 'Success', 'Role updated successfully')` → `showToast('success', t('common.success'), t('roles.messages.update_success'))`
- L77: `showToast('success', 'Success', 'Role created successfully')` → `showToast('success', t('common.success'), t('roles.messages.create_success'))`
- L118: `<label>Description</label>` → `<label>{{ t('roles.labels.description') }}</label>`
- L123: `<label>Priority</label>` → `<label>{{ t('roles.labels.priority') }}</label>`
- L137: `label="Cancel"` → `:label="t('common.cancel')"`

**`RolePermissionsManager.View.vue`:**
- L57: `showToast('success', 'Saved', 'Permissions updated successfully')` → `showToast('success', t('common.saved'), t('roles.messages.permissions_updated'))`
- L80: `label="Save Changes"` → `:label="t('roles.actions.save_permissions')"`

**`PermissionList.View.vue`:**
- L44: `<h1>Permissions</h1>` → `<h1>{{ t('roles.titles.permissions') }}</h1>`
- L73: `header="Module"` → `:header="t('permissions.table.module')"` (need permissions locale? Or reuse?)
- L75: `header="Permission Key"` → `:header="t('permissions.table.key')"`
- L77: `header="Name"` → `:header="t('permissions.table.name')"`
- L83: `header="Description"` → `:header="t('permissions.table.description')"`
  (Note: You may want to add these under roles or create a new permissions section — simplest is to extend roles.json)

- [ ] **Step 7: Lint, test, commit**

```bash
cd app/Admin && pnpm run lint && pnpm run test:unit
git add app/Admin/src/shared/locales/messages/en/users.json \
       app/Admin/src/shared/locales/messages/en/roles.json \
       app/Admin/src/app/plugins/i18n.ts \
       app/Admin/src/features/users/
git commit -m "feat(i18n): migrate users and roles domain to use locale keys"
```

---

### Task 3: Location — create location.json + migrate files

**Files:**
- Create: `shared/locales/messages/en/location.json`
- Modify: `app/plugins/i18n.ts`
- Modify: `features/location/views/CountryList.View.vue`
- Modify: `features/location/views/CountryForm.View.vue`
- Modify: `features/location/views/StateList.View.vue`
- Modify: `features/location/views/StateForm.View.vue`
- Modify: `features/location/stores/country.store.ts`
- Modify: `features/location/stores/state.store.ts`

- [ ] **Step 1: Create location.json**

Write `shared/locales/messages/en/location.json` with the content from the design doc §Key Inventory → location.json.

- [ ] **Step 2: Register in i18n.ts**

```ts
import locationEn from "@/shared/locales/messages/en/location.json";
// Inside messages.en: ...locationEn,
```

- [ ] **Step 3: Migrate country.store.ts**

Add `useI18n`:
```ts
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
```

Replace:
```ts
// L22: showToast('error', 'Error', ...) → showToast('error', t('common.error'), result.errors?.[0]?.message || t('location.messages.load_error'))
// L39: showToast('success', 'Created', 'Country created successfully') → showToast('success', t('common.created'), t('location.messages.create_success'))
// L50: showToast('success', 'Updated', 'Country updated successfully') → showToast('success', t('common.updated'), t('location.messages.update_success'))
// L61: showToast('success', 'Deleted', 'Country removed successfully') → showToast('success', t('common.deleted'), t('location.messages.delete_success'))
```

- [ ] **Step 4: Migrate state.store.ts**

Same pattern as country.store — add `useI18n`, replace toasts with `t()` calls.

- [ ] **Step 5: Migrate view files**

**`CountryList.View.vue`:**
- L53: `showToast('success', 'Deleted', 'Country removed.')` → `showToast('success', t('common.deleted'), t('location.messages.delete_success'))`
- L69: `<h2>Countries</h2>` → `<h2>{{ t('location.titles.countries') }}</h2>`
- L75: `label="New Country"` → `:label="t('location.actions.new_country')"`
- L99: `header="Name"` → `:header="t('location.labels.name')"`
- L111: `header="Calling Code"` → `:header="t('location.labels.calling_code')"`
- L117: `header="Active"` → `:header="t('location.labels.active')"`

**`CountryForm.View.vue`:**
- L58: `showToast('success', 'Updated', 'Country updated successfully')` → `showToast('success', t('common.updated'), t('location.messages.update_success'))`
- L64: `showToast('success', 'Created', 'Country created successfully')` → `showToast('success', t('common.created'), t('location.messages.create_success'))`
- L88: `<label>Name</label>` → `<label>{{ t('location.labels.name') }}</label>`
- L105: `<label>Active</label>` → `<label>{{ t('location.labels.active') }}</label>`
- L114: `label="Cancel"` → `:label="t('common.cancel')"`

**`StateList.View.vue`:**
- L58: `showToast('success', 'Deleted', 'State removed.')` → `showToast('success', t('common.deleted'), t('location.messages.state_delete_success'))`
- L100: `label="New State"` → `:label="t('location.actions.new_state')"`
- L125: `header="Name"` → `:header="t('location.labels.name')"`
- L131: `header="Abbreviation"` → `:header="t('location.labels.abbreviation')"`
- L137: `header="Country"` → `:header="t('location.labels.country')"`
- L143: `header="Active"` → `:header="t('location.labels.active')"`

**`StateForm.View.vue`:**
- L62: `showToast('success', 'Updated', 'State updated successfully')` → `showToast('success', t('common.updated'), t('location.messages.state_update_success'))`
- L68: `showToast('success', 'Created', 'State created successfully')` → `showToast('success', t('common.created'), t('location.messages.state_create_success'))`
- L92: `<label>Name</label>` → `<label>{{ t('location.labels.name') }}</label>`
- L98: `<label>Abbreviation</label>` → `<label>{{ t('location.labels.abbreviation') }}</label>`
- L104: `<label>Country</label>` → `<label>{{ t('location.labels.country') }}</label>`
- L119: `<label>Active</label>` → `<label>{{ t('location.labels.active') }}</label>`
- L128: `label="Cancel"` → `:label="t('common.cancel')"`

- [ ] **Step 6: Lint, test, commit**

```bash
cd app/Admin && pnpm run lint && pnpm run test:unit
git add app/Admin/src/shared/locales/messages/en/location.json \
       app/Admin/src/app/plugins/i18n.ts \
       app/Admin/src/features/location/
git commit -m "feat(i18n): migrate location domain to use locale keys"
```

---

### Task 4: Ordering — extend ordering.json + migrate files

**Files:**
- Modify: `shared/locales/messages/en/ordering.json`
- Modify: `features/ordering/stores/order.store.ts`
- Modify: `features/ordering/views/OrderList.View.vue`
- Modify: `features/ordering/views/OrderDetail.View.vue`
- Modify: `features/ordering/views/OrderForm.View.vue`
- Modify: `features/ordering/components/ShipmentDialog.Component.vue`
- Modify: `features/ordering/components/RefundDialog.Component.vue`
- Modify: `features/ordering/components/ItemDialog.Component.vue`
- Modify: `features/ordering/components/AddressDialog.Component.vue`
- Modify: `features/ordering/fulfillment/stores/fulfillment.store.ts`
- Modify: `features/ordering/fulfillment/views/FulfillmentQueue.View.vue`
- Modify: `features/ordering/dashboard/views/OrderingDashboard.View.vue`

- [ ] **Step 1: Extend ordering.json**

Read the current `ordering.json` first, then add these sections.

Under `ordering.titles` add:
```json
    "audit_log": "Audit Log",
    "customer_communication": "Customer & Communication",
    "logistics": "Logistics & Shipments",
    "financials": "Financials"
```

Under `ordering.messages` add:
```json
    "order_created": "Order created successfully",
    "item_added": "Item added to order",
    "addresses_updated": "Addresses updated",
    "state_advanced": "Order state advanced",
    "order_canceled": "Order canceled",
    "shipment_created": "Shipment created successfully",
    "refund_processed": "Refund processed",
    "shipped": "Order marked as shipped",
    "ship_failed": "Failed to ship",
    "customer_email_required": "Customer email is required",
    "items_required": "Please add at least one item",
    "warehouse_required": "Please select a source warehouse.",
    "items_to_ship_required": "Please select at least one item to ship.",
    "item_added_to_transfer": "Item added to transfer",
    "transfer_shipped": "Transfer shipped",
    "stock_received": "Stock received at destination"
```

Under `ordering.table` add:
```json
    "order_number": "Order #",
    "customer": "Customer",
    "date": "Date",
    "total": "Total",
    "status": "Status",
    "actions": "Actions",
    "product": "Product",
    "price": "Price",
    "qty": "Qty",
    "sku": "SKU"
```

Add new sections:
```json
  "panels": {
    "audit_log": "Audit Log",
    "customer_communication": "Customer & Communication",
    "logistics": "Logistics & Shipments",
    "financials": "Financials"
  },
  "tabs": {
    "general": "General"
  },
  "labels": {
    "currency": "Currency",
    "subtotal": "Subtotal",
    "shipping": "Shipping",
    "discount": "Discount",
    "total": "Total",
    "account": "Account",
    "number": "Number",
    "method": "Method",
    "amount": "Amount",
    "reason": "Reason",
    "quantity": "Quantity",
    "city": "City"
  },
  "actions": {
    "new_order": "New Order",
    "advance_status": "Advance Status",
    "cancel_order": "Cancel Order",
    "add_item": "Add Item",
    "view_profile": "View Profile",
    "track_package": "Track Package",
    "create_manual_shipment": "Create Manual Shipment",
    "refund": "Refund",
    "capture_payment": "Capture Manual Payment",
    "create_shipment": "Create Shipment",
    "process_refund": "Process Refund",
    "add_to_order": "Add Item to Order",
    "update_addresses": "Update Addresses",
    "ship_order": "Ship Order",
    "create_order": "Create Order",
    "cancel": "Cancel"
  },
  "status_labels": {
    "draft": "Draft",
    "placed": "Placed",
    "canceled": "Canceled",
    "expired": "Expired"
  }
```

- [ ] **Step 2: Migrate order.store.ts**

Add `useI18n`, replace toasts:

```ts
// L41: showToast('success', 'Success', 'Order created successfully') → showToast('success', t('common.success'), t('ordering.messages.order_created'))
// L54: showToast('success', 'Success', 'Item added to order') → showToast('success', t('common.success'), t('ordering.messages.item_added'))
// L74: showToast('success', 'Success', 'Addresses updated') → showToast('success', t('common.success'), t('ordering.messages.addresses_updated'))
// L87: showToast('success', 'Success', 'Order state advanced') → showToast('success', t('common.success'), t('ordering.messages.state_advanced'))
// L101: showToast('success', 'Success', 'Order canceled') → showToast('success', t('common.success'), t('ordering.messages.order_canceled'))
```

- [ ] **Step 3: Migrate fulfillment.store.ts**

Same pattern — add `useI18n`, replace toasts.

- [ ] **Step 4: Migrate view files**

Read each file and apply the patterns: replace `showToast` summaries/details, replace `label=""` with `:label="t(...)"`, replace `header=""` with `:header="t(...)"`, replace `<span>Text</span>` with `{{ t(...) }}`.

Key mappings:
- `header="Product"` → `:header="t('ordering.table.product')"`
- `header="Price"` → `:header="t('ordering.table.price')"`
- `header="Qty"` → `:header="t('ordering.table.qty')"`
- `header="Total"` → `:header="t('ordering.table.total')"`
- `header="Status"` → `:header="t('ordering.table.status')"`
- `header="Order #"` → `:header="t('ordering.table.order_number')"`
- `header="Customer"` → `:header="t('ordering.table.customer')"`
- `header="Date"` → `:header="t('ordering.table.date')"`
- `header="Actions"` → `:header="t('ordering.table.actions')"`
- `label="Cancel"` → `:label="t('common.cancel')"`
- `label="Create Shipment"` → `:label="t('ordering.actions.create_shipment')"`
- Panel headers → `t('ordering.panels.*')`, Dialog headers → `t('ordering.actions.*')`
- `<span>Draft</span>` → `{{ t('ordering.status_labels.draft') }}` etc.
- `<span>Subtotal</span>` → `{{ t('ordering.labels.subtotal') }}`

- [ ] **Step 5: Lint, test, commit**

```bash
cd app/Admin && pnpm run lint && pnpm run test:unit
git add app/Admin/src/shared/locales/messages/en/ordering.json app/Admin/src/features/ordering/
git commit -m "feat(i18n): migrate ordering domain to use locale keys"
```

---

### Task 5: Inventory — extend inventory.json + migrate files

**Files:**
- Modify: `shared/locales/messages/en/inventory.json`
- Modify: `features/inventories/views/StockLocationForm.View.vue`
- Modify: `features/inventories/views/StockLocationList.View.vue`
- Modify: `features/inventories/views/StockLocationManager.View.vue`
- Modify: `features/inventories/views/StockTransferForm.View.vue`
- Modify: `features/inventories/views/StockTransferDetail.View.vue`
- Modify: `features/inventories/views/StockItemList.View.vue`
- Modify: `features/inventories/views/InventoryUnitList.View.vue`
- Modify: `features/inventories/views/StockTransferList.View.vue`
- Modify: `features/inventories/components/StockAdjustmentDialog.Component.vue`
- Modify: `features/inventories/dashboard/views/InventoryDashboard.View.vue`

- [ ] **Step 1: Extend inventory.json**

Read current `inventory.json`, add these sections.

Under `inventory.messages` add:
```json
    "item_added_to_transfer": "Item added to transfer",
    "transfer_shipped": "Transfer shipped",
    "stock_received": "Stock received at destination",
    "location_updated": "Location updated",
    "location_created": "Location created",
    "source_destination_same": "Source and destination cannot be the same.",
    "load_error": "Failed to load inventory",
    "load_states_error": "Failed to load states"
```

Under `inventory.actions` add:
```json
    "cancel": "Cancel",
    "add_to_transfer": "Add to Transfer",
    "save": "Save",
    "back": "Back"
```

Under `inventory.table` add (column headers beyond what exists):
```json
    "serial_number": "Serial Number",
    "registered": "Registered",
    "initiated": "Initiated",
    "actions": "Actions",
    "location_name": "Location Name",
    "code": "Code",
    "type": "Type",
    "product": "Product",
    "quantity": "Quantity"
```

Under `inventory.titles` add:
```json
    "stock_movement_history": "Stock Movement History",
    "merchandise": "Merchandise",
    "destination": "Destination",
    "overview": "Overview",
    "locations": "Locations",
    "identification": "Identification",
    "inventory": "Inventory"
```

- [ ] **Step 2: Migrate all inventory view files**

Same patterns as previous tasks. Read each file, replace all hardcoded strings with `t('inventory.*')` calls.

Key hardcoded strings to migrate from the review:

**StockLocationForm.View.vue:**
- L104: showToast ternary with `'Location updated'` → split into two t() calls
- L153: `<span>Identification</span>` → t
- L219: `label="Cancel"` → t

**StockLocationList.View.vue:**
- L31: `<h3>Overview</h3>` → t
- L68: `label="Inventory"` → t
- L76-89: column headers → t

**StockLocationManager.View.vue:**
- L56: `showToast('success', 'Deleted', t(...) || 'Location removed')` → remove fallback

**StockTransferForm.View.vue:**
- L27: `showToast('error', 'Error', 'Source and destination cannot be the same.')` → t
- L35: showToast with fallback → remove fallback

**StockTransferDetail.View.vue:**
- L58: showToast hardcoded → t
- L74: showToast with fallback → remove fallback
- L87: showToast hardcoded → t
- L143: `<span>Merchandise</span>` → t
- L152: column header → t
- L160: column header → t
- L198: `<span>Destination</span>` → t
- L213: dialog header → t
- L233-234: button labels → t

**StockItemList.View.vue:**
- L223: drawer header → t

**InventoryUnitList.View.vue:**
- L93: column header → t
- L108: column header → t

**StockTransferList.View.vue:**
- L101: column header → t

**StockAdjustmentDialog.Component.vue:**
- L42: showToast with fallback → remove fallback
- L85: `label="Cancel"` → t

**InventoryDashboard.View.vue:**
- L47: `<p>Locations</p>` → t
- L59-62: column headers → t

- [ ] **Step 3: Lint, test, commit**

```bash
cd app/Admin && pnpm run lint && pnpm run test:unit
git add app/Admin/src/shared/locales/messages/en/inventory.json app/Admin/src/features/inventories/
git commit -m "feat(i18n): migrate inventory domain to use locale keys"
```

---

### Task 6: Profile — create profile.json + migrate files

**Files:**
- Create: `shared/locales/messages/en/profile.json`
- Modify: `app/plugins/i18n.ts`
- Modify: `features/profile/views/Profile.View.vue`
- Modify: `features/profile/stores/profile.store.ts`

- [ ] **Step 1: Create profile.json**

Use content from design doc §Key Inventory → profile.json.

- [ ] **Step 2: Register in i18n.ts**

```ts
import profileEn from "@/shared/locales/messages/en/profile.json";
// Inside messages.en: ...profileEn,
```

- [ ] **Step 3: Migrate profile.store.ts**

Add `useI18n`, replace:
```ts
// L19: showToast('error', 'Error', ...) → showToast('error', t('common.error'), result.errors?.[0]?.message || t('profile.messages.load_error'))
// L30: showToast('success', 'Updated', 'Profile updated successfully') → showToast('success', t('common.updated'), t('profile.messages.update_success'))
```

- [ ] **Step 4: Migrate Profile.View.vue**

Replace all hardcoded text in both script and template:
- L34: showToast → t('profile.messages.load_error')
- L46: showToast → t('profile.messages.update_success')
- L98: `<span>Account Details</span>` → `{{ t('profile.titles.details') }}`
- L104: `<span>Full Name</span>` → `{{ t('profile.labels.full_name') }}`
- L108: `<span>Email Address</span>` → `{{ t('profile.labels.email') }}`
- L112: `<span>Username</span>` → `{{ t('profile.labels.username') }}`
- L116: `<span>Joined On</span>` → `{{ t('profile.labels.joined') }}`
- L129: `<span>Change Password</span>` → `{{ t('profile.titles.password') }}`
- L135: `<label>Current Password</label>` → `{{ t('profile.labels.current_password') }}`
- L139: `<label>New Password</label>` → `{{ t('profile.labels.new_password') }}`
- L143: `<label>Confirm New Password</label>` → `{{ t('profile.labels.confirm_password') }}`
- L146: `label="Update Password"` → `:label="t('profile.actions.update_password')"`
- L156: `<span>Notifications</span>` → `{{ t('profile.titles.notifications') }}`
- L163: `<p>Email Notifications</p>` → `{{ t('profile.notifications.email') }}`
- L164: `<p>Receive email updates...</p>` → `{{ t('profile.notifications.email_desc') }}`
- L170: `<p>Order Updates</p>` → `{{ t('profile.notifications.order_updates') }}`
- L171: `<p>Get notified...</p>` → `{{ t('profile.notifications.order_updates_desc') }}`
- L177: `<p>Marketing</p>` → `{{ t('profile.notifications.marketing') }}`
- L178: `<p>Receive promotional offers...</p>` → `{{ t('profile.notifications.marketing_desc') }}`
- L184: `<p>Security Alerts</p>` → `{{ t('profile.notifications.security') }}`
- L185: `<p>Important security...</p>` → `{{ t('profile.notifications.security_desc') }}`
- L103: `label="Save Profile"` → `:label="t('profile.actions.save_profile')"`

- [ ] **Step 5: Lint, test, commit**

```bash
cd app/Admin && pnpm run lint && pnpm run test:unit
git add app/Admin/src/shared/locales/messages/en/profile.json \
       app/Admin/src/app/plugins/i18n.ts \
       app/Admin/src/features/profile/
git commit -m "feat(i18n): migrate profile domain to use locale keys"
```

---

### Task 7: Auth + Error + Dashboard — create error.json, extend auth.json, migrate remaining files

**Files:**
- Create: `shared/locales/messages/en/error.json`
- Modify: `shared/locales/messages/en/auth.json`
- Modify: `app/plugins/i18n.ts`
- Modify: `features/auth/views/Profile.View.vue`
- Modify: `features/error/pages/NotFound.View.vue`
- Modify: `features/error/pages/ErrorPage.View.vue`
- Modify: `features/error/pages/AccessDenied.View.vue`
- Modify: `features/dashboard/ui/DashboardPage.View.vue`
- Modify: `features/reports/views/Dashboard.View.vue`

- [ ] **Step 1: Create error.json**

Use content from design doc §Key Inventory → error.json.

- [ ] **Step 2: Extend auth.json**

Add under `auth.labels`:
```json
    "account_details": "Account Details",
    "current_password": "Current Password",
    "new_password": "New Password",
    "confirm_password": "Confirm New Password"
```

Add under `auth.actions`:
```json
    "update_password": "Update Password"
```

Add under `auth.messages`:
```json
    "password_mismatch": "New passwords do not match"
```

- [ ] **Step 3: Register error.json in i18n.ts**

```ts
import errorEn from "@/shared/locales/messages/en/error.json";
// Inside messages.en: ...errorEn,
```

- [ ] **Step 4: Migrate auth/Profile.View.vue**

- L34: `showToast('error', 'Error', 'Failed to load user profile')` → `showToast('error', t('common.error'), t('profile.messages.load_error'))` (profile key already exists)
- L51: `showToast('error', 'Validation Error', 'New passwords do not match')` → `showToast('error', t('common.error'), t('auth.messages.password_mismatch'))`
- L57: `showToast('success', 'Success', 'Password updated successfully')` → `showToast('success', t('common.success'), t('profile.messages.password_updated'))`
- L98: `<span>Account Details</span>` → `{{ t('auth.labels.account_details') }}`
- L104: `<span>Full Name</span>` → `{{ t('profile.labels.full_name') }}`
- L108: `<span>Email Address</span>` → `{{ t('profile.labels.email') }}`
- L112: `<span>Username</span>` → `{{ t('profile.labels.username') }}`
- L116: `<span>Joined On</span>` → `{{ t('profile.labels.joined') }}`
- L129: `<span>Change Password</span>` → `{{ t('profile.titles.password') }}`
- L135: `<label>Current Password</label>` → `{{ t('auth.labels.current_password') }}`
- L139: `<label>New Password</label>` → `{{ t('auth.labels.new_password') }}`
- L143: `<label>Confirm New Password</label>` → `{{ t('auth.labels.confirm_password') }}`
- L146: `label="Update Password"` → `:label="t('auth.actions.update_password')"`

- [ ] **Step 5: Migrate error pages**

**NotFound.View.vue:**
- L32: `label="Go to Dashboard"` → `:label="t('error.not_found.action')"`

**ErrorPage.View.vue:**
- L31: `label="Try Again"` → `:label="t('error.server_error.action_retry')"`
- L37: `label="Go to Dashboard"` → `:label="t('error.server_error.action_home')"`

**AccessDenied.View.vue:**
- L31: `label="Back to Dashboard"` → `:label="t('error.access_denied.action')"`

- [ ] **Step 6: Migrate dashboard files**

**DashboardPage.View.vue:**
- L16: `<p>Roles</p>` → `<p>{{ t('roles.titles.list') }}</p>`
- L22: `<p>Permissions</p>` → `<p>{{ t('roles.titles.permissions') }}</p>`

**Dashboard.View.vue (reports):**
- L88: `<h1>Dashboard</h1>` → `<h1>{{ t('reports.titles.dashboard') }}</h1>` (need key — add or keep as common: `t('navigation.dashboard')` exists)
- L138: `<span>Revenue</span>` → `<span>{{ t('reports.labels.revenue') }}</span>` (need key)
- L203: `header="Order"` → `:header="t('ordering.table.order_number')"`
- L219: `header="Date"` → `:header="t('ordering.table.date')"`
- L224: `header="Status"` → `:header="t('ordering.table.status')"`
- L250: `<span>Activity</span>` → `<span>{{ t('reports.labels.activity') }}</span>` (need key)

- [ ] **Step 7: Lint, test, commit**

```bash
cd app/Admin && pnpm run lint && pnpm run test:unit
git add app/Admin/src/shared/locales/messages/en/error.json \
       app/Admin/src/shared/locales/messages/en/auth.json \
       app/Admin/src/app/plugins/i18n.ts \
       app/Admin/src/features/auth/ \
       app/Admin/src/features/error/ \
       app/Admin/src/features/dashboard/ \
       app/Admin/src/features/reports/
git commit -m "feat(i18n): migrate auth, error, and dashboard domains to use locale keys"
```
