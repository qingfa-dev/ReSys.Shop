<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Card from 'primevue/card'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import Select from 'primevue/select'
import InputText from 'primevue/inputtext'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { formatCurrency } from '@/shared/utils/currency'
import { formatDate } from '@/shared/utils/date'
import { useOrderDetail } from '../composables/useOrderDetail'
import { OrderApi } from '../services/orderApi'
import { PaymentApi } from '@/features/payment/services/paymentApi'
import type { Result } from '@/shared/types'
import type { OrderDetail, OrderStatus, LineItem, OrderFulfillmentState, Shipment, ShipmentStatus } from '../types/order'
import type { PaymentListItem } from '@/features/payment/types/payment'
import { toPaymentQueryParams } from '@/features/payment/types/payment'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()
const { order, loading, fetchOrder } = useOrderDetail()

const orderId = computed(() => route.params.id as string)
const activeTab = ref('0')
const actionLoading = ref(false)

const items = ref<LineItem[]>([])
const itemsLoading = ref(false)
const itemsLoaded = ref(false)

const payments = ref<PaymentListItem[]>([])
const paymentsLoading = ref(false)
const paymentsLoaded = ref(false)

const STATUS_SEVERITY: Record<OrderStatus, string> = {
  Draft: 'warn',
  Placed: 'success',
  Canceled: 'danger',
  Expired: 'secondary',
}

function statusSeverity(status: OrderStatus | undefined): string {
  return status ? STATUS_SEVERITY[status] : 'secondary'
}

const shipments = ref<Shipment[]>([])
const shipmentsLoading = ref(false)
const shipmentsLoaded = ref(false)
const savingShipmentId = ref<string | null>(null)
const draftStatus = ref<Record<string, ShipmentStatus>>({})
const trackingInputs = ref<Record<string, string>>({})

// Transition: Return the shipment statuses reachable from the row's current status, mirroring ShipmentMethod guards.
function allowedShipmentTargets(status: ShipmentStatus): ShipmentStatus[] {
  switch (status) {
    case 'Pending':
      return ['Ready', 'Backorder', 'Canceled']
    case 'Ready':
      return ['Shipped', 'Canceled']
    case 'Backorder':
      return ['Ready', 'Canceled']
    case 'Shipped':
      return ['Delivered']
    default:
      return [] // Delivered and Canceled are terminal states.
  }
}

// Guard: Save only when the selected target is reachable from the row's current status.
function canSaveShipment(shipment: Shipment): boolean {
  const current = shipment.status
  const target = draftStatus.value[shipment.id] ?? current
  return target !== current && allowedShipmentTargets(current).includes(target)
}

const FULFILLMENT_SEVERITY: Record<OrderFulfillmentState, string> = {
  None: 'secondary',
  Pending: 'secondary',
  Partial: 'warn',
  Shipped: 'info',
  Delivered: 'success',
  Canceled: 'danger',
}

function fulfillmentSeverity(state: OrderFulfillmentState | null | undefined): string {
  return state ? FULFILLMENT_SEVERITY[state] : 'secondary'
}

function initShipmentDrafts(shipmentList: Shipment[]) {
  // Seed: Track per-row status and tracking edits for the save flow.
  draftStatus.value = Object.fromEntries(shipmentList.map(s => [s.id, s.status]))
  trackingInputs.value = Object.fromEntries(shipmentList.map(s => [s.id, s.trackingNumber ?? '']))
}

async function loadShipments() {
  if (shipmentsLoaded.value || shipmentsLoading.value) return
  shipmentsLoading.value = true
  // Load: Fetch the order's shipments once the detail page mounts.
  const result = await OrderApi.listShipments(orderId.value)
  shipmentsLoading.value = false
  if (result.isSuccess) {
    shipments.value = result.value.items
    shipmentsLoaded.value = true
    initShipmentDrafts(shipments.value)
  } else {
    handleResult(result)
  }
}

