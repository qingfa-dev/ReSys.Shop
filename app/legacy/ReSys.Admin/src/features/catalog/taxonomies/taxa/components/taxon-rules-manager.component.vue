<script setup lang="ts">
import { ref } from 'vue'
import { useTaxonStore } from '../stores/taxon.store'
import { storeToRefs } from 'pinia'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { TaxonRuleSchema } from '../schemas/taxon.schema'
import { taxonLocales } from '../locales/taxon.locales'
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use'
import { useToast } from '@/shared/composables/toast.use'
import type { TaxonRuleListItem } from '../types/taxon.types'

const props = defineProps<{
  taxonomyId: string
  taxonId: string
}>()

const emit = defineEmits<{
  (e: 'updated'): void
}>()

const taxonStore = useTaxonStore()
const { currentRules, loading } = storeToRefs(taxonStore)
const { handleApiResult } = useApiErrorHandler()
const { showToast } = useToast()

const showRuleDialog = ref(false)
const isEditingRule = ref(false)
const editingRuleId = ref<string | null>(null)
const actionLoading = ref(false)

const {
  defineField: defineRuleField,
  handleSubmit: handleRuleSubmit,
  errors: ruleErrors,
  setValues: setRuleFields,
  resetForm: resetRuleForm,
} = useForm({
  validationSchema: toTypedSchema(TaxonRuleSchema),
  initialValues: { type: 'product_name', match_policy: 'is_equal_to', value: '', property_name: '' },
})

const [rType] = defineRuleField('type')
const [rPolicy] = defineRuleField('match_policy')
const [rValue] = defineRuleField('value')
const [rProperty] = defineRuleField('property_name')

const ruleTypeOptions = [
  { label: 'Product Name', value: 'product_name' },
  { label: 'SKU', value: 'product_sku' },
  { label: 'Price', value: 'product_price' },
  { label: 'Property', value: 'product_property' },
  { label: 'Description', value: 'product_description' },
]

const matchPolicyOptions = [
  { label: 'Equals', value: 'is_equal_to' },
  { label: 'Not Equals', value: 'is_not_equal_to' },
  { label: 'Contains', value: 'contains' },
  { label: 'Starts With', value: 'starts_with' },
  { label: 'Greater Than', value: 'greater_than' },
  { label: 'Less Than', value: 'less_than' },
]

const openNewRule = () => {
  isEditingRule.value = false
  editingRuleId.value = null
  resetRuleForm()
  showRuleDialog.value = true
}

const openEditRule = (rule: TaxonRuleListItem) => {
  isEditingRule.value = true
  editingRuleId.value = rule.id
  setRuleFields({
    type: rule.type,
    match_policy: rule.match_policy,
    value: rule.value,
    property_name: rule.property_name || '',
  })
  showRuleDialog.value = true
}

const onRuleSubmit = handleRuleSubmit(async (formValues) => {
  actionLoading.value = true
  const result =
    isEditingRule.value && editingRuleId.value
      ? await taxonStore.updateRule(
          props.taxonomyId,
          props.taxonId,
          editingRuleId.value,
          formValues,
        )
      : await taxonStore.addRule(props.taxonomyId, props.taxonId, formValues)

  if (result.success) {
    showToast(
      'success',
      taxonLocales.common?.success || 'Success',
      (isEditingRule.value
        ? taxonLocales.messages?.rule_update_success
        : taxonLocales.messages?.rule_create_success) || 'Success',
    )
    showRuleDialog.value = false
    emit('updated')
  } else {
    handleApiResult(result)
  }
  actionLoading.value = false
})

const deleteRule = async (rule: TaxonRuleListItem) => {
  const result = await taxonStore.deleteRule(props.taxonomyId, props.taxonId, rule.id)
  if (result.success) {
    showToast(
      'success',
      taxonLocales.common?.success || 'Success',
      taxonLocales.messages?.rule_delete_success || 'Rule removed',
    )
    emit('updated')
  }
}

const regenerate = async () => {
  const result = await taxonStore.regenerateProducts(props.taxonomyId, props.taxonId)
  if (result.success) {
    showToast(
      'success',
      taxonLocales.common?.success || 'Success',
      taxonLocales.messages?.regenerate_success || 'Task started',
    )
    emit('updated')
  }
}
</script>

