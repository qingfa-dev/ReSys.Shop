<script setup lang="ts">
import { ref, onMounted } from 'vue'
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
import { StateApi } from '../services/stateApi'
import { useCountryStore } from '../stores/countryStore'
import type { StateListItem } from '../types/state'
import { STATE_FILTER_FIELDS, STATE_SORT_FIELDS } from '../types/state'
import Select from 'primevue/select'
import Tag from 'primevue/tag'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const countryStore = useCountryStore()

const selectedCountryId = ref<string | null>(null)
const selectedItems = ref<StateListItem[]>([])

const { items, loading, totalCount, page, pageSize, setSearch, setFilter, refresh } =
  usePagedQuery<StateListItem>('api/locations/states', {
    allowedFilterFields: STATE_FILTER_FIELDS,
    allowedSortFields: STATE_SORT_FIELDS,
    defaultSort: ['name'],
    defaultPageSize: 20,
  })

const columns: ColumnDef[] = [
  { field: 'name', header: 'Name', sortable: true, filter: true },
  { field: 'abbreviation', header: 'Abbreviation', sortable: true, filter: true },
  { field: 'countryName', header: 'Country', sortable: true, filter: true },
  {
    field: 'isActive',
    header: 'Active',
    sortable: true,
    filter: true,
    bodyStyle: 'text-align: center',
  },
  { field: 'actions', header: '', bodyStyle: 'text-align: right; width: 6rem' },
]

onMounted(() => {
  countryStore.fetchActive()
})

function navigateToNew() {
  router.push('/location/states/new')
}

function navigateToEdit(id: string) {
  router.push(`/location/states/${id}`)
}

function onSearch(value: string) {
  setSearch(value)
}

function onCountryFilterChange(countryId: string | null) {
  selectedCountryId.value = countryId
  if (countryId) {
    setFilter(`countryId=${countryId}`)
  } else {
    setFilter('')
  }
}

function confirmDelete() {
  if (selectedItems.value.length === 0) return

  confirm.require({
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these states' : 'this state'}?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const target = selectedItems.value[0]!
      const result = await StateApi.deleteState(target.id)
      if (result.isSuccess) {
        notify.success('State deleted', `${target.name} has been removed.`)
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete state.')
      }
      selectedItems.value = []
      refresh()
    },
  })
}
</script>

<template>
  <PageShell title="States" description="Manage states and provinces for countries">
    <CrudToolbar
      new-label="New State"
      delete-label="Delete"
      :delete-disabled="selectedItems.length === 0"
      :search-placeholder="'Search states...'"
      @new="navigateToNew"
      @delete="confirmDelete"
      @update:search="onSearch"
    >
      <template #header-left>
        <div class="flex items-center gap-2">
          <label class="text-sm text-muted-color whitespace-nowrap">Country:</label>
          <Select
            v-model="selectedCountryId"
            :options="countryStore.activeCountries"
            option-label="name"
            option-value="id"
            placeholder="All Countries"
            show-clear
            class="w-56"
            @change="onCountryFilterChange($event.value)"
          />
        </div>
      </template>
    </CrudToolbar>
    <FilterableDataTable
      :columns="columns"
      :data="items"
      :loading="loading"
      :rows="pageSize"
      :filters="{}"
      :global-filter-fields="['name', 'abbreviation', 'countryName']"
    >
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
        <div class="text-center py-8 text-muted-color">No states found.</div>
      </template>
    </FilterableDataTable>
  </PageShell>
</template>
