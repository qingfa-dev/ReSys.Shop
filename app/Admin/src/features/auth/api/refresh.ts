import { useMutation } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { AuthTokens } from '../model/auth.types'

export function useRefresh() {
  return useMutation({
    mutationFn: (refreshToken: string) =>
      api.post<AuthTokens>('/api/auth/refresh', { refreshToken }),
  })
}
