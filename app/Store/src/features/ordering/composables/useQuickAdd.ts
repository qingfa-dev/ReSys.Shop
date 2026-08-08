import { useCartStore } from '../stores/cartStore'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'

// Composable: Simplified one-click add-to-cart with user feedback.
export function useQuickAdd() {
  const cart = useCartStore()
  const notify = useNotify()
  const { handleError } = useApiErrorHandler()

  async function add(variantId: string): Promise<boolean> {
    // Guard: Reject empty variant ID before hitting the API.
    if (!variantId) { notify.warn('Unavailable'); return false }
    const ok = await cart.addItem(variantId, 1)
    // Notify: Surface success or failure to the user immediately.
    if (ok) notify.success('Added to cart')
    else handleError(new Error(cart.error ?? 'Failed'))
    return ok
  }

  return { add }
}
