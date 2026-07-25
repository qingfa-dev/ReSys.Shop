<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import Column from 'primevue/column'
import DataTable from '@/shared/components/data/DataTable.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import StatusTag from '@/shared/components/data/StatusTag.vue'
import Button from 'primevue/button'
import { useToast } from '@/shared/composables/useToast'
import { OrderApi } from '../api'
import type { OrderResponse } from '../types'
import { ROUTE } from '../routes'

const router = useRouter()
const toast = useToast()

const items = ref<OrderResponse[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

async function load() {
  loading.value = true
  error.value = null
  try {
    const result = await OrderApi.getMany({ page: 1, pageSize: 50 })
    if (result.isSuccess) {
      items.value = (result.items ?? []).filter(o => o.status === 'approved' || o.status === 'processing')
    } else {
      error.value = result.message ?? 'Failed to load'
      items.value = []
    }
  } catch (err) {
    console.error(err)
    error.value = 'Failed to load'
    items.value = []
  }
  loading.value = false
}

async function markComplete(id: string) {
  const result = await OrderApi.complete(id)
  if (result.isSuccess) {
    toast.success('Order completed')
    await load()
  } else {
    toast.error(result.message ?? 'Failed to complete')
  }
}

function goToView(id: string) {
  router.push({ name: ROUTE.ORDERS.VIEW, params: { id } })
}

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount)
}

onMounted(() => load())
</script>

<template>
  <div>
    <PageHeader title="Fulfillment Queue" />
    <LoadingSkeleton v-if="loading && items.length === 0" :rows="5" :columns="5" />
    <ErrorState v-else-if="error" :description="error" @retry="load" />
    <EmptyState v-else-if="items.length === 0" title="No orders to fulfill" description="All orders have been fulfilled." />
    <DataTable v-else :rows="[...items]" :loading="loading" :total-records="items.length">
      <Column field="orderNumber" header="Order #" sortable />
      <Column field="customerName" header="Customer" />
      <Column field="status" header="Status">
        <template #body="{ data }">
          <StatusTag :status="data.status" />
        </template>
      </Column>
      <Column field="total" header="Total">
        <template #body="{ data }">
          {{ formatCurrency(data.total) }}
        </template>
      </Column>
      <template #rowActions="{ data }">
        <div class="flex gap-2">
          <Button label="View" icon="pi pi-eye" severity="secondary" text @click="goToView(data.id)" />
          <Button v-if="data.status !== 'completed'" label="Complete" icon="pi pi-check-circle" severity="success" text @click="markComplete(data.id)" />
        </div>
      </template>
    </DataTable>
  </div>
</template>
