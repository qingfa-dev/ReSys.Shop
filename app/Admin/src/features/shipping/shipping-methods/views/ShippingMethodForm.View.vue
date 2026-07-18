<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useShippingMethodStore } from '../stores/shipping-method.store'
import { shippingMethodService } from '../services/shipping-method.service'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { ShippingMethodSchema } from '../schemas/ShippingMethod.Schema'
import InputText from 'primevue/inputtext'
import InputSwitch from 'primevue/inputswitch'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'

const route = useRoute()
const router = useRouter()
const store = useShippingMethodStore()
const isEdit = !!route.params.id
const submitting = ref(false)

const { handleSubmit, errors } = useForm({
  validationSchema: toTypedSchema(ShippingMethodSchema),
})

onMounted(async () => {
  if (isEdit) {
    await store.fetchById(route.params.id as string)
  }
})

const onSubmit = handleSubmit(async (values) => {
  submitting.value = true
  try {
    if (isEdit) {
      await shippingMethodService.update(route.params.id as string, values)
    } else {
      await shippingMethodService.create(values)
    }
    router.push('/shipping/methods')
  } finally {
    submitting.value = false
  }
})
</script>

<template>
  <form @submit="onSubmit">
    <div class="field">
      <label>Name</label>
      <InputText name="name" />
      <small v-if="errors.name">{{ errors.name }}</small>
    </div>
    <div class="field">
      <label>Carrier</label>
      <InputText name="carrier" />
    </div>
    <div class="field">
      <label>Description</label>
      <Textarea name="description" />
    </div>
    <div class="field">
      <label>Display Order</label>
      <InputNumber name="displayOrder" />
    </div>
    <div class="field">
      <label>Active</label>
      <InputSwitch name="isActive" />
    </div>
    <Button type="submit" :loading="submitting" :label="isEdit ? 'Update' : 'Create'" />
  </form>
</template>
