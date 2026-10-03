<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useNotify } from '@/shared/composables/useNotify'
import { formatCurrency } from '@/shared/utils/currency'
import { formatDateTimeUtc } from '@/shared/utils/date'
import { useOrders } from '../composables/useOrders'
import { useAddresses } from '@/features/profile/composables/useAddresses'
import { useShipping } from '@/features/shipping/composables'
import { OrderApi } from '../services'
import { CheckoutApi } from '../services/checkoutApi'
import { getPaymentMethods } from '@/features/payment/services/paymentApi'
import { PAYMENT_STATE_SEVERITY, type PaymentRecordState } from '@/features/payment/types/payment'
import type { OrderStatus, OrderTrackingResponse, ShipmentStatus } from '../types'

const route = useRoute()
const orders = useOrders()
const addresses = useAddresses()
const shipping = useShipping()

// Method: Resolve the applied shipping method name, falling back to the raw id.
const shippingMethodName = computed(() => {
  const id = orders.currentOrder?.shippingMethodId
  if (!id) return null
  return shipping.methods.find((m) => m.id === id)?.name ?? id
})

usePageTitle(() => (orders.currentOrder ? `Order ${orders.currentOrder.number}` : 'Order'))

// Severity: orderStore exposes statuses but no Tag severity mapping, so the
// view maps each status to a Tag severity (mirrors the Admin SPA's mapping).
const statusSeverity: Record<OrderStatus, 'warn' | 'success' | 'danger' | 'secondary'> = {
  Draft: 'warn',
  Placed: 'success',
  Canceled: 'danger',
  Completed: 'success',
  Expired: 'secondary',
}

// Severity: Shipment status Tag mapping (mirrors the Admin SPA's fulfillment map).
const shipmentSeverity: Record<ShipmentStatus, 'warn' | 'success' | 'danger' | 'secondary' | 'info'> = {
  Pending: 'warn',
  Ready: 'info',
  Shipped: 'info',
  Delivered: 'success',
  Backorder: 'warn',
  Canceled: 'danger',
}

// Dialog: Cancel-order confirmation plus notify feedback for the action buttons.
const confirm = useConfirm()
const notify = useNotify()
const payNowLoading = ref(false)

// Gate: Pay-now applies only to a placed order with an outstanding balance.
const canPayNow = computed(
  () => orders.currentOrder?.status === 'Placed' && (orders.currentOrder.outstandingBalance ?? 0) > 0,
)

// Gate: Cancel-order applies to draft or placed orders.
const canCancelOrder = computed(
  () => orders.currentOrder?.status === 'Draft' || orders.currentOrder?.status === 'Placed',
)

// Dialog: Tracking popup visibility for the Timeline of status events.
const trackingOpen = ref(false)
const tracking = ref<OrderTrackingResponse | null>(null)

// Track: orderStore.fetchOrder fetches tracking but discards the response, so
// the view fetches the tracking endpoint directly to build the timeline.
interface TrackingEvent {
  label: string
  date: string
}

// Timeline: Filter the tracking timestamps to the events that actually happened,
// in the canonical status progression order.
const timelineEvents = computed<TrackingEvent[]>(() => {
  const t = tracking.value
  if (!t) return []
  const events: TrackingEvent[] = [
    { label: 'Order placed', date: t.orderCreatedAt },
    { label: 'Order approved', date: t.orderApprovedAt ?? '' },
    { label: 'Shipped', date: t.shippedAt ?? '' },
    { label: 'Delivered', date: t.deliveredAt ?? '' },
    { label: 'Order completed', date: t.orderCompletedAt ?? '' },
    { label: 'Order canceled', date: t.orderCanceledAt ?? '' },
  ]
  return events.filter(e => e.date.length > 0)
})

// Address: Resolve the order's shipping address id against the saved-address list.
const shipAddress = computed(
  () => addresses.shippingAddresses.find(a => a.id === orders.currentOrder?.shipAddressId) ?? null,
)

// Load: Fetch order detail, addresses and tracking together when the route id changes.
async function loadOrder(): Promise<void> {
  trackingOpen.value = false
  const id = String(route.params.id)
  await Promise.all([orders.fetchOrder(id), addresses.fetchAddresses()])
  const result = await OrderApi.getOrderTracking(id)
  if (result.isSuccess) tracking.value = result.value
}

