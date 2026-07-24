<script setup lang="ts">
import { ref, onMounted } from 'vue'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import { InventoryDashboardApi } from '../api'
import type { InventoryDashboardResponse } from '../types'

const data = ref<InventoryDashboardResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

async function load() {
  loading.value = true
  error.value = null
  const result = await InventoryDashboardApi.get()
  if (result.isSuccess) {
    data.value = result.value
  } else {
    error.value = result.message ?? 'Failed to load dashboard'
  }
  loading.value = false
}

onMounted(() => load())
</script>

<template>
  <div>
    <PageHeader title="Inventory Dashboard" />
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
          <div class="text-sm text-surface-500">Stock Items</div>
          <div class="text-2xl font-semibold">{{ data.totalStockItems }}</div>
        </div>
      </div>
      <div class="col-12 md:col-6 lg:col-4">
        <div class="card p-3 border-round">
          <div class="text-sm text-surface-500">Locations</div>
          <div class="text-2xl font-semibold">{{ data.totalLocations }}</div>
        </div>
      </div>
      <div class="col-12 md:col-6 lg:col-4">
        <div class="card p-3 border-round">
          <div class="text-sm text-surface-500">Low Stock Items</div>
          <div class="text-2xl font-semibold text-orange-500">{{ data.lowStockCount }}</div>
        </div>
      </div>
      <div class="col-12 md:col-6 lg:col-4">
        <div class="card p-3 border-round">
          <div class="text-sm text-surface-500">Out of Stock</div>
          <div class="text-2xl font-semibold text-red-500">{{ data.outOfStockCount }}</div>
        </div>
      </div>
      <div class="col-12 md:col-6 lg:col-4">
        <div class="card p-3 border-round">
          <div class="text-sm text-surface-500">Reserved Quantity</div>
          <div class="text-2xl font-semibold text-blue-500">{{ data.totalReservedQuantity }}</div>
        </div>
      </div>
      <div class="col-12 md:col-6 lg:col-4">
        <div class="card p-3 border-round">
          <div class="text-sm text-surface-500">Pending Transfers</div>
          <div class="text-2xl font-semibold text-purple-500">{{ data.totalTransfersPending }}</div>
        </div>
      </div>
      <div class="col-12 mt-4">
        <div class="card">
          <h3 class="text-lg font-semibold mb-3">Recent Movements</h3>
          <table v-if="data.recentMovements.length > 0" class="w-full">
            <thead>
              <tr class="text-left text-sm text-surface-500">
                <th class="pb-2">SKU</th>
                <th class="pb-2">Location</th>
                <th class="pb-2">Qty</th>
                <th class="pb-2">Direction</th>
                <th class="pb-2">Date</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="m in data.recentMovements" :key="m.id" class="border-top border-surface-200">
                <td class="py-2">{{ m.variantSku }}</td>
                <td class="py-2">{{ m.locationName }}</td>
                <td class="py-2">{{ m.quantity }}</td>
                <td class="py-2">
                  <span :class="m.direction === 'In' ? 'text-green-600' : 'text-red-600'">{{ m.direction }}</span>
                </td>
                <td class="py-2">{{ m.createdAt }}</td>
              </tr>
            </tbody>
          </table>
          <p v-else class="text-surface-500 text-sm">No recent movements</p>
        </div>
      </div>
    </div>
  </div>
</template>
