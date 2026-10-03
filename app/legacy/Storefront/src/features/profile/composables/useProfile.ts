import { computed } from 'vue'
import { useProfileStore } from '@/features/profile/store/profile'

export function useProfile() {
  const store = useProfileStore()

  const profile = computed(() => store.profile)
  const isLoading = computed(() => store.loading)
  const error = computed(() => store.error)
  const fullName = computed(() => store.fullName)

  async function fetchProfile(userId: string) {
    await store.fetchProfile(userId)
  }

  async function updateProfile(userId: string, updates: import('../types/request').UpdateProfileRequest) {
    await store.updateProfile(userId, updates)
  }

  async function uploadAvatar(userId: string, file: File) {
    await store.uploadAvatar(userId, file)
  }

  return {
    profile,
    isLoading,
    error,
    fullName,
    fetchProfile,
    updateProfile,
    uploadAvatar,
  }
}
