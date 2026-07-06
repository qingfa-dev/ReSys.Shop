import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useToast } from '@/shared/composables/toast.use'
import { profileService } from '../services/profile.service'
import type { Profile } from '../types/profile.types'

export const useProfileStore = defineStore('profile', () => {
  const { showToast } = useToast()
  const profile = ref<Profile | null>(null)
  const loading = ref(false)
  const submitting = ref(false)

  async function fetchProfile() {
    loading.value = true
    const result = await profileService.getProfile()
    if (result.success) {
      profile.value = result.data
    } else {
      showToast('error', 'Error', result.error?.detail || 'Failed to load profile')
    }
    loading.value = false
    return result
  }

  async function updateProfile(data: import('../types/profile.types').ProfileUpdateRequest) {
    submitting.value = true
    const result = await profileService.updateProfile(data)
    if (result.success) {
      profile.value = result.data
      showToast('success', 'Updated', 'Profile updated successfully')
    }
    submitting.value = false
    return result
  }

  return { profile, loading, submitting, fetchProfile, updateProfile }
})
