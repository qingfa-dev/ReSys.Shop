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
import { usePaymentMethod } from '../composables/usePaymentMethod'
import { PaymentMethodForms } from '../schemas'
import { PaymentMethodFormMapper } from '../mappers/payment-method.mapper'
import { ROUTE } from '../routes'

const { id, mode, route, router, toast, api } = usePaymentMethod()
const { t } = useI18n()

const schemas = new PaymentMethodForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [name] = defineField('name')
const [code] = defineField('code')
const [description] = defineField('description')
const [isActive] = defineField('isActive')
const [isTestMode] = defineField('isTestMode')
const [displayOrder] = defineField('displayOrder')
const [supportedCurrencies] = defineField('supportedCurrencies')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const title = computed(() => {
  if (mode.value === 'create') return t('payment.methods.form.create_title')
  if (mode.value === 'edit') return t('payment.methods.form.edit_title', { name: name.value || '' })
  return name.value || t('payment.methods.form.view_title')
})

async function loadPaymentMethod() {
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
        isTestMode: result.value.isTestMode ?? undefined,
        displayOrder: result.value.displayOrder ?? undefined,
        supportedCurrencies: result.value.supportedCurrencies ?? undefined,
      })
    } else {
      loadError.value = result.message ?? t('payment.methods.messages.load_failed')
    }
  } catch (err) {
    console.error(err)
    loadError.value = t('payment.methods.messages.load_failed')
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? PaymentMethodFormMapper.toCreate(values)
    : PaymentMethodFormMapper.toUpdate(values)
  try {
    const result = id.value
      ? await api.update(id.value, data)
      : await api.create(data)
    saving.value = false
    if (result.isSuccess) {
      toast.success(id.value
        ? t('payment.methods.messages.update_success')
        : t('payment.methods.messages.create_success'))
      const newId = result.value.id
      router.replace({ name: ROUTE.METHODS.VIEW, params: { id: newId } })
    } else {
      toast.error(result.message ?? t('payment.methods.messages.save_failed'))
    }
  } catch (err) {
    console.error(err)
    saving.value = false
    toast.error(t('payment.methods.messages.save_failed'))
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
  await loadPaymentMethod()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :subtitle="t('payment.methods.form.subtitle')" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <Button
          v-if="mode === 'view'"
          :label="t('payment.methods.actions.edit')"
          icon="pi pi-pencil"
          size="small"
          @click="toggleEdit"
        />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadPaymentMethod" />
    <AppCard v-else>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('payment.methods.labels.name')" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('payment.methods.labels.code')" :error="errors.code" required>
            <input v-model="code" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full">
          <FormField :label="t('payment.methods.labels.description')" :error="errors.description">
            <textarea v-model="description" class="p-inputtext p-component w-full" :disabled="mode === 'view'" rows="3" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-4">
          <FormField :label="t('payment.methods.labels.display_order')" :error="errors.displayOrder">
            <input v-model.number="displayOrder" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-8">
          <FormField :label="t('payment.methods.labels.supported_currencies')" :error="errors.supportedCurrencies">
            <input v-model="supportedCurrencies" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" placeholder="e.g. USD, EUR" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('payment.methods.labels.is_active')">
            <div class="flex items-center gap-2 mt-1">
              <Checkbox v-model="isActive" :binary="true" :disabled="mode === 'view'" input-id="isActive" />
              <label for="isActive">{{ t('payment.methods.labels.active_help') }}</label>
            </div>
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('payment.methods.labels.is_test_mode')">
            <div class="flex items-center gap-2 mt-1">
              <Checkbox v-model="isTestMode" :binary="true" :disabled="mode === 'view'" input-id="isTestMode" />
              <label for="isTestMode">{{ t('payment.methods.labels.test_mode_help') }}</label>
            </div>
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? t('payment.methods.actions.save_create') : t('payment.methods.actions.save_edit')"
        :cancel-label="t('payment.methods.actions.cancel')"
        @save="save"
        @cancel="cancel"
      />
    </AppCard>
  </div>
</template>
