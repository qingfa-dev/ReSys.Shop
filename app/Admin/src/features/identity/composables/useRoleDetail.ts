import { ref } from 'vue'
import type { Ref } from 'vue'
import type { Result } from '@/shared/types'
import type { RoleDetail } from '../types/role'
import { RoleApi } from '../services/roleApi'

export interface UseRoleDetailState {
  role: Ref<RoleDetail | null>
  loading: Ref<boolean>
  error: Ref<string | null>
  fetchRole: (id: string) => Promise<Result<RoleDetail>>
}

export function useRoleDetail() {
  const role = ref<RoleDetail | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchRole(id: string): Promise<Result<RoleDetail>> {
    loading.value = true
    error.value = null
    const result = await RoleApi.getRole(id)
    loading.value = false
    if (result.isSuccess) {
      role.value = result.value
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }
    return result
  }

  return { role, loading, error, fetchRole }
}
