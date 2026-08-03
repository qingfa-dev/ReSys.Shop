import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { RoleListItem } from '../types/role'
import { RoleApi } from '../services/roleApi'

export const useRoleStore = defineStore('roles', () => {
  const activeRoles = ref<RoleListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return
    const result = await RoleApi.getRoles({})
    if (result.isSuccess) {
      activeRoles.value = result.items
      loaded.value = true
    }
  }

  return { activeRoles, loaded, fetchActive }
})
