<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { usePropertyTypeStore } from '../stores/property-type.store'
import { storeToRefs } from 'pinia'
import { propertyTypeLocales } from '../locales/property-type.locales'
import { FilterMatchMode, FilterOperator } from '@primevue/core/api'
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable'
import { useToast } from '@/shared/composables/toast.use'
import { useConfirm } from 'primevue/useconfirm'
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue'
import { QueryBuilder } from '@/shared/utils/query-builder.utils'
import type { PropertyTypeListItem } from '../types/property-type.types'
import { PropertyKindOptions } from '../types/property-kind'

const router = useRouter()
const store = usePropertyTypeStore()
const { items, loading, totalRecords, query } = storeToRefs(store)
const { showToast } = useToast()
const confirm = useConfirm()

const filters = ref<DataTableFilterMeta>({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  name: { operator: FilterOperator.AND, constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }] },
  presentation: { operator: FilterOperator.AND, constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }] },
})

const loadItems = async () => {
  await store.fetchList()
}

const onPage = (event: DataTablePageEvent) => {
  store.fetchList({
    page: event.page !== undefined ? event.page + 1 : 1,
    page_size: event.rows
  })
}

const onSort = (event: DataTableSortEvent) => {
  const builder = new QueryBuilder()
  if (event.sortField) {
    builder.orderBy(event.sortField as string, event.sortOrder === -1 ? 'desc' : 'asc')
  }
  store.fetchList({ sort: builder.build().sort, page: 1 })
}

const onFilter = () => {
  const globalFilter = filters.value.global as { value: string | null }
  const nameFilter = filters.value.name as { constraints: { value: string | null }[] }
  const presentationFilter = filters.value.presentation as { constraints: { value: string | null }[] }

  const builder = new QueryBuilder()
  
  if (nameFilter.constraints[0]?.value) {
    builder.where('Name', '*', nameFilter.constraints[0].value)
  }
  
  if (presentationFilter.constraints[0]?.value) {
    builder.where('Presentation', '*', presentationFilter.constraints[0].value)
  }

  const built = builder.build()
  
  store.fetchList({
    search: globalFilter.value || undefined,
    search_field: globalFilter.value ? ['Name', 'Presentation'] : undefined,
    filter: built.filter,
    page: 1
  })
}

const clearFilters = () => {
  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    name: { operator: FilterOperator.AND, constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }] },
    presentation: { operator: FilterOperator.AND, constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }] },
  }
  onFilter()
}

const createItem = () => {
  router.push({ name: 'catalog.property-types.create' })
}

const editItem = (id: string) => {
  router.push({ name: 'catalog.property-types.edit', params: { id } })
}

const confirmDelete = (item: PropertyTypeListItem) => {
  confirm.require({
    message: `Are you sure you want to delete "${item.name}"?`,
    header: propertyTypeLocales.common?.warning || 'Warning',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: propertyTypeLocales.actions?.cancel,
    acceptLabel: propertyTypeLocales.actions?.delete,
    acceptProps: { severity: 'danger' },
    accept: async () => {
      const result = await store.remove(item.id)
      if (result.success) {
        showToast('success', propertyTypeLocales.common?.success || 'Success', propertyTypeLocales.messages?.delete_success || 'Deleted')
      } else {
        showToast('error', propertyTypeLocales.common?.error || 'Error', 'Failed to delete property type')
      }
    }
  })
}

const getKindLabel = (kind: number) => {
    return PropertyKindOptions.find(o => o.value === kind)?.label || 'Unknown'
}

onMounted(() => {
  loadItems()
})
</script>

<template>
  <Card>
    <template #content>
      <AppBreadcrumb :locales="propertyTypeLocales" />
    
    <div class="flex flex-col items-start justify-between gap-4 mb-8 md:flex-row md:items-center">
      <div>
        <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
          {{ propertyTypeLocales.titles?.list }}
        </h2>
        <div class="flex items-center gap-2 mt-1">
          <span class="text-surface-500 dark:text-surface-400">
            {{ propertyTypeLocales.descriptions?.list }}
          </span>
          <Badge :value="totalRecords" severity="info" class="ml-2" />
        </div>
      </div>
      <Button 
        :label="propertyTypeLocales.actions?.create" 
        icon="pi pi-plus" 
        @click="createItem"
        class="px-4 shadow-lg rounded-xl"
      />
    </div>

    <div class="overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-2xl border-surface-100 dark:border-surface-800">
      <DataTable
        v-model:filters="filters"
        :value="items"
        :loading="loading"
        :totalRecords="totalRecords"
        lazy
        paginator
        :rows="query.page_size"
        :first="((query.page || 1) - 1) * (query.page_size || 10)"
        @page="onPage"
        @sort="onSort"
        @filter="onFilter"
        filterDisplay="menu"
        removableSort
        scrollable
        rowHover
        stripedRows
        showGridlines
      >
        <template #header>
          <div class="flex flex-col items-center justify-between gap-4 md:flex-row">
            <IconField iconPosition="left" class="w-full md:w-72">
              <InputIcon class="pi pi-search" />
              <InputText 
                v-model="(filters.global as any).value" 
                placeholder="Search..." 
                @keyup.enter="onFilter" 
                class="w-full rounded-xl"
              />
            </IconField>
            <Button 
              type="button" 
              icon="pi pi-filter-slash" 
              label="Clear" 
              outlined 
              @click="clearFilters" 
              class="w-full rounded-xl md:w-auto"
            />
          </div>
        </template>

        <template #empty>
          <div class="flex flex-col items-center justify-center py-20 text-surface-400">
            <i class="mb-4 text-6xl pi pi-box opacity-20"></i>
            <p class="text-xl font-medium">{{ propertyTypeLocales.messages?.empty_list }}</p>
          </div>
        </template>

        <Column field="name" :header="propertyTypeLocales.table?.name" sortable>
          <template #body="{ data }">
            <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
          </template>
        </Column>

        <Column field="presentation" :header="propertyTypeLocales.table?.presentation" sortable></Column>

        <Column field="kind" :header="propertyTypeLocales.table?.kind" sortable>
            <template #body="{ data }">
                <Tag :value="getKindLabel(data.kind)" severity="info" />
            </template>
        </Column>

        <Column field="position" :header="propertyTypeLocales.table?.position" sortable class="w-24 text-center">
            <template #body="{ data }">
                <Badge :value="data.position" severity="secondary" />
            </template>
        </Column>

        <Column field="filterable" :header="propertyTypeLocales.table?.filterable" sortable dataType="boolean" class="w-32 text-center">
          <template #body="{ data }">
            <i class="pi" :class="{'pi-check-circle text-green-500': data.filterable, 'pi-times-circle text-surface-400': !data.filterable}"></i>
          </template>
        </Column>

        <Column class="w-32 text-right" frozen alignFrozen="right">
          <template #body="{ data }">
            <div class="flex justify-end gap-1">
              <Button icon="pi pi-pencil" severity="secondary" text rounded @click="editItem(data.id)" />
              <Button icon="pi pi-trash" severity="danger" text rounded @click="confirmDelete(data)" />
            </div>
          </template>
        </Column>
      </DataTable>
    </div>
  </template>
</Card>
</template>

<style scoped>
:deep(.p-datatable-header) {
  background: transparent;
  padding: 1rem;
}
:deep(.p-datatable-thead > tr > th) {
  background: var(--p-content-background);
  color: var(--p-text-color);
  font-size: 0.875rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.025em;
  padding: 1rem 1.5rem;
  border-bottom: 2px solid var(--p-primary-color);
}
:deep(.p-datatable-tbody > tr > td) {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--p-content-border-color);
}
</style>
