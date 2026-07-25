<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter, useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { StatCard, LoadingSkeleton, ErrorState, ListLayout, AppCard } from '@/shared/components'
import Button from 'primevue/button'
import { InventoryDashboardApi } from '../api'
import type { InventoryDashboardResponse } from '../types'
import { ROUTE } from '../routes'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()

const data = ref<InventoryDashboardResponse | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

const metrics = computed(() => [
  { label: t('inventory.dashboard.total_stock_items'), value: data.value?.totalStockItems ?? 0, icon: 'pi pi-box', color: 'primary' as const },
  { label: t('inventory.dashboard.total_locations'), value: data.value?.totalLocations ?? 0, icon: 'pi pi-building', color: 'blue' as const },
  { label: t('inventory.dashboard.low_stock'), value: data.value?.lowStockCount ?? 0, icon: 'pi pi-exclamation-triangle', color: 'orange' as const },
  { label: t('inventory.dashboard.out_of_stock'), value: data.value?.outOfStockCount ?? 0, icon: 'pi pi-times-circle', color: 'red' as const },
  { label: t('inventory.dashboard.total_reserved'), value: data.value?.totalReservedQuantity ?? 0, icon: 'pi pi-lock', color: 'green' as const },
  { label: t('inventory.dashboard.pending_transfers'), value: data.value?.totalTransfersPending ?? 0, icon: 'pi pi-truck', color: 'blue' as const },
])

const quickActions = [
  { label: t('inventory.stocks.title'), icon: 'pi pi-box', route: { name: ROUTE.STOCKS.LIST } },
  { label: t('inventory.locations.title'), icon: 'pi pi-building', route: { name: ROUTE.LOCATIONS.LIST } },
  { label: t('inventory.transfers.title'), icon: 'pi pi-arrows-h', route: { name: ROUTE.TRANSFERS.LIST } },
  { label: t('inventory.movements.title'), icon: 'pi pi-history', route: { name: ROUTE.MOVEMENTS.LIST } },
]

const recentMovements = computed(() =>
  data.value?.recentMovements.map(m => ({
    sku: m.variantSku,
    location: m.locationName,
    qty: m.quantity,
    direction: m.direction,
    date: m.createdAt,
  })) ?? [],
)

async function fetchDashboard() {
  loading.value = true
  error.value = null
  try {
    const result = await InventoryDashboardApi.get()
    if (result.isSuccess) {
      data.value = result.value
    } else {
      error.value = result.message ?? 'Failed to load dashboard data'
    }
  } catch (err) {
    console.error(err)
    error.value = 'Failed to load dashboard data'
  }
  loading.value = false
}

onMounted(fetchDashboard)
</script>

<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('inventory.dashboard.title')"
        :subtitle="t('inventory.dashboard.subtitle')"
        :icon="route.meta?.icon as string | undefined"
      >
        <template #actions>
          <Button
            :label="t('inventory.stocks.title')"
            icon="pi pi-plus"
            size="small"
            @click="router.push({ name: ROUTE.STOCKS.CREATE })"
          />
        </template>
      </PageHeader>
    </template>

    <LoadingSkeleton v-if="loading" :rows="4" :columns="4" />

    <ErrorState
      v-else-if="error"
      :title="error"
      @retry="fetchDashboard"
    />

    <template v-else>
      <div class="grid grid-cols-2 gap-4 lg:grid-cols-3">
        <StatCard v-for="m in metrics" :key="m.label" :label="m.label" :value="m.value" :icon="m.icon" :color="m.color" />
      </div>

      <div class="my-6">
        <p class="mb-3 text-xs font-semibold uppercase tracking-wider text-surface-400">{{ t('inventory.dashboard.quick_actions') }}</p>
        <div class="flex flex-wrap gap-3">
          <Button
            v-for="a in quickActions"
            :key="a.label"
            :label="a.label"
            :icon="a.icon"
            outlined
            size="small"
            @click="router.push(a.route)"
          />
        </div>
      </div>

      <AppCard>
        <p class="mb-3 text-xs font-semibold uppercase tracking-wider text-surface-400">{{ t('inventory.dashboard.recent_movements') }}</p>
        <div v-if="recentMovements.length" class="divide-y divide-surface-200 dark:divide-surface-700">
          <div
            v-for="(m, i) in recentMovements"
            :key="i"
            class="flex items-center justify-between px-2 py-3 transition-colors hover:bg-surface-50 dark:hover:bg-surface-800"
          >
            <div>
              <p class="text-sm font-medium text-surface-900 dark:text-surface-0">{{ m.sku }}</p>
              <p class="text-xs text-surface-400">{{ m.location }}</p>
            </div>
            <div class="flex items-center gap-4">
              <span class="text-sm font-medium">{{ m.qty }}</span>
              <span
                class="rounded px-2 py-0.5 text-xs font-medium"
                :class="m.direction === 'In' ? 'bg-green-100 text-green-700 dark:bg-green-400/10 dark:text-green-400' : 'bg-red-100 text-red-700 dark:bg-red-400/10 dark:text-red-400'"
              >{{ m.direction }}</span>
              <span class="text-xs text-surface-400">{{ m.date }}</span>
            </div>
          </div>
        </div>
        <p v-else class="rounded-lg border border-dashed border-surface-300 px-4 py-8 text-center text-sm text-surface-400 dark:border-surface-700">
          {{ t('inventory.dashboard.no_movements') }}
        </p>
      </AppCard>
    </template>
  </ListLayout>
</template>
