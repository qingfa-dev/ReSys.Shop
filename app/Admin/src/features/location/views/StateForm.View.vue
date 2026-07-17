<script setup lang="ts">
import { watch } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { stateCreateSchema } from '../schemas/state.schema'
import { useStateStore } from '../stores/state.store'
import { useCountryStore } from '../stores/country.store'
import { useToast } from '@/shared/composables/toast.use'
import { storeToRefs } from 'pinia'
import type { State } from '../types/location.domain.types'

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
const { items: countries } = storeToRefs(countryStore)
const { showToast } = useToast()

const { defineField, errors, handleSubmit: submitForm, setValues, resetForm } = useForm({
  validationSchema: toTypedSchema(stateCreateSchema),
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
      showToast('success', 'Updated', 'State updated successfully')
      emit('saved')
    }
  } else {
    const result = await stateStore.createState(values)
    if (result.isSuccess) {
      showToast('success', 'Created', 'State created successfully')
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
      <div class="flex flex-col gap-2">
        <label for="name" class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">Name</label>
        <InputText id="name" v-model="name" placeholder="State name" :invalid="!!errors.name" class="rounded-xl h-11" />
        <small class="text-red-500 ml-1" v-if="errors.name">{{ errors.name }}</small>
      </div>

      <div class="flex flex-col gap-2">
        <label for="abbreviation" class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">Abbreviation</label>
        <InputText id="abbreviation" v-model="abbreviation" placeholder="CA" class="rounded-xl h-11 uppercase" :invalid="!!errors.abbreviation" />
        <small class="text-red-500 ml-1" v-if="errors.abbreviation">{{ errors.abbreviation }}</small>
      </div>

      <div class="flex flex-col gap-2">
        <label for="countryId" class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">Country</label>
        <Select
          id="countryId"
          v-model="countryId"
          :options="countries"
          optionLabel="name"
          optionValue="id"
          placeholder="Select country"
          class="rounded-xl"
          :invalid="!!errors.countryId"
        />
        <small class="text-red-500 ml-1" v-if="errors.countryId">{{ errors.countryId }}</small>
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
        :loading="stateStore.submitting"
        @click="onFormSubmit"
        class="rounded-xl px-6"
      />
    </template>
  </Dialog>
</template>
