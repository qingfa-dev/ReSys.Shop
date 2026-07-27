<template>
  <component
    :is="props.as"
    class="rounded-border border border-surface-200 bg-white dark:border-surface-700 dark:bg-surface-900"
    :class="{
      'p-3': props.padding === 'sm',
      'p-5': props.padding === 'md',
      'p-6': props.padding === 'lg',
    }"
  >
    <slot />
  </component>
</template>

<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    padding?: 'sm' | 'md' | 'lg'
    as?: string
  }>(),
  {
    padding: 'md',
    as: 'div',
  },
)
</script>
