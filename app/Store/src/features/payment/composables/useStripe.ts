import { ref } from 'vue'
import { loadStripe } from '@stripe/stripe-js'
import type { Stripe, StripeElements } from '@stripe/stripe-js'

let stripePromise: Promise<Stripe | null> | null = null
let cardElement: StripeElements | null = null
const loading = ref(false)
const error = ref<string | null>(null)

export function useStripe() {
  async function init(publishableKey?: string): Promise<void> {
    if (stripePromise) return
    loading.value = true
    stripePromise = loadStripe(publishableKey ?? import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY)
    loading.value = false
  }

  async function mount(clientSecret: string, container: HTMLElement): Promise<boolean> {
    const stripe = await stripePromise
    if (!stripe) { error.value = 'Stripe not loaded'; return false }
    cardElement = stripe.elements({ clientSecret })
    cardElement.create('payment').mount(container)
    return true
  }

  function unmount(): void {
    cardElement?.getElement('payment')?.unmount()
    cardElement = null
  }

  return { loading, error, stripePromise, init, mount, unmount }
}
