<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { AppCard } from '@/shared/components'
import Checkbox from 'primevue/checkbox'
import Button from 'primevue/button'
import { useShippingMethod } from '../composables/useShippingMethod'
import { ShippingMethodForms } from '../schemas'
import { ShippingMethodFormMapper } from '../mappers/shipping-method.mapper'
import { ROUTE } from '../routes'

const { id, mode, route, router, toast, api } = useShippingMethod()
const { t } = useI18n()

const schemas = new ShippingMethodForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [name] = defineField('name')
const [code] = defineField('code')
const [description] = defineField('description')
const [isActive] = defineField('isActive')
const [displayOrder] = defineField('displayOrder')
const [estimatedDeliveryMin] = defineField('estimatedDeliveryMin')
const [estimatedDeliveryMax] = defineField('estimatedDeliveryMax')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const title = computed(() => {
  if (mode.value === 'create') return t('shipping.methods.form.create_title')
  if (mode.value === 'edit') return t('shipping.methods.form.edit_title', { name: name.value || '' })
  return name.value || t('shipping.methods.form.view_title')
})

async function loadShippingMethod() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  try {
    const result = await api.get(id.value)
    if (result.isSuccess) {
      setValues({
        name: result.value.name,
        code: result.value.code,
        description: result.value.description ?? undefined,
        isActive: result.value.isActive ?? undefined,
        displayOrder: result.value.displayOrder ?? undefined,
        estimatedDeliveryMin: result.value.estimatedDeliveryMin ?? undefined,
        estimatedDeliveryMax: result.value.estimatedDeliveryMax ?? undefined,
      })
    } else {
      loadError.value = result.message ?? t('shipping.methods.messages.load_failed')
    }
  } catch (err) {
    console.error(err)
    loadError.value = t('shipping.methods.messages.load_failed')
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? ShippingMethodFormMapper.toCreate(values)
    : ShippingMethodFormMapper.toUpdate(values)
  try {
    const result = id.value
      ? await api.update(id.value, data)
      : await api.create(data)
    saving.value = false
    if (result.isSuccess) {
      toast.success(id.value
        ? t('shipping.methods.messages.update_success')
        : t('shipping.methods.messages.create_success'))
      const newId = result.value.id
      router.replace({ name: ROUTE.METHODS.VIEW, params: { id: newId } })
    } else {
      toast.error(result.message ?? t('shipping.methods.messages.save_failed'))
    }
  } catch (err) {
    console.error(err)
    saving.value = false
    toast.error(t('shipping.methods.messages.save_failed'))
  }
})

function cancel() {
  if (id.value) router.push({ name: ROUTE.METHODS.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.METHODS.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.METHODS.EDIT, params: { id: id.value } })
}

onMounted(async () => {
  await loadShippingMethod()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :subtitle="t('shipping.methods.form.subtitle')" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <Button
          v-if="mode === 'view'"
          :label="t('shipping.methods.actions.edit')"
          icon="pi pi-pencil"
          size="small"
          @click="toggleEdit"
        />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadShippingMethod" />
    <AppCard v-else>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('shipping.methods.labels.name')" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('shipping.methods.labels.code')" :error="errors.code" required>
            <input v-model="code" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full">
          <FormField :label="t('shipping.methods.labels.description')" :error="errors.description">
            <textarea v-model="description" class="p-inputtext p-component w-full" :disabled="mode === 'view'" rows="3" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('shipping.methods.labels.display_order')" :error="errors.displayOrder">
            <input v-model.number="displayOrder" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('shipping.methods.labels.estimated_delivery_min')" :error="errors.estimatedDeliveryMin">
            <input v-model.number="estimatedDeliveryMin" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('shipping.methods.labels.estimated_delivery_max')" :error="errors.estimatedDeliveryMax">
            <input v-model.number="estimatedDeliveryMax" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('shipping.methods.labels.is_active')">
            <div class="flex items-center gap-2 mt-1">
              <Checkbox v-model="isActive" :binary="true" :disabled="mode === 'view'" input-id="isActive" />
              <label for="isActive">{{ t('shipping.methods.labels.active_help') }}</label>
            </div>
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? t('shipping.methods.actions.save_create') : t('shipping.methods.actions.save_edit')"
        :cancel-label="t('shipping.methods.actions.cancel')"
        @save="save"
        @cancel="cancel"
      />
    </AppCard>
  </div>
</template>
