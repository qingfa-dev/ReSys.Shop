import { defineStore } from 'pinia'
import { ref } from 'vue'
import { reservationRepository } from '../api/reservation.api'
import type { InventoryUnit } from '../types/inventory-unit.response'
import type { InventoryUnitQuery } from '../types/inventory-unit.query'

export const useReservationStore = defineStore('reservation', () => {
  const items = ref<InventoryUnit[]>([])
  const loading = ref(false)
  const totalRecords = ref(0)
  const query = ref<InventoryUnitQuery>({ page: 1, pageSize: 20, sort: ['-createdAtUtc'] })

  async function fetchItems(params: InventoryUnitQuery = {}) {
    loading.value = true
    query.value = { ...query.value, ...params }
    const result = await reservationRepository.list(query.value)
    if (result.isSuccess) { items.value = result.items; totalRecords.value = result.totalCount || 0 }
    loading.value = false
    return result
  }

  return { items, loading, totalRecords, query, fetchItems }
})
