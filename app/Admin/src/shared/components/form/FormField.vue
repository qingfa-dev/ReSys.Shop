<script setup lang="ts">
withDefaults(defineProps<{
  label: string
  name: string
  error?: string
  required?: boolean
  hint?: string
}>(), {
  required: false,
})

defineSlots<{
  default(): any
  description(): any
}>()
</script>

<template>
  <div class="flex flex-col gap-2">
    <div class="flex items-center gap-2">
      <label
        :for="name"
        class="font-bold text-xs uppercase tracking-wider text-muted-color ml-1"
      >
        {{ label }}
        <span v-if="required" class="text-red-500">*</span>
      </label>
      <slot name="description" />
    </div>
    <slot />
    <small v-if="error" class="p-error">{{ error }}</small>
    <small v-else-if="hint" class="text-muted-color">{{ hint }}</small>
  </div>
</template>
