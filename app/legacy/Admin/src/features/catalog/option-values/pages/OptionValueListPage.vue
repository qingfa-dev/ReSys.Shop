<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useOptionValueStore } from '../store/option-value.store'
import { useOptionTypeStore } from '@/features/catalog/option-types'
import { storeToRefs } from 'pinia'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { z } from 'zod'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'
import PageShell from '@/shared/components/navigation/PageShell.vue'
import { createOptionValueSchema } from '../types/option-value.field'
import { FilterMatchMode, FilterOperator } from '@primevue/core/api'
import type { DataTablePageEvent, DataTableSortEvent, DataTableFilterMeta } from 'primevue/datatable'
import { getFilterValue } from '@/common/api/types/filter.types'
import { useToast } from '@/common/composables/toast.use'
import ConfirmDialog from '@/shared/components/overlays/ConfirmDialog.vue'
import ModalDialog from '@/shared/components/overlays/ModalDialog.vue'
import { useApiErrorHandler } from '@/common/composables/api-error-handler.use'
import FormField from '@/shared/components/form/FormField.vue'
import { QueryBuilder } from '@/common/utils/query-builder.utils'
import type { OptionValueListItem } from '../models/option-value.response'

const { t } = useI18n()

const route = useRoute()
const router = useRouter()
const store = useOptionValueStore()
const typeStore = useOptionTypeStore()
const { items, loading, totalRecords, query } = storeToRefs(store)
const { showToast } = useToast()
const { handleApiResult } = useApiErrorHandler()

const optionTypes = ref<{ label: string, value: string }[]>([])

const showDialog = ref(false)
const isEditing = ref(false)
const editingId = ref<string | null>(null)
const submitting = ref(false)

const {
  defineField,
  handleSubmit: handleFormSubmit,
  errors: formErrors,
  setValues: setFormValues,
  resetForm: resetFormFields
} = useForm({
  validationSchema: toTypedSchema(createOptionValueSchema(t).extend({
    optionTypeId: z.string().min(1, 'Option type is required')
  })),
  initialValues: {
    name: '',
    presentation: '',
    position: 0,
    optionTypeId: ''
  }
})

const [name] = defineField('name')
const [presentation] = defineField('presentation')
const [position] = defineField('position')
const [optionTypeId] = defineField('optionTypeId')

const openNew = () => {
  isEditing.value = false
  editingId.value = null
  resetFormFields()

  const activeFilterId = getFilterValue(filters.value, 'optionTypeId') as string | null
  if (activeFilterId) {
    optionTypeId.value = activeFilterId
  } else if (optionTypes.value.length > 0 && optionTypes.value[0]) {
    optionTypeId.value = optionTypes.value[0].value
  }

  showDialog.value = true
}

const openEdit = (val: OptionValueListItem) => {
  isEditing.value = true
  editingId.value = val.id
  setFormValues({
    name: val.name,
    presentation: val.presentation,
    position: val.position,
    optionTypeId: val.optionTypeId
  })
  showDialog.value = true
}

const onFormSubmit = handleFormSubmit(async (values) => {
  submitting.value = true
  const result = isEditing.value && editingId.value
    ? await store.update(editingId.value, values)
    : await store.create(values.optionTypeId, values)

  if (result.isSuccess) {
    showToast('success', t('common.success'), isEditing.value ? t('catalog.option_values.messages.update_success') : t('catalog.option_values.messages.create_success'))
    showDialog.value = false
    store.fetchList()
  } else {
    handleApiResult(result)
  }
  submitting.value = false
})

const filters = ref<DataTableFilterMeta>({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  name: { operator: FilterOperator.AND, constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }] },
  optionTypeId: { value: null, matchMode: FilterMatchMode.EQUALS }
})

const loadItems = async () => {
  const typesResult = await typeStore.fetchList({ pageSize: 100 })
  const data = typesResult?.isSuccess ? 'items' in typesResult ? typesResult.items : typesResult.value : []
  if (data) {
    optionTypes.value = data.map(t => ({ label: t.presentation || t.name, value: t.id }))
  }

  if (route.query.optionTypeId) {
    filters.value.optionTypeId = { value: route.query.optionTypeId as string, matchMode: FilterMatchMode.EQUALS }
    query.value.optionTypeId = route.query.optionTypeId as string
  }

  await store.fetchList()
}

const onPage = (event: DataTablePageEvent) => {
  store.fetchList({
    page: event.page !== undefined ? event.page + 1 : 1,
    pageSize: event.rows
  })
}

const onSort = (event: DataTableSortEvent) => {
  const builder = new QueryBuilder()
  if (event.sortField) {
    builder.orderBy(event.sortField as string, event.sortOrder === -1 ? 'desc' : 'asc')
  }
  store.fetchList({ sort: builder.build().sort, page: 1 })
}

const onFilter = () => {
  const globalValue = getFilterValue(filters.value, 'global') as string | null
  const nameFilter = filters.value.name as { constraints: { value: string | null }[] }
  const typeFilterValue = getFilterValue(filters.value, 'optionTypeId') as string | null

  const builder = new QueryBuilder()

  if (nameFilter.constraints[0]?.value) {
    builder.where('Name', '*', nameFilter.constraints[0].value)
  }

  const built = builder.build()

  store.fetchList({
    search: globalValue || undefined,
    searchFields: globalValue ? ['Name', 'Presentation'] : undefined,
    filter: built.filter,
    optionTypeId: typeFilterValue || undefined,
    page: 1
  })
}

const clearFilters = () => {
  filters.value = {
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    name: { operator: FilterOperator.AND, constraints: [{ value: null, matchMode: FilterMatchMode.CONTAINS }] },
    optionTypeId: { value: null, matchMode: FilterMatchMode.EQUALS }
  }
  onFilter()
  router.replace({ query: {} })
}

