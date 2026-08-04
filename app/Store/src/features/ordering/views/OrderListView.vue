<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useOrderStore, type OrderStatusFilter } from '../stores/orderStore'
import OrderCard from '../components/OrderCard.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import { useNotify } from '@/shared/composables/useNotify'

const store = useOrderStore()
const notify = useNotify()
const selectedStatus = ref<OrderStatusFilter>('All')

const statusOptions = [
  { label: 'All Orders', value: 'All' },
  { label: 'Placed', value: 'Placed' },
  { label: 'Canceled', value: 'Canceled' },
  { label: 'Expired', value: 'Expired' },
]

// Restore: Derive the dropdown from the persisted store filter when revisiting the page.
function statusFromFilter(filter: string): OrderStatusFilter {
  const match = /^status=(Placed|Canceled|Expired|Draft)$/.exec(filter)
  return (match?.[1] as OrderStatusFilter | undefined) ?? 'All'
}

onMounted(() => {
  selectedStatus.value = statusFromFilter(store.filter)
  store.fetchOrders()
})

function onStatusChange(value: OrderStatusFilter): void {
  selectedStatus.value = value
  store.setStatusFilter(value)
}

async function onCancel(id: string): Promise<void> {
  const ok = await store.cancelOrder(id)
  if (ok) notify.success('Order cancelled', 'Your order was cancelled.')
  else notify.error('Cancel failed', store.error ?? 'Unable to cancel the order.')
}
</script>
<template>
  <div>
    <!-- Section: Orders Header -->
    <div class="flex flex-wrap items-center justify-between gap-4 mb-6">
      <h1 class="text-2xl font-bold text-stone-900">Orders</h1>
      <Select
        :model-value="selectedStatus"
        :options="statusOptions"
        option-label="label"
        option-value="value"
        placeholder="Filter by status"
        class="w-48"
        @update:model-value="(val: OrderStatusFilter) => onStatusChange(val)"
      />
    </div>

    <!-- Section: Error -->
    <Message v-if="store.listError" severity="error" :closable="false" class="mb-4">
      {{ store.listError }}
    </Message>

    <!-- Section: Loading -->
    <div v-if="store.loading" class="space-y-4">
      <Skeleton v-for="i in 3" :key="i" height="6rem" class="rounded-xl" />
    </div>

    <!-- Section: Empty -->
    <EmptyState
      v-else-if="store.items.length === 0"
      icon="pi pi-shopping-bag"
      message="No orders found"
      action-label="Continue Shopping"
      action-to="/shop"
    />

    <!-- Section: Order Cards -->
    <div v-else class="space-y-4">
      <OrderCard v-for="order in store.items" :key="order.id" :order="order" @cancel="(id) => onCancel(id)" />
    </div>

    <!-- Section: Pagination -->
    <Paginator
      v-if="store.totalPages > 1"
      :rows="store.pageSize"
      :total-records="store.totalCount"
      :first="(store.page - 1) * store.pageSize"
      @page="(e: { page: number }) => store.setPage(e.page + 1)"
      class="mt-6"
    />
  </div>
</template>
