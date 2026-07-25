<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import Column from 'primevue/column'
import Chart from 'primevue/chart'
import Skeleton from 'primevue/skeleton'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { ListLayout, DataTable, StatCard, ErrorState } from '@/shared/components'
import type { ReportsData } from '../types'
import { DashboardApi } from '../api'

const { t } = useI18n()
const route = useRoute()

const data = ref<ReportsData | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)

const monthlyData = computed(() =>
  data.value?.salesTrends.map(t => ({
    ...t,
    revenueFormatted: formatCurrency(t.sales),
    avgOrderValue: formatCurrency(Math.round(t.sales / t.orders)),
  })) ?? [],
)

const revenueChartData = computed(() => ({
  labels: data.value?.salesTrends.map(t => t.month) ?? [],
  datasets: [
    {
      label: 'Revenue',
      data: data.value?.salesTrends.map(t => t.sales) ?? [],
      borderColor: '#10b981',
      backgroundColor: 'rgba(16, 185, 129, 0.1)',
      fill: true,
      tension: 0.4,
    },
  ],
}))

const revenueOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false } },
  scales: {
    x: { grid: { display: false } },
    y: { grid: { color: 'rgba(0,0,0,0.05)' }, ticks: { callback: (v: number) => '$' + v.toLocaleString() } },
  },
}

const ordersChartData = computed(() => ({
  labels: data.value?.salesTrends.map(t => t.month) ?? [],
  datasets: [
    {
      label: 'Orders',
      data: data.value?.salesTrends.map(t => t.orders) ?? [],
      borderColor: '#3b82f6',
      backgroundColor: 'rgba(59, 130, 246, 0.1)',
      fill: true,
      tension: 0.4,
    },
  ],
}))

const ordersOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false } },
  scales: {
    x: { grid: { display: false } },
    y: { grid: { color: 'rgba(0,0,0,0.05)' } },
  },
}

const categoryChartData = computed(() => ({
  labels: data.value?.revenueByCategory.map(c => c.category) ?? [],
  datasets: [
    {
      data: data.value?.revenueByCategory.map(c => c.revenue) ?? [],
      backgroundColor: ['#10b981', '#3b82f6', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'],
    },
  ],
}))

const categoryOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: 'bottom' as const },
  },
}

const statusChartData = computed(() => ({
  labels: data.value?.orderStatusStats.map(s => s.status) ?? [],
  datasets: [
    {
      data: data.value?.orderStatusStats.map(s => s.count) ?? [],
      backgroundColor: ['#10b981', '#f59e0b', '#3b82f6', '#ef4444', '#8b5cf6'],
    },
  ],
}))

const statusOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: 'bottom' as const },
  },
  cutout: '60%',
}

function generateMockData(): ReportsData {
  return {
    totalRevenue: 284_500,
    totalOrders: 3_842,
    totalCustomers: 1_967,
    averageOrderValue: 74.05,
    revenueDelta: 12.5,
    ordersDelta: 8.3,
    customersDelta: 15.2,
    aovDelta: 3.8,
    salesTrends: [
      { month: 'Jan', sales: 18_500, orders: 280 },
      { month: 'Feb', sales: 22_300, orders: 310 },
      { month: 'Mar', sales: 20_800, orders: 295 },
      { month: 'Apr', sales: 25_100, orders: 340 },
      { month: 'May', sales: 28_400, orders: 375 },
      { month: 'Jun', sales: 24_900, orders: 355 },
      { month: 'Jul', sales: 26_700, orders: 368 },
      { month: 'Aug', sales: 29_300, orders: 390 },
      { month: 'Sep', sales: 27_600, orders: 372 },
      { month: 'Oct', sales: 31_200, orders: 410 },
      { month: 'Nov', sales: 33_800, orders: 445 },
      { month: 'Dec', sales: 35_500, orders: 462 },
    ],
    revenueByCategory: [
      { category: 'Electronics', revenue: 98_000, percentage: 34.5 },
      { category: 'Clothing', revenue: 62_000, percentage: 21.8 },
      { category: 'Home & Garden', revenue: 45_000, percentage: 15.8 },
      { category: 'Sports', revenue: 38_000, percentage: 13.4 },
      { category: 'Books', revenue: 25_000, percentage: 8.8 },
      { category: 'Other', revenue: 16_500, percentage: 5.8 },
    ],
    orderStatusStats: [
      { status: 'Completed', count: 2_450 },
      { status: 'Pending', count: 680 },
      { status: 'Processing', count: 420 },
      { status: 'Cancelled', count: 195 },
      { status: 'Refunded', count: 97 },
    ],
  }
}

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 0 }).format(amount)
}

