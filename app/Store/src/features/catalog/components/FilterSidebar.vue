<script setup lang="ts">
import { computed } from 'vue'
import type { StoreOptionTypeResponse } from '../types/optionType'

const props = defineProps<{
  optionTypes: StoreOptionTypeResponse[]
  selectedIds: string[]
}>()
const emit = defineEmits<{ toggle: [id: string]; clear: [] }>()

// Map: Only render option types that are filterable
const filterableTypes = computed(() => props.optionTypes.filter(t => t.filterable))
</script>
<template>
  <!-- Section: Option Filters -->
  <div class="space-y-6">
    <!-- Section: Clear Action -->
    <div v-if="selectedIds.length > 0" class="flex justify-end">
      <Button label="Clear all" text severity="secondary" size="small" @click="emit('clear')" />
    </div>

    <!-- Section: Filter Groups -->
    <section v-for="type in filterableTypes" :key="type.id" class="space-y-2">
      <h3 class="text-sm font-semibold text-stone-900">{{ type.presentation ?? type.name }}</h3>
      <div class="space-y-1">
        <label
          v-for="value in type.values"
          :key="value.id"
          class="flex items-center gap-2 text-sm text-stone-700 cursor-pointer"
        >
          <input
            type="checkbox"
            class="rounded border-stone-300 text-stone-900 focus:ring-stone-900"
            :checked="selectedIds.includes(value.id)"
            @change="emit('toggle', value.id)"
          />
          {{ value.presentation ?? value.name }}
        </label>
      </div>
    </section>
  </div>
</template>
