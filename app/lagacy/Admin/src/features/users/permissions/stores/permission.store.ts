import { defineStore } from 'pinia'
import { ref } from 'vue'
import { permissionService } from '../services/permission.service'
import type { PermissionSummary } from '../types/permission.response.type'

export const usePermissionStore = defineStore('permission', () => {
  const items = ref<PermissionSummary[]>([])
  const loading = ref(false)

  async function fetchAll() {
    loading.value = true
    const result = await permissionService.list()
    if (result.isSuccess) items.value = result.value
    loading.value = false
    return result
  }

  return { items, loading, fetchAll }
})
