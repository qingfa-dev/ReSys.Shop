<script setup lang="ts">
import { onMounted, computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useOptionTypeStore } from '../stores/option-type.store'
import { useOptionValueStore } from '@/features/catalog/option-types/option-values/stores/option-value.store'
import { storeToRefs } from 'pinia'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { OptionTypeSchema } from '../schemas/OptionType.Schema'
import { OptionValueSchema } from '@/features/catalog/option-types/option-values/schemas/OptionValue.Schema'
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use'
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue'
import { useToast } from '@/shared/composables/toast.use'
import type { OptionValueListItem } from '@/features/catalog/option-types/option-values/types/OptionValue.Response.Type'
import MetadataManager from '@/shared/components/MetadataManager.Component.vue'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const store = useOptionTypeStore()
const valueStore = useOptionValueStore()
const { loading } = storeToRefs(store)
const { values: optionValues, loading: valueLoading } = storeToRefs(valueStore)
const { handleApiResult } = useApiErrorHandler()
const { showToast } = useToast()

const isEdit = computed(() => route.params.id !== undefined)
const itemId = computed(() => route.params.id as string)

const activeTab = ref(0)

const publicMetadata = ref<Record<string, any>>({})
const privateMetadata = ref<Record<string, any>>({})

const { defineField, handleSubmit, errors, setValues, setErrors, values } = useForm({
  validationSchema: toTypedSchema(OptionTypeSchema),
  initialValues: {
    name: '',
    presentation: '',

    position: 0,
    filterable: false,
  },
})

const [name] = defineField('name')
const [presentation] = defineField('presentation')
const [position] = defineField('position')
const [filterable] = defineField('filterable')

const showValueDialog = ref(false)
const isEditingValue = ref(false)
const editingValueId = ref<string | null>(null)
const submittingValue = ref(false)

const {
  defineField: defineValueField,
  handleSubmit: handleValueSubmit,
  errors: valueErrors,
  setValues: setValueFields,
  resetForm: resetValueForm,
} = useForm({
  validationSchema: toTypedSchema(OptionValueSchema),
  initialValues: {
    name: '',
    presentation: '',
    position: 0,
  },
})

const [vName] = defineValueField('name')
const [vPresentation] = defineValueField('presentation')
const [vPosition] = defineValueField('position')

const openNewValue = () => {
  isEditingValue.value = false
  editingValueId.value = null
  resetValueForm()
  showValueDialog.value = true
}

const openEditValue = (val: OptionValueListItem) => {
  isEditingValue.value = true
  editingValueId.value = val.id
  setValueFields({
    name: val.name,
    presentation: val.presentation,
    position: val.position,
  })
  showValueDialog.value = true
}

const onValueSubmit = handleValueSubmit(async (formValues) => {
  submittingValue.value = true
  const result =
    isEditingValue.value && editingValueId.value
      ? await valueStore.update(editingValueId.value, formValues)
      : await valueStore.create(itemId.value, formValues)

  if (result.isSuccess) {
    showToast(
      'success',
      t('common.success'),
      isEditingValue.value
        ? t('catalog.option_types.messages.value_update_success')
        : t('catalog.option_types.messages.value_create_success'),
    )
    showValueDialog.value = false
  } else {
    handleApiResult(result)
  }
  submittingValue.value = false
})

const deleteValue = async (val: OptionValueListItem) => {
  const result = await valueStore.remove(val.id)
  if (result.isSuccess) {
    showToast(
      'success',
      t('common.success'),
      t('catalog.option_types.messages.value_delete_success'),
    )
  }
}

const loadItem = async () => {
  if (!isEdit.value) {
    store.clearCurrent()
    valueStore.clearValues()
    publicMetadata.value = {}
    privateMetadata.value = {}
    return
  }

  const result = await store.fetchById(itemId.value)
  const handled = handleApiResult(result, { genericError: 'Failed to load option type' })

  if (handled && result.value) {
    setValues({
      name: result.value.name,
      presentation: result.value.presentation,
      position: result.value.position,
      filterable: result.value.filterable,
    })

    await valueStore.fetchValues(itemId.value)
  } else if (!handled) {
    router.push({ name: 'catalog.option-types.list' })
  }
}

const onSubmit = handleSubmit(async (formValues) => {
  const payload = {
    ...formValues,
    publicMetadata: publicMetadata.value,
    privateMetadata: privateMetadata.value,
  }

  const result = isEdit.value
    ? await store.update(itemId.value, payload)
    : await store.create(payload)

  const handled = handleApiResult(result, {
    setErrors,
    fieldNames: Object.keys(values),
    successTitle: t('common.success'),
    successMessage: isEdit.value
      ? t('catalog.option_types.messages.update_success')
      : t('catalog.option_types.messages.create_success'),
    errorTitle: t('common.error'),
  })

  if (handled && !isEdit.value && result.value) {
    router.push({ name: 'catalog.option-types.edit', params: { id: result.value.id } })
  }
  store.fetchList({ pageSize: 100 })
})

