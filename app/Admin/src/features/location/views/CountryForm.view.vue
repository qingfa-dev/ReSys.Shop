<script setup lang="ts">
import { watch } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { countryCreateSchema } from '../schemas/country.schema'
import { useCountryStore } from '../stores/country.store'
import { useToast } from '@/shared/composables/toast.use'
import type { Country } from '../types/country.types'

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
const { showToast } = useToast()

const { defineField, errors, handleSubmit: submitForm, setValues, resetForm } = useForm({
  validationSchema: toTypedSchema(countryCreateSchema),
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
    if (result.success) {
      showToast('success', 'Updated', 'Country updated successfully')
      emit('saved')
    }
  } else {
    const result = await store.createCountry(values)
    if (result.success) {
      showToast('success', 'Created', 'Country created successfully')
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
      <div class="flex flex-col gap-2">
        <label for="name" class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">Name</label>
        <InputText id="name" v-model="name" placeholder="Country name" :invalid="!!errors.name" class="rounded-xl h-11" />
        <small class="text-red-500 ml-1" v-if="errors.name">{{ errors.name }}</small>
      </div>

      <div class="flex flex-col gap-2">
        <label for="isoCode" class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">ISO Code</label>
        <InputText id="isoCode" v-model="isoCode" placeholder="US" maxlength="2" class="rounded-xl h-11 uppercase" :invalid="!!errors.isoCode" />
        <small class="text-red-500 ml-1" v-if="errors.isoCode">{{ errors.isoCode }}</small>
      </div>

      <div class="flex flex-col gap-2">
        <label for="callingCode" class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">Calling Code</label>
        <InputText id="callingCode" v-model="callingCode" placeholder="+1" class="rounded-xl h-11" />
      </div>

      <div class="flex flex-col gap-2">
        <label class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">Active</label>
        <div class="flex items-center gap-2 ml-1">
          <ToggleSwitch v-model="isActive" />
          <span class="text-sm text-surface-500">{{ isActive ? 'Enabled' : 'Disabled' }}</span>
        </div>
      </div>
    </form>

    <template #footer>
      <Button label="Cancel" severity="secondary" text @click="onCancel" class="rounded-xl" />
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
