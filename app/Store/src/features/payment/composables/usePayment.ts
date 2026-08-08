import { ref } from 'vue'
import { loadStripe } from '@stripe/stripe-js'
import type { Stripe, StripeElements } from '@stripe/stripe-js'

// Cache: Singleton Stripe.js promise shared across all usePayment() instances
const stripePromise = ref<Promise<Stripe | null> | null>(null)

// Contract: pre=publishableKey is valid, post=stripePromise is non-null when key present
export function usePayment() {
  const elements = ref<StripeElements | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Guard: Reject early if publishable key is missing from environment
  function init(publishableKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY): void {
    if (!publishableKey) {
      error.value = 'Stripe publishable key is not configured (set VITE_STRIPE_PUBLISHABLE_KEY).'
      return
    }
    // Initialize: Load Stripe.js only on first call; subsequent calls are no-ops
    if (!stripePromise.value) stripePromise.value = loadStripe(publishableKey)
  }

  // Await: Stripe.js load completes; timeout depends on CDN availability
  async function mount(clientSecret: string, container: HTMLElement): Promise<Stripe | null> {
    loading.value = true
    error.value = null
    const stripe = await stripePromise.value
    if (!stripe) {
      error.value = 'Failed to load Stripe.'
      loading.value = false
      return null
    }
    // Create: Stripe Elements card form bound to the payment intent client secret
    elements.value = stripe.elements({ clientSecret })
    const card = elements.value.create('card')
    card.mount(container)
    loading.value = false
    return stripe
  }

  function unmount(): void {
    // Dispose: Detach card element from DOM and release reference
    elements.value?.getElement('card')?.unmount()
    elements.value = null
  }

  return { loading, error, init, mount, unmount, stripePromise }
}
