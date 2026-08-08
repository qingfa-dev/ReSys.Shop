import { defineStore } from 'pinia'
import { ref } from 'vue'
import { ProfileApi } from '../services/profileApi'
import { AccountApi } from '../services/accountApi'
import { useAuthStore } from '@/features/identity/stores/authStore'
import type { ProfileDetail, UpdateProfileRequest } from '../types'

export const useProfileStore = defineStore('profile', () => {
  const profile = ref<ProfileDetail | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)
  const _initialized = ref(false)

  async function init(): Promise<void> {
    if (_initialized.value) return
    _initialized.value = true
    await fetchProfile()
  }

  async function fetchProfile(): Promise<void> {
    loading.value = true
    error.value = null
    const result = await ProfileApi.getProfile()
    if (result.isSuccess) profile.value = result.value
    else error.value = result.message
    loading.value = false
  }

  async function updateProfile(req: UpdateProfileRequest): Promise<boolean> {
    saving.value = true
    error.value = null
    const prev = profile.value
    if (prev) Object.assign(prev, req)
    const result = await ProfileApi.updateProfile(req)
    if (!result.isSuccess) {
      error.value = result.message
      profile.value = prev
    }
    saving.value = false
    return result.isSuccess
  }

  async function deleteProfile(): Promise<boolean> {
    saving.value = true
    const result = await AccountApi.deleteProfile()
    if (result.isSuccess) {
      profile.value = null
      await useAuthStore().logout()
    } else {
      error.value = result.message
    }
    saving.value = false
    return result.isSuccess
  }

  function reset(): void {
    profile.value = null
    error.value = null
    _initialized.value = false
  }

  return { profile, loading, saving, error, init, fetchProfile, updateProfile, deleteProfile, reset }
})
