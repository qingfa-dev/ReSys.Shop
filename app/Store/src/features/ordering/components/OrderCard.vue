<script setup lang="ts">
import { useConfirm } from 'primevue/useconfirm'
import StatusTag from '@/shared/components/StatusTag.vue'
import { formatVnd } from '@/shared/utils/currency'
import { formatDateTimeUtc } from '@/shared/utils/date'
import type { OrderListItem } from '../types/order'
import { isOrderCancellable } from '../types/order'

const props = defineProps<{ order: OrderListItem }>()
const emit = defineEmits<{ cancel: [id: string] }>()
const confirm = useConfirm()

function requestCancel(): void {
  confirm.require({
    message: `Cancel order #${props.order.number}? This cannot be undone.`,
    header: 'Cancel Order',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Keep Order',
    acceptLabel: 'Cancel Order',
    acceptClass: 'p-button-danger',
    accept: () => emit('cancel', props.order.id),
  })
}
</script>
<template>
  <!-- Section: Order Card -->
  <div class="bg-white rounded-xl border border-gray-200 p-6">
    <div class="flex flex-wrap items-center justify-between gap-4">
      <div>
        <div class="flex items-center gap-3">
          <h3 class="text-base font-semibold text-gray-900">#{{ order.number }}</h3>
          <StatusTag :status="order.status" />
        </div>
        <p class="text-sm text-gray-500 mt-1">{{ formatDateTimeUtc(order.createdAtUtc) }}</p>
      </div>
      <div class="text-right">
        <p class="text-sm text-gray-500">Total</p>
        <p class="text-lg font-bold text-gray-900">{{ formatVnd(order.total) }}</p>
      </div>
    </div>
    <div class="mt-4 flex flex-wrap items-center justify-end gap-3">
      <router-link :to="`/account/orders/${order.id}`">
        <Button label="View Details" severity="secondary" outlined size="small" icon="pi pi-eye" />
      </router-link>
      <Button
        v-if="isOrderCancellable(order.status)"
        label="Cancel Order"
        severity="danger"
        outlined
        size="small"
        icon="pi pi-times"
        @click="requestCancel"
      />
    </div>
  </div>
</template>
