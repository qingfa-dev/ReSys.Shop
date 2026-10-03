<script setup lang="ts">
defineProps<{
  saving?: boolean
}>()
</script>

<template>
  <div class="flex flex-col gap-4">
    <div v-if="$slots.header">
      <slot name="header" />
    </div>
    <div v-if="$slots.actions" class="flex items-center justify-end gap-2">
      <slot name="actions" />
    </div>
    <div class="flex flex-1 flex-col gap-4">
      <slot />
      <div v-if="$slots['sub-entities']">
        <slot name="sub-entities" />
      </div>
    </div>
    <div v-if="$slots.footer">
      <slot name="footer" :saving="saving" />
    </div>
  </div>
</template>
