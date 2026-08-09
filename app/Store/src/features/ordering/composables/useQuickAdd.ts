import { useCart } from './useCart'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'

// Composable: Simplified one-click add-to-cart with user feedback.
export function useQuickAdd() {
  const cart = useCart()
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
