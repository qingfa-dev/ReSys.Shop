<template>
  <div class="rounded-border border border-surface-200 bg-white p-5 dark:border-surface-700 dark:bg-surface-900">
    <div class="flex items-start justify-between">
      <div>
        <p class="text-sm text-surface-500">{{ label }}</p>
        <p class="mt-1 text-2xl font-semibold text-surface-900 dark:text-surface-0">
          {{ value }}
        </p>
      </div>
      <div
        class="flex h-11 w-11 items-center justify-center rounded-full"
        :class="iconBg"
      >
        <i :class="[icon, iconColor]" class="text-lg" />
      </div>
    </div>
    <div v-if="delta !== undefined" class="mt-3 flex items-center gap-1 text-sm">
      <i :class="deltaIcon" />
      <span :class="deltaColor" class="font-medium">{{ formattedDelta }}</span>
      <span class="text-surface-400">vs last period</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  label: string;
  value: string | number;
  icon: string;
  color?: 'primary' | 'green' | 'orange' | 'red' | 'blue';
  delta?: number; // e.g. 12.4 or -3.2 (percent)
}>();

const palette: Record<string, { bg: string; text: string }> = {
  primary: { bg: 'bg-primary-100 dark:bg-primary-400/10', text: 'text-primary-600 dark:text-primary-400' },
  green: { bg: 'bg-green-100 dark:bg-green-400/10', text: 'text-green-600 dark:text-green-400' },
  orange: { bg: 'bg-orange-100 dark:bg-orange-400/10', text: 'text-orange-600 dark:text-orange-400' },
  red: { bg: 'bg-red-100 dark:bg-red-400/10', text: 'text-red-600 dark:text-red-400' },
  blue: { bg: 'bg-blue-100 dark:bg-blue-400/10', text: 'text-blue-600 dark:text-blue-400' },
};

const color = computed(() => props.color ?? 'primary')
const iconBg = computed(() => palette[color.value]!.bg)
const iconColor = computed(() => palette[color.value]!.text)

const deltaIcon = computed(() => (props.delta! >= 0 ? 'pi pi-arrow-up text-green-500' : 'pi pi-arrow-down text-red-500'));
const deltaColor = computed(() => (props.delta! >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'));
const formattedDelta = computed(() => `${props.delta! >= 0 ? '+' : ''}${props.delta}%`);
</script>
