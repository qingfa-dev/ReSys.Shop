<script setup lang="ts">
defineProps<{ variant: 'new' | 'sale' }>()
</script>
<template>
  <Tag :severity="variant === 'new' ? 'success' : 'danger'" :value="variant === 'new' ? 'New' : 'Sale'" class="absolute top-3 left-3 z-10" />
</template>
