<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import { PageShell } from '@panel'
import { CrudToolbar, FilterableDataTable } from '@data'

interface ColumnDef {
  field: string
  header: string
  sortable?: boolean
  filter?: boolean
  filterField?: string
  bodyStyle?: string
  style?: string
}
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { CountryApi } from '../services/countryApi'
import type { CountryListItem } from '../types/country'
import { COUNTRY_FILTER_FIELDS, COUNTRY_SORT_FIELDS } from '../types/country'
import Tag from 'primevue/tag'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const selectedItems = ref<CountryListItem[]>([])

const {
  items,
  loading,
  totalCount,
  page,
  pageSize,
  setSearch,
  refresh,
} = usePagedQuery<CountryListItem>('api/locations/countries', {
  allowedFilterFields: COUNTRY_FILTER_FIELDS,
  allowedSortFields: COUNTRY_SORT_FIELDS,
  defaultSort: ['name'],
  defaultPageSize: 20,
})

const columns: ColumnDef[] = [
  { field: 'name', header: 'Name', sortable: true, filter: true },
  { field: 'isoCode', header: 'ISO Code', sortable: true, filter: true },
  { field: 'callingCode', header: 'Calling Code', sortable: true },
  {
    field: 'statesRequired',
    header: 'States Required',
    sortable: true,
    bodyStyle: 'text-align: center',
  },
  {
    field: 'isActive',
    header: 'Active',
    sortable: true,
    filter: true,
    bodyStyle: 'text-align: center',
  },
  { field: 'actions', header: '', bodyStyle: 'text-align: right; width: 6rem' },
]

function navigateToNew() {
  router.push('/location/countries/new')
}

function navigateToEdit(id: string) {
  router.push(`/location/countries/${id}`)
}

function onSearch(value: string) {
  setSearch(value)
}

function confirmDelete() {
  if (selectedItems.value.length === 0) return

  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these countries' : 'this country'}?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const target = selectedItems.value[0]!
      const result = await CountryApi.deleteCountry(target.id)
      if (result.isSuccess) {
        notify.success('Country deleted', `${target.name} has been removed.`)
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete country.')
      }
      selectedItems.value = []
      refresh()
    },
  })
}
</script>

<template>
  <PageShell title="Countries" description="Manage supported countries">
    <CrudToolbar
      new-label="New Country"
      delete-label="Delete"
      :delete-disabled="selectedItems.length === 0"
      :search-placeholder="'Search countries...'"
      @new="navigateToNew"
      @delete="confirmDelete"
      @update:search="onSearch"
    />
    <FilterableDataTable
      :columns="columns"
      :data="items"
      :loading="loading"
      :rows="pageSize"
      :filters="{}"
      :global-filter-fields="['name', 'isoCode', 'callingCode']"
    >
      <template #body-statesRequired="{ data }">
        <Tag :value="data.statesRequired ? 'Yes' : 'No'" :severity="data.statesRequired ? 'info' : 'secondary'" />
      </template>
      <template #body-isActive="{ data }">
        <Tag :value="data.isActive ? 'Active' : 'Inactive'" :severity="data.isActive ? 'success' : 'danger'" />
      </template>
      <template #body-actions="{ data }">
        <div class="flex justify-end gap-2">
          <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
          <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
        </div>
      </template>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No countries found.</div>
      </template>
    </FilterableDataTable>
  </PageShell>
</template>
