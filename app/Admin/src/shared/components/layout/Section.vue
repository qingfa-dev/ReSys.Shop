<script setup lang="ts">
defineProps<{
  title?: string
  description?: string
  collapsible?: boolean
  collapsed?: boolean
}>()

defineEmits<{
  'update:collapsed': [value: boolean]
}>()

defineSlots<{
  default(): any
  actions?(): any
}>()
</script>

<template>
  <section class="mb-8">
    <div v-if="title || $slots.actions" class="section-header flex items-center justify-between mb-4 pb-3 border-b" style="border-color: var(--p-surface-200)">
      <div class="flex items-center gap-3">
        <Button
          v-if="collapsible"
          :icon="collapsed ? 'pi pi-chevron-right' : 'pi pi-chevron-down'"
          text
          rounded
          size="small"
          severity="secondary"
          @click="$emit('update:collapsed', !collapsed)"
        />
        <div>
          <h3 v-if="title" class="text-lg font-semibold m-0" style="color: var(--p-text-color)">{{ title }}</h3>
          <p v-if="description" class="text-sm mt-1" style="color: var(--p-text-muted-color)">{{ description }}</p>
        </div>
      </div>
      <div v-if="$slots.actions" class="flex items-center gap-2">
        <slot name="actions" />
      </div>
    </div>
    <div v-show="!collapsible || !collapsed">
      <slot />
    </div>
  </section>
</template>
