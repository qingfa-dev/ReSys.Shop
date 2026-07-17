---
goal: Full Admin SPA type/schema restructure — all remaining modules (auth, catalog, location, ordering, users, inventories, profile, reports)
version: 2.0
date_created: 2026-07-17
status: Planned
tags: refactor, typescript, architecture, admin, schema
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Complete the type/schema restructure across all remaining Admin SPA modules, following the pattern established in the Taxonomies & Taxa pilot. Each entity gets one `{Entity}.Schema.ts` (with per-field validation messages) and four `{Entity}.{Parameters|Request|Response|Query}.Type.ts` files. This covers 18 entities across 8 modules.

## 1. Requirements & Constraints

- **REQ-001**: Every entity has exactly one `{Entity}.Schema.ts` with validation messages on EVERY constraint
- **REQ-002**: `Parameters.Type.ts` re-exports `z.infer<typeof Schema>` as `{Entity}Parameters`
- **REQ-003**: `Request.Type.ts` extends Parameters via `&` or type alias, never duplicates fields
- **REQ-004**: `Response.Type.ts` defined independently from schemas (API shapes differ from form shapes)
- **REQ-005**: `Query.Type.ts` extends `ServerQueryingParameters` for entities with list endpoints
- **REQ-006**: All imports updated — no stale `*.domain.types.ts` or `*.request.types.ts` survive
- **REQ-007**: File naming: `{Entity}.Schema.ts`, `{Entity}.Parameters.Type.ts`, `{Entity}.Request.Type.ts`, `{Entity}.Response.Type.ts`, `{Entity}.Query.Type.ts`
- **REQ-008**: `pnpm run type-check` passes after each phase; `pnpm run test:unit` passes (pre-existing failures excluded)
- **REQ-009**: Every schema field has a validation message string; messages are user-facing, concise, specific to the constraint
- **CON-001**: Schema files co-located in per-entity `schemas/` directories; type files in per-entity `types/` directories
- **CON-002**: `Request.Type.ts` import of `{Entity}Parameters` is an `import type`, not a value import
- **CON-003**: `z.infer` is used in Parameters files, not in Schema files (Schema files export the inferred type directly)
- **PAT-001**: `Update{Entity}Request` aliases `Create{Entity}Request` or uses `Partial` when update allows omission
- **PAT-002**: Enum option arrays (e.g., `PropertyKindOptions`) live in the entity's `Response.Type.ts` not in schemas
- **PAT-003**: Entities without forms (e.g., Permission, InventoryUnit) still get `Parameters.Type.ts` from schema for consistency

## 2. Implementation Steps

### Phase 1: Auth — Login & ChangePassword

- GOAL-001: Restructure Auth module into per-schema Login and ChangePassword entities

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Rename `auth/schemas/auth.schema.ts` → `schemas/Login.Schema.ts` + `schemas/ChangePassword.Schema.ts` with per-field messages | | |
| TASK-002 | Create `auth/types/Login.Parameters.Type.ts`, `Login.Request.Type.ts`, `Login.Response.Type.ts` | | |
| TASK-003 | Create `auth/types/ChangePassword.Parameters.Type.ts`, `ChangePassword.Request.Type.ts` | | |
| TASK-004 | Delete old `auth.schema.ts`, `auth.model.types.ts`, `auth.types.ts`; update all consumers | | |

#### Login.Schema.ts

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

#### ChangePassword.Schema.ts

```typescript
import { z } from 'zod'

export const ChangePasswordSchema = z
  .object({
    currentPassword: z
      .string()
      .min(1, 'Current password is required')
      .max(128, 'Password must not exceed 128 characters'),
    newPassword: z
      .string()
      .min(6, 'New password must be at least 6 characters')
      .max(128, 'New password must not exceed 128 characters'),
    confirmNewPassword: z
      .string()
      .min(1, 'Please confirm your new password'),
  })
  .refine((data) => data.newPassword === data.confirmNewPassword, {
    message: 'Passwords do not match',
    path: ['confirmNewPassword'],
  })

export type ChangePasswordParameters = z.infer<typeof ChangePasswordSchema>
```

#### Login.Request.Type.ts

```typescript
import type { LoginParameters } from '../schemas/Login.Schema'

export type LoginRequest = LoginParameters & {
  ipAddress?: string
}

export interface RefreshRequest {
  refreshToken: string
  rememberMe?: boolean
  ipAddress?: string
}
```

#### Login.Response.Type.ts

```typescript
export interface AuthenticationResponse {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export interface UserProfile {
  id: string
  email: string
  fullName: string
  roles: string[]
}
```

#### ChangePassword.Request.Type.ts

```typescript
import type { ChangePasswordParameters } from '../schemas/ChangePassword.Schema'

export type ChangePasswordRequest = ChangePasswordParameters
```

---

### Phase 2: Catalog — Option Types & Option Values

- GOAL-002: Restructure OptionType and child OptionValue entities

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Rename `option-types/schemas/option-type.schema.ts` → `schemas/OptionType.Schema.ts` plus type files | | |
| TASK-006 | Rename `option-values/schemas/option-value.schema.ts` → `schemas/OptionValue.Schema.ts` plus type files | | |
| TASK-007 | Delete old files; update all imports across option-types/ services, stores, views, tests, repository | | |

#### OptionType.Schema.ts

```typescript
import { z } from 'zod'

export const OptionTypeSchema = z.object({
  name: z
    .string()
    .min(1, 'Name is required')
    .max(100, 'Name must not exceed 100 characters'),
  presentation: z
    .string()
    .min(1, 'Presentation is required')
    .max(100, 'Presentation must not exceed 100 characters'),
  description: z
    .string()
    .max(500, 'Description must not exceed 500 characters')
    .optional()
    .nullable(),
  filterable: z.boolean().default(false),
  position: z
    .number()
    .int('Position must be a whole number')
    .min(0, 'Position must be non-negative')
    .default(0),
})

export type OptionTypeParameters = z.infer<typeof OptionTypeSchema>
```

