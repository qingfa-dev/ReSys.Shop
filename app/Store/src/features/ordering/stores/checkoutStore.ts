import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as checkoutApi from '../services/checkoutApi'
import * as paymentApi from '@/features/payment/services/paymentApi'
import { useCartStore } from './cartStore'

export type CheckoutStep = 1 | 2 | 3 | 4 | 5

export const useCheckoutStore = defineStore('checkout', () => {
  const currentStep = ref<CheckoutStep>(1)
  const shipAddressId = ref<string | null>(null)
  const paymentIntentId = ref<string | null>(null)
  const orderId = ref<string | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const email = ref('')

  const steps = [
    { label: 'Address', stepNumber: 1 },
    { label: 'Delivery', stepNumber: 2 },
    { label: 'Payment', stepNumber: 3 },
    { label: 'Confirm', stepNumber: 4 },
    { label: 'Complete', stepNumber: 5 },
  ]

  async function goToStep(step: CheckoutStep): Promise<void> {
    const advancing = step > currentStep.value
    // The all-or-nothing /cart/validate gate requires every checkout
    // prerequisite at once, but data is collected incrementally across steps
    // 1-3. Only run it as a final safety net when advancing to the review
    // (Confirm) and completion steps; never gate data-collection transitions
    // (1->2->3) or backward navigation on it.
    if (advancing && step >= 4) {
      loading.value = true
      error.value = null
      try {
        const validateResult = await checkoutApi.validateCheckout()
        if (!validateResult.isSuccess) {
          error.value = validateResult.message ?? 'Please complete the current step first.'
          return
        }
        currentStep.value = step
      } catch {
        // The error interceptor throws HttpError on network failures / non-Result 5xx.
        error.value = 'Please complete the current step first.'
      } finally {
        loading.value = false
      }
      return
    }
    currentStep.value = step
  }

  async function saveAddress(addressId: string, userEmail: string): Promise<boolean> {
    shipAddressId.value = addressId
    email.value = userEmail
    loading.value = true
    error.value = null
    try {
      const result = await checkoutApi.updateCheckout({
        shipAddressId: addressId,
        billAddressId: addressId,
        currency: 'VND',
        email: userEmail,
      })
      if (result.isSuccess) return true
      error.value = result.message ?? 'Failed to save address'
      return false
    } catch {
      error.value = 'Failed to save address'
      return false
    } finally {
      loading.value = false
    }
  }

  async function calculateShipping(methodId: string): Promise<boolean> {
    loading.value = true
    error.value = null
    try {
      const result = await checkoutApi.selectShippingRate({ shippingMethodId: methodId })
      if (result.isSuccess) return true
      error.value = result.message ?? 'Failed to calculate shipping'
      return false
    } catch {
      error.value = 'Failed to calculate shipping'
      return false
    } finally {
      loading.value = false
    }
  }

  async function createPaymentIntent(methodId: string, _amount: number): Promise<string | null> {
    const cart = useCartStore()
    if (!cart.id) {
      error.value = 'Cart is not loaded.'
      return null
    }
    error.value = null
    try {
      const result = await checkoutApi.createPaymentIntent({
        orderId: cart.id,
        paymentMethodId: methodId,
        returnUrl: window.location.origin + '/checkout',
      })
      if (result.isSuccess) {
        paymentIntentId.value = result.value.responseCode ?? result.value.id
        return result.value.clientSecret
      }
      error.value = result.message ?? 'Failed to create payment intent'
      return null
    } catch {
      error.value = 'Failed to create payment intent'
      return null
    }
  }

  async function placeOrder(): Promise<boolean> {
    if (!paymentIntentId.value) return false
    loading.value = true
    error.value = null
    try {
      const result = await checkoutApi.placeOrder({ paymentIntentId: paymentIntentId.value })
      if (result.isSuccess) {
        orderId.value = result.value.id
        currentStep.value = 5
        return true
      }
      error.value = result.message ?? 'Failed to place order'
      return false
    } catch {
      error.value = 'Failed to place order'
      return false
    } finally {
      loading.value = false
    }
  }

  async function confirmPayment(paymentId: string): Promise<void> {
    await paymentApi.confirmPayment(paymentId)
  }

  function reset(): void {
    currentStep.value = 1
    shipAddressId.value = null
    paymentIntentId.value = null
    orderId.value = null
    error.value = null
  }

  return { currentStep, shipAddressId, paymentIntentId, orderId, loading, error, email, steps, goToStep, saveAddress, calculateShipping, createPaymentIntent, placeOrder, confirmPayment, reset }
})
