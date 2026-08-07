<script setup lang="ts">
import type { OrderTrackingResponse } from '../types/order'

const props = defineProps<{ tracking: OrderTrackingResponse }>()

// Format: Display date in human-readable format or dash if null.
function fmt(date: string | null): string {
  if (!date) return '—'
  return new Date(date).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const steps = [
  { label: 'Order Placed', date: props.tracking.orderCreatedAt },
  { label: 'Payment Completed', date: props.tracking.paymentCompletedAt },
  { label: 'Order Approved', date: props.tracking.orderApprovedAt },
  { label: 'Shipped', date: props.tracking.shippedAt },
  { label: 'Estimated Delivery', date: props.tracking.estimatedDeliveryAt },
  { label: 'Delivered', date: props.tracking.deliveredAt },
].filter((s) => s.date || s.label === 'Delivered')
</script>

<template>
  <!-- Section: Order Tracking Timeline -->
  <div class="space-y-4">
    <h3 class="text-lg font-semibold text-stone-900">Order Timeline</h3>
    <div class="relative ml-4">
      <div class="absolute left-0 top-0 bottom-0 w-0.5 bg-stone-200" />
      <div v-for="step in steps" :key="step.label" class="relative pl-8 pb-6 last:pb-0">
        <div
          class="absolute left-0 top-1 w-3 h-3 rounded-full -translate-x-1.5"
          :class="step.date ? 'bg-stone-900' : 'bg-stone-300 border-2 border-stone-400'"
        />
        <p class="text-sm font-medium" :class="step.date ? 'text-stone-900' : 'text-stone-400'">
          {{ step.label }}
        </p>
        <p class="text-xs text-stone-500">{{ fmt(step.date) }}</p>
      </div>
    </div>
  </div>
</template>
