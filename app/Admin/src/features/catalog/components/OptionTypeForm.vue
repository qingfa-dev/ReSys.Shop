<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import { LoadingSkeleton, ErrorState, AppCard, DataTable } from '@/shared/components'
import Column from 'primevue/column'
import Sidebar from 'primevue/sidebar'
import Button from 'primevue/button'
import Checkbox from 'primevue/checkbox'
import { useToast } from '@/shared/composables/useToast'
import { useConfirm } from '@/shared/composables/useConfirm'
import { OptionTypeForms } from '../schemas'
import { OptionTypeFormMapper } from '../mappers/option-type.mapper'
import { OptionTypeApi, OptionValueApi } from '../api'
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

const schemas = new OptionTypeForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
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
  if (mode.value === 'create') return t('catalog.option_types.titles.create')
  if (mode.value === 'edit') return `${t('catalog.option_types.actions.edit')}: ${name.value || ''}`
  return name.value || t('catalog.option_types.titles.edit')
})

const subtitle = computed(() => {
  if (mode.value === 'create') return t('catalog.option_types.descriptions.create')
  return t('catalog.option_types.descriptions.list')
})

async function loadOptionType() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  try {
    const result = await OptionTypeApi.get(id.value)
    if (result.isSuccess) {
      setValues({ name: result.value.name, presentation: result.value.presentation ?? undefined, filterable: result.value.filterable ?? undefined })
    } else {
      loadError.value = result.message ?? 'Failed to load option type'
    }
  } catch (err) {
    console.error(err)
    loadError.value = 'Failed to load option type'
  }
  loading.value = false
}

async function loadOptionValues() {
  if (!id.value) return
  optionValuesLoading.value = true
  try {
    const result = await OptionValueApi.getMany(id.value)
    if (result.isSuccess) { optionValues.value = result.value }
  } catch (err) {
    console.error(err)
  }
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
    toast.success(id.value ? t('catalog.option_types.messages.update_success') : t('catalog.option_types.messages.create_success'))
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
    ? await OptionValueApi.update(id.value, editingOptionValue.value.id, data)
    : await OptionValueApi.create(id.value, data)
  optionValueSaving.value = false
  if (result.isSuccess) {
    toast.success(editingOptionValue.value ? t('catalog.option_values.messages.update_success') : t('catalog.option_values.messages.create_success'))
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
  const result = await OptionValueApi.delete(id.value, ov.id)
  if (result.isSuccess) {
    toast.success(t('catalog.option_values.messages.delete_success'))
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
    <PageHeader :title="title" :subtitle="subtitle" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <Button v-if="mode === 'view'" :label="t('catalog.option_types.actions.edit')" @click="toggleEdit" />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadOptionType" />
    <template v-else>
      <AppCard class="mb-4">
        <div class="grid grid-cols-12 gap-4">
          <div class="col-span-full sm:col-span-6">
            <FormField :label="t('catalog.option_types.labels.name')" :error="errors.name" required>
              <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
            </FormField>
          </div>
          <div class="col-span-full sm:col-span-6">
            <FormField :label="t('catalog.option_types.labels.presentation')">
              <input v-model="presentation" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
            </FormField>
          </div>
        </div>
        <div class="grid grid-cols-12 gap-4 mt-4">
          <div class="col-span-full sm:col-span-6">
            <FormField :label="t('catalog.option_types.labels.filterable')">
              <div class="flex align-items-center gap-2 mt-1">
                <Checkbox v-model="filterable" :binary="true" :disabled="mode === 'view'" input-id="filterable" />
                <label for="filterable">{{ t('catalog.option_types.descriptions.values') }}</label>
              </div>
            </FormField>
          </div>
        </div>
      </AppCard>

      <div v-if="id" class="flex flex-col gap-4">
        <AppCard>
          <div class="flex items-center justify-between mb-3">
            <h3 class="text-lg font-semibold text-surface-900 dark:text-surface-0">{{ t('catalog.option_values.titles.list') }}</h3>
            <Button :label="t('catalog.option_values.actions.add_value')" icon="pi pi-plus" size="small" @click="openAddOptionValue" />
          </div>
          <DataTable
            :rows="optionValues"
            :loading="optionValuesLoading"
            :empty-title="t('catalog.option_values.messages.empty_list')"
            empty-description="Add an option value to get started."
          >
            <Column field="name" :header="t('catalog.option_values.labels.name')" />
            <Column field="presentation" :header="t('catalog.option_values.labels.presentation')" />
            <Column field="position" :header="t('catalog.option_values.labels.position')" />
            <template #rowActions="{ data }">
              <div class="flex gap-1">
                <Button icon="pi pi-pencil" severity="secondary" text rounded size="small" @click="openEditOptionValue(data)" />
                <Button icon="pi pi-trash" severity="danger" text rounded size="small" @click="confirmDeleteOptionValue(data)" />
              </div>
            </template>
          </DataTable>
        </AppCard>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? t('catalog.option_types.actions.save_create') : t('catalog.option_types.actions.save_edit')"
        :cancel-label="t('catalog.option_types.actions.cancel')"
        @save="save"
        @cancel="cancel"
      />
    </template>

    <Sidebar v-model:visible="optionValueSlideoverVisible" :header="t('catalog.option_values.titles.create')" position="right" class="w-full sm:w-96">
      <div class="flex flex-col gap-4">
        <FormField :label="t('catalog.option_values.labels.name')" required>
          <input v-model="optionValueForm.name" type="text" class="p-inputtext p-component w-full" />
        </FormField>
        <FormField :label="t('catalog.option_values.labels.presentation')">
          <input v-model="optionValueForm.presentation" type="text" class="p-inputtext p-component w-full" />
        </FormField>
        <div class="flex gap-2 justify-end mt-4">
          <Button :label="t('catalog.option_values.actions.cancel')" severity="secondary" text @click="optionValueSlideoverVisible = false" />
          <Button :label="editingOptionValue ? t('catalog.option_values.titles.edit') : t('catalog.option_values.titles.create')" icon="pi pi-check" :loading="optionValueSaving" @click="saveOptionValue" />
        </div>
      </div>
    </Sidebar>
  </div>
</template>
