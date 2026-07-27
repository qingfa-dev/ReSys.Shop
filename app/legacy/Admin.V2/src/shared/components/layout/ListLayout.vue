<script setup lang="ts">
</script>

<template>
  <div class="flex flex-col gap-4">
    <div v-if="$slots.header">
      <slot name="header" />
    </div>
    <div v-if="$slots.toolbar">
      <slot name="toolbar" />
    </div>
    <div class="flex flex-1 flex-col">
      <slot />
    </div>
    <div v-if="$slots['bulk-actions']">
      <slot name="bulk-actions" />
    </div>
  </div>
</template>
