import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToast } from '@/common/composables/toast.use'
import { profileService } from '../services/profile.service'
import type { Profile } from '../types/profile.response.type'

export const useProfileStore = defineStore('profile', () => {
  const { showToast } = useToast()
  const { t } = useI18n()
  const profile = ref<Profile | null>(null)
  const loading = ref(false)
  const submitting = ref(false)

  async function fetchProfile() {
    loading.value = true
    const result = await profileService.getProfile()
    if (result.isSuccess) {
      profile.value = result.value
    } else {
      showToast('error', t('common.error'), result.errors?.[0]?.message || t('profile.messages.load_error'))
    }
    loading.value = false
    return result
  }

  async function updateProfile(data: import('../types/profile.request.type').ProfileUpdateRequest) {
    submitting.value = true
    const result = await profileService.updateProfile(data)
    if (result.isSuccess) {
      profile.value = result.value
      showToast('success', t('common.updated'), t('profile.messages.update_success'))
    }
    submitting.value = false
    return result
  }

  return { profile, loading, submitting, fetchProfile, updateProfile }
})
