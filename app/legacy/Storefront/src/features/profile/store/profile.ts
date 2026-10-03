import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Profile } from '../types/entity'
import type { UpdateProfileRequest } from '../types/request'
import { profileService } from '../services/profile.service'

export const useProfileStore = defineStore('profile', () => {
  const profile = ref<Profile | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const fullName = computed(() => profile.value ? `${profile.value.firstName} ${profile.value.lastName}` : '')

  async function fetchProfile(userId: string) {
    loading.value = true
    error.value = null
    try {
      const result = await profileService.getProfile(userId)
      if (result.isSuccess && result.data) {
        profile.value = result.data
      } else {
        throw new Error(result.message || 'Failed to fetch profile')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch profile'
    } finally {
      loading.value = false
    }
  }

  async function updateProfile(userId: string, updates: UpdateProfileRequest) {
    loading.value = true
    error.value = null
    try {
      const result = await profileService.updateProfile(userId, updates)
      if (result.isSuccess && result.data) {
        profile.value = result.data
      } else {
        throw new Error(result.message || 'Failed to update profile')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to update profile'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function uploadAvatar(userId: string, file: File) {
    loading.value = true
    error.value = null
    try {
      const result = await profileService.uploadAvatar(userId, file)
      if (result.isSuccess && result.data) {
        profile.value = result.data
      } else {
        throw new Error(result.message || 'Failed to upload avatar')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to upload avatar'
      throw e
    } finally {
      loading.value = false
    }
  }

  return {
    profile,
    loading,
    error,
    fullName,
    fetchProfile,
    updateProfile,
    uploadAvatar,
  }
})
