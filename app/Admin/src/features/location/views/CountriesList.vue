<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Tag from 'primevue/tag'
import Plus from '@primeicons/vue/plus'
import Trash from '@primeicons/vue/trash'
import Upload from '@primeicons/vue/upload'
import { PageShell } from '@panel'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { CountryApi } from '../services/countryApi'
import type { CountryListItem } from '../types/country'
import { COUNTRY_FILTER_FIELDS, COUNTRY_SORT_FIELDS } from '../types/country'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<CountryListItem[]>([])
const searchTerm = ref('')

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

function navigateToNew() {
  router.push('/location/countries/new')
}

function navigateToEdit(id: string) {
  router.push(`/location/countries/${id}`)
}

function onSearch(value: string) {
  searchTerm.value = value
  setSearch(value)
}

function clearSearch() {
  searchTerm.value = ''
  setSearch('')
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
    <Card>
      <template #content>
        <Toolbar>
          <template #start>
            <Button label="New Country" severity="secondary" class="mr-2" @click="navigateToNew">
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
      </template>
    </Card>

    <DataTable
      ref="dt"
      :value="items"
      :loading="loading"
      :paginator="true"
      :rows="pageSize"
      filter-display="menu"
      data-key="id"
      :global-filter-fields="['name', 'isoCode', 'callingCode']"
      paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
      :rows-per-page-options="[5, 10, 25]"
      current-page-report-template="Showing {first} to {last} of {totalRecords}"
    >
      <template #header>
        <div class="flex justify-between items-center">
          <IconField>
            <InputIcon><i class="pi pi-search" /></InputIcon>
            <InputText
              :model-value="searchTerm"
              placeholder="Search countries..."
              @update:model-value="onSearch($event ?? '')"
            />
          </IconField>
          <Button label="Clear" outlined @click="clearSearch" />
        </div>
      </template>
      <Column field="name" header="Name" :sortable="true" :filter="true" filter-field="name" />
      <Column field="isoCode" header="ISO Code" :sortable="true" :filter="true" filter-field="isoCode" />
      <Column field="callingCode" header="Calling Code" :sortable="true" />
      <Column field="statesRequired" header="States Required" :sortable="true" body-style="text-align: center">
        <template #body="{ data }">
          <Tag :value="data.statesRequired ? 'Yes' : 'No'" :severity="data.statesRequired ? 'info' : 'secondary'" />
        </template>
      </Column>
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
        <div class="text-center py-8 text-muted-color">No countries found.</div>
      </template>
    </DataTable>
  </PageShell>
</template>
