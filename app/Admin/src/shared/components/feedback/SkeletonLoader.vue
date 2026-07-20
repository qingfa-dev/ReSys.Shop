<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  variant?: 'table' | 'card' | 'form' | 'detail' | 'list'
  rows?: number
}>(), {
  variant: 'table',
})

const effectiveRows = computed(() => {
  if (props.rows !== undefined) return props.rows
  if (props.variant === 'table') return 5
  if (props.variant === 'list') return 3
  return 0
})

const tableCols = 4
const cardCols = 2
const formFields = 4
const detailFields = 4
</script>

<template>
  <div class="card">
    <!-- Table variant -->
    <template v-if="variant === 'table'">
      <div class="flex items-center gap-2 mb-4">
        <Skeleton width="16rem" height="2.5rem" />
      </div>
      <div v-for="i in effectiveRows" :key="i" class="flex items-center gap-4 mb-3">
        <Skeleton v-for="j in tableCols" :key="j" height="1.5rem" class="flex-1" />
      </div>
      <div class="flex justify-between mt-4">
        <Skeleton width="10rem" height="2rem" />
        <Skeleton width="16rem" height="2rem" />
      </div>
    </template>

    <!-- Card variant -->
    <template v-if="variant === 'card'">
      <div class="grid grid-cols-2 gap-6">
        <div v-for="i in 4" :key="i">
          <Skeleton width="3rem" height="3rem" borderRadius="50%" class="mb-3" />
          <Skeleton width="60%" height="1.5rem" class="mb-2" />
          <Skeleton width="40%" height="1rem" />
        </div>
      </div>
    </template>

    <!-- Form variant -->
    <template v-if="variant === 'form'">
      <div v-for="i in formFields" :key="i" class="mb-4">
        <Skeleton width="6rem" height="1rem" class="mb-2" />
        <Skeleton height="2.5rem" />
      </div>
    </template>

    <!-- Detail variant -->
    <template v-if="variant === 'detail'">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div v-for="i in detailFields" :key="i">
          <Skeleton width="5rem" height="0.75rem" class="mb-2" />
          <Skeleton width="8rem" height="1.5rem" />
        </div>
      </div>
    </template>

    <!-- List variant -->
    <template v-if="variant === 'list'">
      <div v-for="i in effectiveRows" :key="i" class="flex items-center gap-3 mb-3 pb-3 border-b" style="border-color: var(--p-surface-100)">
        <Skeleton width="2.5rem" height="2.5rem" borderRadius="50%" />
        <div class="flex-1">
          <Skeleton width="60%" height="1rem" class="mb-1" />
          <Skeleton width="40%" height="0.75rem" />
        </div>
      </div>
    </template>
  </div>
</template>
