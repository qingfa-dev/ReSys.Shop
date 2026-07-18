<script setup lang="ts">
withDefaults(defineProps<{
  maxWidth?: '2xl' | '4xl' | '6xl' | '7xl' | 'none'
  card?: boolean
  gap?: boolean
}>(), {
  maxWidth: 'none',
  card: true,
  gap: false,
})

const MAX_WIDTH_CLASS: Record<string, string> = {
  '2xl': 'max-w-2xl mx-auto',
  '4xl': 'max-w-4xl mx-auto',
  '6xl': 'max-w-6xl mx-auto',
  '7xl': 'max-w-7xl mx-auto',
}
</script>

<template>
  <div
    class="p-6"
    :class="[
      MAX_WIDTH_CLASS[maxWidth] || '',
      !card && gap ? 'flex flex-col gap-6' : '',
    ]"
  >
    <Card v-if="card">
      <template #content>
        <slot />
      </template>
    </Card>
    <slot v-else />
  </div>
</template>
