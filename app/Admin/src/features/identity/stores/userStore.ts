import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { UserListItem } from '../types/user'
import { UserApi } from '../services/userApi'

export const useUserStore = defineStore('users', () => {
  const activeUsers = ref<UserListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return
    const result = await UserApi.getUsers({})
    if (result.isSuccess) {
      activeUsers.value = result.items
      loaded.value = true
    }
  }

  return { activeUsers, loaded, fetchActive }
})
