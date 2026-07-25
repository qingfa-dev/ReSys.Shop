<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter, useRoute } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { StatCard, LoadingSkeleton, ErrorState, ListLayout, AppCard } from '@/shared/components'
import Button from 'primevue/button'
import { OrderingDashboardApi } from '../api'
import type { OrderingDashboardResponse } from '../types'
import { ROUTE } from '../routes'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()

const data = ref<OrderingDashboardResponse | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

const metrics = computed(() => [
  { label: t('ordering.dashboard.total_orders'), value: data.value?.totalOrders ?? 0, icon: 'pi pi-shopping-cart', color: 'primary' as const },
  { label: t('ordering.dashboard.pending'), value: data.value?.pendingOrders ?? 0, icon: 'pi pi-clock', color: 'orange' as const },
  { label: t('ordering.dashboard.completed'), value: data.value?.completedOrders ?? 0, icon: 'pi pi-check-circle', color: 'green' as const },
  { label: t('ordering.dashboard.cancelled'), value: data.value?.cancelledOrders ?? 0, icon: 'pi pi-times-circle', color: 'red' as const },
  { label: t('ordering.dashboard.total_revenue'), value: formatCurrency(data.value?.totalRevenue ?? 0), icon: 'pi pi-dollar', color: 'blue' as const },
  { label: t('ordering.dashboard.today_revenue'), value: formatCurrency(data.value?.todayRevenue ?? 0), icon: 'pi pi-chart-line', color: 'green' as const },
])

const recentOrders = computed(() =>
  data.value?.recentOrders.map(o => ({
    id: o.id,
    orderNumber: o.orderNumber,
    customer: o.customerName || o.customerEmail,
    status: o.status,
    total: formatCurrency(o.total),
    date: o.createdAt,
  })) ?? [],
)

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount)
}

function goToOrder(id: string) {
  router.push({ name: ROUTE.ORDERS.VIEW, params: { id } })
}

async function fetchDashboard() {
  loading.value = true
  error.value = null
  try {
    const result = await OrderingDashboardApi.get()
    if (result.isSuccess) {
      data.value = result.value
    } else {
      error.value = result.message ?? t('ordering.dashboard.messages.load_failed')
    }
  } catch (err) {
    console.error(err)
    error.value = t('ordering.dashboard.messages.load_failed')
  }
  loading.value = false
}

onMounted(fetchDashboard)
</script>

<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('ordering.dashboard.title')"
        :subtitle="t('ordering.dashboard.subtitle')"
        :icon="route.meta?.icon as string | undefined"
      >
        <template #actions>
          <Button
            :label="t('ordering.orders.actions.create')"
            icon="pi pi-plus"
            size="small"
            @click="router.push({ name: ROUTE.ORDERS.CREATE })"
          />
        </template>
      </PageHeader>
    </template>

    <LoadingSkeleton v-if="loading" :rows="4" :columns="3" />

    <ErrorState
      v-else-if="error"
      :title="error"
      @retry="fetchDashboard"
    />

    <template v-else>
      <div class="grid grid-cols-2 gap-4 lg:grid-cols-3">
        <StatCard v-for="m in metrics" :key="m.label" :label="m.label" :value="m.value" :icon="m.icon" :color="m.color" />
      </div>

      <AppCard>
        <p class="mb-3 text-xs font-semibold uppercase tracking-wider text-surface-400">
          {{ t('ordering.dashboard.recent_orders') }}
        </p>
        <div v-if="recentOrders.length" class="divide-y divide-surface-200 dark:divide-surface-700">
          <div
            v-for="o in recentOrders"
            :key="o.id"
            class="flex cursor-pointer items-center justify-between px-2 py-3 transition-colors hover:bg-surface-50 dark:hover:bg-surface-800"
            @click="goToOrder(o.id)"
          >
            <div>
              <p class="text-sm font-medium text-surface-900 dark:text-surface-0">{{ o.orderNumber }}</p>
              <p class="text-xs text-surface-400">{{ o.customer }}</p>
            </div>
            <div class="flex items-center gap-4">
              <span class="text-sm font-medium">{{ o.total }}</span>
              <span class="text-xs text-surface-400">{{ o.date }}</span>
            </div>
          </div>
        </div>
        <p v-else class="rounded-lg border border-dashed border-surface-300 px-4 py-8 text-center text-sm text-surface-400 dark:border-surface-700">
          {{ t('ordering.dashboard.no_recent_orders') }}
        </p>
      </AppCard>
    </template>
  </ListLayout>
</template>
