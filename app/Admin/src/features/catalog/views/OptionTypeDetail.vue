<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Plus from '@primeicons/vue/plus'
import Card from 'primevue/card'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { OptionTypeApi } from '../services/optionTypeApi'
import { OptionValueApi } from '../services/optionValueApi'
import { optionTypeSchema, optionTypeName, optionTypePresentation, optionTypePosition } from '../validations/optionType'
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
const pageTitle = computed(() => (isEdit.value ? 'Edit Option Type' : 'New Option Type'))
const pageDescription = computed(() =>
  isEdit.value ? 'Edit an existing option type' : 'Create a new option type',
)
const activeTab = ref('0')

const resolver = zodResolver(optionTypeSchema)
const nameResolver = zodResolver(optionTypeName)
const presentationResolver = zodResolver(optionTypePresentation)
const positionResolver = zodResolver(optionTypePosition)

const form = ref<OptionTypeForm>({
  name: '',
  presentation: '',
  position: 1,
  filterable: false,
})

const loading = ref(false)
const formLoaded = ref(!isEdit.value)

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
    formLoaded.value = true
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

watch(
  () => route.params.id,
  (newId) => {
    if (newId && newId !== 'new') {
      initEditMode(newId as string).then(() => {
        formLoaded.value = true
      })
    }
  },
)

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return

  const data = event.values as OptionTypeForm
  loading.value = true

  const request = {
    name: data.name,
    presentation: data.presentation,
    position: data.position,
    filterable: data.filterable,
  }

  const result = isEdit.value
    ? await OptionTypeApi.updateOptionType(route.params.id as string, request)
    : await OptionTypeApi.createOptionType(request)

  loading.value = false

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
        notify.error(
          'Delete failed',
          result.errors?.[0]?.message ?? 'Could not delete option value.',
        )
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
  <Card>
    <template #content>
      <div class="font-semibold text-xl mb-4">{{ pageTitle }}</div>
      <p v-if="pageDescription" class="text-muted-color mb-4">{{ pageDescription }}</p>

    <Form v-slot="$form" :key="String(formLoaded)" :resolver="resolver" :initial-values="form" @submit="onSubmit">
      <Tabs v-model:value="activeTab">
        <TabList>
          <Tab value="0">General</Tab>
          <Tab v-if="isEdit" value="1">Option Values</Tab>
        </TabList>

        <TabPanels>
          <TabPanel value="0">
            <Card>
              <template #content>
                <div class="flex flex-col gap-6">
                  <div class="font-semibold text-xl">Option Type Details</div>
                    <div class="flex flex-col gap-4">
                      <FormField v-slot="$field" name="name" :resolver="nameResolver" class="flex flex-col gap-1">
                        <label class="text-surface-900 dark:text-surface-0 font-medium">Name <span class="text-red-500">*</span></label>
                        <InputText fluid />
                        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                      </FormField>
                      <FormField v-slot="$field" name="presentation" :resolver="presentationResolver" class="flex flex-col gap-1">
                        <label class="text-surface-900 dark:text-surface-0 font-medium">Presentation <span class="text-red-500">*</span></label>
                        <InputText fluid />
                        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                      </FormField>
                      <FormField v-slot="$field" name="position" :resolver="positionResolver" class="flex flex-col gap-1">
                        <label class="text-surface-900 dark:text-surface-0 font-medium">Position</label>
                        <InputNumber fluid :min="-1" />
                        <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                      </FormField>
                      <FormField v-slot="$field" name="filterable" class="flex flex-col gap-1">
                        <label class="text-surface-900 dark:text-surface-0 font-medium">Filterable</label>
                        <ToggleSwitch />
                      </FormField>
                    </div>
                </div>
              </template>
            </Card>
          </TabPanel>

          <TabPanel v-if="isEdit" value="1">
            <Toolbar>
              <template #start>
                <Button label="Add Value" severity="secondary" @click="openAddDialog">
                  <Plus />
                </Button>
              </template>
            </Toolbar>

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
                    <Button
                      icon="pi pi-pencil"
                      severity="secondary"
                      text
                      rounded
                      aria-label="Edit"
                      @click="openEditDialog(data)"
                    />
                    <Button
                      icon="pi pi-trash"
                      severity="secondary"
                      text
                      rounded
                      aria-label="Delete"
                      @click="confirmDeleteValue(data)"
                    />
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

      <div class="flex justify-end gap-2 pt-4 border-t border-surface mt-4">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="loading" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </Form>

    <OptionValueFormDialog
      :visible="dialogVisible"
      :option-type-id="(route.params.id as string) || ''"
      :editing-value="editingValue"
      @update:visible="dialogVisible = $event"
      @saved="onDialogSaved"
    />
    </template>
  </Card>
</template>
