<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

withDefaults(defineProps<{
  icon?: string
  title: string
  description?: string
  actionLabel?: string
  actionTo?: string
  actionIcon?: string
}>(), {
  icon: 'pi pi-inbox',
})

defineEmits<{
  action: []
}>()
</script>

<template>
  <div class="flex flex-col items-center justify-center py-20">
    <i :class="icon" class="mb-4 text-6xl opacity-20" style="color: var(--p-text-muted-color)" />
    <p class="text-xl font-medium">{{ title }}</p>
    <p v-if="description" class="text-sm mt-1 max-w-md text-center" style="color: var(--p-text-muted-color)">{{ description }}</p>
    <Button
      v-if="actionLabel"
      :label="actionLabel"
      :icon="actionIcon ?? 'pi pi-plus'"
      class="mt-6 rounded-xl"
      @click="actionTo ? router.push(actionTo) : $emit('action')"
    />
  </div>
</template>
