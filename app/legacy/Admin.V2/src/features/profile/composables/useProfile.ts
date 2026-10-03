import { useRoute, useRouter } from 'vue-router'
import { useToast } from '@/shared/composables/useToast'
import { ProfileApi } from '../api'

export function useProfile() {
  const toast = useToast()
  const route = useRoute()
  const router = useRouter()

  return { route, router, toast, api: ProfileApi }
}
