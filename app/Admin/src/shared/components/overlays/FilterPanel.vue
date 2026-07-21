<template>
  <Drawer :visible="visible" position="right" style="width: 22rem" @update:visible="emit('update:visible', $event)">
    <template #header>
      <p class="font-semibold text-surface-900 dark:text-surface-0">Filters</p>
    </template>

    <div class="flex flex-col gap-5">
      <slot />
    </div>

    <template #footer>
      <div class="flex gap-2 border-t border-surface-200 pt-4 dark:border-surface-700">
        <Button class="flex-1" label="Reset" severity="secondary" outlined @click="emit('reset')" />
        <Button class="flex-1" label="Apply" @click="emit('apply')" />
      </div>
    </template>
  </Drawer>
</template>

<script setup lang="ts">
import Drawer from 'primevue/drawer';
import Button from 'primevue/button';

defineProps<{ visible: boolean }>();
const emit = defineEmits<{ 'update:visible': [boolean]; apply: []; reset: [] }>();
</script>
