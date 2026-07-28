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
  <Card>
    <template #content>
      <div class="font-semibold text-xl mb-4">{{ title }}</div>
      <p v-if="description" class="text-muted-color mb-4">{{ description }}</p>
      <slot />
    </template>
  </Card>
</template>
