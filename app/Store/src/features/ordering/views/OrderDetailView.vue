<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { formatCurrency } from '@/shared/utils/currency'
import { formatDateTimeUtc } from '@/shared/utils/date'
import { useOrders } from '../composables/useOrders'
import { useAddresses } from '@/features/profile/composables/useAddresses'
import { useShipping } from '@/features/shipping/composables'
import { OrderApi } from '../services'
import type { OrderStatus, OrderTrackingResponse } from '../types'

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
  Expired: 'secondary',
}

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
        <!-- Section: Page Header — number, status tag, track and reorder actions -->
        <div class="mb-6 flex flex-wrap items-center gap-3">
          <h1 class="text-xl font-bold">{{ orders.currentOrder.number }}</h1>
          <Tag :value="orders.currentOrder.status" :severity="statusSeverity[orders.currentOrder.status]" rounded />
          <div class="ml-auto flex gap-2">
            <Button label="Track" icon="pi pi-history" severity="secondary" variant="text" @click="trackingOpen = true" />
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
            <template #body="{ data }">{{ data.variantId ?? 'Unknown' }}</template>
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
