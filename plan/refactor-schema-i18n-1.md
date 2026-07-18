---
goal: Convert all Zod validation schemas from hardcoded English to i18n factory functions
version: 1.0
date_created: 2026-07-18
owner: Agent
status: Planned
tags: refactor, admin-spa, i18n, schemas
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Convert all 33 Zod schema files from `export const Schema = z.object({...})` (hardcoded English error messages) to `export function createSchema(t: (key: string, args?: Record<string, unknown>) => string)`. Add validation message keys to existing domain locale JSON files. Update all consumer components to call the factory with `t()` from `useI18n()`.

## 1. Requirements & Constraints

- **REQ-001**: Every schema file exports a factory function `create<Name>Schema(t)` instead of a singleton `export const <Name>Schema`
- **REQ-002**: Factory parameter `t` matches the `vue-i18n` `t()` signature: `(key: string, args?: Record<string, unknown>) => string`
- **REQ-003**: All hardcoded English strings replaced with i18n keys using format `validation.{field}.{rule}` — e.g., `validation.name.required`, `validation.sku.max_length`
- **REQ-004**: Validation message keys added to domain-specific locale JSON files under a `validation` key
- **REQ-005**: Every consumer component that imports a schema calls the factory: `const schema = createSchema(t)` and passes the result to `toTypedSchema(schema)`
- **REQ-006**: The `z.infer<typeof Schema>` pattern changes — use `z.infer<ReturnType<typeof createSchema>>` or extract the type separately using `z.ZodObject`
- **CON-001**: No schema file imports `useI18n` directly — parameter injection only
- **CON-002**: No breaking changes to the exported type name — consumers that use `LoginParameters` (type) should still work
- **CON-003**: Existing locale JSON files get a `validation` key added; no new locale files created
- **GUD-001**: i18n key convention: `validation.{domain}.{field}.{rule}` where domain matches locale filename (e.g., `auth.json` → `auth`, `catalog.json` → `catalog`)
- **GUD-002**: Generic rules shared across domains (`.required`, `.max_length`, `.min_length`) use consistent key structure

## 2. Implementation Steps

### Phase 1: Add validation keys to all locale JSON files

- GOAL-001: Add `validation` object to each domain locale file with all message keys used by that domain's schemas

