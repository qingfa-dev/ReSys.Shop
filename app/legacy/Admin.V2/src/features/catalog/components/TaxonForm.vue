<script setup lang="ts">
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import { AppCard } from '@/shared/components'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import { TaxonForms } from '../schemas'
import { TaxonFormMapper } from '../mappers/taxon.mapper'
import { TaxonApi } from '../api/taxon.api'
import type { TaxonResponse } from '../types'
import TaxonRuleManager from './TaxonRuleManager.vue'

import { useToast } from '@/shared/composables/useToast'

const emit = defineEmits<{
  saved: [value: TaxonResponse]
  cancelled: []
}>()

const props = defineProps<{
  taxonomyId: string
  taxon?: TaxonResponse | null
}>()

const { t } = useI18n()
const toast = useToast()
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
  try {
    const result = props.taxon
      ? await TaxonApi.update(props.taxonomyId, props.taxon.id, TaxonFormMapper.toUpdate(values))
      : await TaxonApi.create(props.taxonomyId, TaxonFormMapper.toCreate(values))
    saving.value = false
    if (result.isSuccess) {
      emit('saved', result.value)
    } else {
      toast.error(result.message ?? t('catalog.taxa.messages.save_failed'))
    }
  } catch (err) {
    console.error(err)
    saving.value = false
    toast.error(t('catalog.taxa.messages.save_failed'))
  }
})
</script>

<template>
  <AppCard>
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
      <div class="flex justify-end gap-2">
        <Button :label="t('catalog.taxa.actions.cancel')" severity="secondary" @click="emit('cancelled')" />
        <Button type="submit" :label="t('catalog.taxa.actions.save')" :loading="saving" :disabled="saving" />
      </div>
    </form>
    <fieldset v-if="taxon" class="mt-4 border-1 border-round p-3">
      <legend class="font-semibold">{{ t('catalog.taxa.rules.title') }}</legend>
      <TaxonRuleManager :taxonomy-id="props.taxonomyId" :taxon-id="taxon.id" />
    </fieldset>
  </AppCard>
</template>
