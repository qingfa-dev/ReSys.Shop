<script setup lang="ts">
import { watch } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { createCountrySchema } from '../schemas/country.schema'
import { useCountryStore } from '../stores/country.store'
import { useToast } from '@/common/composables/toast.use'
import FormField from '@/shared/components/FormField.Component.vue'
import type { Country } from '../types/country.response.type'
import { useI18n } from 'vue-i18n'

const props = withDefaults(defineProps<{
  visible: boolean
  item?: Country | null
  isEdit?: boolean
}>(), {
  isEdit: false,
})

const emit = defineEmits<{
  close: []
  saved: []
}>()

const store = useCountryStore()
const { t } = useI18n()
const { showToast } = useToast()

const { defineField, errors, handleSubmit: submitForm, setValues, resetForm } = useForm({
  validationSchema: toTypedSchema(createCountrySchema(t)),
  initialValues: {
    name: '',
    isoCode: '',
    callingCode: '',
    isActive: true,
  },
})

const [name] = defineField('name')
const [isoCode] = defineField('isoCode')
const [callingCode] = defineField('callingCode')
const [isActive] = defineField('isActive')

watch(() => props.visible, (val) => {
  if (val && props.item) {
    setValues({
      name: props.item.name,
      isoCode: props.item.isoCode,
      callingCode: props.item.callingCode || '',
      isActive: props.item.isActive,
    })
  } else if (val) {
    resetForm()
  }
})

const onFormSubmit = submitForm(async (values) => {
  if (props.isEdit && props.item) {
    const result = await store.updateCountry(props.item.id, values)
    if (result.isSuccess) {
      showToast('success', t('common.updated'), t('location.messages.update_success'))
      emit('saved')
    }
  } else {
    const result = await store.createCountry(values)
    if (result.isSuccess) {
      showToast('success', t('common.created'), t('location.messages.create_success'))
      emit('saved')
    }
  }
})

const onCancel = () => {
  resetForm()
  emit('close')
}
</script>

<template>
  <Dialog
    :visible="visible"
    :header="isEdit ? 'Edit Country' : 'New Country'"
    :modal="true"
    :closable="true"
    :style="{ width: '500px' }"
    @update:visible="(val: boolean) => { if (!val) onCancel() }"
    class="rounded-2xl"
  >
    <form @submit="onFormSubmit" class="flex flex-col gap-5 py-2">
      <FormField :label="t('location.labels.name')" name="name" :error="errors.name">
        <InputText v-model="name" placeholder="Country name" :invalid="!!errors.name" class="rounded-xl h-11" />
      </FormField>

      <FormField label="ISO Code" name="isoCode" :error="errors.isoCode">
        <InputText v-model="isoCode" placeholder="US" maxlength="2" class="rounded-xl h-11 uppercase" :invalid="!!errors.isoCode" />
      </FormField>

      <FormField label="Calling Code" name="callingCode">
        <InputText v-model="callingCode" placeholder="+1" class="rounded-xl h-11" />
      </FormField>

      <FormField :label="t('location.labels.active')" name="isActive">
        <div class="flex items-center gap-2 ml-1">
          <ToggleSwitch v-model="isActive" />
          <span class="text-sm text-surface-500">{{ isActive ? 'Enabled' : 'Disabled' }}</span>
        </div>
      </FormField>
    </form>

    <template #footer>
      <Button :label="t('common.cancel')" severity="secondary" text @click="onCancel" class="rounded-xl" />
      <Button
        :label="isEdit ? 'Update' : 'Create'"
        icon="pi pi-check"
        :loading="store.submitting"
        @click="onFormSubmit"
        class="rounded-xl px-6"
      />
    </template>
  </Dialog>
</template>
