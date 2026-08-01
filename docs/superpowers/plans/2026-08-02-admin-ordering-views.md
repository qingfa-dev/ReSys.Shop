# Admin Ordering Views — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace 2 Ordering placeholder views (OrdersList, OrderDetail) with functional views. OrdersList shows a paged table with status filter. OrderDetail shows order overview with status transitions, line items table, and payments table across 3 tabs.

**Architecture:** OrdersList is a read-heavy list view (no create, no delete — orders come from customers). OrderDetail has Overview tab (order info + status transition), Items tab (read-only line items), and Payments tab (read-only payment records). Uses existing `OrderApi` service and types.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue (DataTable, Form, Tabs, Card, Select, Tag), existing `OrderApi`

**Global Constraints:**
- Follows established Catalog/Location view patterns
- All services, types, validations already exist
- View files already exist as placeholders — modify in place

---

### Task 1: OrdersList.vue

**Files:**
- Modify: `app/Admin/src/features/ordering/views/OrdersList.vue`

**Interfaces:**
- Consumes: `OrderApi.getOrders(query)` → `PagedResult<OrderListItem>`
- Consumes: `ORDER_FILTER_FIELDS`, `ORDER_SORT_FIELDS`, `ORDER_SEARCH_FIELDS` from `../types/order`
- Consumes: `ORDERING` from `@/shared/constants/api` → `${ORDERING}/orders`
- Note: No create, no delete

- [ ] **Step 1: Write OrdersList.vue**

DataTable without New or Delete buttons. Columns: Order # (`orderNumber`), Customer Name (nested customer object), Status (Tag badge: Pending/Confirmed/Processing/Shipped/Delivered/Cancelled), Total (`total` formatted as currency), Created At, Actions (View only — navigates to detail).

Toolbar: search bar + filter by status (`Select` dropdown with order statuses: Pending, Confirmed, Processing, Shipped, Delivered, Cancelled), Reload, Export.

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import Select from 'primevue/select'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { ORDERING } from '@/shared/constants/api'
import { OrderApi } from '../services/orderApi'
import type { OrderListItem } from '../types/order'
import { ORDER_FILTER_FIELDS, ORDER_SORT_FIELDS, ORDER_SEARCH_FIELDS } from '../types/order'

const router = useRouter()
const { dt, exportCSV } = useDataTableExport()
const search = ref('')
const statusFilter = ref<string | null>(null)

const statuses = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Cancelled']

const statusSeverity = (s: string) => {
  const map: Record<string, string> = {
    Pending: 'warn', Confirmed: 'info', Processing: 'info',
    Shipped: 'info', Delivered: 'success', Cancelled: 'danger',
  }
  return map[s] ?? 'secondary'
}

const { items, loading, setSearch, setFilter, refresh } = usePagedQuery<OrderListItem>(
  `${ORDERING}/orders`,
  {
    allowedFilterFields: ORDER_FILTER_FIELDS,
    allowedSortFields: ORDER_SORT_FIELDS,
    allowedSearchFields: ORDER_SEARCH_FIELDS,
    defaultSort: ['-createdAtUtc'],
  },
)

function onSearch(value: string) { search.value = value; setSearch(value) }
function clearSearch() { search.value = ''; setSearch('') }
function onStatusFilter(s: string | null) { statusFilter.value = s; setFilter(s ? `status=${s}` : '') }
function navigateToDetail(id: string) { router.push(`/ordering/orders/${id}`) }
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Orders</h1>
      <p class="text-muted-color">View and manage customer orders</p>
    </div>

    <div class="flex items-center gap-3 mb-4 flex-wrap">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText :model-value="search" placeholder="Search orders..." @update:model-value="onSearch" />
      </IconField>
      <Button v-if="search" label="Clear" severity="secondary" icon="pi pi-times" @click="clearSearch" />
      <Select
        :model-value="statusFilter"
        :options="statuses"
        placeholder="Filter by status"
        show-clear
        class="w-48"
        @change="onStatusFilter"
      />
      <div class="flex-1" />
      <Button label="Reload" icon="pi pi-refresh" severity="secondary" @click="refresh" />
      <Button label="Export" icon="pi pi-download" severity="secondary" @click="exportCSV" />
    </div>

    <DataTable
      ref="dt"
      :value="items"
      :loading="loading"
      scrollable paginator :rows="20" :rows-per-page-options="[10,20,50]"
      data-key="id"
    >
      <Column field="orderNumber" header="Order #" :sortable="true" />
      <Column header="Status" :sortable="true" field="status">
        <template #body="{ data }">
          <Tag :value="data.status" :severity="statusSeverity(data.status)" />
        </template>
      </Column>
      <Column field="total" header="Total" :sortable="true">
        <template #body="{ data }">{{ data.total?.toLocaleString('en-US', { style: 'currency', currency: 'USD' }) }}</template>
      </Column>
      <Column field="createdAtUtc" header="Created" :sortable="true">
        <template #body="{ data }">{{ new Date(data.createdAtUtc).toLocaleDateString() }}</template>
      </Column>
      <Column header="Actions" header-style="width:5rem">
        <template #body="{ data }">
          <Button icon="pi pi-eye" severity="secondary" text rounded @click="navigateToDetail(data.id)" />
        </template>
      </Column>
      <template #empty>No orders found.</template>
    </DataTable>
  </div>
