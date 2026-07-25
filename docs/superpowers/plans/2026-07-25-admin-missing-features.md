# Admin SPA — Missing Features Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement 4 missing features: variant image upload/management, order fulfillment workflow, advanced search + filters, and notification system. All features are client-only (no backend changes) and independent of each other.

**Architecture:** Each feature adds 1-3 new components and extends existing stores/pages. VariantImageManager attaches to VariantDetailPage, FulfillmentWorkflow attaches to OrderDetailPage, search/filter extends TableToolbar + all list stores, notification bell extends App.vue topbar. Features are independent and can be built in any order.

**Tech Stack:** Vue 3.5 + TypeScript 6, PrimeVue 5 (Steps, Popover, FileUpload, Tag), Tailwind v4, Pinia 3, Axios 1.18

## Global Constraints

- No new npm packages — reuse existing primevue, @primevue/themes, tailwindcss, pinia, axios
- No backend API changes — use existing endpoint contracts
- All destructive actions use `useConfirm` dialog
- All mutation outcomes show toast notifications via `useToast`
- All catch blocks include `console.error(err)`
- `vue-tsc --noEmit` passes with zero errors after each task
- All Pinia stores expose state as `readonly()` refs

---

### Task 1: Variant Image Manager — component

**Files:**
- Create: `app/Admin/src/features/catalog/components/VariantImageManager.vue`
- Modify: `app/Admin/src/features/catalog/pages/VariantDetailPage.vue`

**Interfaces:**
- Consumes: VariantImageApi (existing: `POST /catalog/variants/{id}/images`, `DELETE .../{id}/images/{imageId}`, `PUT .../images/reorder`)
- Produces: `<VariantImageManager :variant-id="id" :images="images" @update:images="onImagesChanged" />`

- [ ] **Step 1: Create VariantImageManager component**

```vue
<!-- app/Admin/src/features/catalog/components/VariantImageManager.vue -->
<script setup lang="ts">
import { ref } from 'vue'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { VariantImageApi } from '../api'

const props = defineProps<{
  variantId: string
  images: import('../types').VariantImageResponse[]
}>()

const emit = defineEmits<{
  'update:images': [value: import('../types').VariantImageResponse[]]
}>()

const { confirmDelete } = useConfirm()
const toast = useToast()
const uploading = ref(false)

const MAX_IMAGES = 10

async function onFilesSelected(event: Event) {
  const input = event.target as HTMLInputElement
  if (!input.files?.length) return
  const files = Array.from(input.files).filter(f => f.type.startsWith('image/'))
  if (props.images.length + files.length > MAX_IMAGES) {
    toast.error(`Maximum ${MAX_IMAGES} images per variant`)
    return
  }
  uploading.value = true
  for (const file of files) {
    try {
      const formData = new FormData()
      formData.append('file', file)
      const result = await VariantImageApi.upload(props.variantId, formData)
      if (result.isSuccess) {
        emit('update:images', [...props.images, result.value])
        toast.success(`${file.name} uploaded`)
      } else {
        console.error(result.message)
        toast.error(`Failed to upload ${file.name}: ${result.message}`)
      }
    } catch (err) {
      console.error(err)
      toast.error(`Upload failed for ${file.name}`)
    }
  }
  uploading.value = false
  input.value = ''
}

async function deleteImage(imageId: string) {
  const confirmed = await confirmDelete('image')
  if (!confirmed) return
  try {
    const result = await VariantImageApi.delete(props.variantId, imageId)
    if (result.isSuccess) {
      emit('update:images', props.images.filter(i => i.id !== imageId))
      toast.success('Image deleted')
    } else {
      console.error(result.message)
      toast.error(result.message ?? 'Failed to delete image')
    }
  } catch (err) {
    console.error(err)
    toast.error('Failed to delete image')
  }
}

function onDragStart(index: number, event: DragEvent) {
  event.dataTransfer!.effectAllowed = 'move'
  event.dataTransfer!.setData('text/plain', String(index))
}

function onDrop(targetIndex: number, event: DragEvent) {
  event.preventDefault()
  const sourceIndex = Number(event.dataTransfer!.getData('text/plain'))
  if (sourceIndex === targetIndex) return
  const reordered = [...props.images]
  const [moved] = reordered.splice(sourceIndex, 1)
  reordered.splice(targetIndex, 0, moved)
  emit('update:images', reordered)
}

function onDragOver(event: DragEvent) {
  event.preventDefault()
  event.dataTransfer!.dropEffect = 'move'
}

const dropActive = ref(false)
function onDragEnter() { dropActive.value = true }
function onDragLeave() { dropActive.value = false }
</script>

<template>
  <div class="flex flex-col gap-4">
    <div class="flex items-center justify-between">
      <h3 class="text-lg font-semibold">Images</h3>
      <div>
        <input
          ref="fileInput"
          type="file"
          multiple
          accept="image/*"
          class="hidden"
          :disabled="uploading || images.length >= MAX_IMAGES"
          @change="onFilesSelected"
        />
        <Button
          :label="uploading ? 'Uploading...' : 'Upload'"
          icon="pi pi-upload"
          :disabled="uploading || images.length >= MAX_IMAGES"
          @click="($refs.fileInput as HTMLInputElement).click()"
        />
      </div>
    </div>

    <div v-if="!images.length" class="rounded-border border-2 border-dashed border-surface-300 p-8 text-center text-surface-500 dark:border-surface-600">
      <i class="pi pi-images text-3xl mb-2" />
      <p>No images. Drag and drop or click Upload.</p>
    </div>

    <div
      v-else
      class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4"
      :class="{ 'border-2 border-dashed border-primary-400 bg-primary-50 dark:bg-primary-900/20': dropActive }"
      @dragenter="onDragEnter"
      @dragleave="onDragLeave"
    >
      <div
        v-for="(image, index) in images"
        :key="image.id"
        class="group relative cursor-move rounded-border border border-surface-200 overflow-hidden"
        draggable="true"
        @dragstart="onDragStart(index, $event)"
        @dragover="onDragOver"
        @drop="onDrop(index, $event)"
      >
        <img :src="image.url" :alt="image.fileName" class="aspect-square w-full object-cover" />
        <Button
          icon="pi pi-times"
          severity="danger"
          rounded
          size="small"
          class="absolute top-1 right-1 opacity-0 group-hover:opacity-100 transition-opacity"
          @click="deleteImage(image.id)"
        />
        <i
          v-if="image.isPrimary"
          class="pi pi-star-fill absolute bottom-1 left-1 text-yellow-500"
        />
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Integrate VariantImageManager into VariantDetailPage**

Read `app/Admin/src/features/catalog/pages/VariantDetailPage.vue`. Add to `#sub-entities` slot:
```html
<AppCard>
  <VariantImageManager
    :variant-id="id"
    :images="images"
    @update:images="images = $event"
  />
</AppCard>
```

