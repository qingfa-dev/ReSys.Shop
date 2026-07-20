<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { useOptionTypeStore } from '../stores/option-type.store'
import { storeToRefs } from 'pinia'
import { FilterMatchMode, FilterOperator } from '@primevue/core/api'
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable'
import { getFilterValue } from '@/common/api/types/filter.types'
import { useToast } from '@/common/composables/toast.use'
import { useConfirm } from 'primevue/useconfirm'
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'
import { QueryBuilder } from '@/common/utils/query-builder.utils'
import type { OptionTypeListItem } from '../types/option-type.response.type'

const { t } = useI18n()
const router = useRouter()
const store = useOptionTypeStore()
const { items, loading, totalRecords, params: query } = storeToRefs(store)
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
    pageSize: event.rows
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
  const globalValue = getFilterValue(filters.value, 'global') as string | null
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
    search: globalValue || undefined,
    searchFields: globalValue ? ['Name', 'Presentation'] : undefined,
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
  router.push({ name: 'catalog.option-types.create' })
}

const editItem = (id: string) => {
  router.push({ name: 'catalog.option-types.edit', params: { id } })
}

const confirmDelete = (item: OptionTypeListItem) => {
  confirm.require({
    message: `Are you sure you want to delete "${item.name}"?`,
    header: t('common.warning'),
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: t('catalog.option_types.actions.cancel'),
    acceptLabel: t('catalog.option_types.actions.delete'),
    acceptProps: { severity: 'danger' },
    accept: async () => {
      const result = await store.remove(item.id)
      if (result.isSuccess) {
        showToast('success', t('common.success'), t('catalog.option_types.messages.delete_success'))
      } else {
        showToast('error', t('common.error'), t('catalog.option_types.messages.delete_error'))
      }
    }
  })
}

onMounted(() => {
  loadItems()
})
</script>

<template>
  <PageShell maxWidth="7xl">
    <PageHeader
      :title="t('catalog.option_types.titles.list')"
      :description="t('catalog.option_types.descriptions.list')"
    >
      <template #badge>
        <Badge :value="totalRecords" severity="info" />
      </template>
      <template #actions>
        <Button
          :label="t('catalog.option_types.actions.create')"
          icon="pi pi-plus"
          @click="createItem"
          class="px-4 shadow-lg rounded-xl"
        />
      </template>
    </PageHeader>

    <DataTable
        v-model:filters="filters"
        :value="items"
        :loading="loading"
        :totalRecords="totalRecords"
        lazy
        paginator
        :rows="query.pageSize"
        :first="((query.page || 1) - 1) * (query.pageSize || 10)"
        @page="onPage"
        @sort="onSort"
        @filter="onFilter"
        filterDisplay="menu"
        removableSort
        scrollable
        rowHover
      >
        <template #header>
          <div class="flex flex-col items-center justify-between gap-4 md:flex-row">
            <IconField iconPosition="left" class="w-full md:w-72">
              <InputIcon class="pi pi-search" />
              <InputText
                v-model="(filters.global as { value: string | null }).value"
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
            <p class="text-xl font-medium">{{ t('catalog.option_types.messages.empty_list') }}</p>
          </div>
        </template>

        <Column field="name" :header="t('catalog.option_types.table.name')" sortable>
          <template #body="{ data }">
            <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
          </template>
          <template #filter="{ filterModel, filterCallback }">
            <InputText v-model="filterModel.value" type="text" @keydown.enter="filterCallback()" class="p-column-filter" placeholder="Search by name" />
          </template>
        </Column>

        <Column field="presentation" :header="t('catalog.option_types.table.presentation')" sortable>
           <template #filter="{ filterModel, filterCallback }">
            <InputText v-model="filterModel.value" type="text" @keydown.enter="filterCallback()" class="p-column-filter" placeholder="Search by display name" />
          </template>
        </Column>

        <Column field="position" :header="t('catalog.option_types.table.position')" sortable class="w-24 text-center">
            <template #body="{ data }">
                <Badge :value="data.position" severity="secondary" />
            </template>
        </Column>

        <Column field="filterable" :header="t('catalog.option_types.table.filterable')" sortable dataType="boolean" class="w-32 text-center">
          <template #body="{ data }">
            <i class="pi" :class="{'pi-check-circle text-green-500': data.filterable, 'pi-times-circle text-surface-400': !data.filterable}"></i>
          </template>
        </Column>

        <Column class="w-48 text-right" frozen alignFrozen="right">
          <template #body="{ data }">
            <div class="flex justify-end gap-1">
              <Button
                icon="pi pi-list"
                severity="info"
                text
                rounded
                v-tooltip.top="'Manage Values'"
                @click="router.push({ name: 'catalog.option-values.list', query: { optionTypeId: data.id } })"
              />
              <Button icon="pi pi-pencil" severity="secondary" text rounded @click="editItem(data.id)" />
              <Button icon="pi pi-trash" severity="danger" text rounded @click="confirmDelete(data)" />
            </div>
          </template>
        </Column>
      </DataTable>
  </PageShell>
</template>