#### OptionValue.Schema.ts

```typescript
import { z } from 'zod'

export const OptionValueSchema = z.object({
  name: z
    .string()
    .min(1, 'Internal name is required')
    .max(100, 'Name must not exceed 100 characters'),
  presentation: z
    .string()
    .min(1, 'Display name is required')
    .max(100, 'Display name must not exceed 100 characters'),
  position: z
    .number()
    .int('Position must be a whole number')
    .min(0, 'Position must be non-negative')
    .default(0),
})

export type OptionValueParameters = z.infer<typeof OptionValueSchema>
```

#### OptionType.Request.Type.ts

```typescript
import type { OptionTypeParameters } from '../schemas/OptionType.Schema'

export type CreateOptionTypeRequest = OptionTypeParameters
export type UpdateOptionTypeRequest = OptionTypeParameters
```

#### OptionType.Response.Type.ts

```typescript
export interface OptionTypeListItem {
  id: string
  name: string
  presentation: string
  position: number
  filterable: boolean
  optionValuesCount: number
  productsCount: number
  createdAtUtc: string
  modifiedAtUtc: string
}

export type OptionTypeDetail = OptionTypeListItem
```

#### OptionType.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type OptionTypeQuery = ServerQueryingParameters
```

#### OptionValue.Request.Type.ts

```typescript
import type { OptionValueParameters } from '../schemas/OptionValue.Schema'

export type CreateOptionValueRequest = OptionValueParameters & {
  optionTypeId: string
}

export type UpdateOptionValueRequest = OptionValueParameters & {
  optionTypeId?: string
}
```

#### OptionValue.Response.Type.ts

```typescript
export interface OptionValueListItem {
  id: string
  optionTypeId: string
  name: string
  presentation: string
  position: number
}
```

#### OptionValue.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface OptionValueQuery extends ServerQueryingParameters {
  optionTypeId?: string
}
```

---

### Phase 3: Catalog — Property Types

- GOAL-003: Restructure PropertyType entity

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Rename `property-types/schemas/property-type.schema.ts` → `schemas/PropertyType.Schema.ts` plus type files | | |
| TASK-009 | Delete old files; update all imports across property-types/ | | |

#### PropertyType.Schema.ts

```typescript
import { z } from 'zod'

export const PropertyKindEnum = z.enum(['String', 'Integer', 'Float', 'Boolean', 'Date', 'Html'])

export const PropertyTypeSchema = z.object({
  name: z
    .string()
    .min(1, 'Name is required')
    .max(100, 'Name must not exceed 100 characters'),
  presentation: z
    .string()
    .min(1, 'Presentation is required')
    .max(100, 'Presentation must not exceed 100 characters'),
  kind: PropertyKindEnum.default('String'),
  position: z
    .number()
    .int('Position must be a whole number')
    .min(0, 'Position must be non-negative')
    .default(0),
  filterable: z.boolean().default(false),
})

export type PropertyTypeParameters = z.infer<typeof PropertyTypeSchema>
```

#### PropertyType.Request.Type.ts

```typescript
import type { PropertyTypeParameters } from '../schemas/PropertyType.Schema'

export type CreatePropertyTypeRequest = PropertyTypeParameters & {
  publicMetadata?: Record<string, unknown>
  privateMetadata?: Record<string, unknown>
}

export type UpdatePropertyTypeRequest = PropertyTypeParameters & {
  publicMetadata?: Record<string, unknown>
  privateMetadata?: Record<string, unknown>
}
```

#### PropertyType.Response.Type.ts

```typescript
export const PropertyKindOptions = [
  { label: 'String', value: 'String' },
  { label: 'Integer', value: 'Integer' },
  { label: 'Float', value: 'Float' },
  { label: 'Boolean', value: 'Boolean' },
  { label: 'Date', value: 'Date' },
  { label: 'HTML', value: 'Html' },
] as const

export interface PropertyTypeListItem {
  id: string
  name: string
  presentation: string
  kind: string
  position: number
  filterable: boolean
  publicMetadata?: Record<string, unknown>
  privateMetadata?: Record<string, unknown>
}

export type PropertyTypeDetail = PropertyTypeListItem
```

#### PropertyType.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type PropertyTypeQuery = ServerQueryingParameters
```

---

### Phase 4: Catalog — Products & Variants

- GOAL-004: Restructure Product and Variant entities (most complex, multiple existing schema files)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Consolidate `product.schema.ts` + `product.schemas.ts` → `schemas/CreateProduct.Schema.ts`, `schemas/UpdateProduct.Schema.ts`, `schemas/ProductClassification.Schema.ts`, `schemas/Variant.Schema.ts` | | |
| TASK-011 | Create type files for Product, Variant, ProductClassification | | |
| TASK-012 | Delete old type files; update all imports across products/ | | |

#### CreateProduct.Schema.ts

```typescript
import { z } from 'zod'

export const CreateProductSchema = z.object({
  name: z
    .string()
    .min(1, 'Name is required')
    .max(200, 'Name must not exceed 200 characters'),
  slug: z
    .string()
    .min(1, 'Slug is required')
    .max(200, 'Slug must not exceed 200 characters')
    .regex(/^[a-z0-9-]+$/, 'Slug may only contain lowercase letters, numbers, and hyphens'),
  description: z.string().optional(),
  price: z
    .number()
    .min(0, 'Price must be non-negative'),
  sku: z
    .string()
    .max(100, 'SKU must not exceed 100 characters')
    .optional(),
  availableOn: z.string().optional(),
  discontinueOn: z.string().optional(),
  trackInventory: z.boolean().default(true),
  weight: z.number().min(0, 'Weight must be non-negative').optional().nullable(),
  height: z.number().min(0, 'Height must be non-negative').optional().nullable(),
  width: z.number().min(0, 'Width must be non-negative').optional().nullable(),
  depth: z.number().min(0, 'Depth must be non-negative').optional().nullable(),
  metaTitle: z
    .string()
    .max(60, 'Meta title must not exceed 60 characters')
    .optional()
    .nullable(),
  metaDescription: z
    .string()
    .max(160, 'Meta description must not exceed 160 characters')
    .optional()
    .nullable(),
  metaKeywords: z
    .string()
    .max(255, 'Meta keywords must not exceed 255 characters')
    .optional()
    .nullable(),
})

