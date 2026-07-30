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
import { OptionTypeApi } from '../services/optionTypeApi'
import type { OptionTypeListItem } from '../types/optionType'
import { OPTION_TYPE_FILTER_FIELDS, OPTION_TYPE_SORT_FIELDS } from '../types/optionType'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()

const { dt, exportCSV } = useDataTableExport()
const selectedItems = ref<OptionTypeListItem[]>([])
const searchTerm = ref('')
const allowedSearchFields = ['name', 'presentation']

const {
  items,
  loading,
  totalCount,
  page,
  pageSize,
  setSearch,
  refresh,
} = usePagedQuery<OptionTypeListItem>('api/catalog/option-types', {
  allowedFilterFields: OPTION_TYPE_FILTER_FIELDS,
  allowedSortFields: OPTION_TYPE_SORT_FIELDS,
  allowedSearchFields,
  defaultSearchFields: allowedSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['name'],
  defaultPageSize: 20,
})

function navigateToNew() {
  router.push('/catalog/option-types/new')
}

function navigateToEdit(id: string) {
  router.push(`/catalog/option-types/${id}`)
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
    message: `Are you sure you want to delete ${selectedItems.value.length > 1 ? 'these option types' : 'this option type'}?`,
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
        const result = await OptionTypeApi.deleteOptionType(id)
        if (!result.isSuccess) failed++
      }
      selectedItems.value = []
      refresh()
      if (failed === 0) {
        notify.success(
          ids.length > 1 ? 'Option types deleted' : 'Option type deleted',
          ids.length > 1
            ? `${ids.length} option types have been removed.`
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
  <PageShell title="Option Types" description="Manage product option types (Size, Color, Material, etc.)">
    <Card>
      <template #content>
        <Toolbar>
          <template #start>
            <Button label="New" severity="secondary" class="mr-2" @click="navigateToNew">
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
      v-model:selection="selectedItems"
      :value="items"
      :loading="loading"
      :paginator="true"
      :rows="pageSize"
      filter-display="menu"
      data-key="id"
      :global-filter-fields="allowedSearchFields"
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
              placeholder="Search option types..."
              @update:model-value="onSearch($event ?? '')"
            />
          </IconField>
          <Button label="Clear" outlined @click="clearSearch" />
        </div>
      </template>
      <Column field="name" header="Name" :sortable="true" :filter="true" filter-field="name" />
      <Column field="presentation" header="Presentation" :sortable="true" />
      <Column field="position" header="Position" :sortable="true" />
      <Column field="filterable" header="Filterable" :sortable="true" body-style="text-align: center">
        <template #body="{ data }">
          <Tag :value="data.filterable ? 'Yes' : 'No'" :severity="data.filterable ? 'success' : 'secondary'" />
        </template>
      </Column>
      <Column field="optionValuesCount" header="Values" :sortable="true" />
      <Column field="productsCount" header="Products" :sortable="true" />
      <Column header="" body-style="text-align: right; width: 6rem">
        <template #body="{ data }">
          <div class="flex justify-end gap-2">
            <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="navigateToEdit(data.id)" />
            <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="selectedItems = [data]; confirmDelete()" />
          </div>
        </template>
      </Column>
      <template #empty>
        <div class="text-center py-8 text-muted-color">No option types found.</div>
      </template>
    </DataTable>
  </PageShell>
</template>