| Task | File | Keys to Add |
|------|------|-------------|
| **LOC-001** | `shared/locales/messages/en/auth.json` | `validation.credential.required`, `validation.credential.max_length`, `validation.password.required`, `validation.password.max_length` |
| **LOC-002** | `shared/locales/messages/en/catalog.json` | `validation.name.required`, `validation.name.max_length`, `validation.presentation.required`, `validation.presentation.max_length`, `validation.description.max_length`, `validation.position.whole`, `validation.position.min`, `validation.internal_name.required`, `validation.internal_name.max_length`, `validation.display_name.required`, `validation.display_name.max_length`, `validation.sku.required`, `validation.sku.max_length`, `validation.slug.required`, `validation.slug.max_length`, `validation.slug.format`, `validation.price.min`, `validation.weight.min`, `validation.height.min`, `validation.width.min`, `validation.depth.min`, `validation.meta_title.max_length`, `validation.meta_description.max_length`, `validation.meta_keywords.max_length`, `validation.barcode.max_length`, `validation.compare_at_price.min`, `validation.cost_price.min`, `validation.taxon_id.invalid`, `validation.taxonomy.required`, `validation.taxon.name.required`, `validation.taxon.presentation.required`, `validation.taxon.slug.required`, `validation.rule_type.required`, `validation.value.required`, `validation.match_policy.required`, `validation.option_type_ids.min`, `validation.currency.length` |
| **LOC-003** | `shared/locales/messages/en/inventory.json` | `validation.quantity.whole`, `validation.type.required`, `validation.type.invalid`, `validation.reason.max_length`, `validation.reference.max_length`, `validation.name.required`, `validation.name.max_length`, `validation.code.required`, `validation.code.max_length`, `validation.code.format`, `validation.address.required`, `validation.address.max_length`, `validation.city.required`, `validation.city.max_length`, `validation.zip.required`, `validation.zip.max_length`, `validation.country_code.length`, `validation.state_code.max_length`, `validation.phone.max_length`, `validation.stock_item.required`, `validation.stock_item.invalid`, `validation.state.required`, `validation.source_location.required`, `validation.source_location.invalid`, `validation.destination_location.required`, `validation.destination_location.invalid`, `validation.variant.required`, `validation.variant.invalid`, `validation.quantity.min_one` |
| **LOC-004** | `shared/locales/messages/en/location.json` | `validation.name.required`, `validation.name.max_length`, `validation.iso_code.length`, `validation.calling_code.max_length`, `validation.abbreviation.required`, `validation.abbreviation.max_length`, `validation.country.required`, `validation.country.invalid` |
| **LOC-005** | `shared/locales/messages/en/ordering.json` | `validation.first_name.required`, `validation.first_name.max_length`, `validation.last_name.required`, `validation.last_name.max_length`, `validation.address.required`, `validation.address.max_length`, `validation.city.required`, `validation.city.max_length`, `validation.zip.required`, `validation.zip.max_length`, `validation.country_code.length`, `validation.state_code.max_length`, `validation.phone.max_length`, `validation.company.max_length`, `validation.email.required`, `validation.email.invalid`, `validation.currency.length`, `validation.variant.required`, `validation.variant.invalid`, `validation.quantity.whole`, `validation.quantity.min_one`, `validation.items.min_one`, `validation.tracking_number.max_length`, `validation.stock_location.required`, `validation.stock_location.invalid`, `validation.inventory_unit.required`, `validation.inventory_unit.invalid`, `validation.units.min_one` |
| **LOC-006** | `shared/locales/messages/en/users.json` | `validation.email.required`, `validation.email.invalid`, `validation.email.max_length`, `validation.first_name.required`, `validation.first_name.max_length`, `validation.last_name.required`, `validation.last_name.max_length`, `validation.roles.min_one`, `validation.password.min_length`, `validation.password.max_length`, `validation.phone.max_length`, `validation.identifier.required`, `validation.identifier.max_length`, `validation.identifier.format`, `validation.name.required`, `validation.name.max_length`, `validation.description.max_length`, `validation.action.required`, `validation.action.max_length`, `validation.role_name.required`, `validation.role_name.max_length`, `validation.role_name.format`, `validation.display_name.max_length`, `validation.priority.whole`, `validation.priority.min` |
| **LOC-007** | `shared/locales/messages/en/profile.json` | `validation.first_name.required`, `validation.first_name.max_length`, `validation.last_name.required`, `validation.last_name.max_length`, `validation.phone.max_length`, `validation.bio.max_length`, `validation.url.invalid`, `validation.address.required`, `validation.city.required`, `validation.state_province.required`, `validation.postal_code.required`, `validation.country.required` |
| **LOC-008** | `shared/locales/messages/en/payment.json` | Create this file if not exists: `validation.name.required`, `validation.provider.required` |
| **LOC-009** | `shared/locales/messages/en/shipping.json` | Create this file if not exists: `validation.name.required`, `validation.carrier.required`, `validation.shipping_method.required`, `validation.shipping_method.invalid`, `validation.rate.min` |

Each locale file should have the validation keys added under a `"validation"` top-level key. Example for `auth.json`:

```json
{
  "validation": {
    "credential": {
      "required": "Email or Username is required",
      "max_length": "Credential must not exceed 255 characters"
    },
    "password": {
      "required": "Password is required",
      "max_length": "Password must not exceed 128 characters"
    }
  },
  ...existing content...
}
```

### Phase 2: Convert all schema files to factory functions

- GOAL-002: Every `*.Schema.ts` changes from `export const <Name>Schema = z.object(...)` to `export function create<Name>Schema(t)`

#### Pattern for each schema file:

**Before** (`auth/schemas/Login.Schema.ts`):
```typescript
import { z } from 'zod'

export const LoginSchema = z.object({
  credential: z
    .string()
    .min(1, 'Email or Username is required')
    .max(255, 'Credential must not exceed 255 characters'),
  password: z
    .string()
    .min(1, 'Password is required')
    .max(128, 'Password must not exceed 128 characters'),
  rememberMe: z.boolean().optional().default(false),
})

export type LoginParameters = z.infer<typeof LoginSchema>
```

