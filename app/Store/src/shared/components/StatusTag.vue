<script setup lang="ts">
defineProps<{ status: string }>()
const colorMap: Record<string, string> = {
  pending: 'bg-amber-100 text-amber-700',
  confirmed: 'bg-blue-100 text-blue-700',
  shipped: 'bg-purple-100 text-purple-700',
  delivered: 'bg-green-100 text-green-700',
  cancelled: 'bg-red-100 text-red-700',
  in_stock: 'bg-green-100 text-green-700',
  low_stock: 'bg-amber-100 text-amber-700',
  out_of_stock: 'bg-red-100 text-red-700',
  // Order statuses (string-serialized OrderStatus enum)
  Draft: 'bg-gray-100 text-gray-600',
  Placed: 'bg-blue-100 text-blue-700',
  Canceled: 'bg-red-100 text-red-700',
  Expired: 'bg-amber-100 text-amber-700',
}
const cls = colorMap[status] ?? 'bg-gray-100 text-gray-600'
</script>
<template>
  <span class="text-xs font-medium px-2 py-0.5 rounded-full" :class="cls">{{ status.replace(/_/g, ' ') }}</span>
</template>
