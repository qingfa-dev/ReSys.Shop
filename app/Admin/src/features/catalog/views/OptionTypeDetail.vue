<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Toolbar from 'primevue/toolbar'
import Plus from '@primeicons/vue/plus'
import { PageShell } from '@panel'
import { FormSection, FormField } from '@form'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { OptionTypeApi } from '../services/optionTypeApi'
import { OptionValueApi } from '../services/optionValueApi'
import { optionTypeSchema } from '../validations/optionType'
import type { OptionTypeForm } from '../validations/optionType'
import type { OptionValueListItem } from '../types/optionValue'
import { OPTION_VALUE_FILTER_FIELDS, OPTION_VALUE_SORT_FIELDS } from '../types/optionValue'
import OptionValueFormDialog from '../components/OptionValueFormDialog.vue'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const confirm = useConfirm()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Option Type' : 'New Option Type')
const activeTab = ref('0')

const form = ref<OptionTypeForm>({
  name: '',
  presentation: '',
  position: 1,
  filterable: false,
})

const fieldErrors = ref<Record<string, string>>({})
const saving = ref(false)

const dialogVisible = ref(false)
const editingValue = ref<OptionValueListItem | null>(null)

const valueSearchFields = ['name', 'presentation']

const {
  items: optionValues,
  loading: valuesLoading,
  setSearch: setValueSearch,
  setFilter: setValueFilter,
  refresh: refreshValues,
} = usePagedQuery<OptionValueListItem>('api/catalog/option-types/option-values', {
  allowedFilterFields: OPTION_VALUE_FILTER_FIELDS,
  allowedSortFields: OPTION_VALUE_SORT_FIELDS,
  allowedSearchFields: valueSearchFields,
  defaultSearchFields: valueSearchFields,
  defaultSearchMode: 'any',
  defaultSort: ['position', 'name'],
  defaultPageSize: 20,
})

const valueSearchTerm = ref('')

async function initEditMode(id: string) {
  setValueFilter(`optionTypeId=${id}`)

  const result = await OptionTypeApi.getOptionType(id)
  if (result.isSuccess) {
    const ot = result.value
    form.value = {
      name: ot.name,
      presentation: ot.presentation,
      position: ot.position,
      filterable: ot.filterable,
    }
  } else {
    handleResult(result)
    router.push('/catalog/option-types')
  }
}

onMounted(() => {
  if (isEdit.value) {
    initEditMode(route.params.id as string)
  }
})

watch(() => route.params.id, (newId) => {
  if (newId && newId !== 'new') {
    initEditMode(newId as string)
  }
})

