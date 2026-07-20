<script setup lang="ts">
withDefaults(defineProps<{
  label: string
  forId?: string
  required?: boolean
  error?: string
  hint?: string
}>(), {
  required: false,
})

defineSlots<{
  default(): any
}>()
</script>

<template>
  <div class="flex flex-col gap-2">
    <label
      v-if="forId"
      :for="forId"
      class="font-bold text-xs uppercase tracking-wider ml-1"
      style="color: var(--p-text-muted-color)"
    >
      {{ label }}
      <span v-if="required" style="color: var(--p-red-500)">*</span>
    </label>
    <label
      v-else
      class="font-bold text-xs uppercase tracking-wider ml-1"
      style="color: var(--p-text-muted-color)"
    >
      {{ label }}
      <span v-if="required" style="color: var(--p-red-500)">*</span>
    </label>
    <slot />
    <small v-if="error" class="p-error">{{ error }}</small>
    <small v-else-if="hint" style="color: var(--p-text-muted-color)">{{ hint }}</small>
  </div>
</template>