In the script, add `const images = ref<VariantImageResponse[]>([])` and load images from API on mount.

- [ ] **Step 3: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/catalog/components/VariantImageManager.vue app/Admin/src/features/catalog/pages/VariantDetailPage.vue
git commit -m "feat: add VariantImageManager with drag-drop upload, reorder, delete, primary star"
```

---

### Task 2: Order Fulfillment Workflow — component

**Files:**
- Create: `app/Admin/src/features/ordering/components/FulfillmentWorkflow.vue`
- Modify: `app/Admin/src/features/ordering/pages/OrderDetailPage.vue`

**Interfaces:**
- Consumes: OrderApi (existing: `approve(id)`, `complete(id)`, `cancel(id)`, `resume(id)`)
- Produces: `<FulfillmentWorkflow :order-id="id" :status="status" @status-changed="onStatusChanged" />`

- [ ] **Step 1: Create FulfillmentWorkflow component**

```vue
<!-- app/Admin/src/features/ordering/components/FulfillmentWorkflow.vue -->
<script setup lang="ts">
import { ref } from 'vue'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { OrderApi } from '../api'
import type { OrderStatus } from '../types'

const props = defineProps<{
  orderId: string
  status: OrderStatus
}>()

const emit = defineEmits<{
  'status-changed': [newStatus: OrderStatus]
}>()

const { confirmDelete } = useConfirm()
const toast = useToast()
const transitioning = ref(false)

const STATUSES = [
  { label: 'Pending', value: 'Pending' },
  { label: 'Confirmed', value: 'Confirmed' },
  { label: 'Processing', value: 'Processing' },
  { label: 'Picked', value: 'Picked' },
  { label: 'Packed', value: 'Packed' },
  { label: 'Shipped', value: 'Shipped' },
  { label: 'Delivered', value: 'Delivered' },
] as const

const terminalStatuses: OrderStatus[] = ['Delivered', 'Cancelled', 'Returned']