export type CreateProductParameters = z.infer<typeof CreateProductSchema>
```

#### UpdateProduct.Schema.ts

```typescript
import { CreateProductSchema } from './CreateProduct.Schema'

export const UpdateProductSchema = CreateProductSchema.partial()

export type UpdateProductParameters = z.infer<typeof UpdateProductSchema>
```

#### ProductClassification.Schema.ts

```typescript
import { z } from 'zod'

export const ManageClassificationsSchema = z.object({
  taxonIds: z
    .array(z.string().uuid('Invalid taxon ID'))
    .min(1, 'At least one taxon must be selected'),
  mainTaxonId: z
    .string()
    .uuid('Invalid taxon ID')
    .optional()
    .nullable(),
})

export type ManageClassificationsParameters = z.infer<typeof ManageClassificationsSchema>
```

#### Variant.Schema.ts

```typescript
import { z } from 'zod'

export const VariantSchema = z.object({
  sku: z
    .string()
    .min(1, 'SKU is required')
    .max(100, 'SKU must not exceed 100 characters'),
  barcode: z
    .string()
    .max(50, 'Barcode must not exceed 50 characters')
    .optional(),
  price: z
    .number()
    .min(0, 'Price must be non-negative')
    .default(0),
  compareAtPrice: z
    .number()
    .min(0, 'Compare-at price must be non-negative')
    .optional()
    .nullable(),
  costPrice: z
    .number()
    .min(0, 'Cost price must be non-negative')
    .optional()
    .nullable(),
  position: z
    .number()
    .int('Position must be a whole number')
    .min(0, 'Position must be non-negative')
    .default(0),
  trackInventory: z.boolean().default(true),
  weight: z.number().min(0, 'Weight must be non-negative').optional().nullable(),
  height: z.number().min(0, 'Height must be non-negative').optional().nullable(),
  width: z.number().min(0, 'Width must be non-negative').optional().nullable(),
  depth: z.number().min(0, 'Depth must be non-negative').optional().nullable(),
  optionValueIds: z.array(z.string().uuid()).optional(),
})

export type VariantParameters = z.infer<typeof VariantSchema>
```

#### Product.Request.Type.ts

```typescript
import type { CreateProductParameters } from '../schemas/CreateProduct.Schema'
import type { UpdateProductParameters } from '../schemas/UpdateProduct.Schema'
import type { ManageClassificationsParameters } from '../schemas/ProductClassification.Schema'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type CreateProductRequest = CreateProductParameters
export type UpdateProductRequest = UpdateProductParameters
export type ManageClassificationsRequest = ManageClassificationsParameters

export interface ProductSearchParams extends ServerQueryingParameters {
  status?: string
  taxonId?: string
  season?: string
}
```

#### Product.Response.Type.ts

```typescript
export type ProductStatus = 'Draft' | 'Active' | 'Archived'

export interface ProductImage {
  id: string
  productId: string
  variantId: string | null
  url: string
  alt: string | null
  position: number
  role: number
  fileSize: number | null
  width: number | null
  height: number | null
  isDefault: boolean
}

export interface ProductClassification {
  id: string
  productId: string
  taxonId: string
  position: number
  isAutomatic: boolean
  isMain: boolean
  taxonName?: string
  taxonomyName?: string
}

export interface ProductProperty {
  id: string
  propertyTypeId: string
  propertyTypeName: string
  propertyTypePresentation: string
  value: string
}

