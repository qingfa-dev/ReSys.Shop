<template>
  <DataTable
    :value="rows"
    :loading="loading"
    :paginator="true"
    :rows="pageSize"
    :total-records="total"
    :lazy="true"
    @page="onPage"
    @sort="onSort"
    striped-rows
  >
    <slot />
    <template #empty>
      <AppEmptyState message="No records found." />
    </template>
  </DataTable>
</template>

<script setup lang="ts" generic="TRow">
import { DEFAULT_PAGE_SIZE } from '@/shared/config/app'
import AppEmptyState from './AppEmptyState.vue'

withDefaults(
  defineProps<{
    rows: TRow[]
    total: number
    loading?: boolean
    pageSize?: number
  }>(),
  { loading: false, pageSize: DEFAULT_PAGE_SIZE },
)

const emit = defineEmits<{
  page: [event: { page: number; rows: number }]
  sort: [event: { sortField: string | ((item: TRow) => string) | undefined; sortOrder: 0 | 1 | -1 | null | undefined }]
}>()

function onPage(event: { page: number; rows: number }) {
  emit('page', event)
}
function onSort(event: { sortField: string | ((item: TRow) => string) | undefined; sortOrder: 0 | 1 | -1 | null | undefined }) {
  emit('sort', event)
}
</script>
