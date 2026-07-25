<template>
  <div
    class="flex flex-col gap-1 border-b border-surface-200 dark:border-surface-700 pb-4 mb-5 sm:flex-row sm:items-center sm:justify-between">
    <div>
      <nav v-if="breadcrumb?.length" class="flex items-center gap-1.5 text-xs text-surface-500 mb-1">
        <template v-for="(item, i) in breadcrumb" :key="i">
          <router-link v-if="item.to" :to="item.to" class="hover:text-primary-600 transition-colors">
            {{ item.label }}
          </router-link>
          <span v-else>{{ item.label }}</span>
          <i v-if="i < breadcrumb.length - 1" class="pi pi-angle-right text-[10px]" />
        </template>
      </nav>
      <h1 class="text-xl font-semibold text-surface-900 dark:text-surface-0 leading-tight flex items-center gap-2">
        <i v-if="icon" :class="icon" />
        {{ title }}
      </h1>
      <p v-if="subtitle" class="text-sm text-surface-500 dark:text-surface-400">{{ subtitle }}</p>
    </div>
    <div class="flex items-center gap-2 shrink-0">
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup lang="ts">
interface BreadcrumbItem {
  label: string;
  to?: string;
}

defineProps<{
  title: string;
  subtitle: string;
  breadcrumb?: BreadcrumbItem[];
  icon?: string;
}>();
</script>