export interface ProductSummary {
  id: string
  name: string
  slug: string
  description: string | null
  sku: string | null
  price: number
  status: ProductStatus
  imageUrl: string | null
  variantsCount: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export interface ProductDetail extends ProductSummary {
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
  weight: number | null
  height: number | null
  width: number | null
  depth: number | null
  variants: VariantSummary[]
  classifications: ProductClassification[]
  properties: ProductProperty[]
  images: ProductImage[]
}

import type { VariantSummary } from './Variant.Response.Type'
```

#### Product.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface ProductQuery extends ServerQueryingParameters {
  status?: string
  taxonId?: string
  season?: string
}
```

#### Variant.Request.Type.ts

```typescript
import type { VariantParameters } from '../schemas/Variant.Schema'

export type CreateVariantRequest = VariantParameters & {
  productId?: string
}

export type UpdateVariantRequest = Partial<CreateVariantRequest>
```

#### Variant.Response.Type.ts

```typescript
export interface VariantOption {
  name: string
  value: string
}

export interface VariantSummary {
  id: string
  productId: string
  sku: string | null
  barcode: string | null
  price: number
  compareAtPrice: number | null
  costPrice: number | null
  costCurrency?: string
  isMaster: boolean
  position: number
  trackInventory: boolean
  weightUnit?: string
  dimensionsUnit?: string
  options: VariantOption[]
}

export interface VariantDetail extends VariantSummary {
  weight: number | null
  height: number | null
  width: number | null
  depth: number | null
  optionValueIds: string[]
}
```

#### Variant.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type VariantQuery = ServerQueryingParameters
```

---

### Phase 5: Location — Country & State

- GOAL-005: Restructure Location module into per-entity Country and State

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Rename `country.schema.ts` → `schemas/Country.Schema.ts`; rename `state.schema.ts` → `schemas/State.Schema.ts` | | |
| TASK-014 | Create type files for Country and State | | |
| TASK-015 | Delete old location `types/` flat files; update all imports across location/ | | |

#### Country.Schema.ts

```typescript
import { z } from 'zod'

export const CountrySchema = z.object({
  name: z
    .string()
    .min(1, 'Country name is required')
    .max(100, 'Country name must not exceed 100 characters'),
  isoCode: z
    .string()
    .length(2, 'ISO code must be exactly 2 characters')
    .toUpperCase(),
  callingCode: z
    .string()
    .max(10, 'Calling code must not exceed 10 characters')
    .default(''),
  isActive: z.boolean().default(true),
})

export type CountryParameters = z.infer<typeof CountrySchema>
```

#### State.Schema.ts

```typescript
import { z } from 'zod'

export const StateSchema = z.object({
  name: z
    .string()
    .min(1, 'State name is required')
    .max(100, 'State name must not exceed 100 characters'),
  abbreviation: z
    .string()
    .min(1, 'Abbreviation is required')
    .max(10, 'Abbreviation must not exceed 10 characters'),
  countryId: z
    .string()
    .uuid('Invalid country')
    .min(1, 'Country is required'),
  isActive: z.boolean().default(true),
})

export type StateParameters = z.infer<typeof StateSchema>
```

#### Country.Request.Type.ts

```typescript
import type { CountryParameters } from '../schemas/Country.Schema'

export type CreateCountryRequest = CountryParameters
export type UpdateCountryRequest = CountryParameters
```

#### Country.Response.Type.ts

```typescript
export interface Country {
  id: string
  name: string
  isoCode: string
  callingCode: string
  isActive: boolean
  statesRequired?: boolean
  zipcodeRequired?: boolean
  createdAtUtc?: string
  modifiedAtUtc?: string
}
```

#### Country.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type CountryQuery = ServerQueryingParameters
```

#### State.Request.Type.ts

```typescript
import type { StateParameters } from '../schemas/State.Schema'

export type CreateStateRequest = StateParameters
export type UpdateStateRequest = StateParameters
```

#### State.Response.Type.ts

```typescript
export interface State {
  id: string
  name: string
  abbreviation: string
  countryId: string
  countryName?: string
  isActive: boolean
}
```

#### State.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type StateQuery = ServerQueryingParameters
```

---

### Phase 6: Ordering — Order & Fulfillment

- GOAL-006: Create schemas for Order (editing) and Fulfillment; restructure order types

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Create `ordering/schemas/Order.Schema.ts`, `ordering/fulfillment/schemas/Fulfillment.Schema.ts` | | |
| TASK-017 | Create type files for Order and Fulfillment | | |
| TASK-018 | Delete old `order.domain.types.ts`, `order.request.types.ts`; update all imports | | |

#### Order.Schema.ts

```typescript
import { z } from 'zod'

export const AddressSchema = z.object({
  firstName: z.string().min(1, 'First name is required').max(100),
  lastName: z.string().min(1, 'Last name is required').max(100),
  address1: z.string().min(1, 'Address is required').max(200),
  address2: z.string().max(200).optional(),
  city: z.string().min(1, 'City is required').max(100),
  zipCode: z.string().min(1, 'ZIP code is required').max(20),
  countryCode: z.string().length(2, 'Country code must be 2 characters'),
  stateCode: z.string().max(10).optional(),
  phone: z.string().max(30).optional(),
  company: z.string().max(100).optional(),
})

export const LineItemSchema = z.object({
  variantId: z.string().uuid('Invalid variant'),
  quantity: z.number().int('Quantity must be a whole number').min(1, 'Quantity must be at least 1'),
})

export const OrderSchema = z.object({
  email: z
    .string()
    .email('Invalid email format')
    .min(1, 'Email is required'),
  currency: z
    .string()
    .length(3, 'Currency must be a 3-letter code')
    .default('USD'),
  lineItems: z
    .array(LineItemSchema)
    .min(1, 'At least one item is required'),
  shippingAddress: AddressSchema.optional(),
  billingAddress: AddressSchema.optional(),
})

export type OrderParameters = z.infer<typeof OrderSchema>
```

#### Fulfillment.Schema.ts

```typescript
import { z } from 'zod'

export const FulfillmentSchema = z.object({
  trackingNumber: z
    .string()
    .max(100, 'Tracking number must not exceed 100 characters')
    .optional(),
  stockLocationId: z
    .string()
    .uuid('Invalid stock location')
    .min(1, 'Stock location is required'),
  inventoryUnitIds: z
    .array(z.string().uuid('Invalid inventory unit'))
    .min(1, 'At least one unit must be selected'),
})

export type FulfillmentParameters = z.infer<typeof FulfillmentSchema>
```

#### Order.Request.Type.ts

```typescript
import type { OrderParameters } from '../schemas/Order.Schema'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type CreateOrderRequest = OrderParameters

export interface AddOrderItemRequest {
  variantId: string
  quantity: number
}

export interface UpdateAddressesRequest {
  shippingAddress?: Partial<OrderParameters['shippingAddress']>
  billingAddress?: Partial<OrderParameters['billingAddress']>
}

export interface CancelOrderRequest {
  reason?: string
}

export interface OrderSearchParams extends ServerQueryingParameters {
  state?: string
  storeId?: string
  warehouseId?: string
  fromDate?: string
  toDate?: string
}
```

#### Order.Response.Type.ts

```typescript
export interface OrderListItem {
  id: string
  number: string
  state: string
  currency: string
  totalCents: number
  totalDisplay: string
  email?: string
  paymentState?: string
  shipmentState?: string
  createdAtUtc: string
}

export interface AddressDetail {
  id: string
  firstName: string
  lastName: string
  address1: string
  address2?: string
  city: string
  zipCode: string
  countryCode: string
  stateCode?: string
  phone?: string
  company?: string
}

export interface InventoryUnitDetail {
  id: string
  sku: string
  state: string
  serialNumber?: string
  pending: boolean
}

export interface LineItemDetail {
  id: string
  variantId: string
  name: string
  sku: string
  quantity: number
  unitPriceCents: number
  unitPriceDisplay: string
  totalCents: number
  totalDisplay: string
  inventoryUnits: InventoryUnitDetail[]
}

export interface PaymentDetail {
  id: string
  amountCents: number
  amountDisplay: string
  state: string
  methodType: string
  transactionId?: string
  createdAtUtc: string
}

export interface ShipmentDetail {
  id: string
  number: string
  state: string
  trackingNumber?: string
  stockLocationId: string
  stockLocationName?: string
  units: InventoryUnitDetail[]
}

export interface OrderHistoryDetail {
  description: string
  fromState?: string
  toState: string
  triggeredBy?: string
  createdAtUtc: string
  context: Record<string, unknown>
}

export interface OrderDetail extends OrderListItem {
  itemTotalCents: number
  itemTotalDisplay: string
  shipmentTotalCents: number
  shipmentTotalDisplay: string
  lineItems: LineItemDetail[]
  payments: PaymentDetail[]
  shipments: ShipmentDetail[]
  history: OrderHistoryDetail[]
  shippingAddress?: AddressDetail
  billingAddress?: AddressDetail
}
```

#### Order.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface OrderQuery extends ServerQueryingParameters {
  state?: string
  storeId?: string
  warehouseId?: string
  fromDate?: string
  toDate?: string
}
```

#### Fulfillment.Request.Type.ts

```typescript
import type { FulfillmentParameters } from '../schemas/Fulfillment.Schema'

export type CreateFulfillmentRequest = FulfillmentParameters

export interface RefundPaymentRequest {
  amountCents: number
  reason: string
}
```

#### Fulfillment.Response.Type.ts

```typescript
export interface Fulfillment {
  id: string
  shipmentId: string
  trackingNumber?: string
  state: string
  createdAtUtc: string
}
```

---

### Phase 7: Users — User, Role & Permission

- GOAL-007: Create schemas for User, Role, Permission entities; restructure flat users/types/

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | Create `users/schemas/User.Schema.ts`, `users/roles/schemas/Role.Schema.ts`, `users/permissions/schemas/Permission.Schema.ts` | | |
| TASK-020 | Create type files for User, Role, Permission | | |
| TASK-021 | Delete old `users/types/user.domain.types.ts`, `user.request.types.ts`; update all imports across users/, identity/ | | |

#### User.Schema.ts

```typescript
import { z } from 'zod'

export const UserSchema = z.object({
  email: z
    .string()
    .email('Invalid email format')
    .min(1, 'Email is required')
    .max(255, 'Email must not exceed 255 characters'),
  firstName: z
    .string()
    .min(1, 'First name is required')
    .max(100, 'First name must not exceed 100 characters'),
  lastName: z
    .string()
    .min(1, 'Last name is required')
    .max(100, 'Last name must not exceed 100 characters'),
  role: z
    .array(z.string())
    .min(1, 'At least one role must be assigned'),
  password: z
    .string()
    .min(6, 'Password must be at least 6 characters')
    .max(128, 'Password must not exceed 128 characters')
    .optional(),
  phoneNumber: z
    .string()
    .max(30, 'Phone number must not exceed 30 characters')
    .optional(),
  emailConfirmed: z.boolean().optional(),
  isActive: z.boolean().optional().default(true),
})

export type UserParameters = z.infer<typeof UserSchema>
```

#### Role.Schema.ts

```typescript
import { z } from 'zod'

export const RoleSchema = z.object({
  name: z
    .string()
    .min(1, 'Role name is required')
    .max(100, 'Role name must not exceed 100 characters')
    .regex(/^[a-z_]+$/, 'Role name may only contain lowercase letters and underscores'),
  displayName: z
    .string()
    .max(100, 'Display name must not exceed 100 characters')
    .optional(),
  description: z
    .string()
    .max(500, 'Description must not exceed 500 characters')
    .optional(),
  priority: z
    .number()
    .int('Priority must be a whole number')
    .min(0, 'Priority must be non-negative')
    .default(0),
})

export type RoleParameters = z.infer<typeof RoleSchema>
```

#### Permission.Schema.ts

```typescript
import { z } from 'zod'

export const PermissionSchema = z.object({
  identifier: z
    .string()
    .min(1, 'Identifier is required')
    .max(200, 'Identifier must not exceed 200 characters')
    .regex(/^[a-z][a-z0-9_.]+$/, 'Identifier format: lowercase letters, numbers, underscores, dots'),
  name: z
    .string()
    .min(1, 'Name is required')
    .max(100, 'Name must not exceed 100 characters'),
  description: z
    .string()
    .max(500, 'Description must not exceed 500 characters')
    .optional(),
  action: z
    .string()
    .min(1, 'Action is required')
    .max(100, 'Action must not exceed 100 characters'),
})

export type PermissionParameters = z.infer<typeof PermissionSchema>
```

#### User.Request.Type.ts

```typescript
import type { UserParameters } from '../schemas/User.Schema'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type CreateAdminUserRequest = UserParameters

export type UpdateAdminUserRequest = Partial<CreateAdminUserRequest>

export interface UserSearchParams extends ServerQueryingParameters {
  isActive?: boolean
  role?: string
}
```

#### User.Response.Type.ts

```typescript
export interface AdminUserSummary {
  id: string
  email: string
  userName: string | null
  firstName: string | null
  lastName: string | null
  fullName: string | null
  roleNames: string[]
  isActive: boolean
  createdAtUtc: string
  phoneNumber?: string | null
  emailConfirmed?: boolean
  phoneNumberConfirmed?: boolean
  accessFailedCount?: number
  lockoutEnd?: string | null
  lastSignInAtUtc?: string | null
  lastIpAddress?: string | null
}

export interface CustomerSummary {
  id: string
  email: string
  firstName: string | null
  lastName: string | null
  fullName: string | null
  ordersCount: number
  totalSpent: number
  isActive: boolean
  createdAtUtc: string
}
```

#### User.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface UserQuery extends ServerQueryingParameters {
  isActive?: boolean
  role?: string
}
```

#### Role.Request.Type.ts

```typescript
import type { RoleParameters } from '../schemas/Role.Schema'

