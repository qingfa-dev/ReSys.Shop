<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useShippingRateStore } from '../stores/shipping-rate.store'
import { shippingRateService } from '../services/shipping-rate.service'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { createShippingRateSchema } from '../schemas/shipping-rate.schema'
import FormField from '@/shared/components/FormField.Component.vue'
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
  },
})

const [name] = defineField('name')
const [rate] = defineField('rate')

onMounted(async () => {
  if (isEdit) {
    await store.fetchById(route.params.id as string)
    if (store.current) {
      const c = store.current as any
      setValues({
        shippingMethodId: c.shippingMethodId ?? '',
        name: c.name,
        rate: c.cost ?? c.rate ?? 0,
      })
    }
  }
})

const onSubmit = handleSubmit(async (values) => {
  submitting.value = true
  try {
    if (isEdit) await shippingRateService.update(route.params.id as string, values)
    else await shippingRateService.create(values)
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

    <Button type="submit" :loading="submitting" :label="isEdit ? 'Update' : 'Create'" class="rounded-xl" />
  </form>
</template>
