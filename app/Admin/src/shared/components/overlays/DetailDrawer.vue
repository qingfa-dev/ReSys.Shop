<template>
  <Drawer
    :visible="visible"
    position="right"
    :style="{ width: width }"
    class="[&_.p-drawer-header]:border-b [&_.p-drawer-header]:border-surface-200 dark:[&_.p-drawer-header]:border-surface-700"
    @update:visible="emit('update:visible', $event)"
  >
    <template #header>
      <div>
        <p class="font-semibold text-surface-900 dark:text-surface-0">{{ title }}</p>
        <p v-if="subtitle" class="text-sm text-surface-400">{{ subtitle }}</p>
      </div>
    </template>

    <div class="flex h-full flex-col">
      <div class="flex-1 overflow-y-auto py-2">
        <slot />
      </div>
      <div v-if="$slots.footer" class="border-t border-surface-200 pt-4 dark:border-surface-700">
        <slot name="footer" />
      </div>
    </div>
  </Drawer>
</template>

<script setup lang="ts">
import Drawer from 'primevue/drawer';

withDefaults(
  defineProps<{ visible: boolean; title?: string; subtitle?: string; width?: string }>(),
  { width: '28rem' },
);

const emit = defineEmits<{ 'update:visible': [boolean] }>();
</script>
