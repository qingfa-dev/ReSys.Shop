<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import DataTable from '@/shared/components/data/DataTable.vue'
import Column from 'primevue/column'
import Sidebar from 'primevue/sidebar'
import Button from 'primevue/button'
import Checkbox from 'primevue/checkbox'
import { useToast } from '@/shared/composables/useToast'
import { useConfirm } from '@/shared/composables/useConfirm'
import { CatalogForms } from '../schemas'
import { OptionTypeFormMapper } from '../mappers/option-type.mapper'
import { OptionTypeApi } from '../api'
import type { OptionValueResponse, OptionValueRequest } from '../types'
import { ROUTE } from '../routes'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const { confirmDelete } = useConfirm()
const { t } = useI18n()

const id = computed(() => route.params.id as string | undefined)
const mode = computed<'create' | 'view' | 'edit'>(() => {
  if (!id.value) return 'create'
  if (route.name?.toString().endsWith('.edit')) return 'edit'
  return 'view'
})

const schemas = new CatalogForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.createOptionType() : schemas.updateOptionType(),
  ),
})

const [name] = defineField('name')
const [presentation] = defineField('presentation')
const [filterable] = defineField('filterable')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const optionValues = ref<OptionValueResponse[]>([])
const optionValuesLoading = ref(false)

const optionValueSlideoverVisible = ref(false)
const editingOptionValue = ref<OptionValueResponse | null>(null)
const optionValueForm = ref<OptionValueRequest>({ name: '', presentation: null })
const optionValueSaving = ref(false)

const title = computed(() => {
  if (mode.value === 'create') return 'Create Option Type'
  if (mode.value === 'edit') return `Edit: ${name.value || 'Option Type'}`
  return name.value || 'Option Type Detail'
})

async function loadOptionType() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await OptionTypeApi.get(id.value)
  if (result.isSuccess) {
    setValues({ name: result.value.name, presentation: result.value.presentation, filterable: result.value.filterable })
  } else {
    loadError.value = result.message ?? 'Failed to load option type'
  }
  loading.value = false
}

async function loadOptionValues() {
  if (!id.value) return
  optionValuesLoading.value = true
  const result = await OptionTypeApi.getValues(id.value)
  if (result.isSuccess) { optionValues.value = result.value }
  optionValuesLoading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? OptionTypeFormMapper.toCreate(values)
    : OptionTypeFormMapper.toUpdate(values)
  const result = id.value
    ? await OptionTypeApi.update(id.value, data)
    : await OptionTypeApi.create(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? t('catalog.optionType.updated') : t('catalog.optionType.created'))
    const newId = result.value.id
    router.replace({ name: ROUTE.OPTION_TYPES.VIEW, params: { id: newId } })
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  if (id.value) router.push({ name: ROUTE.OPTION_TYPES.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.OPTION_TYPES.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.OPTION_TYPES.EDIT, params: { id: id.value } })
}

function openAddOptionValue() {
  editingOptionValue.value = null
  optionValueForm.value = { name: '', presentation: null }
  optionValueSlideoverVisible.value = true
}

function openEditOptionValue(ov: OptionValueResponse) {
  editingOptionValue.value = ov
  optionValueForm.value = { name: ov.name, presentation: ov.presentation }
  optionValueSlideoverVisible.value = true
}

async function saveOptionValue() {
  if (!optionValueForm.value.name.trim() || !id.value) return
  optionValueSaving.value = true
  const data: OptionValueRequest = { name: optionValueForm.value.name, presentation: optionValueForm.value.presentation || null }
  const result = editingOptionValue.value
    ? await OptionTypeApi.updateValue(id.value, editingOptionValue.value.id, data)
    : await OptionTypeApi.createValue(id.value, data)
  optionValueSaving.value = false
  if (result.isSuccess) {
    toast.success(editingOptionValue.value ? 'Option value updated' : 'Option value created')
    optionValueSlideoverVisible.value = false
    await loadOptionValues()
  } else { toast.error(result.message ?? 'Save failed') }
}

function confirmDeleteOptionValue(ov: OptionValueResponse) {
  confirmDelete({
    target: 'this option value',
    onAccept: () => deleteOptionValueAction(ov),
  })
}

async function deleteOptionValueAction(ov: OptionValueResponse) {
  if (!id.value) return
  const result = await OptionTypeApi.deleteValue(id.value, ov.id)
  if (result.isSuccess) {
    toast.success('Option value deleted')
    await loadOptionValues()
  } else { toast.error(result.message ?? 'Delete failed') }
}

onMounted(async () => {
  await loadOptionType()
  if (id.value) await loadOptionValues()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">Edit</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadOptionType" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Name" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Presentation">
            <input v-model="presentation" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-6">
          <FormField label="Filterable">
            <div class="flex align-items-center gap-2 mt-1">
              <Checkbox v-model="filterable" :binary="true" :disabled="mode === 'view'" input-id="filterable" />
              <label for="filterable">Enable filtering by this option</label>
            </div>
          </FormField>
        </div>
      </div>

      <fieldset v-if="id" class="mt-6 border border-surface-200 dark:border-surface-700 rounded-lg p-4">
        <legend class="text-lg font-semibold text-surface-900 dark:text-surface-0 px-2">Option Values</legend>
        <div class="flex justify-end mb-3">
          <Button label="Add Option Value" icon="pi pi-plus" size="small" @click="openAddOptionValue" />
        </div>
        <DataTable
          :rows="optionValues"
          :loading="optionValuesLoading"
          empty-title="No option values"
          empty-description="Add an option value to get started."
        >
          <Column field="name" header="Name" />
          <Column field="presentation" header="Presentation" />
          <Column field="position" header="Position" />
          <template #rowActions="{ data }">
            <div class="flex gap-1">
              <Button icon="pi pi-pencil" severity="secondary" text rounded size="small" @click="openEditOptionValue(data)" />
              <Button icon="pi pi-trash" severity="danger" text rounded size="small" @click="confirmDeleteOptionValue(data)" />
            </div>
          </template>
        </DataTable>
      </fieldset>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? 'Create Option Type' : 'Save Changes'"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />
    </div>

    <Sidebar v-model:visible="optionValueSlideoverVisible" header="Option Value" position="right" class="w-full sm:w-96">
      <div class="flex flex-col gap-4">
        <FormField label="Name" required>
          <input v-model="optionValueForm.name" type="text" class="p-inputtext p-component w-full" />
        </FormField>
        <FormField label="Presentation">
          <input v-model="optionValueForm.presentation" type="text" class="p-inputtext p-component w-full" />
        </FormField>
        <div class="flex gap-2 justify-end mt-4">
          <Button label="Cancel" severity="secondary" text @click="optionValueSlideoverVisible = false" />
          <Button :label="editingOptionValue ? 'Update' : 'Create'" icon="pi pi-check" :loading="optionValueSaving" @click="saveOptionValue" />
        </div>
      </div>
    </Sidebar>
  </div>
</template>
