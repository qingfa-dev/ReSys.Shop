<script setup lang="ts">
import { ref } from 'vue'
import type { DataTableFilterMeta } from 'primevue'
import FilterSlash from '@primeicons/vue/filter-slash'
import Search from '@primeicons/vue/search'

interface ColumnDef {
  field: string
  header: string
  sortable?: boolean
  filter?: boolean
  filterField?: string
  bodyStyle?: string
  style?: string
}

interface Props {
  columns: ColumnDef[]
  data: any[]
  filters: DataTableFilterMeta
  loading?: boolean
  rows?: number
  paginator?: boolean
  globalFilterFields?: string[]
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  rows: 10,
  paginator: true,
  globalFilterFields: () => [],
})

const emit = defineEmits<{
  (e: 'update:filters', value: DataTableFilterMeta): void
}>()

const globalFilterValue = ref('')
const dt = ref()

const onGlobalFilterChange = (value: string | undefined) => {
  globalFilterValue.value = value ?? ''
}

const clearFilter = () => {
  emit('update:filters', { global: { value: null, matchMode: 'contains' } })
}

const exportCSV = () => {
  dt.value?.exportCSV()
}
</script>

<template>
  <DataTable
    ref="dt"
    :value="data"
    :paginator="paginator"
    :rows="rows"
    :filters="filters"
    :loading="loading"
    :globalFilterFields="globalFilterFields"
    filterDisplay="menu"
    dataKey="id"
    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
    :rowsPerPageOptions="[5, 10, 25]"
    currentPageReportTemplate="Showing {first} to {last} of {totalRecords} products"
  >
    <template #header>
      <div class="flex justify-between items-center">
        <Button type="button" label="Clear" outlined @click="clearFilter">
          <FilterSlash />
        </Button>
        <IconField>
          <InputIcon> <Search /> </InputIcon>
          <InputText v-model="globalFilterValue" placeholder="Search..." fluid @update:modelValue="onGlobalFilterChange" />
        </IconField>
      </div>
    </template>
    <Column v-for="col in columns" :key="col.field" :field="col.field" :header="col.header" :sortable="col.sortable" :filter="col.filter" :filterField="col.filterField || col.field" :bodyStyle="col.bodyStyle" :style="col.style">
      <template v-if="col.field" #body="slotProps">
        <slot :name="`body-${col.field}`" :data="slotProps.data" :field="col.field">
          {{ slotProps.data[col.field] }}
        </slot>
      </template>
    </Column>
    <template #empty>
      <slot name="empty">
        <div class="text-center py-8 text-muted-color">No records found.</div>
      </slot>
    </template>
    <template #loading>
      <slot name="loading">
        <div class="text-center py-8 text-muted-color">Loading...</div>
      </slot>
    </template>
  </DataTable>
</template>
