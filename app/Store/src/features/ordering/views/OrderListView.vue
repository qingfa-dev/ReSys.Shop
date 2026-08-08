<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useOrderStore } from '../stores/orderStore'

usePageTitle('My Orders')
const orders = useOrderStore()

onMounted(() => { orders.fetchOrders() })

function statusSeverity(status: string): 'info' | 'warn' | 'success' | 'danger' {
  const map: Record<string, any> = { Placed: 'info', Shipped: 'warn', Delivered: 'success', Canceled: 'danger' }
  return map[status] ?? 'info'
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })
}
</script>
<template>
  <div>
    <!-- Section: Page Header — title and contextual intro -->
    <h1 class="text-2xl font-bold text-neutral-900 mb-6">Your Orders</h1>

    <!-- Section: Loading State — skeleton placeholders while first fetch runs -->
    <div v-if="orders.loading && orders.items.length === 0" class="space-y-3">
      <Skeleton v-for="i in 3" :key="i" height="5rem" />
    </div>

    <!-- Section: Error State — retry prompt on fetch failure -->
    <div v-else-if="orders.error" class="text-center py-12">
      <p class="text-neutral-500 mb-4">{{ orders.error }}</p>
      <Button label="Retry" severity="secondary" outlined @click="orders.refresh()" />
    </div>

    <!-- Section: Empty State — prompt when user has no orders -->
    <div v-else-if="orders.items.length === 0" class="text-center py-16">
      <i class="pi pi-inbox text-4xl text-neutral-300 mb-4 block" />
      <p class="text-lg font-medium text-neutral-900 mb-2">No orders yet</p>
      <p class="text-sm text-neutral-500 mb-6">When you place an order, it will appear here.</p>
      <Button label="Start Shopping" severity="secondary" outlined as="router-link" to="/shop" />
    </div>

    <!-- Section: Data Table — order history cards with status and total -->
    <div v-else>
      <router-link
        v-for="order in orders.items"
        :key="order.id"
        :to="`/account/orders/${order.id}`"
        class="block mb-3"
      >
        <div class="flex items-center justify-between p-4 bg-white rounded-lg border border-neutral-200 hover:border-neutral-400 transition-colors">
          <div class="flex items-center gap-4">
            <div>
              <p class="text-sm font-semibold text-neutral-900">Order #{{ order.number }}</p>
              <p class="text-xs text-neutral-500">{{ formatDate(order.createdAtUtc) }}</p>
            </div>
          </div>
          <div class="flex items-center gap-4">
            <span class="text-sm font-mono font-medium text-neutral-900">${{ order.total.toFixed(2) }}</span>
            <Tag :value="order.status" :severity="statusSeverity(order.status)" />
          </div>
        </div>
      </router-link>
      <Paginator
        v-if="orders.totalPages > 1"
        :rows="orders.pageSize"
        :total-records="orders.totalCount"
        :first="(orders.page - 1) * orders.pageSize"
        class="mt-6"
        @page="(e: any) => orders.goToPage(e.page + 1)"
      />
    </div>
  </div>
</template>
