<script setup lang="ts">
import { watch } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { createStateSchema } from '../schemas/state.schema'
import { useStateStore } from '../stores/state.store'
import { useCountryStore } from '../../countries/stores/country.store'
import { useToast } from '@/common/composables/toast.use'
import FormField from '@/shared/components/FormField.Component.vue'
import { storeToRefs } from 'pinia'
import type { State } from '../types/state.response.type'
import { useI18n } from 'vue-i18n'

const props = withDefaults(defineProps<{
  visible: boolean
  item?: State | null
  isEdit?: boolean
}>(), {
  isEdit: false,
})

const emit = defineEmits<{
  close: []
  saved: []
}>()

const stateStore = useStateStore()
const countryStore = useCountryStore()
const { t } = useI18n()
const { items: countries } = storeToRefs(countryStore)
const { showToast } = useToast()

const { defineField, errors, handleSubmit: submitForm, setValues, resetForm } = useForm({
  validationSchema: toTypedSchema(createStateSchema(t)),
  initialValues: {
    name: '',
    abbreviation: '',
    countryId: '',
    isActive: true,
  },
})

const [name] = defineField('name')
const [abbreviation] = defineField('abbreviation')
const [countryId] = defineField('countryId')
const [isActive] = defineField('isActive')

watch(() => props.visible, (val) => {
  if (val && props.item) {
    setValues({
      name: props.item.name,
      abbreviation: props.item.abbreviation,
      countryId: props.item.countryId,
      isActive: props.item.isActive,
    })
  } else if (val) {
    resetForm()
  }
})

const onFormSubmit = submitForm(async (values) => {
  if (props.isEdit && props.item) {
    const result = await stateStore.updateState(props.item.id, values)
    if (result.isSuccess) {
      showToast('success', t('common.updated'), t('location.messages.state_update_success'))
      emit('saved')
    }
  } else {
    const result = await stateStore.createState(values)
    if (result.isSuccess) {
      showToast('success', t('common.created'), t('location.messages.state_create_success'))
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
    :header="isEdit ? 'Edit State' : 'New State'"
    :modal="true"
    :closable="true"
    :style="{ width: '500px' }"
    @update:visible="(val: boolean) => { if (!val) onCancel() }"
    class="rounded-2xl"
  >
    <form @submit="onFormSubmit" class="flex flex-col gap-5 py-2">
      <FormField :label="t('location.labels.name')" name="name" :error="errors.name">
        <InputText v-model="name" placeholder="State name" :invalid="!!errors.name" class="rounded-xl h-11" />
      </FormField>

      <FormField :label="t('location.labels.abbreviation')" name="abbreviation" :error="errors.abbreviation">
        <InputText v-model="abbreviation" placeholder="CA" class="rounded-xl h-11 uppercase" :invalid="!!errors.abbreviation" />
      </FormField>

      <FormField :label="t('location.labels.country')" name="countryId" :error="errors.countryId">
        <Select
          v-model="countryId"
          :options="countries"
          optionLabel="name"
          optionValue="id"
          placeholder="Select country"
          class="rounded-xl"
          :invalid="!!errors.countryId"
        />
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
        :loading="stateStore.submitting"
        @click="onFormSubmit"
        class="rounded-xl px-6"
      />
    </template>
  </Dialog>
</template>
