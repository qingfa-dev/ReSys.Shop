import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { CheckoutApi } from '../services/checkoutApi'
import { useCartStore } from './cartStore'
import { useRouter } from 'vue-router'
import { emit } from '@/shared/composables/useStoreEvents'

type Step = 1 | 2 | 3 | 4 | 5

// Store: 5-step checkout wizard state (Address → Delivery → Payment → Confirm → Complete).
export const useCheckoutStore = defineStore('checkout', () => {
  const currentStep = ref<Step>(1)
  const shipAddressId = ref<string | null>(null)
  const shippingMethodId = ref<string | null>(null)
  const paymentMethodId = ref<string | null>(null)
  const paymentIntentId = ref<string | null>(null)
  const paymentClientSecret = ref<string | null>(null)
  const orderId = ref<string | null>(null)
  const email = ref('')
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Compute: Derive step metadata with completion and active state for the stepper UI.
  const steps = computed(() => [
    { label: 'Address', number: 1, complete: currentStep.value > 1, current: currentStep.value === 1 },
    { label: 'Delivery', number: 2, complete: currentStep.value > 2, current: currentStep.value === 2 },
    { label: 'Payment', number: 3, complete: currentStep.value > 3, current: currentStep.value === 3 },
    { label: 'Confirm', number: 4, complete: currentStep.value > 4, current: currentStep.value === 4 },
    { label: 'Complete', number: 5, complete: currentStep.value === 5, current: currentStep.value === 5 },
  ])

  function init(): void {
    const cart = useCartStore()
    const router = useRouter()
    // Guard: Redirect to cart if there are no items to checkout.
    if (cart.isEmpty) { router.push('/cart'); return }
    cart.fetchCart()
  }

  async function saveAddress(addressId: string, userEmail: string): Promise<boolean> {
    // Update: Persist shipping and billing address, then advance to Delivery step.
    loading.value = true
    error.value = null
    try {
      const result = await CheckoutApi.updateCheckout({ shipAddressId: addressId, billAddressId: addressId, email: userEmail })
      if (result.isSuccess) {
        shipAddressId.value = addressId
        email.value = userEmail
        currentStep.value = 2
      } else {
        error.value = result.message
      }
      loading.value = false
      return result.isSuccess
    } catch {
      error.value = 'Failed to save address'
      loading.value = false
      return false
    }
  }

  async function selectShippingRate(methodId: string): Promise<boolean> {
    // Update: Select delivery method and advance to Payment step.
    loading.value = true
    error.value = null
    try {
      const result = await CheckoutApi.selectShippingRate({ shippingMethodId: methodId })
      if (result.isSuccess) {
        shippingMethodId.value = methodId
        currentStep.value = 3
      } else {
        error.value = result.message
      }
      loading.value = false
      return result.isSuccess
    } catch {
      error.value = 'Failed to select shipping'
      loading.value = false
      return false
    }
  }

  async function createPaymentIntent(methodId: string): Promise<boolean> {
    // Call: Create payment intent with gateway, store client secret for confirmation.
    loading.value = true
    error.value = null
    try {
      const cart = useCartStore()
      const result = await CheckoutApi.createPaymentIntent({ orderId: cart.id!, paymentMethodId: methodId })
      if (result.isSuccess) {
        paymentIntentId.value = result.value.responseCode ?? result.value.id
        paymentClientSecret.value = result.value.clientSecret
        paymentMethodId.value = methodId
        currentStep.value = 4
      } else {
        error.value = result.message
      }
      loading.value = false
      return result.isSuccess
    } catch {
      error.value = 'Failed to create payment intent'
      loading.value = false
      return false
    }
  }

  async function placeOrder(): Promise<boolean> {
    // Guard: Require a payment intent before submitting the order.
    if (!paymentIntentId.value) return false
    loading.value = true
    error.value = null
    try {
      const result = await CheckoutApi.placeOrder({ paymentIntentId: paymentIntentId.value })
      if (result.isSuccess) {
        orderId.value = result.value.id
        currentStep.value = 5
        // Raise: Notify other stores (e.g. orderStore) that an order was placed.
        emit({ type: 'checkout:placed', orderId: result.value.id })
      } else {
        error.value = result.message
      }
      loading.value = false
      return result.isSuccess
    } catch {
      error.value = 'Failed to place order'
      loading.value = false
      return false
    }
  }

  function reset(): void {
    // Reset: Return checkout wizard to initial step with all fields cleared.
    currentStep.value = 1
    shipAddressId.value = null
    shippingMethodId.value = null
    paymentMethodId.value = null
    paymentIntentId.value = null
    paymentClientSecret.value = null
    orderId.value = null
    error.value = null
  }

  return {
    currentStep, shipAddressId, shippingMethodId, paymentMethodId, paymentIntentId,
    paymentClientSecret, orderId, email, loading, error, steps,
    init, saveAddress, selectShippingRate, createPaymentIntent, placeOrder, reset,
  }
})