async function saveShipmentStatus(shipment: Shipment) {
  // Guard: A tracking number is required to mark a shipment as Shipped.
  if (draftStatus.value[shipment.id] === 'Shipped' && !trackingInputs.value[shipment.id]?.trim()) {
    notify.error('Shipment', 'A tracking number is required to mark the shipment as Shipped.')
    return
  }
  savingShipmentId.value = shipment.id
  // Save: Persist the edited status and tracking number for the shipment.
  const result = await OrderApi.updateShipmentStatus(shipment.id, {
    status: draftStatus.value[shipment.id] ?? shipment.status,
    trackingNumber: trackingInputs.value[shipment.id],
  })
  savingShipmentId.value = null
  if (result.isSuccess) {
    notify.success('Shipment', `Shipment status updated to "${draftStatus.value[shipment.id]}".`)
    await fetchOrder(orderId.value)
    shipmentsLoaded.value = false
    await loadShipments()
  } else {
    handleResult(result)
  }
}

function currency(value: OrderDetail | null): string {
  return value?.currency ?? 'USD'
}

interface OrderAction {
  label: string
  icon: string
  severity: 'primary' | 'secondary' | 'danger'
  confirmMessage: string
  run: () => Promise<Result<OrderDetail>>
}

function orderActions(status: OrderStatus | undefined): OrderAction[] {
  switch (status) {
    case 'Draft':
      return [
        {
          label: 'Approve',
          icon: 'pi pi-check',
          severity: 'primary',
          confirmMessage: 'Approve this order?',
          run: () => OrderApi.approveOrder(orderId.value),
        },
        {
          label: 'Cancel',
          icon: 'pi pi-times',
          severity: 'danger',
          confirmMessage: 'Cancel this order?',
          run: () => OrderApi.cancelOrder(orderId.value),
        },
      ]
    case 'Placed':
      return [
        {
          label: 'Complete',
          icon: 'pi pi-check-circle',
          severity: 'primary',
          confirmMessage: 'Complete this order?',
          run: () => OrderApi.completeOrder(orderId.value),
        },
        {
          label: 'Cancel',
          icon: 'pi pi-times',
          severity: 'danger',
          confirmMessage: 'Cancel this order?',
          run: () => OrderApi.cancelOrder(orderId.value),
        },
      ]
    case 'Canceled':
      return [
        {
          label: 'Resume',
          icon: 'pi pi-undo',
          severity: 'secondary',
          confirmMessage: 'Resume this order?',
          run: () => OrderApi.resumeOrder(orderId.value),
        },
      ]
    default:
      return []
  }
}

const actions = computed(() => orderActions(order.value?.status))

function runAction(action: OrderAction) {
  // Trigger: Confirm before flushing the order to its next status.
  confirm.require({
    message: action.confirmMessage,
    header: `Confirm ${action.label}`,
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: action.label,
    acceptClass: action.severity === 'danger' ? 'p-button-danger' : undefined,
    accept: async () => {
      actionLoading.value = true
      const result = await action.run()
      actionLoading.value = false
      if (result.isSuccess) {
        notify.success(action.label, `Order #${order.value?.number ?? ''} was ${action.label.toLowerCase()}d.`)
        await fetchOrder(orderId.value)
      } else {
        handleResult(result)
      }
    },
  })
}

async function loadOrder() {
  const result = await fetchOrder(orderId.value)
  if (!result.isSuccess) {
    handleResult(result)
    router.push('/ordering/orders')
  }
}

async function loadItems() {
  if (itemsLoaded.value || itemsLoading.value) return
  itemsLoading.value = true
  // Load: Fetch line items lazily when the items tab opens.
  const result = await OrderApi.getLineItems(orderId.value, { pageSize: 100 })
  itemsLoading.value = false
  if (result.isSuccess) {
    items.value = result.items
    itemsLoaded.value = true
  } else {
    handleResult(result)
  }
}

async function loadPayments() {
  if (paymentsLoaded.value || paymentsLoading.value) return
  paymentsLoading.value = true
  // Load: Fetch payments lazily when the payments tab opens.
  const result = await PaymentApi.getPayments(toPaymentQueryParams({ orderId: orderId.value, pageSize: 100 }))
  paymentsLoading.value = false
  if (result.isSuccess) {
    payments.value = result.items
    paymentsLoaded.value = true
  } else {
    handleResult(result)
  }
}

