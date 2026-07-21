<template>
  <Toolbar class="!border-0 !bg-transparent !p-0 mb-4">
    <template #start>
      <div class="flex items-center gap-2 flex-wrap">
        <IconField>
          <InputIcon class="pi pi-search" />
          <InputText v-model="search" :placeholder="searchPlaceholder" class="w-64" @update:model-value="onSearch" />
        </IconField>
        <Button v-if="showFilterButton" :label="activeFilterCount ? `Filters (${activeFilterCount})` : 'Filters'"
          icon="pi pi-filter" severity="secondary" outlined @click="emit('toggle-filters')" />
        <Button v-if="activeFilterCount" label="Clear" icon="pi pi-times" text severity="secondary"
          @click="emit('clear-filters')" />
      </div>
    </template>
    <template #end>
      <div class="flex items-center gap-2">
        <slot name="secondary-actions" />
        <Button v-if="createLabel" :label="createLabel" icon="pi pi-plus" @click="emit('create')" />
      </div>
    </template>
  </Toolbar>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import Toolbar from 'primevue/toolbar'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import { useDebounce } from '@/shared/composables/useDebounce'

withDefaults(
  defineProps<{
    searchPlaceholder?: string
    createLabel?: string
    showFilterButton?: boolean
    activeFilterCount?: number
  }>(),
  { searchPlaceholder: 'Search…', showFilterButton: true, activeFilterCount: 0 },
)

const emit = defineEmits<{
  search: [value: string]
  create: []
  'toggle-filters': []
  'clear-filters': []
}>()

const search = ref('')
const { debounced: onSearch } = useDebounce((value: string | undefined) => emit('search', value ?? ''), 350)
</script>
