<template>
  <div class="rounded-border border border-surface-200 dark:border-surface-700 overflow-hidden">
    <DataTable
      :value="rows"
      :loading="loading"
      :total-records="totalRecords"
      :rows="pageSize"
      :first="first"
      lazy
      paginator
      data-key="id"
      selection-mode="multiple"
      :selection="selection"
      striped-rows
      responsive-layout="scroll"
      :rows-per-page-options="[10, 20, 50]"
      class="p-datatable-sm"
      @page="onPage"
      @sort="onSort"
      @update:selection="emit('update:selection', $event)"
    >
      <template #empty>
        <EmptyState :title="emptyTitle" :description="emptyDescription" :icon="emptyIcon">
          <template v-if="$slots['empty-actions']" #actions>
            <slot name="empty-actions" />
          </template>
        </EmptyState>
      </template>
      <template #loading>
        <LoadingSkeleton :rows="pageSize" :columns="columnCount" />
      </template>

      <Column v-if="selectable" selection-mode="multiple" header-style="width: 3rem" />

      <slot />

      <Column v-if="$slots.rowActions" header-style="width: 3rem" body-style="text-align:center">
        <template #body="slotProps">
          <slot name="rowActions" :data="slotProps.data" />
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import { computed, useSlots } from 'vue'
import DataTable from 'primevue/datatable'
import type { DataTableSortEvent } from 'primevue/datatable'
import Column from 'primevue/column'
import EmptyState from '@/shared/components/feedback/EmptyState.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'

interface Props {
  rows: unknown[];
  loading?: boolean;
  totalRecords?: number;
  pageSize?: number;
  first?: number;
  selectable?: boolean;
  selection?: unknown[];
  emptyTitle?: string;
  emptyDescription?: string;
  emptyIcon?: string;
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  totalRecords: 0,
  pageSize: 20,
  first: 0,
  selectable: false,
  emptyTitle: 'No records found',
  emptyDescription: 'Try adjusting your search or filters.',
  emptyIcon: 'pi pi-inbox',
});

const emit = defineEmits<{
  page: [{ page: number; rows: number }]
  sort: [event: DataTableSortEvent]
  'update:selection': [unknown[]]
}>();

const slots = useSlots();
// rough column count for skeleton width — counts default-slot Column children
const columnCount = computed(() => (slots.default?.().length ?? 4) + (props.selectable ? 1 : 0));

function onPage(e: { page: number; rows: number }) {
  emit('page', e);
}
function onSort(e: DataTableSortEvent) {
  emit('sort', e)
}
</script>