watch(() => route.params.id, () => void loadOrder(), { immediate: true })

// Load: Fetch shipping methods once so the applied method name can resolve.
onMounted(() => {
  void shipping.fetchMethods()
})

// Pay: Start a hosted-checkout payment for the outstanding balance.
async function payNow(): Promise<void> {
  const order = orders.currentOrder
  if (!order) return
  payNowLoading.value = true
  try {
    const methods = await getPaymentMethods({ pageSize: 50 })
    const method = methods.isSuccess ? methods.items.find((m) => m.active) ?? null : null
    if (!method) {
      notify.warn('No payment methods available')
      return
    }
    const result = await CheckoutApi.createPaymentIntent({ orderId: order.id, paymentMethodId: method.id })
    if (result.isSuccess) {
      // Redirect: Send the customer to the gateway's hosted checkout page.
      if (result.value.checkoutUrl) {
        window.location.href = result.value.checkoutUrl
      } else {
        notify.success('Payment started')
      }
    } else {
      notify.error(result.message ?? 'Could not start payment')
    }
  } finally {
    payNowLoading.value = false
  }
}

// Cancel: Confirm with the user, then cancel and refresh the order detail.
function confirmCancelOrder(): void {
  const order = orders.currentOrder
  if (!order) return
  confirm.require({
    message: `Cancel order ${order.number}?`,
    header: 'Cancel order',
    accept: async () => {
      const ok = await orders.cancelOrder(order.id)
      if (ok) await orders.fetchOrder(order.id)
    },
    reject: () => undefined,
  })
}

// Reorder: cartStore has no reorder action, so the button stays disabled until then.
</script>

