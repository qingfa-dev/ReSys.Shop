import { ref, readonly } from 'vue'
import { defineStore } from 'pinia'
import type { ProfileResponse } from '../types'
import { ProfileApi } from '../api'

export const useProfileStore = defineStore('profile', () => {
  const profile = ref<ProfileResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchProfile() {
    loading.value = true
    error.value = null
    try {
      const result = await ProfileApi.get()
      if (result.isSuccess) {
        profile.value = result.value
      } else {
        error.value = result.message ?? 'Failed to load profile'
        profile.value = null
      }
    } catch (err) {
      console.error(err)
      error.value = 'Failed to load profile'
      profile.value = null
    }
    loading.value = false
  }

  return { profile: readonly(profile), loading: readonly(loading), error: readonly(error), fetchProfile }
})
