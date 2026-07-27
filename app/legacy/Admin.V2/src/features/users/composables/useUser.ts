import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from '@/shared/composables/useToast'
import { UserApi } from '../api'

export function useUser() {
  const toast = useToast()
  const route = useRoute()
  const router = useRouter()

  const id = computed(() => route.params.id as string | undefined)
  const mode = computed<'create' | 'view' | 'edit'>(() => {
    if (!id.value) return 'create'
    return route.name?.toString().endsWith('.edit') ? 'edit' : 'view'
  })

  return { id, mode, route, router, toast, api: UserApi }
}
