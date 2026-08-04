import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as profileApi from '../services/profileApi'
import type { ProfileDetail, UpdateProfileRequest } from '../types/profile'

export const useProfileStore = defineStore('profile', () => {
  // Starts true so the view shows the skeleton on first paint (a false initial value would
  // render the empty form before the fetch resolves).
  const loading = ref(true)
  const profile = ref<ProfileDetail | null>(null)
  const saving = ref(false)
  const error = ref<string | null>(null)

  async function fetchProfile(): Promise<boolean> {
    loading.value = true
    error.value = null
    try {
      const result = await profileApi.getProfile()
      if (result.isSuccess) {
        profile.value = result.value
        return true
      }
      error.value = result.message ?? result.errors[0]?.message ?? 'Failed to load profile'
      return false
    } catch {
      // The error interceptor throws HttpError on network failures / non-Result 5xx.
      error.value = 'Failed to load profile'
      return false
    } finally {
      loading.value = false
    }
  }

  async function updateProfile(req: UpdateProfileRequest): Promise<boolean> {
    saving.value = true
    error.value = null
    try {
      const result = await profileApi.updateProfile(req)
      if (result.isSuccess) {
        profile.value = result.value
        return true
      }
      error.value = result.message ?? result.errors[0]?.message ?? 'Failed to update profile'
      return false
    } catch {
      error.value = 'Failed to update profile'
      return false
    } finally {
      saving.value = false
    }
  }

  return { profile, loading, saving, error, fetchProfile, updateProfile }
})
