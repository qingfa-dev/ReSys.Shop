<template>
  <div class="flex flex-col gap-1">
    <label v-if="label" :for="inputId" class="text-sm font-medium">{{ label }}</label>
    <slot :id="inputId" :invalid="!!error" />
    <small v-if="error" class="text-red-600">{{ error }}</small>
    <small v-else-if="hint" class="text-color-secondary">{{ hint }}</small>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  label?: string
  error?: string
  hint?: string
  id?: string
}>()

const inputId = computed(() => props.id ?? `field-${Math.random().toString(36).slice(2, 8)}`)
</script>
