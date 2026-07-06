import { useMutation } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'

export function useLogout() {
  return useMutation({
    mutationFn: () => api.post<void>('/api/auth/logout'),
  })
}
