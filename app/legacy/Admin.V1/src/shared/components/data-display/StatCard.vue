<script setup lang="ts">
const props = withDefaults(defineProps<{
  title: string
  value: string | number
  icon: string
  iconBg: string
  trendLabel?: string
  trendValue?: number
  trendPositive?: boolean
  skeleton?: boolean
}>(), {
  skeleton: false,
})
</script>

<template>
  <div class="card !mb-0 flex flex-col gap-4 p-6 bg-surface-100 dark:bg-surface-800">
    <template v-if="skeleton">
      <Skeleton width="3rem" height="3rem" borderRadius="50%" />
      <Skeleton width="60%" height="2rem" />
      <Skeleton width="40%" height="1rem" />
    </template>
    <template v-else>
      <div class="flex items-center justify-between">
        <div :class="iconBg" class="flex items-center justify-center rounded-full" style="width: 3rem; height: 3rem">
          <i :class="icon" class="text-xl" />
        </div>
      </div>
      <span class="text-2xl font-black text-surface-900 dark:text-surface-0">{{ value }}</span>
      <div class="flex items-center gap-2">
        <span class="text-sm text-muted-color">{{ title }}</span>
        <template v-if="trendValue !== undefined">
          <i
            :class="trendPositive ? 'pi pi-arrow-up text-green-500' : 'pi pi-arrow-down text-red-500'"
            class="text-xs"
          />
          <span :class="trendPositive ? 'text-green-500' : 'text-red-500'" class="text-xs font-medium">
            {{ trendValue }}%
          </span>
        </template>
      </div>
    </template>
  </div>
</template>
