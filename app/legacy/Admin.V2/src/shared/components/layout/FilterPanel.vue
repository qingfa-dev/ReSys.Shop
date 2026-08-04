<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useI18n } from 'vue-i18n'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import DatePicker from 'primevue/datepicker'
import InputNumber from 'primevue/inputnumber'
import Button from 'primevue/button'

const { t } = useI18n()

export interface ColumnFilterDef {
  field: string
  label: string
  type: 'text' | 'select' | 'date-range' | 'number-range'
  options?: { label: string; value: string }[]
}

export interface FilterConfig {
  field: string
  operator: 'Equal' | 'NotEqual' | 'GreaterThanOrEqual' | 'LessThanOrEqual' | 'Contains' | 'StartsWith' | 'EndsWith'
  value: string | number
  label: string
}

const props = defineProps<{
  definitions: ColumnFilterDef[]
  activeFilters: readonly FilterConfig[]
  visible: boolean
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  apply: [filters: FilterConfig[]]
  clear: []
}>()

interface LocalState {
  textValue: string
  selectValue: string
  numberFrom: number | undefined
  numberTo: number | undefined
  dateFrom: Date | undefined
  dateTo: Date | undefined
}

function createDefaultState(): LocalState {
  return {
    textValue: '',
    selectValue: '',
    numberFrom: undefined,
    numberTo: undefined,
    dateFrom: undefined,
    dateTo: undefined,
  }
}

const localFilters = ref<Record<string, LocalState>>(reactive({}))

function getState(field: string): LocalState {
  let state = localFilters.value[field]
  if (!state) {
    state = createDefaultState()
    localFilters.value[field] = state
  }
  return state
}

function initLocal() {
  for (const def of props.definitions) {
    const existing = props.activeFilters.filter(f => f.field === def.field)
    const state = getState(def.field)
    if (existing.length > 0) {
      if (def.type === 'text' || def.type === 'select') {
        state.textValue = String(existing[0]!.value)
        state.selectValue = String(existing[0]!.value)
      } else if (def.type === 'number-range') {
        const fromFilter = existing.find(f => f.operator === 'GreaterThanOrEqual')
        const toFilter = existing.find(f => f.operator === 'LessThanOrEqual')
        if (fromFilter) state.numberFrom = Number(fromFilter.value)
        if (toFilter) state.numberTo = Number(toFilter.value)
      } else if (def.type === 'date-range') {
        const fromFilter = existing.find(f => f.operator === 'GreaterThanOrEqual')
        const toFilter = existing.find(f => f.operator === 'LessThanOrEqual')
        if (fromFilter) state.dateFrom = new Date(String(fromFilter.value))
        if (toFilter) state.dateTo = new Date(String(toFilter.value))
      }
    }
  }
}

initLocal()

function apply() {
  const filters: FilterConfig[] = []

  for (const def of props.definitions) {
    const field = def.field
    const state = getState(field)

    if (def.type === 'text') {
      if (state.textValue) {
        filters.push({
          field,
          operator: 'Contains',
          value: state.textValue,
          label: `${def.label}: ${state.textValue}`,
        })
      }
    } else if (def.type === 'select') {
      if (state.selectValue) {
        const option = def.options?.find(o => o.value === state.selectValue)
        filters.push({
          field,
          operator: 'Equal',
          value: state.selectValue,
          label: `${def.label}: ${option?.label ?? state.selectValue}`,
        })
      }
    } else if (def.type === 'number-range') {
      if (state.numberFrom != null && state.numberTo != null) {
        const rangeLabel = `${def.label}: ${state.numberFrom} - ${state.numberTo}`
        filters.push({ field, operator: 'GreaterThanOrEqual', value: state.numberFrom, label: rangeLabel })
        filters.push({ field, operator: 'LessThanOrEqual', value: state.numberTo, label: rangeLabel })
      } else if (state.numberFrom != null) {
        filters.push({ field, operator: 'GreaterThanOrEqual', value: state.numberFrom, label: `${def.label}: >= ${state.numberFrom}` })
      } else if (state.numberTo != null) {
        filters.push({ field, operator: 'LessThanOrEqual', value: state.numberTo, label: `${def.label}: <= ${state.numberTo}` })
      }
    } else if (def.type === 'date-range') {
      if (state.dateFrom && state.dateTo) {
        const rangeLabel = `${def.label}: ${state.dateFrom.toISOString().slice(0, 10)} - ${state.dateTo.toISOString().slice(0, 10)}`
        filters.push({ field, operator: 'GreaterThanOrEqual', value: state.dateFrom.toISOString(), label: rangeLabel })
        filters.push({ field, operator: 'LessThanOrEqual', value: state.dateTo.toISOString(), label: rangeLabel })
      } else if (state.dateFrom) {
        filters.push({ field, operator: 'GreaterThanOrEqual', value: state.dateFrom.toISOString(), label: `${def.label}: >= ${state.dateFrom.toISOString().slice(0, 10)}` })
      } else if (state.dateTo) {
        filters.push({ field, operator: 'LessThanOrEqual', value: state.dateTo.toISOString(), label: `${def.label}: <= ${state.dateTo.toISOString().slice(0, 10)}` })
      }
    }
  }

  emit('apply', filters)
  emit('update:visible', false)
}

function clearAll() {
  localFilters.value = reactive({})
  for (const def of props.definitions) {
    localFilters.value[def.field] = createDefaultState()
  }
  emit('clear')
  emit('update:visible', false)
}
</script>

<template>
  <div
    v-if="visible"
    class="rounded-lg border border-surface-200 bg-white p-4 mb-4 dark:border-surface-700 dark:bg-surface-900"
  >
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div v-for="def in definitions" :key="def.field" class="flex flex-col gap-1">
        <label class="text-sm font-medium">{{ def.label }}</label>

        <template v-if="def.type === 'text'">
          <InputText v-model="getState(def.field).textValue" :placeholder="def.label" size="small" />
        </template>

        <template v-else-if="def.type === 'select'">
          <Select
            v-model="getState(def.field).selectValue"
            :options="def.options ?? []"
            option-label="label"
            option-value="value"
            :placeholder="t('general.filters.selectPlaceholder')"
            size="small"
          />
        </template>

        <template v-else-if="def.type === 'number-range'">
          <div class="flex items-center gap-1">
            <InputNumber v-model="getState(def.field).numberFrom" :placeholder="t('general.filters.from')" size="small" class="flex-1" />
            <span class="text-xs text-surface-500">-</span>
            <InputNumber v-model="getState(def.field).numberTo" :placeholder="t('general.filters.to')" size="small" class="flex-1" />
          </div>
        </template>

        <template v-else-if="def.type === 'date-range'">
          <div class="flex items-center gap-1">
            <DatePicker v-model="getState(def.field).dateFrom" :placeholder="t('general.filters.from')" size="small" class="flex-1" />
            <span class="text-xs text-surface-500">-</span>
            <DatePicker v-model="getState(def.field).dateTo" :placeholder="t('general.filters.to')" size="small" class="flex-1" />
          </div>
        </template>
      </div>
    </div>
    <div class="flex items-center justify-end gap-2 mt-4">
      <Button :label="t('general.filters.clearAll')" text size="small" @click="clearAll" />
      <Button :label="t('general.filters.apply')" size="small" @click="apply" />
    </div>
  </div>
</template>
