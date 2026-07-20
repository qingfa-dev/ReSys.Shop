<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

defineProps<{
  title: string
  description?: string
  backTo?: string
  backLabel?: string
}>()

defineSlots<{
  default?(): any
  actions?(): any
}>()

function goBack(backTo?: string) {
  if (backTo) {
    router.push(backTo)
  } else {
    router.back()
  }
}
</script>

<template>
  <div class="flex flex-col items-start justify-between gap-4 mb-8 md:flex-row md:items-center">
    <div class="flex items-center gap-4">
      <Button
        v-if="backTo !== undefined"
        :icon="backLabel ? undefined : 'pi pi-arrow-left'"
        :label="backLabel"
        text
        rounded
        severity="secondary"
        @click="goBack(backTo)"
        class="shrink-0"
        style="background: var(--p-surface-100)"
      />
      <div>
        <h2 class="text-3xl font-black tracking-tight m-0" style="color: var(--p-text-color)">
          {{ title }}
        </h2>
        <div v-if="description || $slots.default" class="flex items-center gap-2 mt-1">
          <span v-if="description" style="color: var(--p-text-muted-color)">{{ description }}</span>
          <slot />
        </div>
      </div>
    </div>
    <div v-if="$slots.actions" class="flex items-center gap-3 shrink-0">
      <slot name="actions" />
    </div>
  </div>
</template>
