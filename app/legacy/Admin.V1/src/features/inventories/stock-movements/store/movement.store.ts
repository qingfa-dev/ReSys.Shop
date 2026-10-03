import { defineStore } from 'pinia'
import { ref } from 'vue'
import { movementRepository } from '../api/movement.api'
import type { StockMovement } from '../types/stock-movement.response'

export const useMovementStore = defineStore('movement', () => {
  const current = ref<StockMovement | null>(null)
  const loading = ref(false)

  async function fetchById(id: string) {
    loading.value = true
    try {
      const result = await movementRepository.getById(id)
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