const activeStep = STATUSES.findIndex(s => s.value === props.status)

async function transition(action: 'approve' | 'complete' | 'cancel' | 'resume') {
  const labels = {
    approve: 'confirm this order',
    complete: 'complete this order',
    cancel: 'cancel this order',
    resume: 'resume this order',
  }
  if (action === 'cancel') {
    const confirmed = await confirmDelete(labels[action])
    if (!confirmed) return
  }
  transitioning.value = true
  try {
    const result = await OrderApi[action](props.orderId)
    if (result.isSuccess) {
      toast.success(`Order ${action}d`)
      emit('status-changed', props.status)
    } else {
      console.error(result.message)
      toast.error(result.message ?? `Failed to ${action} order`)
    }
  } catch (err) {
    console.error(err)
    toast.error(`Failed to ${action} order`)
  }
  transitioning.value = false
}
</script>

<template>
  <div class="flex flex-col gap-4">
    <Steps :model="STATUSES" :active-step="activeStep" />

    <div v-if="!terminalStatuses.includes(status)" class="flex items-center gap-2">
      <Button
        v-if="status === 'Pending'"
        label="Confirm Order"
        icon="pi pi-check"
        :loading="transitioning"
        @click="transition('approve')"
      />
      <Button
        v-if="status === 'Confirmed' || status === 'Processing'"
        label="Mark Complete"
        icon="pi pi-check-circle"
        :loading="transitioning"
        @click="transition('complete')"
      />
      <Button
        label="Cancel Order"
        severity="danger"
        icon="pi pi-times"
        :loading="transitioning"
        outlined
        @click="transition('cancel')"
      />
    </div>
    <div v-else class="text-sm text-surface-500">
      Order is {{ status }} — no actions available.
    </div>
  </div>
</template>
```

- [ ] **Step 2: Integrate into OrderDetailPage**

Read `app/Admin/src/features/ordering/pages/OrderDetailPage.vue`. Add to `#sub-entities` slot:
```html
<AppCard>
  <FulfillmentWorkflow
    :order-id="id"
    :status="order.status"
    @status-changed="loadOrder"
  />
</AppCard>
```

- [ ] **Step 3: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/ordering/components/FulfillmentWorkflow.vue app/Admin/src/features/ordering/pages/OrderDetailPage.vue
git commit -m "feat: add FulfillmentWorkflow with Steps component and contextual action buttons"
```

---

### Task 3: Advanced Search + Filters — TableToolbar enhancement

**Files:**
- Create: `app/Admin/src/shared/components/layout/FilterPanel.vue` (replacing deleted stub)
- Modify: `app/Admin/src/shared/components/layout/TableToolbar.vue`
- Modify: `app/Admin/src/features/catalog/store/product.store.ts`
- Modify: `app/Admin/src/features/catalog/pages/ProductListPage.vue`

**Interfaces:**
- Consumes: Store `query`/`filters` refs, API `getMany` with query+filters params
- Produces: TableToolbar with debounced search + filter chip display, FilterPanel with per-column filter definitions

- [ ] **Step 1: Enhance TableToolbar with debounced search and filter chips**

Read `app/Admin/src/shared/components/layout/TableToolbar.vue`.

Add these props:
```ts
const props = defineProps<{
  searchPlaceholder?: string
  query?: string
  filters?: { field: string; value: string; label: string }[]
  showFilter?: boolean
}>()

const emit = defineEmits<{
  'update:query': [value: string]
  'update:filters': [value: { field: string; value: string; label: string }[]]
  'create': []
  'toggle-filter': []
}>()
```

Add debounced search logic:
```ts
import { ref, watch } from 'vue'

const searchInput = ref(props.query ?? '')
let debounceTimer: ReturnType<typeof setTimeout>

watch(searchInput, (val) => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    emit('update:query', val)
  }, 300)
})

function removeFilter(field: string) {
  emit('update:filters', props.filters?.filter(f => f.field !== field) ?? [])
}
```

Add to template below toolbar actions, above the slot content:
```html
<div v-if="filters && filters.length > 0" class="flex flex-wrap items-center gap-2 mt-2">
  <Tag
    v-for="filter in filters"
    :key="filter.field"
    :value="filter.label"
    severity="info"
    removable
    @remove="removeFilter(filter.field)"
  />
</div>
```

- [ ] **Step 2: Create FilterPanel component**

```vue
<!-- app/Admin/src/shared/components/layout/FilterPanel.vue -->
<script setup lang="ts">
import { ref } from 'vue'

