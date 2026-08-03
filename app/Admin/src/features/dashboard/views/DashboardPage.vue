<script setup lang="ts">
import { onMounted, computed } from 'vue'
import Card from 'primevue/card'
import { useRouter } from 'vue-router'
import { useDashboard } from '../composables/useDashboard'
import { useActiveUsers } from '@/features/identity/composables/useActiveUsers'

const router = useRouter()
const { summary, fetchDashboard } = useDashboard()
const { items: activeUsers, load: loadActiveUsers } = useActiveUsers()

const metrics = computed(() => [
  {
    label: 'Total Products',
    // Compute: Default to zero until the dashboard summary arrives
    value: summary.value?.catalog.totalProducts ?? 0,
    icon: 'pi pi-box',
    color: 'border-t-blue-500',
    link: '/catalog/products',
  },
  {
    label: 'Orders Today',
    value: summary.value?.sales.orderCount ?? 0,
    icon: 'pi pi-shopping-cart',
    color: 'border-t-green-500',
    link: '/ordering/orders',
  },
  {
    label: 'Registered Users',
    value: activeUsers.value.length,
    icon: 'pi pi-users',
    color: 'border-t-purple-500',
    link: '/identity/users',
  },
  {
    label: 'Low Stock Items',
    value: summary.value?.inventory.lowStockCount ?? 0,
    icon: 'pi pi-exclamation-triangle',
    color: 'border-t-orange-500',
    link: '/inventory/stock-items',
  },
])

function navigateTo(path: string) {
  router.push(path)
}

onMounted(async () => {
  // Await: Summary and user count load in parallel on first paint
  await Promise.all([fetchDashboard(), loadActiveUsers()])
})
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Section: Page Header — Store-overview title and subtitle -->
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Dashboard</h1>
      <p class="text-muted-color">Overview of your store at a glance</p>
    </div>
    <!-- Section: KPI Cards — Clickable stat cards linking to each module -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <Card
        v-for="metric in metrics"
        :key="metric.label"
        :class="`cursor-pointer hover:shadow-lg transition-shadow ${metric.color}`"
        :pt="{ root: { class: 'border-t-4' } }"
        @click="navigateTo(metric.link)"
      >
        <template #content>
          <div class="flex items-start justify-between">
            <div>
              <p class="text-sm text-muted-color mb-1">{{ metric.label }}</p>
              <p class="text-3xl font-bold">{{ metric.value.toLocaleString() }}</p>
            </div>
            <i :class="`${metric.icon} text-3xl text-muted-color`" />
          </div>
          <div class="mt-3 text-sm font-medium text-primary">
            View all <i class="pi pi-arrow-right ml-1 text-xs" />
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>