</template>
```

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/ordering/views/OrdersList.vue
git commit -m "feat(ordering): implement orders list view with status filter"
```

---

### Task 2: OrderDetail.vue

**Files:**
- Modify: `app/Admin/src/features/ordering/views/OrderDetail.vue`

**Interfaces:**
- Consumes: `OrderApi.getOrder(id)` → `Result<OrderDetail>`, `updateOrderStatus(id, status)` → `Result<void>` (or similar PATCH)
- Consumes: `OrderApi.getOrderItems(id)` → `PagedResult<OrderItem>` for Items tab
- Consumes: `OrderApi.getOrderPayments(id)` or `PaymentApi.getPayments(query)` filtered by order for Payments tab
- Consumes: `OrderDetail` from `../types/order`

- [ ] **Step 1: Write OrderDetail.vue**

Three-tab view with NO form (not editing in the Form/validation sense — the Overview tab shows order data with a status transition Select):

**Tab 0 (Overview):**
- Read-only fields: Order Number, Customer Name, Subtotal, Tax, Shipping Cost, Total, Created At, Modified At (each displayed as read-only in a grid).
- Editable: Status `Select` with available transitions. On change, calls `OrderApi.updateOrderStatus(id, newStatus)` with confirmation dialog.
- Status transition confirmation: `confirm.require({...})` before calling the API.

**Tab 1 (Items):** Read-only DataTable: Product Name, Variant (SKU), Unit Price, Quantity, Line Total.

**Tab 2 (Payments):** Read-only DataTable: Payment ID, Method, Amount, Status (Tag), Created At. Loaded from relevant payment endpoint.