export type CreateRoleRequest = RoleParameters
export type UpdateRoleRequest = Partial<RoleParameters>
```

#### Role.Response.Type.ts

```typescript
export interface RoleSummary {
  id: string
  name: string
  displayName: string | null
  description: string | null
  priority: number
  isSystem: boolean
  isDefault: boolean
  userCount: number
}
```

#### Role.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type RoleQuery = ServerQueryingParameters
```

#### Permission.Request.Type.ts

```typescript
import type { PermissionParameters } from '../schemas/Permission.Schema'

export type CreatePermissionRequest = PermissionParameters
export type UpdatePermissionRequest = Partial<PermissionParameters>
```

#### Permission.Response.Type.ts

```typescript
export interface PermissionSummary {
  identifier: string
  name: string
  description: string | null
  action: string
}
```

#### Permission.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type PermissionQuery = ServerQueryingParameters
```

---

### Phase 8: Inventories — StockLocation, StockTransfer, StockItem, InventoryUnit, StockMovement

- GOAL-008: Create schemas for all inventory sub-entities; restructure flat inventories/types/

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Create 5 schema files for inventory sub-entities | | |
| TASK-023 | Create type files for each inventory entity | | |
| TASK-024 | Delete old flat `inventory.domain.types.ts`, `inventory.request.types.ts`, `inventory.response.types.ts`; update all imports | | |

#### StockLocation.Schema.ts

```typescript
import { z } from 'zod'

