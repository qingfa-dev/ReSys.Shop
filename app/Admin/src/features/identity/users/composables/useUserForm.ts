import { ref, computed } from 'vue'
import { createUserSchema, updateUserSchema } from '../model/user.schema'
import { useCreateUser, useUpdateUser } from '../api'
import type { UserCreateRequest, UserUpdateRequest } from '../model/user.types'

export function useUserForm() {
  const create = useCreateUser()
  const update = useUpdateUser()
  const errors = ref<Record<string, string | undefined>>({})

  async function submitCreate(input: UserCreateRequest) {
    const parsed = createUserSchema.safeParse(input)
    if (!parsed.success) {
      errors.value = parsed.error.flatten().fieldErrors as Record<string, string | undefined>
      throw new Error('validation')
    }
    errors.value = {}
    return create.mutateAsync(parsed.data as UserCreateRequest)
  }

  async function submitUpdate(input: UserUpdateRequest) {
    const parsed = updateUserSchema.safeParse(input)
    if (!parsed.success) {
      errors.value = parsed.error.flatten().fieldErrors as Record<string, string | undefined>
      throw new Error('validation')
    }
    errors.value = {}
    return update.mutateAsync(parsed.data as UserUpdateRequest)
  }

  return { errors, submitCreate, submitUpdate, isPending: computed(() => create.isPending.value || update.isPending.value) }
}
