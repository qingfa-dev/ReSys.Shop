import { ref } from 'vue'
import type { Ref } from 'vue'
import type { PagedResult } from '@/shared/types'
import type { ProfileListItem } from '../types/profile'
import { ProfileApi } from '../services/profileApi'

export interface UseProfileDetailState {
  profile: Ref<ProfileListItem | null>
  loading: Ref<boolean>
  error: Ref<string | null>
  fetchProfile: (userId: string) => Promise<PagedResult<ProfileListItem>>
}

export function useProfileDetail() {
  const profile = ref<ProfileListItem | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchProfile(userId: string): Promise<PagedResult<ProfileListItem>> {
    loading.value = true
    error.value = null
    const result = await ProfileApi.getProfiles({ pageSize: 100 })
    loading.value = false
    if (result.isSuccess) {
      profile.value = result.items.find(p => p.userId === userId) ?? null
      if (!profile.value) {
        error.value = 'Profile not found.'
      }
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }
    return result
  }

  return { profile, loading, error, fetchProfile }
}