<template>
  <div class="flex flex-col gap-4">
    <div class="flex items-center justify-between">
      <div>
        <h3 class="text-lg font-bold text-surface-900 dark:text-surface-0">
          {{ taxonLocales.titles?.rules }}
        </h3>
        <p class="text-sm text-surface-500">{{ taxonLocales.descriptions?.rules }}</p>
      </div>
      <Button
        :label="taxonLocales.actions?.add_rule"
        icon="pi pi-plus"
        size="small"
        outlined
        @click="openNewRule"
      />
    </div>

    <div v-if="currentRules.length === 0 && !loading" class="flex flex-col items-center justify-center py-12 bg-surface-50 dark:bg-surface-800/50 rounded-2xl border-2 border-dashed border-surface-200 dark:border-surface-700">
        <div class="w-12 h-12 rounded-full bg-surface-100 dark:bg-surface-800 flex items-center justify-center mb-3">
            <i class="pi pi-filter-slash text-surface-400 text-xl"></i>
        </div>
        <p class="text-surface-500 font-medium">{{ (taxonLocales.messages as any).empty_rules || 'No dynamic rules defined yet.' }}</p>
        <p class="text-xs text-surface-400 mt-1 mb-4">{{ (taxonLocales.descriptions as any).rules_empty || 'Add rules to automatically assign products to this category.' }}</p>
        <Button
            :label="taxonLocales.actions?.add_rule"
            icon="pi pi-plus"
            size="small"
            outlined
            @click="openNewRule"
        />
    </div>

    <div v-else class="flex flex-col gap-4">
        <DataTable :value="currentRules" class="border-none rounded-xl overflow-hidden shadow-sm bg-surface-0 dark:bg-surface-900" size="small">
          <Column field="type" :header="(taxonLocales.labels as any).rule_type_header || 'Property'">
            <template #body="{ data }">
              <div class="flex items-center gap-2">
                  <div class="w-2 h-2 rounded-full bg-primary"></div>
                  <span class="capitalize text-xs font-bold text-surface-700 dark:text-surface-200">
                    {{ data.type.replace('product_', '').replace('_', ' ') }}
                  </span>
              </div>
            </template>
          </Column>
          <Column field="match_policy" :header="(taxonLocales.labels as any).rule_policy_header || 'Condition'">
            <template #body="{ data }">
              <span class="text-[11px] px-2 py-0.5 rounded-full bg-surface-100 dark:bg-surface-800 text-surface-500 border border-surface-200 dark:border-surface-700">
                {{ data.match_policy.replace(/_/g, ' ') }}
              </span>
            </template>
          </Column>
          <Column field="value" :header="taxonLocales.labels?.rule_value">
            <template #body="{ data }">
              <span class="font-mono text-xs font-bold text-primary">{{ data.value }}</span>
            </template>
          </Column>
          <Column class="w-24">
            <template #body="{ data }">
              <div class="flex justify-end gap-1">
                <Button
                  icon="pi pi-pencil"
                  text
                  rounded
                  size="small"
                  severity="secondary"
                  @click="openEditRule(data)"
                />
                <Button
                  icon="pi pi-trash"
                  severity="danger"
                  text
                  rounded
                  size="small"
                  @click="deleteRule(data)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
    </div>

    <div class="p-5 bg-gradient-to-br from-primary/10 to-primary/5 rounded-2xl border border-primary/10 mt-6 relative overflow-hidden">
      <div class="absolute -right-4 -top-4 opacity-10 transform rotate-12">
          <i class="pi pi-refresh text-8xl text-primary"></i>
      </div>
      <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 relative z-10">
        <div class="flex flex-col gap-1">
          <span class="text-base font-black text-primary tracking-tight">{{ (taxonLocales.titles as any).regenerate }}</span>
          <p class="text-xs text-surface-600 dark:text-surface-400 max-w-md">
            {{ (taxonLocales.descriptions as any).rules_matching }}
          </p>
        </div>
        <Button
          :label="taxonLocales.actions?.regenerate"
          icon="pi pi-refresh"
          severity="primary"
          class="rounded-xl px-6 shadow-lg shadow-primary/20 whitespace-nowrap"
          :loading="loading"
          @click="regenerate"
        />
      </div>
    </div>

    <!-- Rule Dialog -->
    <Dialog
      v-model:visible="showRuleDialog"
      :header="isEditingRule ? 'Edit Rule' : 'Add Rule'"
      :modal="true"
      :style="{ width: '450px' }"
      class="rounded-2xl"
    >
      <form @submit="onRuleSubmit" class="flex flex-col gap-6 mt-4">
        <div class="flex flex-col gap-2">
          <label class="font-bold text-sm text-surface-700 dark:text-surface-300">{{ taxonLocales.labels?.rule_type }}</label>
          <Select
            v-model="rType"
            :options="ruleTypeOptions"
            optionLabel="label"
            optionValue="value"
            class="w-full rounded-xl"
          />
        </div>

        <div v-if="rType === 'product_property'" class="flex flex-col gap-2">
          <label class="font-bold text-sm text-surface-700 dark:text-surface-300">{{ taxonLocales.labels?.rule_property }}</label>
          <InputText v-model="rProperty" placeholder="Property name..." class="w-full rounded-xl" />
        </div>

        <div class="flex flex-col gap-2">
          <label class="font-bold text-sm text-surface-700 dark:text-surface-300">{{ taxonLocales.labels?.rule_policy }}</label>
          <Select
            v-model="rPolicy"
            :options="matchPolicyOptions"
            optionLabel="label"
            optionValue="value"
            class="w-full rounded-xl"
          />
        </div>

        <div class="flex flex-col gap-2">
          <label class="font-bold text-sm text-surface-700 dark:text-surface-300">{{ taxonLocales.labels?.rule_value }}</label>
          <InputText
            v-model="rValue"
            :placeholder="taxonLocales.placeholders?.rule_value"
            class="w-full rounded-xl"
          />
        </div>

        <div class="flex justify-end gap-2 mt-4">
          <Button
            type="button"
            label="Cancel"
            severity="secondary"
            text
            @click="showRuleDialog = false"
            class="rounded-xl"
          />
          <Button type="submit" label="Save Rule" icon="pi pi-check" :loading="actionLoading" class="rounded-xl px-6" />
        </div>
      </form>
    </Dialog>
  </div>
</template>
