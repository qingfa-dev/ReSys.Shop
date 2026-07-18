<script setup lang="ts">
import { useRouter } from 'vue-router'
import type { VNode } from 'vue'

const router = useRouter()

defineProps<{
  title: string
  description?: string
  back?: boolean
}>()

defineSlots<{
  badge?(): VNode[]
  actions?(): VNode[]
}>()

function goBack() {
  router.back()
}
</script>

<template>
  <div class="flex flex-col items-start justify-between gap-4 mb-8 md:flex-row md:items-center">
    <div class="flex items-center gap-4">
      <Button
        v-if="back"
        icon="pi pi-arrow-left"
        text
        rounded
        severity="secondary"
        @click="goBack"
        class="shrink-0 bg-surface-100 dark:bg-surface-800"
      />
      <div>
        <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50 m-0">
          {{ title }}
        </h2>
        <div v-if="description || $slots.badge" class="flex items-center gap-2 mt-1">
          <span v-if="description" class="text-surface-500 dark:text-surface-400">{{ description }}</span>
          <slot name="badge" />
        </div>
      </div>
    </div>
    <div v-if="$slots.actions" class="flex items-center gap-3 shrink-0">
      <slot name="actions" />
    </div>
  </div>
</template>
