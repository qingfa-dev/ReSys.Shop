<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { getPaymentStatus } from '@/features/payment/services/paymentApi'

usePageTitle('Processing Payment')

const route = useRoute()
const status = ref<'polling' | 'completed' | 'timeout'>('polling')
const error = ref<string | null>(null)
let timer: ReturnType<typeof setInterval> | null = null

// Poll: Wait for the webhook to complete the payment and auto-place the order.
async function poll(): Promise<void> {
  const orderId = typeof route.query.order === 'string' ? route.query.order : null
  if (!orderId) {
    status.value = 'timeout'
    error.value = 'Missing order reference. Please check your orders.'
    return
  }
  const result = await getPaymentStatus(orderId)
  // Debug: Trace the return-page poll in the browser console for local dev.
  console.debug(`[checkout/return] poll order=${orderId} isCompleted=${result.value.isCompleted}`)
  if (result.isSuccess && result.value.isCompleted) {
    status.value = 'completed'
    stopPolling()
  } else if (!result.isSuccess) {
    error.value = result.message ?? 'Could not read payment status.'
  }
}

function stopPolling(): void {
  if (timer) { clearInterval(timer); timer = null }
}

onMounted(() => {
  console.debug('[checkout/return] mounted, starting poll')
  void poll()
  let attempts = 0
  timer = setInterval(async () => {
    attempts += 1
    if (attempts > 30) { status.value = 'timeout'; stopPolling(); return }
    await poll()
  }, 2000)
})

onUnmounted(stopPolling)
</script>

<template>
  <div class="mx-auto max-w-xl space-y-5 px-4 py-8">
    <!-- Section: Page Header - title for the payment return surface -->
    <h1 class="text-2xl font-bold">Payment</h1>

    <!-- Section: Payment Status - polling, completion and timeout messages -->
    <Message v-if="status === 'polling'" severity="info" :closable="false">
      Confirming your payment…
    </Message>
    <Message v-if="status === 'completed'" severity="success" :closable="false">
      Your order has been placed. A confirmation email is on its way.
    </Message>
    <Message v-if="status === 'timeout'" severity="warn" :closable="false">
      We're still confirming your payment. {{ error ?? 'Check your orders in a moment.' }}
    </Message>

    <!-- Section: Action Footer - link to the customer's order list -->
    <Button as="router-link" to="/account/orders" label="View My Orders" icon="pi pi-receipt" />
  </div>
</template>