```vue
<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Card from 'primevue/card'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Button from 'primevue/button'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { OrderApi } from '../services/orderApi'
import type { OrderDetail } from '../types/order'
import type { OrderItem } from '../types/order'
import type { PaymentListItem } from '@/features/payment/types/payment'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const order = ref<OrderDetail | null>(null)
const items = ref<OrderItem[]>([])
const payments = ref<PaymentListItem[]>([])
const loading = ref(false)
const statusChanging = ref(false)
const activeTab = ref('0')

const orderStatuses = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Cancelled']

const statusSeverity = (s: string) => {
  const map: Record<string, string> = {
    Pending: 'warn', Confirmed: 'info', Processing: 'info',
    Shipped: 'info', Delivered: 'success', Cancelled: 'danger',
  }
  return map[s] ?? 'secondary'
}

async function loadOrder() {
  const id = route.params.id as string
  loading.value = true
  const result = await OrderApi.getOrder(id)
  if (result.isSuccess) order.value = result.value
  else { handleResult(result); router.push('/ordering/orders') }
  loading.value = false
}

async function loadItems() {
  const id = route.params.id as string
  const result = await OrderApi.getOrderItems(id)
  if (result.isSuccess) items.value = result.items
}

async function loadPayments() {
  const id = route.params.id as string
  const result = await OrderApi.getOrderPayments(id)
  if (result.isSuccess) payments.value = result.items
}

function confirmStatusChange(newStatus: string) {
  confirm.require({
    message: `Change order status to "${newStatus}"?`,
    header: 'Confirm Status Change',
    icon: 'pi pi-exclamation-triangle',
    accept: async () => {
      statusChanging.value = true
      const result = await OrderApi.updateOrderStatus(route.params.id as string, newStatus)
      statusChanging.value = false
      if (result.isSuccess) {
        if (order.value) order.value.status = newStatus
        notify.success('Status Updated', `Order is now ${newStatus}`)
      } else {
        handleResult(result)
      }
    },
  })
}

watch(activeTab, (tab) => {
  if (tab === '1') loadItems()
  else if (tab === '2') loadPayments()
})

onMounted(loadOrder)
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex items-center gap-4 mb-6">
      <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="router.push('/ordering/orders')" />
      <h1 class="text-2xl font-semibold">Order #{{ order?.orderNumber }}</h1>
      <Tag v-if="order" :value="order.status" :severity="statusSeverity(order.status)" />
    </div>

    <Tabs v-model:value="activeTab" :disabled="loading">
      <TabList>
        <Tab value="0">Overview</Tab>
        <Tab value="1">Items</Tab>
        <Tab value="2">Payments</Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <Card>
            <template #content>
              <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                <div><label class="text-sm text-muted-color">Order Number</label><p class="font-medium">{{ order?.orderNumber }}</p></div>
                <div><label class="text-sm text-muted-color">Customer</label><p class="font-medium">{{ (order as any)?.customerName }}</p></div>
                <div><label class="text-sm text-muted-color">Status</label>
                  <Select
                    :model-value="order?.status"
                    :options="orderStatuses"
                    :disabled="statusChanging"
                    class="w-full mt-1"
                    @change="(e: any) => confirmStatusChange(e.value)"
                  />
                </div>
                <div><label class="text-sm text-muted-color">Subtotal</label><p class="font-medium">{{ order?.subtotal?.toLocaleString('en-US', { style: 'currency', currency: 'USD' }) }}</p></div>
                <div><label class="text-sm text-muted-color">Tax</label><p class="font-medium">{{ order?.tax?.toLocaleString('en-US', { style: 'currency', currency: 'USD' }) }}</p></div>
                <div><label class="text-sm text-muted-color">Shipping Cost</label><p class="font-medium">{{ order?.shippingCost?.toLocaleString('en-US', { style: 'currency', currency: 'USD' }) }}</p></div>
                <div><label class="text-sm text-muted-color">Total</label><p class="font-semibold text-lg">{{ order?.total?.toLocaleString('en-US', { style: 'currency', currency: 'USD' }) }}</p></div>
                <div><label class="text-sm text-muted-color">Created</label><p class="font-medium">{{ order?.createdAtUtc ? new Date(order.createdAtUtc).toLocaleString() : '' }}</p></div>
                <div><label class="text-sm text-muted-color">Last Modified</label><p class="font-medium">{{ order?.modifiedAtUtc ? new Date(order.modifiedAtUtc).toLocaleString() : '' }}</p></div>
              </div>
            </template>
          </Card>
        </TabPanel>
        <TabPanel value="1">
          <Card>
            <template #content>
              <DataTable :value="items" scrollable>
                <Column header="Product" />
                <Column field="sku" header="SKU" />
                <Column field="unitPrice" header="Unit Price">
                  <template #body="{ data }">{{ data.unitPrice?.toLocaleString('en-US', { style: 'currency', currency: 'USD' }) }}</template>
                </Column>
                <Column field="quantity" header="Qty" />
                <Column field="lineTotal" header="Line Total">
                  <template #body="{ data }">{{ data.lineTotal?.toLocaleString('en-US', { style: 'currency', currency: 'USD' }) }}</template>
                </Column>
                <template #empty>No items found.</template>
              </DataTable>
            </template>
          </Card>
        </TabPanel>
        <TabPanel value="2">
          <Card>
            <template #content>
              <DataTable :value="payments" scrollable>
                <Column field="id" header="Payment ID" />
                <Column field="method" header="Method" />
                <Column field="amount" header="Amount">
                  <template #body="{ data }">{{ data.amount?.toLocaleString('en-US', { style: 'currency', currency: 'USD' }) }}</template>
                </Column>
                <Column field="status" header="Status">
                  <template #body="{ data }"><Tag :value="data.status" /></template>
                </Column>
                <Column field="createdAtUtc" header="Date">
                  <template #body="{ data }">{{ new Date(data.createdAtUtc).toLocaleDateString() }}</template>
                </Column>
                <template #empty>No payments recorded.</template>
              </DataTable>
            </template>
          </Card>
        </TabPanel>
      </TabPanels>
    </Tabs>
  </div>
</template>
```

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/ordering/views/OrderDetail.vue
git commit -m "feat(ordering): implement order detail view with overview, items, and payments tabs"
```