async function onSave() {
  fieldErrors.value = {}
  const parsed = optionTypeSchema.safeParse(form.value)

  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      const field = String(issue.path[0])
      if (!fieldErrors.value[field]) {
        fieldErrors.value[field] = issue.message
      }
    }
    return
  }

  saving.value = true
  const data = parsed.data
  const request = {
    name: data.name,
    presentation: data.presentation,
    position: data.position,
    filterable: data.filterable,
  }

  const result = isEdit.value
    ? await OptionTypeApi.updateOptionType(route.params.id as string, request)
    : await OptionTypeApi.createOptionType(request)

  saving.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Option type updated' : 'Option type created')
    if (!isEdit.value && result.value) {
      const created = result.value
      form.value = {
        name: created.name,
        presentation: created.presentation,
        position: created.position,
        filterable: created.filterable,
      }
      router.replace(`/catalog/option-types/${created.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/catalog/option-types')
}

function openAddDialog() {
  editingValue.value = null
  dialogVisible.value = true
}

function openEditDialog(value: OptionValueListItem) {
  editingValue.value = value
  dialogVisible.value = true
}

function onDialogSaved() {
  refreshValues()
}

function confirmDeleteValue(value: OptionValueListItem) {
  confirm.require({
    message: `Are you sure you want to delete "${value.name}"?`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      const result = await OptionValueApi.deleteOptionValue(value.id)
      if (result.isSuccess) {
        notify.success('Option value deleted', `${value.name} has been removed.`)
        refreshValues()
      } else {
        notify.error('Delete failed', result.errors?.[0]?.message ?? 'Could not delete option value.')
      }
    },
  })
}

function onValueSearch(value: string) {
  valueSearchTerm.value = value
  setValueSearch(value)
}
</script>

<template>
  <PageShell :title="pageTitle">
    <div class="flex items-center gap-2 text-muted-color mb-4">
      <router-link to="/" class="hover:text-primary">Home</router-link>
      <i class="pi pi-angle-right text-xs" />
      <router-link to="/catalog/option-types" class="hover:text-primary">Option Types</router-link>
      <i class="pi pi-angle-right text-xs" />
      <span>{{ pageTitle }}</span>
    </div>
    <Toolbar class="mb-8">
      <template #start>
        <h1 class="text-2xl font-bold">{{ pageTitle }}</h1>
      </template>
      <template #end>
        <Button label="Save" icon="pi pi-check" severity="primary" @click="onSave()" />
        <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </template>
    </Toolbar>

    <Tabs v-model:value="activeTab">
      <TabList>
        <Tab value="0">General</Tab>
        <Tab v-if="isEdit" value="1">Option Values</Tab>
      </TabList>

      <TabPanels>
        <TabPanel value="0">
          <FormSection title="Option Type Details">
            <FormField label="Name" :required="true" :invalid="!!fieldErrors.name">
              <InputText v-model="form.name" fluid class="w-full" />
              <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
            </FormField>
            <FormField label="Presentation" :required="true" :invalid="!!fieldErrors.presentation" help-text="Display text shown to customers">
              <InputText v-model="form.presentation" fluid class="w-full" />
              <small v-if="fieldErrors.presentation" class="text-red-500">{{ fieldErrors.presentation }}</small>
            </FormField>
            <FormField label="Position" :invalid="!!fieldErrors.position" help-text="Sort order (lower = first)">
              <InputNumber v-model="form.position" fluid :min="-1" class="w-full" />
              <small v-if="fieldErrors.position" class="text-red-500">{{ fieldErrors.position }}</small>
            </FormField>
            <FormField label="Filterable" help-text="Show in storefront filter panel">
              <ToggleSwitch v-model="form.filterable" />
            </FormField>
          </FormSection>
        </TabPanel>

        <TabPanel v-if="isEdit" value="1">
          <Card>
            <template #content>
              <Toolbar>
                <template #start>
                  <Button label="Add Value" severity="secondary" @click="openAddDialog">
                    <Plus />
                  </Button>
                </template>
              </Toolbar>
            </template>
          </Card>

          <DataTable
            :value="optionValues"
            :loading="valuesLoading"
            data-key="id"
            :global-filter-fields="valueSearchFields"
          >
            <template #header>
              <div class="flex justify-between items-center">
                <IconField>
                  <InputIcon><i class="pi pi-search" /></InputIcon>
                  <InputText
                    :model-value="valueSearchTerm"
                    placeholder="Search values..."
                    @update:model-value="onValueSearch($event ?? '')"
                  />
                </IconField>
              </div>
            </template>
            <Column field="name" header="Name" :sortable="true" />
            <Column field="presentation" header="Presentation" :sortable="true" />
            <Column field="position" header="Position" :sortable="true" />
            <Column header="" body-style="text-align: right; width: 8rem">
              <template #body="{ data }">
                <div class="flex justify-end gap-2">
                  <Button icon="pi pi-pencil" severity="secondary" text rounded aria-label="Edit" @click="openEditDialog(data)" />
                  <Button icon="pi pi-trash" severity="secondary" text rounded aria-label="Delete" @click="confirmDeleteValue(data)" />
                </div>
              </template>
            </Column>
            <template #empty>
              <div class="text-center py-8 text-muted-color">No option values defined.</div>
            </template>
          </DataTable>
        </TabPanel>
      </TabPanels>
    </Tabs>

    <OptionValueFormDialog
      :visible="dialogVisible"
      :option-type-id="(route.params.id as string) || ''"
      :editing-value="editingValue"
      @update:visible="dialogVisible = $event"
      @saved="onDialogSaved"
    />
  </PageShell>
</template>
