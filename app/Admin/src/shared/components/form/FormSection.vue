<script setup lang="ts">
import Card from 'primevue/card'

interface Props {
  title: string
  description?: string
}

withDefaults(defineProps<Props>(), {
  description: '',
})
</script>

<template>
  <Card class="flex flex-col gap-4">
    <div>
      <div class="font-semibold text-xl">{{ title }}</div>
      <p v-if="description" class="text-muted-color mt-1">{{ description }}</p>
    </div>
    <slot />
  </Card>
</template>
