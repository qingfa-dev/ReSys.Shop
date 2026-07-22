# Admin SPA — Consistent List + Detail Page Pattern — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace all 32 admin SPA stub pages with 41 real list + detail pages following a single consistent pattern across 9 modules.

**Architecture:** Convention over abstraction — every top-level entity gets ListPage + DetailPage. Sub-entities render inline on parent DetailPage as `<Fieldset>` sections. No generic composable layer; pages use shared components directly. Consistency enforced by template, not framework.

**Tech Stack:** Vue 3.5 + TypeScript 6, PrimeVue 5 (Aura preset), Tailwind v4, Pinia, Vue Router 5

## Global Constraints

- **PAT-001**: Every top-level entity gets exactly 1 ListPage and 1 DetailPage
- **PAT-002**: Sub-entities on parent DetailPage as `<Fieldset>` sections. No separate routes.
- **PAT-003**: DetailPage mode from route: `/new` = create, `/:id` = view, `/:id/edit` = edit
- **PAT-004**: No specialized page types (TreeManager, separate Create pages)
- **PAT-005**: Shared components only. No new composable layer.
- **PAT-006**: Taxon hierarchy = flat DataTable with CSS depth indentation
- **CON-001**: Use existing `useToast` for feedback, `useConfirm` for destructive actions
- **CON-002**: No new npm packages or dependencies
- **CON-003**: Client-side validation only, inline per page
- **CON-004**: Backend APIs: `/api/{module}/{entity}` pattern, JSON, `Result<T>` envelope

---

### Task 1: Shared API infrastructure

**Files:**
- Create: `app/Admin/src/shared/api/http.ts`
- Create: `app/Admin/src/shared/api/catalog.ts`
- Create: `app/Admin/src/shared/api/inventory.ts`
- Create: `app/Admin/src/shared/api/ordering.ts`
- Create: `app/Admin/src/shared/api/payment.ts`
- Create: `app/Admin/src/shared/api/shipping.ts`
- Create: `app/Admin/src/shared/api/location.ts`
- Create: `app/Admin/src/shared/api/users.ts`
- Create: `app/Admin/src/shared/api/profile.ts`
- Create: `app/Admin/src/shared/api/reports.ts`

**Interfaces:**
- Produces: typed `api<T>()` fetch wrapper + domain API functions consumed by all page tasks.

- [ ] **Step 1: Create HTTP wrapper**

```ts
// app/Admin/src/shared/api/http.ts
const BASE = import.meta.env.VITE_API_URL ?? '/api'

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

export async function api<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${url}`, {
    headers: { 'Content-Type': 'application/json', ...init?.headers },
    ...init,
  })
  if (!res.ok) {
    const body = await res.json().catch(() => ({} as Record<string, unknown>))
    const msg = typeof body === 'object' && body !== null
      ? ((body as Record<string, unknown>).title ?? (body as Record<string, unknown>).detail ?? res.statusText) as string
      : res.statusText
    throw new ApiError(res.status, msg)
  }
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

export function apiDelete(url: string) {
  return api<undefined>(url, { method: 'DELETE' })
}
```

- [ ] **Step 2: Create Catalog API module**

```ts
// app/Admin/src/shared/api/catalog.ts
import { api, apiDelete } from './http'

// --- Products ---
export interface ProductListItem {
  id: string; name: string; slug: string; status: string; masterVariantId?: string; createdAt: string
}
export interface ProductDetail {
  id: string; name: string; slug: string; description?: string; status: string
  metaTitle?: string; metaDescription?: string; metaKeywords?: string
  styleCode?: string; seasonName?: string; materialComposition?: string
  careInstructions?: string; fitNotes?: string; department?: string; genderTarget?: string
  availableOn?: string; discontinueOn?: string; makeActiveAt?: string
  masterVariantId?: string
}
export interface PagedResult<T> { items: T[]; total: number; page: number; pageSize: number }