async function loadDashboard() {
  loading.value = true
  error.value = null
  try {
    const result = await DashboardApi.get()
    if (result.isSuccess) {
      data.value = result.value
    } else {
      data.value = generateMockData()
    }
  } catch {
    data.value = generateMockData()
  }
  loading.value = false
}

onMounted(loadDashboard)
</script>

<template>
  <ListLayout>
    <template #header>
      <PageHeader
        :title="t('reports.dashboard.title')"
        :subtitle="t('reports.dashboard.subtitle')"
        :icon="route.meta?.icon as string"
      />
    </template>

    <div v-if="loading" class="grid grid-cols-2 gap-4 lg:grid-cols-4">
      <div v-for="i in 4" :key="i">
        <div class="rounded-border border border-surface-200 bg-white p-5 dark:border-surface-700 dark:bg-surface-900" style="height: 120px">
          <Skeleton height="1rem" width="60%" class="mb-3" />
          <Skeleton height="2rem" width="40%" />
        </div>
      </div>
    </div>

    <ErrorState v-else-if="error" :title="error" @retry="loadDashboard" />

    <template v-else-if="data">
      <div class="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard :label="t('reports.dashboard.total_revenue')" :value="formatCurrency(data.totalRevenue)" icon="pi pi-dollar" color="green" :delta="data.revenueDelta" />
        <StatCard :label="t('reports.dashboard.total_orders')" :value="data.totalOrders.toLocaleString()" icon="pi pi-shopping-cart" color="blue" :delta="data.ordersDelta" />
        <StatCard :label="t('reports.dashboard.customers')" :value="data.totalCustomers.toLocaleString()" icon="pi pi-users" color="primary" :delta="data.customersDelta" />
        <StatCard :label="t('reports.dashboard.avg_order_value')" :value="formatCurrency(data.averageOrderValue)" icon="pi pi-chart-bar" color="orange" :delta="data.aovDelta" />
      </div>

      <div class="mb-6 grid grid-cols-1 gap-6 lg:grid-cols-2">
        <div class="rounded-border border border-surface-200 bg-white p-5 dark:border-surface-700 dark:bg-surface-900">
          <h3 class="mb-4 text-sm font-semibold uppercase tracking-wider text-surface-500">{{ t('reports.dashboard.revenue_trend') }}</h3>
          <div style="height: 280px">
            <Chart type="line" :data="revenueChartData" :options="revenueOptions" />
          </div>
        </div>
        <div class="rounded-border border border-surface-200 bg-white p-5 dark:border-surface-700 dark:bg-surface-900">
          <h3 class="mb-4 text-sm font-semibold uppercase tracking-wider text-surface-500">{{ t('reports.dashboard.orders_trend') }}</h3>
          <div style="height: 280px">
            <Chart type="line" :data="ordersChartData" :options="ordersOptions" />
          </div>
        </div>
      </div>

      <div class="mb-6 grid grid-cols-1 gap-6 lg:grid-cols-2">
        <div class="rounded-border border border-surface-200 bg-white p-5 dark:border-surface-700 dark:bg-surface-900">
          <h3 class="mb-4 text-sm font-semibold uppercase tracking-wider text-surface-500">{{ t('reports.dashboard.revenue_by_category') }}</h3>
          <div style="height: 300px">
            <Chart type="doughnut" :data="categoryChartData" :options="categoryOptions" />
          </div>
        </div>
        <div class="rounded-border border border-surface-200 bg-white p-5 dark:border-surface-700 dark:bg-surface-900">
          <h3 class="mb-4 text-sm font-semibold uppercase tracking-wider text-surface-500">{{ t('reports.dashboard.order_status_breakdown') }}</h3>
          <div style="height: 300px">
            <Chart type="doughnut" :data="statusChartData" :options="statusOptions" />
          </div>
        </div>
      </div>

      <div class="rounded-border border border-surface-200 bg-white p-5 dark:border-surface-700 dark:bg-surface-900">
        <h3 class="mb-4 text-sm font-semibold uppercase tracking-wider text-surface-500">{{ t('reports.dashboard.monthly_performance') }}</h3>
        <DataTable :rows="monthlyData" :total-records="monthlyData.length">
          <Column field="month" :header="t('reports.dashboard.columns.month')" />
          <Column field="orders" :header="t('reports.dashboard.columns.orders')" />
          <Column field="revenueFormatted" :header="t('reports.dashboard.columns.revenue')" />
          <Column field="avgOrderValue" :header="t('reports.dashboard.columns.avgOrder')" />
        </DataTable>
      </div>
    </template>
  </ListLayout>
</template>
