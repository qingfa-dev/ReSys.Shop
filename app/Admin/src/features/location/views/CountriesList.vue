<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
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
  pageSize,
  setSearch,
  refresh,
} = usePagedQuery<CountryListItem>('api/locations/countries', {
  allowedFilterFields: COUNTRY_FILTER_FIELDS,
  allowedSortFields: COUNTRY_SORT_FIELDS,
  allowedSearchFields: COUNTRY_FILTER_FIELDS,
  defaultSearchFields: COUNTRY_FILTER_FIELDS,
  defaultSearchMode: 'any',
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
      const ids = selectedItems.value.map(i => i.id)
      const names = selectedItems.value.map(i => i.name)
      let failed = 0
      for (const id of ids) {
        const result = await CountryApi.deleteCountry(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      refresh()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'Countries deleted' : 'Country deleted',
          ids.length > 1
            ? `${ids.length} countries have been removed.`
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
  <div class="flex flex-col h-full p-4">
    <div class="flex-none flex flex-col gap-4">
      <div>
        <div class="font-semibold text-xl">Countries</div>
        <p class="text-muted-color mt-1">Manage supported countries</p>
      </div>
    </div>

    <div class="flex-1 min-h-0 mt-4">
      <DataTable size="large"
        ref="dt"
        v-model:selection="selectedItems"
        :value="items"
        :loading="loading"
        scrollable
        :paginator="true"
        :rows="pageSize"
        filter-display="menu"
        data-key="id"
        :global-filter-fields="COUNTRY_FILTER_FIELDS"
        paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
        :rows-per-page-options="[5, 10, 25]"
        current-page-report-template="Showing {first} to {last} of {totalRecords}"
        :pt="{ wrapper: { class: 'h-full' }, tableContainer: { class: 'h-full' } }"
      >
        <Column selection-mode="multiple" header-style="width: 3rem" />
        <template #header>
          <div class="flex justify-between items-center">
            <div class="flex items-center gap-2">
              <FloatLabel variant="on">
                <IconField>
                  <InputIcon class="pi pi-search" />
                  <InputText
                    :model-value="searchTerm"
                    placeholder="Search countries..."
                    @update:model-value="onSearch($event ?? '')"
                  />
                </IconField>
                <label>Search</label>
              </FloatLabel>
              <Button label="Clear" outlined @click="clearSearch" />
            </div>
            <div class="flex items-center gap-2">
              <Button label="New Country" icon="pi pi-plus" severity="primary" @click="navigateToNew" />
              <Button label="Reload" icon="pi pi-sync" severity="secondary" @click="refresh" />
              <Button label="Export" icon="pi pi-upload" severity="secondary" @click="exportCSV" />
            </div>
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
    </div>
  </div>
</template>
