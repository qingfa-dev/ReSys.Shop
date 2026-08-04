<script setup lang="ts">
defineProps<{ score: number }>()

// Map: Color-code similarity score
function badgeClass(score: number): string {
  if (score >= 90) return 'bg-green-100 text-green-700'
  if (score >= 80) return 'bg-amber-100 text-amber-700'
  return 'bg-gray-100 text-gray-600'
}
</script>
<template>
  <span class="text-xs font-medium px-2 py-1 rounded-full" :class="badgeClass(score)">
    {{ Math.round(score) }}% match
  </span>
</template>
