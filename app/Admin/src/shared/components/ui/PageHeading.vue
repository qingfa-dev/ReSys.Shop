<script setup lang="ts">
import Card from 'primevue/card'
import AngleRight from '@primeicons/vue/angle-right'
interface BreadcrumbItem {
  label: string
  to?: string
}

interface StatItem {
  icon: string
  text: string
}

interface ActionItem {
  label: string
  icon?: string
  severity?: 'primary' | 'secondary' | 'info' | 'success' | 'warn' | 'danger' | 'help' | 'contrast'
}

interface Props {
  breadcrumbs?: BreadcrumbItem[]
  title: string
  stats?: StatItem[]
  actions?: ActionItem[]
}

withDefaults(defineProps<Props>(), {
  breadcrumbs: () => [],
  stats: () => [],
  actions: () => [],
})

const emit = defineEmits<{
  (e: 'action', index: number): void
}>()
</script>

<template>
  <Card class="mb-8">
    <div v-if="breadcrumbs.length" class="flex items-center gap-2 text-muted-color mb-4">
      <template v-for="(item, i) in breadcrumbs" :key="i">
        <router-link v-if="item.to" :to="item.to" class="hover:text-primary">{{ item.label }}</router-link>
        <span v-else>{{ item.label }}</span>
        <AngleRight v-if="i < breadcrumbs.length - 1" class="text-xs" />
      </template>
    </div>
    <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
      <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">{{ title }}</h1>
      <div v-if="actions.length" class="flex gap-2">
        <Button
          v-for="(action, i) in actions"
          :key="i"
          :label="action.label"
          :icon="action.icon"
          :severity="action.severity || 'secondary'"
          @click="emit('action', i)"
        />
      </div>
    </div>
    <div v-if="stats.length" class="flex gap-6 mt-4">
      <div v-for="(stat, i) in stats" :key="i" class="flex items-center gap-2">
        <i :class="stat.icon" class="text-primary" />
        <span class="text-surface-600 dark:text-surface-300">{{ stat.text }}</span>
      </div>
    </div>
  </Card>
</template>
