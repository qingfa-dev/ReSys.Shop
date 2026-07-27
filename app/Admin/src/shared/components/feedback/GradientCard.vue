<script setup lang="ts">
interface Props {
  gradient?: string
  outerRadius?: string
  innerRadius?: string
}
const props = withDefaults(defineProps<Props>(), {
  gradient: 'linear-gradient(180deg, rgba(64,150,255,0.4) 0%, rgba(64,150,255,0) 100%)',
  outerRadius: '56px',
  innerRadius: '53px',
})
</script>

<template>
  <div class="border border-surface-200 dark:border-surface-700 rounded-border" :style="{ borderRadius: outerRadius, padding: '0.3rem', background: gradient }">
    <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20 flex flex-col items-center" :style="{ borderRadius: innerRadius }">
      <slot />
    </div>
  </div>
</template>
