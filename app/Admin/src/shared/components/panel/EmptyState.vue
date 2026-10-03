<script setup lang="ts">
import { type Component } from 'vue'
import Card from 'primevue/card'
import Inbox from '@primeicons/vue/inbox'

interface Props {
  title: string
  description?: string
  icon?: Component
  actionLabel?: string
}

withDefaults(defineProps<Props>(), {
  description: '',
  icon: () => Inbox,
})

const emit = defineEmits<{
  (e: 'action'): void
}>()
</script>

<template>
  <Card class="flex flex-col items-center justify-center py-12 gap-4">
    <template #content>
      <component :is="icon" class="text-6xl text-surface-300 dark:text-surface-600" />
      <div class="text-xl font-semibold text-surface-600 dark:text-surface-300">{{ title }}</div>
      <p v-if="description" class="text-muted-color text-center max-w-md">{{ description }}</p>
      <Button v-if="actionLabel" :label="actionLabel" @click="emit('action')" />
      <slot />
    </template>
  </Card>
</template>