<template>
  <Card>
    <template #title>Order Details</template>
    <template #content>
      <!-- Section: Loading State — skeleton blocks while the detail fetches -->
      <div v-if="orders.detailLoading && !orders.currentOrder" class="flex flex-col gap-3">
        <Skeleton height="3rem" />
        <Skeleton height="10rem" />
        <Skeleton height="6rem" />
      </div>

      <!-- Section: Error State — message and retry when the fetch fails -->
      <div v-else-if="orders.error && !orders.currentOrder" class="flex flex-col items-center gap-4 py-8">
        <Message severity="error" :closable="false">{{ orders.error }}</Message>
        <Button label="Retry" severity="secondary" outlined @click="loadOrder" />
      </div>

      <template v-else-if="orders.currentOrder">
        <!-- Section: Page Header — number, status tag and order actions -->
        <div class="mb-6 flex flex-wrap items-center gap-3">
          <h1 class="text-xl font-bold">{{ orders.currentOrder.number }}</h1>
          <Tag :value="orders.currentOrder.status" :severity="statusSeverity[orders.currentOrder.status]" rounded />
          <div class="ml-auto flex gap-2">
            <Button label="Track" icon="pi pi-history" severity="secondary" variant="text" @click="trackingOpen = true" />
            <Button
              v-if="canPayNow"
              label="Pay now"
              icon="pi pi-credit-card"
              severity="primary"
              :loading="payNowLoading"
              @click="payNow"
            />
            <Button
              v-if="canCancelOrder"
              label="Cancel order"
              icon="pi pi-times"
              severity="danger"
              variant="text"
              :loading="orders.cancelLoading"
              @click="confirmCancelOrder"
            />
            <Button
              label="Reorder"
              icon="pi pi-refresh"
              severity="secondary"
              variant="text"
              disabled
              v-tooltip.bottom="'Reorder is not available yet'"
            />
          </div>
        </div>

        <!-- Section: Line Items — ordered items with quantity, price and line total -->
        <DataTable
          :value="orders.currentOrder.lineItems ?? []"
          dataKey="id"
          tableStyle="min-width: 40rem"
          class="mb-6"
        >
          <Column header="Item">
            <template #body="{ data }">
              <div class="flex items-center gap-3">
                <img
                  v-if="data.productImageUrl"
                  :src="data.productImageUrl"
                  :alt="data.productName ?? data.variantId ?? 'Product'"
                  class="h-12 w-12 shrink-0 rounded-md object-cover"
                />
                <RouterLink
                  v-if="data.productId"
                  :to="`/products/${data.productId}`"
                  class="font-medium text-brand hover:underline"
                >
                  {{ data.productName ?? data.variantId ?? 'Unknown' }}
                </RouterLink>
                <span v-else>{{ data.productName ?? data.variantId ?? 'Unknown' }}</span>
              </div>
            </template>
          </Column>
          <Column header="Qty">
            <template #body="{ data }">{{ data.quantity }}</template>
          </Column>
          <Column header="Unit Price">
            <template #body="{ data }">{{ formatCurrency(data.price) }}</template>
          </Column>
          <Column header="Line Total">
            <template #body="{ data }">
              {{ formatCurrency(data.total) }}
              <div v-if="data.adjustmentTotal" class="text-xs text-muted">+ {{ formatCurrency(data.adjustmentTotal) }} adj</div>
            </template>
          </Column>
          <template #empty>
            <Message severity="info" :closable="false">
              No line items on this order.
            </Message>
          </template>
        </DataTable>

        <!-- Section: Timeline — chronological status events from the order payload -->
        <Card class="mb-6">
          <template #title>Order Timeline</template>
          <template #content>
            <Timeline
              v-if="orders.currentOrder.timeline.length > 0"
              :value="orders.currentOrder.timeline"
              layout="vertical"
              align="left"
            >
              <template #opposite="{ item }">
                <span class="text-xs text-muted">{{ item.occurredAtUtc ? formatDateTimeUtc(item.occurredAtUtc) : '—' }}</span>
              </template>
              <template #content="{ item }">
                <span class="font-medium">{{ item.label }}</span>
              </template>
            </Timeline>
            <Message v-else severity="info" :closable="false">No timeline events available.</Message>
          </template>
        </Card>

        <!-- Section: Shipments — tracking number and status per shipment -->
        <Card class="mb-6">
          <template #title>Shipments</template>
          <template #content>
            <DataTable
              v-if="orders.currentOrder.shipments.length > 0"
              :value="orders.currentOrder.shipments"
              dataKey="id"
              tableStyle="min-width: 30rem"
            >
              <Column header="Carrier">
                <template #body="{ data }">{{ data.shippingMethodName ?? data.shippingMethodId }}</template>
              </Column>
              <Column header="Tracking Number">
                <template #body="{ data }">{{ data.trackingNumber }}</template>
              </Column>
              <Column header="Status">
                <template #body="{ data }"><Tag :value="data.status" :severity="shipmentSeverity[data.status as ShipmentStatus]" /></template>
              </Column>
              <Column header="Shipped">
                <template #body="{ data }">{{ data.shippedAtUtc ? formatDateTimeUtc(data.shippedAtUtc) : '—' }}</template>
              </Column>
              <Column header="Delivered">
                <template #body="{ data }">{{ data.deliveredAtUtc ? formatDateTimeUtc(data.deliveredAtUtc) : '—' }}</template>
              </Column>
            </DataTable>
            <Message v-else severity="info" :closable="false">No shipments yet.</Message>
          </template>
        </Card>

        <!-- Section: Payments — recorded transactions and their states -->
        <Card class="mb-6">
          <template #title>Payments</template>
          <template #content>
            <DataTable
              v-if="orders.currentOrder.payments.length > 0"
              :value="orders.currentOrder.payments"
              dataKey="id"
              tableStyle="min-width: 30rem"
            >
              <Column header="Amount">
                <template #body="{ data }">{{ formatCurrency(data.amount) }}</template>
              </Column>
              <Column header="State">
                <template #body="{ data }"><Tag :value="data.state" :severity="PAYMENT_STATE_SEVERITY[data.state as PaymentRecordState]" /></template>
              </Column>
              <Column header="Payment Status">
                <template #body="{ data }">{{ data.paymentStatus ?? '—' }}</template>
              </Column>
              <Column header="Completed">
                <template #body="{ data }">{{ data.completedAtUtc ? formatDateTimeUtc(data.completedAtUtc) : '—' }}</template>
              </Column>
            </DataTable>
            <Message v-else severity="info" :closable="false">No payments recorded.</Message>
          </template>
        </Card>

        <!-- Section: Summary & Shipping — totals card beside the delivery address -->
        <div class="grid grid-cols-1 gap-6 lg:grid-cols-2">
          <Card class="self-start">
            <template #title>Summary</template>
            <template #content>
              <div class="flex flex-col gap-3 text-sm">
                <div class="flex items-center justify-between">
                  <span class="text-muted">Subtotal</span>
                  <span>{{ formatCurrency(orders.currentOrder.itemTotal) }}</span>
                </div>
                <div class="flex items-center justify-between">
                  <span class="text-muted">Shipping</span>
                  <span>{{ formatCurrency(orders.currentOrder.shipmentTotal) }}</span>
                </div>
                <div v-if="orders.currentOrder.adjustmentTotal !== 0" class="flex items-center justify-between">
                  <span class="text-muted">Adjustments / Discounts</span>
                  <span>{{ formatCurrency(orders.currentOrder.adjustmentTotal) }}</span>
                </div>
                <div v-if="shippingMethodName" class="flex items-center justify-between">
                  <span class="text-muted">Shipping method</span>
                  <span>{{ shippingMethodName }}</span>
                </div>
                <div v-if="orders.currentOrder.shippingCalculation" class="flex items-center justify-between">
                  <span class="text-muted">Weight</span>
                  <span>{{ orders.currentOrder.shippingCalculation.totalWeight }} kg</span>
                </div>
                <div v-if="orders.currentOrder.shippingCalculation?.isFreeShipping" class="flex items-center justify-between">
                  <span class="text-muted">Free shipping</span>
                  <span class="font-semibold text-success">Yes</span>
                </div>
                <div class="flex items-center justify-between">
                  <span class="text-muted">Tax</span>
                  <!-- Tax: The order DTO exposes no tax field, so the row shows a dash. -->
                  <span class="text-subtle">—</span>
                </div>
                <Divider />
                <div class="flex items-center justify-between font-semibold">
                  <span>Total</span>
                  <span>{{ formatCurrency(orders.currentOrder.total) }}</span>
                </div>
                <div v-if="orders.currentOrder.paymentTotal !== 0" class="flex items-center justify-between">
                  <span class="text-muted">Paid</span>
                  <span>{{ formatCurrency(orders.currentOrder.paymentTotal) }}</span>
                </div>
                <div v-if="orders.currentOrder.outstandingBalance !== 0" class="flex items-center justify-between">
                  <span class="text-muted">Outstanding</span>
                  <span>{{ formatCurrency(orders.currentOrder.outstandingBalance) }}</span>
                </div>
              </div>
            </template>
          </Card>

          <Card class="self-start">
            <template #title>Shipping Address</template>
            <template #content>
              <div v-if="shipAddress" class="text-sm">
                <div class="font-semibold">{{ shipAddress.firstName }} {{ shipAddress.lastName ?? '' }}</div>
                <div>{{ shipAddress.address1 }}</div>
                <div v-if="shipAddress.address2">{{ shipAddress.address2 }}</div>
                <div>{{ shipAddress.city }}{{ shipAddress.zipCode ? `, ${shipAddress.zipCode}` : '' }}</div>
                <div>{{ shipAddress.countryName }}</div>
                <div v-if="shipAddress.phone" class="mt-1 text-muted">{{ shipAddress.phone }}</div>
              </div>
              <Message v-else severity="warn" :closable="false">
                Shipping address unavailable for this order.
              </Message>
            </template>
          </Card>
        </div>
      </template>
    </template>
  </Card>

  <!-- Section: Tracking Dialog — timeline of status events for the order -->
  <Dialog v-model:visible="trackingOpen" header="Order Tracking" modal>
    <div v-if="timelineEvents.length > 0">
      <!-- Estimate: Show the promised delivery window above the timeline -->
      <div v-if="tracking?.estimatedDeliveryAt" class="mb-4 text-sm text-muted">
        Estimated delivery: {{ formatDateTimeUtc(tracking.estimatedDeliveryAt) }}
      </div>
      <Timeline :value="timelineEvents" layout="vertical" align="left">
        <template #opposite="{ item }">
          <span class="text-xs text-muted">{{ formatDateTimeUtc(item.date) }}</span>
        </template>
        <template #content="{ item }">
          <span class="font-medium">{{ item.label }}</span>
        </template>
      </Timeline>
    </div>
    <Message v-else severity="info" :closable="false">No tracking events available.</Message>
  </Dialog>
</template>
