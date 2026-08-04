# Admin Inventory Views — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace 8 Inventory placeholder views with functional CRUD UIs following the Catalog/Location pattern.

**Architecture:** 3 list+detail pairs (StockItems, StockLocations, StockTransfers) + 2 read-only lists (StockReservations, StockMovements). Detailed views have main-form + related-data tabs (StockItemDetail: Levels tab, StockLocationDetail: Items tab, StockTransferDetail: Line Items tab). All views use existing stores/services/types.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue (DataTable, Form, Tabs, Card, Select, ToggleSwitch), existing `StockItemApi`/etc.

**Global Constraints:**
- Follows established Catalog/Location view patterns
- All services, types, validations, and stores already exist
- View files already exist as placeholders — modify in place

---

## File Structure (modified files only)

```
app/Admin/src/features/inventory/views/
├── StockItemsList.vue       # Replace placeholder
├── StockItemDetail.vue      # Replace placeholder (Form + Stock Levels tabs)
├── StockLocationsList.vue   # Replace placeholder
├── StockLocationDetail.vue  # Replace placeholder (Form + Stock Items tabs)
├── StockReservationsList.vue # Replace placeholder (read-only, no detail)
├── StockTransfersList.vue    # Replace placeholder
├── StockTransferDetail.vue   # Replace placeholder (Form + Line Items tabs)
└── StockMovementsList.vue   # Replace placeholder (read-only, no detail)
```

---

### Task 1: StockItemsList.vue

**Files:**
- Modify: `app/Admin/src/features/inventory/views/StockItemsList.vue`

**Interfaces:**
- Consumes: `StockItemApi.getStockItems(query)` → `PagedResult<StockItemListItem>`, `deleteStockItem(id)` → `Result<void>`
- Consumes: `STOCK_ITEM_FILTER_FIELDS`, `STOCK_ITEM_SORT_FIELDS`, `STOCK_ITEM_SEARCH_FIELDS` from `../types/stockItem`
- Consumes: `INVENTORY` from `@/shared/constants/api` — `${INVENTORY}/stock-items`

- [ ] **Step 1: Write StockItemsList.vue**

Standard list view pattern (see `UsersList.vue` or `ProductsList.vue` for template). Columns: Variant Name (from nested variant object display), SKU, Reorder Point, Reorder Quantity, Low Stock badge (`lowStock ? 'warn' : 'success'`), Actions (Edit, Delete). Search across SKU. Create → `/inventory/stock-items/new`. Edit → `/inventory/stock-items/:id`.

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { INVENTORY } from '@/shared/constants/api'
import { StockItemApi } from '../services/stockItemApi'
import type { StockItemListItem } from '../types/stockItem'
import {
  STOCK_ITEM_FILTER_FIELDS,
  STOCK_ITEM_SORT_FIELDS,
  STOCK_ITEM_SEARCH_FIELDS,
} from '../types/stockItem'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { dt, exportCSV } = useDataTableExport()
const search = ref('')
const selectedItems = ref<StockItemListItem[]>([])

const { items, loading, setSearch, refresh } = usePagedQuery<StockItemListItem>(
  `${INVENTORY}/stock-items`,
  {
    allowedFilterFields: STOCK_ITEM_FILTER_FIELDS,
    allowedSortFields: STOCK_ITEM_SORT_FIELDS,
    allowedSearchFields: STOCK_ITEM_SEARCH_FIELDS,
    defaultSearchFields: ['sku'],
  },
)

function onSearch(value: string) { search.value = value; setSearch(value) }
function clearSearch() { search.value = ''; setSearch('') }
function navigateToNew() { router.push('/inventory/stock-items/new') }
function navigateToEdit(id: string) { router.push(`/inventory/stock-items/${id}`) }

