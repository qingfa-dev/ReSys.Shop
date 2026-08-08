<script setup lang="ts">
import { watch } from 'vue'
import { useRoute } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useOrderStore } from '../stores/orderStore'

usePageTitle('Order')
const route = useRoute()
const store = useOrderStore()

watch(() => route.params.id, (id) => {
  if (typeof id === 'string') store.fetchOrder(id)
}, { immediate: true })

function statusSeverity(s: string): 'info' | 'warn' | 'success' | 'danger' {
  const m: Record<string, any> = { Placed: 'info', Shipped: 'warn', Delivered: 'success', Canceled: 'danger' }
  return m[s] ?? 'info'
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })
}

async function onCancel(): Promise<void> {
  if (store.currentOrder) await store.cancelOrder(store.currentOrder.id)
}
</script>
<template>
  <div>
    <!-- Section: Loading State — skeleton while order detail loads -->
    <div v-if="store.detailLoading" class="space-y-4">
      <Skeleton width="40%" height="2rem" />
      <Skeleton width="100%" height="8rem" />
      <Skeleton width="100%" height="6rem" />
    </div>

    <!-- Section: Error State — retry or return to order list -->
    <div v-else-if="store.error" class="text-center py-12">
      <p class="text-neutral-500 mb-4">{{ store.error }}</p>
      <Button label="Back to Orders" severity="secondary" outlined as="router-link" to="/account/orders" />
    </div>

    <!-- Section: Content Card — order summary, line totals, and cancel action -->
    <div v-else-if="store.currentOrder">
      <!-- Section: Page Header — back link and status badge -->
      <div class="flex items-center justify-between mb-6">
        <router-link to="/account/orders" class="text-sm text-neutral-500 hover:text-neutral-900">&larr; Back to Orders</router-link>
        <Tag :value="store.currentOrder.status" :severity="statusSeverity(store.currentOrder.status)" />
      </div>

      <h1 class="text-2xl font-bold text-neutral-900 mb-1">Order #{{ store.currentOrder.number }}</h1>
      <p class="text-sm text-neutral-500 mb-8">Placed on {{ formatDate(store.currentOrder.createdAtUtc) }}</p>

      <div class="space-y-8">
        <div>
          <h2 class="text-sm font-semibold text-neutral-900 uppercase tracking-wide mb-3">Summary</h2>
          <div class="bg-white border border-neutral-200 rounded-lg divide-y divide-neutral-100">
            <div class="flex justify-between p-4 text-sm"><span class="text-neutral-500">Subtotal</span><span class="font-mono">${{ store.currentOrder.itemTotal.toFixed(2) }}</span></div>
            <div class="flex justify-between p-4 text-sm"><span class="text-neutral-500">Shipping</span><span class="font-mono">${{ store.currentOrder.shipmentTotal.toFixed(2) }}</span></div>
            <div class="flex justify-between p-4 text-sm"><span class="text-neutral-500">Tax</span><span class="font-mono">${{ store.currentOrder.adjustmentTotal.toFixed(2) }}</span></div>
            <div class="flex justify-between p-4 text-sm font-semibold"><span class="text-neutral-900">Total</span><span class="font-mono">${{ store.currentOrder.total.toFixed(2) }}</span></div>
          </div>
        </div>

        <!-- Section: Action Footer — cancel order button for Placed orders -->
        <div v-if="store.currentOrder.status === 'Placed'" class="flex justify-end">
          <ConfirmDialog />
          <Button label="Cancel Order" severity="danger" outlined @click="onCancel()" />
        </div>
      </div>
    </div>
  </div>
</template>
