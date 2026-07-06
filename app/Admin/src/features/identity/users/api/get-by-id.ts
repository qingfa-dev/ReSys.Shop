import { useQuery, type UseQueryReturnType } from '@tanstack/vue-query'
import { computed, type Ref } from 'vue'
import { api } from '@/shared/api/client'
import type { User } from '../model/user.types'
import { usersQueryKeys } from './query-keys'

export function useUser(id: Ref<string | null>): UseQueryReturnType<User, Error> {
  const queryKey = computed(() => usersQueryKeys.detail(id.value ?? ''))
  return useQuery({
    queryKey,
    queryFn: () => api.get<User>(`/api/admin/identity/users/${id.value}`),
    enabled: !!id.value,
  })
}
