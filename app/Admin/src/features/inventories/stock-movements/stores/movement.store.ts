import { defineStore } from 'pinia'
import { ref } from 'vue'
import { movementService } from '../services/movement.service'
import type { StockMovement } from '../types/stock-movement.response.type'

export const useMovementStore = defineStore('movement', () => {
  const current = ref<StockMovement | null>(null)
  const loading = ref(false)

  async function fetchById(id: string) {
    loading.value = true
    try {
      const result = await movementService.getMovementDetail(id)
      if (result.isSuccess && result.value) {
        current.value = result.value
      }
      return result
    } finally {
      loading.value = false
    }
  }

  return {
    current,
    loading,
    fetchById,
  }
})
