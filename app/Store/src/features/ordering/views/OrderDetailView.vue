<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import StatusTag from '@/shared/components/StatusTag.vue'
import { formatCurrency } from '@/shared/utils/currency'
import { formatDateTimeUtc } from '@/shared/utils/date'
import { useNotify } from '@/shared/composables/useNotify'
import { useOrderStore } from '../stores/orderStore'
import { buildOrderTimeline, isOrderCancellable } from '../types/order'

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const store = useOrderStore()

const orderId = computed(() => route.params.id as string)
const order = computed(() => store.currentOrder)
const timeline = computed(() => (order.value ? buildOrderTimeline(order.value) : []))

async function loadOrder(): Promise<void> {
  const ok = await store.fetchOrder(orderId.value)
  if (!ok) notify.error('Order not found', 'The requested order could not be loaded.')
}

function requestCancel(): void {
  const target = order.value
  if (!target) return
  confirm.require({
    message: `Cancel order #${target.number}? This cannot be undone.`,
    header: 'Cancel Order',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Keep Order',
    acceptLabel: 'Cancel Order',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const ok = await store.cancelOrder(target.id)
      if (ok) notify.success('Order cancelled', 'Your order was cancelled.')
      else notify.error('Cancel failed', store.error ?? 'Unable to cancel the order.')
    },
  })
}

// Trigger: Reload when navigating directly between two order detail routes.
watch(orderId, (id) => {
  if (id) {
    store.resetDetail()
    loadOrder()
  }
})

onMounted(loadOrder)
</script>
<template>
  <div>
    <!-- Section: Page Header -->
    <div class="flex flex-wrap items-center gap-3 mb-6">
      <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="router.push('/account/orders')" />
      <h1 class="text-2xl font-bold text-stone-900">{{ order ? `Order #${order.number}` : 'Order' }}</h1>
      <StatusTag v-if="order" :status="order.status" />
      <div class="flex-1" />
      <Button
        v-if="order && isOrderCancellable(order.status)"
        label="Cancel Order"
        severity="danger"
        outlined
        icon="pi pi-times"
        :loading="store.cancelLoading"
        @click="requestCancel"
      />
    </div>

    <!-- Section: Error -->
    <Message v-if="store.error" severity="error" :closable="false" class="mb-4">
      {{ store.error }}
    </Message>

    <!-- Section: Loading -->
    <div v-if="store.detailLoading" class="space-y-4">
      <Skeleton v-for="i in 3" :key="i" height="8rem" class="rounded-xl" />
    </div>

    <!-- Section: Order Detail -->
    <template v-else-if="order">
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Section: Timeline + Totals -->
        <div class="lg:col-span-2 space-y-6">
          <div class="bg-white rounded-xl border border-stone-200 p-6">
            <h3 class="text-lg font-semibold text-stone-900 mb-4">Order Timeline</h3>
            <Timeline :value="timeline" layout="vertical" align="left" data-key="label">
              <template #marker="{ item }">
                <span
                  class="inline-flex items-center justify-center w-8 h-8 rounded-full border-2"
                  :class="item.status === 'Canceled'
                    ? 'border-red-300 bg-red-50 text-red-500'
                    : 'border-teal-300 bg-teal-50 text-teal-500'"
                >
                  <i :class="item.status === 'Canceled' ? 'pi pi-times' : 'pi pi-check'" class="text-xs" />
                </span>
              </template>
              <template #content="{ item }">
                <div class="text-sm">
                  <p class="font-medium text-stone-900">{{ item.label }}</p>
                  <p class="text-stone-500">{{ formatDateTimeUtc(item.date) }}</p>
                </div>
              </template>
            </Timeline>
          </div>

          <!-- Section: Totals -->
          <div class="bg-white rounded-xl border border-stone-200 p-6">
            <h3 class="text-lg font-semibold text-stone-900 mb-4">Order Summary</h3>
            <div class="space-y-2 text-sm">
              <div class="flex justify-between text-stone-600">
                <span>Items ({{ order.itemCount }})</span>
                <span>{{ formatCurrency(order.itemTotal) }}</span>
              </div>
              <div class="flex justify-between text-stone-600">
                <span>Adjustments</span>
                <span>{{ formatCurrency(order.adjustmentTotal) }}</span>
              </div>
              <div class="flex justify-between text-stone-600">
                <span>Shipping</span>
                <span>{{ formatCurrency(order.shipmentTotal) }}</span>
              </div>
              <Divider />
              <div class="flex justify-between font-semibold text-stone-900">
                <span>Total</span>
                <span>{{ formatCurrency(order.total) }}</span>
              </div>
              <div class="flex justify-between text-stone-600">
                <span>Paid</span>
                <span>{{ formatCurrency(order.paymentTotal) }}</span>
              </div>
              <div class="flex justify-between text-stone-600">
                <span>Outstanding Balance</span>
                <span>{{ formatCurrency(order.outstandingBalance) }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Section: Order Details -->
        <div class="bg-white rounded-xl border border-stone-200 p-6 h-fit">
          <h3 class="text-lg font-semibold text-stone-900 mb-4">Order Details</h3>
          <dl class="space-y-3 text-sm">
            <div>
              <dt class="text-stone-500">Checkout State</dt>
              <dd class="font-medium text-stone-900">{{ order.checkoutState }}</dd>
            </div>
            <div>
              <dt class="text-stone-500">Currency</dt>
              <dd class="font-medium text-stone-900">{{ order.currency }}</dd>
            </div>
            <div>
              <dt class="text-stone-500">Email</dt>
              <dd class="font-medium text-stone-900">{{ order.email ?? '—' }}</dd>
            </div>
            <div>
              <dt class="text-stone-500">Payment State</dt>
              <dd class="font-medium text-stone-900 capitalize">{{ order.paymentState ?? '—' }}</dd>
            </div>
            <div>
              <dt class="text-stone-500">Shipment State</dt>
              <dd class="font-medium text-stone-900 capitalize">{{ order.shipmentState ?? '—' }}</dd>
            </div>
            <div>
              <dt class="text-stone-500">Ship Address</dt>
              <dd class="font-mono text-xs text-stone-900 break-all">{{ order.shipAddressId ?? '—' }}</dd>
            </div>
            <div>
              <dt class="text-stone-500">Bill Address</dt>
              <dd class="font-mono text-xs text-stone-900 break-all">{{ order.billAddressId ?? '—' }}</dd>
            </div>
            <Divider />
            <div>
              <dt class="text-stone-500">Created</dt>
              <dd class="font-medium text-stone-900">{{ formatDateTimeUtc(order.createdAtUtc) }}</dd>
            </div>
            <div v-if="order.completedAtUtc">
              <dt class="text-stone-500">Completed</dt>
              <dd class="font-medium text-stone-900">{{ formatDateTimeUtc(order.completedAtUtc) }}</dd>
            </div>
            <div v-if="order.canceledAtUtc">
              <dt class="text-stone-500">Canceled</dt>
              <dd class="font-medium text-stone-900">{{ formatDateTimeUtc(order.canceledAtUtc) }}</dd>
            </div>
            <div v-if="order.modifiedAtUtc">
              <dt class="text-stone-500">Modified</dt>
              <dd class="font-medium text-stone-900">{{ formatDateTimeUtc(order.modifiedAtUtc) }}</dd>
            </div>
          </dl>
        </div>
      </div>
    </template>
  </div>
</template>