interface ColumnFilterDef {
  field: string
  label: string
  type: 'text' | 'select' | 'date-range' | 'number-range'
  options?: { label: string; value: string }[]
}

interface FilterConfig {
  field: string
  operator: 'eq' | 'neq' | 'gte' | 'lte' | 'contains' | 'between'
  value: string | number
  label: string
}

const props = defineProps<{
  definitions: ColumnFilterDef[]
  activeFilters: FilterConfig[]
  visible: boolean
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  'apply': [filters: FilterConfig[]]
  'clear': []
}>()

const localFilters = ref<Record<string, { value: string; operator: string }>>({})

function apply() {
  const filters: FilterConfig[] = Object.entries(localFilters.value)
    .filter(([_, v]) => v.value)
    .map(([field, config]) => {
      const def = props.definitions.find(d => d.field === field)
      return {
        field,
        operator: config.operator as FilterConfig['operator'],
        value: config.value,
        label: `${def?.label ?? field}: ${config.value}`,
      }
    })
  emit('apply', filters)
  emit('update:visible', false)
}

function clear() {
  localFilters.value = {}
  emit('clear')
  emit('update:visible', false)
}
</script>

<template>
  <div v-if="visible" class="rounded-border border border-surface-200 bg-white p-4 dark:border-surface-700 dark:bg-surface-900">
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div v-for="def in definitions" :key="def.field" class="flex flex-col gap-1">
        <label class="text-sm font-medium">{{ def.label }}</label>
        <InputText v-if="def.type === 'text'" v-model="localFilters[def.field].value" />
        <Select v-else-if="def.type === 'select'" v-model="localFilters[def.field].value" :options="def.options" option-label="label" option-value="value" />
      </div>
    </div>
    <div class="flex items-center justify-end gap-2 mt-4">
      <Button label="Clear All" text size="small" @click="clear" />
      <Button label="Apply Filters" size="small" @click="apply" />
    </div>
  </div>
</template>
```

- [ ] **Step 3: Update product store with query/filters support**

Read `app/Admin/src/features/catalog/store/product.store.ts`. Add:
```ts
const query = ref('')
const filters = ref<FilterConfig[]>([])

async function fetchMany(params?: { page?: number; pageSize?: number }) {
  loading.value = true
  error.value = null
  try {
    const result = await ProductApi.getMany({
      page: params?.page ?? page.value + 1,
      pageSize: params?.pageSize ?? pageSize.value,
      query: query.value || undefined,
      filters: filters.value.length > 0 ? filters.value : undefined,
    })
    if (result.isSuccess) {
      items.value = result.items ?? []
      totalRecords.value = result.totalCount ?? 0
      page.value = (params?.page ?? page.value + 1) - 1
    } else {
      console.error(result.message)
      error.value = result.message ?? 'Failed to load'
      items.value = []
      totalRecords.value = 0
    }
  } catch (err) {
    console.error(err)
    error.value = 'Failed to load products'
    items.value = []
    totalRecords.value = 0
  }
  loading.value = false
}
```

Export `query` and `filters` as `readonly()` refs. Add `setFilters(f: FilterConfig[])`, `setQuery(q: string)` actions.

- [ ] **Step 4: Wire ProductListPage to search/filters**

Read `app/Admin/src/features/catalog/pages/ProductListPage.vue`. Update the template's TableToolbar:
```html
<TableToolbar
  v-model:query="store.query"
  :filters="activeFilterChips"
  :search-placeholder="t('catalog.products.searchPlaceholder')"
  show-filter
  @create="router.push({ name: ROUTE.PRODUCTS.CREATE })"
  @toggle-filter="showFilters = !showFilters"
/>
```

Add `FilterPanel` below toolbar:
```html
<FilterPanel
  v-model:visible="showFilters"
  :definitions="productFilterDefs"
  :active-filters="store.filters"
  @apply="onFiltersApplied"
  @clear="store.setFilters([])"
/>
```

Add `FilterChip` removal handler:
```ts
const activeFilterChips = computed(() =>
  store.filters.map(f => ({ field: f.field, value: String(f.value), label: f.label }))
)

function removeFilter(field: string) {
  store.setFilters(store.filters.filter(f => f.field !== field))
}