export const catalogApi = {
  // Products
  getProducts(params: { page?: number; pageSize?: number; search?: string; sort?: string } = {}) {
    const qs = new URLSearchParams(Object.entries(params).filter(([,v]) => v != null).map(([k,v]) => [k, String(v)]))
    return api<PagedResult<ProductListItem>>(`/catalog/products?${qs}`)
  },
  getProduct(id: string) { return api<ProductDetail>(`/catalog/products/${id}`) },
  createProduct(data: Partial<ProductDetail>) { return api<ProductDetail>('/catalog/products', { method: 'POST', body: JSON.stringify(data) }) },
  updateProduct(id: string, data: Partial<ProductDetail>) { return api<ProductDetail>(`/catalog/products/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteProduct(id: string) { return apiDelete(`/catalog/products/${id}`) },
  activateProduct(id: string) { return api<ProductDetail>(`/catalog/products/${id}/activate`, { method: 'POST' }) },

  // Variants (sub-entity of Product)
  getVariants(productId: string) { return api<PagedResult<VariantListItem>>(`/catalog/products/${productId}/variants`) },
  createVariant(productId: string, data: Partial<VariantDetail>) { return api<VariantDetail>(`/catalog/products/${productId}/variants`, { method: 'POST', body: JSON.stringify(data) }) },
  updateVariant(id: string, data: Partial<VariantDetail>) { return api<VariantDetail>(`/catalog/variants/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteVariant(id: string) { return apiDelete(`/catalog/variants/${id}`) },

  // Variant Prices
  getVariantPrices(variantId: string) { return api<PriceListItem[]>(`/catalog/variants/${variantId}/prices`) },
  createVariantPrice(variantId: string, data: Partial<PriceDetail>) { return api<PriceDetail>(`/catalog/variants/${variantId}/prices`, { method: 'POST', body: JSON.stringify(data) }) },
  deleteVariantPrice(variantId: string, priceId: string) { return apiDelete(`/catalog/variants/${variantId}/prices/${priceId}`) },

  // Variant Images
  getVariantImages(variantId: string) { return api<VariantImageItem[]>(`/catalog/variants/${variantId}/images`) },
  deleteVariantImage(id: string) { return apiDelete(`/catalog/variants/images/${id}`) },

  // Taxonomies
  getTaxonomies(params: { page?: number; pageSize?: number; search?: string } = {}) {
    const qs = new URLSearchParams(Object.entries(params).filter(([,v]) => v != null).map(([k,v]) => [k, String(v)]))
    return api<PagedResult<TaxonomyListItem>>(`/catalog/taxonomies?${qs}`)
  },
  getTaxonomy(id: string) { return api<TaxonomyDetail>(`/catalog/taxonomies/${id}`) },
  createTaxonomy(data: Partial<TaxonomyDetail>) { return api<TaxonomyDetail>('/catalog/taxonomies', { method: 'POST', body: JSON.stringify(data) }) },
  updateTaxonomy(id: string, data: Partial<TaxonomyDetail>) { return api<TaxonomyDetail>(`/catalog/taxonomies/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteTaxonomy(id: string) { return apiDelete(`/catalog/taxonomies/${id}`) },

  // Taxons (sub-entity of Taxonomy)
  getTaxons(taxonomyId: string) { return api<TaxonListItem[]>(`/catalog/taxonomies/${taxonomyId}/taxons`) },
  createTaxon(taxonomyId: string, data: Partial<TaxonDetail>) { return api<TaxonDetail>(`/catalog/taxonomies/${taxonomyId}/taxons`, { method: 'POST', body: JSON.stringify(data) }) },
  updateTaxon(id: string, data: Partial<TaxonDetail>) { return api<TaxonDetail>(`/catalog/taxonomies/${taxonomyId}/taxons/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteTaxon(taxonomyId: string, id: string) { return apiDelete(`/catalog/taxonomies/${taxonomyId}/taxons/${id}`) },

  // OptionTypes
  getOptionTypes(params: { page?: number; pageSize?: number; search?: string } = {}) {
    const qs = new URLSearchParams(Object.entries(params).filter(([,v]) => v != null).map(([k,v]) => [k, String(v)]))
    return api<PagedResult<OptionTypeListItem>>(`/catalog/option-types?${qs}`)
  },
  getOptionType(id: string) { return api<OptionTypeDetail>(`/catalog/option-types/${id}`) },
  createOptionType(data: Partial<OptionTypeDetail>) { return api<OptionTypeDetail>('/catalog/option-types', { method: 'POST', body: JSON.stringify(data) }) },
  updateOptionType(id: string, data: Partial<OptionTypeDetail>) { return api<OptionTypeDetail>(`/catalog/option-types/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteOptionType(id: string) { return apiDelete(`/catalog/option-types/${id}`) },

  // OptionValues (sub-entity of OptionType)
  getOptionValues(optionTypeId: string) { return api<OptionValueItem[]>(`/catalog/option-types/${optionTypeId}/values`) },
  createOptionValue(optionTypeId: string, data: Partial<OptionValueItem>) { return api<OptionValueItem>(`/catalog/option-types/${optionTypeId}/values`, { method: 'POST', body: JSON.stringify(data) }) },
  updateOptionValue(optionTypeId: string, id: string, data: Partial<OptionValueItem>) { return api<OptionValueItem>(`/catalog/option-types/${optionTypeId}/values/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteOptionValue(optionTypeId: string, id: string) { return apiDelete(`/catalog/option-types/${optionTypeId}/values/${id}`) },

  // Classifications (sub-entity of Product)
  getClassifications(productId: string) { return api<ClassificationItem[]>(`/catalog/products/${productId}/classifications`) },
  assignClassifications(productId: string, taxonIds: string[]) { return api(`/catalog/products/${productId}/classifications/assign`, { method: 'POST', body: JSON.stringify(taxonIds) }) },
  revokeClassifications(productId: string, taxonIds: string[]) { return api(`/catalog/products/${productId}/classifications/revoke`, { method: 'POST', body: JSON.stringify(taxonIds) }) },

  // Product OptionTypes (sub-entity of Product)
  getProductOptionTypes(productId: string) { return api<number[]>(`/catalog/products/${productId}/option-types`) },
  syncProductOptionTypes(productId: string, optionTypeIds: string[]) { return api(`/catalog/products/${productId}/option-types/sync`, { method: 'POST', body: JSON.stringify(optionTypeIds) }) },

  // Variant OptionValues
  getVariantOptionValues(variantId: string) { return api<number[]>(`/catalog/variants/${variantId}/option-values`) },
  syncVariantOptionValues(variantId: string, optionValueIds: string[]) { return api(`/catalog/variants/${variantId}/option-values/sync`, { method: 'POST', body: JSON.stringify(optionValueIds) }) },
}

// Inline types (used only by catalogApi)
export interface VariantListItem { id: string; sku: string; price: number; isMaster: boolean; stock?: number }
export interface VariantDetail extends VariantListItem { costPrice?: number; weight?: number; weightUnit?: string; height?: number; width?: number; depth?: number; barcode?: string; trackInventory: boolean }
export interface PriceListItem { id: string; amount: number; compareAtAmount?: number; currency: string; isDefault: boolean }
export interface PriceDetail extends PriceListItem { countryIso?: string }
export interface VariantImageItem { id: string; url: string; fileName: string; type: string; width?: number; height?: number }
export interface TaxonomyListItem { id: string; name: string; presentation?: string; position: number; taxonCount?: number }
export interface TaxonomyDetail extends TaxonomyListItem { }
export interface TaxonListItem { id: string; name: string; presentation?: string; depth: number; lft: number; rgt: number; parentId?: string; position: number; slug: string; automatic: boolean; hideFromNav: boolean }
export interface TaxonDetail extends TaxonListItem { description?: string; descriptionHtml?: string; metaTitle?: string; metaDescription?: string; sortOrder?: string; rulesMatchPolicy?: string; permalink?: string; childrenCount: number; taxonomyId: string }
export interface OptionTypeListItem { id: string; name: string; presentation?: string; position: number; filterable: boolean }
export interface OptionTypeDetail extends OptionTypeListItem { }
export interface OptionValueItem { id: string; name: string; presentation?: string; position: number }
export interface ClassificationItem { id: string; taxonId: string; taxonName: string; position: number; isAutomatic: boolean }
```

- [ ] **Step 3: Create Inventory API module**

```ts
// app/Admin/src/shared/api/inventory.ts
import { api, apiDelete } from './http'

export interface StockItemItem { id: string; variantId: string; variantName?: string; sku?: string; countOnHand: number; backorderable: boolean; stockLocationId: string; stockLocationName?: string }
export interface StockLocationItem { id: string; name: string; code: string; active: boolean; isDefault: boolean; city?: string; countryId?: string }
export interface StockLocationDetail extends StockLocationItem { address?: string; phone?: string; adminName?: string; propagateAllVariants: boolean; lowStockThreshold?: number; backorderableDefault: boolean; position: number }
export interface StockMovementItem { id: string; quantity: number; previousCountOnHand: number; action: string; createdAt: string; stockItemId: string; variantName?: string; reason?: string }
export interface StockTransferItem { id: string; number: string; reference?: string; state: string; sourceLocationName?: string; destinationLocationName?: string; createdAt: string }
export interface StockTransferDetail extends StockTransferItem { sourceLocationId: string; destinationLocationId: string }
export interface TransferItemItem { id: string; variantName?: string; quantity: number; receivedQuantity: number }
export interface PagedResult<T> { items: T[]; total: number; page: number; pageSize: number }

export const inventoryApi = {
  getStockItems(params: { page?: number; pageSize?: number; search?: string; locationId?: string } = {}) {
    const qs = new URLSearchParams(Object.entries(params).filter(([,v]) => v != null).map(([k,v]) => [k, String(v)]))
    return api<PagedResult<StockItemItem>>(`/inventory/stocks?${qs}`)
  },
  getStockLocations(params: { page?: number; pageSize?: number; search?: string } = {}) {
    const qs = new URLSearchParams(Object.entries(params).filter(([,v]) => v != null).map(([k,v]) => [k, String(v)]))
    return api<PagedResult<StockLocationItem>>(`/inventory/locations?${qs}`)
  },
  getStockLocation(id: string) { return api<StockLocationDetail>(`/inventory/locations/${id}`) },
  createStockLocation(data: Partial<StockLocationDetail>) { return api<StockLocationDetail>('/inventory/locations', { method: 'POST', body: JSON.stringify(data) }) },
  updateStockLocation(id: string, data: Partial<StockLocationDetail>) { return api<StockLocationDetail>(`/inventory/locations/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteStockLocation(id: string) { return apiDelete(`/inventory/locations/${id}`) },
  getStockMovements(params: { page?: number; pageSize?: number; search?: string } = {}) {
    const qs = new URLSearchParams(Object.entries(params).filter(([,v]) => v != null).map(([k,v]) => [k, String(v)]))
    return api<PagedResult<StockMovementItem>>(`/inventory/movements?${qs}`)
  },
  getStockTransfers(params: { page?: number; pageSize?: number; search?: string } = {}) {
    const qs = new URLSearchParams(Object.entries(params).filter(([,v]) => v != null).map(([k,v]) => [k, String(v)]))
    return api<PagedResult<StockTransferItem>>(`/inventory/transfers?${qs}`)
  },
  getStockTransfer(id: string) { return api<StockTransferDetail>(`/inventory/transfers/${id}`) },
  createStockTransfer(data: Partial<StockTransferDetail>) { return api<StockTransferDetail>('/inventory/transfers', { method: 'POST', body: JSON.stringify(data) }) },
  getTransferItems(transferId: string) { return api<TransferItemItem[]>(`/inventory/transfers/${transferId}/items`) },
  receiveTransfer(id: string) { return api<StockTransferDetail>(`/inventory/transfers/${id}/receive`, { method: 'POST' }) },
  cancelTransfer(id: string) { return api<StockTransferDetail>(`/inventory/transfers/${id}/cancel`, { method: 'POST' }) },
}
```

- [ ] **Step 4: Create remaining domain API modules**

Write `ordering.ts`, `payment.ts`, `shipping.ts`, `location.ts`, `users.ts`, `profile.ts`, `reports.ts` following the same pattern as catalog — typed functions wrapping `api<T>()`. Each exports a `{module}Api` object with `get{Entity}s`, `get{Entity}`, `create{Entity}`, `update{Entity}`, `delete{Entity}` for each top-level entity.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/shared/api/
git commit -m "feat: add shared API infrastructure and domain API modules"
```

---

### Task 2: Catalog — Types, ProductListPage, ProductDetailPage

**Files:**
- Create: `app/Admin/src/features/catalog/types.ts`
- Modify: `app/Admin/src/features/catalog/pages/ProductListPage.vue`
- Create: `app/Admin/src/features/catalog/pages/ProductDetailPage.vue`
- Modify: `app/Admin/src/app/routes/catalog.routes.ts`

**Interfaces:**
- Consumes: `catalogApi`, `useToast`, `useConfirm` from Task 1 + existing composables
- Produces: ProductListPage, ProductDetailPage consumed by routes

- [ ] **Step 1: Create catalog types file**

```ts
// app/Admin/src/features/catalog/types.ts
export interface ProductForm {
  name: string; slug: string; description: string; status: string
  metaTitle: string; metaDescription: string; metaKeywords: string
  styleCode: string; seasonName: string; materialComposition: string
  careInstructions: string; fitNotes: string; department: string; genderTarget: string
  availableOn: string; discontinueOn: string; makeActiveAt: string
}

export function emptyProductForm(): ProductForm {
  return { name: '', slug: '', description: '', status: 'Draft',
    metaTitle: '', metaDescription: '', metaKeywords: '',
    styleCode: '', seasonName: '', materialComposition: '',
    careInstructions: '', fitNotes: '', department: '', genderTarget: '',
    availableOn: '', discontinueOn: '', makeActiveAt: '' }
}
```

- [ ] **Step 2: Implement ProductListPage**

```vue
<!-- app/Admin/src/features/catalog/pages/ProductListPage.vue -->
<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import { default as AppDataTable } from '@/shared/components/data/DataTable.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import BulkActionBar from '@/shared/components/layout/BulkActionBar.vue'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import { catalogApi, type ProductListItem, type PagedResult } from '@/shared/api/catalog'
import { useToast } from '@/shared/composables/useToast'
import { useConfirm } from '@/shared/composables/useConfirm'

const router = useRouter()
const toast = useToast()
const { confirmDelete } = useConfirm()

const items = ref<ProductListItem[]>([])
const total = ref(0)
const loading = ref(false)
const error = ref('')
const page = ref(1)
const pageSize = ref(20)
const search = ref('')
const selection = ref<ProductListItem[]>([])

async function fetchItems() {
  loading.value = true; error.value = ''
  try {
    const result: PagedResult<ProductListItem> = await catalogApi.getProducts({
      page: page.value, pageSize: pageSize.value, search: search.value || undefined,
    })
    items.value = result.items; total.value = result.total
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Failed to load products'
  } finally { loading.value = false }
}

watch([page, search], () => { page.value = search.value ? 1 : page.value; fetchItems() }, { immediate: true })

function onPage(e: { page: number; rows: number }) { page.value = e.page + 1; fetchItems() }

async function deleteProduct(product: ProductListItem) {
  confirmDelete({
    target: `product "${product.name}"`,
    onAccept: async () => {
      try { await catalogApi.deleteProduct(product.id); toast.success('Product deleted'); await fetchItems() }
      catch (e) { toast.error(e instanceof Error ? e.message : 'Delete failed') }
    },
  })
}

const statusSeverity = (s: string) => s === 'Active' ? 'success' : s === 'Draft' ? 'secondary' : 'warn'
</script>

<template>
  <PageHeader title="Products" subtitle="Manage product catalog"
    :breadcrumb="[{ label: 'Catalog', to: '/catalog/dashboard' }, { label: 'Products' }]" />
  <TableToolbar create-label="Add Product" @create="router.push({ name: 'catalog.products.new' })" @search="search = $event" />
  <div v-if="error"><ErrorState :description="error" @retry="fetchItems" /></div>
  <AppDataTable v-else :rows="items" :loading="loading" :total-records="total" :page-size="pageSize"
    :first="(page - 1) * pageSize" v-model:selection="selection" selectable
    empty-title="No products yet" empty-description="Create your first product to get started."
    @page="onPage">
    <Column field="name" header="Name" sortable>
      <template #body="{ data }">
        <router-link :to="{ name: 'catalog.products.detail', params: { id: data.id } }" class="text-primary hover:underline font-medium">
          {{ data.name }}</router-link>
      </template>
    </Column>
    <Column field="slug" header="Slug" sortable />
    <Column field="status" header="Status" sortable>
      <template #body="{ data }"><Tag :value="data.status" :severity="statusSeverity(data.status)" /></template>
    </Column>
    <template #rowActions="{ data }">
      <ActionMenu :items="[
        { label: 'Edit', icon: 'pi pi-pencil', command: () => router.push({ name: 'catalog.products.edit', params: { id: data.id } }) },
        { label: 'Delete', icon: 'pi pi-trash', command: () => deleteProduct(data) },
      ]" />
    </template>
  </AppDataTable>
  <BulkActionBar :count="selection.length" @clear="selection = []" />
</template>

<script lang="ts">
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
</script>
```

Wait — I see a mistake in the above. `useConfirm` should be `confirmDelete`, the import for ErrorState is inside an extra script block incorrectly. Let me fix this in the actual write. The page implementation should be clean. Let me continue writing the plan with correct code.

Actually, I realize the plan writing is massive for 41 pages. Let me use a pragmatic approach: write the first complex module (Catalog) in full detail, then use a compressed pattern for the remaining modules since they follow the exact same template. The compressed tasks still include complete code — just more concise since the pattern is established.

Let me restart the plan file with this approach.<｜end▁of▁thinking｜>

<｜｜DSML｜｜tool_calls>
<｜｜DSML｜｜invoke name="write">
<｜｜DSML｜｜parameter name="content" string="true"># Admin SPA — Consistent List + Detail Page Pattern — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace all 32 admin SPA stub pages with 41 real list + detail pages following a single consistent pattern across 9 modules.

**Architecture:** Convention over abstraction — every top-level entity gets ListPage + DetailPage. Sub-entities render inline on parent DetailPage as `<Fieldset>` sections. No generic composable layer; pages use shared components directly. Consistency enforced by template, not framework.

**Tech Stack:** Vue 3.5 + TypeScript 6, PrimeVue 5 (Aura preset), Tailwind v4, Pinia, Vue Router 5

## Global Constraints

- **PAT-001**: Every top-level entity gets exactly 1 ListPage and 1 DetailPage
- **PAT-002**: Sub-entities on parent DetailPage as `<Fieldset>` sections. No separate routes.
- **PAT-003**: DetailPage mode from route: `/new` = create, `/:id` = view, `/:id/edit` = edit
- **PAT-004**: No specialized page types (TreeManager, separate Create pages)
- **PAT-005**: Shared components only. No new composable layer.
- **PAT-006**: Taxon hierarchy = flat DataTable with CSS depth indentation
- **CON-001**: Use existing `useToast` for feedback, `useConfirm` for destructive actions
- **CON-002**: No new npm packages or dependencies
- **CON-003**: Client-side validation only, inline per page
- **CON-004**: Backend APIs: `/api/{module}/{entity}` pattern, JSON, `Result<T>` envelope

---

### Task 1: Shared API infrastructure

**Files:**
- Create: `app/Admin/src/shared/api/http.ts`
- Create: `app/Admin/src/shared/api/catalog.ts`
- Create: `app/Admin/src/shared/api/inventory.ts`
- Create: `app/Admin/src/shared/api/ordering.ts`
- Create: `app/Admin/src/shared/api/payment.ts`
- Create: `app/Admin/src/shared/api/shipping.ts`
- Create: `app/Admin/src/shared/api/location.ts`
- Create: `app/Admin/src/shared/api/users.ts`
- Create: `app/Admin/src/shared/api/profile.ts`
- Create: `app/Admin/src/shared/api/reports.ts`

**Interfaces:**
- Produces: `ApiError` class, `api<T>()` fetch wrapper, `apiDelete()` helper, typed domain API modules consumed by all page tasks.

- [ ] **Step 1: Create HTTP wrapper**

```ts
// app/Admin/src/shared/api/http.ts
const BASE = import.meta.env.VITE_API_URL ?? '/api'

export class ApiError extends Error {
  constructor(public status: number, message: string) { super(message); this.name = 'ApiError' }
}

export async function api<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${url}`, {
    headers: { 'Content-Type': 'application/json', ...init?.headers },
    ...init,
  })
  if (!res.ok) {
    const body = await res.json().catch(() => ({} as Record<string, unknown>))
    const msg = (typeof body === 'object' && body !== null)
      ? ((body as Record<string, unknown>).title ?? (body as Record<string, unknown>).detail ?? res.statusText) as string
      : res.statusText
    throw new ApiError(res.status, msg)
  }
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

export function apiDelete(url: string) { return api<undefined>(url, { method: 'DELETE' }) }
```

- [ ] **Step 2: Create Catalog API module**

```ts
// app/Admin/src/shared/api/catalog.ts
import { api, apiDelete } from './http'

export function buildQs(params: Record<string, unknown>) {
  return new URLSearchParams(
    Object.entries(params).filter(([, v]) => v != null).map(([k, v]) => [k, String(v)]),
  ).toString()
}

export interface PagedResult<T> { items: T[]; total: number; page: number; pageSize: number }
export interface ProductListItem { id: string; name: string; slug: string; status: string; createdAt: string }
export interface ProductDetail { id: string; name: string; slug: string; description?: string; status: string; metaTitle?: string; metaDescription?: string; metaKeywords?: string; styleCode?: string; seasonName?: string; materialComposition?: string; careInstructions?: string; fitNotes?: string; department?: string; genderTarget?: string; availableOn?: string; discontinueOn?: string; makeActiveAt?: string; masterVariantId?: string }
export interface VariantListItem { id: string; sku: string; price: number; isMaster: boolean }
export interface VariantDetail extends VariantListItem { costPrice?: number; weight?: number; weightUnit?: string; height?: number; width?: number; depth?: number; barcode?: string; trackInventory: boolean }
export interface PriceListItem { id: string; amount: number; compareAtAmount?: number; currency: string; isDefault: boolean }
export interface PriceDetail extends PriceListItem { countryIso?: string }
export interface VariantImageItem { id: string; url: string; fileName: string; type: string; width?: number; height?: number }
export interface TaxonomyListItem { id: string; name: string; presentation?: string; position: number }
export interface TaxonomyDetail extends TaxonomyListItem { }
export interface TaxonListItem { id: string; name: string; depth: number; lft: number; rgt: number; parentId?: string; position: number; slug: string; automatic: boolean; hideFromNav: boolean }
export interface TaxonDetail extends TaxonListItem { description?: string; metaTitle?: string; metaDescription?: string; taxonomyId: string; childrenCount: number; sortOrder?: string }
export interface OptionTypeListItem { id: string; name: string; presentation?: string; position: number; filterable: boolean }
export interface OptionTypeDetail extends OptionTypeListItem { }
export interface OptionValueItem { id: string; name: string; presentation?: string; position: number }
export interface ClassificationItem { id: string; taxonId: string; taxonName: string; position: number; isAutomatic: boolean }

export const catalogApi = {
  getProducts(params?: Record<string, unknown>) { return api<PagedResult<ProductListItem>>(`/catalog/products?${buildQs(params ?? {})}`) },
  getProduct(id: string) { return api<ProductDetail>(`/catalog/products/${id}`) },
  createProduct(data: Partial<ProductDetail>) { return api<ProductDetail>('/catalog/products', { method: 'POST', body: JSON.stringify(data) }) },
  updateProduct(id: string, data: Partial<ProductDetail>) { return api<ProductDetail>(`/catalog/products/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteProduct(id: string) { return apiDelete(`/catalog/products/${id}`) },

  getVariants(productId: string) { return api<PagedResult<VariantListItem>>(`/catalog/products/${productId}/variants`) },
  createVariant(productId: string, data: Partial<VariantDetail>) { return api<VariantDetail>(`/catalog/products/${productId}/variants`, { method: 'POST', body: JSON.stringify(data) }) },
  updateVariant(id: string, data: Partial<VariantDetail>) { return api<VariantDetail>(`/catalog/variants/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteVariant(id: string) { return apiDelete(`/catalog/variants/${id}`) },

  getVariantPrices(variantId: string) { return api<PriceListItem[]>(`/catalog/variants/${variantId}/prices`) },
  createVariantPrice(variantId: string, data: Partial<PriceDetail>) { return api<PriceDetail>(`/catalog/variants/${variantId}/prices`, { method: 'POST', body: JSON.stringify(data) }) },
  deleteVariantPrice(variantId: string, priceId: string) { return apiDelete(`/catalog/variants/${variantId}/prices/${priceId}`) },

  getVariantImages(variantId: string) { return api<VariantImageItem[]>(`/catalog/variants/${variantId}/images`) },
  deleteVariantImage(id: string) { return apiDelete(`/catalog/variants/images/${id}`) },

  getTaxonomies(params?: Record<string, unknown>) { return api<PagedResult<TaxonomyListItem>>(`/catalog/taxonomies?${buildQs(params ?? {})}`) },
  getTaxonomy(id: string) { return api<TaxonomyDetail>(`/catalog/taxonomies/${id}`) },
  createTaxonomy(data: Partial<TaxonomyDetail>) { return api<TaxonomyDetail>('/catalog/taxonomies', { method: 'POST', body: JSON.stringify(data) }) },
  updateTaxonomy(id: string, data: Partial<TaxonomyDetail>) { return api<TaxonomyDetail>(`/catalog/taxonomies/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteTaxonomy(id: string) { return apiDelete(`/catalog/taxonomies/${id}`) },

  getTaxons(taxonomyId: string) { return api<TaxonListItem[]>(`/catalog/taxonomies/${taxonomyId}/taxons`) },
  createTaxon(taxonomyId: string, data: Partial<TaxonDetail>) { return api<TaxonDetail>(`/catalog/taxonomies/${taxonomyId}/taxons`, { method: 'POST', body: JSON.stringify(data) }) },
  updateTaxon(taxonomyId: string, id: string, data: Partial<TaxonDetail>) { return api<TaxonDetail>(`/catalog/taxonomies/${taxonomyId}/taxons/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteTaxon(taxonomyId: string, id: string) { return apiDelete(`/catalog/taxonomies/${taxonomyId}/taxons/${id}`) },

  getOptionTypes(params?: Record<string, unknown>) { return api<PagedResult<OptionTypeListItem>>(`/catalog/option-types?${buildQs(params ?? {})}`) },
  getOptionType(id: string) { return api<OptionTypeDetail>(`/catalog/option-types/${id}`) },
  createOptionType(data: Partial<OptionTypeDetail>) { return api<OptionTypeDetail>('/catalog/option-types', { method: 'POST', body: JSON.stringify(data) }) },
  updateOptionType(id: string, data: Partial<OptionTypeDetail>) { return api<OptionTypeDetail>(`/catalog/option-types/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteOptionType(id: string) { return apiDelete(`/catalog/option-types/${id}`) },

  getOptionValues(optionTypeId: string) { return api<OptionValueItem[]>(`/catalog/option-types/${optionTypeId}/values`) },
  createOptionValue(optionTypeId: string, data: Partial<OptionValueItem>) { return api<OptionValueItem>(`/catalog/option-types/${optionTypeId}/values`, { method: 'POST', body: JSON.stringify(data) }) },
  updateOptionValue(optionTypeId: string, id: string, data: Partial<OptionValueItem>) { return api<OptionValueItem>(`/catalog/option-types/${optionTypeId}/values/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteOptionValue(optionTypeId: string, id: string) { return apiDelete(`/catalog/option-types/${optionTypeId}/values/${id}`) },

  getClassifications(productId: string) { return api<ClassificationItem[]>(`/catalog/products/${productId}/classifications`) },
  syncClassifications(productId: string, taxonIds: string[]) { return api<void>(`/catalog/products/${productId}/classifications/sync`, { method: 'POST', body: JSON.stringify(taxonIds) }) },
  getProductOptionTypes(productId: string) { return api<{optionTypeId: string; position: number}[]>(`/catalog/products/${productId}/option-types`) },
  syncProductOptionTypes(productId: string, optionTypeIds: string[]) { return api<void>(`/catalog/products/${productId}/option-types/sync`, { method: 'POST', body: JSON.stringify(optionTypeIds) }) },
  getVariantOptionValues(variantId: string) { return api<{optionValueId: string}[]>(`/catalog/variants/${variantId}/option-values`) },
  syncVariantOptionValues(variantId: string, optionValueIds: string[]) { return api<void>(`/catalog/variants/${variantId}/option-values/sync`, { method: 'POST', body: JSON.stringify(optionValueIds) }) },
}
```

- [ ] **Step 3: Create Inventory API module**

```ts
// app/Admin/src/shared/api/inventory.ts
import { api, apiDelete, buildQs, type PagedResult } from './catalog'
export { type PagedResult }

export interface StockItemItem { id: string; variantId: string; variantName?: string; sku?: string; countOnHand: number; backorderable: boolean; stockLocationId: string; stockLocationName?: string }
export interface StockLocationItem { id: string; name: string; code: string; active: boolean; isDefault: boolean; city?: string; countryId?: string }
export interface StockLocationDetail extends StockLocationItem { address?: string; phone?: string; adminName?: string; propagateAllVariants: boolean; lowStockThreshold?: number; backorderableDefault: boolean; position: number }
export interface StockMovementItem { id: string; quantity: number; previousCountOnHand: number; action: string; createdAt: string; stockItemId: string; variantName?: string; reason?: string }
export interface StockTransferItem { id: string; number: string; reference?: string; state: string; sourceLocationName?: string; destinationLocationName?: string; createdAt: string }
export interface StockTransferDetail extends StockTransferItem { sourceLocationId: string; destinationLocationId: string }
export interface TransferItemItem { id: string; variantName?: string; quantity: number; receivedQuantity: number }

export const inventoryApi = {
  getStockItems(params?: Record<string, unknown>) { return api<PagedResult<StockItemItem>>(`/inventory/stocks?${buildQs(params ?? {})}`) },
  getStockLocations(params?: Record<string, unknown>) { return api<PagedResult<StockLocationItem>>(`/inventory/locations?${buildQs(params ?? {})}`) },
  getStockLocation(id: string) { return api<StockLocationDetail>(`/inventory/locations/${id}`) },
  createStockLocation(data: Partial<StockLocationDetail>) { return api<StockLocationDetail>('/inventory/locations', { method: 'POST', body: JSON.stringify(data) }) },
  updateStockLocation(id: string, data: Partial<StockLocationDetail>) { return api<StockLocationDetail>(`/inventory/locations/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteStockLocation(id: string) { return apiDelete(`/inventory/locations/${id}`) },
  getStockMovements(params?: Record<string, unknown>) { return api<PagedResult<StockMovementItem>>(`/inventory/movements?${buildQs(params ?? {})}`) },
  getStockTransfers(params?: Record<string, unknown>) { return api<PagedResult<StockTransferItem>>(`/inventory/transfers?${buildQs(params ?? {})}`) },
  getStockTransfer(id: string) { return api<StockTransferDetail>(`/inventory/transfers/${id}`) },
  createStockTransfer(data: Partial<StockTransferDetail>) { return api<StockTransferDetail>('/inventory/transfers', { method: 'POST', body: JSON.stringify(data) }) },
  getTransferItems(transferId: string) { return api<TransferItemItem[]>(`/inventory/transfers/${transferId}/items`) },
  receiveTransfer(id: string) { return api<StockTransferDetail>(`/inventory/transfers/${id}/receive`, { method: 'POST' }) },
  cancelTransfer(id: string) { return api<StockTransferDetail>(`/inventory/transfers/${id}/cancel`, { method: 'POST' }) },
}
```

- [ ] **Step 4: Create Ordering API module**

```ts
// app/Admin/src/shared/api/ordering.ts
import { api, apiDelete, buildQs } from './catalog'
import type { PagedResult } from './catalog'

export interface OrderListItem { id: string; number: string; status: string; total: number; currency: string; email?: string; createdAt: string }
export interface OrderDetail { id: string; number: string; status: string; currency: string; itemTotal: number; adjustmentTotal: number; shipmentTotal: number; total: number; paymentTotal: number; outstandingBalance: number; email?: string; specialInstructions?: string; userId?: string; shippingMethodId?: string; storeId?: string; createdAt: string; updatedAt: string }
export interface OrderLineItem { id: string; variantName?: string; sku?: string; quantity: number; price: number; total: number }
export interface FulfillmentItem { id: string; orderNumber: string; status: string; shippingMethodName?: string; orderId: string }

export const orderingApi = {
  getOrders(params?: Record<string, unknown>) { return api<PagedResult<OrderListItem>>(`/ordering/orders?${buildQs(params ?? {})}`) },
  getOrder(id: string) { return api<OrderDetail>(`/ordering/orders/${id}`) },
  createOrder(data: Partial<OrderDetail>) { return api<OrderDetail>('/ordering/orders', { method: 'POST', body: JSON.stringify(data) }) },
  updateOrder(id: string, data: Partial<OrderDetail>) { return api<OrderDetail>(`/ordering/orders/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  cancelOrder(id: string) { return api<void>(`/ordering/orders/${id}/cancel`, { method: 'POST' }) },
  getOrderLineItems(orderId: string) { return api<OrderLineItem[]>(`/ordering/orders/${orderId}/line-items`) },
  addOrderLineItem(orderId: string, data: Partial<OrderLineItem>) { return api<OrderLineItem>(`/ordering/orders/${orderId}/line-items`, { method: 'POST', body: JSON.stringify(data) }) },
  updateOrderLineItem(orderId: string, id: string, data: Partial<OrderLineItem>) { return api<OrderLineItem>(`/ordering/orders/${orderId}/line-items/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  removeOrderLineItem(orderId: string, id: string) { return apiDelete(`/ordering/orders/${orderId}/line-items/${id}`) },
  getFulfillmentQueue(params?: Record<string, unknown>) { return api<PagedResult<FulfillmentItem>>(`/ordering/fulfillment?${buildQs(params ?? {})}`) },
}
```

- [ ] **Step 5: Create Payment API module**

```ts
// app/Admin/src/shared/api/payment.ts
import { api, buildQs } from './catalog'
import type { PagedResult } from './catalog'

export interface PaymentListItem { id: string; amount: number; currency: string; status: string; methodName?: string; orderNumber?: string; createdAt: string }
export interface PaymentDetail extends PaymentListItem { orderId?: string; paymentMethodId?: string }
export interface PaymentMethodItem { id: string; name: string; code: string; active: boolean; position: number }
export interface PaymentMethodDetail extends PaymentMethodItem { description?: string }

export const paymentApi = {
  getPayments(params?: Record<string, unknown>) { return api<PagedResult<PaymentListItem>>(`/payments/list?${buildQs(params ?? {})}`) },
  getPayment(id: string) { return api<PaymentDetail>(`/payments/list/${id}`) },
  getPaymentMethods(params?: Record<string, unknown>) { return api<PagedResult<PaymentMethodItem>>(`/payments/methods?${buildQs(params ?? {})}`) },
  getPaymentMethod(id: string) { return api<PaymentMethodDetail>(`/payments/methods/${id}`) },
  createPaymentMethod(data: Partial<PaymentMethodDetail>) { return api<PaymentMethodDetail>('/payments/methods', { method: 'POST', body: JSON.stringify(data) }) },
  updatePaymentMethod(id: string, data: Partial<PaymentMethodDetail>) { return api<PaymentMethodDetail>(`/payments/methods/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deletePaymentMethod(id: string) { return api(`/payments/methods/${id}`, { method: 'DELETE' }) as Promise<undefined> },
}
```

- [ ] **Step 6: Create Shipping API module**

```ts
// app/Admin/src/shared/api/shipping.ts
import { api, apiDelete, buildQs } from './catalog'
import type { PagedResult } from './catalog'

export interface ShippingMethodItem { id: string; name: string; code: string; active: boolean; position: number }
export interface ShippingMethodDetail extends ShippingMethodItem { description?: string; trackingUrl?: string }
export interface ShippingRateItem { id: string; name: string; amount: number; currency: string; zoneName?: string; shippingMethodName?: string }
export interface ShippingRateDetail extends ShippingRateItem { shippingMethodId: string; minOrderAmount?: number; maxOrderAmount?: number; minWeight?: number; maxWeight?: number }

export const shippingApi = {
  getShippingMethods(params?: Record<string, unknown>) { return api<PagedResult<ShippingMethodItem>>(`/shipping/methods?${buildQs(params ?? {})}`) },
  getShippingMethod(id: string) { return api<ShippingMethodDetail>(`/shipping/methods/${id}`) },
  createShippingMethod(data: Partial<ShippingMethodDetail>) { return api<ShippingMethodDetail>('/shipping/methods', { method: 'POST', body: JSON.stringify(data) }) },
  updateShippingMethod(id: string, data: Partial<ShippingMethodDetail>) { return api<ShippingMethodDetail>(`/shipping/methods/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteShippingMethod(id: string) { return apiDelete(`/shipping/methods/${id}`) },
  getShippingRates(params?: Record<string, unknown>) { return api<PagedResult<ShippingRateItem>>(`/shipping/rates?${buildQs(params ?? {})}`) },
  getShippingRate(id: string) { return api<ShippingRateDetail>(`/shipping/rates/${id}`) },
  createShippingRate(data: Partial<ShippingRateDetail>) { return api<ShippingRateDetail>('/shipping/rates', { method: 'POST', body: JSON.stringify(data) }) },
  updateShippingRate(id: string, data: Partial<ShippingRateDetail>) { return api<ShippingRateDetail>(`/shipping/rates/${id}`, { method: 'PUT', body: JSON.stringify(data) }) },
  deleteShippingRate(id: string) { return apiDelete(`/shipping/rates/${id}`) },
}
```

- [ ] **Step 7: Create Location, Users, Profile, Reports API modules**

Create `location.ts`, `users.ts`, `profile.ts`, `reports.ts` with typed CRUD functions following the same pattern. Each exports `{module}Api` with `get{Entity}s`, `get{Entity}`, `create{Entity}`, `update{Entity}`, `delete{Entity}` for every top-level entity. Types inline in each file matching backend DTOs.

- [ ] **Step 8: Build and commit**

```bash
cd app/Admin && pnpm run build
```
Expect: build succeeds (types compile, imports resolve).

```bash
git add app/Admin/src/shared/api/
git commit -m "feat: add shared API infrastructure and domain API modules"
```

---

### Task 2: Catalog — Product Pages

**Files:**
- Modify: `app/Admin/src/features/catalog/pages/ProductListPage.vue`
- Create: `app/Admin/src/features/catalog/pages/ProductDetailPage.vue`
- Delete: `app/Admin/src/features/catalog/pages/ProductCreatePage.vue`
- Modify: `app/Admin/src/app/routes/catalog.routes.ts`

**Interfaces:**
- Consumes: `catalogApi`, `useToast`, `useConfirm` from Task 1
- Produces: ProductListPage + ProductDetailPage with inline variant/prices/images sub-tables

- [ ] **Step 1: Replace ProductListPage stub**

```vue
<!-- app/Admin/src/features/catalog/pages/ProductListPage.vue -->
<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import { default as AppDataTable } from '@/shared/components/data/DataTable.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import BulkActionBar from '@/shared/components/layout/BulkActionBar.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import { catalogApi, type ProductListItem, type PagedResult } from '@/shared/api/catalog'
import { useToast } from '@/shared/composables/useToast'
import { useConfirm } from '@/shared/composables/useConfirm'

const router = useRouter()
const toast = useToast()
const { confirmDelete } = useConfirm()

const items = ref<ProductListItem[]>([])
const total = ref(0)
const loading = ref(false)
const error = ref('')
const page = ref(1)
const pageSize = ref(20)
const search = ref('')
const selection = ref<ProductListItem[]>([])

async function fetchItems() {
  loading.value = true; error.value = ''
  try {
    const result: PagedResult<ProductListItem> = await catalogApi.getProducts({
      page: page.value, pageSize: pageSize.value, search: search.value || undefined,
    })
    items.value = result.items; total.value = result.total
  } catch (e) { error.value = e instanceof Error ? e.message : 'Failed to load products' }
  finally { loading.value = false }
}

let initDone = false
onMounted(() => { initDone = true; fetchItems() })
watch([page, search], () => { if (initDone) { page.value = search.value ? 1 : page.value; fetchItems() } })

function onPage(e: { page: number; rows: number }) { page.value = e.page + 1 }

async function deleteProduct(product: ProductListItem) {
  confirmDelete({
    target: `product "${product.name}"`,
    onAccept: async () => {
      try { await catalogApi.deleteProduct(product.id); toast.success('Product deleted'); await fetchItems() }
      catch (e) { toast.error(e instanceof Error ? e.message : 'Delete failed') }
    },
  })
}

function statusSeverity(s: string): 'success' | 'secondary' | 'warn' | 'danger' | 'info' | 'contrast' {
  const map: Record<string, 'success' | 'secondary' | 'warn' | 'danger'> = { Active: 'success', Draft: 'secondary', Archived: 'warn' }
  return map[s] ?? 'info'
}
</script>

<template>
  <PageHeader title="Products" subtitle="Manage product catalog"
    :breadcrumb="[{ label: 'Catalog', to: '/catalog/dashboard' }, { label: 'Products' }]" />
  <TableToolbar create-label="Add Product" @create="router.push({ name: 'catalog.products.new' })" @search="search = $event" />
  <ErrorState v-if="error" :description="error" @retry="fetchItems" />
  <AppDataTable v-else :rows="items" :loading="loading" :total-records="total" :page-size="pageSize"
    :first="(page - 1) * pageSize" v-model:selection="selection" selectable
    empty-title="No products yet" empty-description="Create your first product to get started." @page="onPage">
    <Column field="name" header="Name" sortable>
      <template #body="{ data }: { data: ProductListItem }">
        <router-link :to="{ name: 'catalog.products.detail', params: { id: data.id } }"
          class="text-primary hover:underline font-medium">{{ data.name }}</router-link>
      </template>
    </Column>
    <Column field="slug" header="Slug" sortable />
    <Column field="status" header="Status" sortable>
      <template #body="{ data }: { data: ProductListItem }">
        <Tag :value="data.status" :severity="statusSeverity(data.status)" />
      </template>
    </Column>
    <template #rowActions="{ data }: { data: ProductListItem }">
      <ActionMenu :items="[
        { label: 'Edit', icon: 'pi pi-pencil', command: () => router.push({ name: 'catalog.products.edit', params: { id: data.id } }) },
        { label: 'Delete', icon: 'pi pi-trash', command: () => deleteProduct(data) },
      ]" />
    </template>
  </AppDataTable>
  <BulkActionBar :count="selection.length" @clear="selection = []" />
</template>
```

- [ ] **Step 2: Verify ProductListPage builds**

```bash
cd app/Admin && npx vue-tsc --noEmit 2>&1 | head -30
```
Expect: no type errors from `ProductListPage.vue`.

- [ ] **Step 3: Create ProductDetailPage**

```vue
<!-- app/Admin/src/features/catalog/pages/ProductDetailPage.vue -->
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import { default as AppDataTable } from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Select from 'primevue/select'
import Fieldset from 'primevue/fieldset'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import { catalogApi, type ProductDetail, type VariantListItem, type PriceListItem, type VariantImageItem } from '@/shared/api/catalog'
import { useToast } from '@/shared/composables/useToast'
import { useConfirm } from '@/shared/composables/useConfirm'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { confirmDelete } = useConfirm()

const id = computed(() => route.params.id as string | undefined)
const mode = computed<'create' | 'view' | 'edit'>(() =>
  !id.value ? 'create' : String(route.name).endsWith('.edit') ? 'edit' : 'view',
)

// --- Entity form ---
const item = ref<Partial<ProductDetail>>({ name: '', slug: '', description: '', status: 'Draft' })
const loading = ref(false)
const saving = ref(false)
const loadError = ref('')
const errors = ref<Record<string, string>>({})
const statuses = ['Draft', 'Active', 'Archived']

async function load() {
  if (!id.value) return
  loading.value = true; loadError.value = ''
  try { item.value = await catalogApi.getProduct(id.value) }
  catch (e) { loadError.value = e instanceof Error ? e.message : 'Failed to load product' }
  finally { loading.value = false }
}

function validate(): boolean {
  errors.value = {}
  if (!item.value.name?.trim()) errors.value.name = 'Required'
  if (!item.value.slug?.trim()) errors.value.slug = 'Required'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!validate()) return
  saving.value = true
  try {
    if (mode.value === 'create') {
      const created = await catalogApi.createProduct(item.value)
      toast.success('Product created')
      router.replace({ name: 'catalog.products.detail', params: { id: created.id } })
    } else {
      await catalogApi.updateProduct(id.value!, item.value)
      toast.success('Product updated')
      router.replace({ name: 'catalog.products.detail', params: { id: id.value! } })
    }
  } catch (e) { toast.error(e instanceof Error ? e.message : 'Save failed') }
  finally { saving.value = false }
}

function cancel() {
  if (id.value) router.push({ name: 'catalog.products.detail', params: { id: id.value } })
  else router.push({ name: 'catalog.products.list' })
}

// --- Variants sub-table ---
const variants = ref<VariantListItem[]>([])
const variantsLoading = ref(false)
async function fetchVariants() {
  if (!id.value || mode.value === 'create') return
  variantsLoading.value = true
  try { const r = await catalogApi.getVariants(id.value); variants.value = r.items }
  catch { variants.value = [] }
  finally { variantsLoading.value = false }
}

async function deleteVariant(v: VariantListItem) {
  confirmDelete({
    target: `variant "${v.sku}"`,
    onAccept: async () => {
      try { await catalogApi.deleteVariant(v.id); toast.success('Variant deleted'); await fetchVariants() }
      catch (e) { toast.error(e instanceof Error ? e.message : 'Delete failed') }
    },
  })
}

// --- Prices sub-table ---
const prices = ref<PriceListItem[]>([])
const pricesLoading = ref(false)
const selectedVariantId = ref('')
async function fetchPrices(variantId: string) {
  selectedVariantId.value = variantId; pricesLoading.value = true
  try { prices.value = await catalogApi.getVariantPrices(variantId) }
  catch { prices.value = [] }
  finally { pricesLoading.value = false }
}

async function deletePrice(p: PriceListItem) {
  confirmDelete({
    target: `price ${p.amount} ${p.currency}`,
    onAccept: async () => {
      try { await catalogApi.deleteVariantPrice(selectedVariantId.value, p.id); toast.success('Price deleted'); await fetchPrices(selectedVariantId.value) }
      catch (e) { toast.error(e instanceof Error ? e.message : 'Delete failed') }
    },
  })
}

// --- Images sub-table ---
const images = ref<VariantImageItem[]>([])
const imagesLoading = ref(false)
async function fetchImages(variantId: string) {
  imagesLoading.value = true
  try { images.value = await catalogApi.getVariantImages(variantId) }
  catch { images.value = [] }
  finally { imagesLoading.value = false }
}

async function deleteImage(img: VariantImageItem) {
  confirmDelete({
    target: `image "${img.fileName}"`,
    onAccept: async () => {
      try { await catalogApi.deleteVariantImage(img.id); toast.success('Image deleted') }
      catch (e) { toast.error(e instanceof Error ? e.message : 'Delete failed') }
    },
  })
}

onMounted(async () => { await load(); await fetchVariants() })

const isEditable = computed(() => mode.value === 'edit' || mode.value === 'create')
const bc = computed(() => mode.value === 'create'
  ? [{ label: 'Catalog', to: '/catalog/dashboard' }, { label: 'Products', to: '/catalog/products' }, { label: 'New Product' }]
  : [{ label: 'Catalog', to: '/catalog/dashboard' }, { label: 'Products', to: '/catalog/products' }, { label: item.value.name || 'Product' }])
</script>

<template>
  <PageHeader :title="mode === 'create' ? 'New Product' : (item.name || 'Product')"
    :subtitle="mode === 'view' ? 'View product details' : undefined" :breadcrumb="bc">
    <template v-if="mode === 'view'" #actions>
      <Button label="Edit" icon="pi pi-pencil" @click="router.push({ name: 'catalog.products.edit', params: { id } })" />
    </template>
  </PageHeader>

  <ErrorState v-if="loadError" :description="loadError" @retry="load" />

  <template v-else>
    <form class="flex flex-col gap-6" @submit.prevent="save">
      <!-- Entity fields -->
      <div class="rounded-border border border-surface-200 dark:border-surface-700 p-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="Name" :error="errors.name" required>
            <InputText v-model="item.name" :disabled="!isEditable" class="w-full" :invalid="!!errors.name" />
          </FormField>
          <FormField label="Slug" :error="errors.slug" required hint="URL-friendly identifier">
            <InputText v-model="item.slug" :disabled="!isEditable" class="w-full" :invalid="!!errors.slug" />
          </FormField>
          <FormField label="Status">
            <Select v-model="item.status" :options="statuses" :disabled="!isEditable" class="w-full" />
          </FormField>
          <FormField label="Department">
            <InputText v-model="item.department" :disabled="!isEditable" class="w-full" />
          </FormField>
          <FormField label="Style Code">
            <InputText v-model="item.styleCode" :disabled="!isEditable" class="w-full" />
          </FormField>
          <FormField label="Season">
            <InputText v-model="item.seasonName" :disabled="!isEditable" class="w-full" />
          </FormField>
          <FormField label="Gender Target">
            <InputText v-model="item.genderTarget" :disabled="!isEditable" class="w-full" />
          </FormField>
          <FormField label="Meta Title">
            <InputText v-model="item.metaTitle" :disabled="!isEditable" class="w-full" />
          </FormField>
          <FormField label="Meta Description">
            <Textarea v-model="item.metaDescription" :disabled="!isEditable" class="w-full" rows="2" />
          </FormField>
          <FormField label="Description" class="md:col-span-2">
            <Textarea v-model="item.description" :disabled="!isEditable" class="w-full" rows="4" />
          </FormField>
          <FormField label="Material Composition" class="md:col-span-2">
            <Textarea v-model="item.materialComposition" :disabled="!isEditable" class="w-full" rows="2" />
          </FormField>
        </div>
      </div>

      <!-- Variants sub-table -->
      <Fieldset legend="Variants" :toggleable="true" v-if="id">
        <TableToolbar :show-filter-button="false" create-label="Add Variant" @create="router.push({ name: 'catalog.variants.new', query: { productId: id } })" />
        <AppDataTable :rows="variants" :loading="variantsLoading" empty-title="No variants"
          @row-click="(e: { data: VariantListItem }) => fetchPrices(e.data.id)">
          <Column field="sku" header="SKU" />
          <Column field="price" header="Price">
            <template #body="{ data }: { data: VariantListItem }">{{ data.price.toFixed(2) }}</template>
          </Column>
          <Column field="isMaster" header="Master">
            <template #body="{ data }: { data: VariantListItem }">
              <Tag v-if="data.isMaster" value="Master" severity="info" />
            </template>
          </Column>
          <template #rowActions="{ data }: { data: VariantListItem }">
            <ActionMenu :items="[
              { label: 'Edit', icon: 'pi pi-pencil', command: () => router.push({ name: 'catalog.variants.edit', params: { id: data.id } }) },
              { label: 'Prices', icon: 'pi pi-dollar', command: () => fetchPrices(data.id) },
              { label: 'Images', icon: 'pi pi-image', command: () => fetchImages(data.id) },
              { separator: true },
              { label: 'Delete', icon: 'pi pi-trash', command: () => deleteVariant(data) },
            ]" />
          </template>
        </AppDataTable>
      </Fieldset>

      <!-- Prices sub-table (shown when a variant row is clicked) -->
      <Fieldset v-if="selectedVariantId" legend="Prices">
        <AppDataTable :rows="prices" :loading="pricesLoading" empty-title="No prices set">
          <Column field="currency" header="Currency" />
          <Column field="amount" header="Amount">
            <template #body="{ data }: { data: PriceListItem }">{{ data.amount.toFixed(2) }}</template>
          </Column>
          <Column field="compareAtAmount" header="Compare At">
            <template #body="{ data }: { data: PriceListItem }">{{ data.compareAtAmount?.toFixed(2) ?? '—' }}</template>
          </Column>
          <Column field="isDefault" header="Default">
            <template #body="{ data }: { data: PriceListItem }"><Tag v-if="data.isDefault" value="Default" severity="success" /></template>
          </Column>
          <template #rowActions="{ data }: { data: PriceListItem }">
            <ActionMenu :items="[{ label: 'Delete', icon: 'pi pi-trash', command: () => deletePrice(data) }]" />
          </template>
        </AppDataTable>
      </Fieldset>
    </form>
  </template>

  <FormActions v-if="isEditable" :loading="saving" @save="save" @cancel="cancel" />
</template>
```

- [ ] **Step 4: Update catalog routes for products**

```ts
// app/Admin/src/app/routes/catalog.routes.ts — replace existing product routes in children array
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
    // Products
    {
      path: 'products',
      name: 'catalog.products.list',
      component: () => import('@/features/catalog/pages/ProductListPage.vue'),
    },
    {
      path: 'products/new',
      name: 'catalog.products.new',
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
    },
    {
      path: 'products/:id',
      name: 'catalog.products.detail',
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
    },
    {
      path: 'products/:id/edit',
      name: 'catalog.products.edit',
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
    },
    // Taxonomies
    {
      path: 'taxonomies',
      name: 'catalog.taxonomies.list',
      component: () => import('@/features/catalog/pages/TaxonomyListPage.vue'),
    },
    {
      path: 'taxonomies/new',
      name: 'catalog.taxonomies.new',
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
    },
    {
      path: 'taxonomies/:id',
      name: 'catalog.taxonomies.detail',
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
    },
    {
      path: 'taxonomies/:id/edit',
      name: 'catalog.taxonomies.edit',
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
    },
    // Option Types
    {
      path: 'option-types',
      name: 'catalog.option-types.list',
      component: () => import('@/features/catalog/pages/OptionTypeListPage.vue'),
    },
    {
      path: 'option-types/new',
      name: 'catalog.option-types.new',
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
    },
    {
      path: 'option-types/:id',
      name: 'catalog.option-types.detail',
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

- [ ] **Step 5: Remove old product files and commit**

```bash
rm app/Admin/src/features/catalog/pages/ProductCreatePage.vue
cd app/Admin && pnpm run lint 2>&1 | tail -5
```
Expect: no errors from catalog pages.

```bash
git add app/Admin/src/features/catalog/pages/ app/Admin/src/app/routes/catalog.routes.ts
git commit -m "feat: implement Product list/detail pages with inline variant sub-tables"
```

---

### Task 3: Catalog — Taxonomy Pages (with MPTT depth indentation)

**Files:**
- Create: `app/Admin/src/features/catalog/pages/TaxonomyListPage.vue`
- Create: `app/Admin/src/features/catalog/pages/TaxonomyDetailPage.vue`
- Delete: `app/Admin/src/features/catalog/pages/TaxonListPage.vue`
- Delete: `app/Admin/src/features/catalog/pages/TaxonTreeManagerPage.vue`

**Interfaces:**
- Consumes: `catalogApi` from Task 1, route structure from Task 2 step 4

- [ ] **Step 1: Create TaxonomyListPage**

```vue
<!-- app/Admin/src/features/catalog/pages/TaxonomyListPage.vue -->
<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import { default as AppDataTable } from '@/shared/components/data/DataTable.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import Column from 'primevue/column'
import { catalogApi, type TaxonomyListItem, type PagedResult } from '@/shared/api/catalog'
import { useToast } from '@/shared/composables/useToast'
import { useConfirm } from '@/shared/composables/useConfirm'

const router = useRouter()
const toast = useToast()
const { confirmDelete } = useConfirm()
const items = ref<TaxonomyListItem[]>([]); const total = ref(0)
const loading = ref(false); const error = ref('')
const page = ref(1); const pageSize = ref(20); const search = ref('')

async function fetchItems() {
  loading.value = true; error.value = ''
  try {
    const r: PagedResult<TaxonomyListItem> = await catalogApi.getTaxonomies({ page: page.value, pageSize: pageSize.value, search: search.value || undefined })
    items.value = r.items; total.value = r.total
  } catch (e) { error.value = e instanceof Error ? e.message : 'Failed to load' }
  finally { loading.value = false }
}
let ok = false
onMounted(() => { ok = true; fetchItems() })
watch([page, search], () => { if (ok) fetchItems() })
function onPage(e: { page: number }) { page.value = e.page + 1 }

async function deleteItem(t: TaxonomyListItem) {
  confirmDelete({ target: `taxonomy "${t.name}"`, onAccept: async () => {
    try { await catalogApi.deleteTaxonomy(t.id); toast.success('Deleted'); await fetchItems() }
    catch (e) { toast.error(e instanceof Error ? e.message : 'Delete failed') }
  }})
}
</script>

<template>
  <PageHeader title="Taxonomies" breadcrumb="[{ label: 'Catalog', to: '/catalog/dashboard' }, { label: 'Taxonomies' }]" />
  <TableToolbar create-label="Add Taxonomy" @create="router.push({ name: 'catalog.taxonomies.new' })" @search="search = $event" />
  <ErrorState v-if="error" :description="error" @retry="fetchItems" />
  <AppDataTable v-else :rows="items" :loading="loading" :total-records="total" :page-size="pageSize"
    :first="(page - 1) * pageSize" empty-title="No taxonomies" @page="onPage">
    <Column field="name" header="Name" sortable>
      <template #body="{ data }: { data: TaxonomyListItem }">
        <router-link :to="{ name: 'catalog.taxonomies.detail', params: { id: data.id } }" class="text-primary hover:underline font-medium">{{ data.name }}</router-link>
      </template>
    </Column>
    <Column field="presentation" header="Presentation" />
    <Column field="position" header="Position" />
    <template #rowActions="{ data }: { data: TaxonomyListItem }">
      <ActionMenu :items="[
        { label: 'Edit', icon: 'pi pi-pencil', command: () => router.push({ name: 'catalog.taxonomies.edit', params: { id: data.id } }) },
        { label: 'Delete', icon: 'pi pi-trash', command: () => deleteItem(data) },
      ]" />
    </template>
  </AppDataTable>
</template>
```

- [ ] **Step 2: Create TaxonomyDetailPage with indented taxon sub-table**

```vue
<!-- app/Admin/src/features/catalog/pages/TaxonomyDetailPage.vue -->
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import { default as AppDataTable } from '@/shared/components/data/DataTable.vue'
import TableToolbar from '@/shared/components/layout/TableToolbar.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import InputText from 'primevue/inputtext'
import Fieldset from 'primevue/fieldset'
import Button from 'primevue/button'
import { catalogApi, type TaxonomyDetail, type TaxonListItem } from '@/shared/api/catalog'
import { useToast } from '@/shared/composables/useToast'
import { useConfirm } from '@/shared/composables/useConfirm'

const route = useRoute(); const router = useRouter()
const toast = useToast(); const { confirmDelete } = useConfirm()
const id = computed(() => route.params.id as string | undefined)
const mode = computed<'create'|'view'|'edit'>(() => !id.value ? 'create' : String(route.name).endsWith('.edit') ? 'edit' : 'view')
const isEditable = computed(() => mode.value === 'edit' || mode.value === 'create')

const item = ref<Partial<TaxonomyDetail>>({ name: '', presentation: '' })
const loading = ref(false); const saving = ref(false); const loadError = ref('')
const errors = ref<Record<string, string>>({})

async function load() { if (!id.value) return; loading.value = true; loadError.value = ''
  try { item.value = await catalogApi.getTaxonomy(id.value) }
  catch (e) { loadError.value = e instanceof Error ? e.message : 'Failed to load' }
  finally { loading.value = false } }

function validate(): boolean {
  errors.value = {}
  if (!item.value.name?.trim()) errors.value.name = 'Required'
  return Object.keys(errors.value).length === 0
}

async function save() { if (!validate()) return; saving.value = true
  try {
    if (mode.value === 'create') { const c = await catalogApi.createTaxonomy(item.value); toast.success('Created'); router.replace({ name: 'catalog.taxonomies.detail', params: { id: c.id } }) }
    else { await catalogApi.updateTaxonomy(id.value!, item.value); toast.success('Updated'); router.replace({ name: 'catalog.taxonomies.detail', params: { id: id.value! } }) }
  } catch (e) { toast.error(e instanceof Error ? e.message : 'Save failed') }
  finally { saving.value = false } }

function cancel() { if (id.value) router.push({ name: 'catalog.taxonomies.detail', params: { id: id.value } }); else router.push({ name: 'catalog.taxonomies.list' }) }

// --- Taxon sub-table with depth indentation ---
const taxons = ref<TaxonListItem[]>([])
const taxonsLoading = ref(false)

async function fetchTaxons() {
  if (!id.value || mode.value === 'create') return; taxonsLoading.value = true
  try { taxons.value = (await catalogApi.getTaxons(id.value)).sort((a, b) => a.lft - b.lft) }
  catch { taxons.value = [] }
  finally { taxonsLoading.value = false }
}

async function deleteTaxon(t: TaxonListItem) {
  confirmDelete({ target: `taxon "${t.name}"`, onAccept: async () => {
    try { await catalogApi.deleteTaxon(id.value!, t.id); toast.success('Deleted'); await fetchTaxons() }
    catch (e) { toast.error(e instanceof Error ? e.message : 'Delete failed') }
  }})
}

onMounted(async () => { await load(); await fetchTaxons() })

const bc = computed(() => mode.value === 'create'
  ? [{ label: 'Catalog', to: '/catalog/dashboard' }, { label: 'Taxonomies', to: '/catalog/taxonomies' }, { label: 'New' }]
  : [{ label: 'Catalog', to: '/catalog/dashboard' }, { label: 'Taxonomies', to: '/catalog/taxonomies' }, { label: item.value.name || 'Taxonomy' }])
</script>

<template>
  <PageHeader :title="mode === 'create' ? 'New Taxonomy' : (item.name || 'Taxonomy')" :breadcrumb="bc">
    <template v-if="mode === 'view'" #actions>
      <Button label="Edit" icon="pi pi-pencil" @click="router.push({ name: 'catalog.taxonomies.edit', params: { id } })" />
    </template>
  </PageHeader>
  <ErrorState v-if="loadError" :description="loadError" @retry="load" />
  <template v-else>
    <form class="flex flex-col gap-6" @submit.prevent="save">
      <div class="rounded-border border border-surface-200 dark:border-surface-700 p-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="Name" :error="errors.name" required>
            <InputText v-model="item.name" :disabled="!isEditable" class="w-full" :invalid="!!errors.name" />
          </FormField>
          <FormField label="Presentation">
            <InputText v-model="item.presentation" :disabled="!isEditable" class="w-full" />
          </FormField>
        </div>
      </div>

      <!-- Taxon sub-table with depth indentation -->
      <Fieldset v-if="id" legend="Taxons" :toggleable="true">
        <TableToolbar :show-filter-button="false" create-label="Add Taxon" @create="router.push({ name: 'catalog.taxons.new', query: { taxonomyId: id } })" />
        <div class="rounded-border border border-surface-200 dark:border-surface-700 overflow-hidden">
          <AppDataTable :rows="taxons" :loading="taxonsLoading" empty-title="No taxons in this taxonomy">
            <Column field="name" header="Name">
              <template #body="{ data }: { data: TaxonListItem }">
                <span :style="{ paddingLeft: data.depth * 24 + 'px', display: 'inline-flex', alignItems: 'center', gap: '4px' }">
                  <span v-if="data.depth > 0" class="text-surface-400 text-xs" style="font-family: monospace">
                    {{ Array(data.depth).fill('').map((_, i) => i === data.depth - 1 ? '├─' : '│ ').join('') }}
                  </span>
                  <span>{{ data.name }}</span>
                  <Tag v-if="data.hideFromNav" value="Hidden" severity="warn" class="ml-1 !text-xs" />
                  <Tag v-if="data.automatic" value="Auto" severity="info" class="ml-1 !text-xs" />
                </span>
              </template>
            </Column>
            <Column field="slug" header="Slug" />
            <Column field="position" header="Pos" style="width:4rem" />
            <template #rowActions="{ data }: { data: TaxonListItem }">
              <ActionMenu :items="[
                { label: 'Edit', icon: 'pi pi-pencil', command: () => router.push({ name: 'catalog.taxons.edit', params: { taxonomyId: id, id: data.id } }) },
                { label: 'Delete', icon: 'pi pi-trash', command: () => deleteTaxon(data) },
              ]" />
            </template>
          </AppDataTable>
        </div>
      </Fieldset>
    </form>
  </template>
  <FormActions v-if="isEditable" :loading="saving" @save="save" @cancel="cancel" />
</template>
```

- [ ] **Step 3: Remove old taxon files and commit**

```bash
rm app/Admin/src/features/catalog/pages/TaxonListPage.vue
rm app/Admin/src/features/catalog/pages/TaxonTreeManagerPage.vue
cd app/Admin && pnpm run lint 2>&1 | tail -5
```

```bash
git add app/Admin/src/features/catalog/pages/
git commit -m "feat: implement Taxonomy list/detail pages with MPTT depth-indented taxon sub-table"
```

---

### Task 4: Catalog — OptionType Pages + Dashboard

**Files:**
- Modify: `app/Admin/src/features/catalog/pages/OptionTypeListPage.vue`
- Create: `app/Admin/src/features/catalog/pages/OptionTypeDetailPage.vue`
- Delete: `app/Admin/src/features/catalog/pages/OptionValueListPage.vue`
- Modify: `app/Admin/src/features/catalog/pages/DashboardPage.vue`

- [ ] **Step 1: Implement OptionTypeListPage**

Replace the `PlaceholderPage` stub in `OptionTypeListPage.vue` with a real list page following the same pattern as `TaxonomyListPage` — `PageHeader` + `TableToolbar` + `AppDataTable` with Name, Presentation, Position, Filterable columns. Uses `catalogApi.getOptionTypes`. Row click navigates to `catalog.option-types.detail`, row actions for edit/delete.

- [ ] **Step 2: Implement OptionTypeDetailPage with inline OptionValues sub-table**

Create `OptionTypeDetailPage.vue` following the `TaxonomyDetailPage` pattern — entity form (Name, Presentation, Filterable toggle) + `<Fieldset>` for OptionValues sub-table (Name, Position columns). Sub-entity CRUD via `catalogApi.getOptionValues`/`createOptionValue`/`updateOptionValue`/`deleteOptionValue`.

- [ ] **Step 3: Implement DashboardPage**

Replace stub with `StatCard` grid showing catalog KPIs (total products, total taxonomies, total option types). Fetch counts from backend dashboard endpoint.

- [ ] **Step 4: Remove old file and commit**

```bash
rm app/Admin/src/features/catalog/pages/OptionValueListPage.vue
cd app/Admin && pnpm run lint 2>&1 | tail -5
```

```bash
git add app/Admin/src/features/catalog/pages/
git commit -m "feat: implement OptionType list/detail pages with inline option values; Catalog Dashboard"
```

---

### Task 5: Inventory Pages

**Files:**
- Modify: `app/Admin/src/features/inventory/pages/StockListPage.vue`
- Modify: `app/Admin/src/features/inventory/pages/LocationListPage.vue`
- Create: `app/Admin/src/features/inventory/pages/LocationDetailPage.vue`
- Modify: `app/Admin/src/features/inventory/pages/MovementListPage.vue`
- Modify: `app/Admin/src/features/inventory/pages/TransferListPage.vue`
- Create: `app/Admin/src/features/inventory/pages/TransferDetailPage.vue`
- Modify: `app/Admin/src/features/inventory/pages/DashboardPage.vue`
- Delete: `app/Admin/src/features/inventory/pages/StockImportPage.vue`
- Delete: `app/Admin/src/features/inventory/pages/UnitListPage.vue`
- Modify: `app/Admin/src/app/routes/inventory.routes.ts`

**Interfaces:**
- Consumes: `inventoryApi` from Task 1

- [ ] **Step 1: Replace StockListPage stub**

List page showing stock items with columns: Variant Name, SKU, Count On Hand, Location, Backorderable. Consumes `inventoryApi.getStockItems`. View-only — no create/edit/detail page for stock items (managed via stock movements and transfers).

- [ ] **Step 2: Replace LocationListPage stub + create LocationDetailPage**

`LocationListPage`: standard list with Name, Code, Active, Default columns. Row click → `inventory.locations.detail`.

`LocationDetailPage`: entity form — Name, Code, Address, City, Phone, Country, Admin Name, Position, toggles (Active, Default, PropagateAllVariants, Backorderable Default), Low Stock Threshold number input.

- [ ] **Step 3: Replace MovementListPage stub**

Read-only log list: Quantity, Previous Count, Action, Variant, Reason, Date. No detail page, no create/edit/delete.

- [ ] **Step 4: Replace TransferListPage stub + create TransferDetailPage**

`TransferListPage`: standard list with Number, State, Source, Destination, Date. Row click → `inventory.transfers.detail`.

`TransferDetailPage`: entity fields (Number, Reference, State, Source/Destination locations) + `<Fieldset>` for Transfer Items sub-table (Variant, Quantity, Received Quantity). Action buttons for Receive/Cancel.

- [ ] **Step 5: Update inventory routes**

```ts
// app/Admin/src/app/routes/inventory.routes.ts — key changes:
// REMOVE: stocks/import → StockImportPage, units → UnitListPage
// ADD: locations/new, locations/:id, locations/:id/edit → LocationDetailPage
// ADD: transfers/new, transfers/:id, transfers/:id/edit → TransferDetailPage
```

- [ ] **Step 6: Implement DashboardPage**

`StatCard` grid: Total Stock Items, Low Stock Alerts, Active Locations, Pending Transfers.

- [ ] **Step 7: Remove old files and commit**

```bash
rm app/Admin/src/features/inventory/pages/StockImportPage.vue
rm app/Admin/src/features/inventory/pages/UnitListPage.vue
cd app/Admin && pnpm run lint 2>&1 | tail -5
```

```bash
git add app/Admin/src/features/inventory/pages/ app/Admin/src/app/routes/inventory.routes.ts
git commit -m "feat: implement Inventory list/detail pages; remove StockImport and Unit pages"
```

---

### Task 6: Ordering Pages

**Files:**
- Modify: `app/Admin/src/features/ordering/pages/OrderListPage.vue`
- Create: `app/Admin/src/features/ordering/pages/OrderDetailPage.vue`
- Delete: `app/Admin/src/features/ordering/pages/OrderCreatePage.vue`
- Modify: `app/Admin/src/features/ordering/pages/FulfillmentQueuePage.vue`
- Modify: `app/Admin/src/features/ordering/pages/DashboardPage.vue`
- Modify: `app/Admin/src/app/routes/ordering.routes.ts`

- [ ] **Step 1: Replace OrderListPage stub**

Standard list: Number, Status (Tag), Total, Email, Date. Row click → `ordering.orders.detail`.

- [ ] **Step 2: Create OrderDetailPage**

Entity form: Number (read-only), Status (select), Email, Special Instructions + `<Fieldset>` for Line Items sub-table (Variant, SKU, Quantity, Price, Total). Action buttons for Cancel/Complete/Approve based on status.

- [ ] **Step 3: Replace FulfillmentQueuePage stub**

Read-only queue list: Order Number, Status, Shipping Method. Row click navigates to order detail.

- [ ] **Step 4: Replace DashboardPage stub**

`StatCard` grid: Total Orders, Today's Orders, Pending Fulfillment, Revenue.

- [ ] **Step 5: Update ordering routes + remove old file + commit**

Remove `orders/create → OrderCreatePage`. Add `orders/new`, `orders/:id`, `orders/:id/edit → OrderDetailPage`.

```bash
rm app/Admin/src/features/ordering/pages/OrderCreatePage.vue
cd app/Admin && pnpm run lint
git add app/Admin/src/features/ordering/pages/ app/Admin/src/app/routes/ordering.routes.ts
git commit -m "feat: implement Ordering list/detail pages; merge create into DetailPage"
```

---

### Task 7: Payment Pages

**Files:**
- Modify: `app/Admin/src/features/payment/pages/PaymentListPage.vue`
- Create: `app/Admin/src/features/payment/pages/PaymentDetailPage.vue`
- Modify: `app/Admin/src/features/payment/pages/PaymentMethodListPage.vue`
- Create: `app/Admin/src/features/payment/pages/PaymentMethodDetailPage.vue`
- Modify: `app/Admin/src/app/routes/payment.routes.ts`

- [ ] **Step 1: Replace PaymentListPage stub + create PaymentDetailPage**

`PaymentListPage`: Amount, Currency, Status (Tag), Method, Order Number, Date. PaymentDetailPage shows full payment details — Amount, Currency, Status, Method, Order, plus transaction history if available.

- [ ] **Step 2: Replace PaymentMethodListPage stub + create PaymentMethodDetailPage**

Standard list + detail: Name, Code, Active, Position. Detail form for create/edit.

- [ ] **Step 3: Update payment routes + commit**

Add `list/new`, `list/:id`, `list/:id/edit` for payments. Add `methods/new`, `methods/:id`, `methods/:id/edit` for methods.

```bash
cd app/Admin && pnpm run lint
git add app/Admin/src/features/payment/pages/ app/Admin/src/app/routes/payment.routes.ts
git commit -m "feat: implement Payment list/detail pages"
```

---

### Task 8: Shipping Pages

**Files:**
- Modify: `app/Admin/src/features/shipping/pages/ShippingMethodListPage.vue`
- Create: `app/Admin/src/features/shipping/pages/ShippingMethodDetailPage.vue`
- Modify: `app/Admin/src/features/shipping/pages/ShippingRateListPage.vue`
- Create: `app/Admin/src/features/shipping/pages/ShippingRateDetailPage.vue`
- Modify: `app/Admin/src/app/routes/shipping.routes.ts`

- [ ] **Step 1: Implement ShippingMethod list + detail pages**

Standard list + detail: Name, Code, Active, Position. Detail adds Description, Tracking URL.

- [ ] **Step 2: Implement ShippingRate list + detail pages**

Standard list + detail: Name, Amount, Currency, Method, Zone. Detail adds Min/Max Order Amount and Weight constraints.

- [ ] **Step 3: Update routes + commit**

```bash
git add app/Admin/src/features/shipping/pages/ app/Admin/src/app/routes/shipping.routes.ts
git commit -m "feat: implement Shipping list/detail pages"
```

---

### Task 9: Location Pages

**Files:**
- Modify: `app/Admin/src/features/location/pages/CountryListPage.vue`
- Create: `app/Admin/src/features/location/pages/CountryDetailPage.vue`
- Modify: `app/Admin/src/features/location/pages/StateListPage.vue`
- Create: `app/Admin/src/features/location/pages/StateDetailPage.vue`
- Modify: `app/Admin/src/app/routes/location.routes.ts`

- [ ] **Step 1: Implement Country list + detail pages**

Standard list + detail: Name, ISO Code, Active. States appear as inline sub-table on Country detail.

- [ ] **Step 2: Implement State list + detail pages**

Standard list + detail: Name, Abbreviation, Country.

- [ ] **Step 3: Update routes + commit**

```bash
git add app/Admin/src/features/location/pages/ app/Admin/src/app/routes/location.routes.ts
git commit -m "feat: implement Location list/detail pages"
```

---

### Task 10: Users Pages

**Files:**
- Modify: `app/Admin/src/features/users/pages/StaffListPage.vue`
- Create: `app/Admin/src/features/users/pages/StaffDetailPage.vue`
- Delete: `app/Admin/src/features/users/pages/StaffCreatePage.vue`
- Modify: `app/Admin/src/features/users/pages/CustomerListPage.vue`
- Create: `app/Admin/src/features/users/pages/CustomerDetailPage.vue`
- Modify: `app/Admin/src/features/users/pages/RoleListPage.vue`
- Create: `app/Admin/src/features/users/pages/RoleDetailPage.vue`
- Modify: `app/Admin/src/features/users/pages/PermissionListPage.vue`
- Create: `app/Admin/src/features/users/pages/PermissionDetailPage.vue`
- Modify: `app/Admin/src/app/routes/users.routes.ts`

- [ ] **Step 1: Implement Staff list + detail pages**

Standard list + detail. StaffList: Name, Email, Role, Status. StaffDetail: user fields + role assignment.

- [ ] **Step 2: Implement Customer list + detail pages**

Standard list + detail. CustomerList: Name, Email, Orders, Joined. CustomerDetail: profile info + orders sub-table.

- [ ] **Step 3: Implement Role list + detail pages**

Standard list + detail: Name, Description. Detail includes permissions assignment checkboxes.

- [ ] **Step 4: Implement Permission list + detail pages**

Standard list + detail: Name, Description, Group.

- [ ] **Step 5: Update routes + remove old file + commit**

```bash
rm app/Admin/src/features/users/pages/StaffCreatePage.vue
cd app/Admin && pnpm run lint
git add app/Admin/src/features/users/pages/ app/Admin/src/app/routes/users.routes.ts
git commit -m "feat: implement Users list/detail pages; merge Staff create into DetailPage"
```

---

### Task 11: Profile + Reports Pages

**Files:**
- Modify: `app/Admin/src/features/profile/pages/ProfilePage.vue`
- Modify: `app/Admin/src/features/profile/pages/AddressListPage.vue`
- Modify: `app/Admin/src/features/reports/pages/DashboardPage.vue`

- [ ] **Step 1: Implement ProfilePage with real form**

User profile fields: Name, Email, Avatar (image upload), Bio, Preferences. Edit-only (no create mode). Uses `profileApi` from Task 1.

- [ ] **Step 2: Implement AddressListPage**

Address list sub-page: Street, City, State, Country, Type (Shipping/Billing), Default flag. Inline add/edit since addresses are sub-entities of the user profile.

- [ ] **Step 3: Implement Reports DashboardPage**

`StatCard` grid: Total Revenue, Total Orders, New Customers, Conversion Rate (mock data or from reports API).

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/profile/pages/ app/Admin/src/features/reports/pages/
git commit -m "feat: implement Profile and Reports pages"
```

---

### Task 12: Menu Config Cleanup

**File:**
- Modify: `app/Admin/src/app/config/admin-menu.config.ts`

**Changes:** Remove 8 entries, rename 1 entry (Manager → renamed to point to TaxonomyListPage)

- [ ] **Step 1: Remove dropped menu entries**

```ts
// In admin-menu.config.ts — remove these entries:
// 1. "Add Product" (catalog.products.create → gone)
// 2. "All Categories" (catalog.taxa.list → gone)
// 3. "Manager" → change route from TreeManagerPage to TaxonomyListPage
//    { label: 'Manager', ... to: { name: 'catalog.taxonomies.list' } }
// 4. "Values" (catalog.option-values.list → gone)
// 5. "Import" (inventory.stocks.import → gone)
// 6. "Stock Units" (inventory.units.list → gone)
// 7. "Create Order" (ordering.orders.create → gone)
// 8. "Invite Staff" (users.staff.create → gone)
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/app/config/admin-menu.config.ts
git commit -m "chore: clean up admin menu — remove entries for dropped pages"
```

---

### Task 13: Final Verification

- [ ] **Step 1: Run linter**

```bash
cd app/Admin && pnpm run lint 2>&1
```
Expect: zero errors.

- [ ] **Step 2: Run type check**

```bash
cd app/Admin && npx vue-tsc --noEmit 2>&1
```
Expect: zero type errors.

- [ ] **Step 3: Run unit tests**

```bash
cd app/Admin && pnpm run test:unit 2>&1
```
Expect: all existing tests pass. Smoke tests for each new page verify they mount without crashing.

- [ ] **Step 4: Build**

```bash
cd app/Admin && pnpm run build 2>&1
```
Expect: build succeeds with no warnings.

- [ ] **Step 5: Final commit**

```bash
git add -A
git commit -m "chore: all modules verified — lint, type-check, tests, build pass"
```
