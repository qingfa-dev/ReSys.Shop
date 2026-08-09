import { ref, computed, reactive } from 'vue'
import { CheckoutApi } from '../services/checkoutApi'
import { useRouter } from 'vue-router'
import { emit } from '@/shared/composables/useStoreEvents'

type Step = 1 | 2 | 3 | 4 | 5

interface CartRef {
  id: string | null
  isEmpty: boolean
  fetchCart: () => Promise<boolean>
}

export function useCheckout(getCart: () => CartRef) {
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

  const steps = computed(() => [
    { label: 'Address', number: 1, complete: currentStep.value > 1, current: currentStep.value === 1 },
    { label: 'Delivery', number: 2, complete: currentStep.value > 2, current: currentStep.value === 2 },
    { label: 'Payment', number: 3, complete: currentStep.value > 3, current: currentStep.value === 3 },
    { label: 'Confirm', number: 4, complete: currentStep.value > 4, current: currentStep.value === 4 },
    { label: 'Complete', number: 5, complete: currentStep.value === 5, current: currentStep.value === 5 },
  ])

  function init(): void {
    const cart = getCart()
    const router = useRouter()
    if (cart.isEmpty) { router.push('/cart'); return }
    cart.fetchCart()
  }

  async function saveAddress(addressId: string, userEmail: string): Promise<boolean> {
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
    loading.value = true
    error.value = null
    try {
      const cart = getCart()
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
    if (!paymentIntentId.value) return false
    loading.value = true
    error.value = null
    try {
      const result = await CheckoutApi.placeOrder({ paymentIntentId: paymentIntentId.value })
      if (result.isSuccess) {
        orderId.value = result.value.id
        currentStep.value = 5
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
    currentStep.value = 1
    shipAddressId.value = null
    shippingMethodId.value = null
    paymentMethodId.value = null
    paymentIntentId.value = null
    paymentClientSecret.value = null
    orderId.value = null
    error.value = null
  }

  return reactive({
    currentStep, shipAddressId, shippingMethodId, paymentMethodId, paymentIntentId,
    paymentClientSecret, orderId, email, loading, error, steps,
    init, saveAddress, selectShippingRate, createPaymentIntent, placeOrder, reset,
  })
}
