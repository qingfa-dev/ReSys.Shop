<script setup lang="ts">
import { onMounted } from 'vue'
import { useCheckoutStore } from '../stores/checkoutStore'
import { useCartStore } from '../stores/cartStore'
import CheckoutStepper from '../components/CheckoutStepper.vue'
import CheckoutStepAddress from '../components/CheckoutStepAddress.vue'
import CheckoutStepDelivery from '../components/CheckoutStepDelivery.vue'
import CheckoutStepPayment from '../components/CheckoutStepPayment.vue'
import CheckoutStepConfirm from '../components/CheckoutStepConfirm.vue'
import CheckoutStepComplete from '../components/CheckoutStepComplete.vue'

const checkout = useCheckoutStore()
const cart = useCartStore()

onMounted(() => cart.fetchCart())
</script>
<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <h1 class="text-2xl font-bold text-gray-900 mb-8">Checkout</h1>
    <CheckoutStepper :steps="checkout.steps" :current-step="checkout.currentStep" />
    <Message v-if="checkout.error" severity="error" class="mb-6">{{ checkout.error }}</Message>
    <CheckoutStepAddress v-if="checkout.currentStep === 1" />
    <CheckoutStepDelivery v-if="checkout.currentStep === 2" />
    <CheckoutStepPayment v-if="checkout.currentStep === 3" />
    <CheckoutStepConfirm v-if="checkout.currentStep === 4" />
    <CheckoutStepComplete v-if="checkout.currentStep === 5" />
  </div>
</template>
