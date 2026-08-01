import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { ProfileListItem } from '../types/profile'
import { ProfileApi } from '../services/profileApi'

export const useProfileStore = defineStore('profiles', () => {
  const activeProfiles = ref<ProfileListItem[]>([])
  const loaded = ref(false)

  async function fetchActive(): Promise<void> {
    if (loaded.value) return
    const result = await ProfileApi.getProfiles({ pageSize: 100, sortBy: 'firstName', sortDirection: 'asc' })
    if (result.isSuccess) {
      activeProfiles.value = result.items
      loaded.value = true
    }
  }

  return { activeProfiles, loaded, fetchActive }
})