**After**:
```typescript
import { z } from 'zod'

export function createLoginSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
    credential: z
      .string()
      .min(1, t('auth.validation.credential.required'))
      .max(255, t('auth.validation.credential.max_length')),
    password: z
      .string()
      .min(1, t('auth.validation.password.required'))
      .max(128, t('auth.validation.password.max_length')),
    rememberMe: z.boolean().optional().default(false),
  })
}

export type LoginParameters = z.infer<ReturnType<typeof createLoginSchema>>
```

**Type inference change**: The `CreateProductSchema` and `UpdateProductSchema` pair needs special handling — `UpdateProductSchema` uses `CreateProductSchema.partial()`. Convert to a pattern where both are factories:

```typescript
export function createProductSchema(t: ...) { return z.object({...}) }

export function createUpdateProductSchema(t: ...) {
  return createProductSchema(t).partial()
}
```

| Task | Schema File | Key Changes |
|------|------------|-------------|
| **SCH-001** | `auth/schemas/Login.Schema.ts` | Factory function `createLoginSchema(t)`, 4 messages → locale keys |
| **SCH-002** | `auth/schemas/ChangePassword.Schema.ts` | Factory function `createChangePasswordSchema(t)`, 6 messages → locale keys |
| **SCH-003** | `catalog/option-types/schemas/OptionType.Schema.ts` | Factory, 7 messages |
| **SCH-004** | `catalog/option-types/option-values/schemas/OptionValue.Schema.ts` | Factory, 6 messages |
| **SCH-005** | `catalog/products/schemas/CreateProduct.Schema.ts` | Factory, 14 messages |
| **SCH-006** | `catalog/products/schemas/UpdateProduct.Schema.ts` | Factory, calls `createProductSchema(t).partial()` |
| **SCH-007** | `catalog/products/classifications/schemas/ProductClassification.Schema.ts` | Factory, 2 messages |
| **SCH-008** | `catalog/products/option-types/schemas/ProductOptionType.Schema.ts` | Factory, 1 message |
| **SCH-009** | `catalog/products/variants/schemas/Variant.Schema.ts` | Factory, 12 messages |
| **SCH-010** | `catalog/products/variants/prices/schemas/Price.Schema.ts` | Factory, 2 messages |
| **SCH-011** | `catalog/products/variants/images/schemas/Image.Schema.ts` | Factory, no custom messages (or add `validation.role.range`) |
| **SCH-012** | `catalog/taxonomies/schemas/Taxonomy.Schema.ts` | Factory, 2 messages |
| **SCH-013** | `catalog/taxonomies/schemas/Taxon.Schema.ts` | Factory, 4 messages |
| **SCH-014** | `catalog/taxonomies/schemas/TaxonRule.Schema.ts` | Factory, 3 messages |
| **SCH-015** | `inventories/stock-items/schemas/StockItem.Schema.ts` | Factory, 5 messages |
| **SCH-016** | `inventories/stock-locations/schemas/StockLocation.Schema.ts` | Factory, 16 messages |
| **SCH-017** | `inventories/stock-movements/schemas/StockMovement.Schema.ts` | Factory, 3 messages |
| **SCH-018** | `inventories/stock-transfers/schemas/StockTransfer.Schema.ts` | Factory, 8 messages |
| **SCH-019** | `inventories/inventory-units/schemas/InventoryUnit.Schema.ts` | Factory, 3 messages |
| **SCH-020** | `location/countries/schemas/Country.Schema.ts` | Factory, 4 messages |
| **SCH-021** | `location/states/schemas/State.Schema.ts` | Factory, 6 messages |
| **SCH-022** | `ordering/orders/schemas/Order.Schema.ts` | Factory, 21 messages |
| **SCH-023** | `ordering/fulfillment/schemas/Fulfillment.Schema.ts` | Factory, 5 messages |
| **SCH-024** | `payment/payment-methods/schemas/PaymentMethod.Schema.ts` | Factory, 2 messages |
| **SCH-025** | `payment/payments/schemas/Payment.Schema.ts` | Factory, no custom messages |
| **SCH-026** | `shipping/shipping-methods/schemas/ShippingMethod.Schema.ts` | Factory, 2 messages |
| **SCH-027** | `shipping/shipping-rates/schemas/ShippingRate.Schema.ts` | Factory, 2 messages |
| **SCH-028** | `profile/schemas/Profile.Schema.ts` | Factory, 7 messages |
| **SCH-029** | `profile/addresses/schemas/Address.Schema.ts` | Factory, 5 messages |
| **SCH-030** | `users/schemas/User.Schema.ts` | Factory, 11 messages |
| **SCH-031** | `users/roles/schemas/Role.Schema.ts` | Factory, 7 messages |
| **SCH-032** | `users/permissions/schemas/Permission.Schema.ts` | Factory, 8 messages |
| **SCH-033** | `reports/schemas/Report.Schema.ts` | Factory, no custom messages |