export const StockLocationSchema = z.object({
  name: z
    .string()
    .min(1, 'Location name is required')
    .max(100, 'Location name must not exceed 100 characters'),
  code: z
    .string()
    .min(1, 'Location code is required')
    .max(50, 'Location code must not exceed 50 characters')
    .regex(/^[A-Z0-9_-]+$/, 'Code may contain only uppercase letters, numbers, hyphens, underscores'),
  type: z
    .number()
    .int('Type must be a whole number')
    .min(0, 'Type is required'),
  isDefault: z.boolean().default(false),
  active: z.boolean().default(true),
  address1: z.string().min(1, 'Address is required').max(200),
  address2: z.string().max(200).optional(),
  city: z.string().min(1, 'City is required').max(100),
  zipCode: z.string().min(1, 'ZIP code is required').max(20),
  countryCode: z.string().length(2, 'Country code must be 2 characters'),
  stateCode: z.string().max(10).optional(),
  phone: z.string().max(30).optional(),
  backorderableDefault: z.boolean().optional().default(false),
  propagateAllVariants: z.boolean().optional().default(false),
  lowStockThreshold: z.number().int().min(0).optional(),
  notifyOnLowStock: z.boolean().optional().default(false),
  position: z.number().int().min(0).optional().default(0),
})

export type StockLocationParameters = z.infer<typeof StockLocationSchema>
```

#### StockTransfer.Schema.ts

```typescript
import { z } from 'zod'

export const StockTransferSchema = z.object({
  sourceLocationId: z
    .string()
    .uuid('Invalid source location')
    .min(1, 'Source location is required'),
  destinationLocationId: z
    .string()
    .uuid('Invalid destination location')
    .min(1, 'Destination location is required'),
  reason: z
    .string()
    .max(500, 'Reason must not exceed 500 characters')
    .optional(),
  items: z
    .array(z.object({
      variantId: z.string().uuid('Invalid variant'),
      quantity: z.number().int().min(1, 'Quantity must be at least 1'),
    }))
    .min(1, 'At least one item is required'),
})

export type StockTransferParameters = z.infer<typeof StockTransferSchema>
```

#### StockItem.Schema.ts

```typescript
import { z } from 'zod'

export const StockAdjustmentSchema = z.object({
  quantity: z
    .number()
    .int('Quantity must be a whole number'),
  type: z
    .number()
    .int('Type is required')
    .min(0, 'Invalid adjustment type'),
  reason: z
    .string()
    .max(500, 'Reason must not exceed 500 characters')
    .optional(),
  reference: z
    .string()
    .max(100, 'Reference must not exceed 100 characters')
    .optional(),
})

export type StockAdjustmentParameters = z.infer<typeof StockAdjustmentSchema>
```

#### InventoryUnit.Schema.ts

```typescript
import { z } from 'zod'

export const InventoryUnitSchema = z.object({
  stockItemId: z.string().uuid('Invalid stock item').min(1, 'Stock item is required'),
  serialNumber: z.string().max(100).optional().nullable(),
  state: z.number().int().min(0, 'State is required'),
  orderId: z.string().uuid().optional().nullable(),
  shipmentId: z.string().uuid().optional().nullable(),
})

export type InventoryUnitParameters = z.infer<typeof InventoryUnitSchema>
```

#### StockMovement.Schema.ts

```typescript
import { z } from 'zod'

export const StockMovementSchema = z.object({
  stockItemId: z.string().uuid('Invalid stock item').min(1, 'Stock item is required'),
  quantity: z.number().int('Quantity must be a whole number'),
  reason: z.string().max(500).optional(),
  reference: z.string().max(100).optional(),
})

export type StockMovementParameters = z.infer<typeof StockMovementSchema>
```

#### StockLocation.Request.Type.ts

```typescript
import type { StockLocationParameters } from '../schemas/StockLocation.Schema'

