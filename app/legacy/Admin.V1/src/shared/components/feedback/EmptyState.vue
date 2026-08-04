<script setup lang="ts">
import { useRouter } from 'vue-router'
import type { RouteLocationRaw } from 'vue-router'

const router = useRouter()

const props = withDefaults(defineProps<{
  icon?: string
  title: string
  description?: string
  actionLabel?: string
  actionRoute?: RouteLocationRaw
}>(), {
  icon: 'pi pi-inbox',
})
</script>

<template>
  <div class="flex flex-col items-center justify-center py-20 text-muted-color">
    <i :class="icon" class="mb-4 text-6xl opacity-20" />
    <p class="text-xl font-medium">{{ title }}</p>
    <p v-if="description" class="text-sm mt-1 max-w-md text-center">{{ description }}</p>
    <Button
      v-if="actionLabel && actionRoute"
      :label="actionLabel"
      icon="pi pi-plus"
      class="mt-6 rounded-xl"
      @click="router.push(actionRoute)"
    />
  </div>
</template>
