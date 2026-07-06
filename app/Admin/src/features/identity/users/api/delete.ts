import { useMutation, useQueryClient, type UseMutationReturnType } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { UserId } from '@/shared/types/id'
import { usersQueryKeys } from './query-keys'

export function useDeleteUser(): UseMutationReturnType<void, Error, UserId, unknown> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => api.delete<void>(`/api/admin/identity/users/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: usersQueryKeys.all }),
  })
}
