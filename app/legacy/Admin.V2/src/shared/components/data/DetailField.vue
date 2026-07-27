<template>
  <div class="flex flex-col gap-1 py-2">
    <dt class="text-xs font-medium uppercase tracking-wide text-surface-400">{{ label }}</dt>
    <dd class="text-sm text-surface-900 dark:text-surface-0">
      <slot>{{ value ?? '—' }}</slot>
    </dd>
  </div>
</template>

<script setup lang="ts">
defineProps<{ label: string; value?: string | number | null }>();
</script>
