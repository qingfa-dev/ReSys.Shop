<script setup lang="ts">
defineProps<{ variant: 'new' | 'sale' }>()
</script>
<template>
  <span
    class="absolute top-3 left-3 z-10 text-xs font-semibold uppercase tracking-wide rounded-full px-2.5 py-1"
    :class="variant === 'new' ? 'bg-teal-600 text-white' : 'bg-red-500 text-white'"
  >
    {{ variant === 'new' ? 'New' : 'Sale' }}
  </span>
</template>