export type CreateStockLocationRequest = StockLocationParameters
export type UpdateStockLocationRequest = Partial<StockLocationParameters>
```

#### StockLocation.Response.Type.ts

```typescript
export interface StockLocation {
  id: string
  name: string
  code: string
  active: boolean
  isDefault: boolean
  type: string
  city: string
  countryCode: string
  position?: number
  backorderableDefault?: boolean
  propagateAllVariants?: boolean
  lowStockThreshold?: number
  notifyOnLowStock?: boolean
}

export interface StockLocationDetail extends StockLocation {
  presentation: string | null
  address: {
    address1: string
    address2: string | null
    city: string
    zipCode: string
    countryCode: string
    stateCode: string | null
    phone: string | null
    firstName: string | null
    lastName: string | null
    company: string | null
  }
  publicMetadata: Record<string, unknown>
  privateMetadata: Record<string, unknown>
}
```

#### StockLocation.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type StockLocationQuery = ServerQueryingParameters
```

#### StockTransfer.Request.Type.ts

```typescript
import type { StockTransferParameters } from '../schemas/StockTransfer.Schema'

export type CreateStockTransferRequest = StockTransferParameters
```

#### StockTransfer.Response.Type.ts

```typescript
export type TransferState = 'Draft' | 'InTransit' | 'Received' | 'Canceled'

export interface StockTransfer {
  id: string
  number: string
  referenceNumber: string
  sourceLocationId: string
  sourceLocationName: string
  destinationLocationId: string
  destinationLocationName: string
  state: TransferState
  createdAtUtc: string
}

export interface StockTransferItem {
  variantId: string
  sku: string
  variantName: string
  quantity: number
}

export interface StockTransferDetail extends StockTransfer {
  reason: string | null
  items: StockTransferItem[]
}
```

#### StockTransfer.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type StockTransferQuery = ServerQueryingParameters
```

#### StockItem.Response.Type.ts

```typescript
export interface StockItem {
  id: string
  variantId: string
  sku: string
  variantName: string
  stockLocationId: string
  stockLocationName: string
  countOnHand: number
  quantityReserved?: number
  countAvailable?: number
  backorderable: boolean
}

export interface StockItemDetail extends StockItem {
  backorderLimit: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}
```

#### StockItem.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface StockItemQuery extends ServerQueryingParameters {
  lowStock?: boolean
}
```

#### InventoryUnit.Response.Type.ts

```typescript
export type ReservationState = 'Reserved' | 'Fulfilled' | 'Released' | 'Expired'

export interface InventoryUnit {
  id: string
  stockItemId: string
  sku: string
  serialNumber: string | null
  state: ReservationState
  orderId: string | null
  shipmentId: string | null
  createdAtUtc: string
}
```

#### InventoryUnit.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface InventoryUnitQuery extends ServerQueryingParameters {
  stockItemId?: string
  orderId?: string
  shipmentId?: string
  state?: number
}
```

#### StockMovement.Response.Type.ts

```typescript
export interface StockMovement {
  id: string
  stockItemId: string
  action: string
  quantity: number
  previousCountOnHand: number
  reason: string | null
  reference: string | null
  createdAtUtc: string
  createdBy: string | null
}
```

#### StockMovement.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface StockMovementQuery extends ServerQueryingParameters {
  stockItemId?: string
  type?: number
}
```

---

### Phase 9: Profile & Reports

- GOAL-009: Create schemas for Profile and Report entities

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | Create `profile/schemas/Profile.Schema.ts`, `reports/schemas/Report.Schema.ts` | | |
| TASK-026 | Create type files for Profile and Report | | |
| TASK-027 | Delete old type files; update all imports | | |

#### Profile.Schema.ts

```typescript
import { z } from 'zod'

export const ProfileSchema = z.object({
  firstName: z
    .string()
    .min(1, 'First name is required')
    .max(100, 'First name must not exceed 100 characters'),
  lastName: z
    .string()
    .min(1, 'Last name is required')
    .max(100, 'Last name must not exceed 100 characters'),
  phoneNumber: z
    .string()
    .max(30, 'Phone number must not exceed 30 characters')
    .optional(),
  dateOfBirth: z.string().optional(),
  gender: z.string().max(50).optional(),
  bio: z
    .string()
    .max(1000, 'Bio must not exceed 1000 characters')
    .optional(),
  avatarUrl: z.string().url('Invalid URL').optional().nullable(),
  acceptsEmailMarketing: z.boolean().optional(),
})

export type ProfileParameters = z.infer<typeof ProfileSchema>
```

#### Report.Schema.ts

```typescript
import { z } from 'zod'

export const DashboardQuerySchema = z.object({
  from: z.string().optional(),
  to: z.string().optional(),
})

export type DashboardQueryParameters = z.infer<typeof DashboardQuerySchema>
```

#### Profile.Request.Type.ts

```typescript
import type { ProfileParameters } from '../schemas/Profile.Schema'

export type ProfileUpdateRequest = ProfileParameters

export interface ProfileUpdateRequest {
  preferences?: ProfilePreferences
  notifications?: NotificationPreferences
}

import type { ProfilePreferences, NotificationPreferences } from './Profile.Response.Type'
```

#### Profile.Response.Type.ts

```typescript
export interface Profile {
  id: string
  email: string
  firstName: string
  lastName: string
  phoneNumber?: string
  dateOfBirth?: string
  gender?: string
  bio?: string
  avatarUrl?: string
  preferences?: ProfilePreferences
  notifications?: NotificationPreferences
  isActive: boolean
  acceptsEmailMarketing?: boolean
  createdAtUtc: string
  modifiedAtUtc?: string
}

export interface ProfilePreferences {
  preferredStyle?: string
  preferredFit?: string
  favoriteColors?: string[]
  favoriteCategories?: string[]
  preferredBrands?: string[]
  sizeTop?: string
  sizeBottom?: string
  shoeSize?: string
}

export interface NotificationPreferences {
  enableSms?: boolean
  enableEmail?: boolean
  enableNewsfeeds?: boolean
}
```

