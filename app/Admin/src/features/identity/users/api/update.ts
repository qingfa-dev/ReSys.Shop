import { useMutation, useQueryClient, type UseMutationReturnType } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { User, UserUpdateRequest } from '../model/user.types'
import { usersQueryKeys } from './query-keys'

export function useUpdateUser(): UseMutationReturnType<User, Error, UserUpdateRequest, unknown> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body) => api.put<User>(`/api/admin/identity/users/${body.id}`, body),
    onSuccess: (_data, vars) => {
      qc.invalidateQueries({ queryKey: usersQueryKeys.all })
      qc.invalidateQueries({ queryKey: usersQueryKeys.detail(vars.id) })
    },
  })
}
