<script setup lang="ts">
import { onMounted } from 'vue'
import { useInventoryDashboardStore } from '../stores/inventory-dashboard.store'
import { storeToRefs } from 'pinia'

const store = useInventoryDashboardStore()
const { data, loading } = storeToRefs(store)

onMounted(async () => {
  await store.fetchDashboard()
})
</script>

<template>
  <div class="p-6">
    <div class="mb-8">
      <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
        Inventory Dashboard
      </h2>
      <p class="text-surface-500 dark:text-surface-400">
        Stock levels, locations, and recent movements.
      </p>
    </div>

    <div v-if="loading && !data" class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-6 mb-8">
      <Skeleton v-for="i in 6" :key="i" height="100px" class="rounded-2xl" />
    </div>

    <div v-else-if="data" class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-6 mb-8">
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">SKUs Tracked</p>
        <p class="text-3xl font-bold mt-2">{{ data.totalSkusTracked.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">In Stock</p>
        <p class="text-3xl font-bold mt-2 text-green-600">{{ data.inStockCount.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Out of Stock</p>
        <p class="text-3xl font-bold mt-2 text-red-500">{{ data.outOfStockCount.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Low Stock</p>
        <p class="text-3xl font-bold mt-2 text-orange-500">{{ data.lowStockCount.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Locations</p>
        <p class="text-3xl font-bold mt-2">{{ data.stockLocationCount.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Avg Items/Location</p>
        <p class="text-3xl font-bold mt-2">{{ data.itemsPerLocationAverage.toLocaleString() }}</p>
      </div>
    </div>

    <div v-if="data">
      <h3 class="text-lg font-semibold mb-3">Recent Stock Movements</h3>
      <DataTable :value="data.recentMovements" class="text-sm" stripedRows>
        <Column field="action" header="Action" />
        <Column field="quantity" header="Qty" />
        <Column field="reason" header="Reason" />
        <Column field="createdAtUtc" header="Date">
          <template #body="{ data: row }">
            {{ new Date(row.createdAtUtc).toLocaleDateString() }}
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>
