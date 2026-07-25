<script setup lang="ts">
import { ref, onMounted } from 'vue'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import StatusTag from '@/shared/components/data/StatusTag.vue'
import { useRouter } from 'vue-router'
import { OrderingDashboardApi } from '../api'
import type { OrderingDashboardResponse } from '../types'
import { ROUTE } from '../routes'

const router = useRouter()
const data = ref<OrderingDashboardResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

async function load() {
  loading.value = true
  error.value = null
  const result = await OrderingDashboardApi.get()
  if (result.isSuccess) {
    data.value = result.value
  } else {
    error.value = result.message ?? 'Failed to load dashboard'
  }
  loading.value = false
}

function goToOrder(id: string) {
  router.push({ name: ROUTE.ORDERS.VIEW, params: { id } })
}

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount)
}

onMounted(() => load())
</script>

<template>
  <div>
    <PageHeader title="Orders Dashboard" />
    <div v-if="loading" class="grid">
      <div v-for="i in 6" :key="i" class="col-12 md:col-6 lg:col-4">
        <div class="card p-3 surface-50 border-round" style="height:100px">
          <div class="text-sm text-surface-500 mb-2">Loading...</div>
        </div>
      </div>
    </div>
    <div v-else-if="error" class="card p-4">
      <p class="text-red-500">{{ error }}</p>
      <button class="p-button p-component mt-3" @click="load">Retry</button>
    </div>
    <div v-else-if="data" class="grid">
      <div class="col-12 md:col-6 lg:col-4">
        <div class="card p-3 border-round">
          <div class="text-sm text-surface-500">Total Orders</div>
          <div class="text-2xl font-semibold">{{ data.totalOrders }}</div>
        </div>
      </div>
      <div class="col-12 md:col-6 lg:col-4">
        <div class="card p-3 border-round">
          <div class="text-sm text-surface-500">Pending</div>
          <div class="text-2xl font-semibold text-orange-500">{{ data.pendingOrders }}</div>
        </div>
      </div>
      <div class="col-12 md:col-6 lg:col-4">
        <div class="card p-3 border-round">
          <div class="text-sm text-surface-500">Completed</div>
          <div class="text-2xl font-semibold text-green-500">{{ data.completedOrders }}</div>
        </div>
      </div>
      <div class="col-12 md:col-6 lg:col-4">
        <div class="card p-3 border-round">
          <div class="text-sm text-surface-500">Cancelled</div>
          <div class="text-2xl font-semibold text-red-500">{{ data.cancelledOrders }}</div>
        </div>
      </div>
      <div class="col-12 md:col-6 lg:col-4">
        <div class="card p-3 border-round">
          <div class="text-sm text-surface-500">Total Revenue</div>
          <div class="text-2xl font-semibold text-blue-500">{{ formatCurrency(data.totalRevenue) }}</div>
        </div>
      </div>
      <div class="col-12 md:col-6 lg:col-4">
        <div class="card p-3 border-round">
          <div class="text-sm text-surface-500">Today's Revenue</div>
          <div class="text-2xl font-semibold text-purple-500">{{ formatCurrency(data.todayRevenue) }}</div>
        </div>
      </div>
      <div class="col-12 mt-4">
        <div class="card">
          <h3 class="text-lg font-semibold mb-3">Recent Orders</h3>
          <table v-if="data.recentOrders.length > 0" class="w-full">
            <thead>
              <tr class="text-left text-sm text-surface-500">
                <th class="pb-2">Order #</th>
                <th class="pb-2">Customer</th>
                <th class="pb-2">Status</th>
                <th class="pb-2">Total</th>
                <th class="pb-2">Date</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="o in data.recentOrders" :key="o.id" class="border-top border-surface-200 cursor-pointer" @click="goToOrder(o.id)">
                <td class="py-2">{{ o.orderNumber }}</td>
                <td class="py-2">{{ o.customerName || o.customerEmail }}</td>
                <td class="py-2"><StatusTag :status="o.status" /></td>
                <td class="py-2">{{ formatCurrency(o.total) }}</td>
                <td class="py-2">{{ o.createdAt }}</td>
              </tr>
            </tbody>
          </table>
          <p v-else class="text-surface-500 text-sm">No recent orders</p>
        </div>
      </div>
    </div>
  </div>
</template>
