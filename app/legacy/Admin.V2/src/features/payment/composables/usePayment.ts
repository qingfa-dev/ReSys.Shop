import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from '@/shared/composables/useToast'
import { PaymentApi } from '../api'

export function usePayment() {
  const toast = useToast()
  const route = useRoute()
  const router = useRouter()

  const id = computed(() => route.params.id as string | undefined)
  const mode = computed<'view'>(() => 'view')

  return { id, mode, route, router, toast, api: PaymentApi }
}
