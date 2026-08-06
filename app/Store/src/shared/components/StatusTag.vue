<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{ status: string }>()

const severity = computed(() => {
  const map: Record<string, string> = {
    Placed: 'info',
    Shipped: 'warn',
    Delivered: 'success',
    Canceled: 'danger',
    Draft: 'secondary',
    Expired: 'secondary',
    in_stock: 'success',
    low_stock: 'warn',
    out_of_stock: 'danger',
  }
  return map[props.status] ?? 'secondary'
})
</script>
<template>
  <Tag :severity="severity" :value="status" />
</template>
