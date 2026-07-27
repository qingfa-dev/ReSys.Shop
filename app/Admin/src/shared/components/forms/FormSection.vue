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
  <div class="card flex flex-col gap-4">
    <div>
      <div class="font-semibold text-xl">{{ title }}</div>
      <p v-if="description" class="text-muted-color mt-1">{{ description }}</p>
    </div>
    <slot />
  </div>
</template>
