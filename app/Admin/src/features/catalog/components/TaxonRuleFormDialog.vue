<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Dialog from 'primevue/dialog'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { TaxonRuleApi } from '../services/taxonRuleApi'
import { taxonRuleSchema } from '../validations/taxonRule'
import type { TaxonRuleForm } from '../validations/taxonRule'
import type { TaxonRuleListItem } from '../types/taxonRule'
import { TAXON_RULE_TYPES, TAXON_RULE_MATCH_POLICIES } from '../types/taxonRule'

interface Props {
  visible: boolean
  taxonId: string
  editingRule: TaxonRuleListItem | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'saved'): void
}>()

const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!props.editingRule)
const dialogTitle = computed(() => isEdit.value ? 'Edit Rule' : 'Add Rule')

const form = ref<TaxonRuleForm>({
  type: 'product_name',
  matchPolicy: 'contains',
  value: '',
})

const fieldErrors = ref<Record<string, string>>({})
const saving = ref(false)

watch(
  () => props.visible,
  (v) => {
    if (v) {
      fieldErrors.value = {}
      if (props.editingRule) {
        form.value = {
          type: props.editingRule.type,
          matchPolicy: props.editingRule.matchPolicy,
          value: props.editingRule.value,
        }
      } else {
        form.value = {
          type: 'product_name',
          matchPolicy: 'contains',
          value: '',
        }
      }
    }
  },
)

function close() {
  emit('update:visible', false)
}

async function onSave() {
  fieldErrors.value = {}
  const parsed = taxonRuleSchema.safeParse(form.value)

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
    type: data.type,
    matchPolicy: data.matchPolicy,
    value: data.value,
  }

  const result = isEdit.value
    ? await TaxonRuleApi.updateRule(props.editingRule!.id, { ...request, taxonId: props.taxonId })
    : await TaxonRuleApi.createRule({ ...request, taxonId: props.taxonId })

  saving.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Rule updated' : 'Rule created')
    close()
    emit('saved')
  } else {
    handleResult(result)
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="dialogTitle"
    :modal="true"
    :style="{ width: '500px' }"
    @update:visible="close"
  >
    <div class="flex flex-col gap-4">
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Type</label>
        <Select v-model="form.type" :options="TAXON_RULE_TYPES" fluid :invalid="!!fieldErrors.type" />
        <small v-if="fieldErrors.type" class="text-red-500">{{ fieldErrors.type }}</small>
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Match Policy</label>
        <Select v-model="form.matchPolicy" :options="TAXON_RULE_MATCH_POLICIES" fluid :invalid="!!fieldErrors.matchPolicy" />
        <small v-if="fieldErrors.matchPolicy" class="text-red-500">{{ fieldErrors.matchPolicy }}</small>
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Value</label>
        <InputText v-model="form.value" fluid :invalid="!!fieldErrors.value" />
        <small v-if="fieldErrors.value" class="text-red-500">{{ fieldErrors.value }}</small>
      </div>
    </div>
    <template #footer>
      <Button label="Cancel" severity="secondary" @click="close" />
      <Button label="Save" severity="primary" :loading="saving" @click="onSave" />
    </template>
  </Dialog>
</template>
