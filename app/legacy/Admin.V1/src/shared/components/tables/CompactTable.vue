<script setup lang="ts">
import type { ColumnDef } from './DataTableShell.vue'

withDefaults(defineProps<{
  value: any[]
  columns: ColumnDef[]
  rows?: number
  dataKey?: string
  loading?: boolean
  scrollable?: boolean
  stripedRows?: boolean
}>(), {
  rows: 5,
  dataKey: 'id',
  loading: false,
  scrollable: true,
  stripedRows: false,
})
</script>

<template>
  <DataTable
    :value="value"
    :loading="loading"
    :rows="rows"
    :dataKey="dataKey"
    :scrollable="scrollable"
    :stripedRows="stripedRows"
    rowHover
    showGridlines
    class="rounded-border"
  >
    <Column
      v-for="col in columns"
      :key="col.field"
      :field="col.field"
      :header="col.header"
    >
      <template v-if="col.body" #body="{ data }">
        {{ col.body(data) }}
      </template>
    </Column>
  </DataTable>
</template>
