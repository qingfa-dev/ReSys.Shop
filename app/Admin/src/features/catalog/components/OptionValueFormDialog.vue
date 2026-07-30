<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Dialog from 'primevue/dialog'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { OptionValueApi } from '../services/optionValueApi'
import { optionValueSchema } from '../validations/optionValue'
import type { OptionValueForm } from '../validations/optionValue'
import type { OptionValueListItem } from '../types/optionValue'

interface Props {
  visible: boolean
  optionTypeId: string
  editingValue: OptionValueListItem | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'saved'): void
}>()

const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!props.editingValue)
const dialogTitle = computed(() => isEdit.value ? 'Edit Option Value' : 'Add Option Value')

const form = ref<OptionValueForm>({
  optionTypeId: props.optionTypeId,
  name: '',
  presentation: '',
  position: 1,
})

const fieldErrors = ref<Record<string, string>>({})
const saving = ref(false)

watch(
  () => props.visible,
  (v) => {
    if (v) {
      fieldErrors.value = {}
      if (props.editingValue) {
        form.value = {
          optionTypeId: props.editingValue.optionTypeId,
          name: props.editingValue.name,
          presentation: props.editingValue.presentation,
          position: props.editingValue.position,
        }
      } else {
        form.value = {
          optionTypeId: props.optionTypeId,
          name: '',
          presentation: '',
          position: 1,
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
  const parsed = optionValueSchema.safeParse(form.value)

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
    optionTypeId: data.optionTypeId,
    name: data.name,
    presentation: data.presentation,
    position: data.position,
  }

  const result = isEdit.value
    ? await OptionValueApi.updateOptionValue(props.editingValue!.id, request)
    : await OptionValueApi.createOptionValue(request)

  saving.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Option value updated' : 'Option value created')
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
    :style="{ width: '450px' }"
    @update:visible="close"
  >
    <div class="flex flex-col gap-4">
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Name</label>
        <InputText v-model="form.name" fluid :invalid="!!fieldErrors.name" />
        <small v-if="fieldErrors.name" class="text-red-500">{{ fieldErrors.name }}</small>
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Presentation</label>
        <InputText v-model="form.presentation" fluid :invalid="!!fieldErrors.presentation" />
        <small v-if="fieldErrors.presentation" class="text-red-500">{{ fieldErrors.presentation }}</small>
      </div>
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium">Position</label>
        <InputNumber v-model="form.position" fluid :min="-1" :invalid="!!fieldErrors.position" />
        <small v-if="fieldErrors.position" class="text-red-500">{{ fieldErrors.position }}</small>
      </div>
    </div>
    <template #footer>
      <Button label="Cancel" severity="secondary" @click="close" />
      <Button label="Save" severity="primary" :loading="saving" @click="onSave" />
    </template>
  </Dialog>
</template>
