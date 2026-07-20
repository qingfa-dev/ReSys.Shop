<script setup lang="ts">
import { onMounted } from 'vue'
import { useOrderingDashboardStore } from '../stores/ordering-dashboard.store'
import { storeToRefs } from 'pinia'
import { useI18n } from 'vue-i18n'
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'

const store = useOrderingDashboardStore()
const { t } = useI18n()
const { data, loading } = storeToRefs(store)

onMounted(async () => {
  await store.fetchDashboard()
})
</script>

<template>
  <PageShell maxWidth="7xl">
    <PageHeader :title="t('ordering.titles.dashboard')" :description="t('ordering.descriptions.dashboard')" />

    <div v-if="loading && !data" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6 mb-8">
      <Skeleton v-for="i in 5" :key="i" height="100px" class="rounded-2xl" />
    </div>

    <div v-else-if="data" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6 mb-8">
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">{{ t('ordering.dashboard.total_orders') }}</p>
        <p class="text-3xl font-bold mt-2">{{ data.totalOrders.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">{{ t('ordering.dashboard.pending_fulfillment') }}</p>
        <p class="text-3xl font-bold mt-2">{{ data.pendingFulfillment.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">{{ t('ordering.dashboard.todays_orders') }}</p>
        <p class="text-3xl font-bold mt-2">{{ data.todayOrders.toLocaleString() }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">{{ t('ordering.dashboard.avg_order_value') }}</p>
        <p class="text-3xl font-bold mt-2">${{ data.averageOrderValue.toFixed(2) }}</p>
      </div>
      <div class="rounded-2xl border border-surface-200 dark:border-surface-700 p-5 bg-surface-0 dark:bg-surface-900">
        <p class="text-sm text-surface-500 dark:text-surface-400">{{ t('ordering.dashboard.total_revenue') }}</p>
        <p class="text-3xl font-bold mt-2">${{ data.totalRevenue.toLocaleString() }}</p>
      </div>
    </div>

    <div v-if="data" class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div class="lg:col-span-2">
        <h3 class="text-lg font-semibold mb-3">{{ t('ordering.dashboard.recent_orders') }}</h3>
        <DataTable :value="data.recentOrders" class="text-sm" stripedRows>
          <Column field="number" :header="t('ordering.table.order_number')" />
          <Column field="status" :header="t('ordering.table.status')" />
          <Column field="total" :header="t('ordering.table.total')">
            <template #body="{ data: row }">${{ row.total.toFixed(2) }}</template>
          </Column>
          <Column field="createdAtUtc" :header="t('ordering.table.date')">
            <template #body="{ data: row }">{{ new Date(row.createdAtUtc).toLocaleDateString() }}</template>
          </Column>
        </DataTable>
      </div>
      <div>
        <h3 class="text-lg font-semibold mb-3">{{ t('ordering.dashboard.status_breakdown') }}</h3>
        <div class="space-y-2">
          <div class="flex justify-between text-sm">
            <span>{{ t('ordering.status_labels.draft') }}</span>
            <span class="font-semibold">{{ data.statusBreakdown.draft }}</span>
          </div>
          <div class="flex justify-between text-sm">
            <span>{{ t('ordering.status_labels.placed') }}</span>
            <span class="font-semibold">{{ data.statusBreakdown.placed }}</span>
          </div>
          <div class="flex justify-between text-sm">
            <span>{{ t('ordering.status_labels.canceled') }}</span>
            <span class="font-semibold">{{ data.statusBreakdown.canceled }}</span>
          </div>
          <div class="flex justify-between text-sm">
            <span>{{ t('ordering.status_labels.expired') }}</span>
            <span class="font-semibold">{{ data.statusBreakdown.expired }}</span>
          </div>
        </div>
      </div>
    </div>
  </PageShell>
</template>
