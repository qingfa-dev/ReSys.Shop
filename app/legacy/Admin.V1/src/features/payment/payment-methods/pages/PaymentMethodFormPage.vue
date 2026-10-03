<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { usePaymentMethodStore } from '../store/payment-method.store'
import { paymentMethodRepository } from '../api/admin/payment-method.api'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { createPaymentMethodSchema } from '../types/payment-method.field'
import FormField from '@/shared/components/form/FormField.vue'
import InputText from 'primevue/inputtext'
import InputSwitch from 'primevue/inputswitch'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const store = usePaymentMethodStore()
const isEdit = computed(() => !!route.params.id)
const submitting = ref(false)

const { defineField, handleSubmit, errors, setValues } = useForm({
  validationSchema: toTypedSchema(createPaymentMethodSchema(t)),
  initialValues: {
    name: '',
    code: '',
    description: '',
    isActive: true,
    displayOrder: 0,
  },
})

const [name] = defineField('name')
const [code] = defineField('code')
const [description] = defineField('description')
const [displayOrder] = defineField('displayOrder')
const [isActive] = defineField('isActive')

onMounted(async () => {
  if (isEdit.value) {
    await store.fetchById(route.params.id as string)
    if (store.current) {
      setValues({
        name: store.current.name,
        code: store.current.code,
        description: store.current.description ?? '',
        isActive: store.current.isActive,
        displayOrder: store.current.position ?? 0,
      })
    }
  }
})

const onSubmit = handleSubmit(async (values) => {
  submitting.value = true
  try {
    if (isEdit.value) {
      await paymentMethodRepository.update(route.params.id as string, values)
    } else {
      await paymentMethodRepository.create(values)
    }
    router.push('/payment/methods')
  } finally {
    submitting.value = false
  }
})
</script>

<template>
  <form @submit="onSubmit" class="flex flex-col gap-4 p-4">
    <FormField label="Name" name="name" :error="errors.name">
      <InputText v-model="name" :invalid="!!errors.name" class="w-full rounded-xl h-11" />
    </FormField>

    <FormField label="Code" name="code" :error="errors.code">
      <InputText v-model="code" :invalid="!!errors.code" class="w-full rounded-xl h-11" />
    </FormField>

    <FormField label="Description" name="description">
      <Textarea v-model="description" class="w-full" rows="3" />
    </FormField>

    <FormField label="Display Order" name="displayOrder">
      <InputNumber v-model="displayOrder" class="w-full" :min="0" />
    </FormField>

    <FormField label="Active" name="isActive">
      <InputSwitch v-model="isActive" />
    </FormField>

    <Button type="submit" :loading="submitting" :label="isEdit ? 'Update' : 'Create'" class="rounded-xl" />
  </form>
</template>
