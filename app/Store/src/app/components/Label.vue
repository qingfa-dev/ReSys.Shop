<script setup lang="ts">
// Label: Thin wrapper around native <label> for PrimeVue FloatLabel integration.
defineProps<{
  for?: string
}>()
</script>

<template>
  <label :for="for" class="text-sm font-medium text-heading">
    <slot />
  </label>
</template>
