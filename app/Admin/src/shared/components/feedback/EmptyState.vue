<script setup lang="ts">
interface Props {
  title: string
  description?: string
  icon?: string
  actionLabel?: string
}

withDefaults(defineProps<Props>(), {
  description: '',
  icon: 'pi pi-inbox',
})

const emit = defineEmits<{
  (e: 'action'): void
}>()
</script>

<template>
  <div class="card flex flex-col items-center justify-center py-12 gap-4">
    <i :class="icon" class="text-6xl text-surface-300 dark:text-surface-600" />
    <div class="text-xl font-semibold text-surface-600 dark:text-surface-300">{{ title }}</div>
    <p v-if="description" class="text-muted-color text-center max-w-md">{{ description }}</p>
    <Button v-if="actionLabel" :label="actionLabel" @click="emit('action')" />
    <slot />
  </div>
</template>
