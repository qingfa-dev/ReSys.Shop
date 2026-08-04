<script setup lang="ts">
import { ref, watch } from 'vue'
import Toolbar from 'primevue/toolbar'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

interface FilterChip {
  field: string
  value: string
  label: string
}

const props = withDefaults(
  defineProps<{
    searchPlaceholder?: string
    query?: string
    filters?: FilterChip[]
    showFilter?: boolean
    createLabel?: string
  }>(),
  {
    searchPlaceholder: 'Search...',
    showFilter: false,
    createLabel: '',
  },
)

const emit = defineEmits<{
  'update:query': [value: string]
  'update:filters': [value: FilterChip[]]
  create: []
  'toggle-filter': []
}>()

let debounceTimer: ReturnType<typeof setTimeout> | undefined
const searchInput = ref(props.query ?? '')

watch(() => props.query, (val) => {
  if (val !== searchInput.value) {
    searchInput.value = val ?? ''
  }
})

watch(searchInput, (val) => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    emit('update:query', val)
  }, 350)
})

function removeFilter(field: string) {
  emit('update:filters', props.filters?.filter(f => f.field !== field) ?? [])
}
</script>

<template>
  <Toolbar class="!border-0 !bg-transparent !p-0 mb-4">
    <template #start>
      <div class="flex items-center gap-2 flex-wrap">
        <IconField>
          <InputIcon class="pi pi-search" />
          <InputText v-model="searchInput" :placeholder="searchPlaceholder" class="w-64" />
        </IconField>
        <Button
          v-if="showFilter"
          :label="t('general.filters.label')"
          icon="pi pi-filter"
          :severity="showFilter && (filters?.length ?? 0) > 0 ? 'primary' : 'secondary'"
          :outlined="true"
          @click="emit('toggle-filter')"
        />
      </div>
    </template>
    <template #end>
      <div class="flex items-center gap-2">
        <slot name="secondary-actions" />
        <Button v-if="createLabel" :label="createLabel" icon="pi pi-plus" @click="emit('create')" />
      </div>
    </template>
  </Toolbar>
  <div v-if="filters && filters.length > 0" class="flex flex-wrap items-center gap-2 mb-4">
    <Tag
      v-for="filter in filters"
      :key="filter.field"
      :value="filter.label"
      severity="info"
      removable
      @remove="removeFilter(filter.field)"
    />
  </div>
</template>
