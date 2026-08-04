<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import { useCartStore } from '../stores/cartStore'
import { useCheckoutStore } from '../stores/checkoutStore'
import { getPaymentMethods } from '@/features/payment/services/paymentApi'
import { usePayment } from '@/features/payment/composables/usePayment'
import { formatVnd } from '@/shared/utils/currency'
import type { PaymentMethod } from '@/features/payment/types/payment'

const checkout = useCheckoutStore()
const cart = useCartStore()
const payment = usePayment()

const methods = ref<PaymentMethod[]>([])
const selectedMethodId = ref<string | null>(null)
const loading = ref(true)
const processing = ref(false)
const localError = ref<string | null>(null)
const cardContainer = ref<HTMLElement | null>(null)
const clientSecret = ref<string | null>(null)

// Map: Stripe publishable key from env.
const publishableKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY as string

// Trigger: Fetch real payment methods on mount; init Stripe if a key is configured.
onMounted(async () => {
  const result = await getPaymentMethods()
  if (result.isSuccess) {
    methods.value = result.items
  } else {
    localError.value = result.message ?? 'Failed to load payment methods'
  }
  loading.value = false
  if (publishableKey && publishableKey !== 'pk_test_placeholder') {
    payment.init(publishableKey)
  }
})

// Clean up: Unmount Stripe Elements when leaving the step.
onUnmounted(() => payment.unmount())

// Trigger: Create the payment intent and mount the Stripe card element.
async function createIntent(): Promise<void> {
  if (!selectedMethodId.value || !cart.id) return
  processing.value = true
  localError.value = null
  const secret = await checkout.createPaymentIntent(selectedMethodId.value, cart.subtotal)
  processing.value = false
  if (secret) {
    clientSecret.value = secret
    await nextTick()
    if (cardContainer.value && payment.stripePromise.value) {
      await payment.mount(secret, cardContainer.value)
    }
  } else {
    localError.value = checkout.error ?? 'Unable to create a payment intent.'
  }
}

// Trigger: Confirm payment with Stripe, then advance to the confirm step.
async function pay(): Promise<void> {
  if (!clientSecret.value) return
  processing.value = true
  localError.value = null
  const stripe = await payment.stripePromise.value
  if (!stripe) {
    localError.value = 'Stripe is not available. Please try again later.'
    processing.value = false
    return
  }
  const { error } = await stripe.confirmCardPayment(clientSecret.value)
  processing.value = false
  if (error) {
    localError.value = error.message ?? 'Payment failed. Please try again.'
  } else {
    await checkout.goToStep(4)
  }
}
</script>
<template>
  <!-- Section: Payment Step -->
  <div class="bg-white rounded-xl border border-gray-200 p-6">
    <h2 class="text-lg font-semibold text-gray-900 mb-4">Payment</h2>
    <div class="flex justify-between text-sm text-gray-600 mb-4">
      <span>Order total</span>
      <span class="font-semibold text-gray-900">{{ formatVnd(cart.subtotal) }}</span>
    </div>

    <!-- Section: Loading -->
    <div v-if="loading" class="space-y-3 mb-6">
      <div v-for="i in 2" :key="i" class="h-16 bg-gray-100 rounded-lg animate-pulse" />
    </div>

    <!-- Section: Error -->
    <Message v-if="localError" severity="error" class="mb-6">{{ localError }}</Message>

    <!-- Section: No Methods -->
    <p v-else-if="methods.length === 0" class="text-sm text-gray-500 mb-6">No payment methods available.</p>

    <!-- Section: Payment Methods -->
    <div v-else class="mb-6">
      <span class="block text-sm font-medium text-gray-700 mb-2">Payment method</span>
      <div class="space-y-2">
        <div
          v-for="opt in methods"
          :key="opt.id"
          class="flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors"
          :class="selectedMethodId === opt.id ? 'border-teal-600 bg-teal-50' : 'border-gray-200 hover:border-gray-300'"
          @click="selectedMethodId = opt.id"
        >
          <RadioButton v-model="selectedMethodId" :input-id="`pay-${opt.id}`" :value="opt.id" />
          <label :for="`pay-${opt.id}`" class="text-sm text-gray-700 cursor-pointer">{{ opt.name }}</label>
        </div>
      </div>
      <!-- Section: Continue -->
      <Button v-if="selectedMethodId && !clientSecret" label="Continue to Payment" icon="pi pi-credit-card" class="mt-4 w-full" :loading="processing" @click="createIntent" />
    </div>

    <!-- Section: Stripe Card Element -->
    <div v-if="clientSecret" class="mb-6">
      <p class="text-sm text-gray-500 mb-3">Enter your card details:</p>
      <div ref="cardContainer" class="p-4 border border-gray-200 rounded-lg min-h-[40px]" />
    </div>

    <!-- Section: Actions -->
    <div v-if="clientSecret" class="flex justify-between">
      <Button label="Back" icon="pi pi-arrow-left" severity="secondary" :disabled="processing || checkout.loading" @click="checkout.goToStep(2)" />
      <Button label="Pay" icon="pi pi-credit-card" iconPos="right" :loading="processing" :disabled="!clientSecret" @click="pay" />
    </div>
  </div>
</template>