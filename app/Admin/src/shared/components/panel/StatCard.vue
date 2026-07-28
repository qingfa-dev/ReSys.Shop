<script setup lang="ts">
import Card from 'primevue/card'

interface Props {
  label: string
  value: string | number
  icon: string
  iconBgClass?: string
  subText?: string
  trend?: string
}

withDefaults(defineProps<Props>(), {
  iconBgClass: 'bg-indigo-500',
})
</script>

<template>
  <div class="col-span-12 lg:col-span-6 xl:col-span-3">
    <Card class="mb-0">
      <div class="flex justify-between mb-4">
        <div>
          <span class="block text-muted-color font-medium mb-4">{{ label }}</span>
          <div class="text-surface-900 dark:text-surface-0 font-medium text-xl">{{ value }}</div>
        </div>
        <div :class="['flex justify-center items-center rounded-full', iconBgClass]" style="width: 2.5rem; height: 2.5rem">
          <i :class="icon" class="text-white text-xl" />
        </div>
      </div>
      <span v-if="subText" class="text-muted-color">{{ subText }}</span>
      <span v-if="trend" class="text-primary font-medium">{{ trend }}</span>
    </Card>
  </div>
</template>
