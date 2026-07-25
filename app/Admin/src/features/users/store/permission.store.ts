import { ref, readonly } from 'vue'
import { defineStore } from 'pinia'
import type { PermissionResponse } from '../types'
import { PermissionApi } from '../api'

export const usePermissionStore = defineStore('identity-permission', () => {
  const items = ref<PermissionResponse[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchMany() {
    loading.value = true
    error.value = null
    try {
      const result = await PermissionApi.getMany()
      if (result.isSuccess) {
        items.value = result.value ?? []
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

  return {
    items: readonly(items), loading: readonly(loading),
    error: readonly(error),
    fetchMany,
  }
})
