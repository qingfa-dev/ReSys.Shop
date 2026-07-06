import { useQuery } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { AuthUser } from '../model/auth.types'
import { authQueryKeys } from './query-keys'

export function useCurrentUser() {
  return useQuery({
    queryKey: authQueryKeys.currentUser(),
    queryFn: () => api.get<AuthUser>('/api/auth/me'),
    retry: false,
    staleTime: 60_000,
  })
}