function onFiltersApplied(filters: FilterConfig[]) {
  store.setFilters(filters)
}
```

- [ ] **Step 5: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/shared/components/layout/TableToolbar.vue app/Admin/src/shared/components/layout/FilterPanel.vue app/Admin/src/features/catalog/store/product.store.ts app/Admin/src/features/catalog/pages/ProductListPage.vue
git commit -m "feat: add debounced search, FilterPanel, and filter chips to TableToolbar + product store"
```

---

### Task 4: Propagate search/filter to all remaining list stores and pages

**Files:**
- Modify: All remaining 16 list store files across inventory/location/ordering/payment/shipping/users
- Modify: All remaining list pages

**Interfaces:**
- Consumes: Same pattern as Task 3 store/page updates
- Produces: All list pages have search + filter wired to their stores

- [ ] **Step 1: Update all stores with query/filters support**

For each store file (stock-item, stock-location, stock-movement, stock-reservation, stock-transfer, country, state, order, payment, payment-method, shipping-method, shipping-rate, profile, address, user, role, permission):

Add to each store:
```ts
const query = ref('')
const filters = ref<{ field: string; value: string; label: string; operator: string }[]>([])

// In fetchMany, pass query and filters to API call params
```

Export `query` and `filters` as `readonly()`. Add `setQuery(q: string)`, `setFilters(f: ...)` actions.

- [ ] **Step 2: Update all list pages to wire TableToolbar v-model:query

For each list page, update the `<TableToolbar>` to include:
```html
v-model:query="store.query"
:filters="activeFilterChips"
```

- [ ] **Step 3: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/
git commit -m "feat: propagate search/filter support to all list stores and pages"
```

---

### Task 5: Notification System — store and composable

**Files:**
- Create: `app/Admin/src/shared/composables/useNotification.ts`
- Create: `app/Admin/src/stores/useNotificationStore.ts`

**Interfaces:**
- Produces: `useNotificationStore` with `unreadCount`, `items`, `recentItems`, `fetch()`, `markRead(id)`, `markAllRead()`, `startPolling(ms)`, `stopPolling()`

- [ ] **Step 1: Create notification type definition**

```ts
// app/Admin/src/stores/useNotificationStore.ts (top of file)
export interface Notification {
  id: string
  type: 'order_status' | 'payment_status' | 'stock_alert' | 'system'
  title: string
  message: string
  linkRoute?: { name: string; params?: Record<string, string> }
  isRead: boolean
  createdAt: string
}
```

- [ ] **Step 2: Create notification store**

```ts
// app/Admin/src/stores/useNotificationStore.ts
import { defineStore } from 'pinia'
import { ref, computed, readonly } from 'vue'

export const useNotificationStore = defineStore('notification', () => {
  const items = ref<Notification[]>([])
  let pollingTimer: ReturnType<typeof setInterval> | null = null

  const unreadCount = computed(() => items.value.filter(n => !n.isRead).length)
  const recentItems = computed(() => items.value.slice(0, 5))

  async function fetch() {
    // Client-only fallback — no backend API yet
    // When backend exists: call GET /notifications?limit=50 and map results
  }

  async function markRead(id: string) {
    const item = items.value.find(n => n.id === id)
    if (item) {
      item.isRead = true
      // When backend exists: PUT /notifications/{id}/read
    }
  }

  async function markAllRead() {
    items.value.forEach(n => { n.isRead = true })
    // When backend exists: PUT /notifications/read-all
  }

  function startPolling(intervalMs: number = 30000) {
    stopPolling()
    pollingTimer = setInterval(() => fetch(), intervalMs)
  }

  function stopPolling() {
    if (pollingTimer) {
      clearInterval(pollingTimer)
      pollingTimer = null
    }
  }

  return {
    items: readonly(items),
    unreadCount: readonly(unreadCount),
    recentItems: readonly(recentItems),
    fetch,
    markRead,
    markAllRead,
    startPolling,
    stopPolling,
  }
})
```

- [ ] **Step 3: Create useNotification composable**

```ts
// app/Admin/src/shared/composables/useNotification.ts
import { useNotificationStore } from '@/stores/useNotificationStore'
import { onMounted, onUnmounted } from 'vue'

export function useNotification() {
  const store = useNotificationStore()

  onMounted(() => store.startPolling(30000))
  onUnmounted(() => store.stopPolling())

  return {
    unreadCount: store.unreadCount,
    recentItems: store.recentItems,
    items: store.items,
    markRead: store.markRead,
    markAllRead: store.markAllRead,
    fetch: store.fetch,
  }
}
```

- [ ] **Step 4: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/stores/useNotificationStore.ts app/Admin/src/shared/composables/useNotification.ts
git commit -m "feat: add notification store with polling, mark-read, client-only fallback"
```

