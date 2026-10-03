<template>
  <Transition name="slide-up">
    <div v-if="count > 0"
      class="sticky bottom-4 z-10 mx-auto flex w-fit items-center gap-3 rounded-full border border-surface-200 bg-white px-4 py-2 shadow-lg dark:border-surface-700 dark:bg-surface-900">
      <span class="text-sm font-medium text-surface-700 dark:text-surface-200">
        {{ count }} selected
      </span>
      <Divider layout="vertical" class="!my-0" />
      <div class="flex items-center gap-1">
        <slot />
      </div>
      <Button icon="pi pi-times" text rounded size="small" severity="secondary" @click="emit('clear')" />
    </div>
  </Transition>
</template>

<script setup lang="ts">
import Button from 'primevue/button';
import Divider from 'primevue/divider';

defineProps<{ count: number }>();
const emit = defineEmits<{ clear: [] }>();
</script>

<style scoped>
.slide-up-enter-active,
.slide-up-leave-active {
  transition: all 0.2s ease;
}

.slide-up-enter-from,
.slide-up-leave-to {
  opacity: 0;
  transform: translateY(8px);
}
</style>