#### Profile.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export type ProfileQuery = ServerQueryingParameters
```

#### Report.Response.Type.ts

```typescript
export interface SalesSummary {
  totalRevenue: number
  orderCount: number
  averageOrderValue: number
  revenueTrendPercentage: number
  trendHistory?: Array<{ date: string; revenue: number }>
}

export interface InventorySummary {
  totalVariants: number
  outOfStockCount: number
  lowStockCount: number
  stockAccuracyPercentage: number
}

export interface RecentProduct {
  id: string
  name: string
  slug: string
  createdAtUtc: string
}

export interface CatalogSummary {
  totalProducts: number
  activeProducts: number
  totalVariants: number
  totalTaxonomies: number
  totalTaxons: number
  recentlyAdded: RecentProduct[]
}

export interface ActivityItem {
  id: string
  type: 'Order' | 'Stock'
  title: string
  description: string
  status: string
  timestamp: string
}

export interface RecentActivityResponse {
  items: ActivityItem[]
}
```

#### Report.Query.Type.ts

```typescript
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'

export interface DashboardQuery extends ServerQueryingParameters {
  from?: string
  to?: string
}
```

---

### Phase 10: Final Verification

- GOAL-010: Full build, lint, and test pass across entire Admin app

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-028 | Run `pnpm run type-check` — zero errors | | |
| TASK-029 | Run `pnpm run lint` — zero new errors (pre-existing excluded) | | |
| TASK-030 | Run `pnpm run test:unit` — all schema/type-related tests pass | | |
| TASK-031 | Verify no stale `*.domain.types.ts`, `*.request.types.ts`, `*.response.types.ts`, `*.model.types.ts` files remain anywhere in Admin app | | |
| TASK-032 | Final commit with verification badge | | |

## 3. Alternatives

- **ALT-001**: Keep the old `*.domain.types.ts` naming instead of `*.Response.Type.ts` — rejected because "domain" conflates response model with domain model and doesn't align with the Parameters/Request/Response/Query quadrant
- **ALT-002**: Collapse all types into a single `{Entity}.types.ts` file per entity — rejected because it violates single-responsibility and creates merge conflicts in parallel work
- **ALT-003**: Keep `kebab-case` schema file names (`taxon.schema.ts`) — rejected because `PascalCase.Schema.ts` makes entity names immediately scannable in file listings
- **ALT-004**: Keep Zod `FormData` inferred type naming instead of `Parameters` — rejected because `Parameters` is more descriptive (these types represent the full set of form parameters, not just "form data")

## 4. Dependencies

- **DEP-001**: Zod ^3.22 (already in `app/Admin/package.json`)
- **DEP-002**: `ServerQueryingParameters` from `@/shared/api/types/query.types` (verified)
- **DEP-003**: `ServerResult<T>`, `ServerPagedResult<T>` from `@/shared/api/types/result.types` (verified)
- **DEP-004**: `pnpm run type-check`, `pnpm run lint`, `pnpm run test:unit` scripts in `app/Admin/package.json` (verified)

## 5. Files

- **FILE-001**: ~45 new `{Entity}.Schema.ts` files across all modules (one per entity)
- **FILE-002**: ~150 new `{Entity}.{Parameters|Request|Response|Query}.Type.ts` files
- **FILE-003**: ~30 deleted legacy type files (`*.domain.types.ts`, `*.request.types.ts`, `*.model.types.ts`, `*.response.types.ts`)
- **FILE-004**: ~10 renamed/lowercase schema files (to PascalCase `.Schema.ts`)
- **FILE-005**: ~100+ import-path updates across services, stores, views, components, tests, repositories, mappers

## 6. Testing

- **TEST-001**: Schema spec tests for each entity must pass after file moves (existing: option-type, option-value, property-type)
- **TEST-002**: New schema spec tests for entities without existing ones (user, role, permission, order, fulfillment, stock-location, stock-transfer, profile)
- **TEST-003**: Store spec imports resolve to new type paths
- **TEST-004**: `pnpm run test:unit` passes with zero regressions (pre-existing failures excluded)
- **TEST-005**: `pnpm run lint` passes with zero new errors
- **TEST-006**: No `cannot find module` TypeScript errors remain

## 7. Risks & Assumptions

- **RISK-001**: Vee-validate form bindings use `defineField('fieldName')` referencing schema field names — preserving exact field names (only renaming files) prevents breakage
- **RISK-002**: Enum-to-string migration (PropertyKind numeric → string enum) may require store/view changes if code uses numeric comparison — mitigate by adding legacy numeric getters during transition
- **RISK-003**: Inventory entities lack existing schemas entirely — schema fields derived from existing `request.types.ts` DTOs; may need refinement when integrated with forms
- **ASSUMPTION-001**: All `*.domain.types.ts` exports map 1:1 to `*.Response.Type.ts` exports with identical interface shapes
- **ASSUMPTION-002**: `import type` usage is sufficient for all cross-file type references; no circular `import` (value) dependencies exist
- **ASSUMPTION-003**: Schema renaming from `*.schema.ts` (lowercase) to `*.Schema.ts` (PascalCase) does not break Vite module resolution (Vite resolver is case-sensitive on Linux — verified in pilot)

## 8. Related Specifications / Further Reading

- `plan/refactor-admin-type-schema-1.md` — original full-scope plan (Phase 2 pilot implemented)
- `docs/superpowers/plans/2026-07-17-admin-type-schema-restructure-pilot.md` — pilot execution plan (Taxonomies & Taxa done)
- `app/Admin/src/features/catalog/taxonomies/` — completed reference implementation for pattern
- `plan/refactor-api-to-repository-pattern-1.md` — prior Admin SPA data layer refactor
- `plan/refactor-types-decomposition-1.md` — prior type decomposition plan
