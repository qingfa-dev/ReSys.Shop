import { ref } from 'vue'
import { loadStripe } from '@stripe/stripe-js'
import type { Stripe, StripeElements, StripeCardElement } from '@stripe/stripe-js'

// Cache: Singleton Stripe.js promise shared across all usePayment() instances
const stripePromise = ref<Promise<Stripe | null> | null>(null)

// Contract: pre=publishableKey is valid, post=stripePromise is non-null when key present
export function usePayment() {
  const elements = ref<StripeElements | null>(null)
  const cardElement = ref<StripeCardElement | null>(null)
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

  // Await: Stripe.js load completes; timeout depends on CDN availability.
  // clientSecret is optional: a bare card element (no secret) can still produce a
  // PaymentMethod token, which is needed before the backend creates the intent.
  async function mount(clientSecret: string | undefined, container: HTMLElement): Promise<Stripe | null> {
    loading.value = true
    error.value = null
    const stripe = await stripePromise.value
    if (!stripe) {
      error.value = 'Failed to load Stripe.'
      loading.value = false
      return null
    }
    // Create: Stripe Elements card form; bind to the intent client secret when available
    elements.value = clientSecret ? stripe.elements({ clientSecret }) : stripe.elements()
    cardElement.value = elements.value.create('card')
    cardElement.value.mount(container)
    loading.value = false
    return stripe
  }

  // Token: Create a Stripe PaymentMethod from the mounted card, returning the pm_... id.
  async function createPaymentMethod(): Promise<string | null> {
    const stripe = await stripePromise.value
    if (!stripe || !cardElement.value) {
      error.value = 'Card form is not ready.'
      return null
    }
    const { paymentMethod, error: pmError } = await stripe.createPaymentMethod({
      type: 'card',
      card: cardElement.value,
    })
    if (pmError) {
      error.value = pmError.message ?? 'Failed to collect card details.'
      return null
    }
    return paymentMethod.id
  }

  function unmount(): void {
    // Dispose: Detach card element from DOM and release reference
    cardElement.value?.unmount()
    cardElement.value = null
    elements.value = null
  }

  return { loading, error, init, mount, createPaymentMethod, unmount, stripePromise }
}
