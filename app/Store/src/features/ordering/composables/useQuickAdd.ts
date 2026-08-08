import { useCartStore } from '../stores/cartStore'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'

export function useQuickAdd() {
  const cart = useCartStore()
  const notify = useNotify()
  const { handleError } = useApiErrorHandler()

  async function add(variantId: string): Promise<boolean> {
    if (!variantId) { notify.warn('Unavailable'); return false }
    const ok = await cart.addItem(variantId, 1)
    if (ok) notify.success('Added to cart')
    else handleError(new Error(cart.error ?? 'Failed'))
    return ok
  }

  return { add }
}
