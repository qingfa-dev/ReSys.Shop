import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { OrderApi } from '../services/orderApi'
import { on } from '@/shared/composables/useStoreEvents'
import type { OrderListItem, OrderDetail, OrderTrackingResponse, OrderStatus } from '../types'

export const useOrderStore = defineStore('orders', () => {
  const items = ref<OrderListItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const page = ref(1)
  const pageSize = ref(20)
  const totalCount = ref(0)
  const statusFilter = ref<OrderStatus | 'All'>('All')
  const currentOrder = ref<OrderDetail | null>(null)
  const detailLoading = ref(false)
  const cancelLoading = ref(false)

  const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value))

  async function fetchOrders(): Promise<void> {
    if (loading.value) return
    loading.value = true
    error.value = null
    const result = await OrderApi.getOrders({ pageNumber: page.value, pageSize: pageSize.value })
    if (result.isSuccess) {
      items.value = result.items
      totalCount.value = result.totalCount
    } else {
      error.value = result.message ?? 'Failed to load orders'
    }
    loading.value = false
  }

  async function fetchOrder(id: string): Promise<void> {
    detailLoading.value = true
    const [detail] = await Promise.all([OrderApi.getOrder(id), OrderApi.getOrderTracking(id)])
    if (detail.isSuccess) currentOrder.value = detail.value
    else error.value = detail.message
    detailLoading.value = false
  }

  async function cancelOrder(id: string): Promise<boolean> {
    cancelLoading.value = true
    const result = await OrderApi.cancelOrder(id)
    if (result.isSuccess) {
      const item = items.value.find(o => o.id === id)
      if (item) item.status = 'Canceled'
      if (currentOrder.value?.id === id) currentOrder.value.status = 'Canceled'
    } else {
      error.value = result.message
    }
    cancelLoading.value = false
    return result.isSuccess
  }

  function nextPage(): void { if (page.value < totalPages.value) { page.value++; fetchOrders() } }
  function prevPage(): void { if (page.value > 1) { page.value--; fetchOrders() } }
  function refresh(): void { fetchOrders() }

  on('checkout:placed', () => refresh())

  return {
    items, loading, error, page, pageSize, totalCount, totalPages, statusFilter,
    currentOrder, detailLoading, cancelLoading,
    fetchOrders, fetchOrder, cancelOrder, nextPage, prevPage, refresh,
  }
})
