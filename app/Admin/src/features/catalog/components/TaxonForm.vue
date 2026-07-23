<script setup lang="ts">
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import { TaxonForms } from '../schemas'
import { TaxonFormMapper } from '../mappers/taxon.mapper'
import { TaxonApi } from '../api/taxon.api'
import type { TaxonResponse } from '../types'

const emit = defineEmits<{
  saved: [value: TaxonResponse]
  cancelled: []
}>()

const props = defineProps<{
  taxonomyId: string
  taxon?: TaxonResponse | null
}>()

const { t } = useI18n()
const schemas = new TaxonForms(t)
const saving = ref(false)

const { handleSubmit, defineField, errors } = useForm({
  validationSchema: toTypedSchema(props.taxon ? schemas.update() : schemas.create()),
  initialValues: { name: props.taxon?.name ?? '', presentation: props.taxon?.presentation ?? undefined },
})

const [name] = defineField('name')
const [presentation] = defineField('presentation')

const onSubmit = handleSubmit(async (values) => {
  saving.value = true
  const result = props.taxon
    ? await TaxonApi.update(props.taxonomyId, props.taxon.id, TaxonFormMapper.toUpdate(values))
    : await TaxonApi.create(props.taxonomyId, TaxonFormMapper.toCreate(values))
  saving.value = false
  if (result.isSuccess) {
    emit('saved', result.value)
  }
})
</script>

<template>
  <form @submit="onSubmit" class="flex flex-col gap-3">
    <div>
      <label class="block font-medium mb-1">{{ t('catalog.taxa.labels.name') }}</label>
      <InputText v-model="name" class="w-full" :invalid="!!errors.name" />
      <small v-if="errors.name" class="text-red-500">{{ errors.name }}</small>
    </div>
    <div>
      <label class="block font-medium mb-1">{{ t('catalog.taxa.labels.presentation') }}</label>
      <InputText v-model="presentation" class="w-full" />
    </div>
    <div class="flex justify-content-end gap-2">
      <Button :label="t('catalog.taxa.actions.cancel')" class="p-button-secondary" @click="emit('cancelled')" />
      <Button type="submit" :label="t('catalog.taxa.actions.save')" :loading="saving" :disabled="saving" />
    </div>
  </form>
</template>