### Phase 3: Update all consumer components to use factory functions

- GOAL-003: Every `.vue` file that imports a schema now calls the factory with `t`

Find all consumers:
```bash
grep -rn "from.*schemas/.*\.Schema" app/Admin/src/ --include="*.vue" --include="*.ts" | grep -v ".spec.ts" | grep -v ".test.ts"
```

For each consumer file:
1. Keep existing `import { useI18n } from 'vue-i18n'` and `const { t } = useI18n()`
2. Change import from `import { LoginSchema } from '...'` to `import { createLoginSchema } from '...'`
3. In the `useForm({ validationSchema: toTypedSchema(LoginSchema) })` call, change to `validationSchema: toTypedSchema(createLoginSchema(t))`

**Before** (Login.View.vue):
```typescript
import { LoginSchema } from '../schemas/Login.Schema'
import { useI18n } from 'vue-i18n'
const { t } = useI18n()
// ... later (t is used for UI but not passed to schema)
const { ... } = useForm({
  validationSchema: toTypedSchema(LoginSchema),
})
```

**After**:
```typescript
import { createLoginSchema } from '../schemas/Login.Schema'
import { useI18n } from 'vue-i18n'
const { t } = useI18n()
// ...
const { ... } = useForm({
  validationSchema: toTypedSchema(createLoginSchema(t)),
})
```

| Task | Consumer File | Schema Import Change |
|------|--------------|---------------------|
| **CON-001** | `auth/views/Login.View.vue` | `LoginSchema` → `createLoginSchema(t)` |
| **CON-002** | `auth/views/Profile.View.vue` | `ChangePasswordSchema` → `createChangePasswordSchema(t)` |
| **CON-003** | `catalog/option-types/views/OptionTypeForm.View.vue` | `OptionTypeSchema` → `createOptionTypeSchema(t)` |
| **CON-004** | `catalog/option-types/option-values/views/OptionValueList.View.vue` | `OptionValueSchema` → `createOptionValueSchema(t)` |
| **CON-005** | `catalog/products/views/ProductForm.View.vue` | `CreateProductSchema` / `UpdateProductSchema` |
| **CON-006** | `catalog/products/variants/components/VariantFormDialog.Component.vue` | `VariantSchema` |
| **CON-007** | `catalog/products/classifications/components/ProductClassificationManager.Component.vue` | `ManageClassificationsSchema` |
| **CON-008** | `catalog/taxonomies/views/TaxonomyForm.View.vue` | `TaxonomySchema` |
| **CON-009** | `catalog/taxonomies/taxa/components/TaxonForm.Component.vue` | `TaxonSchema` |
| **CON-010** | `catalog/taxonomies/taxa/components/TaxonRulesManager.Component.vue` | `TaxonRuleSchema` |
| **CON-011** | `inventories/views/StockItemList.View.vue` | `StockAdjustmentSchema` |
| **CON-012** | `inventories/views/StockLocationForm.View.vue` | `StockLocationSchema` |
| **CON-013** | `inventories/views/StockTransferForm.View.vue` | `StockTransferSchema` |
| **CON-014** | `inventories/components/StockAdjustmentDialog.Component.vue` | `StockAdjustmentSchema` |
| **CON-015** | `location/views/CountryForm.View.vue` | `CountrySchema` |
| **CON-016** | `location/views/StateForm.View.vue` | `StateSchema` |
| **CON-017** | `ordering/views/OrderForm.View.vue` | `OrderSchema` |
| **CON-018** | `ordering/components/ShipmentDialog.Component.vue` | `FulfillmentSchema` |
| **CON-019** | `payment/payment-methods/views/PaymentMethodForm.View.vue` | `PaymentMethodSchema` |
| **CON-020** | `shipping/shipping-methods/views/ShippingMethodForm.View.vue` | `ShippingMethodSchema` |
| **CON-021** | `shipping/shipping-rates/views/ShippingRateForm.View.vue` | `ShippingRateSchema` |
| **CON-022** | `profile/views/ProfileForm.View.vue` | `ProfileSchema` |
| **CON-023** | `users/views/StaffForm.View.vue` | `UserSchema` |
| **CON-024** | `users/roles/views/RoleForm.View.vue` | `RoleSchema` |
| **CON-025** | `users/permissions/views/PermissionList.View.vue` | `PermissionSchema` |
| **CON-026** | `ordering/fulfillment/views/FulfillmentQueue.View.vue` | `FulfillmentSchema` |

