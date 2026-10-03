import { ref } from 'vue'
import { loadStripe } from '@stripe/stripe-js'
import type { Stripe, StripeElements } from '@stripe/stripe-js'

// Cache: Singleton Stripe.js promise shared across all useStripe() instances
let stripePromise: Promise<Stripe | null> | null = null
let cardElement: StripeElements | null = null
const loading = ref(false)
const error = ref<string | null>(null)

// Contract: pre=publishableKey is valid or env fallback exists, post=stripePromise is non-null
export function useStripe() {
  async function init(publishableKey?: string): Promise<void> {
    // Guard: Skip re-initialisation if Stripe.js already loaded
    if (stripePromise) return
    loading.value = true
    // Call: Stripe.js CDN - loads the library once per page lifecycle
    stripePromise = loadStripe(publishableKey ?? import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY)
    loading.value = false
  }

  async function mount(clientSecret: string, container: HTMLElement): Promise<boolean> {
    // Check: Stripe.js must be initialised before mounting elements
    const stripe = await stripePromise
    if (!stripe) { error.value = 'Stripe not loaded'; return false }
    // Create: Stripe Elements instance bound to the client secret
    cardElement = stripe.elements({ clientSecret })
    cardElement.create('payment').mount(container)
    return true
  }

  function unmount(): void {
    // Dispose: Detach payment element from DOM and release reference
    cardElement?.getElement('payment')?.unmount()
    cardElement = null
  }

  return { loading, error, stripePromise, init, mount, unmount }
}
