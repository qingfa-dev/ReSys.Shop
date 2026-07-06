import { useMutation, useQueryClient, type UseMutationReturnType } from '@tanstack/vue-query'
import { api } from '@/shared/api/client'
import type { User, UserCreateRequest } from '../model/user.types'
import { usersQueryKeys } from './query-keys'

export function useCreateUser(): UseMutationReturnType<User, Error, UserCreateRequest, unknown> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body) => api.post<User>('/api/admin/identity/users', body),
    onSuccess: () => qc.invalidateQueries({ queryKey: usersQueryKeys.all }),
  })
}
