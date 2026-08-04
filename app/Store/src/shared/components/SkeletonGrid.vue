<script setup lang="ts">
import SkeletonCard from './SkeletonCard.vue'

defineProps<{ count?: number }>()
</script>
<template>
  <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
    <SkeletonCard v-for="i in (count ?? 8)" :key="i" />
  </div>
</template>
