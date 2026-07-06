import { useQuery, type UseQueryReturnType } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { User } from '../model/user.types'
import { usersQueryKeys } from './query-keys'

export function useUser(id: string): UseQueryReturnType<User, Error> {
  return useQuery({
    queryKey: usersQueryKeys.detail(id),
    queryFn: () => api.get<User>(`/api/admin/identity/users/${id}`),
    enabled: !!id,
  })
}
