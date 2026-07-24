<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import Sidebar from 'primevue/sidebar'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import RadioButton from 'primevue/radiobutton'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { TaxonRuleForms } from '../schemas'
import { TaxonRuleFormMapper } from '../mappers/taxon-rule.mapper'
import { TaxonRuleApi } from '../api/taxon-rule.api'
import type { TaxonRuleListItem } from '../types'

const props = defineProps<{
  taxonomyId: string
  taxonId: string
}>()

const { t } = useI18n()
const toast = useToast()
const { confirmDelete } = useConfirm()

const rules = ref<TaxonRuleListItem[]>([])
const loading = ref(false)
const saving = ref(false)
const syncing = ref(false)
const sidebarVisible = ref(false)
const editingRule = ref<TaxonRuleListItem | null>(null)

const schemas = new TaxonRuleForms(t)
const { handleSubmit, defineField, errors, resetForm, setValues } = useForm({
  validationSchema: toTypedSchema(schemas.create()),
})

const [type] = defineField('type')
const [matchPolicy] = defineField('matchPolicy')
const [value] = defineField('value')

const ruleTypes = ['brand', 'taxon', 'price', 'option', 'custom']

async function load() {
  loading.value = true
  const result = await TaxonRuleApi.getMany(props.taxonomyId, props.taxonId)
  if (result.isSuccess) {
    rules.value = result.value
  } else {
    toast.error(result.message ?? 'Failed to load rules')
  }
  loading.value = false
}

function openAdd() {
  editingRule.value = null
  resetForm()
  sidebarVisible.value = true
}

function openEdit(rule: TaxonRuleListItem) {
  editingRule.value = rule
  setValues({
    type: rule.type,
    matchPolicy: rule.matchPolicy,
    value: rule.value,
  })
  sidebarVisible.value = true
}

const submit = handleSubmit(async (values) => {
  saving.value = true
  const data = TaxonRuleFormMapper.toCreate(values)
  const result = editingRule.value
    ? await TaxonRuleApi.update(props.taxonomyId, props.taxonId, editingRule.value.id, data)
    : await TaxonRuleApi.create(props.taxonomyId, props.taxonId, data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(editingRule.value ? 'Rule updated' : 'Rule added')
    sidebarVisible.value = false
    await load()
  } else {
    toast.error(result.message ?? 'Failed to save rule')
  }
})

async function remove(rule: TaxonRuleListItem) {
  confirmDelete({
    target: `rule ${rule.type}`,
    onAccept: async () => {
      const result = await TaxonRuleApi.delete(props.taxonomyId, props.taxonId, rule.id)
      if (result.isSuccess) {
        toast.success('Rule removed')
        await load()
      } else {
        toast.error(result.message ?? 'Failed to remove rule')
      }
    },
  })
}

async function syncAll() {
  syncing.value = true
  const result = await TaxonRuleApi.sync(props.taxonomyId, props.taxonId, {
    rules: rules.value.map(r => ({ id: r.id, type: r.type, matchPolicy: r.matchPolicy, value: r.value })),
  })
  syncing.value = false
  if (result.isSuccess) {
    toast.success('Rules synced')
    rules.value = result.value.rules
  } else {
    toast.error(result.message ?? 'Failed to sync rules')
  }
}

onMounted(load)
</script>

<template>
  <div>
    <div class="flex justify-content-between align-items-center mb-3">
      <h4 class="m-0">{{ t('catalog.taxa.rules.title') }}</h4>
      <div class="flex gap-2">
        <Button :label="t('catalog.taxa.rules.sync')" icon="pi pi-refresh" size="small" class="p-button-outlined" :loading="syncing" :disabled="syncing" @click="syncAll" />
        <Button :label="t('catalog.taxa.rules.add')" icon="pi pi-plus" size="small" @click="openAdd" />
      </div>
    </div>
    <DataTable :value="rules" :loading="loading" striped-rows size="small">
      <Column field="type" :header="t('catalog.taxa.rules.type')" />
      <Column field="matchPolicy" :header="t('catalog.taxa.rules.match_policy')" />
      <Column field="value" :header="t('catalog.taxa.rules.value')" />
      <Column header="">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" size="small" class="p-button-text mr-1" @click="openEdit(data)" />
          <Button icon="pi pi-trash" size="small" class="p-button-text p-button-danger" @click="remove(data)" />
        </template>
      </Column>
    </DataTable>
    <Sidebar v-model:visible="sidebarVisible" :header="editingRule ? 'Edit Rule' : 'Add Rule'" position="right">
      <form @submit="submit" class="flex flex-column gap-3">
        <div>
          <label class="block font-medium mb-1">{{ t('catalog.taxa.rules.type') }}</label>
          <Select v-model="type" :options="ruleTypes" class="w-full" :invalid="!!errors.type" />
          <small v-if="errors.type" class="text-red-500">{{ errors.type }}</small>
        </div>
        <div>
          <label class="block font-medium mb-1">{{ t('catalog.taxa.rules.match_policy') }}</label>
          <div class="flex gap-3">
            <div class="flex align-items-center gap-1">
              <RadioButton v-model="matchPolicy" input-id="match-all" value="All" />
              <label for="match-all">All</label>
            </div>
            <div class="flex align-items-center gap-1">
              <RadioButton v-model="matchPolicy" input-id="match-any" value="Any" />
              <label for="match-any">Any</label>
            </div>
          </div>
          <small v-if="errors.matchPolicy" class="text-red-500">{{ errors.matchPolicy }}</small>
        </div>
        <div>
          <label class="block font-medium mb-1">{{ t('catalog.taxa.rules.value') }}</label>
          <InputText v-model="value" class="w-full" :invalid="!!errors.value" />
          <small v-if="errors.value" class="text-red-500">{{ errors.value }}</small>
        </div>
        <div class="flex justify-content-end gap-2 mt-3">
          <Button type="button" :label="t('catalog.taxa.actions.cancel')" class="p-button-secondary" @click="sidebarVisible = false" />
          <Button type="submit" :label="t('catalog.taxa.actions.save')" :loading="saving" :disabled="saving" />
        </div>
      </form>
    </Sidebar>
  </div>
</template>
