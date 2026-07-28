<script setup lang="ts">
import Card from 'primevue/card'

interface Props {
  title: string
}

defineProps<Props>()
</script>

<template>
  <Card>
    <template #content>
      <div class="font-semibold text-xl mb-4">{{ title }}</div>
      <slot />
    </template>
  </Card>
</template>
