<script setup lang="ts">
import { onMounted } from 'vue'
import { useInventoryDashboardStore } from '../stores/inventory-dashboard.store'
import { storeToRefs } from 'pinia'
import { useI18n } from 'vue-i18n'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'

const { t } = useI18n()

const store = useInventoryDashboardStore()
const { data, loading } = storeToRefs(store)

onMounted(async () => {
  await store.fetchDashboard()
})
</script>

<template>
  <PageShell>
    <PageHeader title="Inventory Dashboard" description="Stock levels, locations, and recent movements." />

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
        <p class="text-3xl font-bold mt-2 text-pink-500">{{ data.outOfStockCount.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">Low Stock</p>
        <p class="text-3xl font-bold mt-2 text-orange-500">{{ data.lowStockCount.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">{{ t('inventory.titles.locations') }}</p>
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
        <Column field="action" :header="t('inventory.table.action')" />
        <Column field="quantity" :header="t('inventory.table.quantity')" />
        <Column field="reason" :header="t('inventory.table.reason')" />
        <Column field="createdAtUtc" :header="t('inventory.table.date')">
          <template #body="{ data: row }">
            {{ new Date(row.createdAtUtc).toLocaleDateString() }}
          </template>
        </Column>
      </DataTable>
    </div>
  </PageShell>
</template>
