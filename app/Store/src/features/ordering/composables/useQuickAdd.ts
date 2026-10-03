import { useCart } from './useCart'
import { useNotify } from '@/shared/composables/useNotify'

// Composable: Simplified one-click add-to-cart with user feedback.
export function useQuickAdd() {
  const cart = useCart()
  const notify = useNotify()

  async function add(variantId: string): Promise<boolean> {
    if (!variantId) { notify.warn('Unavailable'); return false }
    const ok = await cart.addItem(variantId, 1)
    if (ok) notify.success('Added to cart')
    else notify.error(cart.error ?? 'Failed to add to cart')
    return ok
  }

  return { add }
}