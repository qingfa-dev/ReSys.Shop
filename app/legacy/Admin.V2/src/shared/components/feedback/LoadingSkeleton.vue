<template>
  <div class="flex flex-col gap-3 p-4">
    <div v-for="r in rows" :key="r" class="flex items-center gap-4">
      <Skeleton v-for="c in columns" :key="c" height="1.25rem" :width="c === 1 ? '2rem' : undefined" class="flex-1" />
    </div>
  </div>
</template>

<script setup lang="ts">
import Skeleton from 'primevue/skeleton';

withDefaults(defineProps<{ rows?: number; columns?: number }>(), { rows: 5, columns: 4 });
</script>
