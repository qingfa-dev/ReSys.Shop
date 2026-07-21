import { defineStore } from 'pinia'
import { ref } from 'vue'
import { permissionRepository } from '../api/permission.api'
import type { PermissionSummary } from '../types/permission.response'

export const usePermissionStore = defineStore('permission', () => {
  const items = ref<PermissionSummary[]>([])
  const loading = ref(false)

  async function fetchAll() {
    loading.value = true
    const result = await permissionRepository.list()
    if (result.isSuccess) items.value = result.value
    loading.value = false
    return result
  }

  return { items, loading, fetchAll }
})
