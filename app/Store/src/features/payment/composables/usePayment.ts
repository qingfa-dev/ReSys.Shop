import { ref } from 'vue'
import { loadStripe } from '@stripe/stripe-js'
import type { Stripe, StripeElements } from '@stripe/stripe-js'

// Shared across all usePayment() instances so the Stripe.js script loads exactly once.
const stripePromise = ref<Promise<Stripe | null> | null>(null)

export function usePayment() {
  const elements = ref<StripeElements | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // publishableKey defaults to VITE_STRIPE_PUBLISHABLE_KEY; the caller may override it.
  function init(publishableKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY): void {
    if (!publishableKey) {
      error.value = 'Stripe publishable key is not configured (set VITE_STRIPE_PUBLISHABLE_KEY).'
      return
    }
    if (!stripePromise.value) stripePromise.value = loadStripe(publishableKey)
  }

  async function mount(clientSecret: string, container: HTMLElement): Promise<Stripe | null> {
    loading.value = true
    error.value = null
    const stripe = await stripePromise.value
    if (!stripe) {
      error.value = 'Failed to load Stripe.'
      loading.value = false
      return null
    }
    elements.value = stripe.elements({ clientSecret })
    const card = elements.value.create('card')
    card.mount(container)
    loading.value = false
    return stripe
  }

  function unmount(): void {
    elements.value?.getElement('card')?.unmount()
    elements.value = null
  }

  return { loading, error, init, mount, unmount, stripePromise }
}
