<script setup lang="ts">
interface Props {
  rating: number
}

defineProps<Props>()
</script>

<template>
  <div class="flex items-center gap-2">
    <div class="bg-surface-100 flex items-center gap-1 p-1" style="border-radius: 30px">
      <i class="pi pi-star-fill text-yellow-500 text-sm" />
      <span class="text-surface-900 dark:text-surface-0 font-medium text-sm">{{ rating }}</span>
    </div>
  </div>
</template>
