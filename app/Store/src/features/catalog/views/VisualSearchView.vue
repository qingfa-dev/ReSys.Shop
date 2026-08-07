<script setup lang="ts">
import { onMounted } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useVisualSearchStore } from '../stores/visualSearchStore'

usePageTitle('Visual Search')
const vs = useVisualSearchStore()
onMounted(() => vs.loadModels())
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Visual Search' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Visual Search</h1>
    <Card>
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2rem" />
          <Skeleton width="75%" height="1rem" />
        </div>
      </template>
    </Card>
    <Message severity="info" class="mt-4">
      Image upload drop zone, model selector, search button, and results grid will be implemented here.
    </Message>
  </div>
</template>