const deleteOptionValue = async (item: OptionValueListItem) => {
  const result = await store.remove(item.id)
  if (result.isSuccess) {
    showToast('success', t('common.success'), t('catalog.option_values.messages.delete_success'))
    store.fetchList()
  }
}

watch(() => route.query.optionTypeId, (newVal) => {
  if (newVal && newVal !== getFilterValue(filters.value, 'optionTypeId')) {
    filters.value.optionTypeId = { value: newVal as string, matchMode: FilterMatchMode.EQUALS }
    onFilter()
  }
})

onMounted(() => {
  loadItems()
})
</script>

<template>
  <PageShell maxWidth="7xl">
    <PageHeader :title="t('catalog.option_values.titles.list')"
      :description="t('catalog.option_values.descriptions.list')">
      <template #badge>
        <Badge :value="totalRecords" severity="info" />
      </template>
      <template #actions>
        <Button :label="t('catalog.option_values.actions.add_value')" icon="pi pi-plus" @click="openNew" />
      </template>
    </PageHeader>

    <DataTable v-model:filters="filters" :value="items" :loading="loading" :totalRecords="totalRecords" lazy paginator
      :rows="query.pageSize" :first="((query.page || 1) - 1) * (query.pageSize || 10)" @page="onPage" @sort="onSort"
      @filter="onFilter" filterDisplay="menu" removableSort scrollable rowHover>
      <template #header>
        <div class="flex flex-col items-center justify-between gap-4 md:flex-row">
          <div class="flex gap-2 w-full md:w-auto">
            <IconField iconPosition="left" class="w-full md:w-64">
              <InputIcon class="pi pi-search" />
              <InputText v-model="(filters.global as { value: string | null }).value"
                :placeholder="t('catalog.option_values.placeholders.search')" @keyup.enter="onFilter"
                class="w-full rounded-xl" />
            </IconField>
            <Select v-model="(filters.optionTypeId as { value: string | null }).value" :options="optionTypes"
              optionLabel="label" optionValue="value" :placeholder="t('catalog.option_values.placeholders.option_type')"
              showClear @change="onFilter" class="w-full md:w-48 rounded-xl" />
          </div>

          <Button type="button" icon="pi pi-filter-slash" :label="t('catalog.option_values.table.clear_filter')"
            outlined @click="clearFilters" class="w-full rounded-xl md:w-auto" />
        </div>
      </template>

      <template #empty>
        <div class="flex flex-col items-center justify-center py-20 text-surface-400">
          <i class="mb-4 text-6xl pi pi-list opacity-20"></i>
          <p class="text-xl font-medium">{{ t('catalog.option_values.messages.empty_list') }}</p>
        </div>
      </template>

      <Column field="name" :header="t('catalog.option_values.table.name')" sortable>
        <template #body="{ data }">
          <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
        </template>
        <template #filter="{ filterModel, filterCallback }">
          <InputText v-model="filterModel.value" type="text" @keydown.enter="filterCallback()" class="p-column-filter"
            :placeholder="t('catalog.option_values.table.filter_placeholder')" />
        </template>
      </Column>

      <Column field="presentation" :header="t('catalog.option_values.table.presentation')" sortable></Column>

      <Column field="position" :header="t('catalog.option_values.table.position')" sortable class="w-24 text-center">
        <template #body="{ data }">
          <Badge :value="data.position" severity="secondary" />
        </template>
      </Column>

      <Column class="w-32 text-right">
        <template #body="{ data }">
          <div class="flex justify-end gap-1">
            <Button icon="pi pi-pencil" severity="secondary" text rounded @click="openEdit(data)" />
            <ConfirmDialog
              :header="t('common.warning')"
              :message="`Are you sure you want to delete ${data.name}?`"
              :accept-label="t('catalog.option_values.actions.delete')"
              :reject-label="t('catalog.option_values.actions.cancel')"
              @confirm="deleteOptionValue(data)" />
          </div>
        </template>
      </Column>
    </DataTable>

    <ModalDialog v-model="showDialog" :header="isEditing ? 'Edit Option Value' : 'Add Option Value'" maxWidth="max-w-[450px]">
      <form @submit="onFormSubmit" class="flex flex-col gap-4 mt-2">
        <FormField :label="t('catalog.option_values.labels.option_type')" name="optionTypeId" :error="formErrors.optionTypeId">
          <Select v-model="optionTypeId" :options="optionTypes" optionLabel="label" optionValue="value" class="w-full"
            :placeholder="t('catalog.option_values.placeholders.option_type')" :disabled="isEditing" />
        </FormField>

        <FormField :label="t('catalog.option_values.labels.name')" name="name" :error="formErrors.name">
          <InputText id="vName" v-model="name" class="w-full" :invalid="!!formErrors.name"
            :placeholder="t('catalog.option_values.placeholders.name')" />
        </FormField>

        <FormField :label="t('catalog.option_values.labels.presentation')" name="presentation" :error="formErrors.presentation">
          <InputText id="vPresentation" v-model="presentation" class="w-full" :invalid="!!formErrors.presentation"
            :placeholder="t('catalog.option_values.placeholders.presentation')" />
        </FormField>

        <FormField :label="t('catalog.option_values.labels.position')" name="position">
          <InputNumber id="vPosition" v-model="position" class="w-full" showButtons :min="0" />
        </FormField>

        <div class="flex justify-end gap-2 mt-4">
          <Button type="button" :label="t('common.cancel')" severity="secondary" text @click="showDialog = false" />
          <Button type="submit" :label="t('common.save')" icon="pi pi-check" :loading="submitting" />
        </div>
      </form>
    </ModalDialog>
  </PageShell>
</template>