onMounted(() => {
  loadItem()
})

const cancel = () => {
  router.push({ name: 'catalog.option-types.list' })
}
</script>

<template>
  <div class="flex flex-col h-full overflow-hidden">
    <div class="flex items-center justify-between mb-4 bg-surface-0 dark:bg-surface-900 p-4 rounded-2xl border border-surface-100 dark:border-surface-800 shadow-sm">
        <div class="flex items-center gap-3 overflow-hidden">
            <div class="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary shrink-0">
                <i :class="isEdit ? 'pi pi-pencil' : 'pi pi-plus'"></i>
            </div>
            <div class="overflow-hidden">
                <h3 class="text-lg font-black tracking-tight m-0 truncate">{{ isEdit ? (presentation || 'Edit Option Type') : t('catalog.option_types.titles.create') }}</h3>
                <p class="text-xs text-surface-500 m-0 truncate">{{ isEdit ? 'Updating configuration and values' : t('catalog.option_types.descriptions.create') }}</p>
            </div>
        </div>
        <div class="flex items-center gap-2 shrink-0">
            <Button 
                :label="isEdit ? t('catalog.option_types.actions.save_edit') : t('catalog.option_types.actions.save_create')" 
                icon="pi pi-check" 
                class="rounded-xl px-6 shadow-lg shadow-primary/20" 
                :loading="loading"
                @click="onSubmit" 
            />
        </div>
    </div>

    <Card class="flex-1 border-none shadow-sm rounded-3xl bg-surface-0 dark:bg-surface-900 overflow-hidden flex flex-col">
        <template #content>
          <div class="flex flex-col h-full">
            <Tabs v-model:value="activeTab" class="flex-1 flex flex-col overflow-hidden">
                <TabList class="shrink-0">
                    <Tab :value="0">Basic Details</Tab>
                    <Tab v-if="isEdit" :value="1">Option Values</Tab>
                    <Tab :value="2">{{ t('catalog.option_types.tabs.metadata') }}</Tab>
                </TabList>

                <TabPanels class="flex-1 overflow-y-auto p-6 scrollbar-thin">
                    <TabPanel :value="0">
                        <div class="flex flex-col gap-6">
                            <div class="grid grid-cols-1 gap-6">
                                <div class="flex flex-col gap-2">
                                    <label for="name" class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t('catalog.option_types.labels.name') }}</label>
                                    <InputText id="name" v-model="name" class="w-full rounded-xl h-11" :invalid="!!errors.name" :placeholder="t('catalog.option_types.placeholders.name')" />
                                    <small class="text-red-500" v-if="errors.name">{{ errors.name }}</small>
                                </div>

                                <div class="flex flex-col gap-2">
                                    <label for="presentation" class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t('catalog.option_types.labels.presentation') }}</label>
                                    <InputText id="presentation" v-model="presentation" class="w-full rounded-xl h-11" :invalid="!!errors.presentation" :placeholder="t('catalog.option_types.placeholders.presentation')" />
                                    <small class="text-red-500" v-if="errors.presentation">{{ errors.presentation }}</small>
                                </div>

                                <div class="grid grid-cols-2 gap-4">
                                    <div class="flex flex-col gap-2">
                                        <label for="position" class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t('catalog.option_types.labels.position') }}</label>
                                        <InputNumber id="position" v-model="position" showButtons :min="0" class="w-full rounded-xl overflow-hidden" inputClass="h-11" />
                                    </div>
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t('catalog.option_types.labels.filterable') }}</label>
                                        <div class="flex items-center gap-2 h-11">
                                            <ToggleSwitch v-model="filterable" />
                                            <span class="text-xs text-surface-500">{{ filterable ? 'Visible in filters' : 'Internal only' }}</span>
                                        </div>
                                    </div>
                                </div>

                            </div>
                        </div>
                    </TabPanel>

                    <TabPanel v-if="isEdit" :value="1">
                        <div class="flex flex-col gap-4">
                            <div class="flex items-center justify-between">
                                <span class="text-sm font-bold text-surface-500 uppercase tracking-widest">Defined Values</span>
                                <Button :label="t('catalog.option_types.actions.add_value')" icon="pi pi-plus" size="small" text @click="openNewValue" />
                            </div>
                            <DataTable :value="optionValues" :loading="valueLoading" size="small" scrollable rowHover class="rounded-2xl border border-surface-100 dark:border-surface-800 overflow-hidden">
                                <Column field="name" :header="t('catalog.option_types.labels.value_name')" class="font-bold text-xs"></Column>
                                <Column field="presentation" :header="t('catalog.option_types.labels.value_presentation')" class="text-xs"></Column>
                                <Column field="position" :header="t('catalog.option_types.labels.position')" class="w-20 text-center">
                                    <template #body="{ data }">
                                        <Badge :value="data.position" severity="secondary" />
                                    </template>
                                </Column>
                                <Column class="w-24 text-right">
                                    <template #body="{ data }">
                                        <div class="flex justify-end gap-1">
                                            <Button icon="pi pi-pencil" text rounded size="small" severity="secondary" @click="openEditValue(data)" />
                                            <Button icon="pi pi-trash" text rounded size="small" severity="danger" @click="deleteValue(data)" />
                                        </div>
                                    </template>
                                </Column>
                                <template #empty>
                                    <div class="p-8 text-center text-surface-400 italic text-xs">No values defined yet.</div>
                                </template>
                            </DataTable>
                        </div>
                    </TabPanel>

                    <TabPanel :value="2">
                        <div class="flex flex-col gap-8">
                            <MetadataManager v-model="publicMetadata" title="Public Metadata" />
                            <Divider />
                            <MetadataManager v-model="privateMetadata" title="Private Metadata" />
                        </div>
                    </TabPanel>
                </TabPanels>
            </Tabs>
          </div>
        </template>
    </Card>

    <Dialog v-model:visible="showValueDialog" :header="isEditingValue ? 'Edit Value' : 'Add Value'" :modal="true" :style="{ width: '400px' }" class="rounded-3xl shadow-2xl">
      <form @submit="onValueSubmit" class="flex flex-col gap-4 mt-2">
        <div class="flex flex-col gap-2">
          <label for="vName" class="font-bold text-xs uppercase text-surface-500">{{ t('catalog.option_types.labels.value_name') }}</label>
          <InputText id="vName" v-model="vName" class="w-full rounded-xl h-11" :invalid="!!valueErrors.name" :placeholder="t('catalog.option_types.placeholders.value_name')" />
          <small class="text-red-500" v-if="valueErrors.name">{{ valueErrors.name }}</small>
        </div>

        <div class="flex flex-col gap-2">
          <label for="vPresentation" class="font-bold text-xs uppercase text-surface-500">{{ t('catalog.option_types.labels.value_presentation') }}</label>
          <InputText id="vPresentation" v-model="vPresentation" class="w-full rounded-xl h-11" :invalid="!!valueErrors.presentation" :placeholder="t('catalog.option_types.placeholders.value_presentation')" />
          <small class="text-red-500" v-if="valueErrors.presentation">{{ valueErrors.presentation }}</small>
        </div>

        <div class="flex flex-col gap-2">
          <label for="vPosition" class="font-bold text-xs uppercase text-surface-500">{{ t('catalog.option_types.labels.position') }}</label>
          <InputNumber id="vPosition" v-model="vPosition" class="w-full rounded-xl overflow-hidden" inputClass="h-11" showButtons :min="0" />
        </div>

        <div class="flex justify-end gap-2 mt-4">
          <Button type="button" label="Cancel" severity="secondary" text @click="showValueDialog = false" />
          <Button type="submit" label="Save Value" icon="pi pi-check" :loading="submittingValue" class="rounded-xl px-6" />
        </div>
      </form>
    </Dialog>
  </div>
</template>

<style scoped>
:deep(.p-tabs-list) {
    border-bottom: 1px solid var(--p-surface-100);
}
.dark :deep(.p-tabs-list) {
    border-bottom-color: var(--p-surface-800);
}
.scrollbar-thin::-webkit-scrollbar {
    width: 4px;
}
.scrollbar-thin::-webkit-scrollbar-thumb {
    background: var(--p-surface-200);
    border-radius: 4px;
}
.dark .scrollbar-thin::-webkit-scrollbar-thumb {
    background: var(--p-surface-700);
}
</style>

<style scoped>
:deep(.p-datatable-thead > tr > th) {
  background: var(--p-surface-50);
  font-size: 0.75rem;
  text-transform: uppercase;
  font-weight: 700;
  padding: 0.75rem 1rem;
}
:deep(.p-datatable-tbody > tr > td) {
  padding: 0.75rem 1rem;
}

.dark :deep(.p-datatable-thead > tr > th) {
  background: var(--p-surface-800);
}
</style>
