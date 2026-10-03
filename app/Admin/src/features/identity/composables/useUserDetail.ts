import { ref } from 'vue'
import type { Ref } from 'vue'
import type { Result } from '@/shared/types'
import type { UserDetail } from '../types/user'
import { UserApi } from '../services/userApi'

export interface UseUserDetailState {
  user: Ref<UserDetail | null>
  loading: Ref<boolean>
  error: Ref<string | null>
  fetchUser: (id: string) => Promise<Result<UserDetail>>
}

export function useUserDetail() {
  const user = ref<UserDetail | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchUser(id: string): Promise<Result<UserDetail>> {
    loading.value = true
    error.value = null
    const result = await UserApi.getUser(id)
    loading.value = false
    if (result.isSuccess) {
      user.value = result.value
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }
    return result
  }

  return { user, loading, error, fetchUser }
}
