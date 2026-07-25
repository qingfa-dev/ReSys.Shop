<script setup lang="ts">
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import { AppCard } from '@/shared/components'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import { OptionValueForms } from '../schemas'
import { OptionValueFormMapper } from '../mappers/option-value.mapper'
import { OptionValueApi } from '../api/option-value.api'
import type { OptionValueResponse } from '../types'

import { useToast } from '@/shared/composables/useToast'

const emit = defineEmits<{
  saved: [value: OptionValueResponse]
  cancelled: []
}>()

const props = defineProps<{
  optionTypeId: string
  optionValue?: OptionValueResponse | null
}>()

const { t } = useI18n()
const toast = useToast()
const schemas = new OptionValueForms(t)
const saving = ref(false)

const { handleSubmit, defineField, errors } = useForm({
  validationSchema: toTypedSchema(props.optionValue ? schemas.update() : schemas.create()),
  initialValues: { name: props.optionValue?.name ?? '', presentation: props.optionValue?.presentation ?? undefined },
})

const [name] = defineField('name')
const [presentation] = defineField('presentation')

const onSubmit = handleSubmit(async (values) => {
  saving.value = true
  try {
    const result = props.optionValue
      ? await OptionValueApi.update(props.optionTypeId, props.optionValue.id, OptionValueFormMapper.toUpdate(values))
      : await OptionValueApi.create(props.optionTypeId, OptionValueFormMapper.toCreate(values))
    saving.value = false
    if (result.isSuccess) {
      emit('saved', result.value)
    } else {
      toast.error(result.message ?? 'Failed to save option value')
    }
  } catch (err) {
    console.error(err)
    saving.value = false
    toast.error('Failed to save option value')
  }
})
</script>

<template>
  <AppCard>
    <form @submit="onSubmit" class="flex flex-col gap-3">
      <div>
        <label class="block font-medium mb-1">{{ t('catalog.option_values.labels.name') }}</label>
        <InputText v-model="name" class="w-full" :invalid="!!errors.name" />
        <small v-if="errors.name" class="text-red-500">{{ errors.name }}</small>
      </div>
      <div>
        <label class="block font-medium mb-1">{{ t('catalog.option_values.labels.presentation') }}</label>
        <InputText v-model="presentation" class="w-full" />
      </div>
      <div class="flex justify-end gap-2">
        <Button :label="t('catalog.option_values.actions.cancel')" severity="secondary" @click="emit('cancelled')" />
        <Button type="submit" :label="t('catalog.option_values.actions.save')" :loading="saving" :disabled="saving" />
      </div>
    </form>
  </AppCard>
</template>
