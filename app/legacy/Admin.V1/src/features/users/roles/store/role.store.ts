import { defineStore } from 'pinia'
import { ref } from 'vue'
import { roleRepository } from '../api/role.api'
import type { RoleSummary } from '../types/role.response'
import type { ServerQueryingParameters } from '@/common/api/types/query.types'

export const useRoleStore = defineStore('role', () => {
  const items = ref<RoleSummary[]>([])
  const loading = ref(false)
  const totalRecords = ref(0)
  const query = ref<ServerQueryingParameters>({ page: 1, pageSize: 20, sort: ['name'] })

  async function fetchItems(params?: ServerQueryingParameters) {
    loading.value = true
    query.value = { ...query.value, ...params }
    const result = await roleRepository.list(query.value)
    if (result.isSuccess) { items.value = result.items; totalRecords.value = result.totalCount }
    loading.value = false
    return result
  }

  return { items, loading, totalRecords, query, fetchItems }
})
