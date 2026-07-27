<script setup lang="ts">
interface Props {
  title: string
  description?: string
}

withDefaults(defineProps<Props>(), {
  description: '',
})
</script>

<template>
  <div class="card">
    <div class="font-semibold text-xl mb-4">{{ title }}</div>
    <p v-if="description" class="text-muted-color mb-4">{{ description }}</p>
    <slot />
  </div>
</template>
