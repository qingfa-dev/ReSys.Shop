import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as profileApi from '../services/profileApi'
import type { ProfileDetail, UpdateProfileRequest } from '../types/profile'

export const useProfileStore = defineStore('profile', () => {
  const profile = ref<ProfileDetail | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)

  async function fetchProfile(): Promise<boolean> {
    loading.value = true
    error.value = null
    const result = await profileApi.getProfile()
    loading.value = false
    if (result.isSuccess) {
      profile.value = result.value
      return true
    }
    error.value = result.message ?? result.errors[0]?.message ?? 'Failed to load profile'
    return false
  }

  async function updateProfile(req: UpdateProfileRequest): Promise<boolean> {
    saving.value = true
    error.value = null
    const result = await profileApi.updateProfile(req)
    saving.value = false
    if (result.isSuccess) {
      profile.value = result.value
      return true
    }
    error.value = result.message ?? result.errors[0]?.message ?? 'Failed to update profile'
    return false
  }

  return { profile, loading, saving, error, fetchProfile, updateProfile }
})
