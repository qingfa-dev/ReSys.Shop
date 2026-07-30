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
      <div class="flex flex-col gap-6">
        <div>
          <div class="font-semibold text-xl">{{ title }}</div>
          <p v-if="description" class="text-muted-color mt-1">{{ description }}</p>
        </div>
        <div class="flex flex-col gap-4">
          <slot />
        </div>
      </div>
    </template>
  </Card>
</template>
