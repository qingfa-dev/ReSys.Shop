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
import Checkbox from 'primevue/checkbox'
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
  if (mode.value === 'create') return 'Create Payment Method'
  if (mode.value === 'edit') return `Edit: ${name.value || ''}`
  return name.value || 'Payment Method'
})

async function loadPaymentMethod() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
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
    loadError.value = result.message ?? 'Failed to load payment method'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? PaymentMethodFormMapper.toCreate(values)
    : PaymentMethodFormMapper.toUpdate(values)
  const result = id.value
    ? await api.update(id.value, data)
    : await api.create(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? 'Payment method updated' : 'Payment method created')
    const newId = result.value.id
    router.replace({ name: ROUTE.METHODS.VIEW, params: { id: newId } })
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  if (id.value) router.push({ name: ROUTE.METHODS.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.METHODS.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.METHODS.EDIT, params: { id: id.value } })
}

onMounted(loadPaymentMethod)
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">Edit</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadPaymentMethod" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Name" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Code" :error="errors.code" required>
            <input v-model="code" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-12">
          <FormField label="Description" :error="errors.description">
            <textarea v-model="description" class="p-inputtext p-component w-full" :disabled="mode === 'view'" rows="3" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-4">
          <FormField label="Display Order" :error="errors.displayOrder">
            <input v-model.number="displayOrder" type="number" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-8">
          <FormField label="Supported Currencies" :error="errors.supportedCurrencies">
            <input v-model="supportedCurrencies" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" placeholder="e.g. USD, EUR" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-6">
          <FormField label="Active">
            <div class="flex align-items-center gap-2 mt-1">
              <Checkbox v-model="isActive" :binary="true" :disabled="mode === 'view'" input-id="isActive" />
              <label for="isActive">Method is active and available</label>
            </div>
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Test Mode">
            <div class="flex align-items-center gap-2 mt-1">
              <Checkbox v-model="isTestMode" :binary="true" :disabled="mode === 'view'" input-id="isTestMode" />
              <label for="isTestMode">Run in sandbox/test mode</label>
            </div>
          </FormField>
        </div>
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? 'Create' : 'Save'"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />
    </div>
  </div>
</template>
