import { defineStore } from 'pinia'
import { ref } from 'vue'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { ENDPOINTS } from '@/shared/constants/api'
import * as orderApi from '../services/orderApi'
import type { OrderListItem, OrderDetail, OrderStatus } from '../types/order'
import { ORDER_FILTER_FIELDS, ORDER_SORT_FIELDS, ORDER_SEARCH_FIELDS } from '../types/order'

export type OrderStatusFilter = OrderStatus | 'All'

export const useOrderStore = defineStore('orders', () => {
  // Paged order list state (same usePagedQuery pattern as ShopView). immediate:false so
  // the list only loads when the Orders view mounts — the detail route must not trigger it.
  const paged = usePagedQuery<OrderListItem>(ENDPOINTS.orders, {
    defaultPageSize: 10,
    defaultSort: ['-createdAtUtc'],
    allowedFilterFields: ORDER_FILTER_FIELDS,
    allowedSortFields: ORDER_SORT_FIELDS,
    allowedSearchFields: ORDER_SEARCH_FIELDS,
    immediate: false,
  })

  const currentOrder = ref<OrderDetail | null>(null)
  const detailLoading = ref(false)
  const cancelLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchOrders(): Promise<void> {
    await paged.fetch()
  }

  function setStatusFilter(status: OrderStatusFilter): void {
    paged.setFilter(status === 'All' ? '' : `status=${status}`)
  }

  async function fetchOrder(id: string): Promise<boolean> {
    detailLoading.value = true
    error.value = null
    const result = await orderApi.getOrder(id)
    detailLoading.value = false
    if (result.isSuccess) {
      currentOrder.value = result.value
      return true
    }
    error.value = result.message ?? 'Failed to load order'
    return false
  }

  async function cancelOrder(id: string): Promise<boolean> {
    cancelLoading.value = true
    error.value = null
    const result = await orderApi.cancelOrder(id)
    cancelLoading.value = false
    if (result.isSuccess) {
      // Reflect the terminal state locally without a refetch of the detail.
      if (currentOrder.value && currentOrder.value.id === id) {
        currentOrder.value = { ...currentOrder.value, status: 'Canceled', canceledAtUtc: new Date().toISOString() }
      }
      // Keep the list in sync (the canceled order may drop out of a status filter).
      await paged.refresh()
      return true
    }
    error.value = result.message ?? 'Failed to cancel order'
    return false
  }

  function resetDetail(): void {
    currentOrder.value = null
    detailLoading.value = false
    error.value = null
  }

  return {
    // Paged list state + pagination actions from usePagedQuery.
    items: paged.items,
    loading: paged.loading,
    listError: paged.error,
    page: paged.page,
    pageSize: paged.pageSize,
    totalCount: paged.totalCount,
    totalPages: paged.totalPages,
    filter: paged.filter,
    sort: paged.sort,
    search: paged.search,
    searchFields: paged.searchFields,
    searchMode: paged.searchMode,
    fetchOrders,
    setStatusFilter,
    setPage: paged.setPage,
    setPageSize: paged.setPageSize,
    setSort: paged.setSort,
    // Detail state + actions.
    currentOrder,
    detailLoading,
    cancelLoading,
    error,
    fetchOrder,
    cancelOrder,
    resetDetail,
  }
})
