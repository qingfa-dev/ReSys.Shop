<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useShippingRateStore } from '../stores/shipping-rate.store'
import { shippingRateService } from '../services/shipping-rate.service'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { ShippingRateSchema } from '../schemas/ShippingRate.Schema'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Button from 'primevue/button'

const route = useRoute()
const router = useRouter()
const store = useShippingRateStore()
const isEdit = !!route.params.id
const submitting = ref(false)

const { handleSubmit, errors } = useForm({
  validationSchema: toTypedSchema(ShippingRateSchema),
})

onMounted(async () => {
  if (isEdit) await store.fetchById(route.params.id as string)
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
  <form @submit="onSubmit">
    <InputText name="name" placeholder="Rate name" />
    <small v-if="errors.name">{{ errors.name }}</small>
    <InputNumber name="rate" placeholder="Rate" />
    <InputNumber name="fromWeight" placeholder="From weight" />
    <InputNumber name="toWeight" placeholder="To weight" />
    <Button type="submit" :loading="submitting" :label="isEdit ? 'Update' : 'Create'" />
  </form>
</template>