### Phase 4: Update type-only imports

- GOAL-004: Files that import only types from schema files (not the schema itself) may need updates if they use `z.infer<typeof Schema>` — update to `z.infer<ReturnType<typeof createSchema>>`

Find type-only imports:
```bash
grep -rn "Parameters.Type" app/Admin/src/features/ --include="*.ts" --include="*.vue" | head -20
```

Most `*.Parameters.Type.ts` files re-export the inferred type:
```typescript
import type { LoginParameters } from '../schemas/Login.Schema'
export type { LoginParameters }
```

These should work without changes since the type name `LoginParameters` is preserved — only the definition changes from `z.infer<typeof LoginSchema>` to `z.infer<ReturnType<typeof createLoginSchema>>`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| **TYP-001** | Verify all `*.Parameters.Type.ts` still resolve correctly | | |
| **TYP-002** | Run typecheck to catch any type inference issues | | |

### Phase 5: Verification

- GOAL-005: All i18n keys resolve, typecheck passes, no hardcoded English strings remain in schemas

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| **VER-001** | `rg "\.min\(1, '[A-Z]" app/Admin/src/features/` — zero matches (no hardcoded English in schemas) | | |
| **VER-002** | `rg "z\.string\(\)" app/Admin/src/features/*/schemas/` — verify `.min()` calls use `t(...)` | | |
| **VER-003** | `pnpm run lint` — pass | | |
| **VER-004** | `vue-tsc --build` — zero `Cannot find module` errors | | |
| **VER-005** | `pnpm run test:unit` — same pre-existing failures only | | |

## 3. Alternatives

- **ALT-001**: Use Zod's `z.setErrorMap()` global error map — simpler (1 file change vs 60+) but loses per-field specificity and requires i18n to be initialized before Zod is imported, which is fragile
- **ALT-002**: Keep hardcoded English and translate at render time with `{{ t(errors.fieldName) }}` — doesn't work because Zod messages are strings, not keys
- **ALT-003**: Single `validation.json` locale file — rejected in favor of per-domain validation keys to follow existing convention

## 4. Dependencies

- **DEP-001**: Phase 1 (locale keys) must complete before Phase 2 (schemas reference keys)
- **DEP-002**: Phase 2 must complete before Phase 3 (components call factories with t)
- **DEP-003**: All phases must complete before Phase 5 (verification)

## 5. Files

| Scope | Files Modified | Files Created |
|-------|---------------|---------------|
| Locale JSON | ~9 | 0 (keys added to existing) |
| Schema `.ts` files | 33 | 0 |
| Consumer `.vue`/`.ts` files | ~26 | 0 |

## 6. Testing

- **TEST-001**: `rg "\.min\(1, '[A-Z]" app/Admin/src/features/` — zero matches after conversion
- **TEST-002**: `vue-tsc --build` — zero type errors
- **TEST-003**: `pnpm run test:unit` — same baseline failures

## 7. Risks & Assumptions

- **RISK-001**: Some components may pass `t` that doesn't match expected signature — mitigated by consistent `(key: string, args?: Record<string, unknown>) => string` type
- **RISK-002**: `z.infer<ReturnType<typeof createSchema>>` may not work identically to `z.infer<typeof Schema>` — test with `vue-tsc`
- **ASSUMPTION-001**: All 9 domain locale files exist (payment.json and shipping.json may need creation)
- **ASSUMPTION-002**: All consumers have access to `useI18n()` (they do — it's Composition API)

## 8. Related Specifications / Further Reading

- i18n setup: `app/Admin/src/app/plugins/i18n.ts`
- Locale files: `app/Admin/src/shared/locales/messages/en/*.json`
- Zod custom error messages: https://zod.dev/ERROR_HANDLING
- vee-validate + Zod integration: https://vee-validate.logaretm.com/v4/integrations/zod/