function confirmDelete() {
  confirm.require({
    message: `Delete selected stock items?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      for (const item of selectedItems.value) {
        const result = await StockItemApi.deleteStockItem(item.id)
        if (result.isSuccess) notify.success('Deleted', item.id)
        else notify.error('Failed', result.message ?? '')
      }
      selectedItems.value = []
      refresh()
    },
  })
}
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Stock Items</h1>
      <p class="text-muted-color">Manage product inventory</p>
    </div>
    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText :model-value="search" placeholder="Search by SKU..." @update:model-value="onSearch" />
      </IconField>
      <Button v-if="search" label="Clear" severity="secondary" icon="pi pi-times" @click="clearSearch" />
      <div class="flex-1" />
      <Button label="New Stock Item" icon="pi pi-plus" @click="navigateToNew" />
      <Button label="Reload" icon="pi pi-refresh" severity="secondary" @click="refresh" />
      <Button label="Export" icon="pi pi-download" severity="secondary" @click="exportCSV" />
    </div>
    <DataTable
      ref="dt"
      v-model:selection="selectedItems"
      :value="items"
      :loading="loading"
      scrollable paginator :rows="20" :rows-per-page-options="[10,20,50]"
      data-key="id"
    >
      <Column selection-mode="multiple" header-style="width:3rem" />
      <Column field="sku" header="SKU" :sortable="true" />
      <Column field="reorderPoint" header="Reorder Point" :sortable="true" />
      <Column field="reorderQuantity" header="Reorder Qty" :sortable="true" />
      <Column field="lowStock" header="Low Stock">
        <template #body="{ data }">
          <Tag :value="data.lowStock ? 'Yes' : 'No'" :severity="data.lowStock ? 'warn' : 'success'" />
        </template>
      </Column>
      <Column header="Actions" header-style="width:8rem">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" severity="secondary" text rounded @click="navigateToEdit(data.id)" />
          <Button icon="pi pi-trash" severity="danger" text rounded @click="selectedItems = [data]; confirmDelete()" />
        </template>
      </Column>
      <template #empty>No stock items found.</template>
    </DataTable>
  </div>
</template>
```

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/inventory/views/StockItemsList.vue
git commit -m "feat(inventory): implement stock items list view"
```

---

### Task 2: StockItemDetail.vue

**Files:**
- Modify: `app/Admin/src/features/inventory/views/StockItemDetail.vue`

**Interfaces:**
- Consumes: `StockItemApi.getStockItem(id)` → `Result<StockItemDetail>`, `createStockItem(request)` → `Result<StockItemDetail>`, `updateStockItem(id, request)` → `Result<StockItemDetail>`
- Consumes: `StockItemApi.getStockLevels(id)` → `PagedResult<StockLevel>` for the Levels tab
- Consumes: `stockItemSchema`, `StockItemForm` from `../validations/stockItem`
- Consumes: `useVariantStore»fetchActive()` → `activeVariants` for variant selector dropdown

- [ ] **Step 1: Write StockItemDetail.vue**

Two-tab view: **Tab 0 (Main Form)** — Variant selector (`Select` from `variantStore.activeVariants`), SKU (`InputText`), Reorder Point (`InputNumber`), Reorder Quantity (`InputNumber`). **Tab 1 (Stock Levels)** — Read-only DataTable with Location Name, Quantity On Hand, Reserved Quantity columns.

```vue
<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Select from 'primevue/select'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Message from 'primevue/message'
import { Form, FormField, type FormSubmitEvent, zodResolver } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { useVariantStore } from '@/features/catalog/stores/variantStore'
import { StockItemApi } from '../services/stockItemApi'
import { stockItemSchema, type StockItemForm } from '../validations/stockItem'
import type { StockLevel } from '../types/stockItem'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()
const variantStore = useVariantStore()

const resolver = zodResolver(stockItemSchema)
const form = ref<StockItemForm>({ variantId: '', sku: '', reorderPoint: 0, reorderQuantity: 0 })
const formLoaded = ref(false)
const loading = ref(false)
const activeTab = ref('0')
const stockLevels = ref<StockLevel[]>([])
const levelsLoaded = ref(false)

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Stock Item' : 'New Stock Item')

async function initEditMode(id: string) {
  const result = await StockItemApi.getStockItem(id)
  if (result.isSuccess) {
    const s = result.value
    form.value = { variantId: s.variantId ?? '', sku: s.sku, reorderPoint: s.reorderPoint ?? 0, reorderQuantity: s.reorderQuantity ?? 0 }
    formLoaded.value = true
  } else { handleResult(result); router.push('/inventory/stock-items') }
}

async function loadLevels() {
  if (levelsLoaded.value) return
  const result = await StockItemApi.getStockLevels(route.params.id as string)
  if (result.isSuccess) { stockLevels.value = result.items; levelsLoaded.value = true }
}

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return
  loading.value = true
  const data = event.values as StockItemForm
  const request = { variantId: data.variantId, sku: data.sku, reorderPoint: data.reorderPoint, reorderQuantity: data.reorderQuantity }
  const result = isEdit.value
    ? await StockItemApi.updateStockItem(route.params.id as string, request as any)
    : await StockItemApi.createStockItem(request as any)
  loading.value = false
  if (result.isSuccess) {
    notify.success(pageTitle.value, 'Saved')
    if (!isEdit.value && result.value) router.replace(`/inventory/stock-items/${(result.value as any).id}`)
  } else { handleResult(result) }
}

onMounted(async () => {
  await variantStore.fetchActive()
  if (isEdit.value) initEditMode(route.params.id as string)
  else formLoaded.value = true
})
watch(() => route.params.id, (newId) => { if (newId && newId !== 'new') initEditMode(newId as string) })
watch(activeTab, (tab) => { if (isEdit.value && tab === '1') loadLevels() })
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex items-center gap-4 mb-6">
      <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="router.push('/inventory/stock-items')" />
      <h1 class="text-2xl font-semibold">{{ pageTitle }}</h1>
    </div>

    <Form id="stock-item-form" :key="String(formLoaded)" :resolver="resolver" :initial-values="form" @submit="onSubmit">
      <Tabs v-model:value="activeTab">
        <TabList>
          <Tab value="0">General</Tab>
          <Tab v-if="isEdit" value="1">Stock Levels</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="0">
            <Card>
              <template #content>
                <div class="flex flex-col gap-4">
                  <FormField v-slot="$field" name="variantId" class="flex flex-col gap-1">
                    <label>Product Variant <span class="text-red-500">*</span></label>
                    <Select :options="variantStore.activeVariants" option-label="name" option-value="id" fluid show-clear />
                    <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                  </FormField>
                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <FormField v-slot="$field" name="sku" class="flex flex-col gap-1">
                      <label>SKU <span class="text-red-500">*</span></label>
                      <InputText fluid />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                    <FormField v-slot="$field" name="reorderPoint" class="flex flex-col gap-1">
                      <label>Reorder Point</label>
                      <InputNumber fluid :min="0" />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                    <FormField v-slot="$field" name="reorderQuantity" class="flex flex-col gap-1">
                      <label>Reorder Quantity</label>
                      <InputNumber fluid :min="0" />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                  </div>
                </div>
              </template>
            </Card>
          </TabPanel>
          <TabPanel v-if="isEdit" value="1">
            <Card>
              <template #content>
                <DataTable :value="stockLevels" scrollable>
                  <Column field="locationName" header="Location" />
                  <Column field="quantityOnHand" header="On Hand" />
                  <Column field="reservedQuantity" header="Reserved" />
                  <template #empty>No stock levels recorded.</template>
                </DataTable>
              </template>
            </Card>
          </TabPanel>
        </TabPanels>
      </Tabs>
    </Form>

    <div class="flex gap-3 mt-4">
      <Button label="Save" icon="pi pi-check" form="stock-item-form" type="submit" :loading="loading" />
      <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="router.push('/inventory/stock-items')" />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/inventory/views/StockItemDetail.vue
git commit -m "feat(inventory): implement stock item detail view with stock levels tab"
```

---

### Task 3: StockLocationsList.vue

**Files:**
- Modify: `app/Admin/src/features/inventory/views/StockLocationsList.vue`

**Interfaces:**
- Consumes: `StockLocationApi.getStockLocations(query)` → `PagedResult<StockLocationListItem>`, `deleteStockLocation(id)` → `Result<void>`
- Consumes: `STOCK_LOCATION_FILTER_FIELDS`, `STOCK_LOCATION_SORT_FIELDS`, `STOCK_LOCATION_SEARCH_FIELDS`
- Consumes: `INVENTORY` → `${INVENTORY}/stock-locations`

- [ ] **Step 1: Write StockLocationsList.vue**

Standard list view. Columns: Name, Type, Is Active (Tag badge), Actions (Edit, Delete). Search by name. Create → `/inventory/stock-locations/new`.

(Follow same pattern as Task 1 UsersList with `StockLocationApi`, `StockLocationListItem`, and path prefix `/inventory/stock-locations`. See `UsersList.vue` template — substitute entity names and API calls.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/inventory/views/StockLocationsList.vue
git commit -m "feat(inventory): implement stock locations list view"
```

---

### Task 4: StockLocationDetail.vue

**Files:**
- Modify: `app/Admin/src/features/inventory/views/StockLocationDetail.vue`

**Interfaces:**
- Consumes: `StockLocationApi.getStockLocation(id)`, `createStockLocation(request)`, `updateStockLocation(id, request)`
- Consumes: `StockLocationApi.getLocationStockItems(id)` → `PagedResult<LocationStockItem>` for the Items tab
- Consumes: `stockLocationSchema`, `StockLocationForm` from `../validations/stockLocation`

- [ ] **Step 1: Write StockLocationDetail.vue**

Two-tab view: **Tab 0 (Main Form)** — Name, Type (`Select`: Warehouse/Store/Returns), Is Active (`ToggleSwitch`). Address fields group (optional): Street, City, State, Country, Postal Code. **Tab 1 (Stock Items)** — Read-only DataTable: Variant Name, SKU, Quantity On Hand.

(Follow same tabbed detail pattern as `StockItemDetail.vue`: onMounted fetches detail on edit, watch tabs for lazy load, form+tab structure. Address fields are simple InputTexts. Type uses a hardcoded array `['Warehouse','Store','Returns']` as Select options.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/inventory/views/StockLocationDetail.vue
git commit -m "feat(inventory): implement stock location detail view with stock items tab"
```

---

### Task 5: StockReservationsList.vue

**Files:**
- Modify: `app/Admin/src/features/inventory/views/StockReservationsList.vue`

**Interfaces:**
- Consumes: `StockReservationApi.getStockReservations(query)` → `PagedResult<StockReservationListItem>`
- Consumes: `STOCK_RESERVATION_FILTER_FIELDS`, `STOCK_RESERVATION_SORT_FIELDS`
- Note: Read-only — no create/edit/delete buttons

- [ ] **Step 1: Write StockReservationsList.vue**

Read-only DataTable. Columns: Variant (name from nested data), Order #, Quantity, State (Tag badge: Reserved=info/Confirmed=success/Released=warn), Expires At, Created At. Filter by State dropdown. No New/Edit/Delete buttons. Search, Reload, Export only.

Replace the toolbar section: omit New button, keep search + reload + export.

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/inventory/views/StockReservationsList.vue
git commit -m "feat(inventory): implement stock reservations list view (read-only)"
```

---

### Task 6: StockTransfersList.vue

**Files:**
- Modify: `app/Admin/src/features/inventory/views/StockTransfersList.vue`

**Interfaces:**
- Consumes: `StockTransferApi.getStockTransfers(query)` → `PagedResult<StockTransferListItem>`, `deleteStockTransfer(id)` → `Result<void>`

- [ ] **Step 1: Write StockTransfersList.vue**

Standard list view. Columns: Source Location, Destination Location, Status (Tag badge: Draft/warn, InTransit/info, Completed/success, Cancelled/danger), Created At, Actions (Edit, Delete). Create → `/inventory/stock-transfers/new`.

(Standard pattern — see `UsersList.vue` and substitute entity.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/inventory/views/StockTransfersList.vue
git commit -m "feat(inventory): implement stock transfers list view"
```

---

### Task 7: StockTransferDetail.vue

**Files:**
- Modify: `app/Admin/src/features/inventory/views/StockTransferDetail.vue`

**Interfaces:**
- Consumes: `StockTransferApi.getStockTransfer(id)`, `createStockTransfer(request)`, `updateStockTransfer(id, request)`
- Consumes: `StockTransferApi.getTransferLines(id)` → `PagedResult<StockTransferLine>` for Line Items tab
- Consumes: `StockLocationApi.getStockLocations(...)` → for location selectors
- Consumes: `stockTransferSchema`, `StockTransferForm` from `../validations/stockTransfer`

- [ ] **Step 1: Write StockTransferDetail.vue**

Two-tab view: **Tab 0 (Header)** — Source Location (`Select`), Destination Location (`Select`), Status (`Select`: Draft/InTransit/Completed/Cancelled), dates. **Tab 1 (Line Items)** — Read-only DataTable: Stock Item, Quantity.

(Standard tabbed detail pattern. Location selects load from `StockLocationApi.getStockLocations({pageSize:100})`. Status uses hardcoded string array.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/inventory/views/StockTransferDetail.vue
git commit -m "feat(inventory): implement stock transfer detail view with line items tab"
```

---

### Task 8: StockMovementsList.vue

**Files:**
- Modify: `app/Admin/src/features/inventory/views/StockMovementsList.vue`

**Interfaces:**
- Consumes: `StockMovementApi.getStockMovements(query)` → `PagedResult<StockMovementListItem>`
- Note: Read-only audit log — no create/edit/delete

- [ ] **Step 1: Write StockMovementsList.vue**

Read-only DataTable. Columns: Stock Item (name from nested), Originator Type, Quantity Change (+/- formatting with color), Reason, Created At. Search bar. Reload + Export. No New/Edit/Delete buttons.

(Standard read-only pattern — see `StockReservationsList.vue` for toolbar without New button.)

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/inventory/views/StockMovementsList.vue
git commit -m "feat(inventory): implement stock movements list view (read-only)"
```
