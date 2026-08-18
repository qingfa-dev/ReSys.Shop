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
import { PaymentApi } from '../../payment/services/paymentApi'
import { PAYMENT_STATE_SEVERITY } from '../../payment/types/payment'
import type { PaymentRecordState } from '../../payment/types/payment'
import Timeline from 'primevue/timeline'
import type { Result } from '@/shared/types'
import type { OrderDetail, OrderStatus, LineItem, OrderFulfillmentState, ShipmentSummary, ShipmentStatus, PaymentCaptureSummary } from '../types/order'

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

// Shipments: Derive the editable shipment rows from the order detail payload.
const shipments = computed<ShipmentSummary[]>(() => order.value?.shipments ?? [])
// Payments: Derive the payment rows from the order detail payload.
const payments = computed(() => order.value?.payments ?? [])
// Timeline: Derive the status event list from the order detail payload.
const timeline = computed(() => order.value?.timeline ?? [])

const STATUS_SEVERITY: Record<OrderStatus, string> = {
  Draft: 'warn',
  Placed: 'success',
  Canceled: 'danger',
  Expired: 'secondary',
}

function statusSeverity(status: OrderStatus | undefined): string {
  return status ? STATUS_SEVERITY[status] : 'secondary'
}

const savingShipmentId = ref<string | null>(null)
const draftStatus = ref<Record<string, ShipmentStatus>>({})
const trackingInputs = ref<Record<string, string>>({})
const paymentActionId = ref<string | null>(null)

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
function canSaveShipment(shipment: ShipmentSummary): boolean {
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

function initShipmentDrafts(shipmentList: ShipmentSummary[]) {
  // Seed: Track per-row status and tracking edits for the save flow.
  draftStatus.value = Object.fromEntries(shipmentList.map(s => [s.id, s.status]))
  trackingInputs.value = Object.fromEntries(shipmentList.map(s => [s.id, s.trackingNumber ?? '']))
}

// Seed: Rebuild the per-row status/tracking drafts whenever the payload shipments change.
watch(shipments, (list) => initShipmentDrafts(list), { immediate: true })

async function saveShipmentStatus(shipment: ShipmentSummary) {
  // Guard: A tracking number is required to mark a shipment as Shipped.
  if (draftStatus.value[shipment.id] === 'Shipped' && !trackingInputs.value[shipment.id]?.trim()) {
    notify.error('Shipment', 'A tracking number is required to mark the shipment as Shipped.')
    return
  }
  await persistShipmentStatus(shipment, draftStatus.value[shipment.id] ?? shipment.status, trackingInputs.value[shipment.id])
}

// Save: Persist a shipment status transition and refresh the order on success.
async function persistShipmentStatus(shipment: ShipmentSummary, status: ShipmentStatus, trackingNumber?: string) {
  savingShipmentId.value = shipment.id
  const result = await OrderApi.updateShipmentStatus(shipment.id, { status, trackingNumber })
  savingShipmentId.value = null
  if (result.isSuccess) {
    notify.success('Shipment', `Shipment status updated to "${status}".`)
    // Refresh: Re-fetch the order so shipments and timeline reflect the new status.
    await fetchOrder(orderId.value)
  } else {
    handleResult(result)
  }
}

// Gate: Mark Shipped is reachable only from Ready or Backorder.
function canMarkShipped(shipment: ShipmentSummary): boolean {
  return allowedShipmentTargets(shipment.status).includes('Shipped')
}

// Gate: Mark Delivered is reachable only from Shipped.
function canMarkDelivered(shipment: ShipmentSummary): boolean {
  return allowedShipmentTargets(shipment.status).includes('Delivered')
}

async function markShipmentShipped(shipment: ShipmentSummary) {
  // Guard: A tracking number is required to mark a shipment as Shipped.
  if (!trackingInputs.value[shipment.id]?.trim()) {
    notify.error('Shipment', 'A tracking number is required to mark the shipment as Shipped.')
    return
  }
  await persistShipmentStatus(shipment, 'Shipped', trackingInputs.value[shipment.id])
}

async function markShipmentDelivered(shipment: ShipmentSummary) {
  await persistShipmentStatus(shipment, 'Delivered')
}

// Gate: Capture applies only to payments awaiting settlement.
function canCapturePayment(state: PaymentRecordState): boolean {
  return state === 'Pending' || state === 'Processing'
}

// Gate: Refund applies only to completed payments.
function canRefundPayment(state: PaymentRecordState): boolean {
  return state === 'Completed'
}

// Gate: Void applies only to payments that have not completed.
function canVoidPayment(state: PaymentRecordState): boolean {
  return state === 'Pending' || state === 'Processing'
}

// Trigger: Confirm before running a payment action on the row, then reload the order.
function confirmPaymentAction<T>(payment: PaymentCaptureSummary, label: string, message: string, run: () => Promise<Result<T>>) {
  confirm.require({
    message,
    header: `Confirm ${label}`,
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: label,
    accept: async () => {
      paymentActionId.value = payment.id
      const result = await run()
      paymentActionId.value = null
      if (result.isSuccess) {
        notify.success('Payment', `Payment ${label.toLowerCase()}d.`)
        await fetchOrder(orderId.value)
      } else {
        handleResult(result)
      }
    },
  })
}

function capturePayment(payment: PaymentCaptureSummary) {
  confirmPaymentAction(payment, 'Capture', 'Capture this payment?', () => PaymentApi.capturePayment(payment.id))
}

function refundPayment(payment: PaymentCaptureSummary) {
  const amount = formatCurrency(payment.amount, payment.currency)
  confirmPaymentAction(payment, 'Refund', `Refund ${amount} for this payment?`, () =>
    PaymentApi.refundPayment(payment.id, { amount: payment.amount }),
  )
}

function voidPayment(payment: PaymentCaptureSummary) {
  confirmPaymentAction(payment, 'Void', 'Void this payment?', () => PaymentApi.voidPayment(payment.id))
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

watch(activeTab, (tab) => {
  if (tab === '1') loadItems()
})

watch(
  () => route.params.id,
  (id) => {
    if (id) {
      itemsLoaded.value = false
      items.value = []
      loadOrder()
    }
  },
)

onMounted(() => {
  loadOrder()
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
                  <div>
                    <div class="text-sm text-muted-color">Payment Processing</div>
                    <div class="font-medium">{{ order.paymentProcessingAtUtc ? formatDate(order.paymentProcessingAtUtc) : '—' }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Payment Completed</div>
                    <div class="font-medium">{{ order.paymentCompletedAtUtc ? formatDate(order.paymentCompletedAtUtc) : '—' }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Payment Failed</div>
                    <div class="font-medium">{{ order.paymentFailedAtUtc ? formatDate(order.paymentFailedAtUtc) : '—' }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Shipped</div>
                    <div class="font-medium">{{ order.shipmentShippedAtUtc ? formatDate(order.shipmentShippedAtUtc) : '—' }}</div>
                  </div>
                  <div>
                    <div class="text-sm text-muted-color">Delivered</div>
                    <div class="font-medium">{{ order.shipmentDeliveredAtUtc ? formatDate(order.shipmentDeliveredAtUtc) : '—' }}</div>
                  </div>
                </div>

                <!-- Section: Shipments — per-shipment tracking input and status control -->
                <div class="mt-6">
                  <h3 class="font-semibold mb-2">Shipments</h3>
                  <DataTable :value="shipments" scrollable data-key="id" striped-rows>
                    <Column header="Shipping Method">
                      <template #body="{ data }">{{ data.shippingMethodName ?? data.shippingMethodId }}</template>
                    </Column>
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
                        <div class="flex items-center gap-2">
                          <Button
                            icon="pi pi-save"
                            label="Save"
                            size="small"
                            :disabled="!canSaveShipment(data)"
                            :loading="savingShipmentId === data.id"
                            @click="saveShipmentStatus(data)"
                          />
                          <Button
                            icon="pi pi-send"
                            label="Mark Shipped"
                            size="small"
                            severity="info"
                            :disabled="!canMarkShipped(data)"
                            :loading="savingShipmentId === data.id"
                            @click="markShipmentShipped(data)"
                          />
                          <Button
                            icon="pi pi-check"
                            label="Mark Delivered"
                            size="small"
                            severity="success"
                            :disabled="!canMarkDelivered(data)"
                            :loading="savingShipmentId === data.id"
                            @click="markShipmentDelivered(data)"
                          />
                        </div>
                      </template>
                    </Column>
                    <template #empty>No shipments yet for this order.</template>
                  </DataTable>
                </div>

                <!-- Section: Timeline — chronological status events derived from order timestamps -->
                <div class="mt-6">
                  <h3 class="font-semibold mb-2">Timeline</h3>
                  <Timeline :value="timeline" layout="vertical" align="left">
                    <template #opposite="{ item }">
                      <span class="text-xs text-muted-color">{{ item.occurredAtUtc ? formatDate(item.occurredAtUtc) : '—' }}</span>
                    </template>
                    <template #content="{ item }">
                      <span class="font-medium">{{ item.label }}</span>
                    </template>
                  </Timeline>
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
                <DataTable :value="payments" scrollable data-key="id" striped-rows>
                  <Column field="number" header="Number" />
                  <Column field="amount" header="Amount">
                    <template #body="{ data }">{{ formatCurrency(data.amount, data.currency ?? 'USD') }}</template>
                  </Column>
                  <Column field="state" header="State">
                    <template #body="{ data }"><Tag :value="data.state" :severity="PAYMENT_STATE_SEVERITY[data.state as PaymentRecordState]" /></template>
                  </Column>
                  <Column field="paymentStatus" header="Payment Status">
                    <template #body="{ data }">{{ data.paymentStatus ?? '—' }}</template>
                  </Column>
                  <Column header="Actions">
                    <template #body="{ data }">
                      <div class="flex items-center gap-2">
                        <Button
                          v-if="canCapturePayment(data.state)"
                          icon="pi pi-check"
                          label="Capture"
                          size="small"
                          severity="primary"
                          :loading="paymentActionId === data.id"
                          @click="capturePayment(data)"
                        />
                        <Button
                          v-if="canRefundPayment(data.state)"
                          icon="pi pi-refresh"
                          label="Refund"
                          size="small"
                          severity="secondary"
                          :loading="paymentActionId === data.id"
                          @click="refundPayment(data)"
                        />
                        <Button
                          v-if="canVoidPayment(data.state)"
                          icon="pi pi-times"
                          label="Void"
                          size="small"
                          severity="danger"
                          :loading="paymentActionId === data.id"
                          @click="voidPayment(data)"
                        />
                      </div>
                    </template>
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
