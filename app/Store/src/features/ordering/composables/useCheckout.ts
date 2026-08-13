import { ref, computed, reactive, watch } from 'vue'
import { CheckoutApi } from '../services/checkoutApi'
import { useRouter } from 'vue-router'
import { emit } from '@/shared/composables/useStoreEvents'

type Step = 1 | 2 | 3 | 4 | 5

interface CartRef {
  id: string | null
  isEmpty: boolean
  checkoutState: string | null
  fetchCart: (force?: boolean) => Promise<boolean>
}

// Map: Translate the backend checkout state into its wizard step (PAT-001).
function stepOf(state: string | null): Step {
  switch (state) {
    case 'Address': return 1
    case 'Delivery': return 2
    case 'Payment': return 3
    case 'Confirm': return 4
    case 'Complete': return 5
    default: return 1
  }
}

export function useCheckout(getCart: () => CartRef) {
  const displayStep = ref<Step>(1)
  const backendStep = computed<Step>(() => stepOf(getCart().checkoutState))
  const shipAddressId = ref<string | null>(null)
  const shippingMethodId = ref<string | null>(null)
  const paymentMethodId = ref<string | null>(null)
  const paymentIntentId = ref<string | null>(null)
  const paymentClientSecret = ref<string | null>(null)
  const checkoutUrl = ref<string | null>(null)
  const paymentState = ref<string | null>(null)
  const orderId = ref<string | null>(null)
  const email = ref('')
  const loading = ref(false)
  const error = ref<string | null>(null)

  const steps = computed(() => [
    { label: 'Address', number: 1, complete: backendStep.value > 1, current: displayStep.value === 1 },
    { label: 'Delivery', number: 2, complete: backendStep.value > 2, current: displayStep.value === 2 },
    { label: 'Payment', number: 3, complete: backendStep.value > 3, current: displayStep.value === 3 },
    { label: 'Confirm', number: 4, complete: backendStep.value > 4, current: displayStep.value === 4 },
    { label: 'Complete', number: 5, complete: backendStep.value === 5, current: displayStep.value === 5 },
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
        displayStep.value = 2
        await getCart().fetchCart(true)
        return true
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
        displayStep.value = 3
        await getCart().fetchCart(true)
        return true
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

  async function createPaymentIntent(methodId: string, opts: { returnUrl?: string; cancelUrl?: string } = {}): Promise<boolean> {
    loading.value = true
    error.value = null
    try {
      const cart = getCart()
      const result = await CheckoutApi.createPaymentIntent({
        orderId: cart.id!,
        paymentMethodId: methodId,
        returnUrl: opts.returnUrl,
        cancelUrl: opts.cancelUrl,
      })
      if (result.isSuccess) {
        // Payment id is the PaymentCapture.Id — stable across COD and gateway paths.
        paymentIntentId.value = result.value.id
        checkoutUrl.value = result.value.checkoutUrl ?? null
        paymentState.value = result.value.state ?? null
        paymentClientSecret.value = result.value.clientSecret ?? null
        paymentMethodId.value = methodId
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
        displayStep.value = 5
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

  // Guard: Ask the backend whether the cart is ready to place the order.
  async function validateCheckout(): Promise<boolean> {
    loading.value = true
    error.value = null
    try {
      const result = await CheckoutApi.validateCheckout()
      if (!result.isSuccess) {
        error.value = result.message
      }
      loading.value = false
      return result.isSuccess
    } catch {
      error.value = 'Failed to validate checkout'
      loading.value = false
      return false
    }
  }

  // Watch: Regress the wizard when the backend state moves backwards or clears the intent.
  watch(
    () => getCart().checkoutState,
    (cur, prev) => {
      const prevStep = stepOf(prev)
      const curStep = stepOf(cur)
      if (prev === 'Payment' && cur === 'Delivery') {
        paymentClientSecret.value = null
        paymentIntentId.value = null
        paymentMethodId.value = null
        checkoutUrl.value = null
        paymentState.value = null
      }
      if (curStep >= 2 && curStep < (prevStep >= 2 ? prevStep : Number.MAX_SAFE_INTEGER)) {
        displayStep.value = curStep
      }
    },
  )

  function reset(): void {
    displayStep.value = 1
    shipAddressId.value = null
    shippingMethodId.value = null
    paymentMethodId.value = null
    paymentIntentId.value = null
    paymentClientSecret.value = null
    checkoutUrl.value = null
    paymentState.value = null
    orderId.value = null
    error.value = null
  }

  return reactive({
    backendStep, displayStep, shipAddressId, shippingMethodId, paymentMethodId, paymentIntentId,
    paymentClientSecret, checkoutUrl, paymentState, orderId, email, loading, error, steps,
    init, saveAddress, selectShippingRate, createPaymentIntent, placeOrder, validateCheckout, reset,
  })
}
