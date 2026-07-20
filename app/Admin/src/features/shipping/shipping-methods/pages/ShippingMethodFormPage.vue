<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useShippingMethodStore } from '../store/shipping-method.store'
import { shippingMethodRepository } from '../api/shipping-method.api'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { createShippingMethodSchema } from '../types/shipping-method.field'
import FormField from '@/shared/components/form/FormField.vue'
import InputText from 'primevue/inputtext'
import InputSwitch from 'primevue/inputswitch'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import Button from 'primevue/button'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const store = useShippingMethodStore()
const isEdit = computed(() => !!route.params.id)
const submitting = ref(false)

const { defineField, handleSubmit, errors, setValues } = useForm({
  validationSchema: toTypedSchema(createShippingMethodSchema(t)),
  initialValues: {
    name: '',
    description: '',
    carrier: '',
    isActive: true,
    displayOrder: 0,
  },
})

const [name] = defineField('name')
const [description] = defineField('description')
const [carrier] = defineField('carrier')
const [isActive] = defineField('isActive')
const [displayOrder] = defineField('displayOrder')

onMounted(async () => {
  if (isEdit.value) {
    await store.fetchById(route.params.id as string)
    if (store.current) {
      const c = store.current as any
      setValues({
        name: c.name,
        description: c.description ?? '',
        carrier: c.carrier ?? '',
        isActive: c.isActive ?? true,
        displayOrder: c.displayOrder ?? 0,
      })
    }
  }
})

const onSubmit = handleSubmit(async (values) => {
  submitting.value = true
  try {
    if (isEdit.value) {
      await shippingMethodRepository.update(route.params.id as string, values)
    } else {
      await shippingMethodRepository.create(values)
    }
    router.push('/shipping/methods')
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

    <FormField label="Carrier" name="carrier" :error="errors.carrier">
      <InputText v-model="carrier" :invalid="!!errors.carrier" class="w-full rounded-xl h-11" />
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
