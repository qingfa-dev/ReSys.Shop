<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useShippingRateStore } from '../store/shipping-rate.store'
import { shippingRateRepository } from '../api/shipping-rate.api'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { createShippingRateSchema } from '../types/shipping-rate.field'
import FormField from '@/shared/components/form/FormField.vue'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Button from 'primevue/button'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const store = useShippingRateStore()
const isEdit = computed(() => !!route.params.id)
const submitting = ref(false)

const { defineField, handleSubmit, errors, setValues } = useForm({
  validationSchema: toTypedSchema(createShippingRateSchema(t)),
  initialValues: {
    shippingMethodId: '',
    name: '',
    rate: 0,
    fromWeight: null,
    toWeight: null,
    fromTotal: null,
    toTotal: null,
  },
})

const [name] = defineField('name')
const [rate] = defineField('rate')
const [fromWeight] = defineField('fromWeight')
const [toWeight] = defineField('toWeight')
const [fromTotal] = defineField('fromTotal')
const [toTotal] = defineField('toTotal')

onMounted(async () => {
  if (isEdit) {
    await store.fetchById(route.params.id as string)
    if (store.current) {
      const c = store.current as any
      setValues({
        shippingMethodId: c.shippingMethodId ?? '',
        name: c.name,
        rate: c.cost ?? c.rate ?? 0,
        fromWeight: c.fromWeight ?? null,
        toWeight: c.toWeight ?? null,
        fromTotal: c.fromTotal ?? null,
        toTotal: c.toTotal ?? null,
      })
    }
  }
})

const onSubmit = handleSubmit(async (values) => {
  submitting.value = true
  try {
    if (isEdit) await shippingRateRepository.update(route.params.id as string, values)
    else await shippingRateRepository.create(values)
    router.push('/shipping/rates')
  } finally {
    submitting.value = false
  }
})
</script>

<template>
  <form @submit="onSubmit" class="flex flex-col gap-4 p-4">
    <FormField label="Rate Name" name="name" :error="errors.name">
      <InputText v-model="name" placeholder="Rate name" :invalid="!!errors.name" class="w-full rounded-xl h-11" />
    </FormField>

    <FormField label="Rate" name="rate" :error="errors.rate">
      <InputNumber v-model="rate" placeholder="Rate" class="w-full" :min="0" />
    </FormField>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <FormField label="From Weight" name="fromWeight" :error="errors.fromWeight">
        <InputNumber v-model="fromWeight" placeholder="From weight" class="w-full" :min="0" />
      </FormField>
      <FormField label="To Weight" name="toWeight" :error="errors.toWeight">
        <InputNumber v-model="toWeight" placeholder="To weight" class="w-full" :min="0" />
      </FormField>
      <FormField label="From Total" name="fromTotal" :error="errors.fromTotal">
        <InputNumber v-model="fromTotal" placeholder="From total" class="w-full" :min="0" />
      </FormField>
      <FormField label="To Total" name="toTotal" :error="errors.toTotal">
        <InputNumber v-model="toTotal" placeholder="To total" class="w-full" :min="0" />
      </FormField>
    </div>

    <Button type="submit" :loading="submitting" :label="isEdit ? 'Update' : 'Create'" class="rounded-xl" />
  </form>
</template>
