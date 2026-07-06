import { useMutation } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { LoginRequest, AuthTokens } from '../model/auth.types'

export function useLogin() {
  return useMutation({
    mutationFn: (body: LoginRequest) => api.post<AuthTokens>('/api/auth/login', body),
  })
}