watch(activeTab, (tab) => {
  if (tab === '1') loadItems()
  else if (tab === '2') loadPayments()
})

watch(
  () => route.params.id,
  (id) => {
    if (id) {
      itemsLoaded.value = false
      paymentsLoaded.value = false
      shipmentsLoaded.value = false
      items.value = []
      payments.value = []
      shipments.value = []
      loadOrder()
      loadShipments()
    }
  },
)

onMounted(() => {
  loadOrder()
  loadShipments()
})
</script>

<template>
  <div class="flex flex-col h-full p-4">
    <!-- Section: Page Header — back control, order title, and status actions -->
    <div class="flex-none flex flex-wrap items-center gap-3 mb-4">
      <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="router.push('/ordering/orders')" />
      <h1 class="text-2xl font-semibold">Order #{{ order?.number }}</h1>
      <Tag v-if="order" :value="order.status" :severity="statusSeverity(order.status)" />
      <div class="flex-1" />
      <template v-if="order">
        <Button
          v-for="action in actions"
          :key="action.label"
          :label="action.label"
          :icon="action.icon"
          :severity="action.severity"
          :loading="actionLoading"
          @click="runAction(action)"
        />
      </template>
    </div>

    <!-- Section: Content Card — tabbed order overview, items, and payments -->
    <div class="flex-1 min-h-0 overflow-auto">
      <!-- Section: Tabs — switch between overview, items, and payments -->
      <Tabs v-model:value="activeTab" :disabled="loading">
        <TabList>
          <Tab value="0">Overview</Tab>
          <Tab value="1">Items</Tab>
          <Tab value="2">Payments</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="0">
            <!-- Section: Overview — key order totals and timestamps -->
            <Card>
              <template #content>
                <div v-if="order">
                  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                  <div>
                    <div class="text-sm text-muted-color">Order Number</div>
                    <div class="font-medium">{{ order.number }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Status</div>
                    <Tag :value="order.status" :severity="statusSeverity(order.status)" />
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Fulfillment State</div>
                    <Tag v-if="order.fulfillmentState" :value="order.fulfillmentState" :severity="fulfillmentSeverity(order.fulfillmentState)" />
                    <span v-else class="text-muted-color">—</span>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Checkout State</div>
                    <div class="font-medium">{{ order.checkoutState }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Email</div>
                    <div class="font-medium">{{ order.email ?? '—' }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Item Count</div>
                    <div class="font-medium">{{ order.itemCount }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Item Total</div>
                    <div class="font-medium">{{ formatCurrency(order.itemTotal, currency(order)) }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Adjustment Total</div>
                    <div class="font-medium">{{ formatCurrency(order.adjustmentTotal, currency(order)) }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Shipment Total</div>
                    <div class="font-medium">{{ formatCurrency(order.shipmentTotal, currency(order)) }}</div>
                  </div>
                  <div v-if="order.shippingAdjustment">
                    <div class="text-sm text-muted-color">Shipping Adjustment</div>
                    <div class="font-medium">{{ order.shippingAdjustment.label }} — {{ formatCurrency(order.shippingAdjustment.amount, currency(order)) }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Total</div>
                    <div class="font-medium text-lg">{{ formatCurrency(order.total, currency(order)) }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Payment Total</div>
                    <div class="font-medium">{{ formatCurrency(order.paymentTotal, currency(order)) }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Outstanding Balance</div>
                    <div class="font-medium">{{ formatCurrency(order.outstandingBalance, currency(order)) }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Created</div>
                    <div class="font-medium">{{ formatDate(order.createdAtUtc) }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Completed</div>
                    <div class="font-medium">{{ order.completedAtUtc ? formatDate(order.completedAtUtc) : '—' }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Canceled</div>
                    <div class="font-medium">{{ order.canceledAtUtc ? formatDate(order.canceledAtUtc) : '—' }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Modified</div>
                    <div class="font-medium">{{ order.modifiedAtUtc ? formatDate(order.modifiedAtUtc) : '—' }}</div>
                  </div>
                </div>

                <!-- Section: Shipments — per-shipment tracking input and status control -->
                <div class="mt-6">
                  <h3 class="font-semibold mb-2">Shipments</h3>
                  <DataTable :value="shipments" :loading="shipmentsLoading" scrollable data-key="id" striped-rows>
                    <Column field="shippingMethodId" header="Shipping Method" />
                    <Column header="Tracking Number">
                      <template #body="{ data }">
                        <InputText
                          v-model="trackingInputs[data.id]"
                          class="w-full"
                          :class="{ 'ring-2 ring-primary': draftStatus[data.id] === 'Shipped' }"
                          placeholder="Required when shipped"
                        />
                      </template>
                    </Column>
                    <Column header="Status">
                      <template #body="{ data }">
                        <Select v-model="draftStatus[data.id]" :options="allowedShipmentTargets(data.status)" class="w-40" />
                      </template>
                    </Column>
                    <Column header="Shipped">
                      <template #body="{ data }">{{ data.shippedAtUtc ? formatDate(data.shippedAtUtc) : '—' }}</template>
                    </Column>
                    <Column header="Delivered">
                      <template #body="{ data }">{{ data.deliveredAtUtc ? formatDate(data.deliveredAtUtc) : '—' }}</template>
                    </Column>
                    <Column header="Actions">
                      <template #body="{ data }">
                        <Button
                          icon="pi pi-save"
                          label="Save"
                          size="small"
                          :disabled="!canSaveShipment(data)"
                          :loading="savingShipmentId === data.id"
                          @click="saveShipmentStatus(data)"
                        />
                      </template>
                    </Column>
                    <template #empty>No shipments yet for this order.</template>
                  </DataTable>
                </div>
              </div>
              <p v-else class="text-muted-color">{{ loading ? 'Loading order...' : 'Order not found.' }}</p>
              </template>
            </Card>
          </TabPanel>

          <TabPanel value="1">
            <!-- Section: Line Items — the ordered variants with prices -->
            <Card>
              <template #content>
                <DataTable :value="items" :loading="itemsLoading" scrollable data-key="id" striped-rows>
                  <Column field="variantId" header="Variant ID" />
                  <Column field="quantity" header="Quantity" />
                  <Column field="price" header="Price">
                    <template #body="{ data }">{{ formatCurrency(data.price, data.currency ?? 'USD') }}</template>
                  </Column>
                  <Column field="total" header="Line Total">
                    <template #body="{ data }">{{ formatCurrency(data.total, data.currency ?? 'USD') }}</template>
                  </Column>
                  <Column field="adjustmentTotal" header="Adjustment">
                    <template #body="{ data }">{{ formatCurrency(data.adjustmentTotal, data.currency ?? 'USD') }}</template>
                  </Column>
                  <template #empty>No items found.</template>
                </DataTable>
              </template>
            </Card>
          </TabPanel>

          <TabPanel value="2">
            <!-- Section: Payments — recorded transactions for the order -->
            <Card>
              <template #content>
                <DataTable :value="payments" :loading="paymentsLoading" scrollable data-key="id" striped-rows>
                  <Column field="id" header="Payment ID" />
                  <Column field="amount" header="Amount">
                    <template #body="{ data }">{{ formatCurrency(data.amount, data.currency ?? 'USD') }}</template>
                  </Column>
                  <Column field="state" header="State">
                    <template #body="{ data }"><Tag :value="data.state" /></template>
                  </Column>
                  <Column field="paymentStatus" header="Payment Status">
                    <template #body="{ data }">{{ data.paymentStatus ?? '—' }}</template>
                  </Column>
                  <template #empty>No payments recorded.</template>
                </DataTable>
              </template>
            </Card>
          </TabPanel>
        </TabPanels>
      </Tabs>
    </div>
  </div>
</template>
