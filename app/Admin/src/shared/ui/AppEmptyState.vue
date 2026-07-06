<template>
  <div class="flex flex-col items-center justify-center gap-2 p-8 text-center text-color-secondary">
    <i :class="icon ?? 'pi pi-inbox'" class="text-3xl" />
    <p class="font-medium">{{ message }}</p>
    <slot />
  </div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ message: string; icon?: string }>(), { icon: 'pi pi-inbox' })
</script>
