<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import Plus from '@primeicons/vue/plus'
import Trash from '@primeicons/vue/trash'
import Upload from '@primeicons/vue/upload'
import Card from 'primevue/card'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { StateApi } from '../services/stateApi'
import { useCountryStore } from '../stores/countryStore'
import type { StateListItem } from '../types/state'
import { STATE_FILTER_FIELDS, STATE_SORT_FIELDS } from '../types/state'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const countryStore = useCountryStore()

const { dt, exportCSV } = useDataTableExport()
const selectedCountryId = ref<string | null>(null)
const selectedItems = ref<StateListItem[]>([])
const searchTerm = ref('')

const { items, loading, totalCount, page, pageSize, setSearch, setFilter, refresh } =
  usePagedQuery<StateListItem>('api/locations/states', {
    allowedFilterFields: STATE_FILTER_FIELDS,
    allowedSortFields: STATE_SORT_FIELDS,
    allowedSearchFields: STATE_FILTER_FIELDS,
    defaultSearchFields: STATE_FILTER_FIELDS,
    defaultSearchMode: 'any',
    defaultSort: ['name'],
    defaultPageSize: 20,
  })

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
  searchTerm.value = value
  setSearch(value)
}

function clearSearch() {
  searchTerm.value = ''
  setSearch('')
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
      const ids = selectedItems.value.map(i => i.id)
      const names = selectedItems.value.map(i => i.name)
      let failed = 0
      for (const id of ids) {
        const result = await StateApi.deleteState(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      refresh()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'States deleted' : 'State deleted',
          ids.length > 1
            ? `${ids.length} states have been removed.`
            : `${names[0]} has been removed.`,
        )
      } else {
        notify.error(
          'Delete failed',
          `${failed} of ${ids.length} could not be deleted.`,
        )
      }
    },
  })
}
</script>

<template>
  <!-- Page shell -->
  <Card>
    <template #content>
      <div class="font-semibold text-xl mb-4">States</div>
      <p class="text-muted-color mb-4">Manage states and provinces for countries</p>
    <!-- Toolbar -->
    <Toolbar class="mb-4">
          <template #start>
            <Button label="New State" severity="secondary" class="mr-2" @click="navigateToNew">
              <Plus />
            </Button>
            <Button label="Delete" severity="secondary" :disabled="selectedItems.length === 0" @click="confirmDelete">
              <Trash />
            </Button>
          </template>
          <template #end>
            <Button label="Export" severity="secondary" @click="exportCSV">
              <Upload />
            </Button>
          </template>
        </Toolbar>

    <DataTable
      ref="dt"
      v-model:selection="selectedItems"
      :value="items"
      :loading="loading"
      :paginator="true"
      :rows="pageSize"
      filter-display="menu"
      data-key="id"
      :global-filter-fields="STATE_FILTER_FIELDS"
      paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
      :rows-per-page-options="[5, 10, 25]"
      current-page-report-template="Showing {first} to {last} of {totalRecords}"
    >
      <Column selection-mode="multiple" header-style="width: 3rem" />
      <template #header>
        <div class="flex justify-between items-center">
          <IconField>
            <InputIcon><i class="pi pi-search" /></InputIcon>
            <InputText
              :model-value="searchTerm"
              placeholder="Search states..."
              @update:model-value="onSearch($event ?? '')"
            />
          </IconField>
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
            <Button label="Clear" outlined @click="clearSearch" />
          </div>
        </div>
      </template>
      <Column field="name" header="Name" :sortable="true" :filter="true" filter-field="name" />
      <Column field="abbreviation" header="Abbreviation" :sortable="true" :filter="true" filter-field="abbreviation" />
      <Column field="countryName" header="Country" :sortable="true" :filter="true" filter-field="countryName" />
      <Column field="isActive" header="Active" :sortable="true" :filter="true" filter-field="isActive" body-style="text-align: center">
        <template #body="{ data }">
          <Tag :value="data.isActive ? 'Active' : 'Inactive'" :severity="data.isActive ? 'success' : 'danger'" />
        </template>
      </Column>
      <Column header="" body-style="text-align: right; width: 6rem">
        <template #body="{ data }">
          <div class="flex justify-end gap-2">
            <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
            <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No states found.</div>
      </template>
    </DataTable>
    </template>
  </Card>
</template>