---

### Task 6: Notification System — UI (bell icon + popover)

**Files:**
- Create: `app/Admin/src/shared/components/layout/NotificationBell.vue`
- Modify: `app/Admin/src/App.vue`

**Interfaces:**
- Consumes: `useNotification()` composable, notification store
- Produces: Bell icon in topbar with unread badge and Popover panel

- [ ] **Step 1: Create NotificationBell component**

```vue
<!-- app/Admin/src/shared/components/layout/NotificationBell.vue -->
<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useNotification } from '@/shared/composables/useNotification'

const router = useRouter()
const { unreadCount, recentItems, markRead, markAllRead } = useNotification()

function onItemClick(notification: import('@/stores/useNotificationStore').Notification) {
  markRead(notification.id)
  if (notification.linkRoute) {
    router.push(notification.linkRoute)
  }
}
</script>

<template>
  <Popover>
    <template #activator="{ toggle }">
      <Button
        text
        rounded
        :badge="unreadCount > 0 ? String(unreadCount > 99 ? '99+' : unreadCount) : undefined"
        badge-severity="danger"
        @click="toggle"
      >
        <template #icon>
          <i :class="['pi pi-bell', unreadCount > 0 ? 'text-primary-500' : 'text-surface-500']" />
        </template>
      </Button>
    </template>

    <div class="w-80">
      <div class="flex items-center justify-between px-4 py-2 border-b border-surface-200 dark:border-surface-700">
        <span class="font-semibold">Notifications</span>
        <Button
          v-if="unreadCount > 0"
          label="Mark all read"
          text
          size="small"
          @click="markAllRead"
        />
      </div>

      <div v-if="!recentItems.length" class="p-6 text-center text-surface-500">
        <i class="pi pi-inbox text-2xl mb-2" />
        <p class="text-sm">No notifications</p>
      </div>

      <div v-else class="divide-y divide-surface-200 dark:divide-surface-700">
        <div
          v-for="item in recentItems"
          :key="item.id"
          class="flex items-start gap-3 p-3 cursor-pointer hover:bg-surface-50 dark:hover:bg-surface-800"
          :class="{ 'bg-primary-50 dark:bg-primary-900/10': !item.isRead }"
          @click="onItemClick(item)"
        >
          <i
            :class="[
              'mt-0.5 text-xs',
              item.isRead ? 'pi pi-circle text-surface-300' : 'pi pi-circle-fill text-primary-500',
            ]"
          />
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium truncate">{{ item.title }}</p>
            <p class="text-xs text-surface-500 truncate">{{ item.message }}</p>
            <p class="text-xs text-surface-400 mt-0.5">{{ item.createdAt }}</p>
          </div>
        </div>
      </div>
    </div>
  </Popover>
</template>
```

- [ ] **Step 2: Add NotificationBell to App.vue topbar**

Read `app/Admin/src/App.vue` to find the topbar area (likely in a layout component imported by App.vue). Add `<NotificationBell />` next to the user/profile button in the topbar.

The topbar layout is defined in SCSS (`_topbar.scss`) — the rendering likely happens in a layout component. Find the component that renders the topbar (check `shared/components/layout/` or the App.vue template).

Insert `<NotificationBell />` as a sibling of the existing topbar-right buttons.

- [ ] **Step 3: Verify**

Run: `cd app/Admin && npx vue-tsc --noEmit`
Expected: PASS

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/shared/components/layout/NotificationBell.vue app/Admin/src/App.vue
git commit -m "feat: add notification bell with unread badge, popover panel, and polling"
```

---

### Task 7: Final integration verification

**Files:**
- No file changes — verification only.

- [ ] **Step 1: Run TypeScript check**

```bash
cd app/Admin && npx vue-tsc --noEmit
```
Expected: PASS with zero errors.

- [ ] **Step 2: Run lint**

```bash
cd app/Admin && pnpm run lint
```
Expected: PASS with zero errors.

- [ ] **Step 3: Run existing tests**

```bash
cd app/Admin && pnpm run test:unit
```
Expected: All existing tests still pass.

- [ ] **Step 4: Commit**

```bash
git commit -m "chore: missing features verification - vue-tsc clean, no lint errors" --allow-empty
```
