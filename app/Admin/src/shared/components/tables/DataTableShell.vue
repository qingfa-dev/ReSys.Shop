<script setup lang="ts">
import { computed } from 'vue'
import type { RouteLocationRaw } from 'vue-router'
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable'
import { FilterMatchMode } from '@primevue/core/api'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'

export interface ColumnDef {
  field: string
  header: string
  sortable?: boolean
  filter?: boolean
  class?: string
  body?: (data: any) => string
}

const props = withDefaults(defineProps<{
  columns: ColumnDef[]
  value: any[]
  loading?: boolean
  totalRecords?: number
  rows?: number
  lazy?: boolean
  dataKey?: string
  sortField?: string
  sortOrder?: number
  emptyIcon?: string
  emptyTitle?: string
  emptyDescription?: string
  searchPlaceholder?: string
  showCreateButton?: boolean
  createRoute?: RouteLocationRaw
  createLabel?: string
  showExport?: boolean
  showClearFilters?: boolean
}>(), {
  loading: false,
  totalRecords: 0,
  rows: 10,
  lazy: true,
  dataKey: 'id',
  emptyIcon: 'pi-inbox',
  emptyTitle: 'No items found',
  searchPlaceholder: 'Search...',
  showCreateButton: true,
  showExport: false,
  showClearFilters: true,
})

const emit = defineEmits<{
  page: [event: DataTablePageEvent]
  sort: [event: DataTableSortEvent]
  filter: []
  refresh: []
  export: []
}>()

const filters = defineModel<DataTableFilterMeta>('filters')

const globalFilterValue = computed({
  get: () => (filters.value?.global as any)?.value ?? '',
  set: (val: string) => {
    if (!filters.value) filters.value = {} as DataTableFilterMeta
    filters.value.global = { value: val, matchMode: FilterMatchMode.CONTAINS }
  },
})

const skeletonRows = computed(() => Array.from({ length: props.rows }, (_, i) => ({ id: `sk-${i}` })))
</script>

<template>
  <DataTable
    v-model:filters="filters"
    :value="value"
    :loading="loading"
    :totalRecords="totalRecords"
    :lazy="lazy"
    :rows="rows"
    :sortField="sortField"
    :sortOrder="sortOrder"
    :dataKey="dataKey"
    :paginator="true"
    :rowsPerPageOptions="[5, 10, 20, 50]"
    @page="emit('page', $event)"
    @sort="emit('sort', $event)"
    @filter="emit('filter')"
    filterDisplay="menu"
    removableSort
    scrollable
    rowHover
    stripedRows
    showGridlines
    breakpoint="960px"
  >
    <template #header>
      <div class="flex flex-col items-center justify-between gap-4 md:flex-row">
        <IconField iconPosition="left" class="w-full md:w-72">
          <InputIcon class="pi pi-search" />
          <InputText
            v-model="globalFilterValue"
            :placeholder="searchPlaceholder"
            @keyup.enter="emit('filter')"
            class="w-full rounded-xl"
          />
        </IconField>

        <div class="flex items-center gap-2">
          <Button
            v-if="showClearFilters"
            type="button"
            icon="pi pi-filter-slash"
            label="Clear Filters"
            outlined
            @click="emit('filter')"
            class="rounded-border"
          />
          <Button
            v-if="showCreateButton && createRoute"
            :label="createLabel || 'Create'"
            icon="pi pi-plus"
            @click="$router.push(createRoute)"
            class="rounded-border"
          />
          <Button
            v-if="showExport"
            type="button"
            icon="pi pi-download"
            label="Export"
            severity="secondary"
            outlined
            @click="emit('export')"
            class="rounded-border"
          />
          <Button
            type="button"
            icon="pi pi-refresh"
            severity="secondary"
            outlined
            @click="emit('refresh')"
            class="rounded-border"
          />
          <slot name="toolbar-actions" />
        </div>
      </div>
    </template>

    <template #empty>
      <slot name="empty">
        <EmptyState
          :icon="emptyIcon"
          :title="emptyTitle"
          :description="emptyDescription"
        />
      </slot>
    </template>

    <template #loading>
      <div class="p-4 space-y-3">
        <Skeleton v-for="i in skeletonRows.length" :key="i" height="2.5rem" class="bg-surface-100 dark:bg-surface-800" />
      </div>
    </template>

    <Column
      v-for="col in columns"
      :key="col.field"
      :field="col.field"
      :header="col.header"
      :sortable="col.sortable ?? false"
      :filter="col.filter ?? false"
      :class="col.class"
    >
      <template v-if="col.body" #body="{ data }">
        {{ col.body(data) }}
      </template>
    </Column>

    <Column header="Actions" class="w-32 text-right" frozen alignFrozen="right">
      <template #body="{ data }">
        <div class="flex justify-end gap-1">
          <slot name="row-actions" :data="data" />
        </div>
      </template>
    </Column>
  </DataTable>
</template>
