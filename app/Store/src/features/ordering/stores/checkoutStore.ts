import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as checkoutApi from '../services/checkoutApi'
import { useCartStore } from './cartStore'

export type CheckoutStep = 1 | 2 | 3 | 4 | 5

export const useCheckoutStore = defineStore('checkout', () => {
  const currentStep = ref<CheckoutStep>(1)
  const shipAddressId = ref<string | null>(null)
  const shippingMethodId = ref<string | null>(null)
  const paymentIntentId = ref<string | null>(null)
  const orderId = ref<string | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const email = ref('')
  const currency = ref('VND')

  const steps = [
    { label: 'Address', stepNumber: 1 },
    { label: 'Delivery', stepNumber: 2 },
    { label: 'Payment', stepNumber: 3 },
    { label: 'Confirm', stepNumber: 4 },
    { label: 'Complete', stepNumber: 5 },
  ]

  async function goToStep(step: CheckoutStep): Promise<void> {
    loading.value = true
    error.value = null
    // Validate current step before advancing
    const validateResult = await checkoutApi.validateCheckout()
    if (!validateResult.isSuccess && step > currentStep.value) {
      error.value = validateResult.message ?? 'Please complete the current step first.'
      loading.value = false
      return
    }
    currentStep.value = step
    loading.value = false
  }

  async function saveAddress(addressId: string, userEmail: string): Promise<boolean> {
    shipAddressId.value = addressId
    email.value = userEmail
    const result = await checkoutApi.updateCheckout({
      shipAddressId: addressId,
      currency: currency.value,
      email: userEmail,
    })
    return result.isSuccess
  }

  async function calculateShipping(methodId: string): Promise<boolean> {
    shippingMethodId.value = methodId
    const result = await checkoutApi.selectShippingRate({ shippingMethodId: methodId })
    return result.isSuccess
  }

  async function createPaymentIntent(methodId: string, amount: number): Promise<string | null> {
    const cart = useCartStore()
    if (!cart.id) {
      error.value = 'Cart is not loaded.'
      return null
    }
    const result = await checkoutApi.createPaymentIntent({
      orderId: cart.id,
      amount,
      currency: currency.value,
      paymentMethodId: methodId,
    })
    if (result.isSuccess) {
      paymentIntentId.value = result.value.responseCode ?? result.value.id
      return result.value.clientSecret
    }
    return null
  }

  async function placeOrder(): Promise<boolean> {
    if (!paymentIntentId.value) return false
    loading.value = true
    const result = await checkoutApi.placeOrder({ paymentIntentId: paymentIntentId.value })
    if (result.isSuccess) {
      orderId.value = result.value.id
      currentStep.value = 5
      loading.value = false
      return true
    }
    error.value = result.message ?? 'Failed to place order'
    loading.value = false
    return false
  }

  function reset(): void {
    currentStep.value = 1
    shipAddressId.value = null
    shippingMethodId.value = null
    paymentIntentId.value = null
    orderId.value = null
    error.value = null
  }

  return { currentStep, shipAddressId, shippingMethodId, paymentIntentId, orderId, loading, error, email, currency, steps, goToStep, saveAddress, calculateShipping, createPaymentIntent, placeOrder, reset }
})
